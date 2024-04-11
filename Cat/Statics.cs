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

        internal static readonly LinearGradientBrush brush = new()
        {
            GradientStops =
                {
                    new(Colors.Red, 1.0),
                    new(Color.FromArgb((byte)(255 * 0.75), 0xFF, 0x99, 0xFF), 0.75),
                    new(Color.FromArgb((byte)(255 * 0.50), 0x66, 0xFF, 0xFF), 0.55),
                    new(Color.FromArgb((byte)(255 * 0.25), 0xFF, 0x00, 0x00), 0.30),
                    new(Colors.Transparent, 0.0)
                }
        };

        internal static readonly LinearGradientBrush rainbowbrush = new()
        {
            GradientStops =
                {
                    new GradientStop(Colors.Red, 1.0),
                    new GradientStop(Color.FromRgb(255, 127, 0), 0.07),
                    new GradientStop(Colors.Orange, 0.14),
                    new GradientStop(Color.FromRgb(255, 255, 0), 0.21),
                    new GradientStop(Colors.Yellow, 0.28),
                    new GradientStop(Color.FromRgb(127, 255, 0), 0.35),
                    new GradientStop(Colors.Green, 0.42),
                    new GradientStop(Color.FromRgb(0, 255, 127), 0.49),
                    new GradientStop(Color.FromRgb(0, 255, 255), 0.57),
                    new GradientStop(Color.FromRgb(0, 127, 255), 0.64),
                    new GradientStop(Colors.Blue, 0.71),
                    new GradientStop(Color.FromRgb(127, 0, 255), 0.78),
                    new GradientStop(Colors.Indigo, 0.85),
                    new GradientStop(Color.FromRgb(255, 0, 255), 0.92),
                    new GradientStop(Colors.Violet, 0.99),
                    new GradientStop(Colors.Transparent, 0.0)
                }
        };

        internal static readonly LinearGradientBrush dyingrainbow = new()
        {
            GradientStops =
                {
                    new GradientStop(Color.FromArgb(255, 255, 0, 0), 1.0), // Red
                    new GradientStop(Color.FromArgb(230, 255, 127, 0), 0.92), // Orange-Red
                    new GradientStop(Color.FromArgb(204, 255, 165, 0), 0.85), // Orange
                    new GradientStop(Color.FromArgb(178, 255, 255, 0), 0.78), // Yellow-Orange
                    new GradientStop(Color.FromArgb(153, 255, 255, 0), 0.71), // Yellow
                    new GradientStop(Color.FromArgb(127, 127, 255, 0), 0.64), // Yellow-Green
                    new GradientStop(Color.FromArgb(102, 0, 255, 0), 0.57), // Green
                    new GradientStop(Color.FromArgb(76, 0, 255, 127), 0.50), // Green-Cyan
                    new GradientStop(Color.FromArgb(51, 0, 255, 255), 0.43), // Cyan
                    new GradientStop(Color.FromArgb(25, 0, 127, 255), 0.36), // Cyan-Blue
                    new GradientStop(Color.FromArgb(0, 0, 0, 255), 0.29), // Blue
                    new GradientStop(Color.FromArgb(25, 127, 0, 255), 0.22), // Blue-Indigo
                    new GradientStop(Color.FromArgb(51, 75, 0, 130), 0.15), // Indigo
                    new GradientStop(Color.FromArgb(76, 238, 130, 238), 0.08), // Indigo-Violet
                    new GradientStop(Colors.Transparent, 0.0) // Fade to transparent at the end
                }
        };

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