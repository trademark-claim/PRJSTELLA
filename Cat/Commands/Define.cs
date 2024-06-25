//#define SAVETOCLIPBOARD

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Even if you were eaten, there will still be a two way out.
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.ConsumeException]
        internal static async Task<bool> Define()
        {
            string message = "No meanings found...";
            var spin = new Logging.ProgressLogging.SpinnyThing("Downloading output from API(s)");
            var blockref = spin.block;
            var b =  await Task.Run((Func<bool>)(() => { 
                string para1 = (string)(commandstruct?.Parameters[0][0]);
                if (para1 == null || string.IsNullOrWhiteSpace(para1))
                {
                    Logging.Log(["Expected int but parsing failed and returned either a null command struct or a null entry, please submit a bug report."]);
                    Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                    return false;
                }
                (bool? b, Dictionary<string, dynamic>? d) = Task.Run(async () => await Helpers.HTMLStuff.DefineWord(para1)).Result;
                spin.Stop(false);
                spin = new Logging.ProgressLogging.SpinnyThing("Processing Definitions");
                if (b == false || d == null)
                {
                    Interface.AddTextLog($"No definition found for word: {para1}", HOTPINK);
                    actualError = false;
                    return false;
                }
                // Debugging stuff for getting the raw processed value in case it crashed for further analysis of the issue
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
                // If there was a successful return from the API, format it nicely and display it
                if (b == true)
                {
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
                                string synonymsFormatted = wdef["synonyms"] is List<string> slist ? slist.Any() ? $"Synonyms: {string.Join(", ", (List<string>)wdef["synonyms"])}\n" : string.Empty : string.Empty;
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
                                if (wdef["antonyms"] is List<string> alist && alist.Count != 0)
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
                    message = sb.ToString().Replace("\n\n", "\n");
                }
                else
                {
                    if (d["data"].Count < 1 || !d["found"])
                    {
                        Logging.Log(["Error: Dynamic to UrAPIDef casting failed while DefineWord returned true. (Or !def.found)"]);
                        return false;
                    }
                    if (d["data"] is List<Dictionary<string, string>> dlist && dlist is not null)
                    {
                        StringBuilder sb = new StringBuilder($"<t>{d["term"]}</t>   <q>No Phonetic</q>\n<s>Meanings</s>")
                            .AppendLine(string.Join("\n", dlist.Select(x => $"{x["meaning"]}\n<tab><i>{x["example"]}</i>")));
                        message = sb.ToString();
                    }
                }
                Interface.inst.Dispatcher.Invoke(() => Interface.logListBox.Items.Add(Helpers.BackendHelping.FormatTextBlock(message)));
                return true;    
            }));
            spin.Stop();
            Interface.logListBox.Items.Remove(blockref);
            if (message != "No meanings found...")
                Interface.AddLog("Complete!");
            return b;
        }

        /// <summary>
        /// Tutorial for the define command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TDefine()
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
                            .cmdmap["define"]
                        ].desc
                    + "\"",
                    "This command gives you the full definition of an inputted word.",
                    "If you have the Urban Dictionary option on (see 'change settings'), it'll use that if the word isnt found in a proper english dictionary.",
                    "I'll show you how to use it: here's what running 'define ;absquatulate' returns..."
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var continu = await StellaHerself.TCS.Task;
            if (!continu) return;
            Interface.CommandProcessing.ProcessCommand("define ;absquatulate");

        }
    }
}
