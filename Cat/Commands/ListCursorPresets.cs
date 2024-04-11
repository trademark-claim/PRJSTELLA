using System.IO;

namespace Cat
{
    internal static partial class Commands
    {
        [LoggingAspects.Logging]
        [LoggingAspects.ConsumeException]
        internal static bool ListCursorPreset()
        {
            if (commandstruct == null || commandstruct.Value.Parameters[1].Length == 0)
                Interface.AddLog("- " + string.Join("\n- ", Directory.EnumerateDirectories(CursorsFilePath)));
            else
            {
                string dir = commandstruct.Value.Parameters[1][0] as string;
                if (dir == null)
                {
                    var message = "Expected string but parsing failed, command struct or entry was null.";
                    Logging.Log(message);
                    Interface.AddTextLog($"Execution Failed: {message}", RED);
                    return false;
                }
                string fulldir = Path.Combine(CursorsFilePath, dir);
                if (!Directory.Exists(fulldir))
                {
                    Logging.Log($"Requested Directory \"{fulldir}\" does not exist");
                    Interface.AddLog("Preset does not exist.");
                    return false;
                }
                Interface.AddLog("Preset " + dir + "'s edited cursors by ID:\n" +
                    string.Join("\n", BaselineInputs.Cursor.validEntries.Select(entry => entry.Item1).Select(name =>
                        name + (Directory.EnumerateFiles(fulldir).Any(x => x.Contains(name)) ? " ✓" : " ✗"))));
            }

            return true;
        }
    }
}