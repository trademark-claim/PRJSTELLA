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
        [LoggingAspects.AsyncExceptionSwallower]
        internal static async Task<bool> DEP()
        {
            string entry = commandstruct?.Parameters[0][0] as string;
            if (entry == null)
            {
                Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            if (entry == "ffmpeg")
            {
                await Helpers.FFMpegManager.DownloadFFMPEG();
                Logging.Log("DEP Execution " + Helpers.BackendHelping.Glycemia("Complete"));
            }
            else
            {
                Interface.AddLog("Unrecognised Process name.");
                return false;
            }
            return true;
        }
    }
}