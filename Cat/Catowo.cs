#define ImmedShutdown

global using static Cat.BaselineInputs;
global using static Cat.Environment;
global using static Cat.Objects;
global using static Cat.PInvoke;
global using static Cat.Statics;
global using static Cat.Structs;
global using SWC = System.Windows.Controls;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;
using SWM = System.Windows.Media;
using NAudio.Wave;
using SWS = System.Windows.Shapes;
using Microsoft.VisualBasic.Devices;
using System.Printing;
using System.IO;

namespace Cat
{
    internal class Catowo : Window
    {
        
        internal static Catowo inst;
        internal static bool ShuttingDown = false;
        internal static IntPtr keyhook = IntPtr.Zero;

        private readonly SWC.Canvas canvas = new SWC.Canvas();
        internal readonly SWC.Label DebugLabel = new();

        internal int originalStyle = 0, editedstyle = 0;
        internal IntPtr hwnd = IntPtr.Zero;

        private bool RShifted = false, Qd = false, LShifted = false, isCursor = true;

        #region Markers

        private readonly SWS.Ellipse DEBUGMARKER = new()
        {
            Fill = Statics.WHITE,
            Width = 10,
            Height = 10,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        private readonly SWS.Ellipse FUNTMARKER = new()
        {
            Fill = Statics.GREEN,
            Width = 10,
            Height = 10,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        private readonly SWS.Ellipse DANGERMARKER = new()
        {
            Fill = Statics.RED,
            Width = 10,
            Height = 10,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        private readonly SWS.Ellipse SHORTCUTSMARKER = new()
        {
            Fill = Statics.BLUE,
            Width = 10,
            Height = 10,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        #endregion Markers

        private Modes mode = Modes.None;

        private Modes Mode
        {
            get => mode; set
            {
                mode = value;
                Logging.Log($"Mode set to {((ushort)mode)} ({mode})");
                ToggleVis(DEBUGMARKER, mode.HasFlag(Modes.DEBUG));
                ToggleVis(DebugLabel, mode.HasFlag(Modes.DEBUG));
                ToggleVis(FUNTMARKER, mode.HasFlag(Modes.Functionality));
                ToggleVis(DANGERMARKER, mode.HasFlag(Modes.DANGER));
                ToggleVis(SHORTCUTSMARKER, mode.HasFlag(Modes.Shortcuts));
            }
        }

        private static int _screen_ = Array.FindIndex(System.Windows.Forms.Screen.AllScreens, screen => screen.Primary);

        internal int Screen
        {
            get => _screen_; 
            set
            {
                if (value != _screen_)
                {
                    if (value >= 0 && value < System.Windows.Forms.Screen.AllScreens.Length)
                    {
                        ToggleInterface();
                        Screen screen = System.Windows.Forms.Screen.AllScreens[value];
                        var (width, height, working) = Helpers.ScreenSizing.GetAdjustedScreenSize(screen);
                        Top = screen.Bounds.Top;
                        Left = screen.Bounds.Left;
                        Width = width;
                        Height = height - working;
                        Logging.LogP("New Screen Params: ", Top, Left, Width, Height);
                        _screen_ = value;
                        ToggleInterface();
                    }
                }
            }
        }

        #region Low Levels

        private void InitKeyHook()
        {
            Logging.Log("Setting key hook protocal...");
            _keyboardProc = KeyboardProc;
            Logging.Log("hooking...");
            _keyboardHookID = SetKeyboardHook(_keyboardProc);
            Logging.Log($"Hooking protocal {_keyboardProc} hooked with nint {_keyboardHookID}");
            keyhook = _keyboardHookID;
        }

        internal static void DestroyKeyHook()
        {
            Logging.Log("Unhooking key hook...");
            if (_keyboardHookID == IntPtr.Zero)
            {
                Logging.Log("Key hook is default, exiting.");
                return;
            }
            bool b = UnhookWindowsHookExWrapper(_keyboardHookID);
            Logging.Log($"Unhooking successful: {b}");
            if (b)
                keyhook = IntPtr.Zero;
        }

        private static IntPtr SetKeyboardHook(LowLevelKeyboardProc proc)
        {
            Logging.Log("Initing Keyboard hook...");
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                Logging.Log("Hook initiated, setting...");
                return SetWindowsHookExWrapper(WH_KEYBOARD_LL, proc, GetModuleHandleWrapper(curModule.ModuleName), 0);
            }
        }

        private IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            bool isKeyDown = nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN;
            bool isKeyUp = nCode >= 0 && wParam == (IntPtr)WM_KEYUP;
            string log = $"Key{(isKeyDown ? "KeyDown" : "KeyUp")}: {(Keys)vkCode} ({vkCode})" + (RShifted ? " (rshifted) " : "") + (LShifted ? " (Lshifted) " : "") + (Qd ? " (q) " : "");
            Logging.Log(log);
            if (isKeyDown)
            {
                DebugLabel.Content += $"A {Qd}, {RShifted}, {LShifted}, {vkCode}, {wParam}, {(Keys)vkCode}";
                if (!Qd && vkCode == VK_Q)
                {
                    Qd = true;
                    return CallNextHookExWrapper(_keyboardHookID, nCode, wParam, lParam);
                }
                if (!RShifted && vkCode == VK_RSHIFT)
                {
                    RShifted = true;
                    return CallNextHookExWrapper(_keyboardHookID, nCode, wParam, lParam);
                }
                if (!LShifted && vkCode == VK_LSHIFT)
                {
                    LShifted = true;
                    return CallNextHookExWrapper(_keyboardHookID, nCode, wParam, lParam);
                }
                if (Qd)
                {
                    if (RShifted && LShifted)
                    {
                        switch (vkCode)
                        {
                            case VK_E:
                                if (!ShuttingDown)
                                {
                                    ShuttingDown = true;
                                    App.ShuttingDown();
                                }
                                break;

                            case VK_1:
                                Mode ^= Modes.Shortcuts;
                                break;

                            case VK_2:
                                Mode ^= Modes.Functionality;
                                break;

                            case VK_3:
                                Mode ^= Modes.DANGER;
                                break;

                            case VK_4:
                                Mode ^= Modes.DEBUG;
                                break;

                            case VK_0:
                                Mode ^= Modes.DANGER | Modes.Shortcuts | Modes.Functionality | Modes.DEBUG;
                                break;

                            case VK_I:
                                ToggleInterface();
                                break;

                            default:
                                break;
                        }
                    }
                    else
                    {
                        if (mode.HasFlag(Modes.DEBUG))
                        {
                            switch (vkCode)
                            {
                                case VK_K:
                                    SendHello();
                                    return new IntPtr(1);

                                case VK_M:
                                    if (isCursor)
                                        HideCursor();
                                    else
                                        ShowCursor();
                                    isCursor = !isCursor;
                                    break;

                                case VK_S:
                                    ToggleMuteSound();
                                    break;

                                case VK_B:
                                    if (BaselineInputs.Cursor.CurrentCursor == BaselineInputs.Cursor.CursorType.Default)
                                    {
                                        BaselineInputs.Cursor.BlackPoint();
                                    }
                                    else
                                    {
                                        BaselineInputs.Cursor.Reset();
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }
                        else if (mode.HasFlag(Modes.DANGER))
                        {
                            switch (vkCode)
                            {
                                case VK_W:
                                    if (LShifted)
                                        CauseMouseToHaveSpasticAttack(true);
                                    else
                                        CauseMouseToHaveSpasticAttack();
                                    break;

                                default:
                                    break;
                            }
                        }
                        else if (mode.HasFlag(Modes.Functionality))
                        {
                            switch (vkCode)
                            {
                                case VK_S:
                                    ShutDownScreen.ToggleScreen(canvas);
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                    return new IntPtr(1);
                }
            }
            else if (isKeyUp)
            {
                switch (vkCode)
                {
                    case VK_Q:
                        Qd = false;
                        break;

                    case VK_RSHIFT:
                        RShifted = false;
                        break;

                    case VK_LSHIFT:
                        LShifted = false;
                        break;
                }
            }
            return CallNextHookExWrapper(_keyboardHookID, nCode, wParam, lParam);
        }

        #endregion Low Levels

        #region Catowo Creation and Init

        public Catowo()
        {
            Logging.Log("Creating Catowo Window...");
            inst?.Close();
            inst = this;
            Logging.Log("Initialising objects...");
            InitializeWindow();
            CreateVisibleObjects();
            Logging.Log("Objects Initialised");
            InitKeyHook();
            Logging.Log("Catowo Window created!");
            Mode = Modes.None;
        }

        ~Catowo()
        {
            Logging.Log("Cleaning up Catowo...");
            DestroyKeyHook();
            Logging.Log("Catowo Destroyed.");
        }

        internal static Screen GetScreen()
        {
            try
            {
                return System.Windows.Forms.Screen.AllScreens[_screen_];
            }
            catch
            {
                return System.Windows.Forms.Screen.PrimaryScreen;
            }
        }

        private void InitializeWindow()
        {
            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;
            Background = System.Windows.Media.Brushes.Transparent;
            Topmost = true;
            ShowActivated = false;
            ShowInTaskbar = false;
            Left = 0;
            Top = 0;
            _screen_ = Array.FindIndex(System.Windows.Forms.Screen.AllScreens, screen => screen.Primary);
            var scre = GetScreen();
            Width = scre.Bounds.Width;
            Height = scre.Bounds.Height;
            Logging.Log($"Width: {Width}", $"Height: {Height}");
            //System.Windows.MessageBox.Show($"{_screen_}, {Width}, {Height}");

            Loaded += (sender, e) =>
            {
                hwnd = new WindowInteropHelper(this).Handle;
                originalStyle = GetWindowLongWrapper(hwnd, GWL_EXSTYLE);
                SetWindowLongWrapper(hwnd, GWL_EXSTYLE, originalStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
                editedstyle = GetWindowLongWrapper(hwnd, GWL_EXSTYLE);
                Logging.Log($"Set Win Style of Handle {hwnd} from {originalStyle:X} ({originalStyle:B}) [{originalStyle}] to {editedstyle:X} ({editedstyle:B}) [{editedstyle}]");
            };
        }

        private void CreateVisibleObjects()
        {
            canvas.Children.Add(DEBUGMARKER);
            canvas.Children.Add(FUNTMARKER);
            Statics.SetLeft(FUNTMARKER, 4);
            canvas.Children.Add(SHORTCUTSMARKER);
            Statics.SetLeft(SHORTCUTSMARKER, 8);
            canvas.Children.Add(DANGERMARKER);
            Statics.SetLeft(DANGERMARKER, 12);
            canvas.Children.Add(DebugLabel);
            Mode = Modes.None;
            DebugLabel.Foreground = new SolidColorBrush(Colors.LimeGreen);

            this.Content = canvas;
        }

        [Flags]
        private enum Modes : sbyte
        {
            None = 0,
            Shortcuts = 1,
            DANGER = 2,
            Functionality = 4,
            DEBUG = 8
        }

        #endregion Catowo Creation and Init

        #region Interface

        private void ToggleInterface()
        {
            if (Interface.inst != null)
            {
                Interface.inst?.Children.Clear();
                Interface.inst?.parent?.Children.Remove(inst);
                Interface.inst = null;
                Logging.Log($"Changing WinStyle of HWND {hwnd}");
                int os = GetWindowLongWrapper(hwnd, GWL_EXSTYLE);
                SetWindowLongWrapper(hwnd, GWL_EXSTYLE, editedstyle);
                int es = GetWindowLongWrapper(hwnd, GWL_EXSTYLE);
                Logging.Log($"Set WinStyle of HWND {hwnd} from {os:X} ({os:B}) [{os}] to {es:X} ({es:B}) [{es}]");
                InitKeyHook();
            }
            else
            {
                Logging.Log($"Changing WinStyle of HWND {hwnd}");
                int os = GetWindowLongWrapper(hwnd, GWL_EXSTYLE);
                SetWindowLongWrapper(hwnd, GWL_EXSTYLE, originalStyle | WS_EX_LAYERED | WS_EX_TOOLWINDOW);
                int es = GetWindowLongWrapper(hwnd, GWL_EXSTYLE);
                Logging.Log($"Set WinStyle of HWND {hwnd} from {os:X} ({os:B}) [{os}] to {es:X} ({es:B}) [{es}]");
                canvas.Children.Add(new Interface(canvas));
                DestroyKeyHook();
            }
        }

        internal class Interface : Canvas
        {
            private readonly SWS.Rectangle Backg;
            //private readonly SWS.Rectangle Marker = new() { Width = 3, Height = 3, Fill = new SolidColorBrush(Colors.AliceBlue) };
            private SWC.TextBox inputTextBox;
            private static LogListBox logListBox = new();
            internal static Interface? inst = null;
            internal Canvas parent;
            private static ScrollViewer _scrollViewer;

            internal Interface(Canvas parent)
            {
                this.parent = parent;
                CommandProcessing.@interface = this;
                inst?.Children.Clear();
                inst?.parent.Children.Remove(inst);
                inst = null;
                inst = this;
                Backg = InitBackg();
                InitializeComponents();
                //MouseMove += (s, e) => Catowo.inst.ToggleInterface();
            }

            private SWS.Rectangle InitBackg()
            {
                var scre = GetScreen();
                SWS.Rectangle rect = new SWS.Rectangle { Width = scre.Bounds.Width, Height = scre.Bounds.Height, Fill = new SWM.SolidColorBrush(SWM.Colors.Gray), Opacity = 0.8f };
                Logging.Log($"{Catowo.inst.Screen}, {rect.Width} {rect.Height}");
                SetTop(rect, 0);
                SetLeft(rect, 0);
                Children.Add(rect);
                return rect;
            }

            private void InitializeComponents()
            {
                Screen screen = GetScreen();
                var (screenWidth, screenHeight, workAreaHeight) = Helpers.ScreenSizing.GetAdjustedScreenSize(screen);
                double taskbarHeight = screenHeight - workAreaHeight;
                double padding = 20;
                double inputTextBoxHeight = 30;

                inputTextBox = new SWC.TextBox
                {
                    Width = screenWidth - (padding * 2),
                    Height = inputTextBoxHeight,
                    Margin = new Thickness(0, 0, 0, padding),
                    Background = SWM.Brushes.White,
                    Foreground = SWM.Brushes.Black,
                };

                inputTextBox.KeyDown += (s, e) => { if (e.Key == Key.Enter) CommandProcessing.ProcessCommand(); };

#if TESTCOMMANDS
                inputTextBox.Text = "dsi";
                CommandProcessing.ProcessCommand();
                inputTextBox.Text = "ped";
                CommandProcessing.ProcessCommand();
#endif

                SetLeft<double>(inputTextBox, padding);
                SetTop<double>(inputTextBox, screenHeight - taskbarHeight - inputTextBoxHeight - (padding));

                logListBox.Width = screenWidth - (padding * 2);
                logListBox.Height = screenHeight - taskbarHeight - inputTextBoxHeight - (padding * 3);


                SetLeft(logListBox, padding);
                SetTop(logListBox, padding);
                Children.Add(inputTextBox);
                Children.Add(logListBox);
                Catowo.inst.Focus();
                inputTextBox.Focus();

                //Children.Add(Marker);
                //SetLeft(Marker, screenWidth - 10);
                //SetTop(Marker, screenHeight - 10);
            }

            internal async Task Hide()
            {
                Logging.Log("Hiding interface...");
                Visibility = Visibility.Collapsed;
                Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                await Task.Delay(500);
                Logging.Log("Interface hidden");
            }
            
            internal void Show()
            {
                Logging.Log("Showing interface...");
                Visibility = Visibility.Visible;
                Logging.Log("Interface Shown");
            }

            internal void AddLog(string logMessage)
            {
                Dispatcher.Invoke(() => logListBox.Items.Add(logMessage));
                if (_scrollViewer != null)
                    _scrollViewer.ScrollToEnd();
                else
                    logListBox.ScrollIntoView(logListBox.Items[logListBox.Items.Count - 1]);
            }

            internal void AddLog(params string[] logs)
            {
                foreach (string log in logs)
                    Dispatcher.Invoke(() => logListBox.Items.Add(log));
                if (_scrollViewer != null)
                    _scrollViewer.ScrollToEnd();
                else
                    logListBox.ScrollIntoView(logListBox.Items[logListBox.Items.Count - 1]);
            }

            internal void AddTextLog(string logMessage, SWM.Color color)
            {
                Dispatcher.Invoke(() => logListBox.Items.Add(new TextBlock { Text = logMessage, Foreground = new SolidColorBrush(color) }));
                if (_scrollViewer != null)
                    _scrollViewer.ScrollToEnd();
                else
                    logListBox.ScrollIntoView(logListBox.Items[logListBox.Items.Count - 1]);
            }

            internal void AddTextLog(string logMessage, SolidColorBrush brush)
            {
                Dispatcher.Invoke(() => logListBox.Items.Add(new TextBlock { Text = logMessage, Foreground = brush }));
                if (_scrollViewer != null)
                    _scrollViewer.ScrollToEnd();
                else
                    logListBox.ScrollIntoView(logListBox.Items[logListBox.Items.Count - 1]);
            }

            private static class CommandProcessing
            {
                internal static Interface @interface;
                private static IWavePlayer WavePlayer;
                private static AudioFileReader AFR;
                private static bool SilentAudioCleanup = false;
                private static Window? Logger = null;
                private static readonly Dictionary<string, int> cmdmap = new()
                {
                    { "shutdown", 0 },
                    { "exit", 0 },
                    { "std", 0 },
                    
                    { "close", 1 },
                    { "hide", 1 },
                    { "cls", 1 },

                    { "move", 2 },
                    { "change screen", 2 },
                    { "relocate", 2 },
                    
                    { "screenshot", 3 },
                    { "ss", 3 },
                    { "take screenshot", 3 },
                    { "screen capture", 3 },
                    
                    { "capture video", 4 },
                    { "cv", 4 },
                    { "begin video capture", 4 },
                    { "start video capture", 4 },
                    { "record screen", 4 },
                    { "start recording", 4 },

                    { "capture screen", 5 },
                    { "ca", 5 },
                    { "begin audio capture", 5 },
                    { "start audio capture", 5 },
                    { "record audio", 5 },
                    { "start recording audio", 5 },
                    
                    { "stop video", 6 },
                    { "sv", 6 },
                    { "stop video recording", 6 },
                    { "stop recording video", 6 },
                    { "stop video capture", 6 },
                    { "stop screen capture", 6 },
                    { "stop screen recording", 6 },

                    { "sar", 7 },
                    { "stop audio recording", 7 },
                    { "stop recording audio", 7 },
                    { "stop audio capture", 7 },

                    { "play audio", 8 },
                    { "start playing audio", 8 },
                    { "run audio file", 8 },
                    { "play audio file", 8 },
                    { "pa", 8 },

                    { "change settings", 9 },
                    { "cs", 9 },
                    { "edit settings", 9 },
                    { "change setting", 9 },
                    { "set", 9 },

                    { "take process snapshot", 10 },
                    { "tps", 10 },
                    
                    { "start process measuring", 11 },
                    { "start measuring process", 11 },
                    { "begin process measuring", 11 },
                    { "begin measuring process", 11 },
                    { "record process measurements", 11 },
                    { "start process recording", 11 },
                    { "spm", 11 },
                    { "monitor process", 11 },
                    { "start monitoring process", 11 },
                    { "mp", 11 },
                    { "smp", 11 },

                    { "stop process measuring", 12 },
                    { "stop measuring process", 12 },
                    { "finish process measuring", 12 },
                    { "finish measuring process", 12 },
                    { "finish process measurements", 12 },
                    { "finish process recording", 12 },
                    { "fpm", 12 },
                    { "finish process monitor", 12 },
                    { "finish monitoring process", 12 },
                    { "fmp", 12 },

                    { "open logs", 13 },
                    { "open log folder", 13 },
                    { "ol", 13 },
                    { "olf", 13 },

                    { "view log", 14 },
                    { "open log", 14 },
                    { "vl", 14 },
                    
                    { "change cursor", 15 },
                    { "cc", 15 },
                    { "change mouse", 15 },
                    
                    { "reset cursor", 16 },
                    { "rc", 16 },
                    { "reset mouse", 16 },

                    { "plot", 17 },
                    { "plot data", 17 },
                    { "plot graph", 17 },
                    { "create graph", 17 },
                    { "pd", 17 },
                    
                    { "save plot", 18 },
                    { "sp", 18 },
                    { "save plotted data", 18 },
                    { "save plot data", 18 },
                    { "save graph", 18 },
                    
                    { "cat", 19 },
                    { "random cat picture", 19 },
                    { "rcp", 19 },
                    { "random cat", 19 },
                    { "cat picture", 19 },
                    { "kitty", 19 },
                    { "kitty cat", 19 },

                    { "help", 20 },
                    { "h", 20 },
                    { "cmdinfo", 20},
                    { "cmd", 20 },
                    { "command info", 20 },

                    { "display screen information", 21 },
                    { "dsi", 21 },
                    { "show screen info", 21 },
                    { "display screen info", 21 },
                    { "ssi", 21 },
                    { "show screen information", 21 },

                    { "show console", 22 },
                    { "sll", 22 },
                    { "open console", 22 },
                    { "show live logger", 22 },
                    { "open live logger", 22 },
                    { "show logger", 22 },
                    { "open logger", 22 },

                    { "hide console", 23 },
                    { "hll", 23 },
                    { "close console", 23 },
                    { "close live logger", 23 },
                    { "hide live logger", 23 },
                    { "close logger", 23 },
                    { "hide logger", 23 },

                    { "stop audio", 24 },
                    { "sap", 24 },
                    { "stop audio playback", 24 },
                    { "abort audio playback", 24 },
                    { "abort audio", 24 },

                    { "print element details", 25 },
                    { "ped", 25 },

                    { "force logging flush", 26 },
                    { "flush logs", 26 },
                    { "flf", 26 },
                    { "force log flush", 26 },
                    { "force flush logs", 26 },
                };
                private readonly static Dictionary<int, Dictionary<string, object>> Commands = new()
                {
                    {
                        0, new Dictionary<string, object>
                        {
                            { "desc", "Shuts down the entire program" },
                            { "params", "" },
                            { "function", (Action)Shutdown },
                            { "shortcut", "Shift Q E"}
                        }
                    },
                    {
                        1, new Dictionary<string, object>
                        {
                            { "desc", "Closes the interface, the shortcut will open it." },
                            { "params", "" },
                            { "function", (Action)Catowo.inst.ToggleInterface},
                            { "shortcut", "Shift Q I"}
                        }
                    },
                    {
                        2, new Dictionary<string, object>
                        {
                            { "desc", "Shifts the interface screen to another monitor, takes in a number corresponding to the monitor you want it to shift to (1 being primary)" },
                            { "params", "screennum{int}" },
                            { "function", (Action)ChangeScreen },
                            { "shortcut", "Shifts Q (number)"}
                        }
                    },
                    {
                        3, new Dictionary<string, object>
                        {
                            { "desc", "Takes a screenshot of the screen, without the interface. -2 for a stiched image of all screens, -1 for individual screen pics, (number) for an individual screen, leave empty for the current screen Kitty is running on.\nE.g: screenshot ;-2" },
                            { "params", "[mode{int}]" },
                            { "function", (Action)Screenshot },
                            { "shortcut", "Shifts Q S"}
                        }
                    },
                    {
                        4, new Dictionary<string, object>
                         {
                            { "desc", "Begins capturing screen as a video, mutlimonitor support coming soon. Closes the interface when ran." },
                            { "params", "" },
                            { "function", (Action)StartRecording },
                            { "shortcut", "Shifts Q R"}
                         }
                    },
                    {
                        5, new Dictionary<string, object>
                        {
                            { "desc", "Starts capturing system audio, with optional audio input (0/exclusive, 1/inclusive).\n- Exclusive means only audio input, inclusive means audio input and system audio\nE.g: capture audio ;exclusive\nE.g: capture audio ;1" },
                            { "params", "[mode{int/string}]" },
                            { "function", (Action)StartAudioRecording },
                            { "shortcut", ""}
                        }
                    },
                    {
                        6, new Dictionary<string, object>
                        {
                            { "desc", "Stops a currently running recording session, with an optional opening of the recording location after saving (true)\nE.g: stop recording ;true" },
                            { "params", "" },
                            { "function", (Action)StopRecording },
                            { "shortcut", "Shifts Q D"}
                        }
                    },
                    {
                        7, new Dictionary<string, object>
                        {
                            { "desc", "Stops a currently running audio session, with optional opening of the file location after saving.\nE.g: stop audio ;true" },
                            { "params", "" },
                            { "function", (Action)StopAudioRecording },
                            { "shortcut", ""}
                        }
                    },
                    {
                        8, new Dictionary<string, object>
                        {
                            { "desc", "Plays an audio file, present the filepath as an argument with optional looping.\nE.g: play audio ;C:/Downloads/Sussyaudio.mp4 ;true" },
                            { "params", "filepath{str}, [looping{bool}]" },
                            { "function", (Action)PlayAudio },
                            { "shortcut", ""}
                        }
                    },
                    {
                        9, new Dictionary<string, object>
                        {
                            { "desc", "Changes a control setting, you must specify the \nE.g: change setting ;LogAssemblies ;true\nE.g: change setting ;background ;green" },
                            { "params", "variablename{string}, value{string}" },
                            { "function", (Action)ChangeSettings },
                            { "shortcut", ""}
                        }
                    },
                    {
                        10, new Dictionary<string, object>
                        {
                            { "desc", "Takes a 'snapshot' of a specified process and shows information like it's memory usage, cpu usage, etc.\nE.g: take process snapshot ;devenv\nE.g: take process snapshot ;9926381232" },
                            { "params", "process{string/int}" },
                            { "function", (Action)TakeProcessSnapshot },
                            { "shortcut", "Shifts Q T"}
                        }
                    },
                                        {
                        11, new Dictionary<string, object>
                        {
                            { "desc", "Starts measuring a processes's information until stopped.\nE.g: start measuring process ;devenv" },
                            { "params", "process{string/int}" },
                            { "function", (Action)StartProcessMeasuring },
                            { "shortcut", "Shifts Q X"}
                        }
                    },
                                                            {
                        12, new Dictionary<string, object>
                        {
                            { "desc", "Stops a currently running process measuring session, with an optional saving of the data.\nE.g: stop measuring process ;false" },
                            { "params", "[savedata{bool}]" },
                            { "function", (Action)StopProcessMeasuring },
                            { "shortcut", "Shifts Q C"}
                        }
                    },
                                                                                {
                        13, new Dictionary<string, object>
                        {
                            { "desc", "Opens the logs folder.\nE.g: open logs" },
                            { "params", "" },
                            { "function", (Action)OpenLogs },
                            { "shortcut", ""}
                        }
                    },
                    {
                        14, new Dictionary<string, object>
                        {
                            { "desc", "Opens a specified log file for viewing, specifying index or name.\nE.g: view log ;1\nE.g: view log ;Lcc0648800552499facf099d368686f0c" },
                            { "params", "filename{string/int}" },
                            { "function", (Action)ViewLog },
                            { "shortcut", ""}
                        }
                    },
                    {
                        15, new Dictionary<string, object>
                        {
                            { "desc", "(Attempts to) Changes the cursor to the specified cursor file, specifying file path.\nE.g: change cursor ;the/path/to/your/.cur/file" },
                            { "params", "" },
                            { "function", (Action)ChangeCursor },
                            { "shortcut", ""}
                        }
                    },
                    {
                        16, new Dictionary<string, object>
                        {
                            { "desc", "Resets all system cursors" },
                            { "params", "" },
                            { "function", (Action)ResetCursor },
                            { "shortcut", ""}
                        }
                    },
                    {
                        17, new Dictionary<string, object>
                        {
                            { "desc", "Plots a set of data, specifying file path(s) or data in the format: ;int, int, int, ... int ;int, int, int, ... int (two sets of data).\nE.g: plot ;path/to/a/csv/with/two/lines/of/data\nE.g: plot ;path/to/csv/with/x_axis/data ;path/to/2nd/csv/with/y_axis/data\nE.g: plot ;1, 2, 3, 4, 5, 6 ;66, 33, 231, 53242, 564345" },
                            { "params", "filepath{string} | filepath1{string} filepath2{string} | data1{int[]} data2{int[]}" },
                            { "function", (Action)Plot },
                            { "shortcut", ""}
                        }
                    },
                    {
                        18, new Dictionary<string, object>
                        {
                            { "desc", "Saves a currently open plot (Plot must be open) to a file.\nE.g: save plot" },
                            { "params", "" },
                            { "function", (Action)SavePlot },
                            { "shortcut", ""}
                        }
                    },
                    {
                        19, new Dictionary<string, object>
                        {
                            { "desc", "Shows a random kitty :3" },
                            { "params", "" },
                            { "function", (Action)RandomCatPicture },
                            { "shortcut", "Shifts Q K"}
                        }
                    },
                    {
                        20, new Dictionary<string, object>
                        {
                            { "desc", "Shows a list of commands, specific command info or general info.\nE.g: help\nE.g: help ;commands\nE.g:help ;plot" },
                            { "params", "[cmdname{string}]" },
                            { "function", (Action)Help },
                            { "shortcut", ""}
                        }
                    },
                    {
                        21, new Dictionary<string, object>
                        {
                            { "desc", "Displays either all screen information, or just a specified one.\ndsi ;1" },
                            { "params", "[screennumber{int}]" },
                            { "function", (Action)DisplayScreenInformation },
                            { "shortcut", ""}
                        }
                    },
                    {
                        22, new Dictionary<string, object>
                        {
                            { "desc", "Opens the live logger. \nE.g:sll" },
                            { "params", "" },
                            { "function", (Action)OpenLogger},
                            { "shortcut", "Shifts Q ,"}
                        }
                    },
                    {
                        23, new Dictionary<string, object>
                        {
                            { "desc", "Closes an open live logger\nE.g: cll" },
                            { "params", "" },
                            { "function", (Action)CloseLogger },
                            { "shortcut", "Shifts Q ."}
                        }
                    },
                    {
                        24, new Dictionary<string, object>
                        {
                            { "desc", "Aborts a currently playing audio file." },
                            { "params", "" },
                            { "function", (Action)StopAudio },
                            { "shortcut", "Shifts Q V"}
                        }
                    },
                    {
                        25, new Dictionary<string, object>
                        {
                            { "desc", "Prints the interface element details" },
                            { "params", "" },
                            { "function", (Action)PrintElementDetails },
                            { "shortcut", ""}
                        }
                    },
                    {
                        26, new Dictionary<string, object>
                        {
                            { "desc", "Forces a logging flush" },
                            { "params", "" },
                            { "function", (Action)FML },
                            { "shortcut", "Shifts Q F"}
                        }
                    },
                };

                private static string cmdtext;

                private static void FYI()
                    => @interface.AddLog("This feature is coming soon.");

                internal static bool ParseParameters(out object[]? parsedParams)
                {
                    parsedParams = null;
                    Logging.Log("Parsing Parameters...");
                    bool parameterless = false;
                    if (cmdtext.Split(';').Length == 1)
                    {
                        Logging.Log("No parameters inputted.");
                        parameterless = true;
                    }
                    List<string> parameters = [cmdtext,];
                    if (!parameterless) 
                        parameters = cmdtext.Split(';').Select(p => p.Trim()).ToList();
                    string metadata = Commands[cmdmap[parameters[0]]]["params"] as string;
                    if (!parameterless)
                        parameters.RemoveAt(0);
                    else
                        parameters = [];
                    Logging.LogP($"Command metadata ", metadata);
                    Logging.LogP(parameters);
                    if (metadata == "" || metadata == null)
                    {
                        Logging.Log("Empty Metadata, proceeding.");
                        return true;
                    }

                    if (parameterless && (metadata == null || metadata == "" || metadata.Split('[').Length == metadata.Split('{').Length))
                    {
                        Logging.Log("No parameters inputted and function is parameterless or has all optional parameters");
                        return true;
                    }
                    var options = metadata.Split("|");
                    Logging.Log($"Metadata Options: -- {string.Join(", ", options)} -- ({options.Length})");
                    bool isValid = false;
                    (int, int)[] values = new (int, int)[options.Length];
                    for (int i = 0; i < options.Length; i++)
                    {
                        int optionals = Math.Abs(options[i].Split('[').Length - 1);
                        int mincount = Math.Abs(options[i].Split('{').Length - 1) - optionals;
                        int maxcount = mincount + optionals;
                        values[i] = (mincount, maxcount);
                        Logging.Log($"Metacount: {mincount}, parameters: {parameters.Count}");
                        if (parameters.Count >= mincount && parameters.Count <= maxcount)
                        {
                            isValid = true;
                            break;
                        }
                    }
                    if (!isValid)
                    {
                        Logging.Log("Invalid amount of inputted parameters.");
                        Logging.Log($"Expected {string.Join(", ", values)} parameters but recieved {parameters.Count}");
                        @interface.AddTextLog($"[PARSE FAILED] Expected {string.Join(", ", values)} parameters but recieved {parameters.Count}", SWM.Color.FromRgb(220, 30, 30));
                        return false;
                    }

                    bool fullValid = false;
                    object[] temp = new object[options.Length];
                    int actualcounter = 0;
                    for (int i = 0; i < options.Length; i++)
                    {
                        Logging.LogP("Parameters: ", parameters);
                        if (parameters.Count >= values[i].Item1 && parameters.Count <= values[i].Item2)
                        {
                            var optionsy = options[i].Split('{');
                            for (int j = 0; j < optionsy.Length; j++)
                            {
                                string datatype = optionsy[j].Trim().Replace("[", "").Replace("]", "");
                                Logging.Log($"Datatype: {datatype}, J: {j}");
                                if (datatype.Contains('}'))
                                {
                                    if (datatype.Contains('/'))
                                    {
                                        datatype = "string}";
                                    }
                                    switch (datatype)
                                    {
                                        case "string}":
                                            Logging.Log($"Adding type string {parameters[actualcounter]} as parameter {actualcounter}");
                                            temp[i] = (parameters[actualcounter]);
                                            break;
                                        case "int}":
                                            Logging.Log($"Parsing {parameters[actualcounter]} as int...");
                                            if (int.TryParse(parameters[actualcounter], out int iresult))
                                            {
                                                Logging.Log($"Successfully cast to int");
                                                Logging.Log($"Adding type int {parameters[actualcounter]} as parameter {actualcounter}");
                                                temp[i] = (iresult);
                                            }
                                            else
                                            {
                                                Logging.Log("Unsuccessful int casting, trying next set of options (if any)");
                                                isValid = false;
                                            }
                                            break;
                                        case "bool}":
                                            Logging.Log($"Parsing {parameters[actualcounter]} as bool...");
                                            if (bool.TryParse(parameters[actualcounter], out bool bresult))
                                            {
                                                Logging.Log($"Successfully cast to bool");
                                                Logging.Log($"Adding type bool {parameters[actualcounter]} as parameter {actualcounter}");
                                                temp[i] = (bresult);
                                            }
                                            else
                                            {
                                                Logging.Log("Unsuccessful bool casting, trying next set of options (if any)");
                                                isValid = false;
                                            }
                                            break;
                                        case "int[]}":
                                            Logging.Log($"Parsing {parameters[actualcounter]} as int[]...");
                                            List<int> tempints = new();
                                            string[] tempparam = parameters[actualcounter].Replace(" ", "").Split(',');
                                            for (int ilk = 0; ilk < tempparam.Length; ilk++)
                                            {
                                                Logging.Log($"Attempting to parse item {tempparam[ilk]} (#{ilk}) as int...");
                                                if (int.TryParse(tempparam[ilk], out int aresult))
                                                {
                                                    Logging.Log("Successfully parsed item as int");
                                                    tempints.Add(aresult);
                                                }
                                                else
                                                {
                                                    Logging.Log("Parsing failed");
                                                    isValid = false;
                                                    break;
                                                }
                                            }
                                            temp[i] = tempints.ToArray();
                                            break;

                                        default:
                                            Logging.Log("[PARSING FAILED] Unidentified metadata tag for data type. Please contact my creator to fix this, include the log file!");
                                            @interface.AddTextLog("[PARSING FAILED] Unidentified metadata tag for data type. Please contact my creator to fix this, include the log file!", SWM.Color.FromRgb(255, 0, 0));
                                            break;
                                    }
                                    actualcounter++;
                                }
                                if (!isValid)
                                    break;
                            }
                        }


                        if (isValid)
                        {
                            fullValid = true;
                        }
                        else
                        {
                            isValid = true;
                        }
                    }
                
                    if (!fullValid)
                    {
                        Logging.Log("No 1:1 conversion between inputted parameters and metadata found, failing parse.");
                        @interface.AddTextLog("[PARSE FAILED] No matching link between expected parameters and inputted parameters found.", SWM.Color.FromRgb(230, 20, 20));
                        return false;
                    }

                    parsedParams = temp;
                    return true;
                }

                internal static async void ProcessCommand()
                {
                    cmdtext = @interface.inputTextBox.Text.Trim().ToLower();
                    string call = cmdtext.Split(";")[0].Trim();
                    Logging.Log($"Processing Interface Command, Input: {cmdtext}");
                    if (cmdmap.TryGetValue(call, out int value))
                    {
                        int index = value;
                        Dictionary<string, object> metadata = Commands[index];
                        var parts = cmdtext.Split(';');
                        if (parts.Length > 1)
                        {
                            var parametersToLog = string.Join(";", parts.Skip(1));
                            Logging.Log($"Executing command {call}, index {index} with parameters {parametersToLog}");
                        }
                        else
                        {
                            Logging.Log($"Executing command {call}, index {index} with no parameters");
                        }
                        if (metadata.TryGetValue("function", out var actionObj) && actionObj is Action action)
                            action();
                        else if (metadata.TryGetValue("function", out var funcObj) && actionObj is Func<Task> tfunc)
                            await tfunc();
                        else
                        {
                            Logging.Log(">>>ERROR<<< Action nor TFunct not found for the given command ID.");
                            @interface.AddTextLog($"Action nor TFunct object not found for command {call}, stopping command execution.\nThis... shouldn't happen. hm.", SWM.Color.FromRgb(200, 0, 40));
                        }
                        Logging.Log($"Finished Processing command {call}");
                    }
                    else
                    {
                        Logging.Log("Command Not Found");
                        @interface.AddLog($"No recognisable command '{call}', please use 'help ;commands' for a list of commands!");
                    }
                    @interface.inputTextBox.Text = string.Empty;
                    @interface.AddLog("\n");
                }

                private static async void FML()
                {
                    @interface.AddLog("Flushing Log queue...");
                    await Logging.FinalFlush();
                    @interface.AddLog("Logs flushed!");
                }

                private static void Shutdown()
                {
                    @interface.AddTextLog("Shutting down... give me a few moments...", SWM.Color.FromRgb(230, 20, 20));
                    Catowo.inst.Hide();
                    App.ShuttingDown();
                }

                private static void ChangeScreen()
                { 
                    if (ParseParameters(out object[] args) && args != null)
                    {
                        if (args[0] is int arg && arg >= 0 && arg < System.Windows.Forms.Screen.AllScreens.Length)
                        {
                            Logging.Log($"Changing screen to Screen #{arg}");
                            Catowo.inst.Screen = arg;
                        }
                        else
                        {
                            Logging.Log("Screen index out of bounds of array.");
                            @interface.AddLog($"Failed to find screen with index: {args[0]}");
                        }
                    }
                }

                private static async void Screenshot()
                {
                    await @interface.Hide();
                    if (ParseParameters(out object[] args))
                    {
                        Logging.Log("Taking screenshots...");
                        if (args is null)
                            args = new object[1];
                        if (args[0] is null)
                            args[0] = Array.IndexOf(System.Windows.Forms.Screen.AllScreens, System.Windows.Forms.Screen.PrimaryScreen);
                        if (args[0] is int arg && arg >= 0 && arg < System.Windows.Forms.Screen.AllScreens.Length)
                        {
                            Logging.Log($"Capturing screen {arg}");
                            Bitmap bmp = Helpers.Screenshotting.CaptureScreen(arg, out string? error);
                            if (error != "" && error != null)
                            {
                                @interface.AddTextLog(error, RED);
                                @interface.Show();
                                return;
                            }
                            Logging.Log("Captured!");
                            string path = SSFolder + $"Shot{GUIDRegex().Replace(Guid.NewGuid().ToString(), "")}.png";
                            bmp.Save(path, ImageFormat.Png);
                            bmp.Dispose();
                            @interface.AddLog("Screenshot saved!");
                            Logging.Log($"Shot saved to {path}");
                        }
                        else if (args[0] is int arg2 && arg2 == -1)
                        {
                            Logging.Log("Capturing all screens, individual mode");
                            List<Bitmap> bmps = Helpers.Screenshotting.AllIndivCapture(out var errors);
                            errors?.RemoveAll(x => x == null);
                            if (errors != null && errors?.Count > 0)
                            {
                                for (int i = 0; i < errors.Count; i++)
                                {
                                    string? error = errors[i];
                                    if (error != null)
                                        @interface.AddTextLog($"Error when shooting screen {i}" + error, RED);
                                    Logging.Log(error == null? "no error" : error);
                                }
                                @interface.Show();
                                Logging.Log("Exiting Screenshotting due to errors.");
                                return;
                            }
                            Logging.Log($"{bmps.Count} shots taken!");
                            for (int i = 0; i < bmps.Count; i++)
                            {
                                Bitmap bmp = bmps[i];
                                string path = SSFolder + $"IShot{i}{GUIDRegex().Replace(Guid.NewGuid().ToString(), "")}.png";
                                bmp.Save(path, ImageFormat.Png);
                                Logging.Log($"Saved shot {i} to {path}");
                                bmp.Dispose();
                            }
                            @interface.AddLog("Screenshots saved!");
                        }
                        else if (args[0] is int arg3 && arg3 == -2)
                        {
                            Logging.Log("Capturing all screens, stitch mode");
                            Bitmap bmp = Helpers.Screenshotting.StitchCapture(out var error);
                            if (error != "" && error != null)
                            {
                                Logging.Log(error);
                                @interface.AddTextLog(error, RED);
                                @interface.Show();
                                return;
                            }
                            Logging.Log("Captured!");
                            string path = SSFolder + $"SShot{GUIDRegex().Replace(Guid.NewGuid().ToString(), "")}.png";
                            bmp.Save(path, ImageFormat.Png);
                            bmp.Dispose();
                            @interface.AddLog("Screenshot saved!");
                            Logging.Log($"Shot saved to {path}");
                        }
                        else
                        {
                            string str = $"Expected arg1 value within -2 to {System.Windows.Forms.Screen.AllScreens.Length}";
                            @interface.AddTextLog(str, LIGHTRED);
                            Logging.Log(str);
                            @interface.Show();
                            return;
                        }
                    }
                    @interface.Show();
                }

                internal static void PrintElementDetails()
                {
                    @interface.AddLog("Background Rectangle: ", inst.Backg.Width.ToString(), inst.Backg.Height.ToString());
                    @interface.AddLog("Display box: ", logListBox.Width.ToString(), logListBox.Height.ToString(), GetLeft(logListBox).ToString());
                    @interface.AddLog("Input box: ", @interface.inputTextBox.Width.ToString(), @interface.inputTextBox.Height.ToString(), GetLeft(@interface.inputTextBox).ToString(), GetTop(@interface.inputTextBox).ToString());
                }

                private static void StartRecording()
                {
                    @interface.AddLog("Starting screen recording session");
                    Helpers.ScreenRecording.StartRecording(_screen_, VideoFolder + "V" + GUIDRegex().Replace(Guid.NewGuid().ToString(), "") + ".mp4");
                }

                private static void StartAudioRecording()
                {
                    FYI();
                }

                private static void StopRecording() 
                {
                    FML();
                    @interface.AddLog("Ending screen recording session");
                    Helpers.ScreenRecording.StopRecording();
                    @interface.AddLog("Screen recording session ended.");
                }

                private static void StopAudioRecording()
                {
                    FYI();
                }

                private static void PlayAudio()
                {
                    if (ParseParameters(out object[] args))
                    {
                        if (args == null || args[0] == null)
                        {
                            Logging.Log("Args was null, expected Filepath object.");
                            @interface.AddTextLog("Args was null, expected Filepath object.", RED);
                        }

                        try
                        {
                            string filePath = args[0] as string;
                            if (filePath == null || !ValidateFile(filePath))
                            {
                                @interface.AddTextLog($"Filepath {filePath} resulted in a file that was either null, corrupt, unreadable, or protected.", RED);
                            }
                            Logging.Log($"Attempting to play audio file: {filePath}");

                            if (WavePlayer != null)
                            {
                                Logging.Log("An audio file is already playing. Stopping current audio.");
                                @interface.AddLog("An audio file is already playing. Stopping current audio...");
                                StopAudio();
                            }
                            Logging.Log("Creating Waveout and Audio file reader objects...");
                            WavePlayer = new WaveOut();
                            AFR = new AudioFileReader(filePath);
                            WavePlayer.Init(AFR);

                            WavePlayer.PlaybackStopped += (s, e) =>
                            {
                                if (e.Exception != null)
                                {
                                    Logging.Log($"Playback stopped due to an exception.");
                                    Logging.LogError(e.Exception);
                                }
                                else
                                {
                                    Logging.Log("Playback stopped without any exception.");
                                }
                                StopAudio();
                            };
                            Logging.Log("Objects created successfully.");

                            WavePlayer.Play();

                            Logging.Log("Audio playback started successfully.");
                            @interface.AddLog($"Playing {filePath}");
                        }
                        catch (Exception ex)
                        {
                            Logging.Log($"Error while attempting to play audio:");
                            Logging.LogError(ex);
                        }
                    }
                }

                private static void StopAudio()
                {
                    Logging.Log("Stopping Audio playback...");
                    try
                    {
                        if (WavePlayer != null)
                        {
                            WavePlayer.Stop();
                            WavePlayer.Dispose();
                            WavePlayer = null;

                            AFR.Dispose();
                            AFR = null;

                            Logging.Log("Audio playback stopped.");
                            if (!SilentAudioCleanup)
                                @interface.AddLog("Audio playback stopped.");
                        }
                        else
                        {
                            Logging.Log("No audio is currently playing.");
                            if (!SilentAudioCleanup)
                                @interface.AddLog("Yes, I too enjoy perfect silence... but you can't tell me to stop playing nothing -- existence isn't an audio file, yk?");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Log($"Error stopping audio playback.");
                        Logging.LogError(ex);
                    }
                    SilentAudioCleanup = false;
                }

                private static void ChangeSettings()
                {
                    FYI();
                }

                private static void TakeProcessSnapshot()
                {
                    FYI();
                }

                private static void StartProcessMeasuring()
                {
                    FYI();
                }

                private static void StopProcessMeasuring()
                {
                    FYI();
                }

                private static void OpenLogs()
                {
                    FYI();
                }

                private static void ViewLog()
                {
                    FYI();
                }

                private static void ChangeCursor()
                {
                    FYI();
                }

                private static void ResetCursor()
                {
                    FYI();
                }

                private static void Plot()
                {
                    FYI();
                }

                private static void SavePlot()
                {
                    FYI();
                }

                private static void RandomCatPicture()
                {
                    FYI();
                }

                private static void Help()
                {
                    bool success = ParseParameters(out object[]? args);
                    Logging.LogP($"Executing help command with args: ", args);
                    if (success)
                    {
                        if (args == null)
                        {
                            @interface.AddLog("Welcome to the help page!\nThis is the interface for the Kitty program, and is where you can run all the commands");
                            @interface.AddTextLog("Run 'help ;commands' to see a list of commands\nRun 'help ;(cmdname)\n    E.g: 'help ;screenshot'\n  to see extended help for that command.", SWM.Color.FromRgb(0xC0, 0xC0, 0xC0));
                            @interface.AddLog("This is a program created to help automate, manage, and improve overall effectiveness of your computer, currently only for Windows machines.");
                            @interface.AddLog("Uhhh... don't really know what else to put here apart from some general notes:\n   For the PARAMS field when viewing command specific help, the symbols are defined as such:\n      | means OR, so you can input the stuff on the left OR the stuff on the right of the bar\n      [] means OPTIONAL PARAMETER, in other words you don't need to input it.\n      {} denotes a datatype, the expected type you input. bool is true/false, int is any whole number.");
                        }
                        else if (args.Length > 0)
                        {
                            string str = args[0] != null ? args[0].ToString() : "";
                            if (str == null) 
                            {
                                Logging.Log("Something went wronng when getting the string command input... uh oh......REEEEEEEEEEEEEEEEEEEE");
                                @interface.AddTextLog("[(Potentially?) CRITICAL ERROR] Failed to get string value from inputted parameters, even though ParseCommands() returned true. Send bug report with log, thanks! (or just try again)", SWM.Color.FromRgb(0xff, 0xc0, 0xcb));
                                return;
                            }
                            if (str == "commands")
                            {
                                @interface.AddLog("Heres a list of every command:");
                                foreach (int key in Commands.Keys)
                                {
                                    var firstKey = cmdmap.FirstOrDefault(x => x.Value == key).Key;
                                    @interface.AddLog($"- {firstKey}");
                                }
                            }
                            else if (cmdmap.TryGetValue(str, out int result))
                            {
                                    var Keys = cmdmap.Where(x => x.Value == result).Select(x => x.Key).ToArray();
                                    var metadata = Commands[result];
                                    @interface.AddLog($"Command: {Keys[0]}");
                                    @interface.AddLog($"Description: {metadata["desc"]}");
                                    @interface.AddLog($"Parameter Format: {metadata["params"]}");
                                    @interface.AddLog($"Shortcut: {metadata["shortcut"]}");
                                    @interface.AddLog($"Aliases: {string.Join(", ", Keys)}");
                            }
                            else
                            {
                                Logging.Log($"Failed to find command for help command {str}");
                                @interface.AddLog($"Failed to find command '{str}'.");
                            }
                        }
                    }
                }

                internal static void DisplayScreenInformation()
                {
                    if (ParseParameters(out object[] args))
                    {
                        if (args == null)
                        {
                            Logging.Log("Displaying all connected screens' information...");
                            for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; i++)
                            {
                                Screen screen = System.Windows.Forms.Screen.AllScreens[i];
                                if (screen != null)
                                    @interface.AddLog($"Screen {i + 1}", $"   Device Name: {screen.DeviceName}", $"   Bounds: {screen.Bounds.Width}px Width, {screen.Bounds.Height}px Height, {screen.Bounds.X}x, {screen.Bounds.Y}y, {screen.Bounds.Top}px Top, {screen.Bounds.Left}px left.", $"   Is Primary: {screen.Primary}", $"   BPP: {screen.BitsPerPixel}");
                                else
                                    @interface.AddTextLog($"Failed to get Screen #{i}'s information.", RED);
                            }
                        }
                        else if (args[0] is int arg && arg >= 0 && arg < System.Windows.Forms.Screen.AllScreens.Length)
                        {
                            Screen screen = System.Windows.Forms.Screen.AllScreens[(int)args[0]];
                            @interface.AddLog($"Screen {arg + 1}", $"   Device Name: {screen.DeviceName}", $"   Bounds: {screen.Bounds.Width}px Width, {screen.Bounds.Height}px Height, {screen.Bounds.X}x, {screen.Bounds.Y}y, {screen.Bounds.Top}px Top, {screen.Bounds.Left}px left.", $"   Is Primary: {screen.Primary}", $"   BPP: {screen.BitsPerPixel}");
                        }
                        else
                        {
                            Logging.Log("Specified index was outside the bounds of the screen array");
                            @interface.AddTextLog("Please select a valid screen index.", LIGHTRED);
                        }
                    }
                }

                private static void OpenLogger()
                {
                    if (Logger == null)
                    {
                        Logger = Logging.ShowLogger();
                        @interface.AddLog("Live Logging window opened!");
                    }
                    else
                        @interface.AddTextLog("Live logger already open...", HOTPINK);
                }

                private static void CloseLogger()
                {
                    if (Logger != null)
                    {
                        Logger = null;
                        Logging.HideLogger();
                        @interface.AddLog("Live Logging window closed!");
                    }
                    else
                        @interface.AddTextLog("This would be great to run... if there was a log window to run it on.", HOTPINK);
                }
            }

            private class LogListBox : SWC.ListBox
            {
                public LogListBox()
                {
                    Background = SWM.Brushes.Black;
                    Foreground = SWM.Brushes.White;
                    FontSize = 12;
                    Opacity = 0.7f;
                    FontFamily = new SWM.FontFamily("Consolas");

                    SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
                    SetValue(VirtualizingStackPanel.VirtualizationModeProperty, VirtualizationMode.Recycling);
                    ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(VirtualizingStackPanel)));

                    Loaded += (s, e) =>
                    {
                        _scrollViewer = GetScrollViewer(logListBox);
                    };

                    _scrollViewer = GetScrollViewer(this);
                }
            }
        }

#endregion Interface
    }
}