using Newtonsoft.Json.Linq;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows.Documents;
using System.Collections;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Voice command mapping, where keys are keywords and end-values are Actions that determine the command and output message
        /// </summary>
        /// <remarks>
        /// " " means no second keyword needed, or a default case
        /// </remarks>
        private static readonly Dictionary<string, Dictionary<string, VoiceCommand>> commandMap = new Dictionary<string, Dictionary<string, VoiceCommand>>()
        {
            {
                "screenshot", new Dictionary<string, VoiceCommand>()
                {
                    { "primary", new VoiceCommand(() => { command = "screenshot ;0"; mess = "Screenshot taken!"; }, "Takes a screenshot of your primary monitor (index 0)", "screenshot ;0") },
                    { "stitch", new VoiceCommand(() => { command = "screenshot ;-2"; mess = "Screenshot taken!"; }, "Takes a stitch screenshot", "screenshot ;-2") },
                    { " ", new VoiceCommand(() => { command = "screenshot ;-1"; mess = "Screenshot taken!"; }, "Takes a shot of every screen", "screenshot ;-1") }
                }
            },
            {
                "shutdown", new Dictionary<string, VoiceCommand>()
                {
                    { " ", new VoiceCommand(() => { command = "shutdown"; }, "Shuts STELLA down", "shutdown") }
                }
            },
            {
                "kitty", new Dictionary<string, VoiceCommand>()
                {
                    { "show", new VoiceCommand(() => { command = "cat"; mess = "Generating cat!"; }, "Generates a cat!", "cat") },
                    { "generate", new VoiceCommand(() => { command = "cat"; mess = "Generating cat!"; }, "Generates a cat!", "cat") }
                }
            },
            {
                "cat", new Dictionary<string, VoiceCommand>()
                {
                    { "show", new VoiceCommand(() => { command = "cat"; mess = "Generating cat!"; }, "Generates a cat!", "cat") },
                    { "generate", new VoiceCommand(() => { command = "cat"; mess = "Generating cat!"; }, "Generates a cat!", "cat") }
                }
            },
            {
                "song", new Dictionary<string, VoiceCommand>()
                {
                    { "skip", new VoiceCommand(() => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_NEXT_TRACK), "Presses the MEDIA NEXT button for you") },
                    { "next", new VoiceCommand(() => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_NEXT_TRACK), "Presses the MEDIA NEXT button for you") },
                    { "previous", new VoiceCommand(() => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PREV_TRACK), "Presses the MEDIA PREV button for you") },
                    { "resume", new VoiceCommand(() => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PLAY_PAUSE), "Presses the MEDIA PLAY PAUSE button for you") },
                    { "continue", new VoiceCommand(() => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PLAY_PAUSE), "Presses the MEDIA PLAY PAUSE button for you") },
                    { "pause", new VoiceCommand(() => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PLAY_PAUSE), "Presses the MEDIA PLAY PAUSE button for you") },
                    { "stop", new VoiceCommand(() => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PLAY_PAUSE), "Presses the MEDIA PLAY PAUSE button for you") }
                }
            },
            {
                "music", new Dictionary<string, VoiceCommand>()
                {
                    { "skip", new VoiceCommand(() => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_NEXT_TRACK), "Presses the MEDIA NEXT button for you") },
                    { "next", new VoiceCommand(() => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_NEXT_TRACK), "Presses the MEDIA NEXT button for you") },
                    { "previous", new VoiceCommand(() => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PREV_TRACK), "Presses the MEDIA PREV button for you") },
                    { "resume", new VoiceCommand(() => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PLAY_PAUSE), "Presses the MEDIA PLAY PAUSE button for you") },
                    { "continue", new VoiceCommand(() => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PLAY_PAUSE), "Presses the MEDIA PLAY PAUSE button for you") },
                    { "pause", new VoiceCommand(() => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PLAY_PAUSE), "Presses the MEDIA PLAY PAUSE button for you") },
                    { "stop", new VoiceCommand(() => BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PLAY_PAUSE), "Presses the MEDIA PLAY PAUSE button for you") }
                }
            },
            {
                "volume", new Dictionary<string, VoiceCommand>()
                {
                    { "low", new VoiceCommand(() => BaselineInputs.DecrVol20(), "Decreases the volume by 20") },
                    { "decrease", new VoiceCommand(() => BaselineInputs.DecrVol20(), "Decreases the volume by 20") },
                    { "raise", new VoiceCommand(() => BaselineInputs.IncrVol20(), "Increases the volume by 20") },
                    { "increase", new VoiceCommand(() => BaselineInputs.IncrVol20(), "Increases the volume by 20") }
                }
            },
            {
                "sound", new Dictionary<string, VoiceCommand>()
                {
                    { "low", new VoiceCommand(() => BaselineInputs.DecrVol20(), "Decreases the volume by 20") },
                    { "decrease", new VoiceCommand(() => BaselineInputs.DecrVol20(), "Decreases the volume by 20") },
                    { "raise", new VoiceCommand(() => BaselineInputs.IncrVol20(), "Increases the volume by 20") },
                    { "increase", new VoiceCommand(() => BaselineInputs.IncrVol20(), "Increases the volume by 20") },
                    { "mute", new VoiceCommand(() => BaselineInputs.ToggleMuteSound(), "Mutes the audio") },
                    { "silence", new VoiceCommand(() => BaselineInputs.ToggleMuteSound(), "Mutes the audio") },
                    { "unmute", new VoiceCommand(() => BaselineInputs.ToggleMuteSound(), "Mutes the audio") },
                    { "unsilence", new VoiceCommand(() => BaselineInputs.ToggleMuteSound(), "Mutes the audio") }
                }
            },
            {
                "close", new Dictionary<string, VoiceCommand>()
                {
                    { "interface", new VoiceCommand(() => { command = "close"; }, "Closes the interface", "close") },
                    { "console", new VoiceCommand(() => { command = "close console"; }, "Closes the Console", "close console") },
                    { "editor", new VoiceCommand(() => { command = "close editor"; }, "Closes the Log Editor", "close editor") }
                }
            },
            {
                "open", new Dictionary<string, VoiceCommand>()
                {
                    { "interface", new VoiceCommand(() => { if (Interface.inst == null) { Catowo.inst.MakeNormalWindow();
                            Catowo.inst.canvas.Children.Add(new Interface(Catowo.inst.canvas));
                            } }, "Opens the interface") },
                    { "console", new VoiceCommand(() => { command = "show console"; }, "Opens a console", "show console") },
                    { "editor", new VoiceCommand(() => { command = "ole"; }, "Opens a log editor", "ole") },
                }
            },
            {
                "show", new Dictionary<string, VoiceCommand>()
                {
                    { "interface", new VoiceCommand(() => { if (Interface.inst == null) { Catowo.inst.MakeNormalWindow();
                            Catowo.inst.canvas.Children.Add(new Interface(Catowo.inst.canvas));
                            } }, "Opens the interface") },
                    { "console", new VoiceCommand(() => { command = "show console"; }, "Opens a console", "show console") },
                    { "editor", new VoiceCommand(() => { command = "ole"; }, "Opens a log editor", "ole") }
                }
            },
            {
                "quote", new Dictionary<string, VoiceCommand>()
                {
                    { "give", new VoiceCommand(() => GenerateQuote(), "Generates a nice quote for you c:", "quote") },
                    { "generate", new VoiceCommand(() => GenerateQuote(), "Generates a nice quote for you c:", "quote") }
                }
            },
            {
                "window", new Dictionary<string, VoiceCommand>()
                {
                    { "expand", new VoiceCommand(() =>
                    {
                        IntPtr fwh = GetForegroundWindowWrapper();
                        ShowWindowWrapper(fwh, SW_MAXIMIZE);
                    }, "Maximizes the active window") },
                    { "close", new VoiceCommand(() =>
                    {
                        IntPtr fwh = GetForegroundWindowWrapper();
                        PInvoke.GetWindowThreadProcessIdWrapper(fwh, out uint pid);
                        Process activeprocess = Process.GetProcessById((int)pid);
                        activeprocess.Kill(true);
                    }, "Closes the active window (Kills it from the root, without sending a WM. Better than Alt+F4)") },
                    { "maximize", new VoiceCommand(() =>
                    {
                        IntPtr fwh = GetForegroundWindowWrapper();
                        ShowWindowWrapper(fwh, SW_MAXIMIZE);
                    }, "Maximizes the active window") },
                    { "minimize", new VoiceCommand(() =>
                    {
                        IntPtr fwh = GetForegroundWindowWrapper();
                        ShowWindowWrapper(fwh, SW_MINIMIZE);
                    }, "Minimizes the active window") },
                    { "hide", new VoiceCommand(() =>
                    {
                        IntPtr fwh = GetForegroundWindowWrapper();
                        ShowWindowWrapper(fwh, SW_MINIMIZE);
                    }, "Minimizes the active window") }
                }
            },
            {
                "define", new Dictionary<string, VoiceCommand>()
{
    { "", new VoiceCommand(async () =>
    {
        static List<string> JAtOL(JArray jArray)
        {
            List<string> result = new List<string>();

            foreach (var item in jArray)
            {
                result.Add(item.ToString());
            }

            return result;
        }

        var split = rawdio.Split(' ').ToList();
        split.RemoveAll(x => x.Length < 3 || string.IsNullOrWhiteSpace(x));

        if (split.Count > 1)
        {
            int defloc = split.FindIndex(x => x == "define");
            int defCount = split.Count(x => x == "define");
            if ((defloc + 1) < split.Count)
            {
                List<string> words = split.Skip(defloc + 1).Take(Math.Min(split.Count - defloc - 1, 9)).ToList();
                string[] messages = new string[words.Count + 1];
                messages[0] = "Here are the definitions for all the words I heard\nRemember to use the up arrow to cancel, and the left and right to move through them: ";
                int i = 0;

                foreach (string word in words)
                {
                    i++;
                    Logging.Log(new string[] { $"Defining word: {word}" });
                    (bool? b, Dictionary<string, dynamic>? d) = await Helpers.HTMLStuff.DefineWord(word);

                    if (b == false || d == null)
                        return;

                    #if SAVETOCLIPBOARD
                    string deser = Newtonsoft.Json.JsonConvert.SerializeObject(d, Formatting.Indented);
                    try
                    {
                        System.Windows.Clipboard.SetText(deser);
                    }
                    catch (Exception e)
                    {
                        Logging.Log(new string[] { "Error in setting text to clipboard" });
                        Logging.LogError(e);
                    }
                    #endif

                    string message;
                    if (b == true)
                    {
                        StringBuilder sb = new StringBuilder($"<t>{d["word"]}</t>  (<q>{(d.TryGetValue("phonetic", out _) ? d["phonetic"] : "Phonetic unavailable")}</q>)\n<s>Meanings</s>\n");

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
                                    if (wdef["synonyms"] is JArray j)
                                    {
                                        var ll = JAtOL(j);
                                        string synonymsFormatted = $"Synonyms: {string.Join(", ", ll)}";
                                        sb.Append($"<tab><tab>{synonymsFormatted}");
                                    }
                                    else if (wdef["synonyms"] is List<string> lists && lists.Any())
                                    {
                                        string synonymsFormatted = $"Synonyms: {string.Join(", ", lists)}";
                                        sb.Append($"<tab><tab>{synonymsFormatted}");
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logging.Log(new string[] { "Failed to parse synonyms" });
                                    Logging.LogError(e);
                                }
                                try
                                {
                                    if (wdef["antonyms"] is JArray j)
                                    {
                                        var ll = JAtOL(j);
                                        string antonymsFormatted = $"Antonyms: {string.Join(", ", ll)}";
                                        sb.Append($"<tab><tab>{antonymsFormatted}");
                                    }
                                    else if (wdef["antonyms"] is List<string> lists && lists.Any())
                                    {
                                        string antonymsFormatted = $"Antonyms: {string.Join(", ", lists)}";
                                        sb.Append($"<tab><tab>{antonymsFormatted}");
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logging.Log(new string[] { "Failed to parse antonyms" });
                                    Logging.LogError(e);
                                }
                                sb.AppendLine();
                            }
                        }
                        message = sb.ToString().Replace("\n\n", "\n");
                    }
                    else
                    {
                        if (d["data"].Count < 1 || !d["found"])
                        {
                            Logging.Log(new string[] { "Error: Dynamic to UrAPIDef casting failed while DefineWord returned true. (Or !def.found)" });
                            return;
                        }
                        StringBuilder sb = new StringBuilder($"<t>{d["term"]}</t>   <q>No Phonetic</q>\n<s>Meanings</s>")
                            .AppendLine(string.Join("\n", (d["data"] as List<Dictionary<string, string>>).Select(x => $"{x["meaning"]}\n<tab><i>{x["example"]}</i>")));
                        message = sb.ToString();
                    }
                    messages[i] = message;
                }
                StellaHerself.FadeDelay = 10000 * messages.Length;
                StellaHerself.Custom = messages;
                StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
                await StellaHerself.TCS.Task;
            }
        }
    }, "Defines a word and shows it on the bottom right", "define") }

                }
            },
            {
                "log", new Dictionary<string, VoiceCommand>()
                {
                    { "open", new VoiceCommand(() => { Process.Start(new ProcessStartInfo
                                                        {
                                                            FileName = "notepad.exe",
                                                            Arguments = LogPath,
                                                            UseShellExecute = true
                                                        });}, "Opens the currently used log file", "open logs") },
                    { "flush", new VoiceCommand(() => { command = "flush logs"; mess = "Logs Flushed!"; }, "Flushes the log queue", "flush logs") }
                }
            }
        };

        /// <summary>
        /// Schema to hold the voice command instructions
        /// </summary>
        private readonly record struct VoiceCommand(Action Action, string Description, string InnerCommand = "");


        /// <summary>
        /// Function that processes vocal input for commands
        /// </summary>
        /// <param name="audioi"></param>
        [CAspects.ConsumeException]
        [CAspects.Logging]
        //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static void ProcessVoiceCommand(string audioi)
        {
            rawdio = audioi;
            // Local function for replacing commonly misheard words for what they probably are
            static string JSONSubbing(string audiof)
            {
                string pattern = string.Join("|", Objects.VoiceCommandHandler.Speechrecogmap.Keys);
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
            audio += " ";
            foreach (var commandEntry in commandMap)
            {
                if (audio.Contains(commandEntry.Key))
                {
                    foreach (var subCommandEntry in commandEntry.Value)
                    {
                        if (audio.Contains(subCommandEntry.Key))
                        {
                            subCommandEntry.Value.Action();
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
                StellaHerself.Custom = [quotes[random.Next(quotes.Count)]];
                StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            }
        }
    }
}
