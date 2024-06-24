using System.IO;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Creates a new, unique cursor preset
        /// </summary>
        /// <returns>true if successful at all, false otherwise</returns>
        [CAspects.ConsumeException]
        [CAspects.Logging]
        internal static bool AddCursorPreset()
        {
            // Extracts first parameter
            var para1 = commandstruct.Value.Parameters[0][0];
            if (para1 == null)
            {
                Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            // Full path to preset
            string dir = Environment.CursorsFilePath + para1;
            if (Directory.Exists(dir))
            {
                Logging.Log($"Cursor Directory {dir} alrady exists.");
                Interface.AddTextLog($"Preset with name {para1} found, names must be unqiue", RED);
                return false;
            }
            // Creates the preset and preset file
            Directory.CreateDirectory(dir);
            string file = dir + "\\preset.CLF";
            Logging.Log("Creating preset file");
            File.Create(file).Dispose();
            Logging.Log("Created preset file");
            Interface.AddLog($"Preset {para1} created");
            return true;
        }

        /// <summary>
        /// Tutorial function for the AddCursorPreset command
        /// </summary>
        /// <returns></returns>
        [CAspects.ConsumeException]
        [CAspects.Logging]
        internal async static Task TAddCursorPreset()
        {
            // Set the speech bubbles up to not auto fade and cleanup and block input
            StellaHerself.Fading = false;
            StellaHerself.HaveOverlay = false;
            StellaHerself.CleanUp = false;
            // Walk the user through the tutorial, each break meaning the tutorial is either inputting or expecting input
            StellaHerself.Custom = [
                "This is the AddCursorPreset tutorial! (Press left and right arrows to navigate the next two, \nor press the key it asks for. \nPress the Up Arrow to cancel the tutorial.)",
                "Command description:\n\"" + Interface.CommandProcessing.Cmds[Interface.CommandProcessing.cmdmap["add cursor preset"]].desc + "\"",
                "This tutorial will walk you through, letter by letter, on how to execute this command!",
                "First, I'll type the base command in for you, here!"
                ];
            await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            bool continu = await StellaHerself.TCS.Task;
            if (!continu) return;
            Interface.Input = "add cursor preset";
            StellaHerself.Custom = [
                "Now we need to insert our parameters!",
                "The raw parameters are: listname{string}\nThis means that it accepts one parameter, a string\n'listname' is just to help you understand what it wants.\n Which, in this case, is the name of the preset list!",
                "For now, we'll just call it 'test1'."
                ];
            await StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            continu = await StellaHerself.TCS.Task;
            if (!continu)
                return;
            Interface.Input = "add cursor preset ;test1";
            Interface.CommandProcessing.ProcessCommand();
            StellaHerself.Custom = [
                "Now the command has executed, and you can see the output in the UI.",
                $"The preset can be found at {CursorsFilePath}, here, I'll open it for you!",
                //x "Just gonna close the interface..."    
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            continu = await StellaHerself.TCS.Task;
            if (!continu)
                return;
            // Close interface as it gets in the way
            Catowo.inst.ToggleInterface(true, false);
            await Catowo.inst.UIToggleTCS.Task;
            await Task.Delay(200);
            // Do the fancy input instead of just boringly starting a new explorer process
            Console.WriteLine("I dont know why this line makes it work, but it does."); //? I swear it doesnt work without it????
            var vks = ConvertStringToVKArray(CursorsFilePath);
            List<ExtendedInput> extended_inputs = [new ExtendedInput(VK_LWIN, 1), new BaselineInputs.ExtendedInput(VK_R), ];
            extended_inputs.AddRange(vks.Select(k => new ExtendedInput(k, k == VK_LSHIFT ? (byte)1 : (byte)0)));
            extended_inputs.Add(new(VK_RETURN));
            SendKeyboardInput(75, [.. extended_inputs]);
            await BaselineInputs.KeyboardTCS.Task;
            // Finish tutorial
            StellaHerself.Custom = [
                "If the folder opened correctly, you'll see a folder named 'test1'\nwith a 'CLF' file inside it. CLF stands for Cursor List File.",
                "Thanks for following this tutorial!\nRelated commands: 'add cursor to preset', 'list preset', 'remove cursor from preset'."
                ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            await StellaHerself.TCS.Task;
            try
            {
                Logging.Log($"Focusing back to catowo canvas: {Catowo.inst.Focus()}");
                Logging.Log($"Focusing back to Stella bubble: {StellaHerself.Bubble.Focus()}");
            }
            catch { }
            StellaHerself.CleanUp = true;
            StellaHerself.ForceRemove();
            return;
        }
    }
}