using System.Windows.Forms;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Initiates a screen recording session, saving the video to a predetermined path.
        /// </summary>
        /// <returns>True if the recording session starts successfully.</returns>
        /// <remarks>
        /// Logs the start of the recording session and invokes the screen recording functionality provided by Helpers.ScreenRecording.
        /// </remarks>
        internal static bool StartRecording()
        {
            Interface.AddLog("Starting screen recording session");
            Helpers.ScreenRecording.StartRecording(Catowo.inst.Screen, VideoFolder + "V" + GUIDRegex().Replace(Guid.NewGuid().ToString(), "") + ".mp4");
            return true;
        }
    }
}