using System.IO;

namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.Logging]
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

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TChangeCursor()
        {
            ClaraHerself.Fading = false;
            ClaraHerself.HaveOverlay = false;
            ClaraHerself.CleanUp = false;
            ClaraHerself.Custom = [
                "Command description:\n\""
                    + (string)Interface.
                        CommandProcessing
                        .Cmds[Interface
                            .CommandProcessing
                            .cmdmap["change cursor"]
                        ]["desc"]
                    + "\"",
                    "This is the ChangeCursor command, used to change your normal cursor to a .cur or .ani file.",
                    "I'll walk you through changing it to a... cat!",
                    "First, I'll download the cat cursor, one second~"
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            var can = await ClaraHerself.TCS.Task;
            if (!can)
                return;
            Helpers.ExternalDownloading.FromGDrive(SingleCat, Path.Combine(ExternalDownloadsFolder, "cat.ani"));
            await Helpers.ExternalDownloading.TCS.Task;
            ClaraHerself.Custom = [
                    $"Okay, cursor downloaded to {Path.Combine(ExternalDownloadsFolder, "cat.ani")}!",
                    "Now, lets type the command out and run it!"
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            can = await ClaraHerself.TCS.Task;
            if (!can)
                return;
            Interface.Input = $"change cursor ;{Path.Combine(ExternalDownloadsFolder, "cat.ani")}";
            ClaraHerself.Custom = [
                    $"That's the full command,\nafter we run it you should see some UI output\nand your normal cursor should now be a cat!",
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            can = await ClaraHerself.TCS.Task;
            if (!can)
                return;
            Interface.CommandProcessing.ProcessCommand();
        }
    }
}