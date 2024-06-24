namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.ConsumeException]
        [CAspects.Logging]
        [CAspects.InDev]
        internal static bool StartAudioRecording()
        {
            FYI();
            return true;
        }
    }
}