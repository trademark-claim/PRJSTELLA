namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Stops any currently playing audio and releases associated resources.
        /// </summary>
        /// <returns>True if audio playback was stopped successfully, false if an error occurred during the process.</returns>
        /// <remarks>
        /// Checks if an audio file is currently playing and stops it, ensuring all resources are properly disposed.
        /// </remarks>
        [LoggingAspects.ConsumeException]
        internal static bool StopAudio()
        {
            Logging.Log("Stopping Audio playback...");
            try
            {
                if (WavePlayer != null)
                {
                    WavePlayer.Stop();
                    WavePlayer.Dispose();
                    WavePlayer = null;

                    AFR.Dispose();
                    AFR = null;

                    Logging.Log("Audio playback stopped.");
                    if (!SilentAudioCleanup)
                        Interface.AddLog("Audio playback stopped.");
                }
                else
                {
                    Logging.Log("No audio is currently playing.");
                    if (!SilentAudioCleanup)
                        Interface.AddLog("Yes, I too enjoy perfect silence... but you can't tell me to stop playing nothing -- existence isn't an audio file, yk?");
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"Error stopping audio playback.");
                Logging.LogError(ex);
                return false;
            }
            SilentAudioCleanup = false;
            return true;
        }
    }
}