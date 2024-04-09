using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Cat
{
    internal static class Objects
    {
        internal class ShutDownScreen : Canvas
        {
            private static ShutDownScreen inst;

            internal static ShutDownScreen ToggleScreen(Canvas canv)
            { if (inst != null) { canv.Children.Remove(inst); inst = null; return inst; } else { inst = new ShutDownScreen(); canv.Children.Add(inst); return inst; } }

            private ShutDownScreen()
            {
                inst = this;
                Children.Add(new System.Windows.Shapes.Rectangle() { Width = SystemParameters.PrimaryScreenWidth, Height = SystemParameters.PrimaryScreenHeight });
                SetTop(this, 0);
                SetLeft(this, 0);
            }
        }

        internal static class OverlayRect
        {
            private static readonly Rectangle Rectangle = new Rectangle { Width = Catowo.GetScreen().Bounds.Width, Height = Catowo.GetScreen().Bounds.Height, Fill = new SolidColorBrush(Colors.Gray), Opacity = UserData.Opacity };
            internal static void AddToCanvas(Canvas c)
            {
                c.Children.Add(Rectangle);
                UpdateRect();
            }

            private static void UpdateRect()
            {
                Rectangle.Opacity = UserData.Opacity;
                Rectangle.Width = Catowo.GetScreen().Bounds.Width;
                Rectangle.Height = Catowo.GetScreen().Bounds.Height;
            }

            internal static void RemoveFromCanvas(Canvas c)
                => c.Children.Remove(Rectangle);
        }

        internal static class ClaraHerself
        {
            private static byte num = 0;

            private static readonly string[] Introduction = [
                "Hey! It's me, Clara! \nIt seems this is the first time you've opened me (or I've been updated owo).\nIf you want to skip this, please type 'skip'. \nIf you want to view the changelog, type 'changelog'\nIf you want to run through the introduction, just press the right arrow key!",
                "So you wanna do the introduction again... sweet!\nI'm Clara, the Centralised, Logistical, Administrative and Requisition Assistant. \nMy sole purpose is to automate, optimize and otherwise improve your computer experience.\n You can press the left arrow key to move through parts",
                "There are two (at the moment) main modes to this program: Background and Interface.\nInterface is where there's an overlay with a textbox and an output box, where you can enter commands.\n   Key shortcuts won't work here, but this is where most of the functionality is.\nBackground is where there... is no main overlay (you're currently in background mode!).\n   This is what the app will be in 99% of the time.",
                "To open the interface, hold both shifts (both th left and right one), then press and hold Q, then press I! (LShift + RShift + Q + I). \n To close the interface run the 'close' command.\nTo view the help page, run 'help'",
                "Hmmm.. is there anything else..?\nOh right! Local data is stored at C:\\ProgramData\\Kitty\\Cat\\\nHave fun, I hope you enjoy this app! o/"
            ];

            private static string[] CurrentStory = [];

            private static SpeechBubble? bubble;

            private static Canvas? canvas;

            internal enum Mode : byte
            {
                Introduction
            }

            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            internal static void RunClara(Mode mode, Canvas canvas)
            {
                ClaraHerself.canvas = canvas;
                OverlayRect.AddToCanvas(canvas);
                Catowo.inst.MakeNormalWindow();
                Catowo.inst.PreviewKeyDown += ProgressionKeydown; ;
                switch (mode)
                {
                    case Mode.Introduction:
                        CurrentStory = Introduction;
                        break;
                }
                bubble = new SpeechBubble();
                bubble.Text = CurrentStory[num];
                canvas.Children.Add(bubble);
            }

            private static void ProgressionKeydown(object sender, System.Windows.Input.KeyEventArgs e)
            {
                if (e.Key == System.Windows.Input.Key.Left)
                    if (canvas != null) {
                        if (bubble != null)
                        {
                            canvas.Children.Remove(bubble);
                            bubble = null;
                        }
                        if (++num > CurrentStory.Length - 1)
                        {
                            num = 0;
                            Catowo.inst.MakeFunnyWindow();
                            Catowo.inst.PreviewKeyDown -= ProgressionKeydown;
                        }
                        bubble = new SpeechBubble();
                        bubble.Text = CurrentStory[num];
                        canvas.Children.Add(bubble);
                    }
            }

            private class SpeechBubble : Canvas
            {
                private readonly TextBlock textBlock;
                private readonly System.Windows.Shapes.Rectangle rectangle;
                private readonly Polygon tail;
                private const float Control = 5.0F;

                public string Text
                {
                    get => textBlock.Text;
                    set {
                        textBlock.Text = value;
                        UpdateLayout();
                    }
                }

                public double FontSize
                {
                    get => textBlock.FontSize;
                    set => textBlock.FontSize = value;
                }

                public double BubbleOpacity
                {
                    get => Opacity;
                    set => Opacity = value;
                }

                public System.Windows.Media.Brush BubbleColor
                {
                    get => rectangle.Fill;
                    set
                    {
                        rectangle.Fill = value;
                        tail.Fill = value;
                    }
                }

                public Thickness TextPadding
                {
                    get => textBlock.Margin;
                    set => textBlock.Margin = value;
                }


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
                    FontSize = UserData.FontSize;

                    SizeChanged += (s, e) => UpdateLayout();
                }

                public void UpdateLayout()
                {
                    textBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    double textWidth = textBlock.DesiredSize.Width + TextPadding.Left + TextPadding.Right;
                    double textHeight = textBlock.DesiredSize.Height + TextPadding.Top + TextPadding.Bottom;
                    rectangle.Width = textWidth + (Control * 2);
                    rectangle.Height = textHeight + (Control * 2);

                    SetLeft(textBlock, TextPadding.Left + Control);
                    SetTop(textBlock, TextPadding.Top + Control);
                    //tail.Margin = new Thickness(0, rectangle.Height - 30, 0, 0);
                    SetLeft<double>(tail, rectangle.Width - 30);
                    SetTop<double>(tail, rectangle.Height);
                }

            }

        }
    }
}
