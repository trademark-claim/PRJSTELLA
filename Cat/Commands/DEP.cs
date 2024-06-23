namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Downloads external packages or executes processes based on the provided command parameters.
        /// </summary>
        /// <returns>A Task&lt;bool&gt; indicating the success or failure of the operation.</returns>
        /// <remarks>
        /// Attempts to identify and execute a download or process execution based on the input parameters. Specific actions, such as downloading FFMPEG, are determined by the command argument.
        /// </remarks>
        [CAspects.AsyncExceptionSwallower]
        internal static async Task<bool> DEP()
        {
            if (commandstruct?.Parameters[0][0] is not string entry)
            {
                Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            switch (entry)
            {
                case "ffmpeg":
                    _ = new Helpers.EPManagement(Helpers.EPManagement.Processes.FFmpeg);
                    Logging.Log("DEP Execution Complete");
                    break;
                case "cat cursor" or "cat single":
                    Helpers.ExternalDownloading.FromGDrive(SingleCat, System.IO.Path.Combine(ExternalDownloadsFolder, "cat.ani"));
                    break;
                case "cat cursors" or "cat cursor preset" or "cat multiple":
                    Helpers.ExternalDownloading.FromGDrive(GatoZip, System.IO.Path.Combine(ExternalDownloadsFolder, "catz.zip"));
                    break;
                case "hzd" or "horizon zero dawn preset" or "hzd cursors" or "horizon cursors" or "hzd preset":
                    Helpers.ExternalDownloading.FromGDrive(HZDZip, System.IO.Path.Combine(ExternalDownloadsFolder, "hzd.zip"));
                    break;
                case "hearts" or "heart cursors" or "heart preset" or "hearts preset":
                    Helpers.ExternalDownloading.FromGDrive(HeartsZip, System.IO.Path.Combine(ExternalDownloadsFolder, "hearts.zip"));
                    break;
                case "copied city" or "audio sample" or "audio test file":
                    Helpers.ExternalDownloading.FromGDrive(CopiedCityMP3, System.IO.Path.Combine(ExternalDownloadsFolder, "copied_city.mp3"));
                    break;
                default:
                    Interface.AddLog("Unrecognised item name.\nItems: FFMPEG, cat single, cat multiple, hzd cursors, heart cursors, audio test file");
                    return false;
            }
            return true;
        }

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TDEP()
        {
            ClaraHerself.Fading = false;
            ClaraHerself.HaveOverlay = false;
            ClaraHerself.CleanUp = false;
            ClaraHerself.Custom = [
                "Command description:\n\""
            + (string)Interface.
                CommandProcessing
                .Cmds[Interface
                    .CommandProcessing
                    .cmdmap["download expr"]
                ]["desc"]
            + "\"",
            "This command downloads 'optionals', or 'external files' for me to use!",
            $"It downloads them to {ExternalDownloadsFolder}",
            "For example, lets download the simple cat cursor!",
            "We'll run 'download expr ;cat cursor'"
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Interface.Input = "download expr ;cat cursor";
            Interface.CommandProcessing.ProcessCommand();
            ClaraHerself.Custom = [
                $"Perfect!\nIf that worked, you should have a file called 'cat.ani' in the External Downloads folder ({ExternalDownloadsFolder}).",
                "Lets check!",
                ];
            await ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Catowo.inst.ToggleInterface(true, false);
            await Catowo.inst.UIToggleTCS.Task;
            await Task.Delay(200);
            var vks = ConvertStringToVKArray(ExternalDownloadsFolder);
            List<ExtendedInput> exis = [new ExtendedInput(VK_LWIN, 1), new BaselineInputs.ExtendedInput(VK_R),];
            exis.AddRange(vks.Select(k => new ExtendedInput(k, k == VK_LSHIFT ? (byte)1 : (byte)0)));
            exis.Add(new(VK_RETURN));
            SendKeyboardInput(75, [.. exis]);
            await BaselineInputs.KeyboardTCS.Task;
            ClaraHerself.Custom = [
                $"If the folder opened correctly, you'll see a file called cat.ani",
                "Congrats, it downloaded correctly!"
                ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            try
            {
                Logging.Log($"Focusing back to catowo canvas: {Catowo.inst.Focus()}");
                Logging.Log($"Focusing back to Clara bubble: {ClaraHerself.Bubble.Focus()}");
            }
            catch { }
            await ClaraHerself.TCS.Task;
            return;
        }
    }


}