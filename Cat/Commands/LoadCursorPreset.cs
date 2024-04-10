using System.IO;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Loads an entire cursor preset
        /// </summary>
        /// <returns>true if successful at all, false otherwise</returns>
        [LoggingAspects.ConsumeException]
        [LoggingAspects.Logging]
        internal static bool LoadCursorPreset()
        {
            var entryN = commandstruct.Value.Parameters[0][0];
            if (entryN == null)
            {
                Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            string dir = Environment.CursorsFilePath + entryN;
            if (!Directory.Exists(dir))
            {
                Logging.Log($"Directory {dir} not found.");
                Interface.AddTextLog($"No preset with name {entryN} found", RED);
                return false;
            }
            string file = dir + "\\preset.CLF";
            Logging.Log("Preset file: " + file);
            if (!File.Exists(file))
            {
                Logging.Log($"Cursor Preset file ({file}) not found! This shouldn't happen unless someone manually removed it.");
                Interface.AddTextLog("Preset file not found, try re-creating this preset! (Remove the currently existing one though).", RED);
                return false;
            }
            string[] content = File.ReadAllLines(file);
            Logging.Log(content);
            foreach (string line in content)
            {
                uint id = 0;
                string[] split = line.Split("|");
                string cursorname = split[0].Trim();
                switch (cursorname)
                {
                    case "OCR_APPSTARTING":
                        id = OCR_APPSTARTING;
                        break;
                    case "OCR_NORMAL":
                        id = OCR_NORMAL;
                        break;
                    case "OCR_CROSS":
                        id = OCR_CROSS;
                        break;
                    case "OCR_HAND":
                        id = OCR_HAND;
                        break;
                    case "OCR_HELP":
                        id = OCR_HELP;
                        break;
                    case "OCR_IBEAM":
                        id = OCR_IBEAM;
                        break;
                    case "OCR_NO":
                        id = OCR_NO;
                        break;
                    case "OCR_SIZEALL":
                        id = OCR_SIZEALL;
                        break;
                    case "OCR_SIZENESW":
                        id = OCR_SIZENESW;
                        break;
                    case "OCR_SIZENS":
                        id = OCR_SIZENS;
                        break;
                    case "OCR_SIZENWSE":
                        id = OCR_SIZENWSE;
                        break;
                    case "OCR_SIZEWE":
                        id = OCR_SIZEWE;
                        break;
                    case "OCR_UP":
                        id = OCR_UP;
                        break;
                    case "OCR_WAIT":
                        id = OCR_WAIT;
                        break;
                    default:
                        Logging.Log($"Unknown cursor name in preset.CLF: {cursorname}, skipping and moving to next cursor.");
                        Interface.AddTextLog($"Unknown cursor name in preset.CLF: {cursorname}, skipping and moving to next cursor.", HOTPINK);
                        continue;
                }
                if (id != 0)
                {
                    string path = split[1].Trim();
                    if (!File.Exists(path))
                    {
                        Logging.Log($"Cursor at {path} does not exist! Skipping.");
                        Interface.AddTextLog($"Cursor path assigned to {cursorname} returned non-existence, skipping.", HOTPINK);
                        continue;
                    }
                    if (!BaselineInputs.Cursor.ChangeCursor(path, id))
                    {
                        Interface.AddTextLog($"Something went wrong loading the cursor for {cursorname}! Check logs for details.\nSkipping cursor for {cursorname}...", HOTPINK);
                        continue;
                    }
                    Logging.Log($"Cursor for {cursorname} successfully changed!");
                }
            }
            return true;
        }
    }
}