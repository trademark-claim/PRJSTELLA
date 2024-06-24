#define A
//#define B
//#define C

using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Speech.Recognition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using NAudio.Wave;
using SharpAvi;
using SharpAvi.Output;
using SharpAvi.Codecs;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics.Contracts;
using System.Timers;
using System.Runtime.CompilerServices;
using System.Net.NetworkInformation;

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
        /// Need to merge this with STELLA's interface's back ground for continuity
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
        /// Static class for dealing with direct Stella interactions, such as speech bubbles and images / animations (if we can get it)
        /// </summary>
        /// <remarks>
        /// Need to have back progression (using left arrow)
        /// </remarks>
        internal static class StellaHerself
        {
            /// <summary>
            /// Used with the speech bubble arrays to keep track of which array item we're on.
            /// </summary>
            private static byte num = 0;

            private static CancellationTokenSource fadeCancellationTokenSource = new CancellationTokenSource();

            /// <summary>
            /// Holds the introduction text
            /// </summary>
            private static readonly string[] Introduction = [
                "Hey! It's me, Stella! (Made by Nexus) \nIt seems this is the first time you've opened me (or I've been updated).\nIf you want to skip this, press the up arrow. \nIf you want to view the changelog, press the down arrow (not working)'\nIf you want to run through the introduction, just press the right arrow key!",
                "So you wanna do the introduction again... sweet!\nI'm Stella, the Smart Technology for Enhanced Lifestyle and Living Assistance! \nMy sole purpose is to automate, optimize and otherwise improve your computer experience.\n You can press the left arrow key to move through parts.",
                "There are two (at the moment) main modes to this program: Background and Interface.\nInterface is where there's an overlay with a textbox and an output box, where you can enter commands.\n   Key shortcuts won't work here, but this is where most of the functionality is.\nBackground is where there... is no main overlay (you're currently in background mode!).\n   This is what the app will be in 99% of the time.",
                "To open STELLA's interface:\n  Hold both shifts (both th left and right one),\n  Then press and hold Q,\n  then press I!\n  (LShift + RShift + Q + I). \n To close STELLA's interface run the 'close' command.\nTo view the help page, run 'help'",
                "This program is in a pre-pre-pre-pre-alpha stage, and there will be bugs and stuff.\nYou can send logs to me (Discord: _dissociation_) (Gmail: brainjuice.work23@gmail.com) with bug reports and feedback and stuff. Enjoy!",
                "Hmmm.. is there anything else..?\nOh right! Local data is stored at C:\\ProgramData\\Kitty\\Cat\\\nHave fun, I hope you enjoy this app! o/"
            ];

            internal static string[] Custom { get; set; } = ["Uh oh! You shouldn't see this!"];

            /// <summary>
            /// Whichever array is currently being spoken
            /// </summary>
            private static string[] CurrentStory = [];

            /// <summary>
            /// The bubble object being shown
            /// </summary>
            internal static SpeechBubble? Bubble { get; private set; }

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
                Introduction,
                Custom
            }

            internal static int FadeDelay { get; set; } = 1500;

            internal static bool Fading { get; set; } = true;

            internal static bool KeyNavigation { get; set; } = true;

            internal static bool HaveOverlay { get; set; } = true;

            internal static bool CleanUp { get; set; } = true;

            internal static TaskCompletionSource<bool> TCS { get; private set; }

            /// <summary>
            /// Cancer cures smoking.
            /// </summary>
            /// <param name="mode">Which array to run through</param>
            /// <param name="canvas">The canvas</param>
            [CAspects.Logging]
            [CAspects.ConsumeException]
            internal static async Task RunStella(Mode mode, Canvas canvas)
            {
                StellaHerself.canvas = canvas;
                if (Bubble != null && Catowo.inst != null && canvas != null)
                {
                    num = 0;
                    Catowo.inst.PreviewKeyDown -= ProgressionKeydown;
                    canvas.Children.Remove(Bubble);
                    Bubble = null;
                }

                switch (mode)
                {
                    case Mode.Introduction:
                        CurrentStory = Introduction;
                        Catowo.inst.MakeNormalWindow();
                        if (HaveOverlay)
                            overlay = OverlayRect.AddToCanvas(canvas);
                        break;

                    case Mode.Custom:
                        TCS = new TaskCompletionSource<bool>();
                        CurrentStory = Custom;
                        Logging.Log(["Custom Stella Speech: ", CurrentStory]);
                        if (fadeCancellationTokenSource != null && !fadeCancellationTokenSource.IsCancellationRequested)
                        {
                            fadeCancellationTokenSource.Cancel();
                            fadeCancellationTokenSource.Dispose();
                        }
                        fadeCancellationTokenSource = new();
                        var token = fadeCancellationTokenSource.Token;

                        if (Fading)
                            Task.Run(async () =>
                            {
                                await Task.Delay(FadeDelay, token);
                                while (true)
                                {
                                    if (token.IsCancellationRequested)
                                        return;

                                    await Task.Delay(50, token);
                                    try
                                    {
                                        if (Application.Current != null && Application.Current.Dispatcher != null)
                                            Application.Current.Dispatcher.Invoke(() =>
                                            {
                                                try
                                                {
                                                    if (Bubble != null && Catowo.inst != null && canvas != null )
                                                    {
                                                        Bubble.Opacity -= 0.015f;
                                                        if (Bubble.Opacity < 0.0f)
                                                        {
                                                            Bubble.Opacity = 0.0f;
                                                            num = 0;
                                                            Catowo.inst.PreviewKeyDown -= ProgressionKeydown;
                                                            canvas.Children.Remove(Bubble);
                                                            Bubble = null;
                                                            return;
                                                        }
                                                    }
                                                }
                                                catch
                                                {
                                                    _ = "Lorum Ipsum";
                                                }
                                            });
                                    }
                                    catch 
                                    {
                                        _ = "Lorum Ipsum";
                                    }
                                }
                            }, token);
                        break;
                }
                // The first message
                Bubble = new();
                Point location = new(Catowo.inst.Width - 30, Catowo.inst.Height - 30);
                Logging.Log(["Location", location]);
                Bubble.LowerRightCornerFreeze = location;
                Bubble.Text = CurrentStory[num];
                canvas.Children.Add(Bubble);
                Catowo.inst.PreviewKeyDown += ProgressionKeydown;
                return;
            }

            internal static void ForceRemove()
            {
                num = 0;
                Catowo.inst.PreviewKeyDown -= ProgressionKeydown;
                if (Bubble != null)
                {
                    canvas.Children.Remove(Bubble);
                    Bubble = null;
                }
                TCS.SetResult(true);
            }

            internal static void RemoveOverlay()
                => OverlayRect.RemoveFromCanvas(canvas, overlay);

            /// <summary>
            /// Event handler for the pressing of keys while a speech is activated. This is the base method to be used in tangent with specific methods for each set of speech, and is to be loading in and out as needed.
            /// </summary>
            /// <param name="sender">The caller of the event</param>
            /// <param name="e">The key event args</param>
            private static void ProgressionKeydown(object sender, System.Windows.Input.KeyEventArgs e)
            {
                if (KeyNavigation)
                {
                    if (e.Key == Key.Right)
                        if (canvas != null)
                        {
                            if (++num > CurrentStory.Length - 1)
                            {
                                TCS.SetResult(true);
                                num = 0;
                                Catowo.inst.PreviewKeyDown -= ProgressionKeydown;
                                if (CleanUp)
                                {
                                    Catowo.inst.MakeFunnyWindow();
                                    if (HaveOverlay)
                                        OverlayRect.RemoveFromCanvas(canvas, overlay);
                                }
                                if (Bubble != null)
                                {
                                    canvas.Children.Remove(Bubble);
                                    Bubble = null;
                                }
                                return;
                            }
                            if (Bubble != null)
                                Bubble.Text = CurrentStory[num];
                            return;
                        }
                    if (e.Key == Key.Left)
                        if (canvas != null)
                        {
                            num--;
                            if (num >= 0)
                                if (Bubble != null)
                                    Bubble.Text = CurrentStory[num];
                            return;
                        }
                    if (e.Key == Key.Down)
                        if (delegation != null)
                            delegation();
                }

                if (e.Key == Key.Up)
                    if (canvas != null)
                    {
                        TCS.SetResult(false);
                        num = 0;
                        if (CleanUp)
                            Catowo.inst.MakeFunnyWindow();
                        Catowo.inst.PreviewKeyDown -= ProgressionKeydown;
                        OverlayRect.RemoveFromCanvas(canvas, overlay);
                        if (Bubble != null)
                        {
                            canvas.Children.Remove(Bubble);
                            Bubble = null;
                        }
                        Fading = true;
                        FadeDelay = 1500;
                        Custom = [];
                        KeyNavigation = true;
                        HaveOverlay = true;
                        Catowo.Hooking.ChangeSeeking(0, null);
                        return;
                    }
            }

            /// <summary>
            /// Represents a speech bubble UI element.
            /// </summary>
            internal class SpeechBubble : Canvas
            {
                private float _opacity = Environment.UserData.Opacity;

                internal new float Opacity
                { get => _opacity; set { _opacity = value; rectangle.Opacity = value; tail.Opacity = value; textBlock.Opacity = value; } }

                /// <summary>
                /// The text displayed
                /// </summary>
                private TextBlock textBlock;

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
                        Children.Remove(textBlock);
                        textBlock = Helpers.BackendHelping.FormatTextBlock(value);
                        textBlock.TextWrapping = TextWrapping.Wrap;
                        textBlock.Margin = new Thickness(Control);
                        Children.Add(textBlock);
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
                public float BubbleOpacity
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
                        Opacity = 0.7f
                    };

                    tail = new Polygon
                    {
                        Points = new PointCollection(new[] { new Point(0, 0), new Point(15, 0), new Point(7.5, 20) }),
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Fill = new SolidColorBrush(Colors.White),
                        Opacity = 0.7f
                    };

                    textBlock = new TextBlock
                    {
                    };

                    TextPadding = new Thickness(Control);
                    Children.Add(rectangle);
                    Children.Add(tail);
                    Children.Add(textBlock);
                    FontSize = UserData.FontSize;

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

            [CAspects.Logging]
            internal static void Toggle()
            {
                if (!isOn) Run();
                else if (isOn) Stop();
                isOn = !isOn;
            }

            [CAspects.Logging]
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
                    Logging.Log([$"Set Win Style of Handle {hwnd} from {originalStyle:X} ({originalStyle:B}) [{originalStyle}] to {editedstyle:X} ({editedstyle:B}) [{editedstyle}]"]);
                };
                Particles.LineTrail.Init();
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

            [CAspects.Logging]
            internal static void DestroyKeyHook()
            {
                Logging.Log(["Unhooking key hook..."]);
                if (_hookID == IntPtr.Zero)
                {
                    Logging.Log(["Key hook is default, exiting."]);
                    return;
                }
                bool b = UnhookWindowsHookExWrapper(_hookID);
                Logging.Log([$"Unhooking successful: {b}"]);
            }

            [CAspects.Logging]
            private static void Stop()
            {
                if (!isOn) return;
                DestroyKeyHook();
                allencompassing.Close();
                try
                {
                    fadeTimer.Stop();
                }
                catch
                {
                    _ = "Lorum Ipsum";
                }
                fadeTimer = null;
                _method = null;
                Memento = null;
                _hookID = IntPtr.Zero;
                try
                {
                    canvas.Children.Clear();
                    canvas = null;
                }
                catch { _ = "Lorum Ipsum"; }
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

                    [CAspects.Logging]
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
                    if (oncedown)
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

            [CAspects.Logging]
            internal LogEditor() // Runs upon object creation
            {
                Topmost = true;

                var screen = Catowo.GetScreen();
                Width = screen.Bounds.Width;
                Height = screen.Bounds.Height;
                this.Background = WABrush;
                inst?.Close();
                inst = this;
                InitializeComponents();
                LoadLogs();
            }

            [CAspects.Logging]
            private void InitializeComponents()
            {
                mainGrid = new Grid() { Background = Brushes.Transparent };

                Content = mainGrid;
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Star) });
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });

                logListBox = new LoggingListBox() { Background = new SolidColorBrush(Color.FromArgb(0xAA, 0x0, 0x0, 0x0))};
                mainGrid.Children.Add(logListBox);

                menu = new MenuBox(logListBox) { Background = Brushes.Transparent };
                mainGrid.Children.Add(menu);

                files.SelectionChanged += (s, e) => { if (files.SelectedItem is string str && str.StartsWith('L') && str.EndsWith(".LOG")) logListBox.LoadFile(LogFolder + "//" + str); menu.InitializeMenu(); };
                files.Background = Brushes.Transparent;
                mainGrid.Children.Add(files);

                Grid.SetColumn(files, 0);
                Grid.SetColumn(logListBox, 1);
                Grid.SetColumn(menu, 2);

                foreach (var file in Directory.EnumerateFiles(LogFolder))
                    files.Items.Add(file.Replace(LogFolder, ""));
            }

            [CAspects.Logging]
            [CAspects.ConsumeException]
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
                internal static MenuBox minst;
                private static bool isSFiltered = false;

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
                        var x = new OverlayInputBox("What phrase would you like to add a filter for:", LogEditor.inst);
                        x.ShowDialog();
                        if (string.IsNullOrWhiteSpace(OverlayInputBox.Input))
                            return;
                        var current = inst.logListBox.Items.Cast<string>().ToList();
                        var logs = inst.logListBox.baselines.Where(x => x.Contains(OverlayInputBox.Input, StringComparison.InvariantCultureIgnoreCase) || (current.Contains(x) && isSFiltered));
                        isSFiltered = true;
                        inst.logListBox.Items.Clear();
                        foreach (var item in logs)
                            inst.logListBox.Items.Add(item);
                    })),
                    ("Exclude", (Action)(() =>
                    {
                        var x = new OverlayInputBox("What phrase would you like to add an exclusion filter for:", LogEditor.inst);
                        x.ShowDialog();
                        if (string.IsNullOrWhiteSpace(OverlayInputBox.Input))
                            return;
                        var logs = inst.logListBox.Items.Cast<string>().ToList().Where(x => !(x.Contains(OverlayInputBox.Input, StringComparison.InvariantCultureIgnoreCase)));
                        inst.logListBox.Items.Clear();
                        foreach (var item in logs)
                            inst.logListBox.Items.Add(item);
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

                internal void InitializeMenu()
                {
                    Items.Clear();
                    foreach ((string name, Action act) in buttons)
                        Items.Add(new ExecutableText<Action>(act) { Text = (act == null ? "" : "  ") + name });
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
#if Outline
                        AddItem(new OutlineText() { Text = line, StrokeColor = Brushes.Black, TextColor = Brushes.White});
#else
                        AddItem(line);
#endif
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
                var lbl = new Label() { Content = question, Foreground = new SolidColorBrush(Colors.Silver) };
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

            [CAspects.Logging]
            public static void ActivateVoiceCommandHandler()
            {
                ready = true;
                if (Speechrecogmap == null)
                {
                    Speechrecogmap = Helpers.JSONManager.ReadFromJsonFile<Dictionary<string, string>>("speechrecogmap.json");
                }
                recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));
                Logging.Log(["Engine created"]);
                Logging.Log(["Loading Grammer..."]);
                recognizer.LoadGrammar(new DictationGrammar());
                Logging.Log(["Created Grammer."]);
                recognizer.SpeechRecognized += (s, e) =>
                {
                    var result = e.Result;
                    Logging.Log(["Transcription: " + result.Text]);
                    Commands.ProcessVoiceCommand(result.Text);
                };
#if A
                recognizer.SpeechRecognitionRejected += (s, e) =>
                {
                    Logging.Log(["Speech recognition failed."]);
                    Interface.AddLog("Speech recognition failed.");
                };
#endif
#if B
                recognizer.RecognizeCompleted += (s, e) =>
                {
                    Logging.Log(["Recognition completed."]);
                };
#endif
#if C
                recognizer.SpeechDetected += (s, e) =>
                {
                    Logging.Log(["Speech detected."]);
                };
#endif
                Logging.Log(["Events attached"]);
                recognizer.SetInputToDefaultAudioDevice();
            }

            [CAspects.Logging]
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

            [CAspects.Logging]
            public static void StopListening()
            {
                ready = false;
                recognizer.RecognizeAsyncStop();
            }
        }

        internal class BoxSelecter<T> : Window
        {
            protected SWC.ComboBox processComboBox;
            protected TextBox searchTextBox;
            internal T SelectedItem { get; private set; }
            internal TaskCompletionSource<bool> TCS { get; } = new();
            protected List<T> items;

            public BoxSelecter(List<T> items, string title)
            {
                this.items = items;
                InitializeComponents(title);
                Topmost = true;
                processComboBox.ItemsSource = this.items;
                Loaded += (s, e) => searchTextBox.Focus();
                PreviewKeyDown += (s, e) =>
                {
                    if (e.Key == System.Windows.Input.Key.Enter)
                        Close();
                };
            }

            protected void InitializeComponents(string title)
            {
                Title = title;
                Width = 300;
                Height = 150;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;

                StackPanel panel = new StackPanel
                {
                    Orientation = SWC.Orientation.Vertical,
                    Margin = new Thickness(10)
                };

                searchTextBox = new TextBox
                {
                    Margin = new Thickness(0, 0, 0, 10)
                };
                searchTextBox.TextChanged += SearchTextBox_TextChanged;

                processComboBox = new SWC.ComboBox
                {
                    Height = 25,
                    IsSynchronizedWithCurrentItem = true
                };
                processComboBox.SelectionChanged += ProcessComboBox_SelectionChanged;

                panel.Children.Add(searchTextBox);
                panel.Children.Add(processComboBox);
                Content = panel;
            }


            protected virtual void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) 
            {
                processComboBox.ItemsSource = items.Where(item => (item.ToString() ?? "").ToLower().Contains(searchTextBox.Text));
            }

            protected virtual void ProcessComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (processComboBox.SelectedItem is T selectedProcess)
                {
                    SelectedItem = selectedProcess;
                }
            }

            protected override void OnClosed(EventArgs e)
            {
                base.OnClosed(e);
                if (processComboBox.SelectedItem == null)
                {
                    SelectedItem = items[0] == null ? (T)Helpers.BackendHelping.CreateDefault(typeof(T)) : items[0];
                }
                TCS.SetResult(true);
            }
        }


        internal class ProcessSelector : BoxSelecter<Process>
        {
            public int SelectedProcessId { get; private set; }
            private static List<Process> Processes { get => PopulateProcesses(); }

            public ProcessSelector() : base(items: Processes, "Select Process: ")
                => _ = "Lorum Ipsum";

            private static List<Process> PopulateProcesses()
            {
                return Process.GetProcesses()
                    .GroupBy(p => p.ProcessName)
                    .Select(g => g.First())
                    .OrderBy(p => p.ProcessName)
                    .ToList();
            }

            protected override void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
            {
                var filter = searchTextBox.Text.ToLower();
                processComboBox.ItemsSource = Process.GetProcesses()
                    .Where(p => p.ProcessName.ToLower().Contains(filter))
                    .GroupBy(p => p.ProcessName)
                    .Select(g => g.First())
                    .OrderBy(p => p.ProcessName)
                    .ToList();
            }

            protected override void ProcessComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (processComboBox.SelectedItem is Process selectedProcess)
                {
                    SelectedProcessId = selectedProcess.Id;
                }
            }

            protected override void OnClosed(EventArgs e)
            {
                if (processComboBox.SelectedItem == null)
                {
                    SelectedProcessId = -1;
                }
                base.OnClosed(e);
            }
        }

        internal class ProcessManager : Window
        {
            private Viewbox Wrapper { get; } = new();
            private Canvas Canvas { get; } = new();

            private DraggableBlock active;

            private int Refreshrate = 1;

            private bool isRunning = true;

            private readonly Process Prcs;

            private System.Timers.Timer timer;



            public ProcessManager(int AppID)
            {
                Prcs = Process.GetProcessById(AppID);
                Prcs.Exited += (_, _) => isRunning = false;
                Background = WABrush;
                Loaded += (s, e) => InitialiseBase();
                PreviewMouseLeftButtonDown += (s, e) =>
                {
                    if (active != null)
                    {
                        Logging.Log(["Mouse up on SLB element"]);
                        active.PreviewMouseMove += moving;
                        active.diff = new();
                        active.Background = Brushes.Transparent;
                        BorderThickness = new(0);
                        PreviewMouseMove += moving;
                    }
                };
                PreviewMouseLeftButtonUp += (s, e) =>
                {
                    Logging.Log(["Mouse up on SLB element"]);
                    if (active != null)
                    {
                        active.PreviewMouseMove -= moving;
                        active.diff = new();
                        active.Background = Brushes.Transparent;
                    }
                    PreviewMouseMove -= moving;
                };
                Closing += (s, e) =>
                {
                    timer?.Close();
                    timer?.Dispose();
                };
            }

            private void moving(object sender, System.Windows.Input.MouseEventArgs e)
            {
                if (active == null) return;
                Point p1 = new(Canvas.GetLeft(active), Canvas.GetTop(active));
                p1.X = double.IsNaN(p1.X) ? 0 : p1.X;
                p1.Y = double.IsNaN(p1.Y) ? 0 : p1.Y;
                Point p2 = e.GetPosition(active);
                Point p3 = new(p2.X - active.diff.X, p2.Y - active.diff.Y);
                Point p4 = new(p1.X + p3.X, p1.Y + p3.Y);
                Logging.Log([$"Original Position: {p1}", $"Relative Mouse Position: {p2}", $"Difference: {p3}", $"New Position: {p4}"]);
                Canvas.SetTop(active, p4.Y);
                Canvas.SetLeft(active, p4.X);
            }

            private async void InitialiseBase()
            {
                timer = new() { Interval = Refreshrate * 1000 };
                Canvas.Width = ActualWidth;
                Canvas.Height = ActualHeight;

                DraggableBlock cpu = new(this) { Text = "CPU %: ", FontSize = UserData.FontSize },
                    memory = new(this) { Text = "Memory: ", FontSize = UserData.FontSize },
                    gpu = new(this) { Text = "GPU %: ", FontSize = UserData.FontSize },
                    disk = new(this) { Text = "Disk IO: ", FontSize = UserData.FontSize },
                    network = new(this) { Text = "Network: ", FontSize = UserData.FontSize },
                    resources = new(this) { Text = "Resources: ", FontSize = UserData.FontSize },
                    threads = new(this) { Text = "Thread #: ", FontSize = UserData.FontSize },
                    lifetime = new(this) { Text = "Lifetime: ", FontSize = UserData.FontSize },
                    tree = new(this) { Text = "Process Tree: ", FontSize = UserData.FontSize },
                    activity = new(this) { Text = "Activity: ", FontSize = UserData.FontSize },
                    response = new(this) { Text = "Response Time: ", FontSize = UserData.FontSize },
                    calls = new(this) { Text = "Sys Calls: ", FontSize = UserData.FontSize },
                    accesses = new(this) { Text = "Service Accesses: ", FontSize = UserData.FontSize },
                    interactions = new(this) { Text = "Service Interactions: ", FontSize = UserData.FontSize },
                    state = new(this) { Text = "State: ", FontSize = UserData.FontSize },
                    mkhardware = new(this) { Text = "Mouse / Keyboard events: ", FontSize = UserData.FontSize },
                    ohardware = new(this) { Text = "Other hardware events: ", FontSize = UserData.FontSize },
                    user = new(this) { Text = "User: ", FontSize = UserData.FontSize },
                    authevents = new(this) { Text = "Auth Events: ", FontSize = UserData.FontSize },
                    shardware = new(this) { Text = "Other Hardware Stats: ", FontSize = UserData.FontSize },
                    traffic = new(this) { Text = "Network Traffic: ", FontSize = UserData.FontSize },
                    handles = new(this) { Text = "Handles: ", FontSize = UserData.FontSize },
                    privateBytes = new(this) { Text = "Private Bytes: ", FontSize = UserData.FontSize },
                    virtualMemory = new(this) { Text = "Virtual Memory: ", FontSize = UserData.FontSize },
                    ioRead = new(this) { Text = "IO Read Ops: ", FontSize = UserData.FontSize },
                    ioWrite = new(this) { Text = "IO Write Ops: ", FontSize = UserData.FontSize };

                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var iface in networkInterfaces) 
                {
                    Logging.Log([iface.Name]);
                }
                //Logging.FullFlush();
                //await Task.Delay(1000);
                var networkInterfaceName = networkInterfaces.FirstOrDefault()?.Name ?? "Ethernet";
                DraggableBlock networkSent = new(this) { Text = "Network Sent: ", FontSize = UserData.FontSize },
                    networkReceived = new(this) { Text = "Network Received: ", FontSize = UserData.FontSize };

                var cpuCounter = new PerformanceCounter("Process", "% Processor Time", Prcs.ProcessName, true);
                cpuCounter.NextValue();
                var diskCounter = new PerformanceCounter("Process", "IO Data Bytes/sec", Prcs.ProcessName, true);
                var memoryCounter = new PerformanceCounter("Process", "Working Set - Private", Prcs.ProcessName, true);
                var handleCounter = new PerformanceCounter("Process", "Handle Count", Prcs.ProcessName, true);
                var privateBytesCounter = new PerformanceCounter("Process", "Private Bytes", Prcs.ProcessName, true);
                var virtualMemoryCounter = new PerformanceCounter("Process", "Virtual Bytes", Prcs.ProcessName, true);
                var ioReadCounter = new PerformanceCounter("Process", "IO Read Operations/sec", Prcs.ProcessName, true);
                var ioWriteCounter = new PerformanceCounter("Process", "IO Write Operations/sec", Prcs.ProcessName, true);
                var networkSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", "", true);
                var networkReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", "", true);

                timer.Elapsed += (s, e) =>
                {
                    float cpuUsage = cpuCounter.NextValue() / System.Environment.ProcessorCount;
                    float diskUsage = diskCounter.NextValue() / (1024 * 1024); // Convert to MB
                    float memoryUsage = memoryCounter.NextValue() / (1024 * 1024); // Convert to MB
                    float handleCount = handleCounter.NextValue();
                    float privateBytesUsage = privateBytesCounter.NextValue() / (1024 * 1024); // Convert to MB
                    float virtualMemoryUsage = virtualMemoryCounter.NextValue() / (1024 * 1024); // Convert to MB
                    float ioReadOps = ioReadCounter.NextValue();
                    float ioWriteOps = ioWriteCounter.NextValue();
                    float networkSentBytes = networkSentCounter.NextValue() / (1024 * 1024); // Convert to MB
                    float networkReceivedBytes = networkReceivedCounter.NextValue() / (1024 * 1024); // Convert to MB
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            using (this.Dispatcher.DisableProcessing())
                            {

                                cpu.Text = $"CPU %: {cpuUsage:F2}";
                                disk.Text = $"Disk IO: {diskUsage:F2} MB";
                                memory.Text = $"Memory: {memoryUsage:F2} MB";
                                handles.Text = $"Handles: {handleCount:F0}";
                                privateBytes.Text = $"Private Bytes: {privateBytesUsage:F2} MB";
                                virtualMemory.Text = $"Virtual Memory: {virtualMemoryUsage:F2} MB";
                                ioRead.Text = $"IO Read Ops: {ioReadOps:F0}";
                                ioWrite.Text = $"IO Write Ops: {ioWriteOps:F0}";
                                networkSent.Text = $"Network Sent: {networkSentBytes:F2} MB";
                                networkReceived.Text = $"Network Received: {networkReceivedBytes:F2} MB";
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            if (!App.IsShuttingDown)
                                MessageBox.Show("An Error Occured, see logs for details", "Fatal Error Encountered", MessageBoxButton.OK, MessageBoxImage.Error);
                            Logging.LogError(ex);
                        }
                        catch { }
                    }
                };

                DraggableBlock[] blocks = {
            cpu, memory, gpu, disk, network, resources, threads, lifetime, tree,
            activity, response, calls, accesses, interactions, state, mkhardware,
            ohardware, user, authevents, shardware, traffic, handles, privateBytes,
            virtualMemory, ioRead, ioWrite, networkSent, networkReceived
        };
                int y = 10;
                int x = 20;

                foreach (var block in blocks)
                {
                    Canvas.Children.Add(block);
                    Canvas.SetTop(block, y);
                    Canvas.SetLeft(block, x);
                    y += 20;
                }

                Wrapper.Child = Canvas;
                Content = Wrapper;

                timer.Start();
            }

            internal class DraggableBlock : TextBlock
            {
                internal Point diff = new();
                private ProcessManager parent;

                internal DraggableBlock(ProcessManager parent)
                {
                    this.parent = parent;
                    FontSize = UserData.FontSize;
                    Background = Brushes.Transparent;
                    PreviewMouseLeftButtonDown += (s, e) =>
                    {
                        parent.active = this;
                        Logging.Log(["Mouse down on SLB element, moving?"]);
                        diff = e.GetPosition(this);
                        PreviewMouseMove += parent.moving;
                        Logging.Log([$"Diff: {diff}"]);
                        Background = new SolidColorBrush(Color.FromArgb(0xAA, 0x0, 0x0, 0x0));
                    };
                    PreviewMouseLeftButtonUp += (s, e) =>
                    {
                        Logging.Log(["Mouse up on SLB element"]);
                        PreviewMouseMove -= parent.moving;
                        diff = new();
                        Background = Brushes.Transparent;
                    };
                    Loaded += (s, e) => Height = ActualHeight;
                }
            }

        }

        internal class OutlineText : FrameworkElement
        {
            internal static readonly DependencyProperty TextProperty =
                DependencyProperty.Register("Text", typeof(string), typeof(OutlineText),
                    new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender));

            internal static readonly DependencyProperty TextColorProperty =
                DependencyProperty.Register("TextColor", typeof(Brush), typeof(OutlineText),
                    new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

            internal static readonly DependencyProperty StrokeColorProperty =
                DependencyProperty.Register("StrokeColor", typeof(Brush), typeof(OutlineText),
                    new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

            internal static readonly DependencyProperty StrokeThicknessProperty =
                DependencyProperty.Register("StrokeThickness", typeof(double), typeof(OutlineText),
                    new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

            internal static readonly DependencyProperty FontSizeProperty =
                DependencyProperty.Register("FontSize", typeof(double), typeof(OutlineText),
                    new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsRender));

            internal static readonly DependencyProperty FontWeightProperty =
                DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(OutlineText),
                    new FrameworkPropertyMetadata(FontWeights.Normal, FrameworkPropertyMetadataOptions.AffectsRender));

            internal static readonly DependencyProperty FontStyleProperty =
                DependencyProperty.Register("FontStyle", typeof(System.Windows.FontStyle), typeof(OutlineText),
                    new FrameworkPropertyMetadata(FontStyles.Normal, FrameworkPropertyMetadataOptions.AffectsRender));

            internal static readonly DependencyProperty PaddingProperty =
                DependencyProperty.Register("Padding", typeof(Thickness), typeof(OutlineText),
                    new FrameworkPropertyMetadata(new Thickness(5), FrameworkPropertyMetadataOptions.AffectsRender));

            internal string Text
            {
                get { return (string)GetValue(TextProperty); }
                set { SetValue(TextProperty, value); }
            }

            internal Brush TextColor
            {
                get { return (Brush)GetValue(TextColorProperty); }
                set { SetValue(TextColorProperty, value); }
            }

            internal Brush StrokeColor
            {
                get { return (Brush)GetValue(StrokeColorProperty); }
                set { SetValue(StrokeColorProperty, value); }
            }

            internal double StrokeThickness
            {
                get { return (double)GetValue(StrokeThicknessProperty); }
                set { SetValue(StrokeThicknessProperty, value); }
            }

            internal double FontSize
            {
                get { return (double)GetValue(FontSizeProperty); }
                set { SetValue(FontSizeProperty, value); }
            }

            internal FontWeight FontWeight
            {
                get { return (FontWeight)GetValue(FontWeightProperty); }
                set { SetValue(FontWeightProperty, value); }
            }

            internal System.Windows.FontStyle FontStyle
            {
                get { return (System.Windows.FontStyle)GetValue(FontStyleProperty); }
                set { SetValue(FontStyleProperty, value); }
            }

            internal Thickness Padding
            {
                get { return (Thickness)GetValue(PaddingProperty); }
                set { SetValue(PaddingProperty, value); }
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);
                double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

                Typeface typeface = new Typeface(new System.Windows.Media.FontFamily("Segoe UI"), FontStyle, FontWeight, FontStretches.Normal);

                FormattedText outlineText = new FormattedText(
                    Text,
                    CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    typeface,
                    FontSize,
                    StrokeColor, 
                    pixelsPerDip);

                FormattedText mainText = new FormattedText(
                    Text,
                    CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    typeface,
                    FontSize,
                    TextColor,
                    pixelsPerDip);

                Point textOrigin = new(Padding.Left + StrokeThickness, Padding.Top + StrokeThickness);

                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i != 0 || j != 0) 
                        {
                            drawingContext.DrawText(outlineText, new Point(textOrigin.X + i * StrokeThickness, textOrigin.Y + j * StrokeThickness));
                        }
                    }
                }
                drawingContext.DrawText(mainText, textOrigin);
            }


            protected override Size MeasureOverride(Size constraint)
            {
                Typeface typeface = new Typeface(new System.Windows.Media.FontFamily("Segoe UI"), FontStyle, FontWeight, FontStretches.Normal);
                FormattedText formattedText = new FormattedText(
                    Text,
                    CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    typeface,
                    FontSize,
                    TextColor,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                Size desiredSize = new Size(formattedText.Width + Padding.Left + Padding.Right + StrokeThickness * 2,
                                            formattedText.Height + Padding.Top + Padding.Bottom + StrokeThickness * 2);
                return desiredSize;
            }
        }
    }
}