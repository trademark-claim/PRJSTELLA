using System.IO;

namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.Logging]
        [CAspects.ConsumeException]
        [CAspects.InDev]
        internal static bool RemoveCursorFromPreset()
        {
            if (commandstruct?.Parameters[0][0] is not string para1 || commandstruct?.Parameters[0][1] is not string para2)
            {
                Logging.Log(["Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report."]);
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            para1 = para1.Trim(); // The preset name
            para2 = para2.Trim(); // The Cursor ID
            string dir = Path.Combine(Environment.CursorsFilePath, para1);
            if (!Directory.Exists(dir))
            {
                Logging.Log([$"Directory {dir} not found."]);
                Interface.AddTextLog($"No preset with name {para1} found", RED);
                return false;
            }
            string file = Path.Combine(dir, "preset.CLF");
            if (!File.Exists(file))
            {
                Logging.Log([$"Cursor Preset file ({file}) not found!"]);
                Interface.AddTextLog("Preset file not found, try re-creating this preset! (Remove the currently existing one though).", RED);
                return false;
            }

            para2 = para2.ToUpper().Trim();
            if (!(para2 == "OCR_APPSTARTING" || para2 == "OCR_NORMAL" || para2 == "OCR_CROSS" || para2 == "OCR_HAND" ||
                  para2 == "OCR_HELP" || para2 == "OCR_IBEAM" || para2 == "OCR_UNAVAILABLE" || para2 == "OCR_SIZEALL" ||
                  para2 == "OCR_SIZENESW" || para2 == "OCR_SIZENS" || para2 == "OCR_SIZENWSE" || para2 == "OCR_SIZEWE" ||
                  para2 == "OCR_UP" || para2 == "OCR_WAIT"))
            {
                Logging.Log([$"Expected a cursor id constant name, but got {para2}"]);
                Interface.AddTextLog("Error: Expected a cursor constant name from the below list:", RED);
                Interface.AddLog(string.Join("\n", BaselineInputs.Cursor.validEntries.Select(entry => $"- {entry.Item1}: {entry.Item2}")));
                return false;
            }

            string finalpath = Path.Combine(dir, $"{para2}_csr.ani");
            if (!File.Exists(finalpath))
            {
                finalpath = Path.Combine(dir, $"{para2}_csr.cur");
                if (!File.Exists(finalpath))
                {
                    Logging.Log([$"Cursor file for {para2} not found."]);
                    Interface.AddTextLog($"Cursor file for {para2} not found!", RED);
                    return false;
                }
            }

            try
            {
                File.Delete(finalpath);
            }
            catch (Exception e)
            {
                Interface.AddTextLog("Failed to delete cursor file! See logs for error.", RED);
                Logging.Log([$"Failed to delete {finalpath}, see below exception."]);
                Logging.LogError(e);
                return false;
            }

            var content = File.ReadAllLines(file).ToList();
            content.RemoveAll(line => line.Contains(para2));
            File.WriteAllLines(file, content.ToArray());

            Interface.AddLog($"Successfully removed cursor from {para1}");
            return true;
        }

        /// <summary>
        /// Tutorial for the Remove cursor from preset command.
        /// </summary>
        /// <returns></returns>
        [CAspects.ConsumeException]
        [CAspects.Logging]
        internal async static Task TRemoveCursorFromPreset()
        {
            StellaHerself.Fading = false;
            StellaHerself.HaveOverlay = false;
            StellaHerself.CleanUp = false;
            StellaHerself.Custom = [
                "This is the RemoveCursorFromPreset tutorial!",
                "Command description:\n\"" + Interface.CommandProcessing.Cmds[Interface.CommandProcessing.cmdmap["remove cursor from preset"]].desc + "\"",
                "This tutorial will walk you through on how to execute this command!"
            ];
            await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var can = await StellaHerself.TCS.Task;
            if (!can)
                return;

            StellaHerself.Custom = [
                "First, we'll type the base command in for you.",
                "I'll do it for you, just watch!"
            ];
            await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            can = await StellaHerself.TCS.Task;
            if (!can)
                return;

            Interface.Input = "remove cursor from preset";
            StellaHerself.Custom = [
                "Now we need to insert our parameters!",
                "The raw parameters are: \npreset{string}, cursorid{string}\nThis means that it accepts two parameters, two strings.",
                "The first parameter is 'preset', this is the name of the preset you're removing a cursor from.",
                "You can run 'list presets' to see every preset you have."
            ];
            await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            can = await StellaHerself.TCS.Task;
            if (!can)
                return;

            string dir = "placeholder";
            if (Directory.GetDirectories(Environment.CursorsFilePath).Length < 1)
            {
                StellaHerself.Custom = [
                    "It seems you don't have any presets made!",
                    "I'll make you one called 'placeholder' using the 'add cursor preset' command\n(you can see the tutorial for that command by running the 'tutorial ;add cursor preset' command).",
                ];
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

                var bs = new Objects.BoxSelecter<string>([.. Directory.GetDirectories(Environment.CursorsFilePath).Select(item => item.Replace(Environment.CursorsFilePath, ""))], "Choose preset:");
                bs.ShowDialog();
                dir = bs.SelectedItem;
            }

            Interface.Input = $"remove cursor from preset ;{dir}";
            StellaHerself.Custom = [
                $"Now we've done the first parameter, the preset we're removing from!\nWe'll be removing from the {dir} preset.",
                "Next, we have to input the ID of the cursor we want to remove. These are the names of the cursors defined by Windows.",
                "Here, I'll provide a reference to what each cursor id is and what it represents, look at STELLA's interface console! Please choose one!",
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            can = await StellaHerself.TCS.Task;
            if (!can)
                return;

            Interface.AddLog(string.Join("\n", BaselineInputs.Cursor.validEntries.Select(entry => $"- {entry.Item1}: {entry.Item2}")));
            await Task.Delay(500);

            var bs2 = new Objects.BoxSelecter<string>(BaselineInputs.Cursor.validEntries.Select(entry => entry.Item1).ToList(), "Choose cursor id:");
            bs2.ShowDialog();
            string cursor = bs2.SelectedItem;
            Interface.Input = $"remove cursor from preset ;{dir} ;{cursor}";

            StellaHerself.Custom = [
                $"So the {cursor} cursor.. perfect!",
                $"Now we've done both parameters. Let's execute the command!"
            ];
            await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            can = await StellaHerself.TCS.Task;
            if (!can)
                return;

            Interface.CommandProcessing.ProcessCommand();
            Interface.AddLog($"Successfully removed cursor {cursor} from preset {dir}");
            StellaHerself.Custom = [
                "Congrats, you've now removed a cursor from a preset!",
                "You can manage your cursors and presets using related commands such as 'add cursor to preset', 'list presets', and 'remove cursor from preset'.",
                "Thanks for following this tutorial!"
            ];
            await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            try
            {
                Logging.Log([$"Focusing back to catowo canvas: {Catowo.inst.Focus()}"]);
                Logging.Log([$"Focusing back to Stella bubble: {StellaHerself.Bubble.Focus()}"]);
            }
            catch { }
            await StellaHerself.TCS.Task;
            return;
        }
    }
}