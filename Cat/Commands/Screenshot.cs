using System.Drawing.Imaging;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Takes a screenshot based on the specified mode and saves it.
        /// </summary>
        /// <returns>A Task&lt;bool&gt; indicating the success or failure of the screenshot.</returns>
        /// <remarks>
        /// Can take individual screenshots of each screen, a stitched screenshot of all screens, or a screenshot of a specific screen, based on the input parameter.
        /// </remarks>
        [CAspects.AsyncExceptionSwallower]
        [CAspects.Logging]
        internal static async Task<bool> Screenshot()
        {
            if (@interface != null) 
                await @interface?.Hide();
            if (commandstruct?.Parameters[0][0] is not int para1)
            {
                Logging.Log("Expected int but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }

            Logging.Log("Taking screenshots...");
            switch (para1)
            {
                case >= 0 when para1 < Screen.AllScreens.Length:
                    {
                        Logging.Log($"Capturing screen {para1}");
                        Bitmap bmp = Helpers.Screenshotting.CaptureScreen(para1, out string? error);
                        if (error != "" && error != null)
                        {
                            Interface.AddTextLog(error, RED);
                            @interface?.Show();
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
                                Logging.Log(error ?? "no error");
                            }
                            @interface?.Show();
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

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TScreenshot()
        {
            StellaHerself.Fading = false;
            StellaHerself.HaveOverlay = false;
            StellaHerself.CleanUp = false;
            StellaHerself.Custom = [
                "Command description:\n\""
            + Interface.
                CommandProcessing
                .Cmds[Interface
                    .CommandProcessing
                    .cmdmap["screenshot"]
                ].desc
            + "\"",
            "This is my screenshot functionality!\n It takes in one parameter, the capture mode, an integer.",
            "If the value is:",
            "-2, it will take a screenshot of your entire setup, all monitor screens, and stitch them together according to their size and location",
            "-1, it will take individual screenshots of every connected monitor and save them individually",
            "any positive number, it will attempt to find the screen at that index, and if found, will take a screenshot of that screen.",
            "The screenshots are of the DPI, Scaling and resolution of the monitor they're taken on, as they copy it pixel for pixel (1:1 scaling)",
            "Regardless of this very tedious process, it's extremely fast, so don't worry!",
            "The interface, if open, will also close automatically for the screenshot.",
            "Lets take a stitched one by running 'screenshot ;-2'"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await StellaHerself.TCS.Task;
            if (!b) return;
            await Helpers.BackendHelping.EnsureCompletion(Interface.CommandProcessing.ProcessCommand, ["screenshot ;-2"], finishdelayms: 200).Task;
            StellaHerself.Custom = [
                "There, screenshot taken!",
                $"It'll be located at {SSFolder}, lemme open it for ya!"
                ];
            b = await StellaHerself.TCS.Task;
            if (!b) return;
            Catowo.inst.ToggleInterface(true, false);
            await Catowo.inst.UIToggleTCS.Task;
            await Task.Delay(200);
            Console.WriteLine("I dont know why this line makes it work, but it does.");
            var vks = ConvertStringToVKArray(SSFolder);
            List<ExtendedInput> exis = [new ExtendedInput(VK_LWIN, 1), new BaselineInputs.ExtendedInput(VK_R),];
            exis.AddRange(vks.Select(k => new ExtendedInput(k, k == VK_LSHIFT ? (byte)1 : (byte)0)));
            exis.Add(new(VK_RETURN));
            SendKeyboardInput(75, [.. exis]);
        }
    }
}