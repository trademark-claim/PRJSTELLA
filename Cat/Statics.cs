//-------------------------------------------------------------------------------------
// <summary>
//     Statics.cs
//     Provides a collection of utility methods and properties for UI element manipulation
//     and file validation within the Cat application.
// </summary>
//
// <author>Nexus</author>
// <date>2024-04-10</date>
// <copyright>
//     Copyright (c) 2024 Nexus.
//     All rights reserved.
// </copyright>
//
// <notes>
//     This file is designed to support various UI and file handling operations,
//     facilitating easier management of visibility, positioning, and validation
//     within the application's user interface components.
// </notes>
//-------------------------------------------------------------------------------------



using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cat
{
    /// <summary>
    /// Contains utility methods and properties for managing UI elements and files.
    /// </summary>
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

        /// <summary>
        /// Sets the left position of a UIElement.
        /// </summary>
        /// <typeparam name="T">The type of the position value (int, double, float).</typeparam>
        /// <param name="obj">The UIElement to position.</param>
        /// <param name="where">The left position value.</param>
        /// <returns>The position value.</returns>
        internal static T SetLeft<T>(UIElement obj, T where)
        {
            Logging.Log($"Setting {obj.GetType().FullName}'s left position to {where}");
            var here = where is int ? where as int? : where is double ? where as double? : where is float ? where as float? : null;
            if (here != null)
                Canvas.SetLeft(obj, (double)here);
            return where;
        }

        /// <summary>
        /// Sets the top position of a UIElement.
        /// </summary>
        /// <typeparam name="T">The type of the position value (int, double, float).</typeparam>
        /// <param name="obj">The UIElement to position.</param>
        /// <param name="where">The top position value.</param>
        /// <returns>The position value.</returns>
        internal static T SetTop<T>(UIElement obj, T where)
        {
            Logging.Log($"Setting {obj.GetType().FullName}'s top position to {where}");
            var here = where is int ? where as int? : where is double ? where as double? : where is float ? where as float? : null;
            if (here != null)
                Canvas.SetTop(obj, (double)here);
            return where;
        }

        /// <summary>
        /// Toggles the visibility of a UIElement.
        /// </summary>
        /// <param name="obj">The UIElement to toggle.</param>
        /// <returns>True if the element is now visible; otherwise, false.</returns>
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

        /// <summary>
        /// Sets the visibility of a UIElement.
        /// </summary>
        /// <param name="obj">The UIElement to modify.</param>
        /// <param name="isVisible">Determines the visibility state.</param>
        internal static void ToggleVis(UIElement obj, bool isVisible)
        {
            Logging.Log($"Changing Visibility of {obj.GetType().FullName} to {isVisible}");
            obj.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            Logging.Log($"Visibility changed.");
        }

        /// <summary>
        /// Validates a file path for accessibility.
        /// </summary>
        /// <param name="path">The file path to validate.</param>
        /// <returns>True if the file is accessible; otherwise, false.</returns>
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

        /// <summary>
        /// Retrieves the ScrollViewer from a dependency object.
        /// </summary>
        /// <param name="depObj">The dependency object to search within.</param>
        /// <returns>The ScrollViewer if found; otherwise, null.</returns>
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

        /// <summary>
        /// Represents a fixed-size queue that overwrites its oldest elements.
        /// </summary>
        /// <typeparam name="T">The type of elements in the queue.</typeparam>
        public class FixedQueue<T>
        {
            private readonly List<T> _queue;
            private readonly int _maxSize;
            private int atnow;
            internal bool Failed { get; set; } = false;

            public FixedQueue(int maxSize)
            {
                if (maxSize <= 0)
                    throw new ArgumentException("Max size must be greater than 0", nameof(maxSize));

                _queue = new List<T>(maxSize);
                _maxSize = maxSize;
            }

            [LoggingAspects.ConsumeException]
            [LoggingAspects.Logging]
            public T? GetNext()
            {
                if (++atnow > _queue.Count)
                {
                    --atnow;
                    Failed = true;
                    return default;
                }
                return _queue[atnow];
            }

            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            public T? GetPrevious()
            {
                if (--atnow < _queue.Count)
                {
                    ++atnow;
                    Failed = true;
                    return default;
                }
                return _queue[atnow];
            }

            public T First => _queue.First();

            public T Last => _queue.Last();

            public void Enqueue(T item)
            {
                if (_queue.Count == _maxSize)
                    _queue.RemoveAt(9);
                _queue.Add(item);
            }

            public T Dequeue()
            {
                T nya = _queue.Last();
                _queue.Remove(nya);
                return nya;
            }

            public int Count => _queue.Count;

            public IEnumerable<T> Items => _queue;

        }

    }
}