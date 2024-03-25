//#define THROWERRORS
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace Cat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private static DateTime starttime = DateTime.Now;
        private static long maxMemory = 0;
        private static bool isShuttingDown = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            Current.DispatcherUnhandledException += (s, e) =>
            {
                Exception exc = e.Exception;
                Logging.LogError(exc);
                e.Handled = true;
                Logging.FinalFlush().GetAwaiter().GetResult();
                throw exc;
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Exception? exc = e.ExceptionObject as Exception;
                if (exc != null)
                    Logging.Log(exc);
                Logging.FinalFlush().GetAwaiter().GetResult();
                throw exc;
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                var exc = e.Exception;
                Logging.LogError(exc);
                e.SetObserved();
                Logging.FinalFlush().GetAwaiter().GetResult();
                throw exc;
            };

            System.Timers.Timer memoryUsageTimer = new System.Timers.Timer(10000);
            memoryUsageTimer.Elapsed += (sender, args) =>
            {
                long currentMemoryUsage = Process.GetCurrentProcess().PrivateMemorySize64;
                if (currentMemoryUsage > maxMemory)
                {
                    maxMemory = currentMemoryUsage;
                }
            };
            memoryUsageTimer.Start();
            Program.Start();
            base.OnStartup(e);
        }

        internal static void ShuttingDown()
        {
            if (isShuttingDown)
                return;
            isShuttingDown = true;
            DateTime endtime = DateTime.Now;
            TimeSpan dur = endtime - starttime;
            long averageMemoryUsage = maxMemory / 2;
            Logging.Log("Shutting down...");
            Catowo.DestroyKeyHook();
            Logging.Log($"Application Start Time: {starttime}");
            Logging.Log($"Application End Time: {endtime}");
            Logging.Log($"Run Duration: {dur}");
            Logging.Log($"Maximum Memory Usage: {maxMemory} bytes");
            Logging.Log($"Average Memory Usage: {averageMemoryUsage} bytes (approx.)");
            Logging.Log(">> >>DETAILED PROCESS INFORMATION<< <<", Logging.CompileDetails(), ">> >>END DPI<< <<");
            Logging.Log($"Writing log to file at {LogPath} and shutting down... Goodbye!");
            Logging.FinalFlush(true).GetAwaiter().GetResult();
            App.Current.Shutdown();
        }
    }
}