namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Displays the current settings by reading from a configuration file and logging each setting to the interface.
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
            var data = Helpers.IniParsing.GetStructure(UserDataFile);
            foreach (string key in data.Keys)
            {
                Interface.AddLog(key);
                foreach (KeyValuePair<string, string> kvp in data[key])
                {
                    Interface.AddLog($"   {kvp.Key}: {kvp.Value}");
                }
            }
            return true;
        }

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TShowSettings()
        {
            ClaraHerself.Custom = [
                "Command description:\n\""
            + (string)Interface.
                CommandProcessing
                .Cmds[Interface
                    .CommandProcessing
                    .cmdmap["close log editor"]
                ]["desc"]
            + "\"",
            "There's nothing much to this command, just run it and it'll list all the changeable settings by group."
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("show settings");
        }
    }
}