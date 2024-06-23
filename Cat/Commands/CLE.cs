namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.Logging]
        internal static bool CLE()
        {
            editor?.Close();
            Interface.AddLog("Closed!");
            Logging.Log("Editor closed!");
            return true;
        }

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TCLE()
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
                            .cmdmap["close log editor"]
                        ]["desc"]
                    + "\"",
                    "There's nothing much to this command, just run it and it'll close an open log editor. (Open one with 'open log editor')"
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("close log editor");
        }
    }
}