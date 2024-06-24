namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Flushes the logging queue, ensuring all pending log messages are written out.
        /// </summary>
        /// <returns>A Task&lt;bool&gt; indicating the success or failure of the flush operation.</returns>
        /// <remarks>
        /// Asynchronously flushes the log queue, useful for ensuring that all pending log entries are processed and stored as intended, typically before shutdown or when debugging.
        /// </remarks>
        [CAspects.AsyncExceptionSwallower]
        [CAspects.Logging]
        internal static async Task<bool> FML()
        {
            Interface.AddLog("Flushing Log queue...");
            await Logging.FullFlush();
            Interface.AddLog("Logs flushed!");
            return true;
        }

        /// <summary>
        /// Tutorial for the flush logs command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TFlushLogs()
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
                    .cmdmap["close log editor"]
                ].desc
            + "\"",
            "There's nothing much to this command, just run it and it'll flush all volatile logs to the log file for this session. Plain and simple.",
            "You can run it with 'flush logs'"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var continu = await StellaHerself.TCS.Task;
            if (!continu) return;
            Interface.CommandProcessing.ProcessCommand("flush logs");
        }
    }
}