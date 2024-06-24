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
        /// <summary>
        /// Print Raw Binary command, useful for debugging
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Uses the output from a powershell cmd
        /// </remarks>
        internal static bool PRB()
        {
            if (commandstruct.Value.Parameters[0][0].ToString() is not string para1)
            {
                Logging.Log(["Expected int or string but parsing failed and returned either a null command struct or a null entry, please submit a bug report."]);
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            if (!File.Exists(para1))
            {
                if (File.Exists(Path.Combine(NotesFolder, para1)))
                    para1 = Path.Combine(NotesFolder, para1);
                else
                {
                    Interface.AddLog($"File '{para1}' not found!");
                    return false;
                }
            }
            var output = Helpers.BinaryFileHandler.ReturnRawBinary(para1);
            Interface.AddLog(output);
            Logging.Log([output]);
            return true;
        }
    }
}
