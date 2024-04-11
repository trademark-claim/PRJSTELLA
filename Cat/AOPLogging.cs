// -----------------------------------------------------------------------
// <summary>
// File: AOPLogging.cs
// Author: Nexus
// This file contains aspect-oriented programming (AOP) implementations for
// logging, exception handling, and performance measurement aspects across the Cat application.
// </summary>
// -----------------------------------------------------------------------


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
        /// <summary>
        /// Aspect for logging method entry, exit, and execution time if specified.
        /// </summary>
        [Aspect(Scope.Global)]
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple=false)]
        [Injection(typeof(Logging))]
        public class Logging : Attribute
        {
            private static readonly ConcurrentDictionary<string, Stopwatch> Timers = new ConcurrentDictionary<string, Stopwatch>();

            /// <summary>
            /// Invoked before the target method execution starts, logging the entry and starting a timer if needed.
            /// </summary>
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

            /// <summary>
            /// Invoked after the target method execution finishes, logging the exit and execution time if measured.
            /// </summary>
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
                            Cat.Logging.Log($"Exiting method {method.DeclaringType?.FullName?.Replace('+', '.')}.{method.Name}. Execution time: {stopwatch.Elapsed.Seconds}s {stopwatch.Elapsed.Milliseconds}ms {stopwatch.Elapsed.Microseconds}µs {stopwatch.Elapsed.Nanoseconds}ns Return Value: {Cat.Logging.ProcessMessage(returnValue, 0)} of type {returnType.FullName}");
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

        /// <summary>
        /// Aspect for handling exceptions by logging them and optionally providing a user notification.
        /// </summary>
        [Aspect(Scope.Global)]
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false)]
        [Injection(typeof(ConsumeException))]
        public class ConsumeException : Attribute
        {
            /// <summary>
            /// Wraps the target method execution to catch and handle exceptions.
            /// </summary>
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

        /// <summary>
        /// Marks methods for which exceptions should trigger an urgent logging flush.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public class UpsetStomach : Attribute;

        /// <summary>
        /// Marks methods for logging execution time.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public class RecordTime : Attribute;

        /// <summary>
        /// Marks methods for logging execution time.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false)]
        public class InterfaceNotice : Attribute;

        /// <summary>
        /// Aspect for swallowing exceptions in asynchronous methods without unhandled exceptions propagating.
        /// </summary>
        [Aspect(Scope.Global)]
        [Injection(typeof(AsyncExceptionSwallower))]
        public class AsyncExceptionSwallower : Attribute
        {
            /// <summary>
            /// Wraps asynchronous target method execution to manage exceptions gracefully.
            /// </summary>
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

            /// <summary>
            /// Processes a Task&lt;T&gt; to handle its result or any exceptions, encapsulating it into a new TaskCompletionSource&lt;T&gt;.
            /// </summary>
            /// <param name="task">The task being processed.</param>
            /// <param name="resultType">The type of the result expected from the task.</param>
            /// <param name="method">The method base for logging purposes.</param>
            /// <returns>A new task that represents the asynchronous operation, including its result or exception.</returns>
            /// <remarks>
            /// If the task completes successfully, the result is set on the task completion source. If the task fails or is canceled,
            /// the exception is captured and set on the task completion source, or it is marked as canceled, respectively.
            /// </remarks>
            private static object ProcessTaskWithResult(object task, Type resultType, MethodBase method)
            {
                var tcsType = typeof(TaskCompletionSource<>).MakeGenericType(resultType != typeof(void) ? resultType : typeof(object));
                var taskCompletionSource = Activator.CreateInstance(tcsType);

                ((Task)task).ContinueWith(t =>
                {
                    if (t.IsFaulted && t.Exception != null)
                    {
                        HandleException(t.Exception, method, typeof(Task<>).MakeGenericType(resultType));
                        var trySetExceptionMethod = tcsType.GetMethod("TrySetException", new[] { typeof(Exception) });
                        trySetExceptionMethod.Invoke(taskCompletionSource, new object[] { t.Exception.InnerException });
                    }
                    else if (t.IsCanceled)
                    {
                        var trySetCanceledMethod = tcsType.GetMethod("TrySetCanceled", Type.EmptyTypes);
                        trySetCanceledMethod.Invoke(taskCompletionSource, null);
                    }
                    else
                    {
                        if (resultType == typeof(void))
                        {
                            var trySetResultMethod = tcsType.GetMethod("TrySetResult");
                            trySetResultMethod.Invoke(taskCompletionSource, new object[] { null });
                        }
                        else
                        {
                            var resultProp = t.GetType().GetProperty("Result");
                            var result = resultProp.GetValue(t);
                            var trySetResultMethod = tcsType.GetMethod("TrySetResult", new[] { resultType });
                            trySetResultMethod.Invoke(taskCompletionSource, new[] { result });
                        }
                    }
                }, TaskScheduler.Current);

                return tcsType.GetProperty("Task").GetValue(taskCompletionSource);
            }




            /// <summary>
            /// Handles exceptions caught from the execution of a task by logging the error and determining the appropriate return value.
            /// </summary>
            /// <param name="ex">The exception that was caught.</param>
            /// <param name="method">The method base for which the exception occurred.</param>
            /// <param name="returnType">The return type of the method where the exception was caught.</param>
            /// <returns>A faulted task appropriate for the method's return type, encapsulating the caught exception.</returns>
            /// <remarks>
            /// Logs the error and, if an interface notice is warranted, adds an error message to the user interface. If the exception is severe,
            /// a final flush of logs is performed. The method then creates a faulted task matching the expected return type of the method.
            /// </remarks>
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

            /// <summary>
            /// Creates a faulted Task of a specific type, setting the provided exception as the reason for the fault.
            /// </summary>
            /// <param name="resultType">The type of the result that the task would have produced if it had not failed.</param>
            /// <param name="exception">The exception that caused the task to fail.</param>
            /// <returns>A faulted Task of the specified result type.</returns>
            /// <remarks>
            /// Utilizes reflection to create a TaskCompletionSource&lt;T&gt; and set the exception, ensuring the task is in a faulted state with the provided exception.
            /// </remarks>
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