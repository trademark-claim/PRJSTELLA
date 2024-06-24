namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Downloads external packages based on the provided parameter.
        /// </summary>
        /// <returns>A Task&lt;bool&gt; indicating the success or failure of the operation.</returns>
        [CAspects.AsyncExceptionSwallower]
        [CAspects.Logging]
        internal static async Task<bool> DEP()
        {
            if (commandstruct?.Parameters[0][0] is not string para1)
            {
                Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            switch (para1)
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
            StellaHerself.Fading = false;
            StellaHerself.HaveOverlay = false;
            StellaHerself.CleanUp = false;
            StellaHerself.Custom = [
                "Command description:\n\""
            + Interface.
                CommandProcessing
                .Cmds[Interface
                    .CommandProcessing
                    .cmdmap["download expr"]
                ].desc
            + "\"",
            "This command downloads 'optionals', or 'external files' for me to use!",
            $"It downloads them to {ExternalDownloadsFolder}",
            "For example, lets download the simple cat cursor!",
            "We'll run 'download expr ;cat cursor'"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var continu = await StellaHerself.TCS.Task;
            if (!continu) return;
            Interface.Input = "download expr ;cat cursor";
            Interface.CommandProcessing.ProcessCommand();
            StellaHerself.Custom = [
                $"Perfect!\nIf that worked, you should have a file called 'cat.ani' in the External Downloads folder ({ExternalDownloadsFolder}).",
                "Lets check!",
                ];
            await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            continu = await StellaHerself.TCS.Task;
            if (!continu) return;
            Catowo.inst.ToggleInterface(true, false);
            await Catowo.inst.UIToggleTCS.Task;
            await Task.Delay(200);
            var virtual_keys = ConvertStringToVKArray(ExternalDownloadsFolder);
            List<ExtendedInput> xtended_inputs = [new ExtendedInput(VK_LWIN, 1), new BaselineInputs.ExtendedInput(VK_R),];
            xtended_inputs.AddRange(virtual_keys.Select(k => new ExtendedInput(k, k == VK_LSHIFT ? (byte)1 : (byte)0)));
            xtended_inputs.Add(new(VK_RETURN));
            SendKeyboardInput(75, [.. xtended_inputs]);
            await BaselineInputs.KeyboardTCS.Task;
            StellaHerself.Custom = [
                $"If the folder opened correctly, you'll see a file called cat.ani",
                "Congrats, it downloaded correctly!"
                ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            try
            {
                Logging.Log($"Focusing back to catowo canvas: {Catowo.inst.Focus()}");
                Logging.Log($"Focusing back to Stella bubble: {StellaHerself.Bubble.Focus()}");
            }
            catch { }
            await StellaHerself.TCS.Task;
            return;
        }
    }


}