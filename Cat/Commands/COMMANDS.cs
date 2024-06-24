//#define SAVETOCLIPBOARD

using NAudio.Wave;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Cat
{
    /// <summary>
    /// Class that holds every single commamd 
    /// </summary>
    internal static partial class Commands
    {
        /// <summary>
        /// Interface reference link, exact same as <see cref="Catowo.Interface.inst"/>
        /// </summary>
        internal static Interface @interface;
        /// <summary>
        /// Instance link to an open <see cref="LogEditor"/> window
        /// </summary>
        private static Objects.LogEditor editor;
        /// <summary>
        /// Instance link to the waveplayer for audio playing
        /// </summary>
        private static WaveOut? WavePlayer;
        /// <summary>
        /// Audio file reader for play audio command, used in tangent with the <see cref="WavePlayer"/>
        /// </summary>
        private static AudioFileReader AFR;
        /// <summary>
        /// The data of the command to be executed / being executed, changes with <see cref="Interface.CommandProcessing.ProcessCommand(string)"/>
        /// </summary>
        internal static Command? commandstruct;
        /// <summary>
        /// Whether or not to throw messages with audio clean up
        /// </summary>
        private static bool SilentAudioCleanup = false;
        /// <summary>
        /// Reference to the instance of an open console
        /// </summary>
        private static System.Windows.Window? Logger = null;
        /// <summary>
        /// Object to hold temporary information between methods
        /// </summary>
        private static object TempHolder;

        /// <summary>
        /// Tells the program that, when listening for a voice command, does it need the 'Hey Stella' prefix or not.
        /// If true, no prefix is needed.
        /// </summary>
        private static bool NoCall => UserData.RequireNameCallForVoiceCommands;

        /// <summary>
        /// The command to execute
        /// </summary>
        private static string command;
        /// <summary>
        /// The output feedback to the user
        /// </summary>
        private static string mess;
        /// <summary>
        /// The raw audio
        /// </summary>
        private static string rawdio;

        /// <summary>
        /// Voice command mapping, where keys are keywords and end-values are Actions that determine the command and output message
        /// </summary>
        /// <remarks>
        /// " " means no second keyword needed, or a default case
        /// </remarks>
        private static readonly Dictionary<string, Dictionary<string, Action>> commandMap = new Dictionary<string, Dictionary<string, Action>>()
        {
            {
                "screenshot", new Dictionary<string, Action>()
                {
                    { "primary", () => { command = "screenshot ;0"; mess = "Screenshot taken!"; } },
                    { "stitch", () => { command = "screenshot ;-2"; mess = "Screenshot taken!"; } },
                    { " ", () => { command = "screenshot ;-1"; mess = "Screenshot taken!"; } }
                }
            },
            {
                "shutdown", new Dictionary<string, Action>()
                {
                    { " ", () => { command = "shutdown"; } }
                }
            },
            {
                "kitty", new Dictionary<string, Action>()
                {
                    { "show", () => { command = "cat"; mess = "Generating cat!";  } },
                    { "generate", () => { command = "cat"; mess = "Generating cat!"; } }
                }
            },
            {
                "cat", new Dictionary<string, Action>()
                {
                    { "show", () => { command = "cat"; mess = "Generating cat!"; } },
                    { "generate", () => { command = "cat"; mess = "Generating cat!"; } }

                }
            },
            {
                "song", new Dictionary<string, Action>()
                {
                    { "skip", () => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_NEXT_TRACK) },
                    { "next", () => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_NEXT_TRACK) },
                    { "previous", () => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PREV_TRACK) },
                    { "resume", () => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PLAY_PAUSE) },
                    { "continue", () => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PLAY_PAUSE) },
                    { "pause", () => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PLAY_PAUSE) },
                    { "stop", () => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PLAY_PAUSE) }
                }
            },
            {
                "music", new Dictionary<string, Action>()
                {
                    { "skip", () => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_NEXT_TRACK) },
                    { "next", () => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_NEXT_TRACK) },
                    { "previous", () => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PREV_TRACK) },
                    { "resume", () => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PLAY_PAUSE) },
                    { "continue", () => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PLAY_PAUSE) },
                    { "pause", () => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PLAY_PAUSE) },
                    { "stop", () => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PLAY_PAUSE) }
                }
            },
            {
                "volume", new Dictionary<string, Action>()
                {
                    { "low", () => BaselineInputs.DecrVol20() },
                    { "decrease", () => BaselineInputs.DecrVol20() },
                    { "raise", () => BaselineInputs.IncrVol20() },
                    { "increase", () => BaselineInputs.IncrVol20() }
                }
            },
            {
                "sound", new Dictionary<string, Action>()
                {
                    { "low", () => BaselineInputs.DecrVol20() },
                    { "decrease", () => BaselineInputs.DecrVol20() },
                    { "raise", () => BaselineInputs.IncrVol20() },
                    { "increase", () => BaselineInputs.IncrVol20() },
                    { "mute", () => BaselineInputs.ToggleMuteSound() },
                    { "silence", () => BaselineInputs.ToggleMuteSound() },
                    { "unmute", () => BaselineInputs.ToggleMuteSound() },
                    { "unsilence", () => BaselineInputs.ToggleMuteSound() }
                }
            },
            {
                "close", new Dictionary<string, Action>()
                {
                    { "interface", () => { command = "close"; } },
                    { "console", () => { command = "close console"; } },
                    { "editor", () => { command = "close editor"; } }
                }
            },
            {
                "open", new Dictionary<string, Action>()
                {
                    { "interface", () => { if (Interface.inst == null) { Catowo.inst.MakeNormalWindow();
                        Catowo.inst.canvas.Children.Add(new Interface(Catowo.inst.canvas));
                        Objects.CursorEffects.MoveTop();} } },
                    { "console", () => { command = "show console"; } },
                    { "editor", () => { command = "ole"; } },
                    { "discord", () => { System.Diagnostics.Process.Start(UserData.DiscordPath); } }
                }
            },
            {
                "show", new Dictionary<string, Action>()
                {
                    { "interface", () => { if (Interface.inst == null) { Catowo.inst.MakeNormalWindow();
                        Catowo.inst.canvas.Children.Add(new Interface(Catowo.inst.canvas));
                        Objects.CursorEffects.MoveTop();} } },
                    { "console", () => { command = "show console"; } },
                    { "editor", () => { command = "ole"; } },
                    { "discord", () => 
                    { 
                        nint hwnd = Helpers.BackendHelping.FindWindowWithPartialName("Discord");
                        ShowWindowWrapper(hwnd, SW_MAXIMIZE);
                        SetForegroundWindowWrapper(hwnd);
                    }}
                }
            },
            {
                "quote", new Dictionary<string, Action>()
                {
                    { "give", () => GenerateQuote() },
                    { "generate", () => GenerateQuote() }
                }
            },
            {
                "window", new Dictionary<string, Action>()
                {
                    { "expand", () => 
                    {
                        IntPtr fwh = GetForegroundWindowWrapper();
                        ShowWindowWrapper(fwh, SW_MAXIMIZE);
                    } },
                    { "close", () => 
                    {
                        IntPtr fwh = GetForegroundWindowWrapper();
                        SendMessageWrapper(fwh, WM_CLOSE, nint.Zero, nint.Zero);
                    } },
                    { "maximize", () => 
                    {
                        IntPtr fwh = GetForegroundWindowWrapper();
                        ShowWindowWrapper(fwh, SW_MAXIMIZE);
                    } },
                    { "minimize", () => 
                    {
                        IntPtr fwh = GetForegroundWindowWrapper();
                        ShowWindowWrapper(fwh, SW_MINIMIZE);
                    } },
                    { "hide", () => 
                    {
                        IntPtr fwh = GetForegroundWindowWrapper();
                        ShowWindowWrapper(fwh, SW_MINIMIZE);
                    } },

                }
            },
            {
                "define", new Dictionary<string, Action>()
                {
                    // Yes, I could make this a command or integrate it with the existing define command
                    { "", async () => 
                    {
                        // Split the raw audio into it's component words to ignore the replacement therapy by speechrecog.json
                        var split = rawdio.Split(' ').ToList();
                        // Remove all whitespace and words less than 3 letters in length
                        split.RemoveAll(x => x.Length < 3 || string.IsNullOrWhiteSpace(x));
                        if (split.Count > 1)
                        {
                            // find where the define word was
                            int defloc = split.IndexOf("define");
                            if ((defloc + 1) < split.Count)
                            {
                                // The next word is the word to define
                                string word = split[defloc + 1];
                                Logging.Log([$"Defining word: {word}"]);
                                // Use the dict apis to get the definition as a dynamic dictionary.
                                (bool? b, Dictionary<string, dynamic>? d) = await Helpers.HTMLStuff.DefineWord(word);
                                // if it failed to get the word or the dictionary is null, it exits as an invalid attempt
                                if (b == false || d == null)
                                    return;

                                // Debugging segment for saving the raw dictionary to the clipboard for further analysis

#if SAVETOCLIPBOARD
                                string deser = Newtonsoft.Json.JsonConvert.SerializeObject(d, Formatting.Indented);
                                try
                                {
                                    System.Windows.Clipboard.SetText(deser);
                                }
                                catch (Exception e)
                                {
                                    Logging.Log(["Error in setting text to clipboard"]);
                                    Logging.LogError(e);
                                }
#endif
                                // The message to send, defined above the conditions to use after them
                                string message;
                                if (b == true)
                                {
                                    // All the fancy stuff is formatting :3
                                    StringBuilder sb = new($"<t>{d["word"]}</t>  (<q>{(d.TryGetValue("phonetic", out _) ? d["phonetic"] : "Phonetic unavailable")}</q>)\n<s>Meanings</s>\n");

                                    foreach (var meaning in d["meanings"])
                                    {
                                        string partOfSpeechFormatted = $"- <st>{char.ToUpper(meaning["partOfSpeech"].ToString()[0])}{meaning["partOfSpeech"].ToString().Substring(1)}</st>\n";
                                        sb.Append(partOfSpeechFormatted);

                                        foreach (var wdef in meaning["definitions"])
                                        {
                                            string definitionFormatted = $"<tab>{wdef["definition"]}\n";
                                            sb.Append(definitionFormatted);
                                            try
                                            {
                                                string synonymsFormatted = (wdef["synonyms"] as List<string>).Any() ? $"Synonyms: {string.Join(", ", (List<string>)wdef["synonyms"])}\n" : string.Empty;
                                                sb.Append("<tab><tab>")
                                                  .Append(synonymsFormatted);
                                            }
                                            catch (Exception e)
                                            {
                                                Logging.Log(["Failed to parse synonyms"]);
                                                Logging.LogError(e);
                                            }
                                            try
                                            {
                                                if (((List<string>)wdef["antonyms"]).Any())
                                                {
                                                    string antonymsFormatted = $"Antonyms: {string.Join(", ", (List<string>)wdef["antonyms"])}";
                                                    sb.Append($"<tab><tab>{antonymsFormatted}");
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Logging.Log(["Failed to parse antonyms"]);
                                                Logging.LogError(e);
                                            }
                                            sb.AppendLine();
                                        }
                                    }
                                    // Make it smaller, vertically, so it still fits on the screen
                                    message = sb.ToString().Replace("\n\n", "\n");
                                }
                                else
                                {
                                    if (d["data"].Count < 1 || !d["found"])
                                    {
                                        Logging.Log(["Error: Dynamic to UrAPIDef casting failed while DefineWord returned true. (Or !def.found)"]);
                                        return;
                                    }
                                    StringBuilder sb = new StringBuilder($"<t>{d["term"]}</t>   <q>No Phonetic</q>\n<s>Meanings</s>")
                                        .AppendLine(string.Join("\n", (d["data"] as List<Dictionary<string, string>>).Select(x => $"{x["meaning"]}\n<tab><i>{x["example"]}</i>")));
                                    message = sb.ToString();
                                }
                                StellaHerself.FadeDelay = 10000;
                                StellaHerself.Custom = [message,];
                                StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
                            }
                        }
                    } },
                }
            },
        };

        /// <summary>
        /// Function that processes vocal input for commands
        /// </summary>
        /// <param name="audioi"></param>
        [CAspects.ConsumeException]
        [CAspects.Logging]
        internal static void ProcessVoiceCommand(string audioi)
        {
            rawdio = audioi;
            // Local function for replacing commonly misheard words for what they probably are
            static string JSONSubbing(string audiof)
            {
                string pattern = String.Join("|", Objects.VoiceCommandHandler.Speechrecogmap.Keys);
                pattern = Regex.Escape(pattern).Replace("\\|", "|");
                return Regex.Replace(audiof, pattern, match => Objects.VoiceCommandHandler.Speechrecogmap[match.Value]);
            }

            string audio = JSONSubbing(" " + audioi.ToLower().Trim() + " ");
            Logging.Log(["Subbed Audio: " + audio]);
            if (!audio.Contains("Stella") && !VoiceCommandHandler.WasCalled && !NoCall)
                return;
            audio = audio.Replace("hey Stella", "").Replace("Stella", "");
            if (audio.Length < 5)
                VoiceCommandHandler.WasCalled = true;
            bool failure = true;

            foreach (var commandEntry in commandMap)
            {
                if (audio.Contains(commandEntry.Key))
                {
                    foreach (var subCommandEntry in commandEntry.Value)
                    {
                        if (audio.Contains(subCommandEntry.Key))
                        {
                            subCommandEntry.Value.Invoke();
                            failure = false;
                            break;
                        }
                    }
                }
                if (failure == false)
                    break;
            }

            if (command != null)
                Interface.CommandProcessing.ProcessCommand(command);
            if (mess != null)
            {
                StellaHerself.Custom = [mess,];
                StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
                mess = null;
            }
            else if (failure)
                Logging.Log(["Unrecognised command"]);
            VoiceCommandHandler.WasCalled = false;
            command = null;
            mess = null;
            commandstruct = null;
        }

        /// <summary>
        /// Just a lil quote generator :3
        /// </summary>
        [CAspects.Logging]
        private static void GenerateQuote()
        {
            List<string> quotes = Helpers.JSONManager.ExtractValueFromJsonFile<string, List<string>>("misc.json", "quotes");
            if (quotes != null && quotes.Count > 0)
            {
                StellaHerself.Custom = new[] { quotes[random.Next(quotes.Count)] };
                StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            }
        }
    }
}