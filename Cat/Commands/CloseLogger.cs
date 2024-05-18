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
    }
}