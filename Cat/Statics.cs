using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cat
{
    internal static class Statics
    {
        internal static SolidColorBrush RED { get; } = new(Colors.Red);
        internal static SolidColorBrush GREEN { get; } = new(Colors.Green);
        internal static SolidColorBrush BLUE { get; } = new(Colors.Blue);
        internal static SolidColorBrush WHITE { get; } = new(Colors.White);
        internal static SolidColorBrush PURPLE { get; } = new(Colors.Purple);
        internal static SolidColorBrush CYAN { get; } = new(Colors.Cyan);
        internal static SolidColorBrush LIGHTRED { get; } = new(Colors.LightSalmon);
        internal static SolidColorBrush PINK { get; } = new(Colors.Pink);
        internal static SolidColorBrush DEEPPINK { get; } = new(Colors.DeepPink);
        internal static SolidColorBrush HOTPINK { get; } = new(Colors.HotPink);


        internal static T SetLeft<T>(UIElement obj, T where)
        {
            Logging.Log($"Setting {obj.GetType().FullName}'s left position to {where}");
            var here = where is int ? where as int? : where is double ? where as double? : where is float ? where as float? : null;
            if (here != null)
                Canvas.SetLeft(obj, (double)here);
            return where;
        }

        internal static T SetTop<T>(UIElement obj, T where)
        {
            Logging.Log($"Setting {obj.GetType().FullName}'s top position to {where}");
            var here = where is int ? where as int? : where is double ? where as double? : where is float ? where as float? : null;
            if (here != null)
                Canvas.SetTop(obj, (double)here);
            return where;
        }

        internal static bool ToggleVis(UIElement obj)
        {
            Logging.Log($"Toggling Visibility of {obj.GetType().FullName}...");
            if (obj.Visibility == Visibility.Visible)
            {
                obj.Visibility = Visibility.Collapsed;
                Logging.Log($"{obj.GetType().FullName} collapsed.");
                return false;
            }
            obj.Visibility = Visibility.Visible;
            Logging.Log($"{obj.GetType().FullName} made visible");
            return true;
        }

        internal static void ToggleVis(UIElement obj, bool isVisible)
        {
            Logging.Log($"Changing Visibility of {obj.GetType().FullName} to {isVisible}");
            obj.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            Logging.Log($"Visibility changed.");
        }


        internal static bool ValidateFile(string path)
        {
            Logging.Log("Validating " + path);
            try
            {
                using (var fs = File.OpenRead(path)) ;
            }
            catch (Exception ex)
            {
                Logging.Log("Error occured while opening " + path + ", returning false.");
                Logging.LogError(ex);
                return false;
            }
            Logging.Log("File " + path + " opened successfully, returning true.");
            return true;
        }

        [LoggingAspects.Logging]
        [LoggingAspects.ConsumeException]
        internal static ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            Logging.Log("Getting Scroll viewer dependacny object from object: " + depObj.GetType().FullName);
            if (depObj is ScrollViewer)
                return depObj as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = GetScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}