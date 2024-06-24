namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Updates a specific setting based on user input, affecting the application's configuration.
        /// </summary>
        /// <returns>True if the setting is updated successfully, false if the setting name is invalid or the value is not appropriate.</returns>
        /// <remarks>
        /// Parses the setting name and value from the user input, validating against known settings and applying the change if valid.
        /// </remarks>
        [CAspects.Logging]
        [CAspects.ConsumeException]
        internal static bool ChangeSettings()
        {
            try
            {
                var para1 = commandstruct?.Parameters[0][0] as string;
                var para2 = commandstruct?.Parameters[0][1] as string;

                if (para1 == null || para2 == null)
                {
                    var message = "Expected string but parsing failed, command struct or entry was null.";
                    Logging.Log(message);
                    Interface.AddTextLog($"Execution Failed: {message}", RED);
                    return false;
                }

                var normalizedKey = para1.ToLower().Trim();
                var data = Helpers.IniParsing.GetStructure(UserDataFile);

                Logging.Log("Processing NM:", para1, para2);

                foreach (var section in data.Keys)
                {
                    foreach (KeyValuePair<string, string> kvp in data[section])
                    {
                        var currentKey = kvp.Key.ToLower().Trim();
                        if (currentKey == normalizedKey)
                        {
                            if (!Helpers.IniParsing.validation.ContainsKey(kvp.Key))
                            {
                                Logging.Log($"Validation for {kvp.Key} not found.");
                                continue;
                            }

                            var (type, constraints) = Helpers.IniParsing.validation[kvp.Key];
                            if ((Type)type == typeof(float) && constraints is Tuple<float, float> range)
                            {
                                if (float.TryParse(para2, out float result) &&
                                    result >= range.Item1 && result <= range.Item2)
                                {
                                    UserData.UpdateValue(kvp.Key, para2);
                                    Helpers.IniParsing.UpAddValue(UserDataFile, section, kvp.Key, result.ToString());
                                }
                                else
                                {
                                    Interface.AddLog($"Invalid value for {kvp.Key}. Expected a float in the range {range.Item1}-{range.Item2}.");
                                    return false;
                                }
                            }
                            else if ((Type)type == typeof(bool))
                            {
                                if (bool.TryParse(para2, out bool result))
                                {
                                    UserData.UpdateValue(kvp.Key, para2);
                                    Helpers.IniParsing.UpAddValue(UserDataFile, section, kvp.Key, result.ToString());
                                }
                                else
                                {
                                    Interface.AddLog($"Invalid value for {kvp.Key}. Expected a boolean.");
                                    return false;
                                }
                            }
                            else
                            {
                                UserData.UpdateValue(kvp.Key, para2);
                                Helpers.IniParsing.UpAddValue(UserDataFile, section, kvp.Key, para2);
                            }

                            Interface.AddLog($"Updated {kvp.Key} in section {section}.");
                            return true;
                        }
                    }
                }

                Interface.AddLog("Key not found.");
                return false;
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
                Interface.AddTextLog("An unexpected error occurred, check logs for details.", RED);
                return false;
            }
        }
        
        /// <summary>
        /// Tutorial for the Change settings command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TChangeSettings()
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
                            .cmdmap["change settings"]
                        ].desc
                    + "\"",
                    "This command takes two inputs: variablename{string}, and value{string}",
                    "The former is the name of the setting you want to change\nThe latter being the value of that setting.",
                    "The list of variables are as such:"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var continu = await StellaHerself.TCS.Task;
            if (!continu) return;
            Interface.CommandProcessing.ProcessCommand("view settings");
            StellaHerself.Custom = [
                    "Lets change the font size to 15!",
                    "Fontsize must be a floating point value between 1.0 and 50.0.",
                    "We'll be using the command 'cs ;FontSize ;15' for this",
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            continu = await StellaHerself.TCS.Task;
            if (!continu) return;
            Interface.Input = "cs ;FontSize ;15";
            Interface.CommandProcessing.ProcessCommand();
        }
    }
}