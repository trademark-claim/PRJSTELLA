using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cat
{
    internal static partial class Commands
    {
        internal static bool ReadSchema()
        {
            object entry = commandstruct.Value.Parameters[0][0].ToString();
            if (entry == null)
            {
                Logging.Log("Expected int or string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            if (int.TryParse((string)entry, out _))
                entry = int.Parse((string)entry);
            using Helpers.BinaryFileHandler bfh = new(SchemaFile, null);
            List<object> obj;
            if (entry is int a)
                obj = bfh.ExtractObjectAtIndex(a);
            else
            {
                bool b = bfh.ReadSchema((string)entry, out obj);
                if (!b)
                {
                    Interface.AddLog($"Failed to find schema {entry}");
                    Logging.Log($"Failed to find schema {entry}");
                    return false;
                }
            }
            Interface.AddLog(obj[0].ToString());
            foreach (object obje in obj)
            {
                var t = obje as Tuple<string, Helpers.BinaryFileHandler.Types>;
                string m = $"{t.Item1}: {t.Item2}";
                Interface.AddLog(m);
                Logging.Log(m);  
            }
            return true;
        }
    }
}
