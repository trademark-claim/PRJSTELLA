//#define THROWERRORS
using System.Diagnostics;
using System.Windows;

namespace Cat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly static DateTime starttime = DateTime.Now;
        private static long maxMemory = 0;
        private static bool isShuttingDown = false;

        internal static bool IsShuttingDown => isShuttingDown;

        protected override void OnStartup(StartupEventArgs e)
        {
            Current.DispatcherUnhandledException += (s, e) =>
            {
                Exception exc = e.Exception;
                Logging.LogError(exc);
                e.Handled = true;
                Logging.FullFlush().GetAwaiter().GetResult();
                throw exc;
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Exception? exc = e.ExceptionObject as Exception;
                if (exc != null)
                    Logging.Log(exc);
                Logging.FullFlush().GetAwaiter().GetResult();
                throw exc;
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                var exc = e.Exception;
                Logging.LogError(exc);
                e.SetObserved();
                Logging.FullFlush().GetAwaiter().GetResult();
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
            // Attaches the shutdown sequence to app exit, should run it when the app is closed through non-planned methods.
            this.Exit += (sender, e) => ShuttingDown();
        }

        internal static async void ShuttingDown()
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
            await Logging.FullFlush(true);
            App.Current.Shutdown();
        }
    }
}