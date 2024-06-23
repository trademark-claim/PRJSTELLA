namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.ConsumeException]
        [CAspects.Logging]
        internal static bool ResetCursor()
        {
            Interface.AddLog("Resetting cursor...");
            BaselineInputs.Cursor.Reset();
            Interface.AddLog("Cursor Reset!");
            return true;
        }

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TResetCursor()
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
                    .cmdmap["reset cursor"]
                ]["desc"]
            + "\"",
            "There's nothing much to this command, just run it and it'll set all your cursors back to default!",
            "Press the Right arrow to run this command using 'reset cursors', press the up arrow to not."
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("reset cursors");
        }
    }
}