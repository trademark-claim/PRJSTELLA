using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Cat
{
    internal static class Objects
    {
        internal class ShutDownScreen : Canvas
        {
            private static ShutDownScreen inst;


            internal static ShutDownScreen ToggleScreen(Canvas canv) { if (inst != null) { canv.Children.Remove(inst); inst = null; return inst; } else { inst = new ShutDownScreen(); canv.Children.Add(inst); return inst; } }

            private ShutDownScreen()
            {
                inst = this;
                Children.Add(new System.Windows.Shapes.Rectangle() { Width = SystemParameters.PrimaryScreenWidth, Height = SystemParameters.PrimaryScreenHeight });
                SetTop(this, 0);
                SetLeft(this, 0);
            }
        }
    }
}
