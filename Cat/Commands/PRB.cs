using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cat
{
    internal static partial class Commands
    {
        internal static bool PRB()
        {
            string entry = commandstruct.Value.Parameters[0][0].ToString();
            if (entry == null)
            {
                Logging.Log("Expected int or string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            if (!File.Exists(entry))
            {
                if (File.Exists(Path.Combine(NotesFolder, entry)))
                    entry = Path.Combine(NotesFolder, entry);
                else
                {
                    Interface.AddLog($"File '{entry}' not found!");
                    return false;
                }
            }
            var s = Helpers.BinaryFileHandler.ReturnRawBinary(entry);
            Interface.AddLog(s);
            Logging.Log(s);
            return true;
        }
    }
}
