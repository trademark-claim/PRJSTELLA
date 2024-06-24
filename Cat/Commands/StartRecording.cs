using System.IO;

namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.ConsumeException]
        [CAspects.Logging]
        [CAspects.InDev]
        internal static bool StartRecording()
        {
            Interface.AddLog("Command in development, sorry!");
            return true;
        }
    }
}