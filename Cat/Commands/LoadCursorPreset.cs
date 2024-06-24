using Microsoft.VisualBasic;
using NAudio.Utils;
using System.IO;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Loads an entire cursor preset
        /// </summary>
        /// <returns>true if successful at all, false otherwise</returns>
        [CAspects.ConsumeException]
        [CAspects.Logging]
        internal static bool LoadCursorPreset()
        {
            if (commandstruct.Value.Parameters[0][0] is not string para1)
            {
                Logging.Log(["Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report."]);
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            string dir = Environment.CursorsFilePath + para1;
            if (!Directory.Exists(dir))
            {
                Logging.Log([$"Directory {dir} not found."]);
                Interface.AddTextLog($"No preset with name {para1} found", RED);
                return false;
            }
            string file = dir + "\\preset.CLF";
            Logging.Log(["Preset file: " + file]);
            if (!File.Exists(file))
            {
                Logging.Log([$"Cursor Preset file ({file}) not found! This shouldn't happen unless someone manually removed it."]);
                Interface.AddTextLog("Preset file not found, try re-creating this preset! (Remove the currently existing one though).", RED);
                return false;
            }
            bool persistant = false;
            if (commandstruct.Value.Parameters[1].Length > 0)
            {
                persistant = commandstruct.Value.Parameters[1][0] as bool? ?? false;
                if (persistant == true && !UserData.AllowRegistryEdits)
                {
                    Interface.AddLog("For persistant cursor changes (cursors remain custom through computer restarts), please change the AllowRegistryEdits to true!");
                    Interface.AddLog("Loading preset without persistence.");
                    Logging.Log(["Requested registry edit without permissions."]);
                    persistant = false;
                }
                else if (persistant == true && !Helpers.BackendHelping.CheckIfAdmin())
                {
                    Interface.AddLog("This program requires elevation to change registry! Either set LaunchAsAdmin to true and restart or run 'elevate perms'");
                    persistant = false;
                }
            }
            string[] content = File.ReadAllLines(file);
            Logging.Log([content]);
            foreach (string line in content)
            {
                uint id = 0;
                string[] split = line.Split("|");
                string cursorname = split[0].Trim();
                switch (cursorname)
                {
                    case "OCR_APPSTARTING":
                        id = OCR_APPSTARTING;
                        break;

                    case "OCR_NORMAL":
                        id = OCR_NORMAL;
                        break;

                    case "OCR_CROSS":
                        id = OCR_CROSS;
                        break;

                    case "OCR_HAND":
                        id = OCR_HAND;
                        break;

                    case "OCR_HELP":
                        id = OCR_HELP;
                        break;

                    case "OCR_IBEAM":
                        id = OCR_IBEAM;
                        break;

                    case "OCR_UNAVAILABLE":
                        id = OCR_UNAVAILABLE;
                        break;

                    case "OCR_SIZEALL":
                        id = OCR_SIZEALL;
                        break;

                    case "OCR_SIZENESW":
                        id = OCR_SIZENESW;
                        break;

                    case "OCR_SIZENS":
                        id = OCR_SIZENS;
                        break;

                    case "OCR_SIZENWSE":
                        id = OCR_SIZENWSE;
                        break;

                    case "OCR_SIZEWE":
                        id = OCR_SIZEWE;
                        break;

                    case "OCR_UP":
                        id = OCR_UP;
                        break;

                    case "OCR_WAIT":
                        id = OCR_WAIT;
                        break;

                    default:
                        Logging.Log([$"Unknown cursor name in preset.CLF: {cursorname}, skipping and moving to next cursor."]);
                        Interface.AddTextLog($"Unknown cursor name in preset.CLF: {cursorname}, skipping and moving to next cursor.", HOTPINK);
                        continue;
                }
                if (id != 0)
                {
                    string path = split[1].Trim();
                    if (!File.Exists(path))
                    {
                        Logging.Log([$"Cursor at {path} does not exist! Skipping."]);
                        Interface.AddTextLog($"Cursor path assigned to {cursorname} returned non-existence, skipping.", HOTPINK);
                        continue;
                    }
                    if (!BaselineInputs.Cursor.ChangeCursor(path, id, persistant))
                    {
                        Interface.AddTextLog($"Something went wrong loading the cursor for {cursorname}! Check logs for details.\nSkipping cursor for {cursorname}...", HOTPINK);
                        continue;
                    }
                    Logging.Log([$"Cursor for {cursorname} successfully changed!"]);
                }
            }
            Interface.AddLog($"{para1} cursor preset loaded!");
            return true;
        }

        /// <summary>
        /// Tutorial for the load cursor preset command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TLoadCursorPreset()
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
                    .cmdmap["lcp"]
                ].desc
            + "\"",
            "This command takes in two parameters.",
            "The former parameter is the preset to load, and the latter is a persistance flag.",
            "Lets do the first parameter!"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var continu = await StellaHerself.TCS.Task;
            if (!continu) return;
            Interface.Input = "load cursor preset";
            string dir = "cats";
            if (Directory.GetDirectories(CursorsFilePath).Length < 1)
            {
                StellaHerself.Custom = [
                "It seems you dont have any presets made!",
                "I'll download one for you -- one full of cat cursors!", 
                "If you don't want me to do this, press the up arrow and use the 'add cursor preset' and 'add cursor to preset' commands to make a preset to load!, else, press the right arrow and I'll get right to it!"
                ];
                await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
                continu = await StellaHerself.TCS.Task;
                if (!continu)
                    return;
                string path = Path.Combine(ExternalDownloadsFolder, "cats.zip");
                Helpers.ExternalDownloading.FromGDrive(GatoZip, path);
                await Helpers.ExternalDownloading.TCS.Task;
                Interface.AddLog("Unzipping...");
                var spin = new Logging.ProgressLogging.SpinnyThing();
                Helpers.ExternalDownloading.UnzipFile(path, CursorsFilePath);
                await Helpers.ExternalDownloading.TCS.Task;
                spin.Stop();
                StellaHerself.Custom = ["The cats have been created, moving on!"];
                await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
                continu = await StellaHerself.TCS.Task;
                if (!continu)
                    return;
            }
            else
            {
                StellaHerself.Custom = [
                    "Please select one of your existing presets!",
                ];
                await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
                continu = await StellaHerself.TCS.Task;
                if (!continu)
                    return;
                var bs = new Objects.BoxSelecter<string>([.. Directory.GetDirectories(CursorsFilePath).Select(item => item.Replace(CursorsFilePath, ""))], "Choose preset:");
                bs.ShowDialog();
                dir = bs.SelectedItem;
            }
            StellaHerself.Custom = [
                    $"So, the {dir} preset, perfect!",
                    "Now we have to walk through the second parameter, persistance.",
                    "This parameter is optional, and defaults to 'false'.",
                    "It changes the registry linking for the cursors to the custom cursor file path in order to make the changes persist through system restarts",
                    "Or, in simple talk: Your custom cursors stay activated even if you power off your computer",
                    "But in order for this to work, you need to explicitly allow me to modify your registry by elevating my permissions (using the 'elevate perms' command) and changing my 'Allow registry edits' setting to true (using the 'change settings' command).",
                    "For now though, we'll have them non-persistant, so we don't need to input the parameter!",
                    $"Now, Lets load the {dir} preset using 'load cursor preset ;{dir}'"
                ];
            await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            continu = await StellaHerself.TCS.Task;
            if (!continu) return;
            Interface.Input = $"list cursor preset ;{dir}";
            Interface.CommandProcessing.ProcessCommand();
            StellaHerself.Custom = [
                    "Your cursors should now be the ones from within the preset, have fun!",
                ];
            await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            await StellaHerself.TCS.Task;
        }
    }
}