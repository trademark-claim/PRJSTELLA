using System.Diagnostics;

namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.ConsumeException]
        internal static bool OpenLogs()
        {
            Process.Start("explorer.exe", LogFolder);
            Interface.AddLog("Opened Log Folder");
            return true;
        }
    }
}