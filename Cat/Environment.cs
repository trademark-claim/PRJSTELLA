﻿//#define CursorChanges

// -----------------------------------------------------------------------
// Environment.cs
// Contains definitions for environment settings, paths, and utility methods
// for STELLA's runtime environment.
// Author: Nexus
// -----------------------------------------------------------------------

#pragma warning disable CS4014

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Cat
{
    /// <summary>
    /// Provides environment settings, utility methods, and constants for application configuration and runtime operations.
    /// </summary>
    internal static partial class Environment
    {
        /// <summary>Path to STELLA's running directory.</summary>
        internal static readonly string RunningPath = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>Path to the custom black cursor file.</summary>
        internal const string BCURPATH = "black1.cur";

        /// <summary>Random number generator for various operations.</summary>
        internal static readonly Random random = new();

        /// <summary>Path for the current log file, generated dynamically.</summary>
        internal static readonly string LogPath = "C:\\ProgramData\\Kitty\\Cat\\NYANPASU\\Logs\\L" + GUIDRegex().Replace(Guid.NewGuid().ToString(), "") + ".LOG";

        // Paths to various folders used by STELLA
        internal static string Downloads { get => System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + @"\Downloads"; }
        internal const string LogFolder = "C:\\ProgramData\\Kitty\\Cat\\NYANPASU\\Logs\\";
        internal const string AudioFolder = "C:\\ProgramData\\Kitty\\Cat\\NYANPASU\\Audio\\";
        internal const string UserFolder = "C:\\ProgramData\\Kitty\\Cat\\NYANPASU\\User\\";
        internal const string SSFolder = "C:\\ProgramData\\Kitty\\Cat\\NYANPASU\\Screenshots\\";
        internal const string VideoFolder = "C:\\ProgramData\\Kitty\\Cat\\NYANPASU\\Videos\\";
        internal const string NotesFolder = "C:\\ProgramData\\Kitty\\Cat\\NYANPASU\\Notes\\";
        internal const string ExternalDownloadsFolder = "C:\\ProgramData\\Kitty\\Cat\\Downloads\\";
        internal const string EPTempF = "C:\\ProgramData\\Kitty\\Cat\\Processes\\temp";
        internal const string CursorsFilePath = "C:\\ProgramData\\Kitty\\Cat\\NYANPASU\\Cursors\\";
        internal const string UserDataFile = UserFolder + "hello_mr_edit_my_raw_data.ini";
        internal const string FFMPEGPath = ExternalDownloadsFolder + "ffmpeg.exe";
        [Obsolete]
        internal const string FlatCPath = ExternalDownloadsFolder + "flatc.exe";
        internal const string StatsFile = NotesFolder + "stats.bin";
        internal const string SchemaFile = NotesFolder + "schemas.bin";
        internal const long DCID = 1231580969698459688;

        internal const string SingleCat = "1WrlyOud00t0mk53hqMcSlNar1zQ448oK";
        internal const string GatoZip = "1aGAVZIhM4MdDz7VHs5zzH_0r4GlKzxCN";
        internal const string HZDZip = "11gm9hGWWusPa4NdPLgeHf8QXRpeDS-u3";
        internal const string HeartsZip = "1Qit60S3uZ3ABpyi33LiQ7QRT7ANMT2Pj";
        internal const string CopiedCityMP3 = "1ScB_zySR4VuRBPT7ExXaXi_7kpwGiZd9";

        #region VKs and Styles

        internal const int GWL_EXSTYLE = -20;
        internal const int WS_EX_TRANSPARENT = 0x20;
        internal const int WS_EX_LAYERED = 0x80000;
        internal const int WS_EX_TOOLWINDOW = 0x80;
        internal const int WS_EX_TOPMOST = 0x8;
        internal const int WS_EX_NOACTIVATE = 0x08000000;

        internal const int WH_KEYBOARD_LL = 13;
        internal const int WM_KEYDOWN = 0x0100;
        internal const int WM_KEYUP = 0x0101;

        internal const int WH_MOUSE_LL = 14;

        internal enum MouseMessages
        {
            WM_MOUSEMOVE = 0x0200,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
        }

        internal const int VK_LMENU = 0xA4;
        internal const int VK_RMENU = 0xA5;
        internal const int VK_RSHIFT = 0xA1;
        internal const int VK_LSHIFT = 0xA0;
        internal const int VK_BACK = 0x08;
        internal const int VK_ESC = 0x1B;
        internal const int VK_RETURN = 0x0D;
        internal const int VK_LEFT = 0x25;
        internal const int VK_RIGHT = 0x27;
        internal const int VK_UP = 0x26;
        internal const int VK_A = 0x41; // A key
        internal const int VK_B = 0x42; // B key
        internal const int VK_C = 0x43; // C key
        internal const int VK_D = 0x44; // D key
        internal const int VK_E = 0x45; // E key
        internal const int VK_F = 0x46; // F key
        internal const int VK_G = 0x47; // G key
        internal const int VK_H = 0x48; // H key
        internal const int VK_I = 0x49; // I key
        internal const int VK_J = 0x4A; // J key
        internal const int VK_K = 0x4B; // K key
        internal const int VK_L = 0x4C; // L key
        internal const int VK_M = 0x4D; // M key
        internal const int VK_N = 0x4E; // N key
        internal const int VK_O = 0x4F; // O key
        internal const int VK_P = 0x50; // P key
        internal const int VK_Q = 0x51; // Q key
        internal const int VK_R = 0x52; // R key
        internal const int VK_S = 0x53; // S key
        internal const int VK_T = 0x54; // T key
        internal const int VK_U = 0x55; // U key
        internal const int VK_V = 0x56; // V key
        internal const int VK_W = 0x57; // W key
        internal const int VK_X = 0x58; // X
        internal const int VK_Y = 0x59; // Y key
        internal const int VK_Z = 0x5A; // Z key
        internal const int VK_0 = 0x30; // 0 key
        internal const int VK_1 = 0x31; // 1 key
        internal const int VK_2 = 0x32; // 2 key
        internal const int VK_3 = 0x33; // 3 key
        internal const int VK_4 = 0x34; // 4 key
        internal const int VK_5 = 0x35; // 5 key
        internal const int VK_6 = 0x36; // 6 key
        internal const int VK_7 = 0x37; // 7 key
        internal const int VK_8 = 0x38; // 8 key
        internal const int VK_9 = 0x39; // 9 key
        internal const int VK_OEM_MINUS = 0xBD;  // - _
        internal const int VK_OEM_PLUS = 0xBB;   // = +
        internal const int VK_OEM_4 = 0xDB;      // [ {
        internal const int VK_OEM_6 = 0xDD;      // ] }
        internal const int VK_OEM_5 = 0xDC;      // \ |
        internal const int VK_OEM_1 = 0xBA;      // ; :
        internal const int VK_OEM_7 = 0xDE;      // ' "
        internal const int VK_OEM_COMMA = 0xBC;  // , <
        internal const int VK_OEM_PERIOD = 0xBE; // . >
        internal const int VK_OEM_2 = 0xBF;      // / ?
        internal const int VK_OEM_3 = 0xC0;      // ` ~

        internal const ushort VK_VOLUME_DOWN = 0xAE;
        internal const ushort VK_VOLUME_MUTE = 0xAD;
        internal const uint VK_MEDIA_NEXT_TRACK = 0xB0;
        internal const uint VK_MEDIA_PREV_TRACK = 0xB1;
        internal const uint VK_MEDIA_PLAY_PAUSE = 0xB3;

        internal const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        internal const uint KEYEVENTF_KEYUP = 0x0002;
        internal const uint INPUT_KEYBOARD = 1;

        internal const int VK_LWIN = 0x5B; // Left Windows key
        internal const int VK_RWIN = 0x5C; // Right Windows key
        internal const int VK_SPACE = 0x20;

        internal static LowLevelProc _keyboardProc;

        #endregion VKs and Styles

        /// <summary>Maps virtual key codes to character pairs (lowercase, uppercase).</summary>
        internal static readonly Dictionary<int, (char, char)> vkCodeToCharMap = new()
        {
            // Letters
            { VK_A, ('a', 'A')},
            { VK_B, ('b', 'B')},
            { VK_C, ('c', 'C')},
            { VK_D, ('d', 'D')},
            { VK_E, ('e', 'E')},
            { VK_F, ('f', 'F')},
            { VK_G, ('g', 'G')},
            { VK_H, ('h', 'H')},
            { VK_I, ('i', 'I')},
            { VK_J, ('j', 'J')},
            { VK_K, ('k', 'K')},
            { VK_L, ('l', 'L')},
            { VK_M, ('m', 'M')},
            { VK_N, ('n', 'N')},
            { VK_O, ('o', 'O')},
            { VK_P, ('p', 'P')},
            { VK_Q, ('q', 'Q')},
            { VK_R, ('r', 'R')},
            { VK_S, ('s', 'S')},
            { VK_T, ('t', 'T')},
            { VK_U, ('u', 'U')},
            { VK_V, ('v', 'V')},
            { VK_W, ('w', 'W')},
            { VK_X, ('x', 'X')},
            { VK_Y, ('y', 'Y')},
            { VK_Z, ('z', 'Z')},

            // Numbers
            { VK_0, ('0', ')')},
            { VK_1, ('1', '!')},
            { VK_2, ('2', '@')},
            { VK_3, ('3', '#')},
            { VK_4, ('4', '$')},
            { VK_5, ('5', '%')},
            { VK_6, ('6', '^')},
            { VK_7, ('7', '&')},
            { VK_8, ('8', '*')},
            { VK_9, ('9', '(')},

            // Symbols
            { VK_OEM_MINUS, ('-', '_')},
            { VK_OEM_PLUS, ('=', '+')},
            { VK_OEM_4, ('[', '{')},
            { VK_OEM_6, (']', '}')},
            { VK_OEM_5, ('\\', '|')},
            { VK_OEM_1, (';', ':')},
            { VK_OEM_7, ('\'', '"')},
            { VK_OEM_COMMA, (',', '<')},
            { VK_OEM_PERIOD, ('.', '>')},
            { VK_OEM_2, ('/', '?')},
            { VK_OEM_3, ('`', '~')}

        };

        /// <summary>
        /// Maps characters to the vks needed to input to generate them
        /// </summary>
        private static readonly Dictionary<char, int[]> vkMap = new Dictionary<char, int[]>
        {
            {'A', new int[] {VK_A}}, {'B', new int[] {VK_B}}, {'C', new int[] {VK_C}},
            {'D', new int[] {VK_D}}, {'E', new int[] {VK_E}}, {'F', new int[] {VK_F}},
            {'G', new int[] {VK_G}}, {'H', new int[] {VK_H}}, {'I', new int[] {VK_I}},
            {'J', new int[] {VK_J}}, {'K', new int[] {VK_K}}, {'L', new int[] {VK_L}},
            {'M', new int[] {VK_M}}, {'N', new int[] {VK_N}}, {'O', new int[] {VK_O}},
            {'P', new int[] {VK_P}}, {'Q', new int[] {VK_Q}}, {'R', new int[] {VK_R}},
            {'S', new int[] {VK_S}}, {'T', new int[] {VK_T}}, {'U', new int[] {VK_U}},
            {'V', new int[] {VK_V}}, {'W', new int[] {VK_W}}, {'X', new int[] {VK_X}},
            {'Y', new int[] {VK_Y}}, {'Z', new int[] {VK_Z}},
            {'0', new int[] {VK_0}}, {'1', new int[] {VK_1}}, {'2', new int[] {VK_2}}, {'3', new int[] {VK_3}},
            {'4', new int[] {VK_4}}, {'5', new int[] {VK_5}}, {'6', new int[] {VK_6}}, {'7', new int[] {VK_7}},
            {'8', new int[] {VK_8}}, {'9', new int[] {VK_9}},
            {' ', new int[] {VK_SPACE}}, {',', new int[] {VK_OEM_COMMA}}, {'.', new int[] {VK_OEM_PERIOD}},
            {';', new int[] {VK_OEM_1}}, {':', new int[] {VK_LSHIFT, VK_OEM_1}}, {'?', new int[] {VK_LSHIFT, VK_OEM_2}},
            {'!', new int[] {VK_LSHIFT, VK_1}}, {'@', new int[] {VK_LSHIFT, VK_2}}, {'#', new int[] {VK_LSHIFT, VK_3}},
            {'$', new int[] {VK_LSHIFT, VK_4}}, {'%', new int[] {VK_LSHIFT, VK_5}}, {'^', new int[] {VK_LSHIFT, VK_6}},
            {'&', new int[] {VK_LSHIFT, VK_7}}, {'*', new int[] {VK_LSHIFT, VK_8}}, {'(', new int[] {VK_LSHIFT, VK_9}},
            {')', new int[] {VK_LSHIFT, VK_0}}, {'-', new int[] {VK_OEM_MINUS}}, {'_', new int[] {VK_LSHIFT, VK_OEM_MINUS}},
            {'=', new int[] {VK_OEM_PLUS}}, {'+', new int[] {VK_LSHIFT, VK_OEM_PLUS}}, {'[', new int[] {VK_OEM_4}},
            {']', new int[] {VK_OEM_6}}, {'{', new int[] {VK_LSHIFT, VK_OEM_4}}, {'}', new int[] {VK_LSHIFT, VK_OEM_6}},
            {'\\', new int[] {VK_OEM_5}}, {'|', new int[] {VK_LSHIFT, VK_OEM_5}}, {'/', new int[] {VK_OEM_2}},
            {'\'', new int[] {VK_OEM_7}}, {'"', new int[] {VK_LSHIFT, VK_OEM_7}}, {'`', new int[] {VK_OEM_3}},
            {'~', new int[] {VK_LSHIFT, VK_OEM_3}}
        };


        /// <summary>
        /// Converts a string to a sequence of VKs
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.ConsumeException]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static List<ushort> StringToVKs(string input)
        {
            List<ushort> vkArray = new List<ushort>();

            foreach (char c in input.ToUpper())
            {
                if (vkMap.TryGetValue(c, out int[] vk))
                {
                    vkArray.AddRange(vk.Select(k => (ushort)k));
                }
                else
                {
                    vkArray.Add((ushort)VK_OEM_PERIOD);
                }
            }

            return vkArray;
        }


        /// <summary>Maps modifier key virtual codes to their string representations.</summary>
        internal static readonly Dictionary<int, string> ModifierVkCodetoStringMap = new()
        {
            { VK_RSHIFT, "Right Shift" },
            { VK_LSHIFT, "Left Shift" },
            { VK_RMENU, "Right Menu" },
            { VK_LMENU, "Left Menu" },
            { VK_BACK, "Back" },
            { VK_ESC, "Escape" }
        };

        // Cursors
        /// <summary>
        /// Standard arrow and small hourglass
        /// </summary>
        internal const uint OCR_APPSTARTING = 32650;

        /// <summary>
        /// Standard arrow
        /// </summary>
        internal const uint OCR_NORMAL = 32512;

        /// <summary>
        /// Crosshair
        /// </summary>
        internal const uint OCR_CROSS = 32515;

        /// <summary>
        /// Hand
        /// </summary>
        internal const uint OCR_HAND = 32649;

        /// <summary>
        /// Arrow and question mark
        /// </summary>
        internal const uint OCR_HELP = 32651;

        /// <summary>
        /// I-beam
        /// </summary>
        internal const uint OCR_IBEAM = 32513;

        /// <summary>
        /// Slashed circle
        /// </summary>
        internal const uint OCR_UNAVAILABLE = 32648;

        /// <summary>
        /// Four-pointed arrow pointing north, south, east, and west
        /// </summary>
        internal const uint OCR_SIZEALL = 32646;

        /// <summary>
        /// Double-pointed arrow pointing northeast and southwest
        /// </summary>
        internal const uint OCR_SIZENESW = 32643;

        /// <summary>
        /// Double-pointed arrow pointing north and south
        /// </summary>
        internal const uint OCR_SIZENS = 32645;

        /// <summary>
        /// Double-pointed arrow pointing northwest and southeast
        /// </summary>
        internal const uint OCR_SIZENWSE = 32642;

        /// <summary>
        /// Double-pointed arrow pointing west and east
        /// </summary>
        internal const uint OCR_SIZEWE = 32644;

        /// <summary>
        /// Arrow pointing up
        /// </summary>
        internal const uint OCR_UP = 32516;

        /// <summary>
        /// Hourglass
        /// </summary>
        internal const uint OCR_WAIT = 32514;

        /// <summary>
        /// PINVOKE Flag for cursor changing.
        /// </summary>
        internal const uint SPI_SETCURSORS = 0x0057;
        internal const int SW_MAXIMIZE = 3;
        internal const int SW_MINIMIZE = 6;
        internal const uint WM_CLOSE = 0x0010;
        internal const uint SPIF_UPDATEINIFILE = 0x01;
        internal const uint SPIF_SENDCHANGE = 0x02;

        #region Delegates

        internal delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

        internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [GeneratedRegex("[^a-zA-Z0-9]")]
        internal static partial Regex GUIDRegex();

        #endregion Delegates

        /// <summary>Contains user-configurable settings for application behavior.</summary>
        internal static class UserData
        {
            private static string userName = System.Environment.UserName;

            /// <summary>Enables or disables aspect logging.</summary>
            internal static bool AspectLogging = true;

            /// <summary>Enables or disables logging of details.</summary>
            internal static bool LoggingDetails = false;

            /// <summary>Enables or disables full logging.</summary>
            internal static bool FullLogging = true;

            /// <summary>
            /// Provides even more logging information, now including Caller name, path and location.
            /// </summary>
            internal static bool ExtendedLogging = true;

            /// <summary>Enables or disables assembly information logging.</summary>
            internal static bool AssemblyInformation = false;

            /// <summary>Enables or disables environment variables logging.</summary>
            internal static bool EnvironmentVariables = false;

            /// <summary>Enables or disables timing of all methods.</summary>
            internal static bool TimeAll = false;

            /// <summary>Indicates whether STELLA should start automatically.</summary>
            internal static bool Startup = true;

            /// <summary>
            /// If true, the program will start with voice commands activated
            /// </summary>
            internal static bool StartWithVoice = false;

            /// <summary>
            /// If true, the program will start with STELLA's interface opened
            /// </summary>
            internal static bool StartWithInterface = false;

            /// <summary>
            /// If true, the program will start with voice commands activated
            /// </summary>
            internal static bool StartWithConsole = false;

            /// <summary>
            /// Asks the user if they're fine with the program editing the registry.
            /// </summary>
            internal static bool AllowRegistryEdits = false;

            /// <summary>
            /// If true, immedietely checks if the app has admin perms, and if not asks for them.
            /// </summary>
            internal static bool LaunchAsAdmin = false;

            /// <summary>
            /// Allows Stella to query the unofficial urban dictionary api for word definitions
            /// </summary>
            internal static bool AllowUrbanDictionaryDefinitionsWhenWordNotFound = false;

            /// <summary>
            /// See <c><see cref="Commands.NoCall"/></c>
            /// </summary>
            internal static bool RequireNameCallForVoiceCommands = true;

            /// <summary>Default screen brightness setting.</summary>
            internal static float Brightness = 0.7f;

            private static float _opacity = 0.7f;

            /// <summary>Application window opacity.</summary>
            internal static float Opacity
            { get => _opacity; set { _opacity = value; Catowo.Interface.logListBox.UpdateOpacity(); } }

            /// <summary>Font size for something (idk)</summary>
            private static float _fontsize = 10;

            /// <summary>Font size for display elements.</summary>
            internal static float FontSize
            { get => _fontsize; set { _fontsize = value; Catowo.Interface.logListBox.UpdateFontSize(); foreach (Objects.ProcessManager pm in Commands.PMs) pm.InvalidateVisual(); } }

            [Obsolete("Not in use")]
            private static string DiscordExePath = @"C:\Users\<CURRENTUSER>\AppData\Local\Discord\app-1.0.9043\Discord.exe";

            [Obsolete("Not in use")]
            internal static string DiscordPath { get => DiscordExePath.Replace("<CURRENTUSER>", userName); }

            [Obsolete("Not in use")]
            internal static readonly double[] CommandKeys = [];

#if CursorChanges // TODO
            private static bool rainbowEnabled = false;
            private static byte red = 0;
            private static byte blue = 0;
            private static byte green = 0;
            private static byte alpha = 0;
            private static bool useSquare = true;

            internal static byte Red { 
                get => red; 
                set 
                {
                    red = value;
                    rainbowEnabled = false;
                }
            }

            internal static byte Blue
            {
                get => blue;
                set
                {
                    blue = value;
                    rainbowEnabled = false;
                }
            }

            internal static byte Green
            {
                get => green;
                set
                {
                    green = value;
                    rainbowEnabled = false;
                }
            }

            internal static byte Alpha
            {
                get => alpha;
                set
                {
                    alpha = value;
                }
            }

            internal static bool RainbowEnabled
            {
                get => rainbowEnabled;
                set
                {
                    if (true)
                        red = blue = green = 1;
                    rainbowEnabled = value;
                }
            }

            internal static bool UseSquare
            {
                get => useSquare;
                set
                {
                    CursorEffects.UsingASquare = value;
                    useSquare = value;
                }
            }
#endif
            /// <summary>Updates a user setting value based on a key-value pair.</summary>
            /// <param name="key">The setting key to update.</param>
            /// <param name="value">The new value for the setting.</param>
            internal static void UpdateValue(string key, string value)
            {
                switch (key)
                {
#if CursorChanges
                    case nameof(RainbowEnabled):
                        RainbowEnabled = bool.Parse(value); 
                        break;
                    case nameof(Red):
                        Red = byte.Parse(value); 
                        break;
                    case nameof(Green):
                        Green = byte.Parse(value);
                        break;
                    case nameof(Alpha):
                        Alpha = byte.Parse(value);
                        break;
                    case nameof(Blue):
                        Blue = byte.Parse(value);
                        break;
                    case nameof(UseSquare):
                        UseSquare = bool.Parse(value);
                        break;
#endif
                    case nameof(AspectLogging):
                        AspectLogging = bool.Parse(value);
                        break;

                    case nameof(FullLogging):
                        FullLogging = bool.Parse(value);
                        break;

                    case nameof(AssemblyInformation):
                        AssemblyInformation = bool.Parse(value);
                        break;

                    case nameof(EnvironmentVariables):
                        EnvironmentVariables = bool.Parse(value);
                        break;

                    case nameof(Startup):
                        Startup = bool.Parse(value);
                        break;

                    case nameof(Brightness):
                        Brightness = float.Parse(value);
                        break;

                    case nameof(Opacity):
                        Opacity = float.Parse(value);
                        break;

                    case nameof(FontSize):
                        FontSize = float.Parse(value);
                        break;

                    case nameof(LoggingDetails):
                        LoggingDetails = bool.Parse(value);
                        break;

                    case nameof(TimeAll):
                        TimeAll = bool.Parse(value);
                        break;

                    case nameof(AllowRegistryEdits):
                        AllowRegistryEdits = bool.Parse(value);
                        break;

                    case nameof(LaunchAsAdmin):
                        LaunchAsAdmin = bool.Parse(value);
                        break;

                    case nameof(StartWithConsole):
                        StartWithConsole = bool.Parse(value);
                        break;

                    case nameof(StartWithInterface):
                        StartWithInterface = bool.Parse(value);
                        break;

                    case nameof(StartWithVoice):
                        StartWithVoice = bool.Parse(value);
                        break;
                    case nameof(AllowUrbanDictionaryDefinitionsWhenWordNotFound):
                        AllowUrbanDictionaryDefinitionsWhenWordNotFound = bool.Parse(value);
                        break;

                    case nameof(ExtendedLogging):
                        ExtendedLogging = bool.Parse(value);
                        break;

                    case nameof(RequireNameCallForVoiceCommands):
                        RequireNameCallForVoiceCommands = bool.Parse(value);
                        break;

                    default:
                        Logging.Log([$"Unknown key in INI file: {key}"]);
                        break;
                }
            }
        }
    }
}