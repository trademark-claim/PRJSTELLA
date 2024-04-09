using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        internal static class ClaraHerself
        {
            private static readonly string[] Introduction = [
                "Hey! It's me, Clara! \nIt seems this is the first time you've opened me (or I've been updated owo).\nIf you want to skip this, please type 'skip'. \nIf you want to view the changelog, type 'changelog'\nIf you want to run through the introduction, just press the right arrow key!",
                "So you wanna do the introduction again... sweet!\nI'm Clara, the Centralised, Logistical, Administrative and Requisition Assistant, and my sole purpose is to automate, optimize and otherwise improve your "
            ];

            internal enum Mode : byte
            {
                Introduction
            }

            internal static void RunClara(Mode mode)
            {
                switch (mode)
                {
                    case Mode.Introduction:

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