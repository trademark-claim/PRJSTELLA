using System.IO;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Adds a cursor to a preset
        /// </summary>
        /// <returns>true if successful at all, false otherwise</returns>
        [CAspects.ConsumeException]
        [CAspects.Logging]
        internal static bool AddCursorToPreset()
        {
            string entryN = commandstruct.Value.Parameters[0][0] as string,
                entryM = commandstruct.Value.Parameters[0][1] as string,
                entryZ = commandstruct.Value.Parameters[0][2] as string;
            if (entryN == null || entryM == null || entryZ == null)
            {
                Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Catowo.Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            entryN = entryN.Trim();
            entryM = entryM.Trim();
            entryZ = entryZ.Trim().Replace("\"", "");
            string dir = Path.Combine(Environment.CursorsFilePath, entryN);
            if (!Directory.Exists(dir))
            {
                Logging.Log($"Directory {dir} not found.");
                Catowo.Interface.AddTextLog($"No preset with name {entryN} found", RED);
                return false;
            }
            string file = Path.Combine(dir, "preset.CLF");
            Logging.Log("Preset file: " + file);
            if (!File.Exists(file))
            {
                Logging.Log($"Cursor Preset file ({file}) not found! This shouldn't happen unless someone manually removed it.");
                Catowo.Interface.AddTextLog("Preset file not found, try re-creating this preset! (Remove the currently existing one though).", RED);
                return false;
            }
            if (!File.Exists(entryZ))
            {
                Logging.Log($"Requested file {entryZ} not found.");
                Catowo.Interface.AddTextLog($"Requested file {entryZ} not found! Please verify your file and provide a full, absolute file path.", RED);
                return false;
            }
            if (!entryZ.EndsWith(".ani") && !entryZ.EndsWith(".cur"))
            {
                Logging.Log($"Requested file {entryZ} not a .ani or .cur file.");
                Catowo.Interface.AddTextLog($"Requested file {entryZ} was not a supported cursor file! (.ani / .cur).", RED);
                return false;
            }
            bool animated = entryZ.EndsWith(".ani");
            entryM = entryM.ToUpper().Trim();
            if (!(entryM == "OCR_APPSTARTING" || entryM == "OCR_NORMAL" || entryM == "OCR_CROSS" || entryM == "OCR_HAND" ||
                  entryM == "OCR_HELP" || entryM == "OCR_IBEAM" || entryM == "OCR_UNAVAILABLE" || entryM == "OCR_SIZEALL" ||
                  entryM == "OCR_SIZENESW" || entryM == "OCR_SIZENS" || entryM == "OCR_SIZENWSE" || entryM == "OCR_SIZEWE" ||
                  entryM == "OCR_UP" || entryM == "OCR_WAIT"))
            {
                Logging.Log($"Expected a cursor id constant name, but got {entryM}");
                Catowo.Interface.AddTextLog("Error: Expected a cursor constant name from the below list:", RED);
                Interface.AddLog(string.Join("\n", BaselineInputs.Cursor.validEntries.Select(entry => $"- {entry.Item1}: {entry.Item2}")));
                return false;
            }
            string finalpath = Path.Combine(dir, $"{entryM}_csr.{(animated ? "ani" : "cur")}");
            Logging.Log($"Final path: {finalpath}");
            try
            {
                File.Copy(entryZ, finalpath, true);
                File.Delete(entryZ);
            }
            catch (Exception e)
            {
                Interface.AddTextLog("Failed to cut file to local path! See logs for error.", RED);
                Logging.Log($"Failed to copy and delete {entryZ}, see below exception.");
                Logging.LogError(e);
                return false;
            }
            var content = File.ReadAllLines(file).ToList();
            content.RemoveAll(line => line.Contains(entryM));
            content.Add($"{entryM} | {finalpath}");
            File.WriteAllLines(file, content.ToArray());
            Interface.AddLog($"Successfully added cursor to {entryN}");
            return true;
        }

        [CAspects.ConsumeException]
        [CAspects.Logging]
        internal async static Task TAddCursorPreset()
        {
            ClaraHerself.Fading = false;
            ClaraHerself.HaveOverlay = false;
            ClaraHerself.CleanUp = false;
            ClaraHerself.Custom = [
                "This is the AddCursorToPreset tutorial! (Press left and right arrows to navigate the next two, \nor press the key it asks for. \nPress the Up Arrow to cancel the tutorial.)",
                "Command description:\n\"" + (string)Interface.CommandProcessing.Cmds[Interface.CommandProcessing.cmdmap["add cursor to preset"]]["desc"] + "\"",
                "This tutorial will walk you through, letter by letter, on how to execute this command!",
                "First, I'll type the base command in for you, here!",
                ];
            await ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            await ClaraHerself.TCS.Task;
            Interface.inst.inputTextBox.Text = "add cursor to preset";
            ClaraHerself.Custom = [
                "Now we need to insert our parameters!",
                "The raw parameters are: \npreset{string}, cursorid{string}, filepath{string}\nThis means that it accepts three parameters, three strings.",
                "The first parameter is 'preset', this is the name of the preset you're adding a cursor to.\nYou can run 'list presets' to see every preset you have."
                ];
            await ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            await ClaraHerself.TCS.Task;
            if (Directory.GetDirectories(CursorsFilePath).Length < 1)
            {
                ClaraHerself.Custom = [
                "It seems you dont have any presets made!",
                "I'll make you one called 'placeholder' using the 'add cursor preset' command\n(you can see the tutorial for that command by running the 'tutorial ;add cursor preset' command).",                ];
                await ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
                await ClaraHerself.TCS.Task;
                Interface.inst.inputTextBox.Text = "add cursor preset ;placeholder";
                Interface.CommandProcessing.ProcessCommand();
                ClaraHerself.Custom = ["The preset has been created, moving on!"];
                await ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
                await ClaraHerself.TCS.Task;
            }
            else
            {

            }
            ClaraHerself.Custom = [
                ""
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            await ClaraHerself.TCS.Task;
            await Task.Delay(200);
            var vks = ConvertStringToVKArray(CursorsFilePath);
            List<ExtendedInput> exis = [new ExtendedInput(VK_LWIN, 1), new BaselineInputs.ExtendedInput(VK_R),];
            exis.AddRange(vks.Select(k => new ExtendedInput(k, k == VK_LSHIFT ? (byte)1 : (byte)0)));
            exis.Add(new(VK_RETURN));
            SendKeyboardInput(75, [.. exis]);
            ClaraHerself.Custom = [
                "If the folder opened correctly, you'll see a folder named 'test1'\nwith a 'CLF' file inside it. CLF stands for Cursor List File.",
                "Thanks for following this tutorial!\nRelated commands: 'add cursor to preset', 'list preset', 'remove cursor from preset'."
                ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            await ClaraHerself.TCS.Task;
            return;
        }
    }
}