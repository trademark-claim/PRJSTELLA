using System.Text;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Provides help information to the user, either displaying general help or specific command help based on the input.
        /// </summary>
        /// <returns>False if the help request could not be fulfilled, true otherwise.</returns>
        /// <remarks>
        /// If no specific command is requested, displays general help information about the application and how to
        [CAspects.ConsumeException]
        internal static bool Help()
        {
            if (commandstruct == null || commandstruct.Value.Parameters[1].Length < 1)
            {
                Interface.AddLog("Welcome to the help page!\nThis is the interface for the Kitty program, and is where you can run all the commands");
                Interface.AddTextLog("Run 'help ;commands' to see a list of commands\nRun 'help ;(cmdname)\n    E.g: 'help ;screenshot'\n  to see extended help for that command.", System.Windows.Media.Color.FromRgb(0xC0, 0xC0, 0xC0));
                Interface.AddLog("This is a program created to help automate, manage, and improve overall effectiveness of your computer, currently only for Windows machines.");
                Interface.AddLog("Uhhh... don't really know what else to put here apart from some general notes:\n   For the PARAMS field when viewing command specific help, the symbols are defined as such:\n      | means OR, so you can input the stuff on the left OR the stuff on the right of the bar\n      [] means OPTIONAL PARAMETER, in other words you don't need to input it.\n      {} denotes a datatype, the expected type you input. bool is true/false, int is any whole number.");
                return true;
            }
            else
            {
                string str = commandstruct?.Parameters[1][0] as string;
                if (str == null)
                {
                    Logging.Log("Something went wronng when getting the string command input... uh oh......REEEEEEEEEEEEEEEEEEEE");
                    Interface.AddTextLog("[(Potentially?) CRITICAL ERROR] Failed to get string value from inputted parameters, even though ParseCommands() returned true. Send bug report with log, thanks! (or just try again)", System.Windows.Media.Color.FromRgb(0xff, 0xc0, 0xcb));
                    return false;
                }
                if (str == "commands")
                {
                    Interface.AddLog("Heres a list of every command:");
                    string[] s = ["Complete Commands:", "In Development", "Debug Commands"];
                    var list = Interface.CommandProcessing.Cmds.GroupBy(x => x.Value["type"]).ToList();
                    for (int i = 0; i < list.Count; i++) 
                    {
                        Interface.AddLog(s[i]);
                        foreach (var kvp in list[i])
                        {
                            int key = kvp.Key;
                            var firstKey = Interface.CommandProcessing.cmdmap.FirstOrDefault(x => x.Value == key).Key;
                            Interface.AddLog($"- {firstKey}");
                        }
                        Interface.AddLog("");
                    }

                }
                else if (Interface.CommandProcessing.cmdmap.TryGetValue(str, out int result))
                {
                    var Keys = Interface.CommandProcessing.cmdmap.Where(x => x.Value == result).Select(x => x.Key).ToArray();
                    var metadata = Interface.CommandProcessing.Cmds[result];
                    Interface.AddLog($"Command: {Keys[0]}");
                    Interface.AddLog($"Description: {metadata["desc"]}");
                    Interface.AddLog($"Parameter Format: {metadata["params"]}");
                    Interface.AddLog($"Shortcut: {metadata["shortcut"]}");
                    Interface.AddLog($"Aliases: {string.Join(", ", Keys)}");
                }
                else
                {
                    Logging.Log($"Failed to find command for help command {str}");
                    Interface.AddLog($"Failed to find command '{str}'.");
                    return false;
                }
                return true;
            }
        }

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task THelp()
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
                    .cmdmap["close log editor"]
                ]["desc"]
            + "\"",
            "This is the help command, a central command.",
            "You can run it with 'help', and it takes an optional parameter of 'commands' or a specific command name",
            "'help' shows every command by group",
            "'help ;commands' shows a list of all commands by group",
            "'help ;(commandName)', like 'help ;define' shows the help page for that individual command",
            "Here's 'help ;define' run for you:"
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("help ;define");
        }
    }
}