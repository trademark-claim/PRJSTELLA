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
        [CAspects.Logging]
        internal static bool DisplayScreenInformation()
        {
            if (commandstruct == null || commandstruct?.Parameters[1].Length < 1)
            {
                Logging.Log(["Displaying all connected screens' information..."]);
                for (int i = 0; i < Screen.AllScreens.Length; i++)
                {
                    Screen screen = Screen.AllScreens[i];
                    if (screen != null)
                        Interface.AddLog($"Screen {i + 1}", $"   Device Name: {screen.DeviceName}", $"   Bounds: {screen.Bounds.Width}px Width, {screen.Bounds.Height}px Height, {screen.Bounds.X}x, {screen.Bounds.Y}y, {screen.Bounds.Top}px Top, {screen.Bounds.Left}px left.", $"   Is Primary: {screen.Primary}", $"   BPP: {screen.BitsPerPixel}");
                    else
                        Interface.AddTextLog($"Failed to get Screen #{i}'s information.", RED);
                }
                return true;
            }
            else
            {
                int? para1 = (int?)(commandstruct?.Parameters[1][0]);
                if (para1 == null)
                {
                    Logging.Log(["Expected int but parsing failed and returned either a null command struct or a null entry, please submit a bug report."]);
                    Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                    return false;
                }
                int _para1 = para1.Value;
                if (_para1 >= 0 && _para1 < Screen.AllScreens.Length)
                {
                    Screen screen = Screen.AllScreens[_para1];
                    Interface.AddLog($"Screen {_para1 + 1}", $"   Device Name: {screen.DeviceName}", $"   Bounds: {screen.Bounds.Width}px Width, {screen.Bounds.Height}px Height, {screen.Bounds.X}x, {screen.Bounds.Y}y, {screen.Bounds.Top}px Top, {screen.Bounds.Left}px left.", $"   Is Primary: {screen.Primary}", $"   BPP: {screen.BitsPerPixel}");
                }
                else
                {
                    Logging.Log(["Specified index was outside the bounds of the screen array"]);
                    Interface.AddTextLog("Please select a valid screen index.", LIGHTRED);
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Ttorial for the Display screen information command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TDSI()
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
                    .cmdmap["dsi"]
                ].desc
            + "\"",
            "This command has an optional parameter, being a screen index.",
            "Without any parameters, it shows the information of all connected screens.\nWith a parameter it'll only show the information for that screen",
            "You can run it with 'dsi'"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var Continu = await StellaHerself.TCS.Task;
            if (!Continu) return;
            Interface.CommandProcessing.ProcessCommand("dsi");
        }
    }
}