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
        [CAspects.ConsumeException]
        internal static bool Shutdown()
        {
            Interface.AddTextLog("Shutting down... give me a few moments...", System.Windows.Media.Color.FromRgb(230, 20, 20));
            Catowo.inst.Hide();
            App.ShuttingDown();
            return true;
        }
    }
}