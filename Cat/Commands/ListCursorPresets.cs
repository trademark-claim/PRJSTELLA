using System.IO;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Either lists all linked custom cursors in a preset, or all presets, depending on whether or not a parameter was passed
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.ConsumeException]
        internal static bool ListCursorPreset()
        {
            if (commandstruct == null || commandstruct.Value.Parameters[1].Length == 0)
                Interface.AddLog("- " + string.Join("\n- ", Directory.EnumerateDirectories(CursorsFilePath)));
            else
            {
                string dir = commandstruct.Value.Parameters[1][0] as string;
                if (dir == null)
                {
                    var message = "Expected string but parsing failed, command struct or entry was null.";
                    Logging.Log([message]);
                    Interface.AddTextLog($"Execution Failed: {message}", RED);
                    return false;
                }
                string fulldir = Path.Combine(CursorsFilePath, dir);
                if (!Directory.Exists(fulldir))
                {
                    Logging.Log([$"Requested Directory \"{fulldir}\" does not exist"]);
                    Interface.AddLog("Preset does not exist.");
                    return false;
                }
                // The displayal of the preset(s)
                Interface.AddLog("Preset " + dir + "'s edited cursors by ID:\n" +
                    string.Join("\n", BaselineInputs.Cursor.validEntries.Select(entry => entry.Item1).Select(name =>
                        name + (Directory.EnumerateFiles(fulldir).Any(x => x.Contains(name)) ? " ✓" : " ✗"))));
            }

            return true;
        }
        
        /// <summary>
        /// Tutorial for the list cursor preset commmand
        /// </summary>
        /// <returns>Qs</returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TListCursorPreset()
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
                    .cmdmap["lcps"]
                ].desc
            + "\"",
            "This command takes in a single optional parameter.",
            "With no parameters, it shows every custom cursor preset you've made as a list",
            "With an inputted parameter, it shows the custom cursors inside the specified preset\n(specified in the parameter)",
            "Lets run it!"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await StellaHerself.TCS.Task;
            if (!b) return;
            Interface.Input = "list cursor presets";
            string dir = "placeholder";
            if (Directory.GetDirectories(CursorsFilePath).Length < 1)
            {
                StellaHerself.Custom = [
                "It seems you dont have any presets made!",
                "I'll make you one called 'placeholder' using the 'add cursor preset' command\n(you can see the tutorial for that command by running the 'tutorial ;add cursor preset' command).",                ];
                await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
                b = await StellaHerself.TCS.Task;
                if (!b)
                    return;
                Interface.CommandProcessing.ProcessCommand("add cursor preset ;placeholder");
                StellaHerself.Custom = ["The preset has been created, moving on!"];
                await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
                b = await StellaHerself.TCS.Task;
                if (!b)
                    return;
            }
            else
            {
                StellaHerself.Custom = [
                    "Please select one of your existing presets!",
                ];
                await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
                b = await StellaHerself.TCS.Task;
                if (!b)
                    return;
                var bs = new Objects.BoxSelecter<string>([.. Directory.GetDirectories(CursorsFilePath).Select(item => item.Replace(CursorsFilePath, ""))], "Choose preset:");
                bs.ShowDialog();
                dir = bs.SelectedItem;
            }
            StellaHerself.Custom = [
                    $"So, the {dir} preset, perfect!",
                    $"Lets see what cursors you have assigned to it with 'list cursor preset ;{dir}'"
                ];
            await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            b = await StellaHerself.TCS.Task;
            if (!b) return;
            Interface.Input = $"list cursor preset ;{dir}";
            Interface.CommandProcessing.ProcessCommand();
            StellaHerself.Custom = [
                    "You should see an output with a bunch of OCR_Something and then a cross or check",
                    $"Those are the cursor ids: a cross means you dont have a custom cursor linked to it, a check means you do (limited to the {dir} preset, at least)",
                    "And there, thats the list cursor preset command! You can also check out the 'add cursor preset', 'load cursor preset' and 'add cursor to preset' commands!"
                ];
            await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            await StellaHerself.TCS.Task;
        }
    }
}