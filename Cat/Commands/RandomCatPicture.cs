namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Displays a random cat picture in a new window.
        /// </summary>
        /// <returns>True upon successful display of the cat picture.</returns>
        /// <remarks>
        /// Utilizes the Helpers.CatWindow class to create and show a window containing a randomly selected cat image.
        /// </remarks>
        [LoggingAspects.ConsumeException]
        [LoggingAspects.Logging]
        internal static bool RandomCatPicture()
        {
            Interface.AddLog("Generating kitty...");
            var r = new Helpers.CatWindow();
            r.Show();
            return true;
        }
    }
}