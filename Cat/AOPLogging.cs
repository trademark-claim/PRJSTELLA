using AspectInjector.Broker;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Cat
{
    internal static class LoggingAspects
    {
        [Aspect(Scope.Global)]
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple=false)]
        [Injection(typeof(Logging))]
        public class Logging : Attribute
        {
            private static readonly ConcurrentDictionary<string, Stopwatch> Timers = new ConcurrentDictionary<string, Stopwatch>();

            [Advice(Kind.Before, Targets = Target.Method)]
            public void Before(
                [Argument(Source.Metadata)] MethodBase method,
                [Argument(Source.Instance)] object instance,
                [Argument(Source.Arguments)] object[] arguments)
            {
                if (UserData.AspectLogging)
                {
                    if (method.IsDefined(typeof(RecordTime), false) || UserData.TimeAll)
                    {
                        var key = GetUniqueKey(method, instance);
                        var stopwatch = Stopwatch.StartNew();
                        Timers[key] = stopwatch;
                    }
                    Cat.Logging.Log($"Entering {(instance == null ? "static" : "instance")} method {method.DeclaringType?.FullName?.Replace('+', '.')}.{method.Name}");
                    Cat.Logging.LogP($"Arguments:", arguments);
                }
            }

            [Advice(Kind.After, Targets = Target.Method)]
            public void After(
                [Argument(Source.Metadata)] MethodBase method,
                [Argument(Source.Instance)] object instance,
                [Argument(Source.ReturnValue)] object returnValue,
                [Argument(Source.ReturnType)] Type returnType)
            {
                if (UserData.AspectLogging)
                {
                    if (method.IsDefined(typeof(RecordTime), false) || UserData.TimeAll)
                    {
                        var key = GetUniqueKey(method, instance);
                        if (Timers.TryRemove(key, out var stopwatch))
                        {
                            stopwatch.Stop();
                            Cat.Logging.Log($"Exiting method {method.DeclaringType?.FullName?.Replace('+', '.')}.{method.Name}. Execution time: {stopwatch.ElapsedMilliseconds} ms. Return Value: {Cat.Logging.ProcessMessage(returnValue, 0)} of type {returnType.FullName}");
                        }
                    }
                    else Cat.Logging.Log($"Exiting method {method.DeclaringType?.FullName?.Replace('+', '.')}.{method.Name}. Return Value: {Cat.Logging.ProcessMessage(returnValue, 0)} of type {returnType.FullName}");
                }
            }

            private static string GetUniqueKey(MethodBase method, object instance)
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                var instanceId = instance?.GetHashCode() ?? 0;
                return $"{method.DeclaringType?.FullName?.Replace('+', '.')}.{method.Name}-{instanceId}-{threadId}";
            }
        }

        [Aspect(Scope.Global)]
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false)]
        [Injection(typeof(ConsumeException))]
        public class ConsumeException : Attribute
        {
            [Advice(Kind.Around, Targets = Target.Method)]
            public object Around([Argument(Source.Metadata)] MethodBase method,
                                 [Argument(Source.Arguments)] object[] args,
                                 [Argument(Source.Target)] Func<object[], object> target,
                                 [Argument(Source.ReturnType)] Type returnType)
            {
                try
                {
                    return target(args);
                }
                catch (Exception ex)
                {
                    Cat.Logging.Log($"Error while executing command {target.Method.DeclaringType?.FullName?.Replace('+', '.')}.{target.Method.Name}");
                    Cat.Logging.LogError(ex);
                    if (method.IsDefined(typeof(InterfaceNotice), false))
                        Catowo.Interface.AddTextLogR($"Error caught while executing {target.Method.DeclaringType?.FullName?.Replace('+', '.')}.{target.Method.Name}", RED);
                    if (method.IsDefined(typeof(UpsetStomach), false))
                    {
                        Task.Run(async () => await Cat.Logging.FinalFlush()).GetAwaiter().GetResult();
                        throw;
                    }

                    if (returnType == typeof(bool)) return false;
                    else if (returnType.IsValueType) return Activator.CreateInstance(returnType);
                    else return null;
                }
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public class UpsetStomach : Attribute;

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public class RecordTime : Attribute;

        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false)]
        public class InterfaceNotice : Attribute;

        [Aspect(Scope.Global)]
        [Injection(typeof(AsyncExceptionSwallower))]
        public class AsyncExceptionSwallower : Attribute
        {
            [Advice(Kind.Around, Targets = Target.Method)]
            public object Around([Argument(Source.Metadata)] MethodBase method,
                                 [Argument(Source.Arguments)] object[] args,
                                 [Argument(Source.Target)] Func<object[], object> target,
                                 [Argument(Source.ReturnType)] Type returnType)
            {
                if (typeof(Task).IsAssignableFrom(returnType))
                {
                    try
                    {
                        var result = target(args);
                        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                        {
                            var taskType = returnType.GetGenericArguments()[0];
                            return ProcessTaskWithResult(result, taskType, method);
                        }
                        else
                        {
                            return result;
                        }
                    }
                    catch (Exception ex)
                    {
                        return HandleException(ex, method, returnType);
                    }
                }
                else
                {
                    Cat.Logging.Log("[CRITICALLY FATAL ERROR] Asynchronous Exception Swallower attached to a non-async method. Please submit a bug report and attach this log!");
                    Task.Run(async () => await Cat.Logging.FinalFlush()).GetAwaiter().GetResult();
                    throw new InvalidOperationException("Asynchronous Exception Swallower attached to a non-async method.");
                }
            }

            private static object ProcessTaskWithResult(object task, Type resultType, MethodBase method)
            {
                // Process Task<T> to handle its result or exception.
                var taskCompletionSource = (TaskCompletionSource<object>)Activator.CreateInstance(typeof(TaskCompletionSource<>).MakeGenericType(resultType));

                ((Task)task).ContinueWith(t =>
                {
                    if (t.IsFaulted && t.Exception != null)
                    {
                        HandleException(t.Exception, method, typeof(Task<>).MakeGenericType(resultType));
                        taskCompletionSource.TrySetException(t.Exception.InnerExceptions);
                    }
                    else if (t.IsCanceled)
                    {
                        taskCompletionSource.TrySetCanceled();
                    }
                    else
                    {
                        var resultProp = t.GetType().GetProperty("Result");
                        var result = resultProp.GetValue(t);
                        taskCompletionSource.TrySetResult(result);
                    }
                }, TaskScheduler.Current);

                return taskCompletionSource.Task;
            }

            private static object HandleException(Exception ex, MethodBase method, Type returnType)
            {
                var capturedException = ExceptionDispatchInfo.Capture(ex);
                Cat.Logging.LogError(ex);
                if (method.IsDefined(typeof(InterfaceNotice), false))
                {
                    Catowo.Interface.AddTextLogR($"Error caught while executing {method.DeclaringType?.FullName?.Replace('+', '.')}.{method.Name}", RED);
                }

                // Check if UpsetStomach attribute is present.
                if (method.IsDefined(typeof(UpsetStomach), false))
                {
                    Task.Run(async () => await Cat.Logging.FinalFlush()).GetAwaiter().GetResult();
                    capturedException.Throw();
                }

                // Return a faulted task based on the returnType.
                if (returnType == typeof(Task))
                {
                    return Task.FromException(ex);
                }
                else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    return CreateFaultedTask(returnType.GetGenericArguments()[0], ex);
                }
                else
                {
                    Cat.Logging.Log("[CRITICALLY FATAL ERROR] Asynchronous Exception Swallower tried to consume unexpected return type... this should never happen... please make a bug report and submit this log.");
                    Task.Run(async () => await Cat.Logging.FinalFlush()).GetAwaiter().GetResult();
                    throw new InvalidOperationException("Unexpected return type for async method.");
                }
            }

            private static object CreateFaultedTask(Type resultType, Exception exception)
            {
                var method = typeof(TaskCompletionSource<>)
                    .MakeGenericType(resultType)
                    .GetMethod("TrySetException", new Type[] { typeof(Exception) });

                var taskCompletionSource = Activator.CreateInstance(typeof(TaskCompletionSource<>).MakeGenericType(resultType));
                method.Invoke(taskCompletionSource, new object[] { exception });
                return taskCompletionSource.GetType().GetProperty("Task").GetValue(taskCompletionSource);
            }
        }
    }
}