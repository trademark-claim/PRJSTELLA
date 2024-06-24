namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Command to activate the voice command system
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.ConsumeException]
        internal static bool AV()
        {
            Interface.AddLog("Starting Voice Capture...");
            VoiceCommandHandler.StartListeningAndProcessingAsync();
            return true;
        }

        /// <summary>
        /// Tutorial for the Activate voice command
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TAV()
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
                            .cmdmap["activate voice"]
                        ].desc 
                    + "\"",
                    "There's nothing much to this command, just activate it and start chatting to me ^^"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            await StellaHerself.TCS.Task;
        }
    }
}