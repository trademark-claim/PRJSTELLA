using System.IO;

namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.Logging]
        [CAspects.ConsumeException]
        [CAspects.InDev]
        internal static bool RemoveCursorFromPreset()
        {
            if (commandstruct?.Parameters[0][0] is not string para1 || commandstruct?.Parameters[0][1] is not string para2)
            {
                Logging.Log(["Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report."]);
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            para1 = para1.Trim();
            para2 = para2.Trim();
            string path = Path.Combine();
            return true;
        }
    }
}