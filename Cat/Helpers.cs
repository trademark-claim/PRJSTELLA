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
using static Cat.Catowo;

namespace Cat
{
    internal static class Helpers
    {
        internal static class Screenshotting
        {
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

            [LoggingAspects.Logging]
            [LoggingAspects.ConsumeException]
            [LoggingAspects.InterfaceNotice]
            internal static Bitmap StitchCapture(out string? error_message)
            {
                Logging.Log("Entering helper Screenshotting.StitchCapture()");
                error_message = null;

                var totalBounds = Screen.AllScreens.Select(s => s.Bounds).Aggregate(Rectangle.Union);
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

        internal static class ScreenSizing
        {
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

        internal static class ScreenRecording
        {
            private static bool _isRecording = false;
            private static Thread _recordingThread;
            private static int _frameRate = 30;

            [LoggingAspects.Logging]
            public static void StartRecording(int screenIndex, string outputPath)
            {
                if (_isRecording) return;

                _isRecording = true;
                _recordingThread = new Thread(() => RecordScreen(screenIndex, outputPath));
                _recordingThread.Start();
            }

            [LoggingAspects.Logging]
            public static void StopRecording()
            {
                _isRecording = false;
                _recordingThread?.Join();
            }

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

        public class CatWindow : Window
        {
            private readonly HttpClient _client = new HttpClient();
            private readonly SWC.Image _imageControl = new SWC.Image();
            private Logging.ProgressLogging Progress = new("Cat Window Image Download:", true);
            private Logging.SpinnyThing spinnything;

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

        public static class FFMpegManager
        {
            private const string DownloadUrl = "https://www.gyan.dev/ffmpeg/builds/packages/ffmpeg-2024-03-25-git-ecdc94b97f-essentials_build.7z";
            private static Logging.ProgressLogging OverallProgress = new Logging.ProgressLogging("Overall FFMPEG Install:", true);
            private static Logging.ProgressLogging SectionProgress = new Logging.ProgressLogging("Downloading FFMPEG from Gyan.dev:", true);

            [LoggingAspects.Logging]
            public static bool CheckFFMPEGExistence()
            {
                Logging.Log("Checking if FFMpeg.exe exists in allocated directory.");
                bool exists = File.Exists(FFMPEGPath);
                Logging.Log($"FFMpeg existence check returned {exists}");
                return exists;
            }

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

            [LoggingAspects.Logging]
            [LoggingAspects.AsyncExceptionSwallower]
            [LoggingAspects.InterfaceNotice]
            private static async Task Extract7zArchiveAsync(string archivePath)
            {
                Logging.Log($"Extracting {archivePath} to {ExternalProcessesFolder}");
                SectionProgress = new Logging.ProgressLogging("Extracting FFMPEG:", true);
                var loader = new Logging.SpinnyThing();
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

        public static class BackendHelping
        {
            internal static string Glycemia(string word)
            {
                if (word.Length <= 2) return word;
                var middle = word[1..^1].ToCharArray();
                random.Shuffle(middle);
                return word[0] + new string(middle) + word[^1];
            }

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
        }

        internal static class ProgressTesting
        {
            [LoggingAspects.Logging]
            [LoggingAspects.AsyncExceptionSwallower]
            internal static async void GenerateProgressingTest()
            {
                uint rnd = (uint)random.Next(int.MaxValue);
                Logging.ProgressLogging plog = new($"Progress test {rnd}:", true);
                var spin = new Logging.SpinnyThing();
                byte progress = 0;
                while (progress < 100)
                {
                    await Task.Delay(random.Next(1, 100));
                    plog.InvokeEvent(new(++progress));
                }
                spin.Stop();
            }
        }

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
                { "LoggingDetails", (typeof(bool), false) }
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
                        ("Startup", true)
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