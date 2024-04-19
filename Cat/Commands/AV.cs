using System.Diagnostics;
using System.IO;

namespace Cat
{
    internal static partial class Commands
    {
        [LoggingAspects.Logging]
        [LoggingAspects.ConsumeException]
        internal static bool AV()
        {
            Interface.AddLog("Starting Voice Capture...");
            VoiceCommandHandler.StartListeningAndProcessingAsync();
            return true;
        }
    }
}