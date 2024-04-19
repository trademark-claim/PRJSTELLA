#define A
//#define B
//#define C

using Microsoft.VisualBasic.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Speech.Recognition;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace Cat
{
    /// <summary>
    /// Contains classes for creating UI objects like shutdown screens and speech bubbles.
    /// </summary>
    internal static class Objects
    {
        /// <summary>
        /// OLD shutdown screen test that didn't really work too well. For later, I suppose.
        /// </summary>
        /// <remarks>
        /// The windows shutdown circle is an accessible font, use it!
        /// </remarks>
        [Obsolete("Incomplete, discontinued / on hold until further notice", true)]
        internal class ShutDownScreen : Canvas
        {
            private static ShutDownScreen inst;

            /// <summary>
            /// Toggles the shutdown screen on or off.
            /// </summary>
            /// <param name="canv">The canvas to add or remove the shutdown screen from.</param>
            /// <returns>The instance of the shutdown screen.</returns>
            internal static ShutDownScreen ToggleScreen(Canvas canv)
            { if (inst != null) { canv.Children.Remove(inst); inst = null; return inst; } else { inst = new ShutDownScreen(); canv.Children.Add(inst); return inst; } }

            private ShutDownScreen()
            {
                inst = this;
                Children.Add(new System.Windows.Shapes.Rectangle() { Width = SystemParameters.PrimaryScreenWidth, Height = SystemParameters.PrimaryScreenHeight });
                SetTop<double>(this, 0);
                SetLeft<double>(this, 0);
            }
        }

        /// <summary>
        /// Creates an overlay for the current screen at the user's set opacity but set grey colour.
        /// </summary>
        /// <remarks>
        /// Need to merge this with the interface's back ground for continuity
        /// </remarks>
        internal static class OverlayRect
        {
            private static Rectangle MakeOverlay()
                => new Rectangle { Width = Catowo.GetScreen().Bounds.Width, Height = Catowo.GetScreen().Bounds.Height, Fill = new SolidColorBrush(Colors.Gray), Opacity = UserData.Opacity };


            /// <summary>
            /// Adds <see cref="Rectangle"/> to <paramref name="c"/>
            /// </summary>
            /// <param name="c">The canvas to add to</param>
            internal static Rectangle AddToCanvas(Canvas c)
            {
                var r = MakeOverlay();
                c.Children.Add(r);
                return r;
            }

            internal static Rectangle AddToCanvas(System.Drawing.Rectangle rect, Canvas c)
            {
                Rectangle Rectangle = MakeOverlay();
                Rectangle.Width = rect.Width;
                Rectangle.Height = rect.Height;
                c.Children.Add(Rectangle);
                return Rectangle;
            }

            internal static Rectangle AddToCanvas(Rect rect, Canvas c)
            {
                Rectangle Rectangle = MakeOverlay();
                Rectangle.Width = rect.Width;
                Rectangle.Height = rect.Height;
                c.Children.Add(Rectangle);
                return Rectangle;
            }

            /// <summary>
            /// Does the opposite of <see cref="AddToCanvas(Canvas)"/>
            /// </summary>
            /// <param name="c">The canvas in which to remove from</param>
            internal static void RemoveFromCanvas(Canvas c, Rectangle rect)
                => c.Children.Remove(rect);
        }

        /// <summary>
        /// Static class for dealing with direct Clara interactions, such as speech bubbles and images / animations (if we can get it)
        /// </summary>
        /// <remarks>
        /// Need to have back progression (using left arrow)
        /// </remarks>
        internal static class ClaraHerself
        {
            /// <summary>
            /// Used with the speech bubble arrays to keep track of which array item we're on.
            /// </summary>
            private static byte num = 0;

            /// <summary>
            /// Holds the introduction text
            /// </summary>
            private static readonly string[] Introduction = [
                "Hey! It's me, Clara! (Made by Nexus) \nIt seems this is the first time you've opened me (or I've been updated).\nIf you want to skip this, press the up arrow. \nIf you want to view the changelog, press the down arrow (not working)'\nIf you want to run through the introduction, just press the right arrow key!",
                "So you wanna do the introduction again... sweet!\nI'm Clara, the Centralised, Logistical, Administrative and Requisition Assistant. \nMy sole purpose is to automate, optimize and otherwise improve your computer experience.\n You can press the left arrow key to move through parts.",
                "There are two (at the moment) main modes to this program: Background and Interface.\nInterface is where there's an overlay with a textbox and an output box, where you can enter commands.\n   Key shortcuts won't work here, but this is where most of the functionality is.\nBackground is where there... is no main overlay (you're currently in background mode!).\n   This is what the app will be in 99% of the time.",
                "To open the interface:\n  Hold both shifts (both th left and right one),\n  Then press and hold Q,\n  then press I!\n  (LShift + RShift + Q + I). \n To close the interface run the 'close' command.\nTo view the help page, run 'help'",
                "This program is in a pre-pre-pre-pre-alpha stage, and there will be bugs and stuff.\nYou can send logs to me (Discord: _dissociation_) (Gmail: brainjuice.work23@gmail.com) with bug reports and feedback and stuff. Enjoy!",
                "Hmmm.. is there anything else..?\nOh right! Local data is stored at C:\\ProgramData\\Kitty\\Cat\\\nHave fun, I hope you enjoy this app! o/"
            ];

            /// <summary>
            /// Whichever array is currently being spoken
            /// </summary>
            private static string[] CurrentStory = [];

            /// <summary>
            /// The bubble object being shown
            /// </summary>
            private static SpeechBubble? bubble;

            /// <summary>
            /// The canvas object we're attached to
            /// </summary>
            private static Canvas? canvas;

            /// <summary>
            /// The overlay rectangle
            /// </summary>
            private static Rectangle overlay;

            /// <summary>
            /// However lonely you feel, you're never alone. There are literally millions of bugs, mites and bacteria living in your house. Goodnight.
            /// </summary>
            /// <param name="parameters"></param>
            /// <returns></returns>
            private delegate object? UniversalDelegate();

            private static UniversalDelegate delegation = null;//(args) => { _ = "Lorum Ipsum"; return null; };

            /// <summary>
            /// Needs to reflect what arrays we have
            /// </summary>
            internal enum Mode : byte
            {
                Introduction
            }

            /// <summary>
            /// Cancer cures smoking.
            /// </summary>
            /// <param name="mode">Which array to run through</param>
            /// <param name="canvas">The canvas</param>
            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            internal static void RunClara(Mode mode, Canvas canvas)
            {
                ClaraHerself.canvas = canvas;
                overlay = OverlayRect.AddToCanvas(canvas);
                Catowo.inst.MakeNormalWindow();
                switch (mode)
                {
                    case Mode.Introduction:
                        CurrentStory = Introduction;
                        break;
                }
                // The first message
                bubble = new();
                Point location = new(Catowo.inst.Width - 30, Catowo.inst.Height - 30);
                Logging.Log("Location", location);
                bubble.LowerRightCornerFreeze = location;
                bubble.Text = CurrentStory[num];
                canvas.Children.Add(bubble);
                Catowo.inst.PreviewKeyDown += ProgressionKeydown;

            }

            /// <summary>
            /// Event handler for the pressing of keys while a speech is activated. This is the base method to be used in tangent with specific methods for each set of speech, and is to be loading in and out as needed.
            /// </summary>
            /// <param name="sender">The caller of the event</param>
            /// <param name="e">The key event args</param>
            private static void ProgressionKeydown(object sender, System.Windows.Input.KeyEventArgs e)
            {
                if (e.Key == Key.Right)
                    if (canvas != null)
                    {
                        if (++num > CurrentStory.Length - 1)
                        {
                            num = 0;
                            Catowo.inst.MakeFunnyWindow();
                            Catowo.inst.PreviewKeyDown -= ProgressionKeydown;
                            OverlayRect.RemoveFromCanvas(canvas, overlay);
                            if (bubble != null)
                            {
                                canvas.Children.Remove(bubble);
                                bubble = null;
                            }
                            return;
                        }
                        if (bubble != null)
                            bubble.Text = CurrentStory[num];
                        return;
                    }
                if (e.Key == Key.Left)
                    if (canvas != null)
                    {
                        num--;
                        if (num >= 0)
                            if (bubble != null)
                                bubble.Text = CurrentStory[num];
                        return;
                    }
                if (e.Key == Key.Up)
                    if (canvas != null)
                    {
                        num = 0;
                        Catowo.inst.MakeFunnyWindow();
                        Catowo.inst.PreviewKeyDown -= ProgressionKeydown;
                        OverlayRect.RemoveFromCanvas(canvas, overlay);
                        if (bubble != null)
                        {
                            canvas.Children.Remove(bubble);
                            bubble = null;
                        }
                        return;
                    }
                if (e.Key == Key.Down)
                    if (delegation != null)
                        delegation();

            }

            /// <summary>
            /// Represents a speech bubble UI element.
            /// </summary>
            private class SpeechBubble : Canvas
            {
                /// <summary>
                /// The text displayed
                /// </summary>
                private readonly TextBlock textBlock;
                /// <summary>
                /// The bubble part
                /// </summary>
                private readonly Rectangle rectangle;
                /// <summary>
                /// The arrow part that really completes the bubble
                /// </summary>
                private readonly Polygon tail;
                /// <summary>
                /// Controls padding
                /// </summary>
                private const float Control = 5.0F;
                /// <summary>
                /// Fixed position for the lower right corner
                /// </summary>
                internal Point LowerRightCornerFreeze = new(1000, 500);

                /// <summary>
                /// Abstraction Property
                /// </summary>
                public string Text
                {
                    get => textBlock.Text;
                    set
                    {
                        textBlock.Text = value;
                        UpdateLayout();
                    }
                }

                /// <summary>
                /// Abstraction Property
                /// </summary>
                public double FontSize
                {
                    get => textBlock.FontSize;
                    set => textBlock.FontSize = value;
                }

                /// <summary>
                /// Abstraction Property
                /// </summary>
                public double BubbleOpacity
                {
                    get => Opacity;
                    set => Opacity = value;
                }

                /// <summary>
                /// Abstraction Property
                /// </summary>
                public Brush BubbleColor
                {
                    get => rectangle.Fill;
                    set
                    {
                        rectangle.Fill = value;
                        tail.Fill = value;
                    }
                }

                /// <summary>
                /// Abstraction Property
                /// </summary>
                public Thickness TextPadding
                {
                    get => textBlock.Margin;
                    set => textBlock.Margin = value;
                }

                /// <summary>
                /// Constructor
                /// </summary>
                public SpeechBubble()
                {
                    rectangle = new()
                    {
                        RadiusX = 10,
                        RadiusY = 10,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Fill = new SolidColorBrush(Colors.White),
                    };

                    tail = new Polygon
                    {
                        Points = new PointCollection(new[] { new Point(0, 0), new Point(15, 0), new Point(7.5, 20) }),
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Fill = new SolidColorBrush(Colors.White)
                    };

                    textBlock = new TextBlock
                    {
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(Control)
                    };

                    TextPadding = new Thickness(Control);
                    Children.Add(rectangle);
                    Children.Add(tail);
                    Children.Add(textBlock);
                    FontSize = 20.0f;//UserData.FontSize;

                    SizeChanged += (s, e) => UpdateLayout();
                }

                public void UpdateLayout()
                {
                    textBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

                    double textWidth = textBlock.DesiredSize.Width + TextPadding.Left + TextPadding.Right;
                    double textHeight = textBlock.DesiredSize.Height + TextPadding.Top + TextPadding.Bottom;

                    rectangle.Width = textWidth + (Control * 2);
                    rectangle.Height = textHeight + (Control * 2) + 20;
                    SetLeft<double>(textBlock, TextPadding.Left + Control);
                    SetTop<double>(textBlock, TextPadding.Top + Control);
                    SetLeft<double>(tail, rectangle.Width - 10);
                    SetTop<double>(tail, rectangle.Height);
                    double left = LowerRightCornerFreeze.X - rectangle.Width;
                    double top = LowerRightCornerFreeze.Y - rectangle.Height;
                    SetLeft<double>(this, left);
                    SetTop<double>(this, top);
                    Width = rectangle.Width;
                    Height = rectangle.Height;
                }
            }

        }

        internal record class Memento<T>(T Store);

        internal static class CursorEffects
        {
            private static bool isOn = false;
            private static DispatcherTimer fadeTimer;
            private static Window allencompassing;
            private delegate void CursorTrailDelegate(in Point mousepos);
            private static dynamic Memento;
            private static CursorTrailDelegate _method;
            private static Canvas canvas;

            private static LowLevelProc _proc = HookCallback;
            private static nint _hookID = nint.Zero;

            internal static void Toggle()
            {
                if (!isOn) Run();
                else if (isOn) Stop();
                isOn = !isOn;
            }

            private static void Run()
            {
                if (isOn) return;
                int left = Screen.AllScreens.Min(screen => screen.Bounds.Left);
                int top = Screen.AllScreens.Min(screen => screen.Bounds.Top);
                int right = Screen.AllScreens.Max(screen => screen.Bounds.Right);
                int bottom = Screen.AllScreens.Max(screen => screen.Bounds.Bottom);
                int width = right - left;
                int height = bottom - top;
                canvas = new() { IsHitTestVisible = false };
                allencompassing = new Window
                {
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Left = left,
                    Top = top,
                    Width = width,
                    Height = height,
                    Background = Brushes.Transparent,
                    Content = canvas,
                    Topmost = true,
                    ShowInTaskbar = false,
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true
                };
                allencompassing.Show();
                allencompassing.Loaded += (sender, e) =>
                {
                    var hwnd = new WindowInteropHelper(allencompassing).Handle;
                    var originalStyle = GetWindowLongWrapper(hwnd, GWL_EXSTYLE);
                    SetWindowLongWrapper(hwnd, GWL_EXSTYLE, originalStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
                    var editedstyle = GetWindowLongWrapper(hwnd, GWL_EXSTYLE);
                    Logging.Log($"Set Win Style of Handle {hwnd} from {originalStyle:X} ({originalStyle:B}) [{originalStyle}] to {editedstyle:X} ({editedstyle:B}) [{editedstyle}]");
                };
                Particles.LineTrail.Init();
                isOn = true;
            }

            private static IntPtr SetHook(LowLevelProc proc)
            {
                using (System.Diagnostics.Process curProcess = System.Diagnostics.Process.GetCurrentProcess())
                using (System.Diagnostics.ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookExWrapper(WH_MOUSE_LL, proc,
                        GetModuleHandleWrapper(curModule.ModuleName), 0);
                }
            }

            private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0)
                {
                    MOUSEINPUT hookStruct = Marshal.PtrToStructure<MOUSEINPUT>(lParam);
                    switch ((MouseMessages)wParam)
                    {
                        case MouseMessages.WM_MOUSEMOVE:
                            Point p = allencompassing.PointFromScreen(new(hookStruct.pt.X, hookStruct.pt.Y));
                            p.Offset(5, 5);
                            _method(p);
                            break;
                        case MouseMessages.WM_LBUTTONDOWN:
                            Particles.RectangleClick.Trigger.Activate(canvas, allencompassing.PointFromScreen(new(hookStruct.pt.X, hookStruct.pt.Y)));
                            break;
                        case MouseMessages.WM_RBUTTONDOWN:
                            break;
                    }
                }
                return CallNextHookExWrapper(_hookID, nCode, wParam, lParam);
            }

            private static void Stop()
            {
                if (!isOn) return;
            }

            internal static void MoveTop()
            {
                if (allencompassing != null)
                    allencompassing.Topmost = true;
            }

            private static class Particles
            {
                internal static class RectangleTrail
                {
                    private class Effect
                    {
                        internal Rectangle Rect = new() { Fill = Brushes.Pink, Width = 5.0, Height = 5.0, Opacity = 1.0, IsHitTestVisible = false };
                        internal double OpacityDecrement = 0.05;
                    }

                    private static Queue<Effect> effectsQueue = new();

                    internal static void SetUpRectangles()
                    {
                        Memento = new List<Effect>(200);
                        for (int i = 0; i < 200; i++)
                        {
                            Effect effect = new Effect();
                            canvas.Children.Add(effect.Rect);
                            Memento.Add(effect);
                            effectsQueue.Enqueue(effect);
                        }
                        _method = RectangleTick;
                    }

                    internal static void RectangleTick(in Point point)
                    {

                        foreach (Effect effect in Memento)
                        {
                            if (effect.Rect.Opacity > 0)
                            {
                                effect.Rect.Opacity = Math.Max(0, effect.Rect.Opacity - effect.OpacityDecrement);
                            }
                        }

                        if (effectsQueue.Peek().Rect.Opacity == 0)
                        {
                            Effect effect = effectsQueue.Dequeue(); 
                            Canvas.SetTop(effect.Rect, point.Y - effect.Rect.Height / 2);
                            Canvas.SetLeft(effect.Rect, point.X - effect.Rect.Width / 2);
                            effect.Rect.Opacity = 1.0; 
                            effectsQueue.Enqueue(effect);
                        }
                    }
                }

                internal static class LineTrail
                {
                    internal static void Init()
                    {
                        Memento = new DynamicLineDrawer(canvas);
                        _method = (in Point p) => (Memento as DynamicLineDrawer).AddPoint(p);
                        fadeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(15) };
                        _hookID = SetHook(_proc);
                        fadeTimer.Tick += (s, e) => (Memento as DynamicLineDrawer).RemoveOldest();
                        fadeTimer.Start();
                    }
                }

                internal static class BiLineTrail
                {
                    internal static void Init()
                    {
                        Memento = new Tuple<DynamicLineDrawer, DynamicLineDrawer>(new DynamicLineDrawer(canvas, 9, new SolidColorBrush(Colors.White)), new DynamicLineDrawer(canvas, 4, new SolidColorBrush(Colors.Pink)));
                        _method = (in Point p) => { var (m1, m2) = (Memento as Tuple<DynamicLineDrawer, DynamicLineDrawer>); m1.AddPoint(p); m2.AddPoint(p); };
                        fadeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(15) };
                        _hookID = SetHook(_proc);
                        fadeTimer.Tick += (s, e) => { var (m1, m2) = (Memento as Tuple<DynamicLineDrawer, DynamicLineDrawer>); m1.RemoveOldest(); m2.RemoveOldest(); };
                        fadeTimer.Start();
                    }
                }

                internal static class RectangleClick
                {
                    private const double gravity = 0.05;

                    internal class Effect
                    {
                        internal readonly Rectangle rect;
                        private double speedX;
                        private double speedY;
                        private double opacitySpeed = 0.01; // Adjust for faster/slower fade
                        private Canvas canvas;
                        private DateTime lastUpdate = DateTime.Now;

                        internal Effect(Canvas canvas, Point p)
                        {
                            this.canvas = canvas;
                            rect = new Rectangle
                            {
                                Fill = new SolidColorBrush(Color.FromArgb(255, (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256))),
                                Width = 5,
                                Height = 5,
                                Opacity = 1
                            };

                            Canvas.SetLeft(rect, p.X - rect.Width / 2);
                            Canvas.SetTop(rect, p.Y - rect.Height / 2);

                            // Random speed
                            double speed = random.NextDouble() * 2 + 1; // Speed range [1, 3)

                            // More bias towards bottom 180 degrees
                            double angle = random.NextDouble() * 360; // Full 360 degrees

                            angle *= Math.PI / 180; // Convert to radians

                            speedX = speed * Math.Cos(angle);
                            speedY = speed * Math.Sin(angle);

                            canvas.Children.Add(rect);
                        }

                        internal bool Tick()
                        {
                            var now = DateTime.Now;
                            var elapsedTime = (now - lastUpdate).TotalSeconds;
                            lastUpdate = now;

                            Canvas.SetLeft(rect, Canvas.GetLeft(rect) + speedX * elapsedTime * 60);
                            Canvas.SetTop(rect, Canvas.GetTop(rect) + speedY * elapsedTime * 60);

                            // Apply gravity to Y speed
                            speedY += gravity * elapsedTime * 60; // Adjust gravity effect here if needed

                            // Fade out effect
                            rect.Opacity -= opacitySpeed * elapsedTime * 60; ;
                            if (rect.Opacity <= 0)
                            {
                                canvas.Children.Remove(rect);
                                return false; // Effect finished
                            }
                            return true; // Effect continues
                        }
                    }


                    internal static class Trigger
                    {
                        private static readonly List<Effect> effects = new List<Effect>();
                        private static Canvas canvas;

                        internal static void Activate(Canvas targetCanvas, Point p)
                        {
                            canvas = targetCanvas;

                            // Adding several effects at the click point
                            int numberOfEffects = random.Next(10, 51); // Or another number based on your preference
                            for (int i = 0; i < numberOfEffects; i++)
                            {
                                effects.Add(new Effect(canvas, p));
                            }

                            if (!isRenderingSubscribed)
                            {
                                CompositionTarget.Rendering += Update;
                                isRenderingSubscribed = true;
                            }
                        }

                        private static bool isRenderingSubscribed = false;

                        private static void Update(object sender, EventArgs e)
                        {
                            for (int i = effects.Count - 1; i >= 0; i--)
                            {
                                if (!effects[i].Tick())
                                {
                                    effects.RemoveAt(i); // Remove finished effects
                                }
                            }

                            // If no more effects to update, unsubscribe to stop calling Update
                            if (effects.Count == 0 && isRenderingSubscribed)
                            {
                                CompositionTarget.Rendering -= Update;
                                isRenderingSubscribed = false;
                            }
                        }
                    }

                }

            }

        }

        internal readonly record struct Command(string Call, string Raw, object[][]? Parameters = null);

        internal class DynamicLineDrawer
        {
            private Polyline polyline;
            private Point previous = new(-999999, -99999);
            private bool oncedown = false;

            public DynamicLineDrawer(Canvas canvas)
            {
                if (canvas == null) throw new ArgumentNullException(nameof(canvas));

                // Initialize the Polyline and add it to the canvas
                polyline = new Polyline
                {
                    Stroke = dyingrainbow,
                    StrokeThickness = 2,
                };
                canvas.Children.Add(polyline);
            }

            public DynamicLineDrawer(Canvas canvas, double thickness, Brush brush)
            {
                if (canvas == null) throw new ArgumentNullException(nameof(canvas));

                // Initialize the Polyline and add it to the canvas
                polyline = new Polyline
                {
                    Stroke = brush,
                    StrokeThickness = thickness
                };
                canvas.Children.Add(polyline);
            }


            // Method to add a point
            public void AddPoint(Point point)
            {
                if (Helpers.BackendHelping.IsPointWithinOtherPointForSmoothing(point, previous, 5)) return;
                if (Helpers.BackendHelping.IsPointWithinOtherPointForSmoothing(point, previous, 15))
                {
                    oncedown = !oncedown;
                    if(oncedown)
                        return;
                }

                previous = point;
                polyline.Points.Add(point);
                if (polyline.Points.Count > 25)
                    RemoveOldest();
            }

            // Method to remove a point
            public void RemovePoint(Point point)
            {
                polyline.Points.Remove(point);
            }

            public void RemoveOldest()
            {
                if (polyline.Points.Count > 0)
                {
                    polyline.Points.RemoveAt(0);
                }
            }

        }

        internal class LogEditor : Window // Code the GUI here
        {
            private LoggingListBox logListBox;
            private Grid mainGrid; // Content Container
            internal static LogEditor inst = null;
            private MenuBox menu;
            internal int currentExceptionIndex = -1;
            private SWC.ListBox files = new();

            [LoggingAspects.Logging]
            internal LogEditor() // Runs upon object creation
            {
                Topmost = true;

                var screen = Catowo.GetScreen();
                Width = screen.Bounds.Width;
                Height = screen.Bounds.Height;
                this.Background = Brushes.Purple; 
                inst?.Close();
                inst = this;
                InitializeComponents();
                LoadLogs();
            }

            [LoggingAspects.Logging]
            private void InitializeComponents()
            {
                mainGrid = new Grid();
                Content = mainGrid;
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) }); 
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });

                logListBox = new LoggingListBox();
                mainGrid.Children.Add(logListBox);

                menu = new MenuBox(logListBox);
                mainGrid.Children.Add(menu);

                files.SelectionChanged += (s, e) => { if (files.SelectedItem is string str && str.StartsWith('L') && str.EndsWith(".LOG")) logListBox.LoadFile(LogFolder + "//" + str); menu.InitializeMenu(); };
                mainGrid.Children.Add(files);

                Grid.SetColumn(files, 0);
                Grid.SetColumn(logListBox, 1);
                Grid.SetColumn(menu, 2);

                foreach (var file in Directory.EnumerateFiles(LogFolder))
                    files.Items.Add(file.Replace(LogFolder, ""));
            }

            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            private async Task LoadLogs()
            {
                await Logging.FullFlush();
                var files = Directory.EnumerateFiles(LogFolder).ToArray();
                if (files.Length == 0) return;
                logListBox.LoadFile(files[0]);
                return;
            }
           



            private class MenuBox : SWC.ListBox
            {
                private static int searchamount = 0;
                private const int FirstSearch = 8;
                internal static MenuBox minst;

                private static readonly List<(string, Action)> a_buttons = new List<(string, Action)>()
                {
                    ("Sort By", null),
                    ("Severity", (Action)(() =>
                    {
                        var epic = new List<List<string>>()
                        {
                            (new()), // Fatals
                            (new()), // Exceptions
                            (new()), // Other
                            (new()), // The rest
                        };
                        bool isErrorLog = false;
                        foreach(string log in inst.logListBox.Items)
                        {
                            if (log.Contains("FATAL ERROR"))
                                epic[0].Add(log);
                            else if (log.Contains(">>>ERROR"))
                            {
                                epic[1].Add(log);
                                isErrorLog = true;
                            }
                            else if (log.Contains(">>>END OF ERROR") && isErrorLog)
                            {
                                epic[1].Add(log);
                                isErrorLog = false;
                            }
                            else if (log.Contains("ERROR") && !isErrorLog)
                                epic[2].Add(log);
                            else if (isErrorLog)
                                epic[1].Add(log);
                            else epic[3].Add(log);
                        }
                        inst.logListBox.Items.Clear();
                        if (epic[0].Count > 0)
                            epic[0].Add("\n");
                        if (epic[1].Count > 0)
                            epic[1].Add("\n");
                        if (epic[2].Count > 0)
                            epic[2].Add("\n");
                        foreach (List<string> item in epic)
                            foreach (string log in item)
                                inst.logListBox.Items.Add(log);
                        epic = null;
                    })),
                    ("Date", (() => 
                    {
                        var logs = LogEditor.inst.logListBox.Items.Cast<string>().ToList();
                        logs = logs.OrderBy(line => Helpers.BackendHelping.ExtractStringGroups(line, "[", "]", out string[]? results)).ToList();
                        logs.RemoveAll(x => string.IsNullOrWhiteSpace(x));
                        LogEditor.inst.logListBox.Items.Clear();
                        foreach (var log in logs)
                        {
                            LogEditor.inst.logListBox.Items.Add(log);
                        }
                    })),
                    ("Execution Time", () => { MessageBox.Show("This feature is still in development!"); }),
                    ("Alphabetical", () => { var logs = inst.logListBox.Items.Cast<string>().OrderBy(item => { int index = item.IndexOf(']') + 1; while (index < item.Length && !char.IsLetterOrDigit(item[index])) { index++; } return index < item.Length ? item.Substring(index) : ""; }).ToList(); inst.logListBox.Items.Clear(); foreach (string item in logs) inst.logListBox.Items.Add(item); }),
                    ("Set Filter To", null),
                    ("Errors", () => 
                    {
                        var epic = new List<List<string>>()
                        {
                            (new()), // Fatals
                            (new()), // Exceptions
                            (new()), // Other
                        };
                        bool isErrorLog = false;
                        foreach(string log in inst.logListBox.Items)
                        {
                            if (log.Contains("FATAL ERROR"))
                                epic[0].Add(log);
                            else if (log.Contains(">>>ERROR"))
                            {
                                epic[1].Add(log);
                                isErrorLog = true;
                            }
                            else if (log.Contains(">>>END OF ERROR") && isErrorLog)
                            {
                                epic[1].Add(log);
                                isErrorLog = false;
                            }
                            else if (log.Contains("ERROR") && !isErrorLog)
                                epic[2].Add(log);
                            else if (isErrorLog)
                                epic[1].Add(log);
                        }
                        inst.logListBox.Items.Clear();
                        if (epic[0].Count > 0)
                            epic[0].Add("\n");
                        if (epic[1].Count > 0)
                            epic[1].Add("\n");
                        if (epic[2].Count > 0)
                            epic[2].Add("\n");
                        foreach (List<string> item in epic)
                            foreach (string log in item)
                                inst.logListBox.Items.Add(log);
                        epic = null;
                    }),
                    ("Function E&E", () => 
                    {
                        var logs = inst.logListBox.Items.Cast<string>().ToList().Where(x => x.Contains("method", StringComparison.InvariantCultureIgnoreCase) && (x.Contains("entering", StringComparison.InvariantCultureIgnoreCase) || x.Contains("exiting", StringComparison.InvariantCultureIgnoreCase)));
                        inst.logListBox.Items.Clear();
                        foreach (var item in logs)
                            inst.logListBox.Items.Add(item);
                    }),
                    ("Search", (Action)(() => 
                    {
                        var x = new OverlayInputBox("What phrase would you like to filter by:", LogEditor.inst);
                        x.ShowDialog();
                        var logs = inst.logListBox.Items.Cast<string>().ToList().Where(x => x.Contains(OverlayInputBox.Input, StringComparison.InvariantCultureIgnoreCase));
                        inst.logListBox.Items.Clear();
                        foreach (var item in logs)
                            inst.logListBox.Items.Add(item);
                        ++searchamount;
                        minst.buttons.Insert(searchamount + FirstSearch, ($"Search {searchamount}", (Action)(() =>
                        {
                            minst.CompoundingSearch();
                        })));
                        minst.InitializeMenu();
                    })),
                    ("Misc", null),
                    ("Search", () => { MessageBox.Show("This feature is still in development!"); }),
                    ("Nest", () => { MessageBox.Show("This feature is still in development!"); }),
                    ("Reset", () => { inst.logListBox.Items.Clear(); foreach (var item in inst.logListBox.baselines) inst.logListBox.Items.Add(item); })
                };

                private readonly List<(string, Action)> buttons = new List<(string, Action)>();

                private LoggingListBox logListBox;

                internal MenuBox(LoggingListBox logListBox)
                {
                    minst = this;
                    this.logListBox = logListBox;
                    buttons.Clear();
                    foreach (var item in a_buttons)
                        buttons.Add(item);
                    InitializeMenu();
                }

                private void CompoundingSearch()
                {
                    var x = new OverlayInputBox("What phrase would you like to add another filter for:", LogEditor.inst);
                    x.ShowDialog();
                    if (string.IsNullOrWhiteSpace(OverlayInputBox.Input))
                        return;
                    var current = inst.logListBox.Items.Cast<string>().ToList();
                    var logs = inst.logListBox.baselines.Where(x => x.Contains(OverlayInputBox.Input, StringComparison.InvariantCultureIgnoreCase) || current.Contains(x));
                    inst.logListBox.Items.Clear();
                    foreach (var item in logs)
                        inst.logListBox.Items.Add(item);
                    ++searchamount;
                    minst.buttons.Insert(searchamount + FirstSearch, ($"Search {searchamount}", (Action)(() =>
                    {
                        CompoundingSearch();
                    })));
                    InitializeMenu();
                }

                internal void InitializeMenu()
                {
                    searchamount = 0;
                    Items.Clear();
                    foreach ((string name, Action act) in buttons)
                        Items.Add(new ExecutableText<Action>(act) { Text = (act == null? "" : "  ") + name });
                    SelectionChanged += (s, e) => { if (SelectedItem is ExecutableText<Action> act) act.Execute(); UnselectAll(); };
                    SelectionMode = SWC.SelectionMode.Single;
                }
            }

            private class LoggingListBox : Catowo.Interface.LogListBox
            {
                internal List<string> baselines = new List<string>();

                public LoggingListBox()
                {
                    ScrollViewer.SetVerticalScrollBarVisibility(this, ScrollBarVisibility.Auto);
                    ScrollViewer.SetHorizontalScrollBarVisibility(this, ScrollBarVisibility.Auto);
                    Height = LogEditor.inst.Height;
                    var screen = Catowo.GetScreen();
                    Width = screen.Bounds.Width - 400;
                }

                internal void LoadFile(string path)
                {
                    Items.Clear();
                    baselines = File.ReadAllLines(path).ToList();
                    baselines.RemoveAll(string.IsNullOrWhiteSpace);
                    foreach (string line in baselines)
                    {
                        AddItem(line);
                    }
                }
            }

        }

        internal class OverlayInputBox : Window
        {
            internal static string Input { get; private set; }
            private nint hwnd;
            private Rectangle rect;

            internal OverlayInputBox(string question, Window parent)
            {
                Screen screen = Helpers.BackendHelping.GetContainingScreen(parent);
                Helpers.ScreenSizing.GetAdjustedScreenSize(screen, out Rect nb);
                Width = nb.Width;
                WindowStyle = WindowStyle.None;
                AllowsTransparency = true;
                Background = Brushes.Transparent;
                Height = nb.Height;
                Top = nb.Top;
                ShowInTaskbar = false;
                ShowActivated = true;
                //WindowState = WindowState.Maximized;   
                Left = nb.Left;
                Loaded += (sender, e) =>
                {
                    hwnd = new WindowInteropHelper(this).Handle;
                    Topmost = true;
                    int originalStyle = GetWindowLongWrapper(hwnd, GWL_EXSTYLE);
                    SetWindowLongWrapper(hwnd, GWL_EXSTYLE, originalStyle | WS_EX_LAYERED | WS_EX_TOOLWINDOW);
                };
                Init(question, nb);
            }

            private void Init(string question, Rect nb)
            {
                double width = nb.Width;
                double height = nb.Height;
                Canvas canv = new();
                Content = canv;
                rect = OverlayRect.AddToCanvas(nb, canv);
                double margin = width / 6;
                TextBox tb = new() { Width = width - (margin * 2), Height = 20 };
                double aheight = (height / 2) - 10;
                Canvas.SetTop(tb, aheight);
                Canvas.SetLeft(tb, margin);
                PreviewKeyDown += (s, e) =>
                {
                    if (e.Key == System.Windows.Input.Key.Enter)
                    {
                        Input = tb.Text;
                        Close();
                    }
                };
                canv.Children.Add(tb);
                var lbl = new Label() { Content = question, Foreground = new SolidColorBrush(Colors.Silver)};
                Canvas.SetLeft(lbl, margin);
                Canvas.SetTop(lbl, aheight - 20);
                canv.Children.Add(lbl);
            }
        }


        public class ExecutableText<T> : TextBlock
        {
            private Action function;

            public ExecutableText(Action action)
                => function = action;

            internal void Execute()
            {
                function?.Invoke();
            }
        }

        public class NullToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value == null ? Visibility.Collapsed : Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public static class VoiceCommandHandler
        {
            private static SpeechRecognitionEngine recognizer;
            private static bool ready = false;
            internal static bool WasCalled = false;
            internal static Dictionary<string, string> Speechrecogmap { get; private set; }
            
            [LoggingAspects.Logging]
            public static void ActivateVoiceCommandHandler()
            {
                ready = true;
                if (Speechrecogmap == null)
                {
                    Speechrecogmap = Helpers.JSONManager.ReadFromJsonFile<Dictionary<string, string>>("speechrecogmap.json");
                }
                recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));
                Logging.Log("Engine created");
                Logging.Log("Loading Grammer...");
                recognizer.LoadGrammar(new DictationGrammar());
                Logging.Log("Created Grammer.");
                recognizer.SpeechRecognized += (s, e) =>
                {
                    var result = e.Result;
                    Logging.Log("Transcription: " + result.Text);
                    Commands.ProcessVoiceCommand(result.Text);
                };
#if A
                recognizer.SpeechRecognitionRejected += (s, e) =>
                {
                    Logging.Log("Speech recognition failed.");
                    Interface.AddLog("Speech recognition failed.");
                };
#endif
#if B
                recognizer.RecognizeCompleted += (s, e) =>
                {
                    Logging.Log("Recognition completed.");
                };
#endif
#if C
                recognizer.SpeechDetected += (s, e) =>
                {
                    Logging.Log("Speech detected.");
                };
#endif
                Logging.Log("Events attached");
                recognizer.SetInputToDefaultAudioDevice();
            }

            [LoggingAspects.Logging]
            public static Task StartListeningAndProcessingAsync()
            {
                if (ready)
                    return Task.CompletedTask;
                ActivateVoiceCommandHandler();

                return Task.Run(() =>
                {
                    try
                    {
                        recognizer.RecognizeAsync(RecognizeMode.Multiple);
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError(ex);
                    }
                });
            }

            [LoggingAspects.Logging]
            public static void StopListening()
            {
                ready = false;
                recognizer.RecognizeAsyncStop();
            }
        }

    }
}
