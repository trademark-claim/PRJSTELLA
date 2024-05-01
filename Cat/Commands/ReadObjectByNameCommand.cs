﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cat
{
    internal static partial class Commands
    {
        [LoggingAspects.Logging]
        internal static bool ReadObject()
        {
            string file = commandstruct.Value.Parameters[0][0] as string, entry = commandstruct.Value.Parameters[0][1].ToString();
            if (entry == null || file == null)
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
            Dictionary<string, dynamic> decereal;
            using (Helpers.BinaryFileHandler bfh = new(file, true))
            {
                bool success = bfh.FindObjectIndexByName(entry, out int index);
                if (!success || index == -1)
                {
                    Interface.AddLog($"No object with name '{entry}' found");
                    Logging.Log($"No object with name '{entry}' found");
                    return false;
                }
                items = bfh.ExtractObjectAtIndex(index);
                decereal = bfh.DeserialiseObject(items);
            }
            Logging.Log("Data: ");
            Logging.Log(items);
            Interface.AddLog("Data:");
            foreach (var kvp in decereal)
                Interface.AddLog($"{kvp.Key}: {Logging.ProcessMessage(kvp.Value)}");
            return true;
        }
    }
}