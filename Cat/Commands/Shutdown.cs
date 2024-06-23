namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Initiates the application shutdown process, performing cleanup and closing operations.
        /// </summary>
        /// <returns>True if the shutdown process is initiated successfully.</returns>
        /// <remarks>
        /// Logs the shutdown intention, hides the application window, and triggers any necessary shutdown logic encapsulated in the App.ShuttingDown method.
        /// </remarks>
        [CAspects.AsyncExceptionSwallower]
        internal static async Task<bool> Shutdown()
        {
            Interface.AddTextLog("Shutting down... give me a few moments...", System.Windows.Media.Color.FromRgb(230, 20, 20));
            await Task.Delay(1000);
            Catowo.inst.Hide();
            App.ShuttingDown();
            return true;
        }

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TShutdown()
        {
            ClaraHerself.Custom = [
                "Command description:\n\""
            + (string)Interface.
                CommandProcessing
                .Cmds[Interface
                    .CommandProcessing
                    .cmdmap["close log editor"]
                ]["desc"]
            + "\"",
            "There's nothing much to this command, just run it and it'll shut me down entirely (auto flushing logs and performing cleanup)",
            "Press the right arrow for me to do this, press the up arrow to cancel."
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("shutdown");
        }
    }
}