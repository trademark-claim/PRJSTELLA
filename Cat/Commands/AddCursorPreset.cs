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
            var entryN = commandstruct.Value.Parameters[0][0];
            if (entryN == null)
            {
                Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            string dir = Environment.CursorsFilePath + entryN;
            if (Directory.Exists(dir))
            {
                Logging.Log($"Cursor Directory {dir} alrady exists.");
                Interface.AddTextLog($"Preset with name {entryN} found, names must be unqiue", RED);
                return false;
            }
            Directory.CreateDirectory(dir);
            string file = dir + "\\preset.CLF";
            Logging.Log("Creating preset file");
            File.Create(file).Dispose();
            Logging.Log("Created preset file");
            Interface.AddLog($"Preset {entryN} created");
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
                "This is the AddCursorPreset tutorial! (Press left and right arrows to navigate the next two, \nor press the key it asks for. \nPress the Up Arrow to cancel the tutorial.)",
                "Command description:\n\"" + (string)Interface.CommandProcessing.Cmds[Interface.CommandProcessing.cmdmap["add cursor preset"]]["desc"] + "\"",
                "This tutorial will walk you through, letter by letter, on how to execute this command!",
                "First, I'll type the base command in for you, here!",
                ];
            await ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            bool can = await ClaraHerself.TCS.Task;
            if (!can)
                return;
            Interface.Input = "add cursor preset";
            ClaraHerself.Custom = [
                "Now we need to insert our parameters!",
                "The raw parameters are: listname{string}\nThis means that it accepts one parameter, a string\n'listname' is just to help you understand what it wants.\n Which, in this case, is the name of the preset list!",
                "For now, we'll just call it 'test1'."
                ];
            await ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            can = await ClaraHerself.TCS.Task;
            if (!can)
                return;
            Interface.Input = "add cursor preset ;test1";
            Interface.CommandProcessing.ProcessCommand();
            ClaraHerself.Custom = [
                "Now the command has executed, and you can see the output in the UI.",
                $"The preset can be found at {CursorsFilePath}, here, I'll open it for you!",
                //"Just gonna close the interface..."    
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            can = await ClaraHerself.TCS.Task;
            if (!can)
                return;
            Catowo.inst.ToggleInterface(true, false);
            await Catowo.inst.UIToggleTCS.Task;
            await Task.Delay(200);
            Console.WriteLine("I dont know why this line makes it work, but it does.");
            var vks = ConvertStringToVKArray(CursorsFilePath);
            List<ExtendedInput> exis = [new ExtendedInput(VK_LWIN, 1), new BaselineInputs.ExtendedInput(VK_R), ];
            exis.AddRange(vks.Select(k => new ExtendedInput(k, k == VK_LSHIFT ? (byte)1 : (byte)0)));
            exis.Add(new(VK_RETURN));
            SendKeyboardInput(75, [.. exis]);
            await BaselineInputs.KeyboardTCS.Task;
            ClaraHerself.Custom = [
                "If the folder opened correctly, you'll see a folder named 'test1'\nwith a 'CLF' file inside it. CLF stands for Cursor List File.",
                "Thanks for following this tutorial!\nRelated commands: 'add cursor to preset', 'list preset', 'remove cursor from preset'."
                ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            await ClaraHerself.TCS.Task;
            try
            {
                Logging.Log($"Focusing back to catowo canvas: {Catowo.inst.Focus()}");
                Logging.Log($"Focusing back to Clara bubble: {ClaraHerself.Bubble.Focus()}");
            }
            catch { }
            ClaraHerself.CleanUp = true;
            ClaraHerself.ForceRemove();
            return;
        }
    }
}