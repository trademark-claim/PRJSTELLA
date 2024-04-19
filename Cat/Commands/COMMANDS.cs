using NAudio.Wave;
using static Cat.Structs;
using System.Text.RegularExpressions;
using System.Drawing.Text;
namespace Cat
{
    /// <summary>
    /// Holds all commands
    /// </summary>
    internal static partial class Commands
    {
        internal static Interface @interface;
        private static WaveOut? WavePlayer;
        private static AudioFileReader AFR;
        internal static Command? commandstruct;
        private static bool SilentAudioCleanup = false;
        private static System.Windows.Window? Logger = null;

        [LoggingAspects.Logging]
        internal static void ProcessVoiceCommand(string audio)
        {
            string JSONSubbing(string audiof)
            {
                string pattern = String.Join("|", Objects.VoiceCommandHandler.Speechrecogmap.Keys);
                pattern = Regex.Escape(pattern).Replace("\\|", "|");
                return Regex.Replace(audiof, pattern, match => Objects.VoiceCommandHandler.Speechrecogmap[match.Value]);
            }

            audio = JSONSubbing(audio.ToLower().Trim());
            Logging.Log("Subbed Audio: " + audio);
            if (!audio.Contains("stella") && !VoiceCommandHandler.WasCalled)
                return;
            audio = audio.Replace("hey stella", "").Replace("stella", "");
            if (audio.Length < 5)
                VoiceCommandHandler.WasCalled = true;
            string command = null;
            bool failure = true;

            if (audio.Contains("screenshot"))
            {
                if (audio.Contains("primary"))
                    command = "screenshot ;0";
                else if (audio.Contains("stitch"))
                    command = "screenshot ;-2";
                else command = "screenshot ;-1";
            }
            else if (audio.Contains("shutdown"))
                command = "shutdown";
            else if ((audio.Contains("show") || audio.Contains("generate")) && (audio.Contains("kitty") || audio.Contains("cat")))
                command = "cat";
            else if ((audio.Contains("skip") || audio.Contains("next")) && audio.Contains("song"))
            {
                failure = false;
                BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_NEXT_TRACK);
            }
            else if (audio.Contains("previous") && audio.Contains("song"))
            {
                failure = false;
                BaselineInputs.SendKeyboardInput((ushort)VK_MEDIA_PREV_TRACK);
            }
            else if ((audio.Contains("low") || audio.Contains("decrease")) && (audio.Contains("volume") || audio.Contains("sound")))
            {
                failure = false;
                BaselineInputs.DecrVol20();
            }
            else if ((audio.Contains("raise") || audio.Contains("increase")) && (audio.Contains("volume") || audio.Contains("sound")))
            {
                failure = false;
                BaselineInputs.IncrVol20();
            }

            if (command != null)
                Interface.CommandProcessing.ProcessCommand(command);
            else if (failure)
                Logging.Log("Unrecognised command");
            VoiceCommandHandler.WasCalled = false;
        }

    }
}