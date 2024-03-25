using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Cat
{
    internal static class Helpers
    {
        internal static class Screenshotting
        {
            internal static Bitmap CaptureScreen(int screenIndex, out string? error_message)
            {
                Logging.Log($"Entering Helper method: Screenshotting.CaptureScreen() with params {screenIndex}");
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
            internal static (double Width, double Height, double WorkingAreaHeight) GetAdjustedScreenSize(Screen screen)
            {
                var dpiX = GetSystemDpi("DpiX");
                var dpiY = GetSystemDpi("Dpi");
                double screenWidth = screen.Bounds.Width / dpiX;
                double screenHeight = screen.Bounds.Height / dpiY;
                double workAreaHeight = screen.WorkingArea.Height / dpiY;

                return (screenWidth, screenHeight, workAreaHeight);
            }

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

            public static void StartRecording(int screenIndex, string outputPath)
            {
                if (_isRecording) return;

                _isRecording = true;
                _recordingThread = new Thread(() => RecordScreen(screenIndex, outputPath));
                _recordingThread.Start();
            }
            public static void StopRecording()
            {
                _isRecording = false;
                _recordingThread?.Join(); 
            }

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

            private static byte[]? BitmapToBytes(Bitmap image)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    image.Save(stream, ImageFormat.Bmp);
                    return stream.ToArray();
                }
            }
        }

    }
}
