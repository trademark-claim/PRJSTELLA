namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Stops the current screen recording session and logs the action.
        /// </summary>
        /// <returns>True if the recording session is stopped successfully.</returns>
        /// <remarks>
        /// Invokes a final logging flush and then stops the recording session using Helpers.ScreenRecording, logging the end of the session.
        /// </remarks>
        [LoggingAspects.ConsumeException]
        internal static bool StopRecording()
        {
            FML();
            Interface.AddLog("Ending screen recording session");
            ScreenRecorder.StopRecording();
            Interface.AddLog("Screen recording session ended.");
            return true;
        }
    }
}