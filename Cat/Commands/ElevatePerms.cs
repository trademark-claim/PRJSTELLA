namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Command that checks if the app is running as admin, and if not, restarts asking for it
        /// </summary>
        /// <returns>true if already has admin, otherwise nothing as it kills the application.</returns>
        [CAspects.UpsetStomach]
        [CAspects.ConsumeException]
        [CAspects.Logging]
        internal static bool KillMyselfAndGetGodPowers()
        {
            bool? rah = Helpers.BackendHelping.RestartWithAdminRightsIfNeeded();
            if (rah == null)
            {
                Interface.AddLog("Already has admin perms");
                return true;
            }
            return rah ?? true;
        }

        /// <summary>
        /// Tutorial for the elevate perms command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TKillMyselfAndGetGodPowers()
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
                    .cmdmap["elevate perms"]
                ].desc
            + "\"",
            "There's nothing much to this command, just run it ('elevate perms') and it'll attempt to restart me but with elevated perms -- keep in mind that all volatile states will be wiped.",
            "If you want me to run this command, press the right arrow key, else press the up arrow."
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await StellaHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("elevate perms");
        }
    }
}