using System.IO;

namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.ConsumeException]
        internal static bool ChangeCursor()
        {
            var entryN = commandstruct?.Parameters[0][0] as string;
            if (entryN == null)
            {
                var message = "Expected string but parsing failed, command struct or entry was null.";
                Logging.Log(message);
                Interface.AddTextLog($"Execution Failed: {message}", RED);
                return false;
            }
            if (!File.Exists(entryN) || (!entryN.EndsWith(".ani") && !entryN.EndsWith(".cur")))
            {
                Logging.Log($"{entryN} does not exist / could not be found as a file");
                Interface.AddTextLog("Please input a valid filepath! (.ani, .cur)", RED);
                return false;
            }
            Interface.AddLog($"Changing to {entryN}...");
            BaselineInputs.Cursor.ChangeCursor(entryN);
            return true;
        }
    }
}