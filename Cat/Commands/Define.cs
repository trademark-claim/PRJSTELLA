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
        internal static bool Define()
        {
            string entry = (string)(commandstruct?.Parameters[0][0]);
            if (entry == null || string.IsNullOrWhiteSpace(entry))
            {
                Logging.Log("Expected int but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            (bool? b, Dictionary<string, dynamic>? d) = Task.Run(async () => await Helpers.HTMLStuff.DefineWord(entry)).Result;
            if (b == false)
            {
                return false;
            }
            if (d == null)
            {
                return false;
            }
#if SAVETOCLIPBOARD
                                string deser = Newtonsoft.Json.JsonConvert.SerializeObject(d, Formatting.Indented);
                                try
                                {
                                    System.Windows.Clipboard.SetText(deser);
                                }
                                catch (Exception e)
                                {
                                    Logging.Log("Error in setting text to clipboard");
                                    Logging.LogError(e);
                                }
#endif
            string message;
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
                            string synonymsFormatted = (wdef["synonyms"] as List<string>).Any() ? $"Synonyms: {string.Join(", ", (List<string>)wdef["synonyms"])}\n" : string.Empty;
                            sb.Append("<tab><tab>")
                              .Append(synonymsFormatted);
                        }
                        catch (Exception e)
                        {
                            Logging.Log("Failed to parse synonyms");
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
                            Logging.Log("Failed to parse antonyms");
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
                    Logging.Log("Error: Dynamic to UrAPIDef casting failed while DefineWord returned true. (Or !def.found)");
                    return false;
                }
                StringBuilder sb = new StringBuilder($"<t>{d["term"]}</t>   <q>No Phonetic</q>\n<s>Meanings</s>")
                    .AppendLine(string.Join("\n", (d["data"] as List<Dictionary<string, string>>).Select(x => $"{x["meaning"]}\n<tab><i>{x["example"]}</i>")));
                message = sb.ToString();
            }
            Interface.AddLog(message);
            return true;
        }

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TDefine()
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
                            .cmdmap["define"]
                        ]["desc"]
                    + "\"",
                    "This command gives you the full definition of an inputted word.",
                    "If you have the Urban Dictionary option on (see 'change settings'), it'll use that if the word isnt found in a proper english dictionary.",
                    "I'll show you how to use it: here's what running 'define ;absquatulate' returns..."
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("define ;absquatulate");

        }
    }
}
