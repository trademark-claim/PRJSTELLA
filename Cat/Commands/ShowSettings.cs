using SharpCompress;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Displays the current settings by reading from a configuration file and logging each setting to STELLA's interface.
        /// </summary>
        /// <returns>Always returns true, indicating the method has completed execution.</returns>
        /// <remarks>
        /// Iterates through all settings obtained from the configuration file, logging both the setting name and its value.
        /// </remarks>
        [CAspects.Logging]
        [CAspects.ConsumeException]
        [CAspects.InterfaceNotice]
        internal static bool ShowSettings()
        {
            // Yes, I could break it down, see the below comment for that, but this is more fun :3
            /* 
                var data = Helpers.IniParsing.GetStructure(UserDataFile);
                foreach (string key in data.Keys)
                {
                    Interface.AddLog(key);
                    foreach (KeyValuePair<string, string> kvp in data[key])
                    {
                        Interface.AddLog($"   {kvp.Key}: {kvp.Value}");
                    }
                }
             */
            Helpers.IniParsing.GetStructure(UserDataFile).ToList().ForEach(kv => { Interface.AddLog(kv.Key); kv.Value.ToList().ForEach(subKv => Interface.AddLog($"   {subKv.Key}: {subKv.Value}")); });
            return true;
        }

        /// <summary>
        /// Tutorial for the show settings command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TShowSettings()
        {
            StellaHerself.Custom = [
                "Command description:\n\""
            + Interface.
                CommandProcessing
                .Cmds[Interface
                    .CommandProcessing
                    .cmdmap["show settings"]
                ].desc
            + "\"",
            "There's nothing much to this command, just run it and it'll list all the changeable settings by group."
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await StellaHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("show settings");
        }
    }
}