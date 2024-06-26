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
        internal static async Task<bool> RandomCatPicture()
        {
            int para1 = 1;
            if (commandstruct?.Parameters?[0].Length > 0)
            {
                if (commandstruct?.Parameters[0][0] is int para2 && para2 > 0)
                {
                    para1 = Math.Min(50, para2);
                }
            }
            Logging.Log(["Params:", commandstruct?.Parameters]);
            Logging.Log(["Cat amount: ", para1]);
            for (int i = 0; i < para1; i++)
            {
                Interface.AddLog("Generating kitty...");
                var kat_window = new Helpers.CatWindow();
                kat_window.Show();
            }

            return true;
        }


        /// <summary>
        /// Tutorial for the Kat Generator command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TRandomCatPicture()
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
                    .cmdmap["generate cat"]
                ].desc
            + "\"",
            "There's nothing much to this command, just run it and it'll show a fluffy friend!!! \\o/)"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await StellaHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("generate cat");
        }
    }
}