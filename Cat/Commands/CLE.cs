namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Command to close log editors
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        internal static bool CLE()
        {
            editor?.Close();
            Interface.AddLog("Closed!");
            Logging.Log(["Editor closed!"]);
            return true;
        }

        /// <summary>
        /// Tutorial command for the close log editor command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TCLE()
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
                            .cmdmap["close log editor"]
                        ].desc
                    + "\"",
                    "There's nothing much to this command, just run it and it'll close an open log editor. (Open one with 'open log editor')"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var continu = await StellaHerself.TCS.Task;
            if (!continu) return;
            Interface.CommandProcessing.ProcessCommand("close log editor");
        }
    }
}