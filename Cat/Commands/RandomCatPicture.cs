namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Displays a random cat picture in a new window.
        /// </summary>
        /// <returns>True upon successful display of the cat picture.</returns>
        /// <remarks>
        /// Utilizes the Helpers.CatWindow class to create and show a window containing a randomly selected cat image.
        /// </remarks>
        [CAspects.ConsumeException]
        [CAspects.Logging]
        internal static bool RandomCatPicture()
        {
            Interface.AddLog("Generating kitty...");
            var r = new Helpers.CatWindow();
            r.Show();
            return true;
        }

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TRandomCatPicture()
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
                    .cmdmap["generate cat"]
                ]["desc"]
            + "\"",
            "There's nothing much to this command, just run it and it'll show a fluffy friend!!! \\o/)"
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("generate cat");
        }
    }
}