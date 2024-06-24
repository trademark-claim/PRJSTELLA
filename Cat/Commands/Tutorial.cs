using System.Windows.Media;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Tutorial command, runs the T(CommandName) methods
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.ConsumeException]
        internal static async Task<bool> Tutorial()
        {
            if (commandstruct.Value.Parameters[0][0] is not string para1)
            {
                Logging.Log(["Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report."]);
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            if (!Interface.CommandProcessing.cmdmap.TryGetValue(para1, out int id))
            {
                Interface.AddTextLog($"Unknown value: {para1}", Colors.Red);
                Logging.Log([$"{para1} was not found in command mapping"]);
                return false;
            }
            else if (Interface.CommandProcessing.Cmds[id].tutorial == null)
            {
                Interface.AddTextLog($"{para1} command has no implemented tutorial yet, sorry!", Colors.Red);
                Logging.Log([$"Tutorial for {para1} command not linked."]);
                return false;
            }
            else if (Interface.CommandProcessing.Cmds[id].tutorial is not Func<Task> action)
            {
                Interface.AddTextLog($"(Probably) MAJOR ERROR: Linked tutorial funct type did not match expected.", Colors.DeepPink);
                Logging.Log([$"{para1} tutorial action type expected Func<Task> but got {Interface.CommandProcessing.Cmds[id].tutorial.GetType().FullName}"]);
                return false;
            }
            else
            {
                StellaHerself.Fading = false;
                StellaHerself.HaveOverlay = false;
                StellaHerself.CleanUp = false;
                StellaHerself.Custom = [
                    "(Remember, use the left arrow to go back within sections, use the right arrow to move to the next, and the up arrow to quit!)",
                ];
                StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
                var b = await StellaHerself.TCS.Task;
                if (!b) return true;
                    action();
            }
            return true;
        }

        /// <summary>
        /// Tutorial for the... tutorial command?
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TTutorial()
        {
            StellaHerself.Custom = [
                "You... want a tutorial... for the tutorial command.. huh?",
                "Just do 'tutorial ;commandname', where you can get 'commandname' from 'help ;commands'",
                "My time is precious, and so is yours!!!",
                "Don't mess around!"
                ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await StellaHerself.TCS.Task;
            if (!b) return;
        }
    }
}