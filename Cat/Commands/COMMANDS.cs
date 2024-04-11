using NAudio.Wave;

namespace Cat
{
    /// <summary>
    /// Holds all commands
    /// </summary>
    internal static partial class Commands
    {
        internal static Interface @interface;
        private static WaveOut? WavePlayer;
        private static AudioFileReader AFR;
        internal static Command? commandstruct;
        private static bool SilentAudioCleanup = false;
        private static System.Windows.Window? Logger = null;
    }
}