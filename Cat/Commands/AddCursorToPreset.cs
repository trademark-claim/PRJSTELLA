using System.IO;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Adds a cursor to a preset
        /// </summary>
        /// <returns>true if successful at all, false otherwise</returns>
        [LoggingAspects.ConsumeException]
        [LoggingAspects.Logging]
        internal static bool AddCursorToPreset()
        {
            string entryN = commandstruct.Value.Parameters[0][0] as string, entryM = commandstruct.Value.Parameters[0][1] as string;
            if (entryN == null)
            {
                Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Catowo.Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            string dir = Environment.CursorsFilePath + entryN;
            if (!Directory.Exists(dir))
            {
                Logging.Log($"Directory {dir} not found.");
                Catowo.Interface.AddTextLog($"No preset with name {entryN} found", RED);
                return false;
            }
            string file = dir + "\\preset.CLF";
            Logging.Log("Preset file: " + file);
            if (!File.Exists(file))
            {
                Logging.Log($"Cursor Preset file ({file}) not found! This shouldn't happen unless someone manually removed it.");
                Catowo.Interface.AddTextLog("Preset file not found, try re-creating this preset! (Remove the currently existing one though).", RED);
                return false;
            }
            uint id = 0;
            switch (entryN)
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
                    Logging.Log($"Expected a cursor id constant name, but got {entryM}");
                    Catowo.Interface.AddTextLog("Error", HOTPINK); //todo Complete this
                    return false;
            }
            return true;
        }
    }
}