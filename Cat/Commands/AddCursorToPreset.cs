using System.IO;

namespace Cat
{
    internal static partial class Commands
    {
        private static HashSet<(string, string)> validEntries = new()
        {
            ("OCR_APPSTARTING", "The cursor indicating that an application is starting or loading."),
            ("OCR_NORMAL", "The normal cursor, usually a pointer for selection."),
            ("OCR_CROSS", "A crosshair cursor, often used for precise alignment or selection."),
            ("OCR_HAND", "A hand cursor, typically used to indicate a clickable link or object."),
            ("OCR_HELP", "A help cursor, usually a question mark or pointer with a question mark, indicating help or more information is available."),
            ("OCR_IBEAM", "An I-beam cursor, used to indicate that text can be edited or selected."),
            ("OCR_NO", "A cursor indicating that an action cannot be taken, often shown as a circle with a line through it."),
            ("OCR_SIZEALL", "A sizing cursor, indicating that an object can be resized in any direction."),
            ("OCR_SIZENESW", "A diagonal sizing cursor, indicating resizing from the northeast to southwest or vice versa."),
            ("OCR_SIZENS", "A vertical sizing cursor, indicating resizing in the north-south direction."),
            ("OCR_SIZENWSE", "A diagonal sizing cursor, indicating resizing from the northwest to southeast or vice versa."),
            ("OCR_SIZEWE", "A horizontal sizing cursor, indicating resizing in the west-east direction."),
            ("OCR_UP", "An up arrow cursor, often used to indicate an upward action or movement."),
            ("OCR_WAIT", "A wait cursor, typically shown as an hourglass or spinning circle, indicating a process is ongoing and the user must wait.")
        };


        /// <summary>
        /// Adds a cursor to a preset
        /// </summary>
        /// <returns>true if successful at all, false otherwise</returns>
        [LoggingAspects.ConsumeException]
        [LoggingAspects.Logging]
        internal static bool AddCursorToPreset()
        {
            string entryN = commandstruct.Value.Parameters[0][0] as string, 
                entryM = commandstruct.Value.Parameters[0][1] as string, 
                entryZ = commandstruct.Value.Parameters[0][2] as string;
            if (entryN == null || entryM == null || entryZ == null)
            {
                Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Catowo.Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            entryN = entryN.Trim();
            entryM = entryM.Trim();
            entryZ = entryZ.Trim();
            string dir = Path.Combine(Environment.CursorsFilePath, entryN);
            if (!Directory.Exists(dir))
            {
                Logging.Log($"Directory {dir} not found.");
                Catowo.Interface.AddTextLog($"No preset with name {entryN} found", RED);
                return false;
            }
            string file = Path.Combine(dir, "preset.CLF");
            Logging.Log("Preset file: " + file);
            if (!File.Exists(file))
            {
                Logging.Log($"Cursor Preset file ({file}) not found! This shouldn't happen unless someone manually removed it.");
                Catowo.Interface.AddTextLog("Preset file not found, try re-creating this preset! (Remove the currently existing one though).", RED);
                return false;
            }
            if (!File.Exists(entryZ)) 
            {
                Logging.Log($"Requested file {entryZ} not found.");
                Catowo.Interface.AddTextLog($"Requested file {entryZ} not found! Please verify your file and provide a full, absolute file path.", RED);
                return false;
            }
            if (!entryZ.EndsWith(".ani") && !entryZ.EndsWith(".cur"))
            {
                Logging.Log($"Requested file {entryZ} not a .ani or .cur file.");
                Catowo.Interface.AddTextLog($"Requested file {entryZ} was not a supported cursor file! (.ani / .cur).", RED);
                return false;
            }
            bool animated = entryZ.EndsWith(".ani");
            entryM = entryM.ToUpper().Trim();
            if (!(entryM == "OCR_APPSTARTING" || entryM == "OCR_NORMAL" || entryM == "OCR_CROSS" || entryM == "OCR_HAND" ||
                  entryM == "OCR_HELP" || entryM == "OCR_IBEAM" || entryM == "OCR_NO" || entryM == "OCR_SIZEALL" ||
                  entryM == "OCR_SIZENESW" || entryM == "OCR_SIZENS" || entryM == "OCR_SIZENWSE" || entryM == "OCR_SIZEWE" ||
                  entryM == "OCR_UP" || entryM == "OCR_WAIT"))
            {
                Logging.Log($"Expected a cursor id constant name, but got {entryM}");
                Catowo.Interface.AddTextLog("Error: Expected a cursor constant name from the below list:", RED);
                Interface.AddLog(string.Join("\n", validEntries.Select(entry => $"- {entry.Item1}: {entry.Item2}")));
                return false;
            }
            string finalpath = Path.Combine(dir, $"{entryM}_csr.{(animated ? "ani" : "cur")}");
            Logging.Log($"Final path: {finalpath}");
            try
            {
                File.Copy(entryZ, finalpath, true);
                File.Delete(entryZ);
            }
            catch (Exception e)
            {
                Interface.AddTextLog("Failed to cut file to local path! See logs for error.", RED);
                Logging.Log($"Failed to copy and delete {entryZ}, see below exception.");
                Logging.LogError(e);
                return false;
            }
            var content = File.ReadAllLines(file).ToList();
            content.RemoveAll(line => line.Contains(entryM));
            content.Add($"{entryM} | {finalpath}");
            File.WriteAllLines(file, content.ToArray());
            return true;
        }
    }
}