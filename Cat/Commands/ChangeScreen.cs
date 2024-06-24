namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Changes the current screen to the one specified by the user, updating STELLA's interface accordingly.
        /// </summary>
        /// <returns>True if the screen change is successful, false otherwise.</returns>
        /// <remarks>
        /// Validates the provided screen index against the available screens and, if valid, moves STELLA's interface to the specified screen.
        /// </remarks>
        [CAspects.ConsumeException]
        internal static bool ChangeScreen()
        {
            int? para1 = (int?)(commandstruct?.Parameters[0][0]);
            if (para1 == null)
            {
                Logging.Log(["Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report."]);
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            if (para1 >= 0 && para1 < System.Windows.Forms.Screen.AllScreens.Length)
            {
                Logging.Log([$"Changing screen to Screen #{para1}"]);
                Catowo.inst.Screen = para1.Value;
                return true;
            }
            else
            {
                Logging.Log(["Screen index out of bounds of array."]);
                Interface.AddLog($"Failed to find screen with index: {para1}");
                return false;
            }
        }

        /// <summary>
        /// Tutorial for changing screen command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TChangeScreen()
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
                            .cmdmap["change screen"]
                        ].desc
                    + "\"",
                    "This is the change screen command! It takes in one parameter: an integer being the id of the screen to move to.",
                    "All you need to do is enter a number and if it's a valid screen it'll change!",
                    "You can use the 'dsi' command to see your connected screens."
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            await StellaHerself.TCS.Task;
        }
    }
}