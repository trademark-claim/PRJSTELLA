#define ImmedShutdown

/***************************************************************************************
 * 
 *  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                        FILE NAME: Catowo.cs
 *  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *  
 *  - File:        Catowo.CS
 *  - Authors:     Nexus
 *  - Created:     Not sure (fix)
 *  - Description: Main file for where all the commmands are defined and their logic is implemented
 *  
 *  - Updates/Changes:
 *      > [Date] - [Change Description] - [Author if different]
 *  
 *  - Notes:
 *      > This file will be continuously worked on throughout the project
 *      > Consider splitting each different command into their own file just to make it neater
 *      > 
 *  
 ***************************************************************************************/



global using static Cat.BaselineInputs;
global using static Cat.Environment;
global using static Cat.Objects;
global using static Cat.PInvoke;
global using static Cat.Statics;
global using static Cat.Structs;
global using SWC = System.Windows.Controls;
using NAudio.Wave;
using System.CodeDom;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Diagnostics;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using SWM = System.Windows.Media;
using SWS = System.Windows.Shapes;

namespace Cat
{
    internal class Catowo : Window
    {
        /// <summary>
        /// Singleton instance of the Catowo application class.
        /// </summary>
        internal static Catowo inst;
        /// <summary>
        /// Indicates whether the application is currently shutting down.
        /// Used to handle shutdown logic gracefully.
        /// </summary>
        internal static bool ShuttingDown = false;
        /// <summary>
        /// Holds a pointer to the keyboard hook used for global key event handling.
        /// </summary>
        internal static IntPtr keyhook = IntPtr.Zero;
        /// <summary>
        /// The main canvas used in the application's user interface.
        /// </summary>
        private readonly SWC.Canvas canvas = new SWC.Canvas();
        /// <summary>
        /// Label used for displaying debug information within the application's UI.
        /// </summary>
        internal readonly SWC.Label DebugLabel = new();

        /// <summary>
        /// Stores the original window style prior to any modifications made by the application.
        /// </summary>
        internal int originalStyle = 0;
        /// <summary>
        /// Stores the modified windows style
        /// </summary>    
        internal int editedstyle = 0;
        /// <summary>
        /// Stores the pointer to the window's handler
        /// </summary>
        internal IntPtr hwnd = IntPtr.Zero;
        /// <summary>
        /// Flag indicating whether the right shift key is currently pressed.
        /// </summary>
        private bool RShifted = false,
                /// <summary>
                /// Flag indicating whether the 'Q' key is currently pressed.
                /// </summary>
                Qd = false,
                /// <summary>
                /// Flag indicating whether the left shift key is currently pressed.
                /// </summary>
                LShifted = false,
                /// <summary>
                /// Flag indicating the current cursor state, where true denotes the default cursor
                /// and false denotes a custom or modified cursor state.
                /// </summary>
                isCursor = true;

        #region Markers

        /// <summary>
        /// Represents a debug marker as a white ellipse centered within its parent container. 
        /// Used for visual debugging to mark specific positions in the UI.
        /// </summary>
        private readonly SWS.Ellipse DEBUGMARKER = new()
        {
            Fill = Statics.WHITE,
            Width = 10,
            Height = 10,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        /// <summary>
        /// Represents a function (fun) marker as a green ellipse centered within its parent container.
        /// Used for visually marking areas related to functions or features that are stable and safe to use.
        /// </summary>
        private readonly SWS.Ellipse FUNTMARKER = new()
        {
            Fill = Statics.GREEN,
            Width = 10,
            Height = 10,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        /// <summary>
        /// Represents a danger marker as a red ellipse centered within its parent container.
        /// Used for visually marking areas that are critical, potentially dangerous, or require special attention.
        /// </summary>
        private readonly SWS.Ellipse DANGERMARKER = new()
        {
            Fill = Statics.RED,
            Width = 10,
            Height = 10,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        /// <summary>
        /// Represents a shortcuts marker as a blue ellipse centered within its parent container.
        /// Used for visually marking keyboard shortcuts or areas providing direct access to functionality.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the current mode of the application. Setting this property triggers
        /// several side effects including logging the new mode, and toggling the visibility
        /// of debug, functionality, danger, and shortcuts markers based on the current mode flags.
        /// </summary>
        /// <value>
        /// The current mode of the application, represented by the <see cref="Modes"/> enumeration.
        /// </value>
        /// <remarks>
        /// Setting this property logs the change, formats the log with both the numeric and
        /// textual representation of the mode, and updates the visibility of various UI markers:
        /// - DEBUGMARKER and DebugLabel visibility is toggled based on the DEBUG flag.
        /// - FUNTMARKER visibility is toggled based on the Functionality flag.
        /// - DANGERMARKER visibility is toggled based on the DANGER flag.
        /// - SHORTCUTSMARKER visibility is toggled based on the Shortcuts flag.
        /// This ensures that the UI elements are shown or hidden according to the application's current mode.
        /// </remarks>
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
        /// <summary>
        /// Stores the index of the primary screen in the array of all connected screens.
        /// The primary screen is determined by iterating through <see cref="System.Windows.Forms.Screen.AllScreens"/>
        /// and identifying the screen marked as primary.
        /// </summary>
        private static int _screen_ = Array.FindIndex(System.Windows.Forms.Screen.AllScreens, screen => screen.Primary);

        /// <summary>
        /// Gets or sets the index of the current screen used by the application within the array of all connected screens.
        /// Changing the screen index updates the application's interface to match the dimensions and position of the selected screen.
        /// </summary>
        /// <value>
        /// The index of the current screen. Must be a valid index within <see cref="System.Windows.Forms.Screen.AllScreens"/>.
        /// </value>
        /// <remarks>
        /// Setting this property to a new value checks if the value is different from the current screen index and
        /// within the valid range of connected screens. If so, it triggers the interface toggle process, updates the
        /// application's dimensions and position based on the new screen's properties, and logs the new screen parameters.
        /// This ensures the application is properly aligned and sized according to the selected screen's dimensions and working area.
        /// </remarks>
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

        [LoggingAspects.Logging]
        /// <summary>
        /// Initializes the keyboard hook by setting a callback for keyboard events and logging the process.
        /// </summary>
        /// <remarks>
        /// This method logs the start of the keyboard hook setting process, assigns the keyboard procedure callback,
        /// and then sets the keyboard hook with the system. It logs each step of the process, including the successful
        /// hooking and the associated hook ID. The hook ID is then stored for future reference and unhooking if necessary.
        /// </remarks>
        private void InitKeyHook()
        {
            Logging.Log("Setting key hook protocal...");
            _keyboardProc = KeyboardProc;
            Logging.Log("hooking...");
            _keyboardHookID = SetKeyboardHook(_keyboardProc);
            Logging.Log($"Hooking protocal {_keyboardProc} hooked with nint {_keyboardHookID}");
            keyhook = _keyboardHookID;
        }

        [LoggingAspects.Logging]
        /// <summary>
        /// Unhooks the previously set keyboard hook and logs the process.
        /// </summary>
        /// <remarks>
        /// Initiates by logging the intent to unhook. If the current hook ID is the default value (indicating no hook is set),
        /// logs this status and exits. Otherwise, attempts to unhook using the UnhookWindowsHookExWrapper method and logs the result.
        /// If successful, resets the global hook ID to its default value.
        /// </remarks>

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

        [LoggingAspects.Logging]
        /// <summary>
        /// Sets up a low-level keyboard hook to monitor keystroke events across the entire system.
        /// </summary>
        /// <param name="proc">The callback procedure that will be invoked with every keyboard event.</param>
        /// <returns>A handle to the keyboard hook.</returns>
        /// <remarks>
        /// This method initializes a global keyboard hook by invoking SetWindowsHookExWrapper, passing it
        /// the type of hook (WH_KEYBOARD_LL), the callback procedure, and the module handle obtained from
        /// the current process's main module. It logs the process of hook initialization and setting.
        /// </remarks>
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

        [LoggingAspects.Logging]
        /// <summary>
        /// Processes keyboard events captured by the global hook.
        /// </summary>
        /// <param name="nCode">A code the hook procedure uses to determine how to process the message.</param>
        /// <param name="wParam">The identifier of the keyboard message. This parameter can be WM_KEYDOWN, WM_KEYUP, etc.</param>
        /// <param name="lParam">A pointer to a KBDLLHOOKSTRUCT structure that contains details about the keystroke message.</param>
        /// <returns>
        /// If nCode is less than 0, the method calls CallNextHookExWrapper using the same parameters. Otherwise, it processes
        /// the keystroke event and may block the message by returning a non-zero value, or pass it to the next hook by returning
        /// the result of CallNextHookExWrapper.
        /// </returns>
        /// <remarks>
        /// This method checks for specific key combinations (e.g., Q, RShift, LShift) and performs actions based on the current
        /// application mode and the keys pressed. Actions can include shutting down the application, toggling modes, showing or hiding
        /// the cursor, and more. It logs each key event with its details.
        /// </remarks>
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

        /// <summary>
        /// Constructs a new instance of the Catowo window, setting up the application's environment, initializing key hooks, and configuring the initial mode.
        /// </summary>
        /// <remarks>
        /// This constructor logs the creation process, closes any existing instance, initializes the window and visible objects, sets up key hooks, and logs the completion of the window creation. It initializes the application mode to 'None'.
        /// </remarks>
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

        /// <summary>
        /// Finalizes an instance of the Catowo class, ensuring that key hooks are properly destroyed and resources are cleaned up.
        /// </summary>
        /// <remarks>
        /// Logs the start of the cleanup process, destroys the keyboard hook, and logs the completion of the destruction process.
        /// </remarks>
        ~Catowo()
        {
            Logging.Log("Cleaning up Catowo...");
            DestroyKeyHook();
            Logging.Log("Catowo Destroyed.");
        }

        [LoggingAspects.Logging]
        /// <summary>
        /// Retrieves the <see cref="Screen"/> object representing the currently selected screen, falling back to the primary screen if the current screen cannot be determined.
        /// </summary>
        /// <returns>The <see cref="Screen"/> object for the current or primary screen.</returns>
        /// <remarks>
        /// Attempts to return the screen at the index specified by the internal screen index. If this operation fails, for example, due to an invalid index, the primary screen is returned instead. This method uses exception handling to manage any errors during this process.
        /// </remarks>
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

        /// <summary>
        /// Initializes the main window of the application, setting its appearance and configuring its initial position and size based on the primary screen.
        /// </summary>
        /// <remarks>
        /// Sets window properties to enable transparency, remove the standard window style, and ensure it stays on top and is not activated by default. The method also calculates the window's initial dimensions based on the primary screen's resolution and adjusts the window's extended style to support these features. It logs the window's width and height upon completion.
        /// </remarks>
        private void InitializeWindow()
        {
            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;
            Background = System.Windows.Media.Brushes.Transparent;
            Topmost = true;
            ShowActivated = false;
            ShowInTaskbar = true; // When making new code, set this to true so you can close the crashed app
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
        /// <summary>
        /// Creates and adds visual elements to the application's main canvas, setting their initial properties and positions.
        /// </summary>
        /// <remarks>
        /// Adds debug, functionality, shortcuts, and danger markers to the canvas, adjusting their positions accordingly. It also configures and adds a debug label with a specific foreground color. This method is part of the window initialization process and sets the application's content to the prepared canvas.
        /// </remarks>
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

        [LoggingAspects.Logging]
        [LoggingAspects.ConsumeException]
        [LoggingAspects.UpsetStomach]

        /// <summary>
        /// Toggles the visibility and functionality of the application's interface.
        /// </summary>
        /// <returns>A boolean indicating the visibility state of the interface after the toggle operation. <c>true</c> if the interface is now visible, otherwise <c>false</c>.</returns>
        /// <remarks>
        /// If the interface is currently visible, this method clears it and resets the window style to its edited state, logging the change. If the interface is not visible, it sets the window style to include layering and tool window properties, adds a new interface instance to the canvas, and logs the update. In both cases, the method adjusts key hooking accordingly.
        /// </remarks>
        private bool ToggleInterface()
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
                return false;
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
                return true;
            }
        }

        internal class Interface : Canvas
        {

            private readonly SWS.Rectangle Backg;

            //internal static readonly List<Logging.ProgressLogging> progresses = new();
            private SWC.TextBox inputTextBox;

            internal static LogListBox logListBox = new();
            internal static Interface? inst = null;
            internal Canvas parent;
            private static ScrollViewer _scrollViewer;
            /// <summary>
            /// Represents the graphical user interface layer of the application, providing methods and properties to manage its visibility and interactions.
            /// </summary>
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

            /// <summary>
            /// Initializes the background rectangle for the interface, setting its dimensions and opacity.
            /// </summary>
            /// <returns>A rectangle that serves as the background for the interface.</returns>
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

            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            [LoggingAspects.UpsetStomach]

            /// <summary>
            /// Initializes the components of the interface, including the input text box and log list box, and sets their properties and event handlers.
            /// </summary>
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

                inputTextBox.PreviewKeyDown += (s, e) => {
                    Logging.Log(((int)e.Key));
                    switch (e.Key)
                    {
                        case Key.Enter:
                            CommandProcessing.ProcessCommand();
                            break;
                        case Key.Up:
                            CommandProcessing.HistoryUp();
                            break;
                        case Key.Down:
                            CommandProcessing.HistoryDown();
                            break;
                    }

                };

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

            /// <summary>
            /// Asynchronously hides the interface, setting its visibility to collapsed and ensuring the UI updates immediately.
            /// </summary>
            internal async Task Hide()
            {
                Logging.Log("Hiding interface...");
                Visibility = Visibility.Collapsed;
                Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                await Task.Delay(500);
                Logging.Log("Interface hidden");
            }

            /// <summary>
            /// Makes the interface visible.
            /// </summary>
            internal void Show()
            {
                Logging.Log("Showing interface...");
                Visibility = Visibility.Visible;
                Logging.Log("Interface Shown");
            }

            [LoggingAspects.ConsumeException]

            /// <summary>
            /// Adds a log message to the interface's log list box.
            /// </summary>
            /// <param name="logMessage">The message to log.</param>
            /// <returns>An integer representing the position of the newly added log message in the log list box.</returns>
            internal static int AddLog(string logMessage)
            {
                Interface? instance = inst;
                if (instance == null) return -2;
                int value = instance.Dispatcher.Invoke(() => logListBox.AddItem(logMessage));
                if (_scrollViewer != null)
                    _scrollViewer.ScrollToEnd();
                else
                    logListBox.ScrollIntoView(logListBox.Items[logListBox.Items.Count - 1]);
                return value;
            }

            [LoggingAspects.ConsumeException]

            /// <summary>
            /// Edits a log message in the interface's log list box at a specified index.
            /// </summary>
            /// <param name="message">The new log message.</param>
            /// <param name="id">The index of the log message to edit.</param>
            /// <param name="fromEnd">Whether the index is counted from the end of the log list.</param>
            internal static int AddLog(params string[] logs)
            {
                Interface? instance = inst;
                if (instance == null) return -2;
                foreach (string log in logs)
                    instance.Dispatcher.Invoke(() => logListBox.AddItem(log));
                if (_scrollViewer != null)
                    _scrollViewer.ScrollToEnd();
                else
                    logListBox.ScrollIntoView(logListBox.Items[logListBox.Items.Count - 1]);
                return logListBox.Items.Count - 1;
            }

            [LoggingAspects.ConsumeException]

            /// <summary>
            /// Edits a log message in the interface's log list box at a specified index.
            /// </summary>
            /// <param name="message">The new log message.</param>
            /// <param name="id">The index of the log message to edit.</param>
            /// <param name="fromEnd">Whether the index is counted from the end of the log list.</param>
            internal static void EditLog(string message, int id, bool fromEnd)
            {
                Interface? instance = inst;
                if (instance == null) return;
                int itemnum = fromEnd ? ((logListBox.Items.Count - 1) - (id - 1)) : id;
                var item = logListBox.Items[itemnum];
                switch (item)
                {
                    case TextBlock tb:
                        instance.Dispatcher.Invoke(() => (logListBox.Items[itemnum] as TextBlock).Text = message);
                        break;

                    default:
                        instance.Dispatcher.Invoke(() => logListBox.Items[itemnum] = message);
                        break;
                }
                instance.InvalidateVisual();
                if (_scrollViewer != null)
                    _scrollViewer.ScrollToEnd();
                else
                    logListBox.ScrollIntoView(logListBox.Items[logListBox.Items.Count - 1]);
            }

            [LoggingAspects.ConsumeException]

            /// <summary>
            /// Adds a textual log message to the interface's log list box with specified text color.
            /// </summary>
            /// <param name="logMessage">The log message to add.</param>
            /// <param name="color">The color of the text.</param>
            /// <returns>An integer representing the position of the newly added log message in the log list box.</returns>
            internal static int AddTextLog(string logMessage, SWM.Color color)
            {
                Interface? instance = inst;
                if (instance == null) return -2;
                TextBlock block = new TextBlock { Text = logMessage, Foreground = new SolidColorBrush(color) };
                int value = instance.Dispatcher.Invoke(() => logListBox.AddItem(block));
                if (_scrollViewer != null)
                    _scrollViewer.ScrollToEnd();
                else
                    logListBox.ScrollIntoView(logListBox.Items[logListBox.Items.Count - 1]);
                return value;
            }

            [LoggingAspects.ConsumeException]
            internal static (int, TextBlock) AddTextLogR(string logMessage, SolidColorBrush brush = null)
            {
                Interface? instance = inst;
                if (instance == null) return (-2, null);
                if (brush == null) brush = new(Colors.White);
                TextBlock block = new TextBlock { Text = logMessage, Foreground = brush };
                int value = instance.Dispatcher.Invoke(() => logListBox.AddItem(block));
                if (_scrollViewer != null)
                    _scrollViewer.ScrollToEnd();
                else
                    logListBox.ScrollIntoView(logListBox.Items[logListBox.Items.Count - 1]);
                return (value, block);
            }

            [LoggingAspects.ConsumeException]
            internal static int AddTextLog(string logMessage, SolidColorBrush brush)
            {
                Interface? instance = inst;
                if (instance == null) return -2;
                int value = instance.Dispatcher.Invoke(() => logListBox.AddItem(new TextBlock { Text = logMessage, Foreground = brush }));
                if (_scrollViewer != null)
                    _scrollViewer.ScrollToEnd();
                else
                    logListBox.ScrollIntoView(logListBox.Items[logListBox.Items.Count - 1]);
                return value;
            }

            /// <summary>
            /// Provides methods for processing user commands input into the interface, including executing specific actions based on command identifiers and managing command history.
            /// </summary>
            private static class CommandProcessing
            {
                internal static Interface @interface;
                private static IWavePlayer WavePlayer;
                private static AudioFileReader AFR;
                private static ParameterParsing.Command? commandstruct;
                private static bool SilentAudioCleanup = false;
                private static Window? Logger = null;
                private static readonly FixedQueue<string> History = new(10);

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

                    { "download expr", 27 },

                    { "run progress bar test", 28 },

                    { "view settings", 29 },
                    { "see settings", 29 },
                    { "print settings", 29 },
                    { "vs", 29 },

                };

                /// <summary>
                /// Defines a dictionary mapping command identifiers to their descriptions, parameters, associated functions, and shortcuts.
                /// </summary>
                /// <remarks>
                /// Each command is identified by an integer key and contains a dictionary with the following keys:
                /// - <c>desc</c>: A string describing what the command does.
                /// - <c>params</c>: A string detailing the parameters the command accepts, with types and optionality.
                /// - <c>function</c>: A delegate to the function that implements the command's functionality.
                /// - <c>shortcut</c>: A string representing the keyboard shortcut associated with the command, if any.
                /// Commands are used throughout the application to implement functionality accessible through the user interface or keyboard shortcuts.
                /// </remarks>
                private static readonly Dictionary<int, Dictionary<string, object>> Commands = new()
                {
                    {
                        0, new Dictionary<string, object>
                        {
                            { "desc", "Shuts down the entire program" },
                            { "params", "" },
                            { "function", (Func<bool>)Shutdown },
                            { "shortcut", "Shift Q E"}
                        }
                    },
                    {
                        1, new Dictionary<string, object>
                        {
                            { "desc", "Closes the interface, the shortcut will open it." },
                            { "params", "" },
                            { "function", (Func<bool>)Catowo.inst.ToggleInterface},
                            { "shortcut", "Shift Q I"}
                        }
                    },
                    {
                        2, new Dictionary<string, object>
                        {
                            { "desc", "Shifts the interface screen to another monitor, takes in a number corresponding to the monitor you want it to shift to (1 being primary)" },
                            { "params", "screennum{int}" },
                            { "function", (Func<bool>)ChangeScreen },
                            { "shortcut", "Shifts Q (number)"}
                        }
                    },
                    {
                        3, new Dictionary<string, object>
                        {
                            { "desc", "Takes a screenshot of the screen, without the interface. -2 for a stiched image of all screens, -1 for individual screen pics, (number) for an individual screen, leave empty for the current screen Kitty is running on.\nE.g: screenshot ;-2" },
                            { "params", "[mode{int}]" },
                            { "function", (Func<Task<bool>>)Screenshot },
                            { "shortcut", "Shifts Q S"}
                        }
                    },
                    {
                        4, new Dictionary<string, object>
                         {
                            { "desc", "Begins capturing screen as a video, mutlimonitor support coming soon. Closes the interface when ran." },
                            { "params", "" },
                            { "function", (Func<bool>)StartRecording },
                            { "shortcut", "Shifts Q R"}
                         }
                    },
                    {
                        5, new Dictionary<string, object>
                        {
                            { "desc", "Starts capturing system audio, with optional audio input (0/exclusive, 1/inclusive).\n- Exclusive means only audio input, inclusive means audio input and system audio\nE.g: capture audio ;exclusive\nE.g: capture audio ;1" },
                            { "params", "[mode{int/string}]" },
                            { "function", (Func<bool>)StartAudioRecording },
                            { "shortcut", ""}
                        }
                    },
                    {
                        6, new Dictionary<string, object>
                        {
                            { "desc", "Stops a currently running recording session, with an optional opening of the recording location after saving (true)\nE.g: stop recording ;true" },
                            { "params", "" },
                            { "function", (Func<bool>)StopRecording },
                            { "shortcut", "Shifts Q D"}
                        }
                    },
                    {
                        7, new Dictionary<string, object>
                        {
                            { "desc", "Stops a currently running audio session, with optional opening of the file location after saving.\nE.g: stop audio ;true" },
                            { "params", "" },
                            { "function", (Func<bool>)StopAudioRecording },
                            { "shortcut", ""}
                        }
                    },
                    {
                        8, new Dictionary<string, object>
                        {
                            { "desc", "Plays an audio file, present the filepath as an argument with optional looping.\nE.g: play audio ;C:/Downloads/Sussyaudio.mp4 ;true" },
                            { "params", "filepath{str}, [looping{bool}]" },
                            { "function", (Func<bool>)PlayAudio },
                            { "shortcut", ""}
                        }
                    },
                    {
                        9, new Dictionary<string, object>
                        {
                            { "desc", "Changes a control setting, you must specify the \nE.g: change setting ;LogAssemblies ;true\nE.g: change setting ;background ;green" },
                            { "params", "variablename{string}, value{string}" },
                            { "function", (Func<bool>)ChangeSettings },
                            { "shortcut", ""}
                        }
                    },
                    {
                        10, new Dictionary<string, object>
                        {
                            { "desc", "Takes a 'snapshot' of a specified process and shows information like it's memory usage, cpu usage, etc.\nE.g: take process snapshot ;devenv\nE.g: take process snapshot ;9926381232" },
                            { "params", "process{string/int}" },
                            { "function", (Func<bool>)TakeProcessSnapshot },
                            { "shortcut", "Shifts Q T"}
                        }
                    },
                                        {
                        11, new Dictionary<string, object>
                        {
                            { "desc", "Starts measuring a processes's information until stopped.\nE.g: start measuring process ;devenv" },
                            { "params", "process{string/int}" },
                            { "function", (Func<bool>)StartProcessMeasuring },
                            { "shortcut", "Shifts Q X"}
                        }
                    },
                                                            {
                        12, new Dictionary<string, object>
                        {
                            { "desc", "Stops a currently running process measuring session, with an optional saving of the data.\nE.g: stop measuring process ;false" },
                            { "params", "[savedata{bool}]" },
                            { "function", (Func<bool>)StopProcessMeasuring },
                            { "shortcut", "Shifts Q C"}
                        }
                    },
                                                                                {
                        13, new Dictionary<string, object>
                        {
                            { "desc", "Opens the logs folder.\nE.g: open logs" },
                            { "params", "" },
                            { "function", (Func<bool>)OpenLogs },
                            { "shortcut", ""}
                        }
                    },
                    {
                        14, new Dictionary<string, object>
                        {
                            { "desc", "Opens a specified log file for viewing, specifying index or name.\nE.g: view log ;1\nE.g: view log ;Lcc0648800552499facf099d368686f0c" },
                            { "params", "filename{string/int}" },
                            { "function", (Func<bool>)ViewLog },
                            { "shortcut", ""}
                        }
                    },
                    {
                        15, new Dictionary<string, object>
                        {
                            { "desc", "(Attempts to) Changes the cursor to the specified cursor file, specifying file path.\nE.g: change cursor ;the/path/to/your/.cur/file" },
                            { "params", "" },
                            { "function", (Func<bool>)ChangeCursor },
                            { "shortcut", ""}
                        }
                    },
                    {
                        16, new Dictionary<string, object>
                        {
                            { "desc", "Resets all system cursors" },
                            { "params", "" },
                            { "function", (Func<bool>)ResetCursor },
                            { "shortcut", ""}
                        }
                    },
                    {
                        17, new Dictionary<string, object>
                        {
                            { "desc", "Plots a set of data, specifying file path(s) or data in the format: ;int, int, int, ... int ;int, int, int, ... int (two sets of data).\nE.g: plot ;path/to/a/csv/with/two/lines/of/data\nE.g: plot ;path/to/csv/with/x_axis/data ;path/to/2nd/csv/with/y_axis/data\nE.g: plot ;1, 2, 3, 4, 5, 6 ;66, 33, 231, 53242, 564345" },
                            { "params", "filepath{string} | filepath1{string} filepath2{string} | data1{int[]} data2{int[]}" },
                            { "function", (Func<bool>)Plot },
                            { "shortcut", ""}
                        }
                    },
                    {
                        18, new Dictionary<string, object>
                        {
                            { "desc", "Saves a currently open plot (Plot must be open) to a file.\nE.g: save plot" },
                            { "params", "" },
                            { "function", (Func<bool>)SavePlot },
                            { "shortcut", ""}
                        }
                    },
                    {
                        19, new Dictionary<string, object>
                        {
                            { "desc", "Shows a random kitty :3" },
                            { "params", "" },
                            { "function", (Func<bool>)RandomCatPicture },
                            { "shortcut", "Shifts Q K"}
                        }
                    },
                    {
                        20, new Dictionary<string, object>
                        {
                            { "desc", "Shows a list of commands, specific command info or general info.\nE.g: help\nE.g: help ;commands\nE.g:help ;plot" },
                            { "params", "[cmdname{string}]" },
                            { "function", (Func<bool>)Help },
                            { "shortcut", ""}
                        }
                    },
                    {
                        21, new Dictionary<string, object>
                        {
                            { "desc", "Displays either all screen information, or just a specified one.\ndsi ;1" },
                            { "params", "[screennumber{int}]" },
                            { "function", (Func<bool>)DisplayScreenInformation },
                            { "shortcut", ""}
                        }
                    },
                    {
                        22, new Dictionary<string, object>
                        {
                            { "desc", "Opens the live logger. \nE.g:sll" },
                            { "params", "" },
                            { "function", (Func<bool>)OpenLogger},
                            { "shortcut", "Shifts Q ,"}
                        }
                    },
                    {
                        23, new Dictionary<string, object>
                        {
                            { "desc", "Closes an open live logger\nE.g: cll" },
                            { "params", "" },
                            { "function", (Func<bool>)CloseLogger },
                            { "shortcut", "Shifts Q ."}
                        }
                    },
                    {
                        24, new Dictionary<string, object>
                        {
                            { "desc", "Aborts a currently playing audio file." },
                            { "params", "" },
                            { "function", (Func<bool>)StopAudio },
                            { "shortcut", "Shifts Q V"}
                        }
                    },
                    {
                        25, new Dictionary<string, object>
                        {
                            { "desc", "Prints the interface element details" },
                            { "params", "" },
                            { "function", (Func<bool>)PrintElementDetails },
                            { "shortcut", ""}
                        }
                    },
                    {
                        26, new Dictionary<string, object>
                        {
                            { "desc", "Forces a logging flush" },
                            { "params", "" },
                            { "function", (Func<Task<bool>>)FML },
                            { "shortcut", "Shifts Q F"}
                        }
                    },
                    {
                        27, new Dictionary<string, object>
                        {
                            { "desc", "Downloads exprs" },
                            { "params", "processname{string}" },
                            { "function", (Func<Task<bool>>)DEP },
                            { "shortcut", ""}
                        }
                    },
                    {
                        28, new Dictionary<string, object>
                        {
                            { "desc", "Generates a progress bar test" },
                            { "params", "" },
                            { "function", (Func<bool>)GPT },
                            { "shortcut", ""}
                        }
                    },
                    {
                        29, new Dictionary<string, object>
                        {
                            { "desc", "Prints all user settings in the format:\n[Section]\n  [Key]: [Value]" },
                            { "params", "" },
                            { "function", (Func<bool>)ShowSettings },
                            { "shortcut", ""}
                        }
                    },
                };

                private static string cmdtext;

                private static void FYI()
                    => AddLog("This feature is coming soon.");

                [LoggingAspects.Logging]

                /// <summary>
                /// Navigates to the previous command in the history queue and displays it in the input text box.
                /// </summary>
                /// <remarks>
                /// If there is a previously executed command available, it retrieves and sets it as the current text of the input box.
                /// If no previous command is available or if retrieving the previous command fails, no action is taken.
                /// </remarks>
                internal static void HistoryUp()
                {
                    string? previousraw = History.GetNext();
                    if (previousraw == null || History.Failed)
                    {
                        History.Failed = false;
                        return;
                    }
                    @interface.inputTextBox.Text = previousraw;
                }

                [LoggingAspects.Logging]

                /// <summary>
                /// Navigates to the next command in the history queue and displays it in the input text box.
                /// </summary>
                /// <remarks>
                /// If there is a next command available, it retrieves and sets it as the current text of the input box.
                /// If no next command is available or if retrieving the next command fails, no action is taken.
                /// This method complements the HistoryUp method, allowing users to navigate through the command history.
                /// </remarks>
                internal static void HistoryDown()
                {
                    string? nextraw = History.GetPrevious();
                    if (nextraw == null || History.Failed)
                    {
                        History.Failed = false;
                        return;
                    }
                    @interface.inputTextBox.Text = nextraw;
                }

                [LoggingAspects.Logging]

                /// <summary>
                /// Processes the command currently entered in the input text box, executing the associated action.
                /// </summary>
                /// <remarks>
                /// Extracts the command from the input text box, logs the command input, and attempts to find and execute the command using the command map.
                /// If the command is successfully found and parsed, it executes the associated action or function.
                /// If the command execution involves an asynchronous operation, it waits for the operation to complete.
                /// Logs an error and updates the interface with feedback if the command cannot be found, fails to parse, or if the associated action or function cannot be executed.
                /// Clears the input text box upon completion.
                /// </remarks>
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
                            Logging.Log($"Executing command {call}, index {index} with entered parameters {parametersToLog}");
                        }
                        else
                        {
                            Logging.Log($"Executing command {call}, index {index} with no entered parameters");
                        }
                        bool parsestate = ParameterParsing.ParseCommand(cmdtext, value, out ParameterParsing.Command? commandstruct2, out string? error_message);
                        if (commandstruct2 != commandstruct && commandstruct2 != null)
                        {
                            commandstruct = commandstruct2;
                            History.Enqueue(commandstruct.Value.Raw);
                        }

                        if (!parsestate)
                        {
                            Logging.Log("Failed to parse command.");
                            Interface.AddTextLog("Execution terminated.", RED);
                            return;
                        }
                        if (!string.IsNullOrEmpty(error_message))
                            AddTextLog(error_message, RED);

                        if (metadata.TryGetValue("function", out var actionObj) && actionObj is Func<bool> func)
                            func();
                        else if (metadata.TryGetValue("function", out var funcObj) && actionObj is Func<Task<bool>> tfunc)
                            await tfunc();
                        else
                        {
                            Logging.Log(">>>ERROR<<< Action nor TFunct not found for the given command ID.");
                            Interface.AddTextLog($"Action nor TFunct object not found for command {call}, stopping command execution.\nThis... shouldn't happen. hm.", SWM.Color.FromRgb(200, 0, 40));
                        }
                        Logging.Log($"Finished Processing command {call}");
                    }
                    else
                    {
                        Logging.Log("Command Not Found");
                        Interface.AddLog($"No recognisable command '{call}', please use 'help ;commands' for a list of commands!");
                    }
                    @interface.inputTextBox.Text = string.Empty;
                    Interface.AddLog("\n");
                }

                [LoggingAspects.Logging]
                [LoggingAspects.ConsumeException]
                [LoggingAspects.InterfaceNotice]

                /// <summary>
                /// Displays the current settings by reading from a configuration file and logging each setting to the interface.
                /// </summary>
                /// <returns>Always returns true, indicating the method has completed execution.</returns>
                /// <remarks>
                /// Iterates through all settings obtained from the configuration file, logging both the setting name and its value.
                /// </remarks>
                internal static bool ShowSettings()
                {
                    var data = Helpers.IniParsing.GetStructure(UserDataFile);
                    foreach (string key in data.Keys)
                    {
                        Interface.AddLog(key);
                        foreach (KeyValuePair<string, string> kvp in data[key])
                        {
                            Interface.AddLog($"   {kvp.Key}: {kvp.Value}");
                        }
                    }
                    return true;
                }

                [LoggingAspects.ConsumeException]

                /// <summary>
                /// Initiates a test to generate a progressing test sequence.
                /// </summary>
                /// <returns>Always returns true, indicating the method has completed execution.</returns>
                /// <remarks>
                /// This method is used to trigger a progress test, useful for debugging or demonstration purposes.
                /// </remarks>
                private static bool GPT()
                {
                    Helpers.ProgressTesting.GenerateProgressingTest();
                    return true;
                }

                [LoggingAspects.AsyncExceptionSwallower]

                /// <summary>
                /// Downloads external packages or executes processes based on the provided command parameters.
                /// </summary>
                /// <returns>A Task&lt;bool&gt; indicating the success or failure of the operation.</returns>
                /// <remarks>
                /// Attempts to identify and execute a download or process execution based on the input parameters. Specific actions, such as downloading FFMPEG, are determined by the command argument.
                /// </remarks>
                private static async Task<bool> DEP()
                {
                    string entry = commandstruct?.Parameters[0][0] as string;
                    if (entry == null)
                    {
                        Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                        AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                        return false;
                    }
                    if (entry == "ffmpeg")
                    {
                        await Helpers.FFMpegManager.DownloadFFMPEG();
                        Logging.Log("DEP Execution " + Helpers.BackendHelping.Glycemia("Complete"));
                    }
                    else
                    {
                        Interface.AddLog("Unrecognised Process name.");
                        return false;
                    }
                    return true;
                }

                [LoggingAspects.AsyncExceptionSwallower]

                /// <summary>
                /// Flushes the logging queue, ensuring all pending log messages are written out.
                /// </summary>
                /// <returns>A Task&lt;bool&gt; indicating the success or failure of the flush operation.</returns>
                /// <remarks>
                /// Asynchronously flushes the log queue, useful for ensuring that all pending log entries are processed and stored as intended, typically before shutdown or when debugging.
                /// </remarks>
                private static async Task<bool> FML()
                {
                    Interface.AddLog("Flushing Log queue...");
                    await Logging.FinalFlush();
                    Interface.AddLog("Logs flushed!");
                    return true;
                }

                [LoggingAspects.ConsumeException]

                /// <summary>
                /// Initiates the application shutdown process, performing cleanup and closing operations.
                /// </summary>
                /// <returns>True if the shutdown process is initiated successfully.</returns>
                /// <remarks>
                /// Logs the shutdown intention, hides the application window, and triggers any necessary shutdown logic encapsulated in the App.ShuttingDown method.
                /// </remarks>
                private static bool Shutdown()
                {
                    Interface.AddTextLog("Shutting down... give me a few moments...", SWM.Color.FromRgb(230, 20, 20));
                    Catowo.inst.Hide();
                    App.ShuttingDown();
                    return true;
                }

                [LoggingAspects.ConsumeException]

                /// <summary>
                /// Changes the current screen to the one specified by the user, updating the application's interface accordingly.
                /// </summary>
                /// <returns>True if the screen change is successful, false otherwise.</returns>
                /// <remarks>
                /// Validates the provided screen index against the available screens and, if valid, moves the application's interface to the specified screen.
                /// </remarks>
                private static bool ChangeScreen()
                {
                    int? entry = (int?)(commandstruct?.Parameters[0][0]);
                    if (entry == null)
                    {
                        Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                        AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                        return false;
                    }
                    if (entry >= 0 && entry < System.Windows.Forms.Screen.AllScreens.Length)
                    {
                        Logging.Log($"Changing screen to Screen #{entry}");
                        Catowo.inst.Screen = entry.Value;
                        return true;
                    }
                    else
                    {
                        Logging.Log("Screen index out of bounds of array.");
                        Interface.AddLog($"Failed to find screen with index: {entry}");
                        return false;
                    }
                }

                [LoggingAspects.AsyncExceptionSwallower]
                [LoggingAspects.Logging]

                /// <summary>
                /// Takes a screenshot based on the specified mode and saves it to a predetermined location.
                /// </summary>
                /// <returns>A Task&lt;bool&gt; indicating the success or failure of the screenshot operation.</returns>
                /// <remarks>
                /// Supports taking individual screenshots of each screen, a stitched screenshot of all screens, or a screenshot of a specific screen, based on the input parameter.
                /// </remarks>
                private static async Task<bool> Screenshot()
                {
                    await @interface.Hide();
                    int? entryN = (int?)(commandstruct?.Parameters[0][0]);
                    if (entryN == null)
                    {
                        Logging.Log("Expected int but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                        AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                        return false;
                    }
                    int entry = entryN.Value;
                    Logging.Log("Taking screenshots...");
                    switch (entry)
                    {
                        case >= 0 when entry < System.Windows.Forms.Screen.AllScreens.Length:
                            {
                                Logging.Log($"Capturing screen {entry}");
                                Bitmap bmp = Helpers.Screenshotting.CaptureScreen(entry, out string? error);
                                if (error != "" && error != null)
                                {
                                    AddTextLog(error, RED);
                                    @interface.Show();
                                    return false;
                                }
                                Logging.Log("Captured!");
                                string path = SSFolder + $"Shot{GUIDRegex().Replace(Guid.NewGuid().ToString(), "")}.png";
                                bmp.Save(path, ImageFormat.Png);
                                bmp.Dispose();
                                AddLog("Screenshot saved!");
                                Logging.Log($"Shot saved to {path}");
                                break;
                            }

                        case -1:
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
                                            AddTextLog($"Error when shooting screen {i}" + error, RED);
                                        Logging.Log(error == null ? "no error" : error);
                                    }
                                    @interface.Show();
                                    Logging.Log("Exiting Screenshotting due to errors.");
                                    return false;
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
                                AddLog("Screenshots saved!");
                                break;
                            }

                        case -2:
                            {
                                Logging.Log("Capturing all screens, stitch mode");
                                Bitmap bmp = Helpers.Screenshotting.StitchCapture(out var error);
                                if (error != "" && error != null)
                                {
                                    Logging.Log(error);
                                    AddTextLog(error, RED);
                                    @interface.Show();
                                    return false;
                                }
                                Logging.Log("Captured!");
                                string path = SSFolder + $"SShot{GUIDRegex().Replace(Guid.NewGuid().ToString(), "")}.png";
                                bmp.Save(path, ImageFormat.Png);
                                bmp.Dispose();
                                AddLog("Screenshot saved!");
                                Logging.Log($"Shot saved to {path}");
                                break;
                            }

                        default:
                            {
                                string str = $"Expected arg1 value within -2 to {System.Windows.Forms.Screen.AllScreens.Length}";
                                AddTextLog(str, LIGHTRED);
                                Logging.Log(str);
                                @interface.Show();
                                return false;
                            }
                    }
                    @interface.Show();
                    return true;
                }

                [LoggingAspects.ConsumeException]

                /// <summary>
                /// Logs the details of key interface elements to the application's log display, including dimensions and positions.
                /// </summary>
                /// <returns>Always returns true, indicating the method completed its execution.</returns>
                /// <remarks>
                /// Useful for debugging layout issues or for verifying that interface elements are being initialized with the correct properties.
                /// </remarks>
                internal static bool PrintElementDetails()
                {
                    Interface.AddLog("Background Rectangle: ", inst.Backg.Width.ToString(), inst.Backg.Height.ToString());
                    Interface.AddLog("Display box: ", logListBox.Width.ToString(), logListBox.Height.ToString(), GetLeft(logListBox).ToString());
                    Interface.AddLog("Input box: ", @interface.inputTextBox.Width.ToString(), @interface.inputTextBox.Height.ToString(), GetLeft(@interface.inputTextBox).ToString(), GetTop(@interface.inputTextBox).ToString());
                    return true;
                }

                [LoggingAspects.ConsumeException]
                [LoggingAspects.Logging]

                /// <summary>
                /// Initiates a screen recording session, saving the video to a predetermined path.
                /// </summary>
                /// <returns>True if the recording session starts successfully.</returns>
                /// <remarks>
                /// Logs the start of the recording session and invokes the screen recording functionality provided by Helpers.ScreenRecording.
                /// </remarks>
                private static bool StartRecording()
                {
                    Interface.AddLog("Starting screen recording session");
                    Helpers.ScreenRecording.StartRecording(_screen_, VideoFolder + "V" + GUIDRegex().Replace(Guid.NewGuid().ToString(), "") + ".mp4");
                    return true;
                }

                [LoggingAspects.ConsumeException]
                [LoggingAspects.Logging]

                /// <summary>
                /// Placeholder for starting audio recording functionality. Currently notifies the user of upcoming features.
                /// </summary>
                /// <returns>Always returns true as a placeholder for future implementation.</returns>
                /// <remarks>
                /// This method is a stub for future development and currently triggers a notification about unimplemented functionality.
                /// </remarks>
                private static bool StartAudioRecording()
                {
                    FYI();
                    return true;
                }

                [LoggingAspects.ConsumeException]

                /// <summary>
                /// Stops the current screen recording session and logs the action.
                /// </summary>
                /// <returns>True if the recording session is stopped successfully.</returns>
                /// <remarks>
                /// Invokes a final logging flush and then stops the recording session using Helpers.ScreenRecording, logging the end of the session.
                /// </remarks>
                private static bool StopRecording()
                {
                    FML();
                    Interface.AddLog("Ending screen recording session");
                    Helpers.ScreenRecording.StopRecording();
                    Interface.AddLog("Screen recording session ended.");
                    return true;
                }

                [LoggingAspects.ConsumeException]
                
                /// <summary>
                /// Placeholder for stopping audio recording functionality. Currently notifies the user of upcoming features.
                /// </summary>
                /// <returns>Always returns true as a placeholder for future implementation.</returns>
                /// <remarks>
                /// This method is a stub for future development and currently triggers a notification about unimplemented functionality.
                /// </remarks>
                private static bool StopAudioRecording()
                {
                    FYI();
                    return true;
                }

                [LoggingAspects.ConsumeException]

                /// <summary>
                /// Attempts to play an audio file specified by the user input.
                /// </summary>
                /// <returns>True if the audio playback starts successfully, false if there is an error or the file path is invalid.</returns>
                /// <remarks>
                /// Validates the file path before attempting playback. Stops any currently playing audio before starting the new audio file.
                /// </remarks>
                private static bool PlayAudio()
                {
                    string entry = commandstruct?.Parameters[0][0] as string;
                    if (entry == null)
                    {
                        Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                        AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                        return false;
                    }
                    try
                    {
                        if (string.IsNullOrWhiteSpace(entry) || !ValidateFile(entry))
                        {
                            Logging.Log($"Invalid or inaccessible file path: {entry}");
                            Interface.AddTextLog($"Invalid or inaccessible file path: {entry}", RED);
                            return false;
                        }
                        Logging.Log($"Attempting to play audio file: {entry}");

                        if (WavePlayer != null)
                        {
                            Logging.Log("An audio file is already playing. Stopping current audio.");
                            Interface.AddLog("An audio file is already playing. Stopping current audio...");
                            StopAudio();
                        }
                        Logging.Log("Creating Waveout and Audio file reader objects...");
                        WavePlayer = new WaveOut();
                        AFR = new AudioFileReader(entry);
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
                        Interface.AddLog($"Playing {entry}");
                    }
                    catch (Exception ex)
                    {
                        Logging.Log($"Error while attempting to play audio:");
                        Logging.LogError(ex);
                        return false;
                    }
                    return true;
                }

                [LoggingAspects.ConsumeException]

                /// <summary>
                /// Stops any currently playing audio and releases associated resources.
                /// </summary>
                /// <returns>True if audio playback was stopped successfully, false if an error occurred during the process.</returns>
                /// <remarks>
                /// Checks if an audio file is currently playing and stops it, ensuring all resources are properly disposed.
                /// </remarks>
                private static bool StopAudio()
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
                                Interface.AddLog("Audio playback stopped.");
                        }
                        else
                        {
                            Logging.Log("No audio is currently playing.");
                            if (!SilentAudioCleanup)
                                Interface.AddLog("Yes, I too enjoy perfect silence... but you can't tell me to stop playing nothing -- existence isn't an audio file, yk?");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Log($"Error stopping audio playback.");
                        Logging.LogError(ex);
                        return false;
                    }
                    SilentAudioCleanup = false;
                    return true;
                }

                [LoggingAspects.ConsumeException]

                /// <summary>
                /// Updates a specific setting based on user input, affecting the application's configuration.
                /// </summary>
                /// <returns>True if the setting is updated successfully, false if the setting name is invalid or the value is not appropriate.</returns>
                /// <remarks>
                /// Parses the setting name and value from the user input, validating against known settings and applying the change if valid.
                /// </remarks>
                private static bool ChangeSettings()
                {
                    try
                    {
                        var entryN = commandstruct?.Parameters[0][0] as string;
                        var entryM = commandstruct?.Parameters[0][1] as string;

                        if (entryN == null || entryM == null)
                        {
                            var message = "Expected string but parsing failed, command struct or entry was null.";
                            Logging.Log(message);
                            AddTextLog($"Execution Failed: {message}", RED);
                            return false;
                        }

                        var normalizedKey = entryN.ToLower().Trim();
                        var data = Helpers.IniParsing.GetStructure(UserDataFile);

                        Logging.Log("Processing NM:", entryN, entryM);

                        foreach (var section in data.Keys)
                        {
                            foreach (KeyValuePair<string, string> kvp in data[section])
                            {
                                var currentKey = kvp.Key.ToLower().Trim();
                                if (currentKey == normalizedKey)
                                {
                                    if (!Helpers.IniParsing.validation.ContainsKey(kvp.Key))
                                    {
                                        Logging.Log($"Validation for {kvp.Key} not found.");
                                        continue;
                                    }

                                    var (type, constraints) = Helpers.IniParsing.validation[kvp.Key];
                                    if ((Type)type == typeof(float) && constraints is Tuple<float, float> range)
                                    {
                                        if (float.TryParse(entryM, out float result) &&
                                            result >= range.Item1 && result <= range.Item2)
                                        {
                                            UserData.UpdateValue(kvp.Key, entryM);
                                            Helpers.IniParsing.UpAddValue(UserDataFile, section, kvp.Key, result.ToString());
                                        }
                                        else
                                        {
                                            Interface.AddLog($"Invalid value for {kvp.Key}. Expected a float in the range {range.Item1}-{range.Item2}.");
                                            return false;
                                        }
                                    }
                                    else if ((Type)type == typeof(bool))
                                    {
                                        if (bool.TryParse(entryM, out bool result))
                                        {
                                            UserData.UpdateValue(kvp.Key, entryM);
                                            Helpers.IniParsing.UpAddValue(UserDataFile, section, kvp.Key, result.ToString());
                                        }
                                        else
                                        {
                                            Interface.AddLog($"Invalid value for {kvp.Key}. Expected a boolean.");
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        UserData.UpdateValue(kvp.Key, entryM);
                                        Helpers.IniParsing.UpAddValue(UserDataFile, section, kvp.Key, entryM);
                                    }

                                    Interface.AddLog($"Updated {kvp.Key} in section {section}.");
                                    return true;
                                }
                            }
                        }

                        Interface.AddLog("Key not found.");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError(ex);
                        AddTextLog("An unexpected error occurred, check logs for details.", RED);
                        return false;
                    }
                }


                [LoggingAspects.ConsumeException]

                /// <summary>
                /// Placeholder method for taking a snapshot of process metrics.
                /// </summary>
                /// <returns>Always returns true as a placeholder for future implementation.</returns>
                /// <remarks>
                /// Intended for future use to capture and log detailed metrics of a specified process.
                /// </remarks>
                private static bool TakeProcessSnapshot()
                {
                    FYI();
                    return true;
                }

                [LoggingAspects.ConsumeException]
                private static bool StartProcessMeasuring()
                {
                    FYI();
                    return true;
                }

                [LoggingAspects.ConsumeException]
                private static bool StopProcessMeasuring()
                {
                    FYI();
                    return true;
                }

                [LoggingAspects.ConsumeException]
                private static bool OpenLogs()
                {
                    FYI();
                    return true;
                }

                [LoggingAspects.ConsumeException]
                private static bool ViewLog()
                {
                    FYI();
                    return true;
                }

                [LoggingAspects.ConsumeException]
                private static bool ChangeCursor()
                {
                    FYI();
                    return true;
                }

                [LoggingAspects.ConsumeException]
                private static bool ResetCursor()
                {
                    FYI();
                    return true;
                }

                [LoggingAspects.ConsumeException]
                [LoggingAspects.Logging]
                private static bool Plot()
                {
                    FYI();
                    return true;
                }

                [LoggingAspects.ConsumeException]
                private static bool SavePlot()
                {
                    FYI();
                    return true;
                }

                [LoggingAspects.ConsumeException]
                [LoggingAspects.Logging]

                /// <summary>
                /// Displays a random cat picture in a new window.
                /// </summary>
                /// <returns>True upon successful display of the cat picture.</returns>
                /// <remarks>
                /// Utilizes the Helpers.CatWindow class to create and show a window containing a randomly selected cat image.
                /// </remarks>
                private static bool RandomCatPicture()
                {
                    AddLog("Generating kitty...");
                    var r = new Helpers.CatWindow();
                    r.Show();
                    return true;
                }

                [LoggingAspects.ConsumeException]
                /// <summary>
                /// Provides help information to the user, either displaying general help or specific command help based on the input.
                /// </summary>
                /// <returns>False if the help request could not be fulfilled, true otherwise.</returns>
                /// <remarks>
                /// If no specific command is requested, displays general help information about the application and how to
                private static bool Help()
                {
                    if (commandstruct == null || commandstruct.Value.Parameters[1].Length < 1)
                    {
                        Interface.AddLog("Welcome to the help page!\nThis is the interface for the Kitty program, and is where you can run all the commands");
                        Interface.AddTextLog("Run 'help ;commands' to see a list of commands\nRun 'help ;(cmdname)\n    E.g: 'help ;screenshot'\n  to see extended help for that command.", SWM.Color.FromRgb(0xC0, 0xC0, 0xC0));
                        Interface.AddLog("This is a program created to help automate, manage, and improve overall effectiveness of your computer, currently only for Windows machines.");
                        Interface.AddLog("Uhhh... don't really know what else to put here apart from some general notes:\n   For the PARAMS field when viewing command specific help, the symbols are defined as such:\n      | means OR, so you can input the stuff on the left OR the stuff on the right of the bar\n      [] means OPTIONAL PARAMETER, in other words you don't need to input it.\n      {} denotes a datatype, the expected type you input. bool is true/false, int is any whole number.");
                        return false;
                    }
                    else
                    {
                        string str = commandstruct?.Parameters[1][0] as string;
                        if (str == null)
                        {
                            Logging.Log("Something went wronng when getting the string command input... uh oh......REEEEEEEEEEEEEEEEEEEE");
                            Interface.AddTextLog("[(Potentially?) CRITICAL ERROR] Failed to get string value from inputted parameters, even though ParseCommands() returned true. Send bug report with log, thanks! (or just try again)", SWM.Color.FromRgb(0xff, 0xc0, 0xcb));
                            return false;
                        }
                        if (str == "commands")
                        {
                            Interface.AddLog("Heres a list of every command:");
                            foreach (int key in Commands.Keys)
                            {
                                var firstKey = cmdmap.FirstOrDefault(x => x.Value == key).Key;
                                Interface.AddLog($"- {firstKey}");
                            }
                        }
                        else if (cmdmap.TryGetValue(str, out int result))
                        {
                            var Keys = cmdmap.Where(x => x.Value == result).Select(x => x.Key).ToArray();
                            var metadata = Commands[result];
                            Interface.AddLog($"Command: {Keys[0]}");
                            Interface.AddLog($"Description: {metadata["desc"]}");
                            Interface.AddLog($"Parameter Format: {metadata["params"]}");
                            Interface.AddLog($"Shortcut: {metadata["shortcut"]}");
                            Interface.AddLog($"Aliases: {string.Join(", ", Keys)}");
                        }
                        else
                        {
                            Logging.Log($"Failed to find command for help command {str}");
                            Interface.AddLog($"Failed to find command '{str}'.");
                            return false;
                        }
                        return true;
                    }
                }

                [LoggingAspects.ConsumeException]

                /// <summary>
                /// Displays information about all connected screens or a specific screen, based on user input.
                /// </summary>
                /// <returns>True if the information could be displayed, false if there was an issue with the input or fetching the screen data.</returns>
                /// <remarks>
                /// Information includes device name, resolution, bounds, primary status, and bits per pixel for each screen.
                /// </remarks>
                internal static bool DisplayScreenInformation()
                {
                    if (commandstruct == null || commandstruct?.Parameters[1].Length < 1)
                    {
                        Logging.Log("Displaying all connected screens' information...");
                        for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; i++)
                        {
                            Screen screen = System.Windows.Forms.Screen.AllScreens[i];
                            if (screen != null)
                                Interface.AddLog($"Screen {i + 1}", $"   Device Name: {screen.DeviceName}", $"   Bounds: {screen.Bounds.Width}px Width, {screen.Bounds.Height}px Height, {screen.Bounds.X}x, {screen.Bounds.Y}y, {screen.Bounds.Top}px Top, {screen.Bounds.Left}px left.", $"   Is Primary: {screen.Primary}", $"   BPP: {screen.BitsPerPixel}");
                            else
                                Interface.AddTextLog($"Failed to get Screen #{i}'s information.", RED);
                        }
                        return true;
                    }
                    else
                    {
                        int? entryN = (int?)(commandstruct?.Parameters[1][0]);
                        if (entryN == null)
                        {
                            Logging.Log("Expected int but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                            AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                            return false;
                        }
                        int entry = entryN.Value;
                        if (entry >= 0 && entry < System.Windows.Forms.Screen.AllScreens.Length)
                        {
                            Screen screen = System.Windows.Forms.Screen.AllScreens[entry];
                            Interface.AddLog($"Screen {entry + 1}", $"   Device Name: {screen.DeviceName}", $"   Bounds: {screen.Bounds.Width}px Width, {screen.Bounds.Height}px Height, {screen.Bounds.X}x, {screen.Bounds.Y}y, {screen.Bounds.Top}px Top, {screen.Bounds.Left}px left.", $"   Is Primary: {screen.Primary}", $"   BPP: {screen.BitsPerPixel}");
                        }
                        else
                        {
                            Logging.Log("Specified index was outside the bounds of the screen array");
                            Interface.AddTextLog("Please select a valid screen index.", LIGHTRED);
                            return false;
                        }
                        return true;
                    }
                }

                [LoggingAspects.ConsumeException]

                /// <summary>
                /// Opens a live logging window to display real-time log messages.
                /// </summary>
                /// <returns>True if the logger was opened successfully, false if it was already open.</returns>
                /// <remarks>
                /// Ensures that only one instance of the logging window is open at any given time.
                /// </remarks>
                private static bool OpenLogger()
                {
                    if (Logger == null)
                    {
                        Logger = Logging.ShowLogger();
                        Interface.AddLog("Live Logging window opened!");
                    }
                    else
                        Interface.AddTextLog("Live logger already open...", HOTPINK);
                    return true;
                }

                [LoggingAspects.ConsumeException]

                /// <summary>
                /// Closes the currently open live logging window.
                /// </summary>
                /// <returns>True if the logger was closed successfully, false if it was not open to begin with.</returns>
                /// <remarks>
                /// Verifies if the logging window is open before attempting to close it.
                /// </remarks>
                private static bool CloseLogger()
                {
                    if (Logger != null)
                    {
                        Logger = null;
                        Logging.HideLogger();
                        Interface.AddLog("Live Logging window closed!");
                    }
                    else
                        Interface.AddTextLog("This would be great to run... if there was a log window to run it on.", HOTPINK);
                    return true;
                }

                /// <summary>
                /// Provides functionality for parsing command strings into structured command objects, enabling command execution based on user input.
                /// </summary>
                private static class ParameterParsing
                {
                    internal readonly record struct Command(string Call, string Raw, object[][]? Parameters = null);

                    [LoggingAspects.ConsumeException]
                    [LoggingAspects.Logging]
                    internal static bool ParseCommand(in string raw, in int num, out Command? command, out string? error_message)
                    {
                        //!! I'm going to leave comments here because this will probably be rather complex :p
                        // First, get the different sequences of expected parameters
                        command = null;
                        error_message = "";
                        string metadata = CommandProcessing.Commands[num]["params"] as string;
                        string call = raw.Split(";")[0].Trim();
                        // If metadata is null then something's gone wrong with extracting it from the
                        // Commands dictionary... which is really bad.
                        if (metadata == null)
                        {
                            Logging.Log("[CRITICAL ERROR] Metadata was unresolved, command cannot be processed. You'll have to make a bug report (attach this log) so this can be fixed in the code behind, apologies for the inconvenience.");
                            error_message = "Metadata resolve error, please see logs for details.";
                            return false;
                        }
                        metadata = metadata.Replace(", ", "").Trim();
                        // The command doesn't accept parameters, so skip parsing them and execute.
                        if (metadata == string.Empty || metadata == "")
                        {
                            Logging.Log("Command accepts no parameters, skipping parse.");
                            command = new(call, raw);
                            return true;
                        }

                        var linputs = raw.Split(";").ToList();
                        linputs.RemoveAt(0);
                        string[] inputs = linputs.ToArray();
                        // Split the metadata into every expected sequence
                        string[] optionals = metadata.Contains('|') ? metadata.Split('|') : [metadata,];
                        Logging.LogP("Optionals", optionals);
                        List<string> couldbes = new(optionals.Length);
                        foreach (string sequence in optionals)
                        {
                            bool? status = ParseSequence(inputs, sequence, out error_message, out object[][]? parsedparams);
                            if (status == false && (error_message != null && error_message != ""))
                                return false;
                            if (status == null)
                                return true;
                            if (status == true)
                            {
                                command = new(call, raw, parsedparams);
                                return true;
                            }
                        }
                        Logging.Log($"[PARSE FAILED] No matching sequence to input found. Please use 'help ;{call}' to see expected command parameters.");
                        error_message = "Unrecognised arguments." + (error_message != "" && error_message != null ? "Additional Error(s): " + error_message : "");
                        return false;
                    }

                    [LoggingAspects.ConsumeException]
                    [LoggingAspects.Logging]
                    private static bool? ParseSequence(string[] inputs, string sequence, out string? error_message, out object[][]? parsedparams)
                    {
                        error_message = null;
                        parsedparams = null;
                        int all = sequence.Count(c => c == '{');
                        int flex = sequence.Count(c => c == '[');
                        int fix = all - flex;
                        Logging.Log($"All: {all}", $"Flex: {flex}", $"Fixed: {fix}");
                        if (fix == 0 && inputs.Length == 0)
                        {
                            Logging.Log("Sequence only accepts optionals and there were no given inputs. End of Parse");
                            return null;
                        }

                        if (fix > inputs.Length)
                        {
                            string mes = "[PARSE ERROR] Inputs were less than sequence expected";
                            Logging.Log(mes);
                            return false;
                        }
                        if (inputs.Length > all)
                        {
                            Logging.Log("More inputs than expected, exiting sequence");
                            return false;
                        }
                        string[]? results;
                        if (Helpers.BackendHelping.ExtractStringGroups(sequence, "{", "}", out results))
                        {
                            if (results == null)
                            {
                                Logging.Log("[CRITICAL ERROR] Metadata grouping resulted null, command cannot be processed. You'll have to make a bug report (attach this log) so this can be fixed in the code behind, apologies for the inconvenience.");
                                error_message = "Metadata resolve error, please see logs for details.";
                                return false;
                            }
                            List<object> flexparams = new(flex), fixparams = new(fix);
                            for (int i = 0; i < results.Length; i++)
                            {
                                //Logging.Log(i, all - i, flex, fix, "");
                                string[] types = results[i].Split('/');
                                Logging.LogP("Types: ", types);
                                bool isValid = false;
                                foreach (string type in types)
                                {
                                    switch (type)
                                    {
                                        case "int":
                                            if (int.TryParse(inputs[i], out int result))
                                            {
                                                Logging.Log($"Successfully cast input #{i}, {inputs[i]} to int.");
                                                isValid = true;
                                                if (all - i + 1 < flex)
                                                    flexparams.Add(result);
                                                else
                                                    fixparams.Add(result);
                                            }
                                            else Logging.Log($"Failed to cast input #{i}, {inputs[i]} to int.");
                                            break;

                                        case "bool":
                                            if (bool.TryParse(inputs[i], out bool bresult))
                                            {
                                                Logging.Log($"Successfully cast input #{i}, {inputs[i]} to bool.");
                                                isValid = true;
                                                if (all - (i + 1) < flex)
                                                    flexparams.Add(bresult);
                                                else
                                                    fixparams.Add(bresult);
                                            }
                                            else Logging.Log($"Failed to cast input #{i}, {inputs[i]} to bool.");
                                            break;

                                        case "string":
                                            isValid = true;
                                            if (all - (i + 1) < flex)
                                                flexparams.Add(inputs[i]);
                                            else
                                                fixparams.Add(inputs[i]);
                                            break;
                                    }
                                    if (isValid) break;
                                }
                                if (!isValid)
                                {
                                    Logging.Log($"Expected {results[i]}, not whatever was inputted.");
                                    return false;
                                }
                            }
                            parsedparams = [fixparams.ToArray(), flexparams.ToArray()];
                            Logging.LogP("Parsed Params object:", parsedparams);
                            return true;
                        }
                        else
                        {
                            Logging.Log("[PARSE ERROR] Failed to extract string groupings! This shouldn't happen... please send a bug report and attach this log, thanks!");
                            return false;
                        }
                    }
                }
            }

            /// <summary>
            /// A custom ListBox control designed to display log messages within the application. It supports virtualization for performance optimization with large numbers of log entries.
            /// </summary>
            internal class LogListBox : SWC.ListBox
            {

                /// <summary>
                /// Initializes a new instance of the LogListBox class, setting up its visual appearance and configuring virtualization for efficient rendering of log messages.
                /// </summary>
                public LogListBox()
                {
                    Background = SWM.Brushes.Black;
                    Foreground = SWM.Brushes.White;
                    FontSize = UserData.FontSize;
                    Opacity = UserData.Opacity;
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

                /// <summary>
                /// Adds a new item to the log list box.
                /// </summary>
                /// <typeparam name="T">The type of the item being added to the log list box.</typeparam>
                /// <param name="Item">The item to add to the log list box. This could be a string message or a more complex data structure depending on the logging needs.</param>
                /// <returns>The index at which the new item was inserted.</returns>
                internal int AddItem<T>(T Item)
                {
                    return Items.Add(Item);
                }

                /// <summary>
                /// Updates the font size of the log messages displayed in the log list box to match the user-defined setting.
                /// </summary>
                internal void UpdateFontSize()
                {
                    FontSize = UserData.FontSize;
                    InvalidateVisual();
                }

                /// <summary>
                /// Updates the opacity of the log list box to match the user-defined setting, allowing for adjustable visibility.
                /// </summary>
                internal void UpdateOpacity()
                {
                    Opacity = UserData.Opacity;
                    InvalidateVisual();
                }
            }
        }

        #endregion Interface
    }
}