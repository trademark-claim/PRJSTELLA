using Microsoft.VisualBasic;
using SharpCompress;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Provides help information to the user, either displaying general help or specific command help based on the input.
        /// </summary>
        /// <returns>False if the help request failed, true otherwise.</returns>
        /// <remarks>
        /// If no specific command is requested, displays general help information about me
        /// </remarks>
        [CAspects.Logging]
        [CAspects.ConsumeException]
        internal static bool Help()
        {
            if (commandstruct == null || commandstruct.Value.Parameters[1].Length < 1)
            {
                Interface.AddLog("Welcome to the help page!\nThis is STELLA's interface for the Kitty program, and is where you can run all the commands");
                Interface.AddTextLog("Run 'help ;commands' to see a list of commands\nRun 'help ;(cmdname)\n    E.g: 'help ;screenshot'\n  to see extended help for that command.", System.Windows.Media.Color.FromRgb(0xC0, 0xC0, 0xC0));
                Interface.AddLog("This is a program created to help automate, manage, and improve overall effectiveness of your computer, currently only for Windows machines.");
                Interface.AddLog("Uhhh... don't really know what else to put here apart from some general notes:\n   For the PARAMS field when viewing command specific help, the symbols are defined as such:\n      | means OR, so you can input the stuff on the left OR the stuff on the right of the bar\n      [] means OPTIONAL PARAMETER, in other words you don't *need* to input it, they'll often have default values.\n      {} denotes a datatype, the expected type you input. bool is true/false, int is any whole number.");
                Interface.AddLog("Oh, and:\n Voice Commands, Shortcuts and Documentational help is here!\nRun 'help ;voice commands' to see the voice commands.\nRun 'help ;shortcuts' to see the keyboard shortcuts.\nRun 'help ;documentation' to see the documentation");
                return true;
            }
            else
            {
                string para1 = commandstruct?.Parameters[1][0] as string;
                if (para1 == null)
                {
                    Logging.Log(["Something went wronng when getting the string command input... uh oh......"]);
                    Interface.AddTextLog("[(Potentially?) CRITICAL ERROR] Failed to get string value from inputted parameters, even though ParseCommands() returned true. Send bug report with log, thanks! (or just try again)", System.Windows.Media.Color.FromRgb(0xff, 0xc0, 0xcb));
                    return false;
                }
                if (para1 == "commands")
                {
                    Interface.AddLog("Heres a list of every command:");
                    string[] catagories = ["Complete Commands:", "In Development", "Debug Commands"];
                    var commands = Interface.CommandProcessing.Cmds.GroupBy(x => x.Value.type).ToList();
                    for (int i = 0; i < commands.Count; i++) 
                    {
                        Interface.AddLog(catagories[i]);
                        foreach (var kvp in commands[i])
                        {
                            int key = kvp.Key;
                            var firstKey = Interface.CommandProcessing.cmdmap.FirstOrDefault(x => x.Value == key).Key;
                            Interface.AddLog($"- {firstKey}");
                        }
                        Interface.AddLog("");
                    }

                }
                else if (para1 == "documentation")
                {
                    Interface.AddLog("Here're the documentation links: ");
                    void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
                        e.Handled = true;
                    }

                    (string, string)[] strings = [
                        ("https://github.com/trademark-claim/laughing-octo-garbanzo/blob/master/README.md", "ReadMe"),
                        ("https://github.com/trademark-claim/laughing-octo-garbanzo/blob/master/Installation.md", "Installation Guide"),
                        ("https://github.com/trademark-claim/laughing-octo-garbanzo/blob/master/ref_manual.md", "Reference manual"),
                        ("https://github.com/trademark-claim/laughing-octo-garbanzo/blob/master/user_guide.md", "User Guide")
                        ];

                    foreach ((string s1, string s2) in strings) {
                        TextBlock textBlock = new();
                        textBlock.Inlines.Add(" - Click ");
                        Hyperlink hyperlink = new(new Run("here"))
                        {
                            NavigateUri = new Uri(s1)
                        };
                        hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
                        textBlock.Inlines.Add(hyperlink);
                        textBlock.Inlines.Add(" for the " + s2);
                        Interface.logListBox.Items.Add(textBlock);
                    }
                }
                else if (para1 == "shortcuts")
                {
                    ((string[])[
                        "Here are the shortcuts: ",
                        "Primary Shortcuts:",
                        "LShift + RShift + Q + E: Triggers the shutdown sequence",
                        "LShift + RShift + Q + I: Opens the interface",
                        "LShift + RShift + Q + V: Toggles the STT stuff",
                        "LShift + RShift + Q + L: Opens a Log Editor",
                        "LShift + RShift + Q + O: Opens the local data folder",
                        "LShift + RShift + Q + B: Opens a live logger",
                        "",
                        "Quick Clicks (Q C) Shortcuts;",
                        "Q + C + K: Force kills the active window",
                        "Q + C + 0: Resets your cursors",
                        "Q + C + 1-9: (Literally Press 1,2,3,4...8, or 9) Toggles the preset linked to that number",
                        "Q + C + E: Toggles the cursor trail and click effects",
                        "Q + C + B: Takes a screenshot of every connected screen",
                        "Q + C + N: Takes a stitch screenshot",
                        "",
                        "Macros:",
                        "M + T + 1-9: (Literally Press 1,2,3,4...8, or 9) Sends the macro keystrokes",
                        "M + T + E: Opens the Macro Editor"
                        ]).ForEach(x => Interface.AddLog(x));
                }
                else if (para1 == "voice commands")
                {
                    Interface.AddLog("Voice commands:");

                    var groupedCommands = commandMap
                        .SelectMany(c => c.Value, (category, command) => new { Command = command.Key, Description = command.Value.Description, Category = category.Key, command.Value.InnerCommand})
                        .GroupBy(c => c.Description)
                        .Select((group, index) => new
                        {
                            CommandNumber = index + 1,
                            Description = group.Key,
                            InnerCommand = group.First().InnerCommand,
                            Commands = group.Select(g => $"\"{g.Command} {g.Category}\"").ToList()
                        });

                    foreach (var group in groupedCommands)
                    {
                        Interface.AddLog($"Command #{group.CommandNumber}");
                        Interface.AddLog($"      Called with: {string.Join(", ", group.Commands)}");
                        Interface.AddLog($"      Description: {group.Description}");
                        if (group.InnerCommand != "")
                            Interface.AddLog("      Command Version: '" + group.InnerCommand + "'");
                    }
                }

                else if (Interface.CommandProcessing.cmdmap.TryGetValue(para1, out int result))
                {
                    var Keys = Interface.CommandProcessing.cmdmap.Where(x => x.Value == result).Select(x => x.Key).ToArray();
                    var metadata = Interface.CommandProcessing.Cmds[result];
                    Interface.AddLog($"Command: {Keys[0]}");
                    Interface.AddLog($"Description: {metadata.desc}");
                    Interface.AddLog($"Parameter Format: {metadata.parameters}");
                    Interface.AddLog($"Shortcut: {metadata.shortcut}");
                    Interface.AddLog($"Aliases: {string.Join(", ", Keys)}");
                }
                else
                {
                    Logging.Log([$"Failed to find command for help command {para1}"]);
                    Interface.AddLog($"Failed to find command '{para1}'.");
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Tutorial for the help command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task THelp()
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
                    .cmdmap["help"]
                ].desc
            + "\"",
            "This is the help command, a central command.",
            "You can run it with 'help', and it takes an optional parameter of 'commands' or a specific command name",
            "'help' shows every command by group",
            "'help ;commands' shows a list of all commands by group",
            "'help ;(commandName)', like 'help ;define' shows the help page for that individual command",
            "Here's 'help ;define' run for you:"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await StellaHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("help ;define");
        }
    }
}