using System.Diagnostics;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Command to open the log folder
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.ConsumeException]
        internal static bool OpenLogs()
        {
            Process.Start("explorer.exe", LogFolder);
            Interface.AddLog("Opened Log Folder");
            return true;
        }

        /// <summary>
        /// Tutorial for the open logs command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TOpenLogs()
        {
            StellaHerself.Fading = false;
            StellaHerself.HaveOverlay = false;
            StellaHerself.CleanUp = false;
            StellaHerself.Custom = [
                "Command description:\n\""
            + Interface.
                CommandProcessing
                .Cmds[Interface
                    .CommandProcessing
                    .cmdmap["open logs"]
                ].desc
            + "\"",
            $"There's nothing much to this command, just run it and it'll open the folder that holds the log files ({LogFolder})"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await StellaHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("open logs");
        }
    }
}