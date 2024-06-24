namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Feedback method for commands without implemented functionality
        /// </summary>
        private static void FYI()
            => Interface.AddLog("This feature is coming soon.");
    }
}