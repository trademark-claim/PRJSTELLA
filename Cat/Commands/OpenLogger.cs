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
    }
}