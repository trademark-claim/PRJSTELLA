// -----------------------------------------------------------------------
// Helpers.cs
// Contains utility methods for screenshotting, screen recording,
// managing configurations through INI files, and other helper functions.
// Author: Nexus
// -----------------------------------------------------------------------

using IniParser;
using IniParser.Model;
using Newtonsoft.Json;
using SharpCompress.Archives;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms.Design;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using static Cat.Helpers.BinaryFileHandler;
using CLR = System.Drawing.Color;
using Formatting = Newtonsoft.Json.Formatting;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
//using IEnumerable = System.Collections.Generic.IEnumerable;

namespace Cat
{
    /// <summary>
    /// Provides static utility methods and classes for various operations like
    /// screenshotting, screen recording, INI file parsing, and more.
    /// </summary>
    internal static partial class Helpers
    {
        /// <summary>
        /// Contains methods for capturing screenshots of the screen(s).
        /// </summary>
        internal static class Screenshotting
        {
            /// <summary>
            /// Captures a screenshot of a specific screen.
            /// </summary>
            /// <param name="screenIndex">The index of the screen to capture.</param>
            /// <param name="error_message">Out parameter that will contain any error messages generated.</param>
            /// <returns>A Bitmap object of the captured screen.</returns>
            [LoggingAspects.Logging]
            [LoggingAspects.InterfaceNotice]
            [LoggingAspects.ConsumeException]
            internal static Bitmap CaptureScreen(int screenIndex, out string? error_message)
            {
                error_message = "";
                if (screenIndex < 0 || screenIndex >= System.Windows.Forms.Screen.AllScreens.Length)
                {
                    error_message = $"Invalid screen index {screenIndex}";
                    Logging.Log($"Invalid screen index {screenIndex}");
                    return new Bitmap(1, 1);  // Consider whether a small bitmap like this is suitable as a fallback.
                }

                var screen = Screen.AllScreens[screenIndex];
                var bounds = screen.Bounds;

                IntPtr desktopDC = GetWindowDCWrapper(GetDesktopWindowWrapper());
                IntPtr memoryDC = CreateCompatibleDCWrapper(desktopDC);
                IntPtr bitmap = CreateCompatibleBitmapWrapper(desktopDC, bounds.Width, bounds.Height);
                IntPtr oldBitmap = SelectObjectWrapper(memoryDC, bitmap);

                try
                {
                    BitBltWrapper(memoryDC, 0, 0, bounds.Width, bounds.Height, desktopDC, bounds.X, bounds.Y, PInvoke.CopyPixelOperation.SourceCopy);
                    Bitmap bmp = Image.FromHbitmap(bitmap);
                    return bmp;
                }
                finally
                {
                    SelectObjectWrapper(memoryDC, oldBitmap);
                    DeleteObjectWrapper(bitmap);
                    ReleaseDCWrapper(GetDesktopWindowWrapper(), desktopDC);
                    DeleteDCWrapper(memoryDC);
                }
            }


            /// <summary>
            /// Captures screenshots of all screens individually.
            /// </summary>
            /// <param name="errorMessages">Outputs a list of error messages corresponding to each screen capture attempt. A null entry indicates a successful capture.</param>
            /// <returns>A list of Bitmap objects representing the captured screens. Screens that encountered errors will have a corresponding null entry in the list.</returns>
            [LoggingAspects.Logging]
            internal static List<Bitmap> AllIndivCapture(out List<string?> errorMessages)
            {
                Logging.Log("Entering helper method Screenshotting.AllIndivCapture().");
                var bitmaps = new List<Bitmap>();
                errorMessages = new List<string?>();

                for (int i = 0; i < Screen.AllScreens.Length; i++)
                {
                    Logging.Log($"Attempting to capture screen at index: {i}");
                    var bmp = CaptureScreen(i, out string? error);
                    if (string.IsNullOrEmpty(error))
                    {
                        bitmaps.Add(bmp);
                        errorMessages.Add(null);
                    }
                    else
                    {
                        Logging.Log($"Error capturing screen {i}: {error}");
                        bitmaps.Add(null);
                        errorMessages.Add(error);
                    }
                }

                Logging.Log("Exiting helper method Screenshotting.AllIndivCapture()");
                return bitmaps;
            }

            /// <summary>
            /// Captures and stitches together screenshots of all screens into a single Bitmap.
            /// </summary>
            /// <param name="error_message">Outputs an error message if the stitching process fails.</param>
            /// <returns>A Bitmap object representing the stitched together screenshots of all screens. Returns a minimal Bitmap in case of failure.</returns>
            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            [LoggingAspects.InterfaceNotice]
            internal static Bitmap StitchCapture(out string? error_message)
            {
                Logging.Log("Entering helper Screenshotting.StitchCapture()");
                error_message = null;

                var totalBounds = Screen.AllScreens.Select(s => s.Bounds).Aggregate(System.Drawing.Rectangle.Union);
                Logging.Log($"Aggregated Bounds: {totalBounds.Width}px Width, {totalBounds.Height}px Height, {totalBounds.X}x, {totalBounds.Y}y");
                IntPtr desktopWnd = GetDesktopWindowWrapper();
                IntPtr desktopDC = GetWindowDCWrapper(desktopWnd);
                if (desktopDC == IntPtr.Zero)
                {
                    error_message = "Failed to get desktop device context.";
                    Logging.Log(error_message);
                    return new Bitmap(1, 1);
                }

                IntPtr memoryDC = CreateCompatibleDCWrapper(desktopDC);
                Logging.Log($"memoryDC: {memoryDC}");
                IntPtr bitmap = CreateCompatibleBitmapWrapper(desktopDC, totalBounds.Width, totalBounds.Height);
                IntPtr oldBitmap = SelectObjectWrapper(memoryDC, bitmap);

                Bitmap stitchedBitmap = new Bitmap(totalBounds.Width, totalBounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                try
                {
                    using (Graphics g = Graphics.FromImage(stitchedBitmap))
                    {
                        g.Clear(CLR.Transparent);
                        foreach (var screen in Screen.AllScreens)
                        {
                            Logging.Log($"Capturing screen: {screen.DeviceName}");
                            BitBltWrapper(memoryDC, screen.Bounds.X - totalBounds.X, screen.Bounds.Y - totalBounds.Y, screen.Bounds.Width, screen.Bounds.Height, desktopDC, screen.Bounds.X, screen.Bounds.Y, PInvoke.CopyPixelOperation.SourceCopy);
                        }
                        Bitmap result = Image.FromHbitmap(SelectObjectWrapper(memoryDC, oldBitmap));
                        g.DrawImageUnscaled(result, 0, 0);
                        result.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    error_message = $"Error stitching screens: {ex.Message}";
                    Logging.LogError(ex);
                    return new Bitmap(1, 1);
                }
                finally
                {
                    DeleteObjectWrapper(bitmap);
                    ReleaseDCWrapper(desktopWnd, desktopDC);
                    DeleteDCWrapper(memoryDC);
                }

                Logging.Log("Exiting helpermethod Screenshotting.StitchCapture()");
                return stitchedBitmap;
            }
        }

        /// <summary>
        /// Provides methods for handling screen size adjustments and DPI calculations.
        /// </summary>
        internal static class ScreenSizing
        {
            /// <summary>
            /// Calculates the adjusted screen size based on the system's DPI settings.
            /// </summary>
            /// <param name="screen">The screen for which to calculate the adjusted size.</param>
            /// <returns>A tuple containing the adjusted width, height, and working area height of the screen.</returns>
            /// <remarks>
            /// This method considers the DPI settings of the system to adjust the screen size for high DPI displays.
            /// </remarks>
            [LoggingAspects.Logging]
            internal static (double Width, double Height, double WorkingAreaHeight) GetAdjustedScreenSize(Screen screen)
            {
                var dpiX = GetSystemDpi("DpiX");
                var dpiY = GetSystemDpi("Dpi");
                double screenWidth = screen.Bounds.Width / dpiX;
                double screenHeight = screen.Bounds.Height / dpiY;
                double workAreaHeight = screen.WorkingArea.Height / dpiY;

                return (screenWidth, screenHeight, workAreaHeight);
            }

            /// <summary>
            /// Calculates the adjusted screen size based on the system's DPI settings.
            /// </summary>
            /// <param name="screen">The screen for which to calculate the adjusted size.</param>
            /// <returns>A tuple containing the adjusted width, height, and working area height of the screen.</returns>
            /// <remarks>
            /// This method considers the DPI settings of the system to adjust the screen size for high DPI displays.
            /// </remarks>
            [LoggingAspects.Logging]
            internal static void GetAdjustedScreenSize(Screen screen, out Rect newbounds)
            {
                var dpiX = GetSystemDpi("DpiX");
                var dpiY = GetSystemDpi("Dpi");
                double screenWidth = screen.Bounds.Width / dpiX;
                double screenHeight = screen.Bounds.Height / dpiY;
                newbounds = new() { Width = screenWidth, Height = screenHeight, Location = new(screen.Bounds.Top / dpiY, screen.Bounds.Left / dpiX) };
                return;
            }

            /// <summary>
            /// Retrieves the system's DPI setting for a given DPI property.
            /// </summary>
            /// <param name="dpiPropertyName">The name of the DPI property to retrieve.</param>
            /// <returns>The system's DPI setting for the given property, normalized to a scale where 96 DPI equals 1.0.</returns>
            /// <remarks>
            /// This method is used internally to support high DPI displays by calculating screen dimensions accurately.
            /// </remarks>
            [LoggingAspects.Logging]
            private static double GetSystemDpi(string dpiPropertyName)
            {
                var dpiProperty = typeof(SystemParameters).GetProperty(dpiPropertyName, BindingFlags.NonPublic | BindingFlags.Static);
                if (dpiProperty != null)
                {
                    int dpi = (int)dpiProperty.GetValue(null, null);
                    return dpi / 96.0;
                }
                return 1.0;
            }
        }

        /// <summary>
        /// A custom window for displaying cat images.
        /// </summary>
        public class CatWindow : Window
        {
            private readonly HttpClient _client = new HttpClient();
            private readonly SWC.Image _imageControl = new SWC.Image();
            private Logging.ProgressLogging Progress = new("Cat Window Image Download:", true);
            private Logging.ProgressLogging.SpinnyThing spinnything;

            /// <summary>
            /// Initializes a new instance of the <see cref="CatWindow"/> class.
            /// </summary>
            /// <remarks>
            /// This constructor initializes the window, sets it to always be on top, and begins the process
            /// of fetching and displaying a random cat image from an external service.
            /// </remarks>
            public CatWindow()
            {
                Logging.Log("Constructing Cat window");
                Title = "Random Cat Photo";
                Content = _imageControl;
                Logging.Log("Window Loaded");
                spinnything = new();
                FetchAndDisplayCatImage();
                Topmost = true;
            }

            /// <summary>
            /// Fetches a random cat image from the CATaaS (Cats as a Service) API and displays it in the window.
            /// </summary>
            /// <remarks>
            /// This method asynchronously gets a cat image URL from the CATaaS API, downloads the image,
            /// and displays it within the window. If the image fetch fails, an error is logged.
            /// </remarks>
            [LoggingAspects.Logging]
            [LoggingAspects.AsyncExceptionSwallower]
            [LoggingAspects.InterfaceNotice]
            private async Task FetchAndDisplayCatImage()
            {
                Logging.Log("Getting Cat image from https://cataas.com/cat?json=true");
                try
                {
                    string url = "https://cataas.com/cat?json=true";
                    HttpResponseMessage response = await _client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Logging.Log($"Response body: {responseBody}");
                    using (JsonDocument doc = JsonDocument.Parse(responseBody))
                    {
                        string id = doc.RootElement.GetProperty("_id").GetString();
                        Logging.Log($"ID: {id}");
                        string imageUrl = $"https://cataas.com/cat/{id}";
                        Logging.Log("Init'ing Bitmap");
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri(imageUrl, UriKind.Absolute);
                        bitmapImage.EndInit();
                        Logging.Log("Ending Init of Bitmap, loading...");
                        bitmapImage.DownloadCompleted += (s, e) =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                Logging.Log("Source downloaded for bitmap! Halving size...");
                                _imageControl.Source = bitmapImage;
                                _imageControl.Width = bitmapImage.PixelWidth / 2;
                                _imageControl.Height = bitmapImage.PixelHeight / 2;
                                Width = _imageControl.Width;
                                Height = _imageControl.Height;
                                Logging.Log("Bitmap complete, window should adjust size automatically.");
                                Interface.AddLog("Here is your kitty!");
                                spinnything.Stop();
                                Show();
                            });
                        };
                        bitmapImage.DownloadProgress += (s, e) => Progress.InvokeEvent(new((byte)e.Progress));
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogError(ex);
                    spinnything.Stop();
                }
            }
        }

        internal class EPManagement
        {
            private readonly Logging.ProgressLogging OverallProgress;
            private Logging.ProgressLogging SectionProgress;
            private readonly ProcessDetails ffmpeg = new(FFMPEGPath, @"https://www.gyan.dev/ffmpeg/builds/packages/ffmpeg-2024-03-25-git-ecdc94b97f-essentials_build.7z", "ffmpeg.exe", "ffmpeg-2024-03-25-git-ecdc94b97f-essentials_build", ".7z", "bin");
            private readonly ProcessDetails inuse;

            internal static bool FFmpegInstalled { get => CheckIfFileExists(FFMPEGPath); }

            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            internal EPManagement(Processes process)
            {
                inuse = process switch
                {
                    Processes.FFmpeg => ffmpeg,
                    _ => throw new NotImplementedException(),
                };
                OverallProgress = new($"Obtaining {inuse.Filename}...", true);
                Task.Run(async () => await DownloadAndExtractFile());
            }

            [LoggingAspects.Logging]
            private static bool CheckIfFileExists(string path)
            {
                Logging.Log($"Checking if {path} exists...");
                bool exists = File.Exists(path);
                Logging.Log($"File existence check returned {exists}");
                return exists;
            }


            /// <summary>
            /// Downloads and extracts a file from a remote server if it does not already exist locally.
            /// </summary>
            /// <remarks>
            /// This method asynchronously downloads a compressed file from a predefined URL,
            /// extracts it, and places the contents in a designated directory.
            /// </remarks>
            /// <param name="downloadUrl">The URL from which to download the file.</param>
            /// <param name="fileExtension">The extension of the compressed file (e.g., ".zip", ".7z").</param>
            [LoggingAspects.Logging]
            [LoggingAspects.AsyncExceptionSwallower]
            [LoggingAspects.InterfaceNotice]
            private async Task DownloadAndExtractFile()
            {
                SectionProgress = new("Downloading...", true);
                if (!CheckIfFileExists(inuse.Existance))
                {
                    Logging.Log($"Starting download from {inuse.downloadurl}...");
                    HttpClient client = new();

                    try
                    {
                        using (var response = await client.GetAsync(inuse.downloadurl, HttpCompletionOption.ResponseHeadersRead))
                        {
                            response.EnsureSuccessStatusCode();
                            var totalBytes = response.Content.Headers.ContentLength ?? 0;
                            Logging.Log("Downloading file...");

                            using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
                            {
                                string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + inuse.Extension);
                                Logging.Log($"Temporary download path: {tempPath}");

                                using (var streamToWriteTo = File.Open(tempPath, FileMode.Create))
                                {
                                    await CopyContentAsync(streamToReadFrom, streamToWriteTo, totalBytes);
                                }

                                Logging.Log("File downloaded, extracting...");
                                await ExtractArchive(tempPath);
                                File.Delete(tempPath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError(ex);
                        Logging.Log("Failed to download or extract the file.");
                    }
                }
                else
                {
                    Logging.Log("File already exists, skipping download.");
                }
            }

            private async Task ExtractArchive(string filePath)
            {
                switch (inuse.Extension)
                {
                    case ".7z":
                        await Extract7zArchiveAsync(filePath);
                        break;
                    case ".zip":
                        await ExtractZipArchive(filePath);
                        break;
                    default:
                        throw new NotSupportedException("Unsupported file extension for extraction.");
                }
            }

            /// <summary>
            /// Extracts the downloaded archive and moves the executable to the correct directory.
            /// </summary>
            /// <param name="archivePath">The path to the downloaded archive.</param>
            /// <remarks>
            /// After downloading the archive, this method extracts the contents and moves the
            /// executable to a predefined location for future use.
            /// </remarks>
            [LoggingAspects.Logging]
            [LoggingAspects.AsyncExceptionSwallower]
            [LoggingAspects.InterfaceNotice]
            private async Task Extract7zArchiveAsync(string archivePath)
            {
                Logging.Log($"Extracting {archivePath} to {ExternalProcessesFolder}");
                SectionProgress = new Logging.ProgressLogging("Extracting...", true);
                var loader = new Logging.ProgressLogging.SpinnyThing();
                try
                {
                    if (!Directory.Exists(ExternalProcessesFolder))
                    {
                        Directory.CreateDirectory(ExternalProcessesFolder);
                    }

                    using (var archive = SharpCompress.Archives.SevenZip.SevenZipArchive.Open(archivePath))
                    {
                        var totalEntries = archive.Entries.Count;
                        int entriesExtracted = 0;

                        foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                        {
                            await Task.Run(() => entry.WriteToDirectory(ExternalProcessesFolder, new SharpCompress.Common.ExtractionOptions() { ExtractFullPath = true, Overwrite = true }));
                            entriesExtracted++;
                            int percentComplete = (int)(((double)entriesExtracted / totalEntries) * 100);
                            SectionProgress.InvokeEvent(new((byte)percentComplete));
                            OverallProgress.InvokeEvent(new((byte)(50 + percentComplete / 2)));
                        }
                    }
                    Interface.AddLog("Locating and Moving executable...");
                    string extractedFolderPath = Path.Combine(ExternalProcessesFolder, inuse.Archivename);
                    if (inuse.Nest != null)
                        extractedFolderPath = Path.Combine(extractedFolderPath, inuse.Nest);
                    string extractpath = Path.Combine(extractedFolderPath, inuse.Filename);

                    if (File.Exists(extractpath))
                    {
                        File.Move(extractpath, inuse.Existance, true);
                        Logging.Log($"{inuse.Filename} moved to {inuse.Existance}.");
                        Interface.AddLog("Download Complete");
                        Directory.Delete(extractedFolderPath, true);
                    }
                    else
                    {
                        Logging.Log($"ERROR: {inuse.Filename} not found in expected path: {extractpath}");
                    }
                    loader.Stop();
                    Logging.Log("Extraction and file movement completed successfully.");
                }
                catch (Exception ex)
                {
                    Logging.Log($"Error during extraction or file movement");
                    Logging.LogError(ex);
                }

                SectionProgress.InvokeEvent(new(100));
                OverallProgress.InvokeEvent(new(100));
            }

            [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
            [Obsolete("This is a stub")]
            private async Task ExtractZipArchive(string archivepath)
            {
                throw new NotImplementedException();
            }


            /// <summary>
            /// Copies content from a source stream to a destination stream, reporting progress.
            /// </summary>
            /// <param name="source">The source stream to copy from.</param>
            /// <param name="destination">The destination stream to copy to.</param>
            /// <param name="totalBytes">The total number of bytes expected to copy.</param>
            /// <remarks>
            /// This method is used during the download process to copy the contents of the downloaded
            /// file to a local file while reporting the progress of the download.
            /// </remarks>
            [LoggingAspects.Logging]
            [LoggingAspects.AsyncExceptionSwallower]
            [LoggingAspects.InterfaceNotice]
            private async Task CopyContentAsync(Stream source, Stream destination, long totalBytes)
            {
                byte[] buffer = new byte[81920];
                int bytesRead;
                long totalRead = 0;

                try
                {
                    while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await destination.WriteAsync(buffer, 0, bytesRead);
                        totalRead += bytesRead;
                        int percentComplete = (int)((totalRead * 100) / totalBytes);
                        SectionProgress.InvokeEvent(new((byte)percentComplete));
                        OverallProgress.InvokeEvent(new((byte)(Math.Round((double)percentComplete / 2))));
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogError(ex);
                    Logging.Log("Error copying content during FFMPEG download.");
                }
            }

            internal enum Processes : byte
            {
                FFmpeg,
            }

            private readonly record struct ProcessDetails(string Existance, string downloadurl, string Filename, string Archivename, string Extension, string Nest);
        }

        /// <summary>
        /// Provides miscellaneous backend helper functions.
        /// </summary>
        public static partial class BackendHelping
        {
            [LoggingAspects.Logging]
            internal static object[] GetPropertyValues(object obj)
            {
                List<object> values = new List<object>();
                PropertyInfo[] properties = obj.GetType().GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    if (property.GetIndexParameters().Length == 0)
                    {
                        try
                        {
                            values.Add(property.GetValue(obj));
                        }
                        catch (Exception ex)
                        {
                            Logging.LogError(ex);
                        }
                    }
                }
                return values.ToArray();
            }
            //xx values.RemoveAll(x => x == null || (x is string a && string.IsNullOrWhiteSpace(a)) || (x is System.Collections.IEnumerable enu && enu.Cast<object>().Count() < 1));


            [LoggingAspects.Logging]
            internal static string ExtractGuid(string log)
            {
                var match = ErrorGuidRegex().Match(log);
                if (match.Success)
                {
                    return match.Groups[1].Value; //!The first captured group... should... be the GUID.
                }
                return "";
            }

            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            internal static IntPtr FindWindowWithPartialName(string name)
            {
                IntPtr result = IntPtr.Zero;
                EnumWindowsProc callback = (hWnd, lParam) =>
                {
                    string windowText = GetWindowTextWrapper(hWnd);
                    if (windowText.Contains(name, StringComparison.OrdinalIgnoreCase))
                    {
                        result = hWnd;
                        return false;
                    }
                    return true; 
                };
                EnumWindowsWrapper(callback, IntPtr.Zero);

                if (result != IntPtr.Zero)
                {
                    string finalWindowTitle = GetWindowTextWrapper(result);
                    Logging.Log($">PINVOKE< Final matched window title: '{finalWindowTitle}'");
                }

                return result;
            }

            public static Screen GetContainingScreen(System.Windows.Window window)
            {
                var windowInteropHelper = new System.Windows.Interop.WindowInteropHelper(window);
                IntPtr handle = windowInteropHelper.Handle;
                return Screen.FromHandle(handle);
            }

            public static bool IsPointWithinOtherPointForSmoothing(double pointX, double pointY, double centerX, double centerY)
            {
                double dx = pointX - centerX;
                double dy = pointY - centerY;
                double distanceSquared = dx * dx + dy * dy;
                return distanceSquared <= 1;
            }

            public static bool IsPointWithinOtherPointForSmoothing(Point a, Point center, double radius)
            {
                double dx = a.X - center.X;
                double dy = a.Y - center.Y;
                double distanceSquared = dx * dx + dy * dy;

                return distanceSquared <= radius * radius;
            }

            /// <summary>
            /// Rearranges the characters of a word, leaving the first and last characters in place.
            /// </summary>
            /// <param name="word">The word to process.</param>
            /// <returns>A new string with the middle characters of the original word shuffled randomly.</returns>
            /// <remarks>
            /// If the input word is two characters or less, it is returned unchanged. This method is intended to demonstrate
            /// how shuffling the middle characters of a word can often still leave the word recognizable to humans.
            /// </remarks>
            internal static string Glycemia(string word)
            {
                if (word.Length <= 2) return word;
                var middle = word[1..^1].ToCharArray();
                random.Shuffle(middle);
                return word[0] + new string(middle) + word[^1];
            }

            /// <summary>
            /// Extracts groups of strings from a larger string based on specified start and end delimiters.
            /// </summary>
            /// <param name="word">The string to search within.</param>
            /// <param name="sequencestarter">The delimiter that marks the start of a sequence.</param>
            /// <param name="sequenceender">The delimiter that marks the end of a sequence.</param>
            /// <param name="results">An array of strings that were found between the delimiters.</param>
            /// <returns>True if the operation was successful, False otherwise.</returns>
            /// <remarks>
            /// This method uses regular expressions to identify and extract sequences of characters
            /// that are enclosed by specified start and end delimiters.
            /// </remarks>
            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            internal static bool ExtractStringGroups(string word, string sequencestarter, string sequenceender, out string[]? results)
            {
                results = null;
                List<string> matches = new();
                if (string.IsNullOrEmpty(word) || word.Length <= 2 || !word.Contains(sequencestarter) || !word.Contains(sequenceender))
                {
                    Logging.Log("Invalid input for ExtractStringGroups()");
                    return false;
                }
                try
                {
                    string escapedStart = Regex.Escape(sequencestarter);
                    string escapedEnd = Regex.Escape(sequenceender);
                    string pattern = $"{escapedStart}(.*?){escapedEnd}";
                    matches.AddRange(from Match match in Regex.Matches(word, pattern, RegexOptions.Compiled)
                                     where match.Success
                                     select match.Groups[1].Value);
                }
                catch (Exception e)
                {
                    Logging.LogError(e);
                    Logging.Log("Exiting helper method ExtractStringGroups() due to error");
                    return false;
                }
                results = matches.ToArray();
                return true;
            }

            internal static TextBlock FormatTextBlock(string text)
            {
                TextBlock textBlock = new TextBlock();
                text = text.Replace("<tab>", new string(' ', 3)); 

                string[] tokens = text.Split(new[] { "<t>", "</t>", "<i>", "</i>", "<l>", "</l>", "<s>", "</s>", "<st>", "</st>", "<q>", "</q>" }, StringSplitOptions.None);
                bool isBoldUnderlined = false, isItalic = false, isSilver = false, isLargerBold = false, isUnderlined = false, silverital = false;

                foreach (var token in tokens)
                {
                    switch (token)
                    {
                        case "<t>": isBoldUnderlined = true; break;
                        case "</t>": isBoldUnderlined = false; break;
                        case "<i>": isItalic = true; break;
                        case "</i>": isItalic = false; break;
                        case "<l>": isSilver = true; break;
                        case "</l>": isSilver = false; break;
                        case "<s>": isLargerBold = true; break;
                        case "</s>": isLargerBold = false; break;
                        case "<st>": isUnderlined = true; break;
                        case "</st>": isUnderlined = false; break;
                        case "<q>": silverital = true; break;
                        case "</q>": silverital = false; break;
                        default:
                            var run = new Run(token);
                            if (isBoldUnderlined)
                            {
                                run.FontWeight = FontWeights.Bold;
                                run.TextDecorations = TextDecorations.Underline;
                                run.FontSize = textBlock.FontSize + 4;
                            }
                            if (isItalic) run.FontStyle = FontStyles.Italic;
                            if (isSilver) run.Foreground = new SolidColorBrush(Colors.Silver);
                            if (silverital)
                            {
                                run.Foreground = new SolidColorBrush(Colors.Silver);
                                run.FontStyle = FontStyles.Italic;
                            }
                            if (isLargerBold)
                            {
                                run.FontWeight = FontWeights.Bold;
                                run.FontSize = textBlock.FontSize + 2;
                            }
                            if (isUnderlined) run.TextDecorations = TextDecorations.Underline;
                            textBlock.Inlines.Add(run);
                            break;
                    }
                }

                return textBlock;
            }

            [LoggingAspects.Logging]
            public static bool CheckIfAdmin()
            {
                using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
                {
                    var principal = new System.Security.Principal.WindowsPrincipal(identity);
                    return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
                }
            }

            /// <summary>
            /// Checks if the current process has administrative privileges and, if not,
            /// attempts to restart the program with elevated privileges.
            /// </summary>
            [LoggingAspects.Logging]
            public static bool? RestartWithAdminRightsIfNeeded()
            {
                if (CheckIfAdmin())
                {
                    Logging.Log("The application is already running with administrative privileges.");
                    return null;
                }
                else
                {
                    try
                    {
                        var exePath = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
                        var processInfo = new ProcessStartInfo
                        {
                            UseShellExecute = true,
                            WorkingDirectory = System.Environment.CurrentDirectory,
                            FileName = exePath,
                            Verb = "runas"
                        };

                        Process.Start(processInfo);
                        App.ShuttingDown();
                    }
                    catch (Exception ex)
                    {
                        Logging.Log($"Error restarting application with administrative rights.");
                        Logging.Log(ex);
                        return false;
                    }
                }
                return true;
            }

            [GeneratedRegex(@"ERROR (\S+) START")]
            private static partial Regex ErrorGuidRegex();
        }

        /// <summary>
        /// Contains methods for testing and generating progress logs.
        /// </summary>
        internal static class ProgressTesting
        {
            /// <summary>
            /// Initiates a progress test by incrementally logging progress from 0 to 100.
            /// </summary>
            /// <remarks>
            /// This method simulates a progressing operation by randomly delaying between 1 to 100 milliseconds before incrementing
            /// a progress value. The progress is logged using a <see cref="Logging.ProgressLogging"/> instance, which visually represents
            /// the progression of an operation in the application's logging system. A spinning indicator is also activated to provide
            /// visual feedback during the test's execution. This method serves as a demonstration of asynchronous progress reporting
            /// within a potentially long-running operation.
            /// </remarks>
            [LoggingAspects.Logging]
            [LoggingAspects.AsyncExceptionSwallower]
            internal static async void GenerateProgressingTest()
            {
                uint rnd = (uint)random.Next(int.MaxValue);
                Logging.ProgressLogging plog = new($"Progress test {rnd}:", true);
                var spin = new Logging.ProgressLogging.SpinnyThing();
                byte progress = 0;
                while (progress < 100)
                {
                    await Task.Delay(random.Next(1, 100));
                    plog.InvokeEvent(new(++progress));
                }
                spin.Stop();
            }
        }

        /// <summary>
        /// Handles parsing, reading, and writing of INI configuration files.
        /// </summary>
        internal static class IniParsing
        {
            internal static readonly Dictionary<string, (object, object)> validation = new Dictionary<string, (object, object)>()
            {
                { "Brightness", (typeof(float), (0.0f, 1.0f)) },
                { "Opacity", (typeof(float), (0.0f, 1.0f)) },
                { "Startup", (typeof(bool), false) },
                { "AspectLogging", (typeof(bool), false) },
                { "FullLogging", (typeof(bool), false) },
                { "AssemblyInformation", (typeof(bool), false) },
                { "EnvironmentVariables", (typeof(bool), false) },
                { "FontSize", (typeof(float), (1.0f, 50.0f)) },
                { "TimeAll", (typeof(bool), false) },
                { "LoggingDetails", (typeof(bool), false) },
                { "AllowRegistryEdits", (typeof(bool), false) },
                { "LaunchAsAdmin", (typeof(bool), false) },
                { "StartWithInterface", (typeof(bool), false)},
                { "StartWithConsole", (typeof(bool), false)},
                { "StartWithVoice", (typeof(bool), false)},
                { "AllowUrbanDictionaryDefinitionsWhenWordNotFound", (typeof(bool), false)},
            };

            internal static readonly Dictionary<string, List<(string, object)>> initalsettings = new()
            {
                {
                    "Display", new() {
                        ("Brightness", 0.8f),
                        ("Opacity", 0.7f),
                        ("FontSize", 10)
                    }
                },
                {
                    "Startup", new() {
                        ("Startup", true),
                        ("StartWithConsole",  false),
                        ("StartWithInterface",  false),
                        ("StartWithVoice",  false),
                    }
                },
                {
                    "Permissions", new() {
                        ("AllowRegistryEdits", false),
                        ("LaunchAsAdmin", false),
                        ("AllowUrbanDictionaryDefinitionsWhenWordNotFound", false),
                    }
                },
                {
                    "Logging", new()
                    {
                        ("AspectLogging", true),
                        ("FullLogging", true),
                        ("AssemblyInformation", false),
                        ("EnvironmentVariables", false),
                        ("TimeAll", false),
                        ("LoggingDetails", false)
                    }
                }
            };

            /// <summary>
            /// Generates user data by initializing settings based on predefined initial settings.
            /// </summary>
            /// <remarks>
            /// This method constructs an INI file structure from a predefined dictionary of settings and writes it to a file.
            /// It's used for initializing user configuration settings upon application setup or when resetting to default settings.
            /// </remarks>
            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            [LoggingAspects.UpsetStomach]
            internal static void GenerateUserData()
            {
                IniData data = new();
                foreach (string key in initalsettings.Keys)
                {
                    if (data[key] == null)
                        data.Sections.AddSection(key);
                    foreach ((string innerkey, object value) in initalsettings[key])
                    {
                        data[key][innerkey] = value.ToString();
                    }
                }
                FileIniDataParser parser = new();
                parser.WriteFile(UserDataFile, data);
            }

            /// <summary>
            /// Retrieves a value from an INI file based on the specified section and key.
            /// </summary>
            /// <param name="filePath">The path to the INI file.</param>
            /// <param name="section">The section in the INI file containing the key.</param>
            /// <param name="key">The key whose value is to be retrieved.</param>
            /// <returns>The value associated with the specified key within the given section, or null if not found.</returns>
            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            [LoggingAspects.InterfaceNotice]
            public static string GetValue(string filePath, string section, string key)
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile(filePath);

                if (data[section] != null && data[section].ContainsKey(key))
                {
                    return data[section][key];
                }

                return null;
            }

            /// <summary>
            /// Retrieves the entire structure of an INI file as a nested dictionary.
            /// </summary>
            /// <param name="filePath">The path to the INI file.</param>
            /// <returns>A dictionary representing the sections of the INI file, each containing a dictionary of key-value pairs.</returns>
            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            [LoggingAspects.InterfaceNotice]
            public static Dictionary<string, Dictionary<string, string>> GetStructure(string filePath)
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile(filePath);

                var result = new Dictionary<string, Dictionary<string, string>>();
                foreach (var section in data.Sections)
                {
                    var sectionDict = new Dictionary<string, string>();
                    foreach (var key in section.Keys)
                    {
                        sectionDict.Add(key.KeyName, key.Value);
                    }
                    result.Add(section.SectionName, sectionDict);
                }

                return result;
            }

            /// <summary>
            /// Updates or adds a value in an INI file for a specific section and key.
            /// </summary>
            /// <param name="filePath">The path to the INI file.</param>
            /// <param name="section">The section in the INI file where the key-value pair should be updated or added.</param>
            /// <param name="key">The key to update or add.</param>
            /// <param name="value">The value to assign to the key.</param>
            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            [LoggingAspects.InterfaceNotice]
            public static void UpAddValue(string filePath, string section, string key, string value)
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile(filePath);

                if (data[section] == null)
                {
                    data.Sections.AddSection(section);
                }

                data[section][key] = value;
                parser.WriteFile(filePath, data);
            }

            /// <summary>
            /// Removes an entry from an INI file based on the specified section and key.
            /// </summary>
            /// <param name="filePath">The path to the INI file.</param>
            /// <param name="section">The section from which to remove the key.</param>
            /// <param name="key">The key to remove.</param>
            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            [LoggingAspects.InterfaceNotice]
            public static void RemoveEntry(string filePath, string section, string key)
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile(filePath);

                if (data[section] != null && data[section].ContainsKey(key))
                {
                    data[section].RemoveKey(key);
                    parser.WriteFile(filePath, data);
                }
            }
        }

        internal static class JSONManager
        {
            public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
            {
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Auto
                };

                string json = JsonConvert.SerializeObject(objectToWrite, settings);

                using (StreamWriter file = File.CreateText(filePath))
                {
                    JsonSerializer serializer = JsonSerializer.Create(settings);
                    serializer.Serialize(file, objectToWrite);
                }
            }

            // Reads an object from a JSON file.
            public static T ReadFromJsonFile<T>(string filePath) where T : new()
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("File not found.", filePath);

                using (StreamReader file = File.OpenText(filePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return (T)serializer.Deserialize(file, typeof(T));
                }
            }

            public static TValue ExtractValueFromJsonFile<TKey, TValue>(string filePath, TKey key)
            {
                var dictionary = ReadFromJsonFile<Dictionary<TKey, TValue>>(filePath);

                if (dictionary.TryGetValue(key, out TValue value))
                    return value;

                throw new KeyNotFoundException($"Key '{key}' not found in the JSON file.");
            }
        }

        internal static class HTMLStuff
        {
            [LoggingAspects.Logging]
            private static async Task<(bool, dynamic? d)> GetDictAPIDefinition(string word)
            {
                string url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
                Nullable<bool> d = null;
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage res = await client.GetAsync(url))
                using (HttpContent content = res.Content)
                {
                    string data = await content.ReadAsStringAsync();
                    if (data == null || data.Contains("No Definitions Found"))
                    {
                        return (false, d);
                    }
                    else return (true, JsonConvert.DeserializeObject<List<Dictionary<string, dynamic>>>(data)[0]);
                }
            }

            [LoggingAspects.Logging]
            private static async Task<(bool, dynamic? d)> GetUAPIDefinition(string word)
            {
                string url = $"https://unofficialurbandictionaryapi.com/api/search?term={word}&strict=false&matchCase=false&limit=3&page=1&multiPage=false&";
                Nullable<bool> d = null;
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage res = await client.GetAsync(url))
                using (HttpContent content = res.Content)
                {
                    string data = await content.ReadAsStringAsync();
                    if (data == null || data.Contains("No Definitions Found"))
                    {
                        return (false, d);
                    }
                    else return (true, JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(data));
                }
            }

            [LoggingAspects.Logging]
            [LoggingAspects.AsyncExceptionSwallower]
            internal static async Task<(bool?, Dictionary<string, dynamic>?)> DefineWord(string word)
            {
                (bool state, dynamic? d) = await GetDictAPIDefinition(word);
                if ((state == false || d == null))
                    if (!UserData.AllowUrbanDictionaryDefinitionsWhenWordNotFound)
                        return (false, null);
                    else
                    {
                        (state, d) = await GetUAPIDefinition(word);
                        if (state == false || d == null)
                            return (false, null);
                        else return (null, d);
                    }
                return (true, d);
            }

            [Obsolete("Use Dynamic Dictionary Instead", true)]
            internal class DAPIDefinitionFULL
            {
                internal string word { get; set; }
                internal string phonetic { get; set; }
                internal List<PhoneticEntry> phonetics { get; set; }

                [JsonProperty(PropertyName="meanings")]
                internal List<Meaning> meanings { get; set; }
                internal License license { get; set; }
                internal List<string> sourceUrls { get; set; }

                internal class PhoneticEntry
                {
                    internal string text { get; set; }
                    internal string audio { get; set; }
                    internal string sourceUrl { get; set; }
                    internal License license { get; set; }
                }

                internal class Meaning
                {
                    internal string partOfSpeech { get; set; }
                    internal List<Definition> definitions { get; set; }
                    internal List<string> synonyms { get; set; }
                    internal List<string> antonyms { get; set; }
                }

                internal class Definition
                {
                    internal string definition { get; set; }
                    internal List<string> synonyms { get; set; }
                    internal List<string> antonyms { get; set; }
                }

                internal class License
                {
                    internal string name { get; set; }
                    internal string url { get; set; }
                }
            }

            [Obsolete("Use Dynamic Dictionary Instead", true)]
            internal class UrAPIDefinitionFULL
            {
                internal int statusCode { get; private set; }
                internal string term { get; private set; }
                internal bool found { get; private set; }

                [JsonProperty("params")]
                internal Dictionary<string, string> Params { get; private set; }
                internal string totalPages { get; private set; }
                internal List<Entry> data { get; private set; }


                internal class Entry
                {
                    internal string word { get; private set; }
                    internal string meaning { get; private set; }
                    internal string example { get; private set; }
                    internal string contributor { get; private set; }
                    internal string date { get; private set; }
                }
            }
        }

        internal sealed class BinaryFileHandler : IDisposable
        {
            private FileStream fs;
            private dynamic BinaryAccessStream;
            private BinaryReader? backup;
            /// <summary>
            /// True = reading, false = writing, null = both
            /// </summary>
            private readonly bool? reading;
            private readonly string filename;

            [LoggingAspects.Logging]
            internal static bool CheckTypeMatch(byte enumValue, object obj)
            {
                if (obj == null)
                    return false;

                Types typeEnum;
                try
                {
                    typeEnum = (Types)enumValue;
                }
                catch (Exception)
                {
                    Logging.Log($"ERROR: Schema type '{enumValue}' unsuccessfully case to to BFH.Types!");
                    return false;
                }

                Type objectType = obj.GetType();
                Logging.Log($"Object type: {objectType.FullName}", $"Target Type: {typeEnum}");

                return typeEnum switch
                {
                    Types.Boolean => objectType == typeof(bool),
                    Types.Byte => objectType == typeof(byte),
                    Types.Bytes => objectType == typeof(byte[]),
                    Types.Char => objectType == typeof(char),
                    Types.Chars => objectType == typeof(char[]),
                    Types.Decimal => objectType == typeof(decimal),
                    Types.Double => objectType == typeof(double),
                    Types.Int16 => objectType == typeof(short),
                    Types.Int32 => objectType == typeof(int),
                    Types.Int64 => objectType == typeof(long),
                    Types.SByte => objectType == typeof(sbyte),
                    Types.Single => objectType == typeof(float),
                    Types.String => objectType == typeof(string),
                    Types.UInt16 => objectType == typeof(ushort),
                    Types.UInt32 => objectType == typeof(uint),
                    Types.UInt64 => objectType == typeof(ulong),
                    _ => false,
                };
            }

            internal enum Types : byte
            {
                Read = 0,
                SevenBitEncodedInt = 1,
                SevenBitEncodedInt64 = 2,
                Boolean = 3,
                Byte = 4,
                Bytes = 5,
                Char = 6,
                Chars = 7,
                Decimal = 8,
                Double = 9,
                [Obsolete("Unsupported from 5.0 or something, use Single or bytes.", true)]
                Half = 10,
                Int16 = 11,
                Int32 = 12,
                Int64 = 13,
                SByte = 14,
                Single = 15,
                String = 16,
                UInt16 = 17,
                UInt32 = 18,
                UInt64 = 19,
                
                ArrayCounter = 99,
                List = 100,
                Failed = 255,
            }

            internal static string ReturnRawBinary(string filepath)
            {
                string output = "";
                ProcessStartInfo processStartInfo = new()
                {
                    FileName = "powershell.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new())
                {
                    process.StartInfo = processStartInfo;
                    try
                    {
                        process.Start();
                    }
                    catch (Exception ex)
                    {
                        Interface.AddLog("Error when starting Powershell. Check your anti-virus");
                        Logging.Log(ex);
                        return "";
                    }

                    using (StreamWriter sw = process.StandardInput)
                    {
                        if (sw.BaseStream.CanWrite)
                        {
                            sw.WriteLine($"format-hex \"{filepath}\" | more");
                        }
                    }
                    output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                }

                return output;
            }

            [LoggingAspects.Logging]
            private (byte[], List<Types>) SerializeObject(object obj, int schema, bool? skipReflection /*true for skipping property serialisation, null for using the object as its (not put in an array), false for default property usage (classes)*/= false)
            {
                Logging.Log($"Recursion: {skipReflection}");
                object[] inputs = [];
                System.Collections.IEnumerable enume = null;
                if (skipReflection == true)
                    inputs = [obj];
                else if (skipReflection == null)
                {
                    if ((obj is System.Collections.IEnumerable enu))
                        enume = enu;
                    else
                        inputs = Helpers.BackendHelping.GetPropertyValues(obj);
                }
                else
                    inputs = Helpers.BackendHelping.GetPropertyValues(obj);
                if (enume != null)
                    enume = inputs;
                Logging.Log("Inputs", inputs);
                if (enume != null)
                    Logging.Log("Enume", enume);


                List<Types> typeswritten = [];
                Logging.Log("Writing to Memory stream...");
                using (var ms = new MemoryStream())
                {
                    using (var w = new BinaryWriter(ms))
                    {
                        if (schema != -2)
                        {
                            Logging.Log($"Attaching schema {schema} to object");
                            w.Write(schema);
                        }
                        foreach (var item in skipReflection == null? enume : inputs)
                        {
                            switch (item)
                            {
                                case string str:
                                    Logging.Log("Writing string");
                                    w.Write((byte)Types.String);
                                    w.Write(str);
                                    typeswritten.Add(Types.String);
                                    break;
                                case bool boo:
                                    Logging.Log("Writing bool");
                                    w.Write((byte)Types.Boolean);
                                    w.Write(boo);
                                    typeswritten.Add(Types.Boolean);
                                    break;
                                case byte bt:
                                    Logging.Log("Writing byte");
                                    w.Write((byte)Types.Byte);
                                    w.Write(bt);
                                    typeswritten.Add(Types.Byte);
                                    break;
                                case byte[] bts:
                                    Logging.Log("Writing byte array");
                                    w.Write((byte)Types.Bytes);
                                    w.Write(bts.Length);
                                    w.Write(bts);
                                    typeswritten.Add(Types.Bytes);
                                    break;
                                case char c:
                                    Logging.Log("Writing char");
                                    w.Write((byte)Types.Char);
                                    w.Write(c);
                                    typeswritten.Add(Types.Char);
                                    break;
                                case char[] cs:
                                    Logging.Log("Writing char array");
                                    w.Write((byte)Types.Chars);
                                    w.Write(cs.Length);
                                    w.Write(cs);
                                    typeswritten.Add(Types.Chars);
                                    break;
                                case decimal d:
                                    Logging.Log("Writing decimal");
                                    w.Write((byte)Types.Decimal);
                                    w.Write(d);
                                    typeswritten.Add(Types.Decimal);
                                    break;
                                case double dbl:
                                    Logging.Log("Writing double");
                                    w.Write((byte)Types.Double);
                                    w.Write(dbl);
                                    typeswritten.Add(Types.Double);
                                    break;
                                case short s:
                                    Logging.Log("Writing short");
                                    w.Write((byte)Types.Int16);
                                    w.Write(s);
                                    typeswritten.Add(Types.Int16);
                                    break;
                                case int i:
                                    Logging.Log("Writing int");
                                    w.Write((byte)Types.Int32);
                                    w.Write(i);
                                    typeswritten.Add(Types.Int32);
                                    break;
                                case long l:
                                    Logging.Log("Writing long");
                                    w.Write((byte)Types.Int64);
                                    w.Write(l);
                                    typeswritten.Add(Types.Int64);
                                    break;
                                case sbyte sb:
                                    Logging.Log("Writing sbyte");
                                    w.Write((byte)Types.SByte);
                                    w.Write(sb);
                                    typeswritten.Add(Types.SByte);
                                    break;
                                case float f:
                                    Logging.Log("Writing float");
                                    w.Write((byte)Types.Single);
                                    w.Write(f);
                                    typeswritten.Add(Types.Single);
                                    break;
                                case ushort us:
                                    Logging.Log("Writing ushort");
                                    w.Write((byte)Types.UInt16);
                                    w.Write(us);
                                    typeswritten.Add(Types.UInt16);
                                    break;
                                case uint ui:
                                    Logging.Log("Writing uint");
                                    w.Write((byte)Types.UInt32);
                                    w.Write(ui);
                                    typeswritten.Add(Types.UInt32);
                                    break;
                                case ulong ul:
                                    Logging.Log("Writing ulong");
                                    w.Write((byte)Types.UInt64);
                                    w.Write(ul);
                                    typeswritten.Add(Types.UInt64);
                                    break;
                                case Tuple<byte, ulong, long> T_item when T_item.Item2 == 0xF123ABCF:
                                    Logging.Log($"Processing special tuple with marker {T_item.Item1}");
                                    switch (T_item.Item1)
                                    {
                                        case 32 when T_item.Item3 <= int.MaxValue:
                                            Logging.Log("Writing 7-bit encoded int");
                                            w.Write((byte)Types.SevenBitEncodedInt);
                                            w.Write7BitEncodedInt((int)T_item.Item3);
                                            typeswritten.Add(Types.SevenBitEncodedInt);
                                            break;
                                        case 64:
                                            Logging.Log("Writing 7-bit encoded int64");
                                            w.Write((byte)Types.SevenBitEncodedInt64);
                                            w.Write7BitEncodedInt64(T_item.Item3);
                                            typeswritten.Add(Types.SevenBitEncodedInt64);
                                            break;
                                        default:
                                            throw new ArgumentException($"Expected 32 or 64 marker but got {T_item.Item1}");
                                    }
                                    break;
                                case System.Collections.IEnumerable enu when item is not string && item is not byte[] && item is not char[]:
                                    Logging.Log("Writing enumerable");
                                    List<object> items = enu.Cast<object>().ToList();
                                    w.Write((byte)Types.List);
                                    w.Write(items.Count);
                                    typeswritten.Add(Types.List);
                                    foreach (var subItem in items)
                                    {
                                        Logging.Log($"Serializing item of type {subItem.GetType()}");
                                        (var a, var b) = SerializeObject(subItem, -2, true);
                                        w.Write(a);
                                        typeswritten.AddRange(b);
                                    }
                                    break;
                                default:
                                    Logging.Log($"Unsupported type {item.GetType()}");
                                    throw new ArgumentException($"Unsupported type {item.GetType()}");
                            }
                        }
                        w.Flush();
                    }
                    Logging.Log("Memory stream write operation complete.");
                    Logging.Log(ms.ToArray());
                    return (ms.ToArray(), typeswritten);
                }
            }

            internal BinaryFileHandler(string filepath, bool? reading)
            {
                filename = filepath;
                FileAccess access = reading == true ? FileAccess.Read : reading == false ? FileAccess.Write : FileAccess.ReadWrite;
                fs = new(filepath, FileMode.Open, access);
                if (reading == true)
                    BinaryAccessStream = new BinaryReader(fs);
                else
                    BinaryAccessStream = new BinaryWriter(fs);
            }

            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            [LoggingAspects.RecordTime]
            internal List<Types> AddObject(object obj, int schema = -1, bool? skipreflect = false)
            {
                Logging.Log("Seeking end..");
                fs.Seek(0, SeekOrigin.End);
                Logging.Log($"End: {fs.Position}");
                if (!fs.CanRead)
                {
                    Logging.Log("ERROR: Cannot write to binary file in read mode!");
                    return [Types.Failed,];
                }
                Logging.Log($"Serializing {obj.GetType().FullName}...");
                (var stream, List<Types> typeswritten) = SerializeObject(obj, schema, skipreflect);
                BinaryWriter w = (BinaryWriter)BinaryAccessStream;
                Logging.Log($"Writing stream length {stream.Length}");
                w.Write(stream.Length);
                Logging.Log($"Writing stream...");
                w.Write(stream);
                Logging.Log("Write complete.");
                w.Flush();
                return typeswritten;
            }

            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            [LoggingAspects.RecordTime]
            internal List<Types> AddBareObjects(params object[] obj)
            {
                Logging.Log("Seeking end..");
                fs.Seek(0, SeekOrigin.End);
                Logging.Log($"End: {fs.Position}");
                if (reading == true)
                {
                    Logging.Log("ERROR: Cannot write to binary file in read mode!");
                    return [Types.Failed,];
                }
                List<Types> typeswritten = [];
                BinaryWriter w = (BinaryWriter)BinaryAccessStream;
                foreach (var obje in obj)
                {
                    Logging.Log($"Serializing {obje.GetType().FullName}...");
                    (var stream, List<Types> atypeswritten) = SerializeObject(obje, -1, true);
                    typeswritten.AddRange(atypeswritten);
                    Logging.Log($"Writing stream length {stream.Length}");
                    w.Write(stream.Length);
                    Logging.Log($"Writing stream...");
                    w.Write(stream);
                    Logging.Log("Write complete.");
                }
                w.Flush();
                return typeswritten;
            }

            internal List<object> ExtractObjectAtIndex(int index, bool schema = false)
            {
                if (!fs.CanRead)
                {
                    Interface.AddLog("Cannot read in write mode!");
                    Logging.Log("Set Stream to Read mode when trying to read a file!");
                    return [];
                }
                if (index < 0)
                    return [];
                int number = 0;
                BinaryReader r = BinaryAccessStream;
                fs.Seek(0, SeekOrigin.Begin);
                int ObjLength = ReadLength(r, fs.Length);
                if (ObjLength == -1)
                    return [];
                while (number != index)
                {
                    number++;
                    fs.Seek(ObjLength, SeekOrigin.Current);
                    ObjLength = ReadLength(r, fs.Length);
                    if (ObjLength == -1)
                        return [];
                }
                return schema ? ReadSchemaObject(fs.Position, ObjLength) : ReadObject(fs.Position, ObjLength);
            }

            private List<object> ReadObject(long position, int length)
            {
                List<List<object>> objects = [[]];
                int currentCollectionIndex = 0;
                List<int> remainingItems = [int.MaxValue];
                BinaryReader r = (BinaryReader)BinaryAccessStream;

                try
                {
                    fs.Seek(position, SeekOrigin.Begin);
                    objects[currentCollectionIndex].Add(r.ReadInt32());
                    long finalPosition = position + length;

                    while (fs.Position < finalPosition)
                    {
                        if (fs.Position + 1 > finalPosition)
                        {
                            throw new EndOfStreamException("Not enough data available to read the type identifier.");
                        }

                        byte type = r.ReadByte();
                        switch (type)
                        {
                            case (byte)Types.String:
                                objects[currentCollectionIndex].Add(r.ReadString());
                                break;
                            case (byte)Types.Boolean:
                                objects[currentCollectionIndex].Add(r.ReadBoolean());
                                break;
                            case (byte)Types.Byte:
                                objects[currentCollectionIndex].Add(r.ReadByte());
                                break;
                            case (byte)Types.Bytes:
                                int bytesLength = ReadLength(r, finalPosition);
                                objects[currentCollectionIndex].Add(r.ReadBytes(bytesLength));
                                break;
                            case (byte)Types.Char:
                                objects[currentCollectionIndex].Add(r.ReadChar());
                                break;
                            case (byte)Types.Chars:
                                int charsLength = ReadLength(r, finalPosition);
                                objects[currentCollectionIndex].Add(r.ReadChars(charsLength));
                                break;
                            case (byte)Types.Decimal:
                                objects[currentCollectionIndex].Add(r.ReadDecimal());
                                break;
                            case (byte)Types.Double:
                                objects[currentCollectionIndex].Add(r.ReadDouble());
                                break;
                            case (byte)Types.Int16:
                                objects[currentCollectionIndex].Add(r.ReadInt16());
                                break;
                            case (byte)Types.Int32:
                                objects[currentCollectionIndex].Add(r.ReadInt32());
                                break;
                            case (byte)Types.Int64:
                                objects[currentCollectionIndex].Add(r.ReadInt64());
                                break;
                            case (byte)Types.SByte:
                                objects[currentCollectionIndex].Add(r.ReadSByte());
                                break;
                            case (byte)Types.Single:
                                objects[currentCollectionIndex].Add(r.ReadSingle());
                                break;
                            case (byte)Types.UInt16:
                                objects[currentCollectionIndex].Add(r.ReadUInt16());
                                break;
                            case (byte)Types.UInt32:
                                objects[currentCollectionIndex].Add(r.ReadUInt32());
                                break;
                            case (byte)Types.UInt64:
                                objects[currentCollectionIndex].Add(r.ReadUInt64());
                                break;
                            case (byte)Types.SevenBitEncodedInt:
                                objects[currentCollectionIndex].Add(r.Read7BitEncodedInt());
                                break;
                            case (byte)Types.SevenBitEncodedInt64:
                                objects[currentCollectionIndex].Add(r.Read7BitEncodedInt64());
                                break;
                            case (byte)Types.List:
                                int listCount = ReadLength(r, finalPosition);
                                remainingItems.Add(listCount);
                                currentCollectionIndex++;
                                objects.Add([]);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("Unknown type encountered in binary file.");
                        }

                        if (remainingItems[currentCollectionIndex] > 0)
                        {
                            remainingItems[currentCollectionIndex]--;
                            if (remainingItems[currentCollectionIndex] == 0 && currentCollectionIndex > 0)
                            {
                                FinalizeCollection(objects, remainingItems, ref currentCollectionIndex);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogError(ex);
                    throw;
                }

                return objects[0];
            }

            private static void FinalizeCollection(List<List<object>> objects, List<int> remainingItems, ref int currentCollectionIndex)
            {
                objects[currentCollectionIndex - 1].Add(objects[currentCollectionIndex]);
                objects.RemoveAt(currentCollectionIndex);
                remainingItems.RemoveAt(currentCollectionIndex);
                currentCollectionIndex--;
            }

            private int ReadLength(BinaryReader r, long finalPosition)
            {
                if (fs.Position + 4 > finalPosition)
                { 
                    return -1;
                }
                return r.ReadInt32();
            }

            private bool ReadName(BinaryReader r, out string s)
            {
                try
                {
                    s = r.ReadString();
                }
                catch
                {
                    s = "";
                    return false;
                }
                return true;
            }

            internal bool FindObjectIndexByName(string name, out int index)
            {
                index = -1;
                if (!fs.CanRead)
                {
                    Interface.AddLog("Cannot read in write mode!");
                    Logging.Log("Set Stream to Read mode when trying to read a file!");
                    return false;
                }
                fs.Seek(0, SeekOrigin.Begin);
                BinaryReader r = (BinaryReader)BinaryAccessStream;
                int i = ReadLength(r, fs.Length);
                _ = ReadLength(r, fs.Length);
                long originalposition = fs.Position;
                while (i != -1)
                {
                    try
                    {
                        byte type = r.ReadByte();
                        Logging.Log($"Read byte: {type}");
                        if (type == (byte)Types.String)
                        {
                            bool b = ReadName(r, out string s);
                            if (!b || s == null)
                                continue;
                            Logging.Log($"Comparing {s} and {name}...");
                            if (s.Contains(name, StringComparison.InvariantCultureIgnoreCase))
                            {
                                ++index;
                                return true;
                            }
                        }
                    }
                    catch
                    {
                        return false;
                    }
                    index++;
                    fs.Seek(originalposition + i, SeekOrigin.Begin);
                    i = ReadLength(r, fs.Length);
                    _ = ReadLength(r, fs.Length);
                    originalposition = fs.Position;
                }
                return false;
            }

            public async void Dispose()
            {
                try
                {
                    await fs.FlushAsync();
                }
                catch { }
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    backup?.Dispose();
                    BinaryAccessStream?.Dispose();
                    fs?.Dispose();
                }
            }


            internal bool AddSchema(out int index, string name, params (Types, string)[] types) 
            {
                index = 0;
                if (filename != SchemaFile || !fs.CanWrite || !fs.CanRead)
                {
                    Interface.AddLog("Need to have read write perms to the schema file");
                    Logging.Log("Need to have read write perms to the schema file");
                    return false;
                }
                backup = new(fs, System.Text.Encoding.UTF8, true);
                fs.Seek(0, SeekOrigin.Begin);
                while (fs.Position < fs.Length)
                {
                    int len = ReadLength(backup, fs.Length);
                    fs.Seek(len, SeekOrigin.Current);
                    index++;
                }
                object[] owo = new object[(types.Length * 2) + 1];
                owo[0] = name;
                for (int i = 0; i < types.Length; i++)
                {
                    owo[i + 1] = types[i].Item1;
                    owo[i + 2] = types[i].Item2;
                }
                Logging.Log("OwO stuff: ", owo);
                fs.Seek(0, SeekOrigin.End);
                AddObject(owo, -2, null);
                return true;
            }

            internal List<object> ReadSchema(int index)
            {
                if (index < 0)
                {
                    return [];
                }
                return ExtractObjectAtIndex(index, true);
            }

            internal bool ReadSchema(string name, out List<object> objs)
            {
                bool b = FindObjectIndexByName(name, out int index);
                if (!b || index == -1)
                {
                    objs = [];
                    return false;
                }
                objs = ExtractObjectAtIndex(index, true);
                return true;
            }

            private List<object> ReadSchemaObject(long positon, int length)
            {
                BinaryReader r = BinaryAccessStream;
                List<object> values = new();
                fs.Seek(positon, SeekOrigin.Begin);
                values.Add(r.ReadString());
                while (fs.Position < length)
                    values.Add((r.ReadString(), (Types)r.ReadByte()));

                return values;
            }

            [LoggingAspects.Logging]
            internal Dictionary<string, dynamic> DeserialiseObject(List<object> data)
            {
                Logging.Log("Deserialising: ", data);
                var d = new Dictionary<string, dynamic>();
                if (data == null || data.Count < 1)
                    return d;
                if (data[0] is not int a)
                {
                    Logging.Log("ERROR: Int was not first object, assigning 1-base index numbers instead");
                    for (int i = 0; i < data.Count; i++)
                        d.Add((i + 1).ToString(), data[i]);
                    return d;
                }
                List<object> l = ReadSchema(a);
                if (l == null || l.Count < 1)
                {
                    Logging.Log($"ERROR: Failed to find schema at index {a} for object, assigning 1-base index numbers instead");
                    for (int i = 0; i < data.Count; i++)
                        d.Add((i + 1).ToString(), data[i]);
                    return d;
                }
                if (l.Count / 2 != data.Count)
                {
                    Logging.Log("ERROR: Data length and List length do not match!");
                }
                int j = 0;
                for (int i = 0; i < l.Count; i += 2)
                {
                    if (i + 1 > l.Count - 1)
                    {
                        Logging.Log("ERROR: Odd amount of values in schema reading, has to be an error as they're sorted in name-type pairs. Skipping (this will probably break the rest of the deserialisation).");
                        continue;
                    }
                    if (j > data.Count - 1)
                    {
                        Logging.Log("ERROR: Attempted to access index larger than data array's allocation! This... shouldn't happen. Are you using the right schema?");
                        break;
                    }
                    if (CheckTypeMatch((byte)l[i + 1], data[j]))
                        d.Add((string)l[i], data[j]);
                    j++;
                }
                return d;
            }
        }

        internal class ProcessSelector : Window
        {
            private SWC.ComboBox processComboBox;
            private TextBox searchTextBox;

            public int SelectedProcessId { get; private set; }

            public ProcessSelector()
            {
                InitializeComponents();
                PopulateProcesses();
                Topmost = true;
            }

            private void InitializeComponents()
            {
                Title = "Select a Process";
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
                    DisplayMemberPath = "ProcessName",
                    IsSynchronizedWithCurrentItem = true
                };
                processComboBox.SelectionChanged += ProcessComboBox_SelectionChanged;

                panel.Children.Add(searchTextBox);
                panel.Children.Add(processComboBox);
                Content = panel;
            }

            private void PopulateProcesses()
            {
                processComboBox.ItemsSource = Process.GetProcesses()
                    .GroupBy(p => p.ProcessName)
                    .Select(g => g.First())
                    .OrderBy(p => p.ProcessName)
                    .ToList();
            }

            private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
            {
                var filter = searchTextBox.Text.ToLower();
                processComboBox.ItemsSource = Process.GetProcesses()
                    .Where(p => p.ProcessName.ToLower().Contains(filter))
                    .GroupBy(p => p.ProcessName)
                    .Select(g => g.First())
                    .OrderBy(p => p.ProcessName)
                    .ToList();
            }

            private void ProcessComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (processComboBox.SelectedItem is Process selectedProcess)
                {
                    SelectedProcessId = selectedProcess.Id;
                }
            }

            protected override void OnClosed(EventArgs e)
            {
                base.OnClosed(e);
                if (processComboBox.SelectedItem == null)
                {
                    SelectedProcessId = -1;
                }
            }
        }
    }
}