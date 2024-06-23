using NAudio.Wave;
using System.IO;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Attempts to play an audio file specified by the user input.
        /// </summary>
        /// <returns>True if the audio playback starts successfully, false if there is an error or the file path is invalid.</returns>
        /// <remarks>
        /// Validates the file path before attempting playback. Stops any currently playing audio before starting the new audio file.
        /// Currently doesn't work
        /// </remarks>
        [CAspects.ConsumeException]
        internal static bool PlayAudio()
        {
            string entry = commandstruct?.Parameters[0][0] as string;
            if (entry == null)
            {
                Logging.Log("Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report.");
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            try
            {
                if (string.IsNullOrWhiteSpace(entry) || !ValidateFile(entry))
                {
                    Logging.Log($"Invalid or inaccessible file path: {entry}");
                    Interface.AddTextLog($"Invalid or inaccessible file path: {entry}", RED);
                    return false;
                }
                Logging.Log($"Attempting to play audio file: {entry}");

                if (WavePlayer != null)
                {
                    Logging.Log("An audio file is already playing. Stopping current audio.");
                    Interface.AddLog("An audio file is already playing. Stopping current audio...");
                    StopAudio();
                }
                Logging.Log("Creating Waveout and Audio file reader objects...");
                WavePlayer = new WaveOut();
                AFR = new AudioFileReader(entry);
                WavePlayer.Init(AFR);

                WavePlayer.PlaybackStopped += (s, e) =>
                {
                    if (e.Exception != null)
                    {
                        Logging.Log($"Playback stopped due to an exception.");
                        Logging.LogError(e.Exception);
                    }
                    else
                    {
                        Logging.Log("Playback stopped without any exception.");
                    }
                    StopAudio();
                };
                Logging.Log("Objects created successfully.");

                WavePlayer.Play();

                Logging.Log("Audio playback started successfully.");
                Interface.AddLog($"Playing {entry}");
            }
            catch (Exception ex)
            {
                Logging.Log($"Error while attempting to play audio:");
                Logging.LogError(ex);
                return false;
            }
            return true;
        }

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TPlayAudio()
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
                    .cmdmap["play audio"]
                ]["desc"]
            + "\"",
            "This command plays an audio file on your system (yes, there are no ads :p). It will be expanded for looping, on the spot changing, overlaying and marco-binding, but those come later.",
            "For now, lets show you how to use it!\nI'll download a song, I mean audio sample for us to try it with, please hold..."
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await ClaraHerself.TCS.Task;
            if (!b) return;
            string path = Path.Combine(ExternalDownloadsFolder, "copied_city.mp3");
            Helpers.ExternalDownloading.FromGDrive(CopiedCityMP3, path);
            await Helpers.ExternalDownloading.TCS.Task;
            ClaraHerself.Custom = [
            "Alright sweet, you now have a copy of the 'Copied City' theme from NieR: Automata, a great game you should try~",
            $"It's been downloaded to {path}",
            $"Lets play it by running 'play audio ;{path}'"
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand($"play audio ;{path}");
        }
    }
}