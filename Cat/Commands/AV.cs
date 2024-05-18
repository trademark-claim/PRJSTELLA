namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.Logging]
        [CAspects.ConsumeException]
        internal static bool AV()
        {
            Interface.AddLog("Starting Voice Capture...");
            VoiceCommandHandler.StartListeningAndProcessingAsync();
            return true;
        }
    }
}