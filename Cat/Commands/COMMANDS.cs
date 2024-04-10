using NAudio.Wave;

namespace Cat
{
    internal static partial class Commands
    {
        internal static Interface @interface;
        private static WaveOut? WavePlayer;
        private static AudioFileReader AFR;
        private static Command? commandstruct;
        internal static bool SilentAudioCleanup = false;
        private static System.Windows.Window? Logger = null;
    }
}