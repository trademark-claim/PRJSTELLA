namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Changes the current screen to the one specified by the user, updating the application's interface accordingly.
        /// </summary>
        /// <returns>True if the screen change is successful, false otherwise.</returns>
        /// <remarks>
        /// Validates the provided screen index against the available screens and, if valid, moves the application's interface to the specified screen.
        /// </remarks>
        [CAspects.ConsumeException]
        internal static bool ChangeScreen()
        {
            int? entry = (int?)(commandstruct?.Parameters[0][0]);
            if (entry == null)
            {
                Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            if (entry >= 0 && entry < System.Windows.Forms.Screen.AllScreens.Length)
            {
                Logging.Log($"Changing screen to Screen #{entry}");
                Catowo.inst.Screen = entry.Value;
                return true;
            }
            else
            {
                Logging.Log("Screen index out of bounds of array.");
                Interface.AddLog($"Failed to find screen with index: {entry}");
                return false;
            }
        }
    }
}