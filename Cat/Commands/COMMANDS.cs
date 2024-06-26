//#define SAVETOCLIPBOARD

using NAudio.Wave;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
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
        internal static System.Windows.Window? Logger = null;
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
        /// Flag to send the <b><i><c>"Something went wrong when executing (command)"</c></i></b> or not.
        /// </summary>
        private static bool actualError = true;
        /// <summary>
        /// Abstraction prioperty
        /// </summary>
        internal static bool ActualError {
            get => actualError;
            // The only need to externally change it is to make it true again, hence why this'll always be set to true.
            set => actualError = true;
        }
    }
}