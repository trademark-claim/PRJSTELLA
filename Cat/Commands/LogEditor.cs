namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Command to open the log editor
        /// </summary>
        /// <returns></returns>
        internal static bool OpenLogEditor()
        {
            // Close any previously existing ones
            editor?.Close();
            editor = new LogEditor();
            editor.Show();
            Interface.AddLog("Opened Log Editor");
            return true;
        }

        /// <summary>
        /// Tutorial for the open log editor command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TOpenLogEditor()
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
                    .cmdmap["open log editor"]
                ].desc
            + "\"",
            "There's nothing much to this command, just run it and it'll open a log editor. (Close one with 'close log editor')"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await StellaHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("open log editor");
        }
    }
}