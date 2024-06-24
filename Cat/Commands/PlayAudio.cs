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
            if (commandstruct?.Parameters[0][0] is not string para1)
            {
                Logging.Log(["Expected string but parsing failed and returned either a null command struct or a null entry, please submit a bug report."]);
                Interface.AddTextLog("Execution Failed: Command struct or entry was null, check logs.", RED);
                return false;
            }
            try
            {
                if (string.IsNullOrWhiteSpace(para1) || !ValidateFile(para1))
                {
                    Logging.Log([$"Invalid or inaccessible file path: {para1}"]);
                    Interface.AddTextLog($"Invalid or inaccessible file path: {para1}", RED);
                    return false;
                }
                Logging.Log([$"Attempting to play audio file: {para1}"]);

                if (WavePlayer != null)
                {
                    Logging.Log(["An audio file is already playing. Stopping current audio."]);
                    Interface.AddLog("An audio file is already playing. Stopping current audio...");
                    StopAudio();
                }
                Logging.Log(["Creating Waveout and Audio file reader objects..."]);
                WavePlayer = new WaveOut();
                AFR = new AudioFileReader(para1);
                WavePlayer.Init(AFR);

                WavePlayer.PlaybackStopped += (s, e) =>
                {
                    if (e.Exception != null)
                    {
                        Logging.Log([$"Playback stopped due to an exception."]);
                        Logging.LogError(e.Exception);
                    }
                    else
                    {
                        Logging.Log(["Playback stopped without any exception."]);
                    }
                    StopAudio();
                };
                Logging.Log(["Objects created successfully."]);

                WavePlayer.Play();

                Logging.Log(["Audio playback started successfully."]);
                Interface.AddLog($"Playing {para1}");
            }
            catch (Exception ex)
            {
                Logging.Log([$"Error while attempting to play audio:"]);
                Logging.LogError(ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tutorial for the play audio command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TPlayAudio()
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
                    .cmdmap["play audio"]
                ].desc
            + "\"",
            "This command plays an audio file on your system (yes, there are no ads :p). It will be expanded for looping, on the spot changing, overlaying and marco-binding, but those come later.",
            "For now, lets show you how to use it!\nI'll download a song, I mean audio sample for us to try it with, please hold..."
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var continu = await StellaHerself.TCS.Task;
            if (!continu) return;
            string path = Path.Combine(ExternalDownloadsFolder, "copied_city.mp3");
            Helpers.ExternalDownloading.FromGDrive(CopiedCityMP3, path);
            await Helpers.ExternalDownloading.TCS.Task;
            StellaHerself.Custom = [
            "Alright sweet, you now have a copy of the 'Copied City' theme from NieR: Automata, a great game you should try~",
            $"It's been downloaded to {path}",
            $"Lets play it by running 'play audio ;{path}'"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            continu = await StellaHerself.TCS.Task;
            if (!continu) return;
            Interface.CommandProcessing.ProcessCommand($"play audio ;{path}");
        }
    }
}