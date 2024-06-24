namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Command for resetting the cursor to basic 
        /// </summary>
        /// <returns></returns>
        [CAspects.ConsumeException]
        [CAspects.Logging]
        internal static bool ResetCursor()
        {
            Interface.AddLog("Resetting cursor...");
            BaselineInputs.Cursor.Reset();
            Interface.AddLog("Cursor Reset!");
            return true;
        }

        /// <summary>
        /// Tutorial for the reset cursor command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TResetCursor()
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
                    .cmdmap["reset cursor"]
                ].desc
            + "\"",
            "There's nothing much to this command, just run it and it'll set all your cursors back to default!",
            "Press the Right arrow to run this command using 'reset cursors', press the up arrow to not."
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var continu = await StellaHerself.TCS.Task;
            if (!continu) return;
            Interface.CommandProcessing.ProcessCommand("reset cursors");
        }
    }
}