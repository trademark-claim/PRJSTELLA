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
                var entryN = commandstruct?.Parameters[0][0] as string;
                var entryM = commandstruct?.Parameters[0][1] as string;

                if (entryN == null || entryM == null)
                {
                    var message = "Expected string but parsing failed, command struct or entry was null.";
                    Logging.Log(message);
                    Interface.AddTextLog($"Execution Failed: {message}", RED);
                    return false;
                }

                var normalizedKey = entryN.ToLower().Trim();
                var data = Helpers.IniParsing.GetStructure(UserDataFile);

                Logging.Log("Processing NM:", entryN, entryM);

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
                                if (float.TryParse(entryM, out float result) &&
                                    result >= range.Item1 && result <= range.Item2)
                                {
                                    UserData.UpdateValue(kvp.Key, entryM);
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
                                if (bool.TryParse(entryM, out bool result))
                                {
                                    UserData.UpdateValue(kvp.Key, entryM);
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
                                UserData.UpdateValue(kvp.Key, entryM);
                                Helpers.IniParsing.UpAddValue(UserDataFile, section, kvp.Key, entryM);
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

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TChangeSettings()
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
                            .cmdmap["change settings"]
                        ]["desc"]
                    + "\"",
                    "This command takes two inputs: variablename{string}, and value{string}",
                    "The former is the name of the setting you want to change\nThe latter being the value of that setting.",
                    "The list of variables are as such:"
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("view settings");
            ClaraHerself.Custom = [
                    "Lets change the font size to 15!",
                    "Fontsize must be a floating point value between 1.0 and 50.0.",
                    "We'll be using the command 'cs ;FontSize ;15' for this",
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Interface.Input = "cs ;FontSize ;15";
            Interface.CommandProcessing.ProcessCommand();
        }
    }
}