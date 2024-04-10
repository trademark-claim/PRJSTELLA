using System.IO;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Creates a new, unique cursor preset
        /// </summary>
        /// <returns>true if successful at all, false otherwise</returns>
        [LoggingAspects.ConsumeException]
        [LoggingAspects.Logging]
        internal static bool AddCursorPreset()
        {
            var entryN = commandstruct.Value.Parameters[0][0];
            if (entryN == null)
            {
                Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            string dir = Environment.CursorsFilePath + entryN;
            if (Directory.Exists(dir))
            {
                Logging.Log($"Cursor Directory {dir} alrady exists.");
                Interface.AddTextLog($"Preset with name {entryN} found, names must be unqiue", RED);
                return false;
            }
            string file = dir + "\\preset.CLF";
            Logging.Log("Creating preset file");
            File.Create(file);
            Logging.Log("Created preset file");
            Interface.AddLog($"Preset {entryN} created");
            return true;
        }
    }
}