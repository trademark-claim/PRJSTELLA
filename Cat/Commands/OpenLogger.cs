namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Opens a live logging window to display real-time log messages.
        /// </summary>
        /// <returns>True if the logger was opened successfully, false if it was already open.</returns>
        /// <remarks>
        /// Ensures that only one instance of the logging window is open at any given time.
        /// </remarks>
        [CAspects.ConsumeException]
        internal static bool OpenLogger()
        {
            if (Logger == null)
            {
                Logger = Logging.ShowLogger();
                Logger.Focusable = true;
                Logger.Focus();
                Interface.AddLog("Live Logging window opened!");
            }
            else
                Interface.AddTextLog("Live logger already open...", HOTPINK);
            return true;
        }

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TOpenLogger()
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
                    .cmdmap["open console"]
                ]["desc"]
            + "\"",
            "There's nothing much to this command, just run it and it'll open a live logger, so you can see log messages as I send them. (close one with 'close console')"
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("close log editor");
        }
    }
}