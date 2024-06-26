using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cat
{
    internal static partial class Commands
    {
        internal static bool OpenSettings()
        {
            new Objects.SettingsWindow().Show();
            Interface.AddLog("Opened Settings Menu!");
            return true;
        }

        /// <summary>
        /// Tutorial for the open settings command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TOpenSettings()
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
                            .cmdmap["open settings"]
                        ].desc
                    + "\"",
                    "There's nothing much to this command, just activate it and a window will open up -- Change the settings you want to, click save and voila! Command done~"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            await StellaHerself.TCS.Task;
        }
    }
}
