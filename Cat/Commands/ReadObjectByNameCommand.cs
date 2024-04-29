using System;
using System.Collections.Generic;
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
            string entry = commandstruct.Value.Parameters[0][0].ToString();
            if (entry == null)
            {
                Interface.AddLog("Query was null, reQturning");
                Logging.Log("Query was null, returning");
                return false;
            }
            List<object> items;
            using (Helpers.BinaryFileHandler bfh = new(StatsFile, true))
            {
                bool success = bfh.FindObjectIndexByName(entry, out int index);
                if (!success || index == -1)
                {
                    Interface.AddLog($"No object with name '{entry}' found");
                    Logging.Log($"No object with name '{entry}' found");
                    return false;
                }
                items = bfh.ExtractObjectAtIndex(index);
            }
            Interface.AddLog("Data: ");
            Interface.AddLog(Logging.ProcessMessage(items));
            return true;
        }
    }
}
