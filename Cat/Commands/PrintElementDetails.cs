using static Cat.Catowo.Interface;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Logs the details of key interface elements to STELLA's log display, including dimensions and positions.
        /// </summary>
        /// <returns>Always returns true, indicating the method completed its execution.</returns>
        /// <remarks>
        /// Useful for debugging layout issues or for verifying that interface elements are being initialized with the correct properties.
        /// </remarks>
        [CAspects.ConsumeException]
        [CAspects.CDebug]
        [CAspects.Logging]
        internal static bool PrintElementDetails()
        {
            AddLog("Background Rectangle: ", inst.Backg.Width.ToString(), inst.Backg.Height.ToString());
            AddLog("Display box: ", logListBox.Width.ToString(), logListBox.Height.ToString(), Canvas.GetLeft(logListBox).ToString());
            AddLog("Input box: ", @interface.inputTextBox.Width.ToString(), @interface.inputTextBox.Height.ToString(), Canvas.GetLeft(@interface.inputTextBox).ToString(), Canvas.GetTop(@interface.inputTextBox).ToString());
            return true;
        }
    }
}