namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Closes the currently open live logging window.
        /// </summary>
        /// <returns>True if the logger was closed successfully, false if it was not open to begin with.</returns>
        /// <remarks>
        /// Verifies if the logging window is open before attempting to close it.
        /// </remarks>
        [CAspects.ConsumeException]
        internal static bool CloseLogger()
        {
            if (Logger != null)
            {
                Logger = null;
                Logging.HideLogger();
                Interface.AddLog("Live Logging window closed!");
            }
            else
                Interface.AddTextLog("This would be great to run... if there was a log window to run it on.", HOTPINK);
            return true;
        }

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TCloseLogger()
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
                            .cmdmap["close console"]
                        ]["desc"]
                    + "\"",
                    "There's nothing much to this command, just run it and it'll close an open console window. (Open one with 'show console')"
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("close console");
        }
    }
}