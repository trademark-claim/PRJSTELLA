using System.Windows.Media;

namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.Logging]
        [CAspects.ConsumeException]
        internal static bool Tutorial()
        {
            var entryN = commandstruct.Value.Parameters[0][0];
            if (entryN == null)
            {
                Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            if (!Interface.CommandProcessing.cmdmap.TryGetValue(entryN.ToString(), out int id))
            {
                Interface.AddTextLog($"Unknown value: {entryN}", Colors.Red);
                Logging.Log($"{entryN} was not found in command mapping");
                return false;
            }
            else if (Interface.CommandProcessing.Cmds[id].tutorial == null)
            {
                Interface.AddTextLog($"{entryN} command has no implemented tutorial yet, sorry!", Colors.Red);
                Logging.Log($"Tutorial for {entryN} command not linked.");
                return false;
            }
            else if (Interface.CommandProcessing.Cmds[id].tutorial is not Func<Task> action)
            {
                Interface.AddTextLog($"MAJOR ERROR: Linked tutorial funct type did not match expected.", Colors.DeepPink);
                Logging.Log($"{entryN} tutorial action type expected Func<Task> but got {Interface.CommandProcessing.Cmds[id].tutorial.GetType().FullName}");
                return false;
            }
            else action();
            return true;
        }

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TTutorial()
        {
            StellaHerself.Custom = [
                "You... want a tutorial... for the tutorial command.. huh?",
                "My time is precious, and so is yours!!!",
                "Don't mess around!"
                ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await StellaHerself.TCS.Task;
            if (!b) return;
        }
    }
}