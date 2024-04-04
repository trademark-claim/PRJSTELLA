using AspectInjector.Broker;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Cat
{
    internal static class LoggingAspects
    {
        [Aspect(Scope.Global)]
        [AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
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

                if (method.IsDefined(typeof(RecordTime), false))
                {
                    var key = GetUniqueKey(method, instance);
                    var stopwatch = Stopwatch.StartNew();
                    Timers[key] = stopwatch;
                }
                Cat.Logging.Log($"Entering {(instance == null ? "static" : "instance")} method {method.DeclaringType?.FullName}.{method.Name}");
                Cat.Logging.LogP($"Arguments:", arguments);
            }

            [Advice(Kind.After, Targets = Target.Method)]
            public void After(
                [Argument(Source.Metadata)] MethodBase method,
                [Argument(Source.Instance)] object instance,
                [Argument(Source.ReturnValue)] object returnValue,
                [Argument(Source.ReturnType)] Type returnType)
            {
                if (method.IsDefined(typeof(RecordTime), false))
                {
                    var key = GetUniqueKey(method, instance);
                    if (Timers.TryRemove(key, out var stopwatch))
                    {
                        stopwatch.Stop();
                        Cat.Logging.Log($"Exiting method {method.DeclaringType?.FullName}.{method.Name}. Execution time: {stopwatch.ElapsedMilliseconds} ms. Return Value: {Cat.Logging.ProcessMessage(returnValue, 0)} of type {returnType.FullName}");
                    }
                }
                else Cat.Logging.Log($"Exiting method {method.DeclaringType?.FullName}.{method.Name}. Return Value: {Cat.Logging.ProcessMessage(returnValue, 0)} of type {returnType.FullName}");
            }

            private static string GetUniqueKey(MethodBase method, object instance)
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                var instanceId = instance?.GetHashCode() ?? 0;
                return $"{method.DeclaringType?.FullName}.{method.Name}-{instanceId}-{threadId}";
            }
        }

        [Aspect(Scope.Global)]
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
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
                    Cat.Logging.Log($"Error while executing command {target.Method.DeclaringType}.{target.Method.Name}");
                    Cat.Logging.LogError(ex);

                    if (method.IsDefined(typeof(InterfaceNotice), false))
                        Catowo.Interface.AddTextLogR($"Error caught while executing {target.Method.DeclaringType}.{target.Method.Name}", RED);
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

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
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
                    return HandleAsync(method, args, target, returnType).ConfigureAwait(false);
                }
                else
                {
                    Cat.Logging.Log("[CRITICALLY FATAL ERROR] Asynchronous Exception Swallower attached to a non-async method. Please submit a bug report and attach this log!");
                    Task.Run(async () => await Cat.Logging.FinalFlush()).GetAwaiter().GetResult();
                    throw new InvalidOperationException("Asynchronous Exception Swallower attached to a non-async method.");
                }
            }

            private async Task<object> HandleAsync(MethodBase method, object[] args, Func<object[], object> target, Type returnType)
            {
                try
                {
                    var task = (Task)target(args);
                    await task.ConfigureAwait(false);
                    var resultProperty = returnType.GetProperty("Result");
                    return returnType.IsGenericType ? resultProperty.GetValue(task) : null;
                }
                catch (Exception ex)
                {
                    Cat.Logging.LogError(ex);
                    if (method.IsDefined(typeof(InterfaceNotice), false))
                        Catowo.Interface.AddTextLogR($"Error caught while executing {target.Method.DeclaringType}.{target.Method.Name}", RED);
                    if (method.IsDefined(typeof(UpsetStomach), false))
                    {
                        Task.Run(async () => await Cat.Logging.FinalFlush()).GetAwaiter().GetResult();
                        throw;
                    }

                    if (returnType == typeof(Task)) return Task.CompletedTask;
                    else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<bool>))
                    {
                        var taskType = returnType.GetGenericArguments()[0];
                        var defaultValue = taskType.IsValueType ? Activator.CreateInstance(taskType) : null;
                        return CreateCompletedTask(taskType, false);
                    }
                    else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<int>))
                    {
                        var taskType = returnType.GetGenericArguments()[0];
                        return CreateCompletedTask(taskType, -7911);
                    }
                    else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        var taskType = returnType.GetGenericArguments()[0];
                        var defaultValue = taskType.IsValueType ? Activator.CreateInstance(taskType) : null;
                        return CreateCompletedTask(taskType, defaultValue);
                    }
                    else
                    {
                        Cat.Logging.Log("[CRITICALLY FATAL ERROR] Asynchronous Exception Swallower tried to consume unexpected return type... this should never happen... please make a bug report and submit this log.");
                        Task.Run(async () => await Cat.Logging.FinalFlush()).GetAwaiter().GetResult();
                        throw new InvalidOperationException("Unexpected return type for async method.");
                    }
                }
            }

            private static object CreateCompletedTask(Type resultType, object result)
            {
                var method = typeof(Task).GetMethod(nameof(Task.FromResult))
                                         .MakeGenericMethod(resultType);
                return method.Invoke(null, new[] { result });
            }
        }
    }
}