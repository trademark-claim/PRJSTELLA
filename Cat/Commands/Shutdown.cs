namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Initiates shutdown, performing cleanup and closing operations.
        /// </summary>
        /// <returns>True</returns>
        /// <remarks>
        /// Logs the shutdown intention, hides STELLA window, and triggers any necessary shutdown logic encapsulated in the App.ShuttingDown method.
        /// </remarks>
        [CAspects.AsyncExceptionSwallower]
        [CAspects.Logging]
        internal static async Task<bool> Shutdown()
        {
            Interface.AddTextLog("Shutting down... give me a few moments...", System.Windows.Media.Color.FromRgb(230, 20, 20));
            await Task.Delay(1000); //4 fx
            Catowo.inst.Hide();
            App.ShuttingDown();
            return true;
        }

        /// <summary>
        /// Tutorial command for the shutdown method
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TShutdown()
        {
            StellaHerself.Custom = [
                "Command description:\n\""
            + Interface.
                CommandProcessing
                .Cmds[Interface
                    .CommandProcessing
                    .cmdmap["shutdown"]
                ].desc
            + "\"",
            "There's nothing much to this command, just run it and it'll shut me down entirely (auto flushing logs and performing cleanup)",
            "Press the right arrow for me to do this, press the up arrow to cancel."
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await StellaHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("shutdown");
        }
    }
}