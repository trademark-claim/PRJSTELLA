namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Downloads external packages or executes processes based on the provided command parameters.
        /// </summary>
        /// <returns>A Task&lt;bool&gt; indicating the success or failure of the operation.</returns>
        /// <remarks>
        /// Attempts to identify and execute a download or process execution based on the input parameters. Specific actions, such as downloading FFMPEG, are determined by the command argument.
        /// </remarks>
        [CAspects.AsyncExceptionSwallower]
        internal static async Task<bool> DEP()
        {
            if (commandstruct?.Parameters[0][0] is not string entry)
            {
                Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            if (entry == "ffmpeg")
            {
                _ = new Helpers.EPManagement(Helpers.EPManagement.Processes.FFmpeg);
                Logging.Log("DEP Execution Complete");
            }
            else
            {
                Interface.AddLog("Unrecognised Process name. (FFMPEG)");
                return false;
            }
            return true;
        }
    }
}