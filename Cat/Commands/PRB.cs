using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cat
{
    internal static partial class Commands
    {
        internal static bool PRB()
        {
            var s = Helpers.BinaryFileHandler.ReturnRawBinary("stats.bin");
            Interface.AddLog(s);
            Logging.Log(s);
            return true;
        }
    }
}
