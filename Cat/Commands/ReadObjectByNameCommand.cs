using NAudio.SoundFont;
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
        /// Complex command for reading an object from a binary file
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.ConsumeException]
        internal static bool ReadObject()
        {
            if (commandstruct.Value.Parameters[0][1].ToString() is not string para1 || commandstruct.Value.Parameters[0][0] is not string file)
            {
                Logging.Log("Expected int or string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            if (!File.Exists(file))
            {
                if (File.Exists(Path.Combine(NotesFolder, file)))
                    file = Path.Combine(NotesFolder, file);
                else
                {
                    Interface.AddLog($"File '{file}' not found!");
                    return false;
                }
            }
            List<object> items;
            Dictionary<string, dynamic> de_cereal;
            using (Helpers.BinaryFileHandler bfh = new(file, true))
            {
                bool success = bfh.FindObjectIndexByName(para1, out int index);
                if (!success || index == -1)
                {
                    Interface.AddLog($"No object with name '{para1}' found");
                    Logging.Log($"No object with name '{para1}' found");
                    return false;
                }
                items = bfh.ExtractObjectAtIndex(index);
                de_cereal = bfh.DeserialiseObject(items);
            }
            Logging.Log("Data: ");
            Logging.Log(items);
            Interface.AddLog("Data:");
            foreach (var kvp in de_cereal)
                Interface.AddLog($"{kvp.Key}: {Logging.ProcessMessage(kvp.Value)}");
            return true;
        }

        /// <summary>
        /// 'Tutorial' for the read object command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TReadObject()
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
                    .cmdmap["read object"]
                ].desc
            + "\"",
            "Complex command for my binary storage functionality. Please refer to the 'Binary Storage' section of the user manual for details."
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await StellaHerself.TCS.Task;
            if (!b) return;
        }
    }
}
