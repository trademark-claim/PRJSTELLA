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
        [LoggingAspects.Logging]
        [LoggingAspects.ConsumeException]
        [LoggingAspects.InterfaceNotice]
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
    }
}