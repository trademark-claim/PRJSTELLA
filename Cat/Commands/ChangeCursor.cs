using System.IO;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Command to change your default cursor
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.ConsumeException]
        internal static bool ChangeCursor()
        {
            var para1 = commandstruct?.Parameters[0][0] as string;
            if (para1 == null)
            {
                var message = "Expected string but parsing failed, command struct or entry was null.";
                Logging.Log(message);
                Interface.AddTextLog($"Execution Failed: {message}", RED);
                return false;
            }
            if (!File.Exists(para1) || (!para1.EndsWith(".ani") && !para1.EndsWith(".cur")))
            {
                Logging.Log($"{para1} does not exist / could not be found as a file");
                Interface.AddTextLog("Please input a valid filepath! (.ani, .cur)", RED);
                return false;
            }
            Interface.AddLog($"Changing to {para1}...");
            BaselineInputs.Cursor.ChangeCursor(para1);
            return true;
        }


        /// <summary>
        /// Tutorial for the change cursor command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TChangeCursor()
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
                            .cmdmap["change cursor"]
                        ].desc
                    + "\"",
                    "This is the ChangeCursor command, used to change your normal cursor to a .cur or .ani file.",
                    "I'll walk you through changing it to a... cat!",
                    "First, I'll download the cat cursor, one second~"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var continu = await StellaHerself.TCS.Task;
            if (!continu)
                return;
            Helpers.ExternalDownloading.FromGDrive(SingleCat, Path.Combine(ExternalDownloadsFolder, "cat.ani"));
            await Helpers.ExternalDownloading.TCS.Task;
            StellaHerself.Custom = [
                    $"Okay, cursor downloaded to {Path.Combine(ExternalDownloadsFolder, "cat.ani")}!",
                    "Now, lets type the command out and run it!"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            continu = await StellaHerself.TCS.Task;
            if (!continu)
                return;
            Interface.Input = $"change cursor ;{Path.Combine(ExternalDownloadsFolder, "cat.ani")}";
            StellaHerself.Custom = [
                    $"That's the full command,\nafter we run it you should see some UI output\nand your normal cursor should now be a cat!",
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            continu = await StellaHerself.TCS.Task;
            if (!continu)
                return;
            Interface.CommandProcessing.ProcessCommand();
        }
    }
}