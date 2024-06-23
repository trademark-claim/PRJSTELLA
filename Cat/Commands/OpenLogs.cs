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

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TOpenLogs()
        {
            ClaraHerself.Fading = false;
            ClaraHerself.HaveOverlay = false;
            ClaraHerself.CleanUp = false;
            ClaraHerself.Custom = [
                "Command description:\n\""
            + (string)Interface.
                CommandProcessing
                .Cmds[Interface
                    .CommandProcessing
                    .cmdmap["open logs"]
                ]["desc"]
            + "\"",
            $"There's nothing much to this command, just run it and it'll open the folder that holds the log files ({LogFolder})"
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("open logs");
        }
    }
}