#define ImmedShutdown
//#define CrashWary

/***************************************************************************************
 *
 *  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                        FILE NAME: Catowo.cs
 *  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *
 ***************************************************************************************/

// Static globals so I dont have to type out System.Windows.Shapes.Primitives.Rect or System.Windows.Media.Brushes every time
global using static Cat.BaselineInputs;
global using static Cat.Environment;
global using static Cat.Objects;
global using static Cat.PInvoke;
global using static Cat.Catowo.Hooking;
global using static Cat.Statics;
global using static Cat.Structs;
global using Interface = Cat.Catowo.Interface;
global using Command = Cat.Objects.Command;
global using Application = System.Windows.Application;
global using SWC = System.Windows.Controls;
global using Canvas = System.Windows.Controls.Canvas;
global using Label = System.Windows.Controls.Label;
global using TextBox = System.Windows.Controls.TextBox;
global using Key = System.Windows.Input.Key;
global using Brush = System.Windows.Media.Brush;
global using Brushes = System.Windows.Media.Brushes;
global using Color = System.Windows.Media.Color;
global using MessageBox = System.Windows.MessageBox;
global using Point = System.Windows.Point;
global using Rectangle = System.Windows.Shapes.Rectangle;
global using Size = System.Windows.Size;
global using Image = System.Drawing.Image;
global using ListBox = System.Windows.Controls.ListBox;
global using Control = System.Windows.Controls.Control;
using NAudio.Wave;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

using System.Windows.Controls;

using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

using SWM = System.Windows.Media;

using SWS = System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using System.Windows.Automation.Text;
using System.Windows.Input;

namespace Cat
{
    /// <summary>
    /// Main window for the whole program. 
    /// </summary>
    /// <remarks>
    /// Don't ask why its named this, before STELLA the placeholder name was "Kitty"
    /// </remarks>
    internal class Catowo : Window
    {
        /// <summary>
        /// Singleton instance of the Catowo application class.
        /// </summary>
        internal static Catowo inst;

        /// <summary>
        /// Indicates whether STELLA is currently shutting down.
        /// Used to handle shutdown logic gracefully.
        /// </summary>
        internal static bool ShuttingDown = false;

        /// <summary>
        /// Holds a pointer to the keyboard hook used for global key event handling.
        /// </summary>
        internal static IntPtr keyhook = IntPtr.Zero;

        /// <summary>
        /// The main canvas used
        /// </summary>
        internal readonly SWC.Canvas canvas = new SWC.Canvas();

        /// <summary>
        /// Label used for displaying debug information
        /// </summary>
        internal readonly SWC.Label DebugLabel = new();

        /// <summary>
        /// Private reference to the pointer of the key hook used
        /// </summary>
        private static IntPtr _keyboardHookID = IntPtr.Zero;

        /// <summary>
        /// Stores the original window style prior to any modifications
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
        private bool RShifted = false;
                /// <summary>
                /// Flag indicating whether the 'Q' key is currently pressed.
                /// </summary>
        private bool Qd = false;
                /// <summary>
                /// Flag indicating whether the left shift key is currently pressed.
                /// </summary>
        private bool LShifted = false;
                /// <summary>
                /// Flag indicating the current cursor state, where true denotes the default cursor
                /// and false denotes a custom or modified cursor state.
                /// </summary>
        private bool isCursor = true;
                /// <summary>
                /// Flag indicating whether the C key is down or nah
                /// </summary>
        private bool Cd = false;
        /// <summary>
        /// Flag indicating whether the M key is down or nah
        /// </summary>
        private bool Md = false;
        /// <summary>
        /// Flag indicating whether the T key is down or nah
        /// </summary>
        private bool Td = false;

        internal void ResetStates()
            => RShifted = LShifted = Qd = Cd = Md = Td = false;

        #region Markers

        /// <summary>
        ///  debug marker
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
        /// a function (fun) mark related to functions or features.
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
        ///  a danger marker as a red ellipse marking critical, potentially dangerous, or special stuff
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
        ///  a shortcuts marker for shortcut activation
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

        /// <summary>
        /// BEING PHASED OUT mode of shortcuts
        /// </summary>
        private Modes mode = Modes.None;

        /// <summary>
        /// Gets or sets the current mode of STELLA. Setting this property triggers
        /// several side effects including logging the new mode, and toggling the visibility
        /// of debug, functionality, danger, and shortcuts markers based on the current mode flags.
        /// </summary>
        /// <value>
        /// The current mode of STELLA, represented by the <see cref="Modes"/> enumeration.
        /// </value>
        /// <remarks>
        /// Setting this property logs the change, formats the log with both the numeric and
        /// textual representation of the mode, and updates the visibility of various UI markers:
        /// - DEBUGMARKER and DebugLabel visibility is toggled based on the DEBUG flag.
        /// - FUNTMARKER visibility is toggled based on the Functionality flag.
        /// - DANGERMARKER visibility is toggled based on the DANGER flag.
        /// - SHORTCUTSMARKER visibility is toggled based on the Shortcuts flag.
        /// This ensures that the UI elements are shown or hidden according to STELLA's current mode.
        /// </remarks>
        private Modes Mode
        {
            get => mode; set
            {
                mode = value;
                Logging.Log([$"Mode set to {((ushort)mode)} ({mode})"]);
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
        private static int _screen_ = 1; //Array.FindIndex(System.Windows.Forms.Screen.AllScreens, screen => screen.Primary);

        internal static int _Screen { get => _screen_; }

        /// <summary>
        /// Gets or sets the index of the current screen used by STELLA within the array of all connected screens.
        /// Changing the screen index updates STELLA's interface to match the dimensions and position of the selected screen.
        /// </summary>
        /// <value>
        /// The index of the current screen. Must be a valid index within <see cref="System.Windows.Forms.Screen.AllScreens"/>.
        /// </value>
        /// <remarks>
        /// Setting this property to a new value checks if the value is different from the current screen index and
        /// within the valid range of connected screens. If so, it triggers STELLA's interface toggle process, updates the
        /// application's dimensions and position based on the new screen's properties, and logs the new screen parameters.
        /// This ensures STELLA is properly aligned and sized according to the selected screen's dimensions and working area.
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
                        ChangeScreen(value);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the changing of STELLA's interface to another screen
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private async Task ChangeScreen(int value)
        {
            ToggleInterface(true);
            await UIToggleTCS.Task;
            Screen screen = System.Windows.Forms.Screen.AllScreens[value];
            _screen_ = value;
            await Task.Delay(100);
            var (rect, _) = Helpers.ScreenSizing.GetAdjustedScreenSize(screen);
            Top = rect.Top;
            Left = rect.Left;
            Width = rect.Width;
            Height = rect.Height;
            Logging.Log(["New Screen Params: ", Top, Left, Width, Height]);
            ToggleInterface(true);
            await UIToggleTCS.Task;
            return;
        }

        #region Low Levels

        /// <summary>
        /// Handles the key hooking (and maybe mouse sometime)
        /// </summary>
        internal static class Hooking
        {
            /// <summary>
            /// Mirror Property
            /// </summary>
            private static bool Td { get => inst.Td; set => inst.Td = value; }
            /// <summary>
            /// Mirror Property
            /// </summary>
            private static bool Md { get => inst.Md; set => inst.Md = value; }
            /// <summary>
            /// Mirror Property
            /// </summary>
            private static bool Qd { get => inst.Qd; set => Catowo.inst.Qd = value; }
            /// <summary>
            /// Mirror Property
            /// </summary>
            private static bool RShifted { get => inst.RShifted; set => inst.RShifted = value; }
            /// <summary>
            /// Mirror Property
            /// </summary>
            private static bool LShifted { get => inst.LShifted; set => inst.LShifted = value; }
            /// <summary>
            /// Mirror Property
            /// </summary>
            private static Label DebugLabel { get => inst.DebugLabel;  }
            /// <summary>
            /// Mirror Property
            /// </summary>
            private static Modes mode { get => inst.mode; set => inst.mode = value; }
            /// <summary>
            /// Mirror Property
            /// </summary>
            private static bool Cd { get => inst.Cd; set => inst.Cd = value; }
            /// <summary>
            /// Mirror Property
            /// </summary>
            private static bool isCursor { get => inst.isCursor; set => inst.isCursor = value; }
            /// <summary>
            /// Mirror Property
            /// </summary>
            private static Modes Mode { get => inst.Mode; set => inst.Mode = value; }
            /// <summary>
            /// Mirror Property
            /// </summary>
            private static List<int> SeekKey = [];

            /// <summary>
            /// Changes the state of 'key seeking', being the forcing of the user to input one or more specified keys and blocking all else
            /// </summary>
            internal static void ChangeSeeking(int vkCode, bool? state)
            {
                if (state == null) SeekKey.Clear();
                else if (state == true) SeekKey.Add(vkCode);
                else SeekKey.Remove(vkCode);
            }

            /// <summary>
            /// Initializes the keyboard hook by setting a callback for keyboard events and logging the process.
            /// </summary>
            /// <remarks>
            /// This method logs the start of the keyboard hook setting process, assigns the keyboard procedure callback,
            /// and then sets the keyboard hook with the system. It logs each step of the process, including the successful
            /// hooking and the associated hook ID. The hook ID is then stored for future reference and unhooking if necessary.
            /// </remarks>
            [CAspects.Logging]
            [CAspects.ConsumeException]
            internal static void InitKeyHook()
            {
                Logging.Log(["Setting key hook protocal..."]);
                _keyboardProc = KeyboardProc;
                Logging.Log(["hooking..."]);
                _keyboardHookID = SetKeyboardHook(_keyboardProc);
                Logging.Log([$"Hooking protocal {_keyboardProc} hooked with nint {_keyboardHookID}"]);
                keyhook = _keyboardHookID;
            }

            /// <summary>
            /// Unhooks the previously set keyboard hook and logs the process.
            /// </summary>
            /// <remarks>
            /// Initiates by logging the intent to unhook. If the current hook ID is the default value (indicating no hook is set),
            /// logs this status and exits. Otherwise, attempts to unhook using the UnhookWindowsHookExWrapper method and logs the result.
            /// If successful, resets the global hook ID to its default value.
            /// </remarks>

            [CAspects.Logging]
            [CAspects.ConsumeException]
            internal static void DestroyKeyHook()
            {
                Logging.Log(["Unhooking key hook..."]);
                if (_keyboardHookID == IntPtr.Zero)
                {
                    Logging.Log(["Key hook is default, exiting."]);
                    return;
                }
                bool b = UnhookWindowsHookExWrapper(_keyboardHookID);
                Logging.Log([$"Unhooking successful: {b}"]);
                if (b)
                    keyhook = IntPtr.Zero;
            }

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
            [CAspects.Logging]
            internal static IntPtr SetKeyboardHook(LowLevelProc proc)
            {
                Logging.Log(["Initing Keyboard hook..."]);
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    Logging.Log(["Hook initiated, setting..."]);
                    return SetWindowsHookExWrapper(WH_KEYBOARD_LL, proc, GetModuleHandleWrapper(curModule.ModuleName), 0);
                }
            }

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
            /// application mode and the keys pressed. Actions can include shutting down STELLA, toggling modes, showing or hiding
            /// the cursor, and more. It logs each key event with its details.
            /// </remarks>
            [CAspects.Logging]
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            internal static IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                bool isKeyDown = nCode >= 0 && wParam == WM_KEYDOWN;
                bool isKeyUp = nCode >= 0 && wParam == WM_KEYUP;
                string log = $"Key{(isKeyDown ? "KeyDown" : "KeyUp")}: {(Keys)vkCode} ({vkCode})" + (RShifted ? " (rshifted) " : "") + (LShifted ? " (Lshifted) " : "") + (Qd ? " (q) " : "");
                Logging.Log([log]);
                if (isKeyDown)
                {
                    if (SeekKey.Count > 0)
                    {
                        if (!SeekKey.Contains(vkCode))
                        {
                            if (vkCode != VK_ESC)
                                SeekKey = [];
                            return new IntPtr(1);
                        }
                        SeekKey.Remove(vkCode);
                    }

                    if (StellaHerself.TCS != null && !StellaHerself.TCS.Task.IsCompleted)
                    {
                        if (vkCode == VK_LEFT)
                        {
                            var keyEventArgs = new System.Windows.Input.KeyEventArgs(
                                Keyboard.PrimaryDevice,
                                PresentationSource.FromVisual(Catowo.inst.canvas),
                                0,
                                Key.Left
                            )
                            {
                                RoutedEvent = Keyboard.PreviewKeyDownEvent
                            };
                            Catowo.inst.canvas.RaiseEvent(keyEventArgs);
                        }
                        else if (vkCode == VK_UP)
                        {
                            var keyEventArgs = new System.Windows.Input.KeyEventArgs(
                                Keyboard.PrimaryDevice,
                                PresentationSource.FromVisual(Catowo.inst.canvas),
                                0,
                                Key.Up
                            )
                            {
                                RoutedEvent = Keyboard.PreviewKeyDownEvent
                            };
                            Catowo.inst.canvas.RaiseEvent(keyEventArgs);
                        }
                        else if (vkCode == VK_RIGHT)
                        {
                            var keyEventArgs = new System.Windows.Input.KeyEventArgs(
                                Keyboard.PrimaryDevice,
                                PresentationSource.FromVisual(Catowo.inst.canvas),
                                0,
                                Key.Right
                            )
                            {
                                RoutedEvent = Keyboard.PreviewKeyDownEvent
                            };
                            Catowo.inst.canvas.RaiseEvent(keyEventArgs);
                        }
                    }

                    DebugLabel.Content += $"{Qd}, {RShifted}, {LShifted}, {vkCode}, {wParam}, {(Keys)vkCode}";
                    switch (vkCode)
                    {
                        case VK_Q:
                            if (!Qd)
                            {
                                Qd = true;
                                return CallNextHookExWrapper(_keyboardHookID, nCode, wParam, lParam);
                            }
                            break;

                        case VK_RSHIFT:
                            if (!RShifted)
                            {
                                RShifted = true;
                                return CallNextHookExWrapper(_keyboardHookID, nCode, wParam, lParam);
                            }
                            break;

                        case VK_LSHIFT:
                            if (!LShifted)
                            {
                                LShifted = true;
                                return CallNextHookExWrapper(_keyboardHookID, nCode, wParam, lParam);
                            }
                            break;

                        case VK_C:
                            if (!Cd)
                            {
                                Cd = true;
                                return CallNextHookExWrapper(_keyboardHookID, nCode, wParam, lParam);
                            }
                            break;

                        case VK_M:
                            if (!Md)
                            {
                                Md = true;
                                return CallNextHookExWrapper(_keyboardHookID, nCode, wParam, lParam);
                            }
                            break;

                        case VK_T:
                            if (!Td)
                            {
                                Td = true;
                                return CallNextHookExWrapper(_keyboardHookID, nCode, wParam, lParam);
                            }
                            break;
                    }
                    if (Md && Td)
                    {
                        switch (vkCode)
                        {
                            case >= VK_1 and <= VK_9:
                                string item = vkCodeToCharMap[vkCode].Item1.ToString();
                                break;
                        }
                    }
                    if (Qd)
                    {
                        if (Cd)
                        {
                            switch (vkCode)
                            {
                                case VK_0:
                                    BaselineInputs.Cursor.Reset();
                                    break;

                                case >= VK_1 and <= VK_9:
                                    string item = vkCodeToCharMap[vkCode].Item1.ToString();
                                    Logging.Log([item]);
                                    BaselineInputs.Cursor.LoadPresetByIndex(int.Parse(item));
                                    break;

                                case VK_E:
                                    Objects.CursorEffects.Toggle();
                                    break;
                                case VK_B:
                                    Interface.CommandProcessing.ProcessCommand("screenshot ;-1");
                                    break;
                                case VK_N:
                                    Interface.CommandProcessing.ProcessCommand("screenshot ;-2");
                                    break;
                                case VK_K:
                                    IntPtr fwh = GetForegroundWindowWrapper();
                                    PInvoke.GetWindowThreadProcessIdWrapper(fwh, out uint pid);
                                    Process activeprocess = Process.GetProcessById((int)pid);
                                    activeprocess.Kill(true);
                                    break;
                            }
                        }
                        else if (RShifted && LShifted)
                        {
                            switch (vkCode)
                            {
                                case VK_V:
                                    Interface.CommandProcessing.ProcessCommand("toggle stt");
                                    break;
                                case VK_L:
                                    Interface.CommandProcessing.ProcessCommand("ole");
                                    break;
                                case VK_O:
                                    Process.Start(new ProcessStartInfo
                                    {
                                        FileName = "explorer.exe",
                                        Arguments = "C:\\ProgramData\\Kitty\\Cat\\NYANPASU",
                                        UseShellExecute = true
                                    });
                                    break;
                                case VK_B:
                                    Interface.CommandProcessing.ProcessCommand("show console");
                                    break;
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
                                    inst.ToggleInterface();
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
                                        //ShutDownScreen.ToggleScreen(canvas);
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

                        case VK_C:
                            Cd = false;
                            break;

                        case VK_M:
                            Md = false;
                            break;

                        case VK_T:
                            Td = false;
                            break;
                    }
                }
                return CallNextHookExWrapper(_keyboardHookID, nCode, wParam, lParam);
            }
        }

        #endregion Low Levels

        #region Catowo Creation and Init

        /// <summary>
        /// Constructs a new instance of the Catowo window, setting up STELLA's environment, initializing key hooks, and configuring the initial mode.
        /// </summary>
        /// <remarks>
        /// This constructor logs the creation process, closes any existing instance, initializes the window and visible objects, sets up key hooks, and logs the completion of the window creation. It initializes STELLA mode to 'None'.
        /// </remarks>
        public Catowo()
        {
            Logging.Log(["Creating Catowo Window..."]);
            inst?.Close();
            inst = this;
            Logging.Log(["Initialising objects..."]);
            InitializeWindow();
            CreateVisibleObjects();
            Logging.Log(["Objects Initialised"]);
            InitKeyHook();
            Logging.Log(["Catowo Window created!"]);
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
            Logging.Log(["Cleaning up Catowo..."]);
            DestroyKeyHook();
            Logging.Log(["Catowo Destroyed."]);
        }

        /// <summary>
        /// Retrieves the <see cref="Screen"/> object representing the currently selected screen, falling back to the primary screen if the current screen cannot be determined.
        /// </summary>
        /// <returns>The <see cref="Screen"/> object for the current or primary screen.</returns>
        /// <remarks>
        /// Attempts to return the screen at the index specified by the internal screen index. If this operation fails, for example, due to an invalid index, the primary screen is returned instead.
        /// </remarks>
        [CAspects.Logging]
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
        /// Initializes the main window of STELLA, setting its appearance and configuring its initial position and size based on the primary screen.
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
            ShowInTaskbar = false; /* [OUTDATED BUT MIGHT BE USEFUL]: */ //x When making new code, set this to true t can close the crashed app
            Left = 0;
            Top = 0;
            _screen_ = Array.FindIndex(System.Windows.Forms.Screen.AllScreens, screen => screen.Primary);
            var scre = GetScreen();
            Width = scre.Bounds.Width;
            Height = scre.Bounds.Height;
            Logging.Log([$"Width: {Width}", $"Height: {Height}"]);
            //x System.Windows.MessageBox.Show($"{_screen_}, {Width}, {Height}");

            // Runs config that has to be run AFTER the window has been loaded
            Loaded += (sender, e) =>
            {
                hwnd = new WindowInteropHelper(this).Handle;
                originalStyle = GetWindowLongWrapper(hwnd, GWL_EXSTYLE);
                SetWindowLongWrapper(hwnd, GWL_EXSTYLE, originalStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
                editedstyle = GetWindowLongWrapper(hwnd, GWL_EXSTYLE);
                Logging.Log([$"Set Win Style of Handle {hwnd} from {originalStyle:X} ({originalStyle:B}) [{originalStyle}] to {editedstyle:X} ({editedstyle:B}) [{editedstyle}]"]);
                if (!Program.hadUserData)
                    StellaHerself.RunStella(StellaHerself.Mode.Introduction, canvas);
            };
        }

        /// <summary>
        /// Part of the debugging process: Creates and adds visual elements to STELLA's main canvas, setting their initial properties and positions.
        /// </summary>
        /// <remarks>
        /// Adds debug, functionality, shortcuts, and danger markers to the canvas, adjusting their positions accordingly. It also configures and adds a debug label with a specific foreground color. This method is part of the window initialization process and sets STELLA's content to the prepared canvas.
        /// </remarks>
        private void CreateVisibleObjects()
        {
            canvas.Children.Add(DEBUGMARKER);
            canvas.Children.Add(FUNTMARKER);
            Statics.SetLeft<double>(FUNTMARKER, 4);
            canvas.Children.Add(SHORTCUTSMARKER);
            Statics.SetLeft<double>(SHORTCUTSMARKER, 8);
            canvas.Children.Add(DANGERMARKER);
            Statics.SetLeft<double>(DANGERMARKER, 12);
            canvas.Children.Add(DebugLabel);
            Mode = Modes.None;
            DebugLabel.Foreground = new SolidColorBrush(Colors.LimeGreen);
            Content = canvas;
        }

        /// <summary>
        /// The current mode of the app
        /// </summary>
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

        /// <summary>
        /// TCS for the alertion of when the animation for the toggling of the UI is done
        /// </summary>
        internal TaskCompletionSource<bool> UIToggleTCS { get; private set; }

        /// <summary>
        /// Toggles the visibility and functionality of STELLA's interface.
        /// </summary>
        /// <returns>A boolean indicating the visibility state of STELLA's interface after the toggle operation. <c>true</c> if STELLA's interface is now visible, otherwise <c>false</c>.</returns>
        /// <remarks>
        /// If STELLA's interface is currently visible, this method clears it and resets the window style to its edited state, logging the change. If STELLA's interface is not visible, it sets the window style to include layering and tool window properties, adds a new interface instance to the canvas, and logs the update. In both cases, the method adjusts key hooking accordingly.
        /// </remarks>
        [CAspects.Logging]
        [CAspects.ConsumeException]
        [CAspects.UpsetStomach]
        internal async Task<bool> ToggleInterface(bool animation = true, bool makefunny = true)
        {
            UIToggleTCS = new();
            ResetStates();
            if (Interface.inst != null)
            {
                if (animation)
                {
                    Interface.inst.ShrinkAndDisappear(() =>
                    {
                        try
                        {
                            Interface.inst?.Children?.Clear();
                            Interface.inst?.parent?.Children.Remove(Interface.inst);
                            Interface.inst = null;
                        }
                        catch (Exception ex)
                        {
                            Logging.LogError(ex);
                        }
                    });
                    await Interface.inst.AnimationTCS.Task;
                }
                else
                {
                    Interface.inst.Children?.Clear();
                    Interface.inst.parent?.Children.Remove(Interface.inst);
                    Interface.inst = null;
                }
                if (makefunny)
                    MakeFunnyWindow();
                UIToggleTCS.SetResult(true);
                return true;
            }
            else
            {
                if (makefunny)
                    MakeNormalWindow();
                canvas.Children.Add(new Interface(canvas));
                await Interface.inst.AnimationTCS.Task;
                Interface.inst.inputTextBox?.Focus();
                UIToggleTCS.SetResult(true);
                return true;
            }
        }

        /// <summary>
        /// Removes the special state of the window; the pass through and key hooking and transparency
        /// </summary>
        [CAspects.Logging]
        internal void MakeNormalWindow()
        {
            Logging.Log([$"Changing WinStyle of HWND {hwnd}"]);
            int os = GetWindowLongWrapper(hwnd, GWL_EXSTYLE);
            SetWindowLongWrapper(hwnd, GWL_EXSTYLE, originalStyle | WS_EX_LAYERED | WS_EX_TOOLWINDOW);
            int es = GetWindowLongWrapper(hwnd, GWL_EXSTYLE);
            Logging.Log([$"Set WinStyle of HWND {hwnd} from {os:X} ({os:B}) [{os}] to {es:X} ({es:B}) [{es}]"]);
            DestroyKeyHook();
        }

        /// <summary>
        /// Makes the window special again; transparency, passthough, hooking, etc
        /// </summary>
        [CAspects.Logging]
        internal void MakeFunnyWindow()
        {
            Logging.Log([$"Changing WinStyle of HWND {hwnd}"]);
            int os = GetWindowLongWrapper(hwnd, GWL_EXSTYLE);
            SetWindowLongWrapper(hwnd, GWL_EXSTYLE, editedstyle);
            int es = GetWindowLongWrapper(hwnd, GWL_EXSTYLE);
            Logging.Log([$"Set WinStyle of HWND {hwnd} from {os:X} ({os:B}) [{os}] to {es:X} ({es:B}) [{es}]"]);
            InitKeyHook();
        }

        /// <summary>
        /// The main UI for STELLA
        /// </summary>
        internal class Interface : Canvas
        {
            /// <summary>
            ///  The background overlay
            /// </summary>
            internal SWS.Rectangle Backg { get; set; }
            /// <summary>
            /// the command input box
            /// </summary>
            internal SWC.TextBox inputTextBox { get; private set; }

            /// <summary>
            /// Mirror property because its so much nicer to just type <b><i><c>Interface.Input</c></i></b> than it is to type <b><i><c>Interface.inst.inputTextBox.Text</c></i></b>
            /// </summary>
            internal static string Input { get => inst.inputTextBox.Text; set => inst.inputTextBox.Text = value; }

            /// <summary>
            /// The UI Output
            /// </summary>
            /// <remarks>
            /// Static so that it can be updated while the UI is closed, and it keeps the text through reopening STELLA's interface
            /// </remarks>
            internal static LogListBox logListBox = new();
            /// <summary>
            /// Singleton instance of STELLA's interface
            /// </summary>
            internal static Interface? inst = null;
            /// <summary>
            /// The parent STELLA's interface is connected to, for now this is always <b><i><c><see cref="Catowo"/>.<see cref="Catowo.canvas"/></c></i></b>
            /// </summary>
            internal Canvas parent;
            /// <summary>
            /// The scroll viewer of the UI, for auto scrolling when new outputs appear
            /// </summary>
            private static ScrollViewer _scrollViewer;

            /// <summary>
            /// Represents the graphical user interface layer of STELLA, providing methods and properties to manage its visibility and interactions.
            /// </summary>
            internal Interface(Canvas parent)
            {
                //var screen = GetScreen();
                //SetTop(this, 0 - screen.Bounds.Height);
                this.parent = parent;
                CommandProcessing.@interface = this;
                inst?.Children.Clear();
                inst?.parent.Children.Remove(inst);
                inst = null;
                inst = this;
                Backg = InitBackg();
                InitializeComponents();
                AnimateUp();
                Loaded += (_, _) => inputTextBox?.Focus();
                //SetBottom(this, 0);
                //MouseMove += (s, e) => Catowo.inst.ToggleInterface();
            }

            /// <summary>
            /// Initializes the background rectangle for STELLA's interface, setting its dimensions and opacity.
            /// </summary>
            /// <returns>A rectangle that serves as the background for STELLA's interface.</returns>
            private SWS.Rectangle InitBackg()
            {
                var scre = GetScreen();
                var (rect, _) = Helpers.ScreenSizing.GetAdjustedScreenSize(scre);
                Backg = new SWS.Rectangle { Width = rect.Width, Height = rect.Height, Fill = new SWM.SolidColorBrush(SWM.Colors.Gray), Opacity = UserData.Opacity };
                Logging.Log([$"{Catowo.inst.Screen}, {Backg.Width} {Backg.Height}"]);
#if CrashWary // This just makes it so that it leaves a small margin for me to still interact with the windows behind it when STELLA crashes
                rect.Height = rect.Height - 50;
                SetTop<double>(rect, 50);
#else
                SetTop<double>(Backg, 0);
#endif
                SetLeft<double>(Backg, 0);
                Children.Add(Backg);
                return Backg;
            }

            /// <summary>
            /// Method that initialises the visual components of the UI
            /// </summary>
            [CAspects.Logging]
            [CAspects.ConsumeException]
            [CAspects.UpsetStomach]
            private void InitializeComponents()
            {
                Screen screen = GetScreen();
                var (rect, workAreaHeight) = Helpers.ScreenSizing.GetAdjustedScreenSize(screen);
#if CrashWary
                rect.Height = rect.Height - 50;
#endif
                double taskbarHeight = rect.Height - workAreaHeight;
                double padding = 20;
                double inputTextBoxHeight = 30;

                inputTextBox = new SWC.TextBox
                {
                    Width = rect.Width - (padding * 2),
                    Height = inputTextBoxHeight,
                    Margin = new Thickness(0, 0, 0, padding),
                    Background = WABrush,
                    Foreground = SWM.Brushes.Black,
                    Focusable = true
                };

                inputTextBox.PreviewKeyDown += (s, e) =>
                {
                    Logging.Log([((int)e.Key)]);
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
#if CrashWary
                SetTop<double>(inputTextBox, (screenHeight - taskbarHeight - inputTextBoxHeight - (padding)) + 50);
#else
                SetTop<double>(inputTextBox, rect.Height - taskbarHeight - inputTextBoxHeight - (padding));
#endif

                logListBox.Width = rect.Width - (padding * 2);
                logListBox.Height = rect.Height - taskbarHeight - inputTextBoxHeight - (padding * 3);

                SetLeft<double>(logListBox, padding);
#if CrashWary
                SetTop<double>(logListBox, padding + 50);
#else
                SetTop<double>(logListBox, padding);
#endif
                Children.Add(inputTextBox);
                Children.Add(logListBox);
                inputTextBox.Focus();
            }

            /// <summary>
            /// Updates the UI's visuals
            /// </summary>
            internal void UpdateInterface()
            {
                Screen screen = GetScreen();
                var (Rect, workAreaHeight) = Helpers.ScreenSizing.GetAdjustedScreenSize(screen);

                double taskbarHeight = Rect.Height- workAreaHeight;
                double padding = 20;
                double inputTextBoxHeight = 30;
                inputTextBox.Width = Rect.Width - (padding * 2);
                inputTextBox.Height = inputTextBoxHeight;
                SetTop<double>(inputTextBox, Rect.Height - taskbarHeight - inputTextBoxHeight - (padding));
                logListBox.Width = Rect.Width - (padding * 2);
                logListBox.Height = Rect.Height - taskbarHeight - inputTextBoxHeight - (padding * 3);

                Backg.Width = Rect.Width;
                Backg.Height = Rect.Height;
            }

            /// <summary>
            /// Thread-Safely hides STELLA's interface, setting its visibility to collapsed and ensuring the UI updates immediately.
            /// </summary>
            internal async Task Hide()
            {
                Logging.Log(["Hiding interface..."]);
                Dispatcher.Invoke(() => { Visibility = Visibility.Collapsed; }, DispatcherPriority.Render);
                await Task.Delay(500);
                Logging.Log(["Interface hidden"]);
            }

            /// <summary>
            /// Makes STELLA's interface visible.
            /// </summary>
            internal void Show()
            {
                Logging.Log(["Showing interface..."]);
                Visibility = Visibility.Visible;
                inputTextBox.Focus();
                Logging.Log(["Interface Shown"]);
            }

            /// <summary>
            /// Adds a log message to STELLA's interface's log list box.
            /// </summary>
            /// <param name="logMessage">The message to log.</param>
            /// <returns>An integer representing the position of the newly added log message in the log list box.</returns>
            [CAspects.ConsumeException]
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

            /// <summary>
            /// Edits a log message in STELLA's interface's log list box at a specified index.
            /// </summary>
            /// <param name="message">The new log message.</param>
            /// <param name="id">The index of the log message to edit.</param>
            /// <param name="fromEnd">Whether the index is counted from the end of the log list.</param>
            [CAspects.ConsumeException]
            internal static int AddLog(params string[] logs)
            {
                Interface? instance = inst;
                if (instance == null) return -2;
                // Log each log
                foreach (string log in logs)
                    instance.Dispatcher.Invoke(() => logListBox.AddItem(log));
                if (_scrollViewer != null)
                    _scrollViewer.ScrollToEnd();
                else
                    logListBox.ScrollIntoView(logListBox.Items[logListBox.Items.Count - 1]);
                return logListBox.Items.Count - 1;
            }

            /// <summary>
            /// Edits a log message in STELLA's interface's log list box at a specified index.
            /// </summary>
            /// <param name="message">The new log message.</param>
            /// <param name="id">The index of the log message to edit.</param>
            /// <param name="fromEnd">Whether the index is counted from the end of the log list.</param>
            [CAspects.ConsumeException]
            internal static void EditLog(string message, int id, bool fromEnd)
            {
                Interface? instance = inst;
                if (instance == null) return;
                // Find the index of the item
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
                // Immedietely flag the ui for visual update
                instance.InvalidateVisual();
                if (_scrollViewer != null)
                    _scrollViewer.ScrollToEnd();
                else
                    logListBox.ScrollIntoView(logListBox.Items[logListBox.Items.Count - 1]);
            }

            /// <summary>
            /// Adds a log message to STELLA's interface's log list box with specified text color.
            /// </summary>
            /// <param name="logMessage">The log message to add.</param>
            /// <param name="color">The color of the text.</param>
            /// <returns>An integer representing the position of the newly added log message in the log list box.</returns>
            [CAspects.ConsumeException]
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

            /// <summary>
            /// Same as <b><i><c><see cref="Catowo.Interface"/>.<see cref="Interface.AddTextLog(string, SolidColorBrush)"/></c></i></b> but it returns the index of the log AND the log item itself as a <b><i><c><see cref="Tuple"/>(<see cref="int"/>, <see cref="TextBlock"/>)</c></i></b>            
            /// </summary>
            /// <param name="logMessage"></param>
            /// <param name="brush"></param>
            /// <returns></returns>
            [CAspects.ConsumeException]
            internal static (int, TextBlock) AddTextLogR(string logMessage, SolidColorBrush brush = null)
            {
                Interface? instance = inst;
                TextBlock block = null;
                int value = -2;
                Catowo.inst.Dispatcher.Invoke(() =>
                {
                    if (instance == null)
                        return;
                    brush ??= new(Colors.White);
                    block = new TextBlock { Text = logMessage, Foreground = brush };
                    value = instance.Dispatcher.Invoke(() => logListBox.AddItem(block));
                    if (_scrollViewer != null)
                        _scrollViewer.ScrollToEnd();
                    else
                        logListBox.ScrollIntoView(logListBox.Items[logListBox.Items.Count - 1]);
                });
                return (value, block);
            }

            /// <summary>
            /// Same as <b><i><c><see cref="Catowo.Interface"/>.<see cref="Interface.AddTextLog(string, SolidColorBrush)"/></c></i></b> but it takes in a <b><i><c><see cref="SolidColorBrush"/></c></i></b> instead of a <b><i><c><see cref="Color"/></c></i></b>
            /// </summary>
            /// <param name="logMessage"></param>
            /// <param name="brush"></param>
            /// <returns></returns>
            [CAspects.ConsumeException]
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

            internal TaskCompletionSource<bool> AnimationTCS { get; private set; }

            /// <summary>
            /// Animated the interface to move up
            /// </summary>
            internal void AnimateUp()
            {
                AnimationTCS = new();
                TranslateTransform trans = new TranslateTransform();
                RenderTransform = trans;
                double screenHeight = Catowo.GetScreen().Bounds.Height;
                double initialOffset = screenHeight -ActualHeight;
                double duration = 1.0;
                trans.Y = -initialOffset;

                Storyboard storyboard = new Storyboard();

                DoubleAnimation bounceUp = new DoubleAnimation()
                {
                    From = -initialOffset,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(duration),
                    EasingFunction = new BounceEase { EasingMode = EasingMode.EaseOut, Bounces = 10, Bounciness = 2 }
                };
                Storyboard.SetTarget(bounceUp, this);
                Storyboard.SetTargetProperty(bounceUp, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
                storyboard.Children.Add(bounceUp);
                storyboard.Begin(this, true);
                AnimationTCS.SetResult(true);
            }

            /// <summary>
            /// Closing animation
            /// </summary>
            /// <param name="complete"></param>
            internal async void ShrinkAndDisappear(Action complete)
            {
                AnimationTCS = new();
                var sc = Catowo.GetScreen().Bounds;
                double initialWidth = sc.Width;
                double initialHeight = sc.Height;
                ScaleTransform scale = new(1, 1);
                TranslateTransform translate = new();
                TransformGroup transforms = new();
                transforms.Children.Add(scale);
                transforms.Children.Add(translate);
                RenderTransform = transforms;
                RenderTransformOrigin = new(0.5, 0.5);

                double duration = 0.25; 

                Storyboard storyboard = new();
                DoubleAnimation scaleAnimation = new()
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(duration)
                };
                Storyboard.SetTarget(scaleAnimation, this);
                Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath("RenderTransform.Children[0].ScaleX"));
                storyboard.Children.Add(scaleAnimation);

                DoubleAnimation scaleAnimationY = scaleAnimation.Clone();
                Storyboard.SetTargetProperty(scaleAnimationY, new PropertyPath("RenderTransform.Children[0].ScaleY"));
                storyboard.Children.Add(scaleAnimationY);

                DoubleAnimation translateXAnimation = new()
                {
                    From = 0,
                    To = initialWidth / 2,
                    Duration = TimeSpan.FromSeconds(duration)
                };
                Storyboard.SetTarget(translateXAnimation, this);
                Storyboard.SetTargetProperty(translateXAnimation, new PropertyPath("RenderTransform.Children[1].X"));
                storyboard.Children.Add(translateXAnimation);

                DoubleAnimation translateYAnimation = new()
                {
                    From = 0,
                    To = initialHeight / 2,
                    Duration = TimeSpan.FromSeconds(duration)
                };
                Storyboard.SetTarget(translateYAnimation, this);
                Storyboard.SetTargetProperty(translateYAnimation, new PropertyPath("RenderTransform.Children[1].Y"));
                storyboard.Children.Add(translateYAnimation);

                // Animation begins
                storyboard.Begin(this, true);
                await Task.Delay(TimeSpan.FromSeconds(duration + 0.1)); 
                Logging.Log(["Shrink animation complete by async delay."]);
                complete?.Invoke();
                AnimationTCS.SetResult(true);
            }

            /// <summary>
            /// Provides methods for processing user commands input into STELLA's interface, including executing specific actions based on command identifiers and managing command history.
            /// </summary>
            internal static class CommandProcessing
            {
                /// <summary>
                /// Interface instance reference
                /// </summary>
                internal static Interface @interface { get => Commands.@interface; set => Commands.@interface = value; }
                /// <summary>
                /// Schema built for the executing command
                /// </summary>
                private static Command? commandstruct;
                /// <summary>
                /// Command history, stores the past 10 commands (backwards, need to fix ^^;)
                /// </summary>
                private static readonly FixedQueue<string> History = new(10);
                /// <summary>
                /// The raw command text
                /// </summary>
                private static string cmdtext;

                /// <summary>
                /// Reference map of command aliases to their actual functional index
                /// </summary>
                internal static Dictionary<string, int> cmdmap { get; } = new()
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
                    { "dexpr", 27 },
                    { "d expr", 27 },
                    { "download external item", 27 },

                    { "run progress bar test", 28 },

                    { "view settings", 29 },
                    { "see settings", 29 },
                    { "print settings", 29 },
                    { "vs", 29 },

                    { "load cursor preset", 30 },
                    { "lcp", 30 },
                    { "load cursors", 30 },

                    { "add cursor preset", 31 },
                    { "acp", 31 },
                    { "create cursor preset", 31 },

                    { "add cursor to preset", 32 },
                    { "actp", 32 },

                    { "remove cursor from preset", 33 },
                    { "rcfp", 33 },

                    { "list cursor presets", 34 },
                    { "lcps", 34 },
                    { "list cursor preset", 34 },

                    { "open log editor", 35 },
                    { "ole", 35},

                    { "elevate perms", 36 },
                    { "ep", 36 },

                    { "test error", 37 },

                    { "activate voice", 38 },
                    { "toggle stt", 38 },
                    { "toggle voice", 38 },

                    { "close log editor", 39 },
                    { "cle", 39 },

                    { "define", 40 },

                    { "tutorial", 41 },

                    { "read object", 42 },
                    { "ro", 42 },

                    { "read binary", 43 },
                    { "rb", 43 },

                    { "toggle cursor effects", 46 },
                    { "tce", 46 },

                    { "shrink", 44 },
                    { "grow", 45 },

                    { "open settings", 47 },
                    { "ops", 47 }
                };

                /// <summary>
                /// Defines mapping command identifiers to their descriptions, parameters, associated functions, and shortcuts.
                /// </summary>
                /// <remarks>
                /// Each command is identified by an integer key and contains a dictionary with the following keys:
                /// - <c>desc</c>: A string describing what the command does.
                /// - <c>params</c>: A string detailing the parameters the command accepts, with types and optionality.
                /// - <c>function</c>: A delegate to the function that implements the command's functionality.
                /// - <c>shortcut</c>: A string representing the keyboard shortcut associated with the command, if any.
                /// Commands are used throughout STELLA to implement functionality accessible through the user interface or keyboard shortcuts.
                /// </remarks>
                /// <example>
                /// a regal PAIN in the ass to refactor
                /// </example>
                internal static Dictionary<int, CommandSchema> Cmds { get; } = new()
                {
                    {
                        0, new CommandSchema(
                            "Shuts down the entire program",
                            "",
                            (Func<Task<bool>>)Cat.Commands.Shutdown,
                            Commands.TShutdown,
                            "LShift RShift Q E",
                            0
                        )
                    },
                    {
                        1, new CommandSchema(
                            "Closes STELLA's interface, the shortcut will open it.",
                            "",
                            (async () => { return await Catowo.inst.ToggleInterface(); }),
                            null,
                            "LShift RShift Q I",
                            0
                        )
                    },
                    {
                        2, new CommandSchema(
                            "Shifts STELLA's interface screen to another monitor, takes in a number corresponding to the monitor you want it to shift to (1 being primary)",
                            "screennum{int}",
                            Cat.Commands.ChangeScreen,
                            Commands.TChangeScreen,
                            "LShift RShift Q (number)",
                            0
                        )
                    },
                    {
                        3, new CommandSchema(
                            "Takes a screenshot of the screen, without STELLA's interface. -2 for a stitched image of all screens, -1 for individual screen pics, (number) for an individual screen, leave empty for the current screen Kitty is running on.\nE.g: screenshot ;-2",
                            "[mode{int}]",
                            (Func<Task<bool>>)Cat.Commands.Screenshot,
                            Commands.TScreenshot,
                            "LShift RShift Q S",
                            0
                        )
                    },
                    {
                        4, new CommandSchema(
                            "Begins capturing screen as a video, multi-monitor support coming soon. Closes STELLA's interface when run.",
                            "",
                            Cat.Commands.StartRecording,
                            null,
                            "LShift RShift Q R",
                            2
                        )
                    },
                    {
                        5, new CommandSchema(
                            "Starts capturing system audio, with optional audio input (0/exclusive, 1/inclusive).\n- Exclusive means only audio input, inclusive means audio input and system audio\nE.g: capture audio ;exclusive\nE.g: capture audio ;1",
                            "[mode{int/string}]",
                            Cat.Commands.StartAudioRecording,
                            null,
                            "",
                            2
                        )
                    },
                    {
                        6, new CommandSchema(
                            "Stops a currently running recording session, with an optional opening of the recording location after saving (true)\nE.g: stop recording ;true",
                            "",
                            Cat.Commands.StopRecording,
                            null,
                            "LShift RShift Q D",
                            2
                        )
                    },
                    {
                        7, new CommandSchema(
                            "Stops a currently running audio session, with optional opening of the file location after saving.\nE.g: stop audio ;true",
                            "",
                            Cat.Commands.StopAudioRecording,
                            null,
                            "",
                            2
                        )
                    },
                    {
                        8, new CommandSchema(
                            "Plays an audio file, present the filepath as an argument with optional looping.\nE.g: play audio ;C:/Downloads/Sussyaudio.mp4 ;true",
                            "filepath{string}, [looping{bool}]",
                            Cat.Commands.PlayAudio,
                            Commands.TPlayAudio,
                            "",
                            0
                        )
                    },
                    {
                        9, new CommandSchema(
                            "Changes a control setting, you must specify the \nE.g: change setting ;LogAssemblies ;true\nE.g: change setting ;background ;green",
                            "variablename{string}, value{string}",
                            Cat.Commands.ChangeSettings,
                            Commands.TChangeSettings,
                            "",
                            0
                        )
                    },
                    {
                        10, new CommandSchema(
                            "Takes a 'snapshot' of a specified process and shows information like its memory usage, CPU usage, etc.\nE.g: take process snapshot ;devenv\nE.g: take process snapshot ;9926381232",
                            "process{string/int}",
                            Cat.Commands.TakeProcessSnapshot,
                            null,
                            "LShift RShift Q T",
                            2
                        )
                    },
                    {
                        11, new CommandSchema(
                            "Starts measuring a process's information until stopped.\nE.g: start measuring process ;devenv",
                            "",
                            Cat.Commands.StartProcessMeasuring,
                            Commands.TStartProcessMeasuring,
                            "LShift RShift Q X",
                            2
                        )
                    },
                    {
                        12, new CommandSchema(
                            "Stops a currently running process measuring session, with an optional saving of the data.\nE.g: stop measuring process ;false",
                            "[savedata{bool}]",
                            Cat.Commands.StopProcessMeasuring,
                            null,
                            "LShift RShift Q C",
                            2
                        )
                    },
                    {
                        13, new CommandSchema(
                            "Opens the logs folder.\nE.g: open logs",
                            "",
                            Cat.Commands.OpenLogs,
                            Commands.TOpenLogs,
                            "",
                            0
                        )
                    },
                    {
                        15, new CommandSchema(
                            "(Attempts to) Changes the cursor to the specified cursor file, specifying file path.\nE.g: change cursor ;the/path/to/your/.cur/file",
                            "path{string}",
                            Cat.Commands.ChangeCursor,
                            Commands.TChangeCursor,
                            "",
                            0
                        )
                    },
                    {
                        16, new CommandSchema(
                            "Resets all system cursors",
                            "",
                            Cat.Commands.ResetCursor,
                            Commands.TResetCursor,
                            "",
                            0
                        )
                    },
                    {
                        17, new CommandSchema(
                            "Plots a set of data, specifying file path(s) or data in the format: ;int, int, int, ... int ;int, int, int, ... int (two sets of data).\nE.g: plot ;path/to/a/csv/with/two/lines/of/data\nE.g: plot ;path/to/csv/with/x_axis/data ;path/to/2nd/csv/with/y_axis/data\nE.g: plot ;1, 2, 3, 4, 5, 6 ;66, 33, 231, 53242, 564345",
                            "filepath{string} | filepath1{string} filepath2{string} | data1{int[]} data2{int[]}",
                            Cat.Commands.Plot,
                            null,
                            "",
                            2
                        )
                    },
                    {
                        18, new CommandSchema(
                            "Saves a currently open plot (Plot must be open) to a file.\nE.g: save plot",
                            "",
                            Cat.Commands.SavePlot,
                            null,
                            "",
                            2
                        )
                    },
                    {
                        19, new CommandSchema(
                            "Shows a random kitty :3",
                            "[cats{int}]",
                            Cat.Commands.RandomCatPicture,
                            Commands.TRandomCatPicture,
                            "LShift RShift Q K",
                            0
                        )
                    },
                    {
                        20, new CommandSchema(
                            "Shows a list of commands, specific command info or general info.\nE.g: help\nE.g: help ;commands\nE.g:help ;plot",
                            "[cmdname{string}]",
                            Cat.Commands.Help,
                            Commands.THelp,
                            "",
                            0
                        )
                    },
                    {
                        21, new CommandSchema(
                            "Displays either all screen information, or just a specified one.\ndsi ;1",
                            "[screennumber{int}]",
                            Cat.Commands.DisplayScreenInformation,
                            Commands.TDSI,
                            "",
                            1
                        )
                    },
                    {
                        22, new CommandSchema(
                            "Opens the live logger. \nE.g:sll",
                            "",
                            Cat.Commands.OpenLogger,
                            Commands.TOpenLogger,
                            "LShift RShift Q ,",
                            0
                        )
                    },
                    {
                        23, new CommandSchema(
                            "Closes an open live logger\nE.g: cll",
                            "",
                            Cat.Commands.CloseLogger,
                            Commands.TCloseLogger,
                            "LShift RShift Q .",
                            0
                        )
                    },
                    {
                        24, new CommandSchema(
                            "Aborts a currently playing audio file.",
                            "",
                            Cat.Commands.StopAudio,
                            Commands.TStopAudio,
                            "LShift RShift Q V",
                            0
                        )
                    },
                    {
                        25, new CommandSchema(
                            "Prints STELLA's interface element details",
                            "",
                            Cat.Commands.PrintElementDetails,
                            null,
                            "",
                            1
                        )
                    },
                    {
                        26, new CommandSchema(
                            "Forces a logging flush",
                            "",
                            (Func<Task<bool>>)Cat.Commands.FML,
                            Commands.TFlushLogs,
                            "LShift RShift Q F",
                            0
                        )
                    },
                    {
                        27, new CommandSchema(
                            "Downloads exprs",
                            "processname{string}",
                            (Func<Task<bool>>)Cat.Commands.DEP,
                            Commands.TDEP,
                            "",
                            0
                        )
                    },
                    {
                        28, new CommandSchema(
                            "Generates a progress bar test",
                            "",
                            Cat.Commands.GPT,
                            null,
                            "",
                            1
                        )
                    },
                    {
                        29, new CommandSchema(
                            "Prints all user settings in the format:\n[Section]\n  [Key]: [Value]",
                            "",
                            Cat.Commands.ShowSettings,
                            Commands.TShowSettings,
                            "",
                            0
                        )
                    },
                    {
                        30, new CommandSchema(
                            "Loads a specified cursor preset",
                            "listname{string}, [persistent{bool}]",
                            Cat.Commands.LoadCursorPreset,
                            Commands.TLoadCursorPreset,
                            "",
                            0
                        )
                    },
                    {
                        31, new CommandSchema(
                            "Creates a new cursor preset",
                            "listname{string}",
                            Cat.Commands.AddCursorPreset,
                            Commands.TAddCursorPreset,
                            "",
                            0
                        )
                    },
                    {
                        32, new CommandSchema(
                            "Adds a cursor to a preset",
                            "preset{string}, cursorid{string}, filepath{string}",
                            Cat.Commands.AddCursorToPreset,
                            Commands.TAddCursorToPreset,
                            "",
                            0
                        )
                    },
                    {
                        33, new CommandSchema(
                            "Removes a cursor from a preset",
                            "preset{string}, cursorid{string}",
                            Cat.Commands.RemoveCursorFromPreset,
                            null,
                            "",
                            0
                        )
                    },
                    {
                        34, new CommandSchema(
                            "Lists all presets, or optionally all cursors changed in a preset",
                            "[preset{string}]",
                            Cat.Commands.ListCursorPreset,
                            Commands.TListCursorPreset,
                            "",
                            0
                        )
                    },
                    {
                        35, new CommandSchema(
                            "Opens the log editing GUI",
                            "",
                            Cat.Commands.OpenLogEditor,
                            Commands.TOpenLogEditor,
                            "",
                            0
                        )
                    },
                    {
                        36, new CommandSchema(
                            "Restarts STELLA asking for elevation (admin)",
                            "",
                            Cat.Commands.KillMyselfAndGetGodPowers,
                            Commands.TKillMyselfAndGetGodPowers,
                            "",
                            0
                        )
                    },
                    {
                        37, new CommandSchema(
                            "[DEBUG] Throws an error",
                            "",
                            Cat.Commands.ThrowError,
                            null,
                            "",
                            2
                        )
                    },
                    {
                        38, new CommandSchema(
                            "Activates voice recognition",
                            "",
                            Cat.Commands.AV,
                            Commands.TAV,
                            "",
                            0
                        )
                    },
                    {
                        39, new CommandSchema(
                            "Closes the log editor",
                            "",
                            Cat.Commands.CLE,
                            Commands.TCLE,
                            "",
                            0
                        )
                    },
                    {
                        40, new CommandSchema(
                            "Defines a word using the DictionaryAPI or the UrbanDictionaryAPI (if allowed)",
                            "word{string}",
                            Cat.Commands.Define,
                            Commands.TDefine,
                            "",
                            0
                        )
                    },
                    {
                        41, new CommandSchema(
                            "Runs the tutorial sequence for a given command.\nRun it with 'tutorial ;commandName' where 'commandName' is the name of any command.",
                            "commandname{string}",
                            Cat.Commands.Tutorial,
                            Commands.TTutorial,
                            "",
                            0
                        )
                    },
                    {
                        42, new CommandSchema(
                            "Reads a saved object from a data file",
                            "filepath {string}, objectname{string}",
                            Commands.ReadObject,
                            Commands.TReadObject,
                            "",
                            2
                        )
                    },
                    {
                        43, new CommandSchema(
                            "[DEBUG] Prints the Raw BinaHex and Translation of a binary file",
                            "filename{string}",
                            Commands.PRB,
                            null,
                            "",
                            1
                        )
                    },
                    {
                        44, new CommandSchema(
                                "",
                                "",
                                () => { Catowo.inst.Top = Catowo.inst.Left = 20;  Catowo.inst.Width = Catowo.inst.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 10; Catowo.inst.WindowState = WindowState.Normal;  return true;},
                                null,
                                "",
                                1
                            )
                    },
                    {
                        45, new CommandSchema(
                            "",
                            "",
                            () => { Catowo.inst.WindowState = WindowState.Maximized; return true; },
                            null,
                            "",
                            1
                            )
                    },
                    {
                        46, new CommandSchema(
                            "Toggles Cursor effects!",
                            "",
                            (() =>
                            {
                                CursorEffects.Toggle();
                                return true;
                            }),
                            null,
                            "",
                            0
                        )
                    },
                    {
                        47, new CommandSchema(
                            "Opens the settings menu",
                            "", 
                            Commands.OpenSettings,
                            Commands.TOpenSettings,
                            "",
                            0
                        )
                    }
                };

                /// <summary>
                /// Structure to hold the commands for indexing 
                /// </summary>
                /// <param name="desc">The description of the command</param>
                /// <param name="parameters">The parameters the command accepts</param>
                /// <param name="function">The actual function of the command</param>
                /// <param name="tutorial">The tutorial function of the command</param>
                /// <param name="shortcut">The shortcut (if any) for the command to be executed with</param>
                /// <param name="type">Whether its complete, in dev or just a debug command</param>
                internal readonly record struct CommandSchema(string desc, string parameters, Delegate function, Delegate tutorial, string shortcut, int type);

                /// <summary>
                /// Navigates to the previous command in the history queue and displays it in the input text box.
                /// </summary>
                /// <remarks>
                /// If there is a previously executed command available, it retrieves and sets it as the current text of the input box.
                /// If no previous command is available or if retrieving the previous command fails, no action is taken.
                /// </remarks>
                [CAspects.Logging]
                internal static void HistoryUp()
                {
                    string? previousraw = History.GetPrevious();
                    if (previousraw == null || History.Failed)
                    {
                        History.Failed = false;
                        return;
                    }
                    @interface.inputTextBox.Text = previousraw;
                }

                /// <summary>
                /// Navigates to the next command in the history queue and displays it in the input text box.
                /// </summary>
                /// <remarks>
                /// If there is a next command available, it retrieves and sets it as the current text of the input box.
                /// If no next command is available or if retrieving the next command fails, no action is taken.
                /// This method complements the HistoryUp method, allowing users to navigate through the command history.
                /// </remarks>
                [CAspects.Logging]
                internal static void HistoryDown()
                {
                    string? nextraw = History.GetNext();
                    if (nextraw == null || History.Failed)
                    {
                        History.Failed = false;
                        return;
                    }
                    @interface.inputTextBox.Text = nextraw;
                }

                /// <summary>
                /// Processes the command currently entered in the input text box, executing the associated action.
                /// </summary>
                /// <remarks>
                /// Extracts the command from the input text box, logs the command input, and attempts to find and execute the command using the command map.
                /// If the command is successfully found and parsed, it executes the associated action or function.
                /// If the command execution involves an asynchronous operation, it waits for the operation to complete.
                /// Logs an error and updates STELLA's interface with feedback if the command cannot be found, fails to parse, or if the associated action or function cannot be executed.
                /// Clears the input text box upon completion.
                /// </remarks>
                [CAspects.Logging]
                [MethodImpl(MethodImplOptions.AggressiveOptimization)]
                internal static async void ProcessCommand(string non_interface_text = null)
                {
                    commandstruct = null;
                    Commands.commandstruct = null;
                    // Determine to use inputted text or inserted text
                    if (non_interface_text == null && @interface != null) cmdtext = @interface.inputTextBox.Text.Trim().ToLower();
                    else cmdtext = non_interface_text;
                    // queue the raw text
                    History.Enqueue(cmdtext);
                    // Get the call of the command (the name of the command being invoked, no parameters)
                    string call = cmdtext.Split(";")[0].Trim();
                    Logging.Log([$"Processing Interface Command, Input: {cmdtext}"]);

                    // Check if call is valid
                    if (cmdmap.TryGetValue(call, out int value))
                    {
                        // Get the call's functional index
                        int index = value;
                        // Get the metadata linked to the index
                        CommandSchema metadata = Cmds[index];
                        // Split the cmdtext parameters
                        var parts = cmdtext.Split(';');
                        // Check for any parameters
                        if (parts.Length > 1)
                        {
                            var parametersToLog = string.Join(";", parts.Skip(1));
                            Logging.Log([$"Executing command {call}, index {index} with entered parameters {parametersToLog}"]);
                        }
                        else Logging.Log([$"Executing command {call}, index {index} with no entered parameters"]);

                        // Parse the command
                        bool parsestate = ParameterParsing.ParseCommand(cmdtext, value, out Command? commandstruct2, out string? error_message);
                        if (commandstruct2 != commandstruct && commandstruct2 != null)
                        {
                            commandstruct = commandstruct2;
                            Commands.commandstruct = commandstruct;
                        }

                        if (!parsestate)
                        {
                            Logging.Log(["Failed to parse command."]);
                            Interface.AddTextLog("Execution terminated.", RED);
                            return;
                        }
                        if (!string.IsNullOrEmpty(error_message))
                            Interface.AddTextLog(error_message, RED);

                        bool? result = null;
                        // Execute the function
                        if (metadata.function is Func<bool> func)
                            result = func();
                        else if (metadata.function is Func<Task<bool>> tfunc)
                            result = await tfunc();
                        else
                        {
                            Logging.Log([">>>ERROR<<< Action nor TFunct not found for the given command ID."]);
                            Interface.AddTextLog($"Action nor TFunct object not found for command {call}, stopping command execution.\nThis... shouldn't happen. hm.", SWM.Color.FromRgb(200, 0, 40));
                        }
                        if (result == false && Commands.ActualError)
                            Interface.AddTextLog($"Something went wrong executing {cmdtext}", RED);
                        Commands.ActualError = true;
                        Logging.Log([$"Finished Processing command {call}"]);
                    }
                    else
                    {
                        Logging.Log(["Command Not Found"]);
                        Interface.AddLog($"No recognisable command '{call}', please use 'help ;commands' for a list of commands!");
                    }
                    if (@interface != null && non_interface_text == null)
                        @interface.inputTextBox.Text = string.Empty;
                    Interface.AddLog("\n");
                }

                /// <summary>
                /// Provides functionality for parsing command strings into structured command objects, enabling command execution based on user input.
                /// </summary>
                private static class ParameterParsing
                {
                    /// <summary>
                    /// Main method for command parsing 
                    /// </summary>
                    [CAspects.ConsumeException]
                    [CAspects.Logging]
                    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
                    internal static bool ParseCommand(
                        in string raw, 
                        in int num, 
                        out Command? command, 
                        out string? error_message)
                    {
                        //!! I'm going to leave comments here because this will probably be rather complex :p

                        command = null;
                        error_message = "";
                        string call = raw.Split(";")[0].Trim();
                        // First, get the different sequences of expected parameters
                        // If metadata is null then something's gone wrong with extracting it from the
                        // Commands dictionary... which is really bad.
                        if (CommandProcessing.Cmds[num].parameters is not string metadata)
                        {
                            Logging.Log(["[CRITICAL ERROR] Metadata was unresolved, command cannot be processed. You'll have to make a bug report (attach this log) so this can be fixed in the code behind, apologies for the inconvenience."]);
                            error_message = "Metadata resolve error, please see logs for details.";
                            return false;
                        }
                        metadata = metadata.Replace(", ", "").Trim();
                        // The command doesn't accept parameters, so skip parsing them and execute.
                        if (metadata == string.Empty || metadata == "")
                        {
                            Logging.Log(["Command accepts no parameters, skipping parse."]);
                            command = new(call, raw);
                            return true;
                        }
                        // split parameters
                        var linputs = raw.Split(";").ToList();
                        linputs.RemoveAt(0);
                        string[] inputs = linputs.ToArray();
                        // Split the metadata into every accepted pattern (patterns seperated by | )
                        string[] patterns = metadata.Contains('|') ? metadata.Split('|') : [metadata,];
                        Logging.Log(["Optionals", patterns]);
                        //x List<string> couldbes = new(optionals.Length);

                        // Check the input against each pattern
                        foreach (string pattern in patterns)
                        {
                            // Parse the pattern
                            bool? status = ParsePattern(inputs, pattern, out error_message, out object[][]? parsedparams);
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
                        Logging.Log([$"[PARSE FAILED] No matching sequence to input found. Please use 'help ;{call}' to see expected command parameters."]);
                        error_message = "Unrecognised arguments / argument pattern." + (error_message != "" && error_message != null ? "Additional Error(s): " + error_message : "");
                        Interface.AddTextLog(error_message, Colors.Red);
                        return false;
                    }

                    /// <summary>
                    /// Submodule for parsing individual patterns
                    /// </summary>
                    /// <param name="inputs"></param>
                    /// <param name="sequence"></param>
                    /// <param name="error_message"></param>
                    /// <param name="parsedparams"></param>
                    /// <returns></returns>
                    [CAspects.ConsumeException]
                    [CAspects.Logging]
                    internal static bool? ParsePattern(string[] inputs, string sequence, out string? error_message, out object[][]? parsedparams)
                    {
                        error_message = null;
                        parsedparams = null;
                        int all = sequence.Count(c => c == '{'); // count all placeholders
                        int flex = sequence.Count(c => c == '['); // count optional placeholders
                        int fix = all - flex; // fixed placeholders
                        Logging.Log([$"All: {all}", $"Flex: {flex}", $"Fixed: {fix}"]);

                        if (fix == 0 && inputs.Length == 0)
                        {
                            Logging.Log(["Sequence only accepts optionals and there were no given inputs. End of Parse"]);
                            return null; // early exit for only optional placeholders and no inputs
                        }

                        if (fix > inputs.Length)
                        {
                            string mes = "[PARSE ERROR] Inputs were less than sequence expected";
                            Logging.Log([mes]);
                            return false; // not enough inputs
                        }
                        if (inputs.Length > all)
                        {
                            Logging.Log(["More inputs than expected, exiting sequence"]);
                            return false; // too many inputs
                        }

                        string[]? results;
                        if (Helpers.BackendHelping.ExtractStringGroups(sequence, "{", "}", out results))
                        {
                            if (results == null)
                            {
                                Logging.Log(["[CRITICAL ERROR] Metadata grouping resulted null, command cannot be processed. You'll have to make a bug report (attach this log) so this can be fixed in the code behind, apologies for the inconvenience."]);
                                error_message = "Metadata resolve error, please see logs for details.";
                                return false; // critical error
                            }

                            List<object> flexparams = new(flex), fixparams = new(fix);
                            for (int i = 0; i < results.Length; i++)
                            {
                                if (i > inputs.Length - 1) break; // break if more placeholders than inputs

                                string[] types = results[i].Split('/');
                                Logging.Log(["Types: ", types]);
                                bool isValid = false;

                                foreach (string type in types)
                                {
                                    switch (type)
                                    {
                                        case "int":
                                            if (int.TryParse(inputs[i], out int result))
                                            {
                                                Logging.Log([$"Successfully cast input #{i}, {inputs[i]} to int."]);
                                                isValid = true;
                                                if (all - i + 1 < flex)
                                                    flexparams.Add(result); // add to flexparams
                                                else
                                                    fixparams.Add(result); // add to fixparams
                                            }
                                            else Logging.Log([$"Failed to cast input #{i}, {inputs[i]} to int."]);
                                            break;

                                        case "bool":
                                            if (bool.TryParse(inputs[i], out bool bresult))
                                            {
                                                Logging.Log([$"Successfully cast input #{i}, {inputs[i]} to bool."]);
                                                isValid = true;
                                                if (all - (i + 1) < flex)
                                                    flexparams.Add(bresult); // add to flexparams
                                                else
                                                    fixparams.Add(bresult); // add to fixparams
                                            }
                                            else Logging.Log([$"Failed to cast input #{i}, {inputs[i]} to bool."]);
                                            break;

                                        case "string":
                                            isValid = true;
                                            if (all - (i + 1) < flex)
                                                flexparams.Add(inputs[i].Trim().ToLower()); // add to flexparams
                                            else
                                                fixparams.Add(inputs[i].Trim().ToLower()); // add to fixparams
                                            break;
                                    }
                                    if (isValid) break; // exit loop if valid
                                }
                                if (!isValid)
                                {
                                    Logging.Log([$"Expected {results[i]}, not whatever was inputted."]);
                                    return false; // invalid input
                                }
                            }
                            parsedparams = [fixparams.ToArray(), flexparams.ToArray()]; // set parsed params
                            Logging.Log(["Parsed Params object:", parsedparams]);
                            return true; // successful parse
                        }
                        else
                        {
                            Logging.Log(["[PARSE ERROR] Failed to extract string groupings! This shouldn't happen... please send a bug report and attach this log, thanks!"]);
                            return false; // extraction error
                        }
                    }

                }
            }

            /// <summary>
            /// A custom ListBox control designed to display log messages within STELLA. It supports virtualization for performance optimization with large numbers of log entries.
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

                    KeyDown += (s, e) =>
                    {
                        switch (e.Key)
                        {
                            case Key.C:
                                string? text = SelectedItems?.Cast<string>()?.Order().Select(x => x + "\n")?.ToString();
                                text ??= "";
                                System.Windows.Clipboard.SetText(text);
                                UnselectAll();
                                break;
                            case Key.Escape:
                                UnselectAll();
                                break;
                        }
                    };
                    SelectionMode = SWC.SelectionMode.Extended;

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