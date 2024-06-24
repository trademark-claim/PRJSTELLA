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
            ///<summary>
            /// Input parameters being the preset, cur id and file path to use
            /// </summary>
            string para1 = commandstruct.Value.Parameters[0][0] as string,
                para2 = commandstruct.Value.Parameters[0][1] as string,
                para3 = commandstruct.Value.Parameters[0][2] as string;
            if (para1 == null || para2 == null || para3 == null)
            {
                Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Catowo.Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            // Clean up the inputs
            para1 = para1.Trim();
            para2 = para2.Trim();
            para3 = para3.Trim().Replace("\"", "");
            string dir = Path.Combine(Environment.CursorsFilePath, para1);
            // Ensuring validity of inputs
            if (!Directory.Exists(dir))
            {
                Logging.Log($"Directory {dir} not found.");
                Catowo.Interface.AddTextLog($"No preset with name {para1} found", RED);
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
            if (!File.Exists(para3))
            {
                Logging.Log($"Requested file {para3} not found.");
                Catowo.Interface.AddTextLog($"Requested file {para3} not found! Please verify your file and provide a full, absolute file path.", RED);
                return false;
            }
            if (!para3.EndsWith(".ani") && !para3.EndsWith(".cur"))
            {
                Logging.Log($"Requested file {para3} not a .ani or .cur file.");
                Catowo.Interface.AddTextLog($"Requested file {para3} was not a supported cursor file! (.ani / .cur).", RED);
                return false;
            }
            bool animated = para3.EndsWith(".ani");
            para2 = para2.ToUpper().Trim();
            if (!(para2 == "OCR_APPSTARTING" || para2 == "OCR_NORMAL" || para2 == "OCR_CROSS" || para2 == "OCR_HAND" ||
                  para2 == "OCR_HELP" || para2 == "OCR_IBEAM" || para2 == "OCR_UNAVAILABLE" || para2 == "OCR_SIZEALL" ||
                  para2 == "OCR_SIZENESW" || para2 == "OCR_SIZENS" || para2 == "OCR_SIZENWSE" || para2 == "OCR_SIZEWE" ||
                  para2 == "OCR_UP" || para2 == "OCR_WAIT"))
            {
                Logging.Log($"Expected a cursor id constant name, but got {para2}");
                Catowo.Interface.AddTextLog("Error: Expected a cursor constant name from the below list:", RED);
                Interface.AddLog(string.Join("\n", BaselineInputs.Cursor.validEntries.Select(entry => $"- {entry.Item1}: {entry.Item2}")));
                return false;
            }
            // Completing command
            string finalpath = Path.Combine(dir, $"{para2}_csr.{(animated ? "ani" : "cur")}");
            Logging.Log($"Final path: {finalpath}");
            try
            {
                File.Copy(para3, finalpath, true);
                File.Delete(para3);
            }
            catch (Exception e)
            {
                Interface.AddTextLog("Failed to cut file to local path! See logs for error.", RED);
                Logging.Log($"Failed to copy and delete {para3}, see below exception.");
                Logging.LogError(e);
                return false;
            }
            // Overwrite any existing custom cursors for that id
            var content = File.ReadAllLines(file).ToList();
            content.RemoveAll(line => line.Contains(para2));
            content.Add($"{para2} | {finalpath}");
            File.WriteAllLines(file, content.ToArray());
            Interface.AddLog($"Successfully added cursor to {para1}");
            return true;
        }

        /// <summary>
        /// Tutorial for the Add cursor to preset command.
        /// </summary>
        /// <returns></returns>
        [CAspects.ConsumeException]
        [CAspects.Logging]
        internal async static Task TAddCursorToPreset()
        {
            StellaHerself.Fading = false;
            StellaHerself.HaveOverlay = false;
            StellaHerself.CleanUp = false;
            StellaHerself.Custom = [
                "This is the AddCursorToPreset tutorial! (Press left and right arrows to navigate the next two, \nor press the key it asks for. \nPress the Up Arrow to cancel the tutorial.)",
                "Command description:\n\"" + Interface.CommandProcessing.Cmds[Interface.CommandProcessing.cmdmap["add cursor to preset"]].desc + "\"",
                "This tutorial will walk you through on how to execute this command!",
                "Firstly, we need a cursor to actually add -- I'll download one for you!\nYou like cats.. right? :3c",
                "Oh, and by the way, cursor accepted file extensions are .cur (static cursor files) and .ani (animated cursor files)."
                ];
            await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var can = await StellaHerself.TCS.Task;
            if (!can)
                return;
            Helpers.ExternalDownloading.FromGDrive(SingleCat, Path.Combine(ExternalDownloadsFolder, "cat.ani"));
            await Helpers.ExternalDownloading.TCS.Task;

            StellaHerself.Custom = [
                $"Perfect!\nIf that worked, you should have a file called 'cat.ani' in the External Downloads folder ({ExternalDownloadsFolder}).\nWe'll use this file later, so keep it in mind (don't touch it though...)",
                "Now, I'll type the base command in for you, here!",
                ];
            await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            can = await StellaHerself.TCS.Task;
            if (!can)
                return;
            Interface.Input = "add cursor to preset";
            StellaHerself.Custom = [
                "Now we need to insert our parameters!",
                "The raw parameters are: \npreset{string}, cursorid{string}, filepath{string}\nThis means that it accepts three parameters, three strings.",
                "The first parameter is 'preset', this is the name of the preset you're adding a cursor to.\nYou can run 'list presets' to see every preset you have."
                ];
            await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            can = await StellaHerself.TCS.Task;
            if (!can)
                return;
            string dir = "placeholder";
            if (Directory.GetDirectories(CursorsFilePath).Length < 1)
            {
                StellaHerself.Custom = [
                "It seems you dont have any presets made!",
                "I'll make you one called 'placeholder' using the 'add cursor preset' command\n(you can see the tutorial for that command by running the 'tutorial ;add cursor preset' command).",                ];
                await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
                can = await StellaHerself.TCS.Task;
                if (!can)
                    return;
                Interface.Input = "add cursor preset ;placeholder";
                Interface.CommandProcessing.ProcessCommand();
                StellaHerself.Custom = ["The preset has been created, moving on!"];
                await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
                can = await StellaHerself.TCS.Task;
                if (!can)
                    return;
            }
            else
            {
                StellaHerself.Custom = [
                    "Please select one of your existing presets!",
                ];
                await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
                can = await StellaHerself.TCS.Task;
                if (!can)
                    return;
                var bs = new Objects.BoxSelecter<string>([.. Directory.GetDirectories(CursorsFilePath).Select(item => item.Replace(CursorsFilePath, ""))], "Choose preset:");
                bs.ShowDialog();
                dir = bs.SelectedItem;
            }
            Interface.Input = $"add cursor to preset ;{dir}";
            StellaHerself.Custom = [
                $"Now we've done the first parameter, the preset we're adding to!\nWe'll be adding to the {dir} preset.",
                "Next, we have to input the ID of the cursor we want to change. These are the names of the cursors defined by windows.",
                "Here, I'll provide a reference to what each cursor id is and what it represents, look at the interface console! Please choose one!",
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            can = await StellaHerself.TCS.Task;
            if (!can)
                return;
            Interface.AddLog(string.Join("\n", BaselineInputs.Cursor.validEntries.Select(entry => $"- {entry.Item1}: {entry.Item2}")));
            await Task.Delay(500);
            var bs2 = new Objects.BoxSelecter<string>(BaselineInputs.Cursor.validEntries.Select(entry => entry.Item1).ToList(), "Choose preset:");
            bs2.ShowDialog();
            string cursor = bs2.SelectedItem;
            Interface.Input = $"add cursor to preset ;{dir} ;{cursor}";
            StellaHerself.Custom = [
                $"So the {cursor} cursor.. perfect!",
                $"Now we've done the first and second parameters, there's just one more to go!\n",
                "Finally, you have to tell me where to find the new cursor to use -- remember I downloaded one for you earlier?",
                $"If you haven't touched it, it should still be at {ExternalDownloadsFolder}/cat.ani",
                "Lets try it!"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            can = await StellaHerself.TCS.Task;
            if (!can)
                return;
            Interface.Input = $"add cursor to preset ;{dir} ;{cursor} ;{ExternalDownloadsFolder}/cat.ani";
            Interface.CommandProcessing.ProcessCommand();
            Catowo.inst.ToggleInterface(true, false);
            await Catowo.inst.UIToggleTCS.Task;
            await Task.Delay(200);
            var vks = ConvertStringToVKArray(Path.Combine(CursorsFilePath, dir));
            List<ExtendedInput> exis = [new ExtendedInput(VK_LWIN, 1), new BaselineInputs.ExtendedInput(VK_R),];
            exis.AddRange(vks.Select(k => new ExtendedInput(k, k == VK_LSHIFT ? (byte)1 : (byte)0)));
            exis.Add(new(VK_RETURN));
            SendKeyboardInput(75, [.. exis]);
            await BaselineInputs.KeyboardTCS.Task;
            StellaHerself.Custom = [
                $"If the folder opened correctly, you'll see a folder named {dir}\nwith an updated 'CLF' file and a file called {cursor}_csr.ani",
                "Congrats, you've now added a cursor to a preset!\nYou can find more sweet cursors at: http://www.rw-designer.com/gallery",
                "Thanks for following this tutorial!\nRelated commands: 'add cursor preset', 'list preset', 'remove cursor from preset'."
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