using System.Drawing.Imaging;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Takes a screenshot based on the specified mode and saves it to a predetermined location.
        /// </summary>
        /// <returns>A Task&lt;bool&gt; indicating the success or failure of the screenshot operation.</returns>
        /// <remarks>
        /// Supports taking individual screenshots of each screen, a stitched screenshot of all screens, or a screenshot of a specific screen, based on the input parameter.
        /// </remarks>
        [CAspects.AsyncExceptionSwallower]
        [CAspects.Logging]
        internal static async Task<bool> Screenshot()
        {
            if (@interface != null) 
                await @interface.Hide();
            int? entryN = (int?)(commandstruct?.Parameters[0][0]);
            if (entryN == null)
            {
                Logging.Log("Expected int but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            int entry = entryN.Value;
            Logging.Log("Taking screenshots...");
            switch (entry)
            {
                case >= 0 when entry < System.Windows.Forms.Screen.AllScreens.Length:
                    {
                        Logging.Log($"Capturing screen {entry}");
                        Bitmap bmp = Helpers.Screenshotting.CaptureScreen(entry, out string? error);
                        if (error != "" && error != null)
                        {
                            Interface.AddTextLog(error, RED);
                            @interface.Show();
                            return false;
                        }
                        Logging.Log("Captured!");
                        string path = SSFolder + $"Shot{GUIDRegex().Replace(Guid.NewGuid().ToString(), "")}.png";
                        bmp.Save(path, ImageFormat.Png);
                        bmp.Dispose();
                        Interface.AddLog("Screenshot saved!");
                        Logging.Log($"Shot saved to {path}");
                        break;
                    }

                case -1:
                    {
                        Logging.Log("Capturing all screens, individual mode");
                        List<Bitmap> bmps = Helpers.Screenshotting.AllIndivCapture(out var errors);
                        errors?.RemoveAll(x => x == null);
                        if (errors != null && errors?.Count > 0)
                        {
                            for (int i = 0; i < errors.Count; i++)
                            {
                                string? error = errors[i];
                                if (error != null)
                                    Interface.AddTextLog($"Error when shooting screen {i}" + error, RED);
                                Logging.Log(error == null ? "no error" : error);
                            }
                            @interface.Show();
                            Logging.Log("Exiting Screenshotting due to errors.");
                            return false;
                        }
                        Logging.Log($"{bmps.Count} shots taken!");
                        for (int i = 0; i < bmps.Count; i++)
                        {
                            Bitmap bmp = bmps[i];
                            string path = SSFolder + $"IShot{i}{GUIDRegex().Replace(Guid.NewGuid().ToString(), "")}.png";
                            bmp.Save(path, ImageFormat.Png);
                            Logging.Log($"Saved shot {i} to {path}");
                            bmp.Dispose();
                        }
                        Interface.AddLog("Screenshots saved!");
                        break;
                    }

                case -2:
                    {
                        Logging.Log("Capturing all screens, stitch mode");
                        Bitmap bmp = Helpers.Screenshotting.StitchCapture(out var error);
                        if (error != "" && error != null)
                        {
                            Logging.Log(error);
                            Interface.AddTextLog(error, RED);
                            @interface.Show();
                            return false;
                        }
                        Logging.Log("Captured!");
                        string path = SSFolder + $"SShot{GUIDRegex().Replace(Guid.NewGuid().ToString(), "")}.png";
                        bmp.Save(path, ImageFormat.Png);
                        bmp.Dispose();
                        Interface.AddLog("Screenshot saved!");
                        Logging.Log($"Shot saved to {path}");
                        break;
                    }

                default:
                    {
                        string str = $"Expected arg1 value within -2 to {System.Windows.Forms.Screen.AllScreens.Length}";
                        Interface.AddTextLog(str, LIGHTRED);
                        Logging.Log(str);
                        @interface.Show();
                        return false;
                    }
            }
            @interface?.Show();
            return true;
        }
    }
}