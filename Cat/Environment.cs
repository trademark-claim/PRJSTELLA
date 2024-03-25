using System.Text.RegularExpressions;

namespace Cat
{
    internal static partial class Environment
    {
        internal static readonly string RunningPath = AppDomain.CurrentDomain.BaseDirectory;
        internal const string BCURPATH = "black1.cur";
        internal static readonly Random random = new();
        internal static readonly string LogPath = "C:\\ProgramData\\Kitty\\Cat\\NYANPASU\\Logs\\L" + GUIDRegex().Replace(Guid.NewGuid().ToString(), "") + ".txt";

        internal const string LogFolder = "C:\\ProgramData\\Kitty\\Cat\\NYANPASU\\Logs\\";
        internal const string AudioFolder = "C:\\ProgramData\\Kitty\\Cat\\NYANPASU\\Audio\\";
        internal const string UserFolder = "C:\\ProgramData\\Kitty\\Cat\\NYANPASU\\User\\";
        internal const string SSFolder = "C:\\ProgramData\\Kitty\\Cat\\NYANPASU\\Screenshots\\";
        internal const string VideoFolder = "C:\\ProgramData\\Kitty\\Cat\\NYANPASU\\Videos\\";
        internal const string NotesFolder = "C:\\ProgramData\\Kitty\\Cat\\NYANPASU\\Notes\\";
        internal const string ExternalProcessesFolder = "C:\\ProgramData\\Kitty\\Cat\\Processes\\";

        internal const string FFMPEGPath = ExternalProcessesFolder + "ffmpeg.exe";


        internal static bool AssemblyInformation = false;
        internal static bool EnvironmentVariables = false;

        #region VKs and Styles

        internal const int GWL_EXSTYLE = -20;
        internal const int WS_EX_TRANSPARENT = 0x20;
        internal const int WS_EX_LAYERED = 0x80000;
        internal const int WS_EX_TOOLWINDOW = 0x80;

        internal const int WS_EX_NOACTIVATE = 0x08000000;

        internal const int WH_KEYBOARD_LL = 13;
        internal const int WM_KEYDOWN = 0x0100;
        internal const int WM_KEYUP = 0x0101;
        internal const int VK_LMENU = 0xA4;
        internal const int VK_RMENU = 0xA5;
        internal const int VK_RSHIFT = 0xA1;
        internal const int VK_LSHIFT = 0xA0;
        internal const int VK_BACK = 0x08;
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
        internal const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        internal const uint KEYEVENTF_KEYUP = 0x0002;
        internal const uint INPUT_KEYBOARD = 1;

        internal static IntPtr _keyboardHookID = IntPtr.Zero;
        internal static LowLevelKeyboardProc _keyboardProc;

        #endregion VKs and Styles

        internal static readonly Dictionary<int, (char, char)> vkCodeToCharMap = new()
        {
            // Letters
            {VK_A, ('a', 'A')},
            {VK_B, ('b', 'B')},
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

        internal const uint OCR_NORMAL = 32512;
        internal const uint SPI_SETCURSORS = 0x0057;
        internal const uint SPIF_UPDATEINIFILE = 0x01;
        internal const uint SPIF_SENDCHANGE = 0x02;

        #region Delegates

        internal delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [GeneratedRegex("[^a-zA-Z0-9]")]
        internal static partial Regex GUIDRegex();

        #endregion Delegates
    }
}