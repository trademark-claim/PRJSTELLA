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
        [LoggingAspects.AsyncExceptionSwallower]
        internal static async Task<bool> FML()
        {
            Interface.AddLog("Flushing Log queue...");
            await Logging.FullFlush();
            Interface.AddLog("Logs flushed!");
            return true;
        }
    }
}