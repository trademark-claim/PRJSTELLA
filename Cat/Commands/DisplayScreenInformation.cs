namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Displays information about all connected screens or a specific screen, based on user input.
        /// </summary>
        /// <returns>True if the information could be displayed, false if there was an issue with the input or fetching the screen data.</returns>
        /// <remarks>
        /// Information includes device name, resolution, bounds, primary status, and bits per pixel for each screen.
        /// </remarks>
        [CAspects.ConsumeException]
        internal static bool DisplayScreenInformation()
        {
            if (commandstruct == null || commandstruct?.Parameters[1].Length < 1)
            {
                Logging.Log("Displaying all connected screens' information...");
                for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; i++)
                {
                    Screen screen = System.Windows.Forms.Screen.AllScreens[i];
                    if (screen != null)
                        Interface.AddLog($"Screen {i + 1}", $"   Device Name: {screen.DeviceName}", $"   Bounds: {screen.Bounds.Width}px Width, {screen.Bounds.Height}px Height, {screen.Bounds.X}x, {screen.Bounds.Y}y, {screen.Bounds.Top}px Top, {screen.Bounds.Left}px left.", $"   Is Primary: {screen.Primary}", $"   BPP: {screen.BitsPerPixel}");
                    else
                        Interface.AddTextLog($"Failed to get Screen #{i}'s information.", RED);
                }
                return true;
            }
            else
            {
                int? entryN = (int?)(commandstruct?.Parameters[1][0]);
                if (entryN == null)
                {
                    Logging.Log("Expected int but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                    Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                    return false;
                }
                int entry = entryN.Value;
                if (entry >= 0 && entry < System.Windows.Forms.Screen.AllScreens.Length)
                {
                    Screen screen = System.Windows.Forms.Screen.AllScreens[entry];
                    Interface.AddLog($"Screen {entry + 1}", $"   Device Name: {screen.DeviceName}", $"   Bounds: {screen.Bounds.Width}px Width, {screen.Bounds.Height}px Height, {screen.Bounds.X}x, {screen.Bounds.Y}y, {screen.Bounds.Top}px Top, {screen.Bounds.Left}px left.", $"   Is Primary: {screen.Primary}", $"   BPP: {screen.BitsPerPixel}");
                }
                else
                {
                    Logging.Log("Specified index was outside the bounds of the screen array");
                    Interface.AddTextLog("Please select a valid screen index.", LIGHTRED);
                    return false;
                }
                return true;
            }
        }

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TDSI()
        {
            ClaraHerself.Fading = false;
            ClaraHerself.HaveOverlay = false;
            ClaraHerself.CleanUp = false;
            ClaraHerself.Custom = [
                "Command description:\n\""
            + (string)Interface.
                CommandProcessing
                .Cmds[Interface
                    .CommandProcessing
                    .cmdmap["dsi"]
                ]["desc"]
            + "\"",
            "This command has an optional parameter, being a screen index.",
            "Without any parameters, it shows the information of all connected screens.\nWith a parameter it'll only show the information for that screen",
            "You can run it with 'dsi'"
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("dsi");
        }
    }
}