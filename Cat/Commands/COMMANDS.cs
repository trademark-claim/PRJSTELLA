using NAudio.Wave;
using System.Text.RegularExpressions;

namespace Cat
{
    /// <summary>
    /// Holds all commands
    /// </summary>
    internal static partial class Commands
    {
        internal static Interface @interface;
        private static Objects.LogEditor editor;
        private static WaveOut? WavePlayer;
        private static AudioFileReader AFR;
        internal static Command? commandstruct;
        private static bool SilentAudioCleanup = false;
        private static Objects.ScreenRecorder ScreenRecorder;
        private static System.Windows.Window? Logger = null;

        private const bool NoCall = true;

        private static string command;

        private static readonly Dictionary<string, Dictionary<string, Action>> commandMap = new Dictionary<string, Dictionary<string, Action>>()
        {
            {
                "screenshot", new Dictionary<string, Action>()
                {
                    { "primary", () => command = "screenshot ;0" },
                    { "stitch", () => command = "screenshot ;-2" },
                    { " ", () => command = "screenshot ;-1" }
                }
            },
            {
                "shutdown", new Dictionary<string, Action>()
                {
                    { " ", () => command = "shutdown" }
                }
            },
            {
                "kitty", new Dictionary<string, Action>()
                {
                    { "show", () => command = "cat" },
                    { "generate", () => command = "cat" }
                }
            },
            {
                "cat", new Dictionary<string, Action>()
                {
                    { "show", () => command = "cat" },
                    { "generate", () => command = "cat" }
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
                    { "interface", () => command = "close" },
                    { "console", () => command = "close console" },
                    { "editor", () => command = "close editor" }
                }
            },
            {
                "open", new Dictionary<string, Action>()
                {
                    { "interface", () => Catowo.inst.ToggleInterface() },
                    { "console", () => command = "show console" },
                    { "editor", () => command = "ole" }
                }
            },
            {
                "quote", new Dictionary<string, Action>()
                {
                    { "give", () => GenerateQuote() },
                    { "generate", () => GenerateQuote() }
                }
            }
        };

        [LoggingAspects.Logging]
        internal static void ProcessVoiceCommand(string audioi)
        {
            string JSONSubbing(string audiof)
            {
                string pattern = String.Join("|", Objects.VoiceCommandHandler.Speechrecogmap.Keys);
                pattern = Regex.Escape(pattern).Replace("\\|", "|");
                return Regex.Replace(audiof, pattern, match => Objects.VoiceCommandHandler.Speechrecogmap[match.Value]);
            }

            string audio = JSONSubbing(" " + audioi.ToLower().Trim() + " ");
            Logging.Log("Subbed Audio: " + audio);
            if (!audio.Contains("stella") && !VoiceCommandHandler.WasCalled && !NoCall)
                return;
            audio = audio.Replace("hey stella", "").Replace("stella", "");
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
            else if (failure)
                Logging.Log("Unrecognised command");
            VoiceCommandHandler.WasCalled = false;
        }

        [LoggingAspects.Logging]
        private static void GenerateQuote()
        {
            List<string> quotes = Helpers.JSONManager.ExtractValueFromJsonFile<string, List<string>>("misc.json", "quotes");
            if (quotes != null && quotes.Count > 0)
            {
                ClaraHerself.Custom = new[] { quotes[random.Next(quotes.Count)] };
                ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            }
        }
    }
}