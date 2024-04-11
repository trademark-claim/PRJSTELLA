using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Cat
{
    /// <summary>
    /// Provides logging functionalities for the application.
    /// </summary>
    internal static class Logging
    {
        private static LogWindow? inst;
        private static readonly Queue<string> logQueue = new Queue<string>();
        private static readonly object logLock = new object();
        private static System.Threading.Timer logFlushTimer;
        private static readonly SemaphoreSlim fileWriteSemaphore = new SemaphoreSlim(1, 1);
        private static readonly int MaxQueueSize = 500;

        /// <summary>
        /// Displays the live logger window.
        /// </summary>
        /// <returns>The opened logger window.</returns>
        internal static Window ShowLogger()
        {
            Log("Showing Live Logger...");
            inst = new LogWindow();
            inst.Show();
            Log("Live logger opened");
            return inst;
        }

        /// <summary>
        /// Logs an error with detailed information.
        /// </summary>
        /// <param name="exc">The exception to log.</param>
        /// <param name="initial">Indicates if this is the initial error in a potential chain of errors.</param>
        internal static async void LogError(Exception exc, bool initial = true)
        {
            Log($">>>ERROR START<<<\nMessage:\n   {exc.Message}\nSource:\n   {exc.Source}\nMethod Base:\n   {exc.TargetSite?.Module}.{exc.TargetSite?.DeclaringType}.{exc.TargetSite?.Name} ({exc.TargetSite})\nStackTrace:\n   {exc.StackTrace}\nData:\n   {string.Join("   \n- ", exc.Data.Keys.Cast<object>().Zip(exc.Data.Values.Cast<object>()))}\nHLink:\n   {exc.HelpLink}\nHResult:\n   {exc.HResult}\n>>>END OF ERROR<<<");
            if (exc.InnerException != null)
            {
                Log("The above was the cause of the following exception: ");
                LogError(exc.InnerException, false);
            }
            if (initial)
            {
                await FinalFlush();
            }
        }

        /// <summary>
        /// Hides the live logger window if it's open.
        /// </summary>
        internal static void HideLogger()
        {
            Log("Closing Live logger...");
            inst?.Close();
            inst = null;
            Log("Live logger closed.");
        }

        /// <summary>
        /// Initializes static members of the <see cref="Logging"/> class. Sets up a timer for flushing logs to file periodically.
        /// </summary>
        static Logging()
        {
            logFlushTimer = new System.Threading.Timer((e) =>
            {
                FlushLogToFile();
            }, null, 10000, 300000);
        }

        /// <summary>
        /// Flushes log messages from the queue to a file synchronously.
        /// </summary>
        private static void FlushLogToFile()
        {
            lock (logLock)
            {
                while (logQueue.Count > 0)
                {
                    WriteLog(logQueue.Dequeue());
                }
            }
        }

        /// <summary>
        /// Writes a log message to the log file asynchronously.
        /// </summary>
        /// <param name="text">The log message to be written.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task WriteLog(string text)
        {
            await fileWriteSemaphore.WaitAsync();
            try
            {
                File.AppendAllText(LogPath, text);
            }
            finally
            {
                fileWriteSemaphore.Release();
            }
        }

        /// <summary>
        /// Logs a series of messages with special processing.
        /// </summary>
        /// <param name="messages">The messages to log.</param>
        internal static void LogP(params object[] messages)
        {
            foreach (var message in messages)
            {
                string processed = ProcessMessage(message, 0);
                Log(processed);
            }
            Log(System.Environment.NewLine, true);
        }

        /// <summary>
        /// Logs one or more messages to the logging system.
        /// </summary>
        /// <param name="message">The message(s) to log.</param>
        internal static void Log(params object[] message)
        {
            foreach (object mess in message)
                Log(mess?.ToString());
            Log(System.Environment.NewLine, true);
        }


        /// <summary>
        /// Logs a single message to the logging system, optionally without prepending the datetime stamp.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="nodatetime">If true, does not prepend the datetime stamp to the log message.</param>
        private static void Log(string message, bool nodatetime = false)
        {
            if (UserData.FullLogging)
            {
                string currentTime = DateTime.Now.ToString("HH:mm:ss:fff");
                string formattedMessage = $"[{currentTime}] {message}"; //+ System.Environment.NewLine
                if (nodatetime)
                    formattedMessage = $"{message}{System.Environment.NewLine}";
                lock (logLock)
                {
                    logQueue.Enqueue(formattedMessage);
                    if (logQueue.Count >= MaxQueueSize)
                    {
                        FlushLogToFile();
                    }
                }
                System.Windows.Application.Current.Dispatcher.Invoke(() => inst?.AddLog(message));
            }
        }

        /// <summary>
        /// Processes and formats a message or enumerable collection for logging.
        /// </summary>
        /// <param name="message">The message or enumerable collection to process.</param>
        /// <param name="indentLevel">The indent level for formatting enumerable collections.</param>
        internal static string ProcessMessage(object message, int indentLevel)
        {
            if (message == null)
                return string.Empty;

            if (message is IEnumerable enumerable && !(message is string))
            {
                var sb = new StringBuilder("Enum:");
                sb.AppendLine();
                foreach (var item in enumerable)
                    sb.AppendLine(new string(' ', indentLevel * 2) + "- " + ProcessMessage(item, indentLevel + 1));
                return sb.ToString().TrimEnd();
            }
            else
                return message.ToString();
        }

        /// <summary>
        /// Performs a final flush of log messages to the log file asynchronously. Optionally marks the log with an end statement.
        /// </summary>
        /// <param name="end">If true, appends an end log statement.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal static async Task FinalFlush(bool end = false)
        {
            await fileWriteSemaphore.WaitAsync();
            try
            {
                StringBuilder finalLog = new StringBuilder();
                lock (logLock)
                {
                    while (logQueue.Count > 0)
                    {
                        finalLog.Append(logQueue.Dequeue());
                    }
                }
                if (end)
                {
                    finalLog.AppendLine("[END LOG Nya~]");
                }
                File.AppendAllText(LogPath, finalLog.ToString());
            }
            finally
            {
                fileWriteSemaphore.Release();
            }
        }

        /// <summary>
        /// A window dedicated to displaying real-time logs.
        /// </summary>
        private class LogWindow : Window
        {
            private readonly SWC.ListBox _listBox;
            private ScrollViewer _scrollViewer;

            /// <summary>
            /// Initializes a new instance of the <see cref="LogWindow"/> class, creating the log viewer UI.
            /// </summary>
            public LogWindow()
            {
                Log("Creating Log Window...");
                inst?.Close();
                inst = this;
                Title = "Log Viewer";
                Width = 500;
                Height = 400;

                Background = System.Windows.Media.Brushes.Black;
                _listBox = new()
                {
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    FontSize = 12,
                    Foreground = System.Windows.Media.Brushes.White,
                    Background = System.Windows.Media.Brushes.Black,
                };

                _listBox.Loaded += (s, e) =>
                {
                    _scrollViewer = GetScrollViewer(_listBox);
                };

                _listBox.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
                _listBox.SetValue(VirtualizingStackPanel.VirtualizationModeProperty, VirtualizationMode.Recycling);
                _listBox.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(VirtualizingStackPanel)));
                _scrollViewer = GetScrollViewer(_listBox);
                Content = _listBox;
                Log("Logging Window Created");
            }

            /// <summary>
            /// Adds a log message to the log viewer.
            /// </summary>
            /// <param name="logMessage">The log message to add.</param>
            public void AddLog(string logMessage)
            {
                inst?.Dispatcher.Invoke(() => _listBox.Items.Add(logMessage));

                if (_scrollViewer != null)
                    _scrollViewer.ScrollToEnd();
                else
                    _listBox.ScrollIntoView(_listBox.Items[_listBox.Items.Count - 1]);
            }

            /// <summary>
            /// Adds a colored text log message to the log viewer.
            /// </summary>
            /// <param name="logMessage">The log message to add.</param>
            /// <param name="color">The color of the text.</param>
            public void AddTextLog(string logMessage, System.Windows.Media.Color color)
            {
                inst?.Dispatcher.Invoke(() => _listBox.Items.Add(new TextBlock() { Text = logMessage, Foreground = new SolidColorBrush(color) }));

                if (_scrollViewer != null)
                    _scrollViewer.ScrollToEnd();
                else
                    _listBox.ScrollIntoView(_listBox.Items[_listBox.Items.Count - 1]);
            }
        }

        /// <summary>
        /// Compiles various system and process details into a single string for logging.
        /// </summary>
        /// <returns>A string containing detailed system and process information.</returns>
        internal static string CompileDetails()
        {
            StringBuilder sb = new();
            if (UserData.LoggingDetails)
            {
                sb.AppendLine(GetProcessDetails());
                sb.AppendLine(GHI());
                sb.AppendLine(GSI());
                sb.AppendLine(GNI());
                sb.AppendLine(GCLI());
            }
            if (UserData.AssemblyInformation)
                sb.AppendLine(GLAI());
            if (UserData.EnvironmentVariables)
                sb.AppendLine(GEV());
            return sb.ToString();
        }

        /// <summary>
        /// Gathers detailed process information.
        /// </summary>
        /// <returns>A string containing detailed process information.</returns>
        private static string GetProcessDetails()
        {
            return $"System Information" +
                    $"\n ├──Operating System: {System.Environment.OSVersion}" +
                    $"\n ├──.NET Framework Version: {System.Environment.Version}" +
                    $"\n ├──Machine Name: {System.Environment.MachineName}" +
                    $"\n ├──User Domain: {System.Environment.UserDomainName}" +
                    $"\n └──Is 64 Bit: {System.Environment.Is64BitOperatingSystem}" +
                    $"\nProcess Information" +
                    $"\n ├──Process ID: {System.Environment.ProcessId}" +
                    $"\n ├──Process Name: {Process.GetCurrentProcess().ProcessName}" +
                    $"\n ├──Virtal Memory Size: {Process.GetCurrentProcess().VirtualMemorySize64}" +
                    $"\n ├──Working Set Size: {Process.GetCurrentProcess().WorkingSet64}" +
                    $"\n ├──Start Time: {Process.GetCurrentProcess().StartTime}" +
                    $"\n └──User Processor Time: {Process.GetCurrentProcess().UserProcessorTime}" +
                    $"\nThread Information" +
                    $"\n ├──Thread ID: {System.Environment.CurrentManagedThreadId}" +
                    $"\n ├──Thread State: {Thread.CurrentThread.ThreadState}" +
                    $"\n └──Thread Priority: {Thread.CurrentThread.Priority}" +
                    $"\nMemory Information" +
                    $"\n ├──Total Memory: {GC.GetTotalMemory(false)}" +
                    $"\n └──Memory Usage: {Process.GetCurrentProcess().PrivateMemorySize64}" +
                    $"\nDisk Information" +
                    $"\n ├──Current Directory: {System.Environment.CurrentDirectory}" +
                    $"\n └──System Directory: {System.Environment.SystemDirectory}" +
                    $"\nPerformance Metrics" +
                    $"\n ├──CPU Usage: " + new PerformanceCounter("Processor", "% Processor Time", "_Total").NextValue().ToString() +
                    $"\n └──Available Memory: " + new PerformanceCounter("Memory", "Available MBytes").NextValue().ToString() +
                    $"\nUser Session" +
                    $"\n ├──User Name: {System.Environment.UserName}" +
                    $"\n └──Actions: [To be Implemented]" +
                    $"\nDatabase Metrics" +
                    $"\n └── [Undefined]" +
                    $"\nMiscellaneous" +
                    $"\n ├──Enviroment Variables: N/A" +
                    $"\n ├──Loaded Assemblies: {AppDomain.CurrentDomain.GetAssemblies()}" +
                    $"\n └──CMD Line Args: {System.Environment.GetCommandLineArgs()}";
        }

        /// <summary>
        /// Gathers extended hardware information using WMI queries.
        /// </summary>
        /// <returns>A string containing extended hardware information.</returns>
        private static string GHI()
        {
            StringBuilder sb = new StringBuilder("Extended Hardware Information:\n");

            var searcher = new ManagementObjectSearcher("select * from Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                sb.AppendLine($" ├──CPU: {obj["Name"]}");
                sb.AppendLine($" │  ├──Manufacturer: {obj["Manufacturer"]}");
                sb.AppendLine($" │  ├──Cores: {obj["NumberOfCores"]}");
                sb.AppendLine($" │  └──Max Clock Speed: {obj["MaxClockSpeed"]} MHz\n");
                sb.AppendLine($" │  ├──Architecture: {(ProcessorArchitecture)Convert.ToInt32(obj["Architecture"])}");
                sb.AppendLine($" │  ├──Thread Count: {obj["ThreadCount"]}");
                sb.AppendLine($" │  └──L2 Cache Size: {obj["L2CacheSize"]} KB\n");
            }

            searcher = new ManagementObjectSearcher("select * from Win32_PhysicalMemory");
            foreach (var obj in searcher.Get())
            {
                sb.AppendLine($" ├──RAM: {Math.Round(Convert.ToDouble(obj["Capacity"]) / 1024 / 1024 / 1024, 2)} GB");
                sb.AppendLine($" │  ├──Speed: {obj["Speed"]} MHz");
                sb.AppendLine($" │  └──Manufacturer: {obj["Manufacturer"]}\n");
                sb.AppendLine($" ├──RAM Module: {obj["PartNumber"]}");
                sb.AppendLine($" │  ├──Form Factor: {GetMemoryFormFactor(Convert.ToInt32(obj["FormFactor"]))}");
                sb.AppendLine($" │  └──Memory Type: {GetMemoryType(Convert.ToInt32(obj["MemoryType"]))}\n");
            }

            searcher = new ManagementObjectSearcher("select * from Win32_DiskDrive");
            foreach (var obj in searcher.Get())
            {
                sb.AppendLine($" ├──Disk Drive: {obj["Model"]}");
                sb.AppendLine($" │  ├──Type: {obj["MediaType"]}");
                sb.AppendLine($" │  ├──Size: {Math.Round(Convert.ToDouble(obj["Size"]) / 1024 / 1024 / 1024, 2)} GB");
                sb.AppendLine($" │  └──Partitions: {obj["Partitions"]}\n");
                sb.AppendLine($" ├──Disk Drive: {obj["Model"]}");
                sb.AppendLine($" │  ├──Firmware Version: {obj["FirmwareRevision"]}");
                sb.AppendLine($" │  └──Interface Type: {obj["InterfaceType"]}\n");
            }

            var keyboardSearcher = new ManagementObjectSearcher("select * from Win32_Keyboard");
            foreach (var obj in keyboardSearcher.Get())
            {
                sb.AppendLine($" ├──Keyboard: {obj["Description"]}");
                sb.AppendLine($" │  ├──Layout: {obj["Layout"]}");
                sb.AppendLine($" │  ├──Number of Function Keys: {obj["NumberOfFunctionKeys"]}");
                sb.AppendLine($" │  ├──Status: {obj["Status"]}");
                sb.AppendLine($" │  └──Plug and Play Device ID: {obj["PNPDeviceID"]}\n");
            }

            var mouseSearcher = new ManagementObjectSearcher("select * from Win32_PointingDevice");
            foreach (var obj in mouseSearcher.Get())
            {
                sb.AppendLine($" ├──Pointing Device: {obj["Name"]}");
                sb.AppendLine($" │  ├──Manufacturer: {obj["Manufacturer"]}");
                sb.AppendLine($" │  ├──Device Interface: {obj["HardwareType"]}");
                sb.AppendLine($" │  ├──Number of Buttons: {obj["NumberOfButtons"]}");
                sb.AppendLine($" │  ├──Status: {obj["Status"]}");
                sb.AppendLine($" │  └──Plug and Play Device ID: {obj["PNPDeviceID"]}\n");
            }

            return sb.ToString();
        }


        /// <summary>
        /// Gathers security information about the current user session.
        /// </summary>
        /// <returns>A string containing security information.</returns>
        private static string GSI()
        {
            var currentUser = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(currentUser);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Security Information:");
            sb.AppendLine($" ├──User Name: {currentUser.Name}");
            sb.AppendLine($" ├── Actor: {currentUser.Actor}");
            sb.AppendLine($" ├── Impersonation Level: {currentUser.ImpersonationLevel}");
            sb.AppendLine($" ├── Auth Type: {currentUser.AuthenticationType}");
            sb.AppendLine($" ├── Owner: {currentUser.Owner}");
            sb.AppendLine($" ├── System User: {currentUser.IsSystem}");
            sb.AppendLine($" ├── Guest User: {currentUser.IsGuest}");
            sb.AppendLine($" ├── Anon User: {currentUser.IsAnonymous}");
            sb.AppendLine($" ├──Is Authenticated: {currentUser.IsAuthenticated}");
            sb.AppendLine($" └──Is Administrator: {principal.IsInRole(WindowsBuiltInRole.Administrator)}");
            return sb.ToString();
        }

        /// <summary>
        /// Gathers detailed network information.
        /// </summary>
        /// <returns>A string containing detailed network information.</returns>
        private static string GNI()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Detailed Network Information:");
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetEnabled = TRUE");

                foreach (ManagementObject obj in searcher.Get())
                {
                    sb.AppendLine($" ├──Adapter Name: {obj["Name"]}");
                    sb.AppendLine($" │  ├──Description: {obj["Description"]}");
                    sb.AppendLine($" │  ├──Status: {obj["Status"]}");
                    sb.AppendLine($" │  ├──Speed: {(ulong)obj["Speed"] / 1_000_000} Mbps");
                    sb.AppendLine($" │  ├──MAC Address: {obj["MACAddress"]}");
                    sb.AppendLine($" │  ├──Manufacturer: {obj["Manufacturer"]}");
                    sb.AppendLine($" │  ├──Net Connection ID: {obj["NetConnectionID"]}");
                    sb.AppendLine($" │  └──Adapter Type: {obj["AdapterType"]}\n");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error retrieving network information: {ex.Message}");
            }

            var ipProperties = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(ni => new
                {
                    ni.Description,
                    ni.Name,
                    IPAddresses = ni.GetIPProperties().UnicastAddresses.Select(ip => ip.Address.ToString()).ToList()
                });

            foreach (var prop in ipProperties)
            {
                sb.AppendLine($" ├──Interface: {prop.Name} ({prop.Description})");
                foreach (var ip in prop.IPAddresses)
                {
                    sb.AppendLine($" │  └──IP Address: {ip}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gathers command line arguments passed to the application.
        /// </summary>
        /// <returns>A string containing the command line arguments.</returns>
        private static string GCLI()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Command Line Arguments:");
            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                sb.AppendLine($" ├──{arg}");
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Gathers information about loaded assemblies.
        /// </summary>
        /// <returns>A string containing information about loaded assemblies.</returns>
        private static string GLAI()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Loaded Assemblies:");
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                sb.AppendLine($" ├──Name: {assembly.FullName}");
                sb.AppendLine($" ├──Attributes:\n -  {string.Join("\n - ", assembly.CustomAttributes)}");
                sb.AppendLine($" ├──Image RTVer: {assembly.ImageRuntimeVersion}");
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Gathers environment variables.
        /// </summary>
        /// <returns>A string containing the environment variables.</returns>
        private static string GEV()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Environment Variables:");
            foreach (DictionaryEntry de in System.Environment.GetEnvironmentVariables())
            {
                sb.AppendLine($" ├──{de.Key}: {de.Value}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts a memory form factor code into a human-readable format.
        /// </summary>
        /// <param name="code">The memory form factor code.</param>
        /// <returns>A string representing the memory form factor.</returns>
        private static string GetMemoryFormFactor(int code)
        {
            switch (code)
            {
                case 0x09: return "DIMM";
                case 0x0D: return "SODIMM";
                case 0x0F: return "Micro-DIMM";
                case 0x10: return "Mini-DIMM";
                case 0x19: return "72-pin SO-DIMM";
                case 0x1A: return "144-pin SO-DIMM";
                case 0x1B: return "168-pin DIMM";
                case 0x1C: return "184-pin DIMM";
                case 0x1D: return "200-pin SO-DIMM";
                default: return "Unknown";
            }
        }

        /// <summary>
        /// Converts a memory type code into a human-readable format.
        /// </summary>
        /// <param name="code">The memory type code.</param>
        /// <returns>A string representing the memory type.</returns>
        internal static string GetMemoryType(int code)
        {
            switch (code)
            {
                case 0x01: return "Other";
                case 0x02: return "Unknown";
                case 0x03: return "DRAM";
                case 0x04: return "EDRAM";
                case 0x05: return "VRAM";
                case 0x06: return "SRAM";
                case 0x07: return "RAM";
                case 0x08: return "ROM";
                case 0x09: return "FLASH";
                case 0x0A: return "EEPROM";
                case 0x0B: return "FEPROM";
                case 0x0C: return "EPROM";
                case 0x0D: return "CDRAM";
                case 0x0E: return "3DRAM";
                case 0x0F: return "SDRAM";
                case 0x10: return "SGRAM";
                case 0x11: return "RDRAM";
                case 0x12: return "DDR";
                case 0x13: return "DDR2";
                case 0x14: return "DDR2 SDRAM FB-DIMM";
                case 0x15: return "Reserved";
                case 0x16: return "Reserved";
                case 0x17: return "Reserved";
                case 0x18: return "DDR3";
                case 0x19: return "FBD2";
                case 0x1A: return "DDR4";
                case 0x1B: return "LPDDR";
                case 0x1C: return "LPDDR2";
                case 0x1D: return "LPDDR3";
                case 0x1E: return "LPDDR4";
                case 0x1F: return "Logical non-volatile device";
                case 0x20: return "HBM";
                case 0x21: return "HBM2";
                case 0x22: return "DDR5";
                case 0x23: return "LPDDR5";
                // Check the latest SMBIOS specification for new memory types.
                default: return "Unknown";
            }
        }

        /// <summary>
        /// Represents a progress logging mechanism that updates a log with the current progress.
        /// </summary>
        internal class ProgressLogging
        {
            private byte progress = 0;
            private string title;
            private readonly bool @interface;

            private event Action<ProgressUpdateEventArgs> OnProgressUpdate;

            private TextBlock block;

            /// <summary>
            /// Initializes a new instance of the <see cref="ProgressLogging"/> class.
            /// </summary>
            /// <param name="title">The title of the progress log.</param>
            /// <param name="LogToInterface">A value indicating whether the log should be output to an interface.</param>
            /// <remarks>
            /// When <paramref name="LogToInterface"/> is true, progress updates are displayed on the user interface.
            /// Otherwise, progress updates are logged internally.
            /// </remarks>
            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            [LoggingAspects.InterfaceNotice]
            internal ProgressLogging(string title, bool LogToInterface)
            {
                @interface = LogToInterface;
                if (@interface)
                {
                    //Catowo.Interface.progresses.Add(this);
                    (_, block) = Catowo.Interface.AddTextLogR($"{title} [----------------------------------------------------------------------------------------------------]");
                }

                OnProgressUpdate += (puea) =>
                {
                    progress = puea.NewProgress;
                    Log(title + "Progress: " + progress + "%");
                    if (puea.Note != null)
                        Log("Note: " + puea.Note);
                    if (@interface)
                    {
                        string bar = title + " [" + string.Concat(Enumerable.Repeat("|", progress)) + string.Concat(Enumerable.Repeat("-", 100 - progress)) + "]";
                        if (block != null)
                            block.Text = bar;
                        Catowo.Interface.logListBox.Items.Refresh();
                    }
                    if (progress == 100)
                    {
                        Log(title + "Complete!");
                        if (@interface)
                            Catowo.Interface.AddLog(title + " Compelte!");
                        block = null;
                    }
                };
            }

            /// <summary>
            /// Invokes the progress update event.
            /// </summary>
            /// <param name="e">The progress update event arguments containing the new progress value and an optional note.</param>
            internal void InvokeEvent(ProgressUpdateEventArgs e)
            {
                Logging.Log("Progress Invocation");
                OnProgressUpdate?.Invoke(e);
            }

            /// <summary>
            /// Provides data for the progress update event.
            /// </summary>
            internal class ProgressUpdateEventArgs
            {
                /// <summary>
                /// Gets the new progress value.
                /// </summary>
                internal byte NewProgress { get; private set; }

                /// <summary>
                /// Gets an optional note associated with the progress update.
                /// </summary>
                internal string? Note { get; private set; }

                /// <summary>
                /// Initializes a new instance of the <see cref="ProgressUpdateEventArgs"/> class.
                /// </summary>
                /// <param name="np">The new progress value.</param>
                internal ProgressUpdateEventArgs(byte np)
                {
                    NewProgress = np;
                }
            }


            /// <summary>
            /// Represents an animated loading indicator.
            /// </summary>
            internal class SpinnyThing
            {
                private readonly string[] animation = { "|", "/", "-", "\\" };
                private TextBlock block;
                private byte num = 0;
                private readonly DispatcherTimer timer;

                /// <summary>
                /// Initializes a new instance of the <see cref="SpinnyThing"/> class and starts the animation.
                /// </summary>
                internal SpinnyThing()
                {
                    (_, block) = Catowo.Interface.AddTextLogR(animation[num++]);
                    timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromMilliseconds(50);
                    timer.Tick += Timer_Tick;
                    timer.Start();
                }

                /// <summary>
                /// Handles the timer tick event to cycle through animation frames.
                /// </summary>
                /// <param name="sender">The source of the event.</param>
                /// <param name="e">An EventArgs object that contains the event data.</param>
                private void Timer_Tick(object sender, EventArgs e)
                {
                    block.Text = animation[num++];
                    if (num == animation.Length) num = 0;
                    Catowo.Interface.logListBox.Items.Refresh();
                }

                /// <summary>
                /// Stops the animation and removes the animation block from the log list box.
                /// </summary>
                internal void Stop()
                {
                    timer.Stop();
                    timer.Tick -= Timer_Tick;
                    Catowo.Interface.logListBox.Items.Remove(block);
                }
            }
        }
    }
}