// -----------------------------------------------------------------------
// Helpers.cs
// Contains utility methods for screenshotting, screen recording,
// managing configurations through INI files, and other helper functions.
// Author: Nexus
// -----------------------------------------------------------------------


using IniParser;
using IniParser.Model;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Cat
{
    /// <summary>
    /// Provides static utility methods and classes for various operations like
    /// screenshotting, screen recording, INI file parsing, and more.
    /// </summary>
    internal static class Helpers
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
                    return new Bitmap(1, 1);
                }
                var screen = Screen.AllScreens[screenIndex];
                var bounds = screen.Bounds;
                IntPtr desktopDC = GetWindowDCWrapper(GetDesktopWindowWrapper());
                IntPtr memoryDC = CreateCompatibleDCWrapper(desktopDC);
                IntPtr bitmap = CreateCompatibleBitmapWrapper(desktopDC, bounds.Width, bounds.Height);
                IntPtr oldBitmap = SelectObjectWrapper(memoryDC, bitmap);
                Logging.Log($"desktopDC: {desktopDC}", $"memoryDC: {memoryDC}");
                BitBltWrapper(memoryDC, 0, 0, bounds.Width, bounds.Height, desktopDC, bounds.X, bounds.Y, PInvoke.CopyPixelOperation.SourceCopy);
                SelectObjectWrapper(memoryDC, oldBitmap);
                Bitmap bmp = Image.FromHbitmap(bitmap);
                DeleteObjectWrapper(bitmap);
                ReleaseDCWrapper(GetDesktopWindowWrapper(), desktopDC);
                DeleteDCWrapper(memoryDC);
                Logging.Log("Exiting helper method Screenshotting.CaptureScreen()");
                return bmp;
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
                        g.Clear(Color.Transparent);
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
                    Logging.Log(">>>ERROR<<<", "Message: ", ex.Message, "Stacktrace: ", ex.StackTrace, "Inner Exception: ", ex.InnerException, "Data: ", ex.Data, "Source: ", ex.Source, "Help link: ", ex.HelpLink, "HResult: ", ex.HResult, $"TargetSite: {ex.TargetSite?.Module}.{ex.TargetSite?.DeclaringType}.{ex.TargetSite?.Name}");
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
        /// Contains methods for starting and stopping screen recording sessions.
        /// </summary>
        internal static class ScreenRecording
        {
            private static bool _isRecording = false;
            private static Thread _recordingThread;
            private static int _frameRate = 30;

            /// <summary>
            /// Starts recording the specified screen to the given output path using FFMPEG.
            /// </summary>
            /// <param name="screenIndex">The index of the screen to record.</param>
            /// <param name="outputPath">The file path where the recording will be saved.</param>
            /// <remarks>
            /// This method begins a new thread for the recording process and utilizes FFMPEG for capturing the screen.
            /// </remarks>
            [LoggingAspects.Logging]
            public static void StartRecording(int screenIndex, string outputPath)
            {
                if (_isRecording) return;

                _isRecording = true;
                _recordingThread = new Thread(() => RecordScreen(screenIndex, outputPath));
                _recordingThread.Start();
            }

            /// <summary>
            /// Stops the ongoing screen recording.
            /// </summary>
            /// <remarks>
            /// This method signals the recording thread to stop and waits for it to finish.
            /// </remarks>
            [LoggingAspects.Logging]
            public static void StopRecording()
            {
                _isRecording = false;
                _recordingThread?.Join();
            }

            /// <summary>
            /// Captures the screen at the specified index and writes the video frames to the output path using FFMPEG.
            /// </summary>
            /// <param name="screenIndex">The index of the screen to capture.</param>
            /// <param name="outputPath">The file path to save the recorded video.</param>
            /// <remarks>
            /// This method captures the screen in a loop until recording is stopped. It captures screen frames and feeds them to FFMPEG.
            /// </remarks>
            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            [LoggingAspects.InterfaceNotice]
            private static void RecordScreen(int screenIndex, string outputPath)
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = FFMPEGPath,
                    Arguments = $"-y -f rawvideo -pixel_format rgb24 -video_size {Screen.AllScreens[screenIndex].Bounds.Width}x{Screen.AllScreens[screenIndex].Bounds.Height} -framerate {_frameRate} -i - -vf format=yuv420p -c:v libx264 \"{outputPath}\"",
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process ffmpeg = Process.Start(psi))
                {
                    while (_isRecording)
                    {
                        string? errorMessage;
                        Bitmap frame = Screenshotting.CaptureScreen(screenIndex, out errorMessage);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Logging.Log($"CaptureScreen error: {errorMessage}");
                            continue;
                        }

                        byte[]? imageBytes = BitmapToBytes(frame);

                        ffmpeg.StandardInput.BaseStream.Write(imageBytes, 0, imageBytes.Length);
                        ffmpeg.StandardInput.BaseStream.Flush();

                        Thread.Sleep(1000 / _frameRate);
                    }

                    ffmpeg.StandardInput.Close();
                    ffmpeg.WaitForExit();
                }

                Logging.Log("Recording stopped.");
            }

            /// <summary>
            /// Converts a Bitmap image to a byte array.
            /// </summary>
            /// <param name="image">The Bitmap image to convert.</param>
            /// <returns>A byte array representing the Bitmap image.</returns>
            /// <remarks>
            /// This method is used to convert screen frames into a byte array for processing or storage.
            /// </remarks>
            [LoggingAspects.Logging]
            private static byte[]? BitmapToBytes(Bitmap image)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    image.Save(stream, ImageFormat.Bmp);
                    return stream.ToArray();
                }
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

        /// <summary>
        /// Manages the downloading and installation of FFMpeg.
        /// </summary>
        public static class FFMpegManager
        {
            private const string DownloadUrl = "https://www.gyan.dev/ffmpeg/builds/packages/ffmpeg-2024-03-25-git-ecdc94b97f-essentials_build.7z";
            private static Logging.ProgressLogging OverallProgress = new Logging.ProgressLogging("Overall FFMPEG Install:", true);
            private static Logging.ProgressLogging SectionProgress = new Logging.ProgressLogging("Downloading FFMPEG from Gyan.dev:", true);

            /// <summary>
            /// Checks if the FFMpeg executable exists in the predefined directory.
            /// </summary>
            /// <returns>A boolean value indicating the existence of the FFMpeg executable.</returns>
            [LoggingAspects.Logging]
            public static bool CheckFFMPEGExistence()
            {
                Logging.Log("Checking if FFMpeg.exe exists in allocated directory.");
                bool exists = File.Exists(FFMPEGPath);
                Logging.Log($"FFMpeg existence check returned {exists}");
                return exists;
            }

            /// <summary>
            /// Downloads and extracts FFMPEG from a remote server if it does not already exist locally.
            /// </summary>
            /// <remarks>
            /// This method asynchronously downloads a compressed file containing FFMPEG from a predefined URL,
            /// extracts it, and places the executable in a designated directory.
            /// </remarks>
            [LoggingAspects.Logging]
            [LoggingAspects.AsyncExceptionSwallower]
            [LoggingAspects.InterfaceNotice]
            public static async Task DownloadFFMPEG()
            {
                if (!CheckFFMPEGExistence())
                {
                    Logging.Log("Starting FFMPEG download...");
                    HttpClient client = new HttpClient();

                    try
                    {
                        using (var response = await client.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                        {
                            response.EnsureSuccessStatusCode();
                            var totalBytes = response.Content.Headers.ContentLength ?? 0;
                            Logging.Log("Downloading FFMPEG...");

                            using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
                            {
                                string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".7z");
                                Logging.Log($"Temporary Download path: {tempPath}");

                                using (var streamToWriteTo = File.Open(tempPath, FileMode.Create))
                                {
                                    await CopyContentAsync(streamToReadFrom, streamToWriteTo, totalBytes);
                                }

                                Logging.Log("FFMPEG downloaded, extracting...");
                                await Extract7zArchiveAsync(tempPath);
                                File.Delete(tempPath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError(ex);
                        Logging.Log("Failed to download or extract FFMPEG.");
                    }
                }
                else
                {
                    Logging.Log("FFMpeg.exe already exists, skipping download.");
                }
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
            private static async Task CopyContentAsync(Stream source, Stream destination, long totalBytes)
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

            /// <summary>
            /// Extracts the downloaded FFMPEG archive and moves the executable to the correct directory.
            /// </summary>
            /// <param name="archivePath">The path to the downloaded FFMPEG archive.</param>
            /// <remarks>
            /// After downloading the FFMPEG archive, this method extracts the contents and moves the FFMPEG
            /// executable to a predefined location for future use.
            /// </remarks>
            [LoggingAspects.Logging]
            [LoggingAspects.AsyncExceptionSwallower]
            [LoggingAspects.InterfaceNotice]
            private static async Task Extract7zArchiveAsync(string archivePath)
            {
                Logging.Log($"Extracting {archivePath} to {ExternalProcessesFolder}");
                SectionProgress = new Logging.ProgressLogging("Extracting FFMPEG:", true);
                var loader = new Logging.ProgressLogging.SpinnyThing();
                try
                {
                    if (!Directory.Exists(ExternalProcessesFolder))
                    {
                        Directory.CreateDirectory(ExternalProcessesFolder);
                    }

                    using (var archive = SharpCompress.Archives.SevenZip.SevenZipArchive.Open(archivePath))
                    {
                        var totalEntries = archive.Entries.Count();
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
                    string extractedFolderPath = Path.Combine(ExternalProcessesFolder, "ffmpeg-2024-03-25-git-ecdc94b97f-essentials_build", "bin");
                    string extractedFFmpegPath = Path.Combine(extractedFolderPath, "ffmpeg.exe");
                    string finalPath = Path.Combine(ExternalProcessesFolder, "ffmpeg.exe");

                    if (File.Exists(extractedFFmpegPath))
                    {
                        File.Move(extractedFFmpegPath, finalPath, true);
                        Logging.Log($"ffmpeg.exe moved to {finalPath}.");
                        Interface.AddLog("FFMPeg " + Helpers.BackendHelping.Glycemia("Complete"));
                        Directory.Delete(extractedFolderPath, true);
                    }
                    else
                    {
                        Logging.Log($"ffmpeg.exe not found in expected path: {extractedFFmpegPath}");
                    }
                    loader.Stop();
                    Logging.Log("Extraction and file movement completed successfully.");
                }
                catch (Exception ex)
                {
                    Logging.Log($"Error during extraction or file movement: {ex.Message}");
                    Logging.LogError(ex);
                }

                SectionProgress.InvokeEvent(new(100));
                OverallProgress.InvokeEvent(new(100));
            }
        }

        /// <summary>
        /// Provides miscellaneous backend helper functions.
        /// </summary>
        public static class BackendHelping
        {
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
                { "LaunchAsAdmin", (typeof(bool), false) }
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
                    "Misc", new() {
                        ("Startup", true),
                        ("AllowRegistryEdits", false),
                        ("LaunchAsAdmin", false)
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
                IniData data= new();
                foreach (string key in initalsettings.Keys)
                {
                    if (data[key] == null)
                        data.Sections.AddSection(key);
                    foreach((string innerkey, object value) in initalsettings[key])
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
    }
}