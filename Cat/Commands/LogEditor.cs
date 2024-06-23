namespace Cat
{
    internal static partial class Commands
    {
        internal static bool OpenLogEditor()
        {
            editor?.Close();
            editor = new LogEditor();
            editor.Show();
            Interface.AddLog("Opened Log Editor");
            return true;
        }

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TOpenLogEditor()
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
                    .cmdmap["open log editor"]
                ]["desc"]
            + "\"",
            "There's nothing much to this command, just run it and it'll open a log editor. (Close one with 'close log editor')"
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("open log editor");
        }
    }
}