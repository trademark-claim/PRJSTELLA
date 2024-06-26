using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Cat
{
    /*
     ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 
      
            All of this is unmanaged Interop with (usually) base windows functions (written in C/C++). Refer to official documentation and pinvoke.net for details.
      
     ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 
     */

    /// <summary>
    /// Holds the wrappers and interop methods (the latter hidden)
    /// </summary>
    internal static partial class PInvoke
    {
        /// <summary>
        /// Gets the position of the cursor in system coordinates
        /// </summary>
        /// <param name="lpPoint"></param>
        /// <returns></returns>
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetCursorPos(out POINT lpPoint);

        /// <summary>
        /// Sets the cursor to a system coordinate location
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetCursorPos(int X, int Y);

        /// <summary>
        /// Sends raw input to the functional layer -- being anything from keyboard input to mouse and audio
        /// </summary>
        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        /// <summary>
        /// Gets the current window style (combination of flags)
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Sets the window style (Transparency, Pass through, top most, etc)
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// Hooks in a method to be triggered by function hooks (so like key input)
        /// </summary>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

        /// <summary>
        /// Removes a function hook given the hook holder key
        /// </summary>
        [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool UnhookWindowsHookEx(IntPtr hhk);

        /// <summary>
        /// Tells the hook to yield hold of the sequence to the next hook
        /// </summary>
        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        private static partial IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Gets a module handle (pointer) given a name
        /// </summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// Gets the state of a key (given virtual key code)
        /// </summary>
        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial short GetAsyncKeyState(int vKey);

        /// <summary>
        /// Increments the cursor display count by 1
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern int ShowCursor([MarshalAs(UnmanagedType.Bool)] bool bShow);

        /// <summary>
        /// Loads the handle for a cursor visual loaded given file
        /// </summary>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursorFromFile(string path);

        /// <summary>
        /// Sets a cursor to a prepared cursor (given handle) using ID
        /// </summary>
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetSystemCursor(IntPtr hcur, uint id);

        /// <summary>
        /// Changes low level settings files, mostly used here for cursors
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        /// <summary>
        /// Returns the desktop window
        /// </summary>
        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial IntPtr GetDesktopWindow();

        /// <summary>
        /// Gets the Device Content object for a window (given handle), used for screen capture
        /// </summary>
        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial IntPtr GetWindowDC(IntPtr hWnd);

        /// <summary>
        /// Copies pixels one by one from a Window Device context to a Bitmap device content, given location, width, hieght and operation mode
        /// </summary>
        [LibraryImport("gdi32.dll", SetLastError = true)]
        private static partial uint BitBlt(IntPtr hDestDC, int xDest, int yDest, int wDest, int hDest, IntPtr hSrcDC, int xSrc, int ySrc, CopyPixelOperation rop);

        /// <summary>
        /// Releases the device Context objects for later use
        /// </summary>
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        /// <summary>
        /// Creates a bitmap from the .NET object to an Interop object
        /// </summary>
        [LibraryImport("gdi32.dll", SetLastError = true)]
        private static partial IntPtr CreateCompatibleBitmap(IntPtr hDC, int width, int height);

        /// <summary>
        /// Creates a device context from interop to .NET
        /// </summary>
        [LibraryImport("gdi32.dll", SetLastError = true)]
        private static partial IntPtr CreateCompatibleDC(IntPtr hDC);

        /// <summary>
        /// Returns the handle of a requested object
        /// </summary>
        [LibraryImport("gdi32.dll", SetLastError = true)]
        private static partial IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        /// <summary>
        /// Deletes an object given handle
        /// </summary>
        [LibraryImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// Deletes a device context
        /// </summary>
        [LibraryImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DeleteDC(IntPtr hDC);

        /// <summary>
        /// Gets the handle of the currently focused window
        /// </summary>
        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial IntPtr GetForegroundWindow();

        /// <summary>
        /// Shows a window given handler
        /// </summary>
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Sends a WM to a given window, allows for parameter input
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Why are black guys eyes red after sex? From the pepper spray.
        /// </summary>
        /// <param name="enumProc"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError =true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);


        #region Internal Methods Wrapping P/Invoke

        internal static uint GetWindowThreadProcessIdWrapper(IntPtr hWnd, out uint processId)
        {
            Logging.Log([$">PINVOKE< Getting Process ID given handle: {hWnd}"]);
            uint result = GetWindowThreadProcessId(hWnd, out processId);
            Logging.Log([$">PINVOKE< Thread ID: {result}, Process ID: {processId}"]);
            LogMarshalError();
            return result;
        }

        internal static bool SetForegroundWindowWrapper(IntPtr hWnd)
        {
            Logging.Log([">PINVOKE< Setting foreground window..."]);
            bool result = SetForegroundWindow(hWnd);
            Logging.Log([$">PINVOKE< Set froground window to {hWnd}."]);
            LogMarshalError();
            return result;
        }

        internal static bool EnumWindowsWrapper(EnumWindowsProc callback, IntPtr lParam)
        {
            Logging.Log([">PINVOKE< Starting window enumeration..."]);
            bool result = EnumWindows(callback, lParam);
            Logging.Log([">PINVOKE< Window enumeration completed."]);
            LogMarshalError();
            return result;
        }

        internal static int GetWindowTextLengthWrapper(IntPtr hWnd)
        {
            Logging.Log([$">PINVOKE< Getting text length from window handle: {hWnd}..."]);
            int length = GetWindowTextLength(hWnd);
            Logging.Log([$">PINVOKE< Text length: {length}"]);
            LogMarshalError();
            return length;
        }

        internal static string GetWindowTextWrapper(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            if (length > 0)
            {
                StringBuilder sb = new StringBuilder(length + 1);
                Logging.Log([$">PINVOKE< Getting text from window handle: {hWnd}..."]);
                GetWindowText(hWnd, sb, sb.Capacity);
                Logging.Log([$">PINVOKE< Retrieved text: {sb}"]);
                LogMarshalError();
                return sb.ToString();
            }
            return string.Empty;
        }

        internal static IntPtr SendMessageWrapper(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam)
        {
            Logging.Log([">PINVOKE< Getting Foreground Desktop Window..."]);
            IntPtr hWnd2 = SendMessage(hWnd, Msg, wParam, lParam);
            Logging.Log([$">PINVOKE< GetForegroundWindow returned hWnd: {hWnd2}"]);
            LogMarshalError();
            return hWnd2;
        }


        internal static bool ShowWindowWrapper(IntPtr hWnd, int nCmdShow)
        {
            Logging.Log([$">PINVOKE< Sending {nCmdShow} message to {hWnd}..."]);
            bool b = ShowWindow(hWnd, nCmdShow);
            Logging.Log([$">PINVOKE< ShowWindow returned bool: {b}"]);
            LogMarshalError();
            return b;
        }

        internal static IntPtr GetForegroundWindowWrapper()
        {
            Logging.Log([">PINVOKE< Getting Foreground Desktop Window..."]);
            IntPtr hWnd = GetForegroundWindow();
            Logging.Log([$">PINVOKE< GetForegroundWindow returned hWnd: {hWnd}"]);
            LogMarshalError();
            return hWnd;
        }

        internal static IntPtr GetDesktopWindowWrapper()
        {
            Logging.Log([">PINVOKE< Getting Desktop Window..."]);
            IntPtr hWnd = GetDesktopWindow();
            Logging.Log([$">PINVOKE< GetDesktopWindow returned hWnd: {hWnd}"]);
            LogMarshalError();
            return hWnd;
        }

        internal static IntPtr GetWindowDCWrapper(IntPtr hWnd)
        {
            Logging.Log([">PINVOKE< Getting Window DC..."]);
            IntPtr hDC = GetWindowDC(hWnd);
            Logging.Log([$">PINVOKE< GetWindowDC for hWnd: {hWnd} returned hDC: {hDC}"]);
            LogMarshalError();
            return hDC;
        }

        internal static bool ReleaseDCWrapper(IntPtr hWnd, IntPtr hDC)
        {
            Logging.Log([">PINVOKE< Releasing DC..."]);
            bool result = ReleaseDC(hWnd, hDC);
            Logging.Log([$">PINVOKE< ReleaseDC for hWnd: {hWnd} and hDC: {hDC} returned {result}"]);
            LogMarshalError();
            return result;
        }

        internal static bool DeleteDCWrapper(IntPtr hDC)
        {
            Logging.Log([">PINVOKE< Deleting DC..."]);
            bool result = DeleteDC(hDC);
            Logging.Log([$">PINVOKE< DeleteDC for hDC: {hDC} returned {result}"]);
            LogMarshalError();
            return result;
        }

        internal static IntPtr CreateCompatibleDCWrapper(IntPtr hDC)
        {
            Logging.Log([">PINVOKE< Creating Compatible DC..."]);
            IntPtr compatibleDC = CreateCompatibleDC(hDC);
            Logging.Log([$">PINVOKE< CreateCompatibleDC for hDC: {hDC} returned compatibleDC: {compatibleDC}"]);
            LogMarshalError();
            return compatibleDC;
        }

        internal static uint BitBltWrapper(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, CopyPixelOperation dwRop)
        {
            Logging.Log([">PINVOKE< Performing BitBlt..."]);
            uint result = BitBlt(hdcDest, nXDest, nYDest, nWidth, nHeight, hdcSrc, nXSrc, nYSrc, dwRop);
            Logging.Log([$">PINVOKE< BitBlt operation returned {result}"]);
            LogMarshalError();
            return result;
        }

        internal static bool DeleteObjectWrapper(IntPtr hObject)
        {
            Logging.Log([">PINVOKE< Deleting GDI Object..."]);
            bool result = DeleteObject(hObject);
            Logging.Log([$">PINVOKE< DeleteObject operation returned {result} for object handle: {hObject}"]);
            LogMarshalError();
            return result;
        }

        internal static IntPtr CreateCompatibleBitmapWrapper(IntPtr hdc, int nWidth, int nHeight)
        {
            Logging.Log([">PINVOKE< Creating Compatible Bitmap..."]);
            IntPtr bitmap = CreateCompatibleBitmap(hdc, nWidth, nHeight);
            Logging.Log([$">PINVOKE< CreateCompatibleBitmap returned bitmap handle: {bitmap}"]);
            LogMarshalError();
            return bitmap;
        }

        internal static IntPtr SelectObjectWrapper(IntPtr hdc, IntPtr hgdiobj)
        {
            Logging.Log([">PINVOKE< Selecting GDI Object..."]);
            IntPtr prevObject = SelectObject(hdc, hgdiobj);
            Logging.Log([$">PINVOKE< SelectObject operation returned previous object handle: {prevObject}"]);
            LogMarshalError();
            return prevObject;
        }

        internal static bool GetCursorPosWrapper(out POINT lpPoint)
        {
            Logging.Log([">PINVOKE< Getting Cursor Position..."]);
            bool b = GetCursorPos(out lpPoint);
            Logging.Log([$">PINVOKE< GetCursorPos returned {b} with position {lpPoint.X}, {lpPoint.Y}"]);
            LogMarshalError();
            return b;
        }

        internal static bool SetCursorPosWrapper(int X, int Y)
        {
            Logging.Log([$">PINVOKE< Setting Cursor Position to X: {X}, Y: {Y}..."]);
            bool result = SetCursorPos(X, Y);
            Logging.Log([$">PINVOKE< SetCursorPos returned {result}"]);
            LogMarshalError();
            return result;
        }

        internal static uint SendInputWrapper(uint nInputs, INPUT[] pInputs)
        {
            Logging.Log([$">PINVOKE< Sending {nInputs} Inputs..."]);
            uint result = SendInput(nInputs, pInputs, INPUT.Size);
            Logging.Log([$">PINVOKE< SendInput processed {result} inputs"]);
            LogMarshalError();
            return result;
        }

        internal static int GetWindowLongWrapper(IntPtr hWnd, int nIndex)
        {
            Logging.Log([$">PINVOKE< Getting Window Long, hWnd: {hWnd}, nIndex: {nIndex}..."]);
            int result = GetWindowLong(hWnd, nIndex);
            Logging.Log([$">PINVOKE< GetWindowLong returned {result}"]);
            LogMarshalError();
            return result;
        }

        internal static int SetWindowLongWrapper(IntPtr hWnd, int nIndex, int dwNewLong)
        {
            Logging.Log([$">PINVOKE< Setting Window Long, hWnd: {hWnd}, nIndex: {nIndex}, NewLong: {dwNewLong}..."]);
            int result = SetWindowLong(hWnd, nIndex, dwNewLong);
            Logging.Log([$">PINVOKE< SetWindowLong returned {result}"]);
            LogMarshalError();
            return result;
        }

        internal static IntPtr SetWindowsHookExWrapper(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId)
        {
            Logging.Log([$">PINVOKE< Setting Windows Hook, idHook: {idHook}, Thread ID: {dwThreadId}..."]);
            IntPtr result = SetWindowsHookEx(idHook, lpfn, hMod, dwThreadId);
            Logging.Log([$">PINVOKE< SetWindowsHookEx returned {result}"]);
            LogMarshalError();
            return result;
        }

        internal static bool UnhookWindowsHookExWrapper(IntPtr hhk)
        {
            Logging.Log([$">PINVOKE< Unhooking Windows Hook, hhk: {hhk}..."]);
            bool result = UnhookWindowsHookEx(hhk);
            Logging.Log([$">PINVOKE< UnhookWindowsHookEx returned {result}"]);
            LogMarshalError();
            return result;
        }

        internal static IntPtr CallNextHookExWrapper(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam)
        {
            IntPtr result = CallNextHookEx(hhk, nCode, wParam, lParam);
            LogMarshalError();
            return result;
        }

        internal static IntPtr GetModuleHandleWrapper(string lpModuleName)
        {
            Logging.Log([$">PINVOKE< Getting Module Handle, Module Name: {lpModuleName}..."]);
            IntPtr result = GetModuleHandle(lpModuleName);
            Logging.Log([$">PINVOKE< GetModuleHandle returned {result}"]);
            LogMarshalError();
            return result;
        }

        internal static short GetAsyncKeyStateWrapper(int vKey)
        {
            short result = GetAsyncKeyState(vKey);
            LogMarshalError();
            return result;
        }

        internal static int ShowCursorWrapper(bool bShow)
        {
            Logging.Log([$">PINVOKE< {(bShow ? "Showing" : "Hiding")} cursor..."]);
            int result = ShowCursor(bShow);
            Logging.Log([$">PINVOKE< ShowCursor action resulted in cursor count {result}"]);
            LogMarshalError();
            return result;
        }

        internal static IntPtr LoadCursorFromFileWrapper(string path)
        {
            Logging.Log([$">PINVOKE< Loading Cursor from file, Path: {path}..."]);
            IntPtr result = LoadCursorFromFile(path);
            Logging.Log([$">PINVOKE< LoadCursorFromFile returned {result}"]);
            LogMarshalError();
            return result;
        }

        internal static bool SetSystemCursorWrapper(IntPtr hcur, uint id)
        {
            Logging.Log([$">PINVOKE< Setting System Cursor, Cursor Handle: {hcur}, ID: {id}..."]);
            bool result = SetSystemCursor(hcur, id);
            Logging.Log([$">PINVOKE< SetSystemCursor returned {result}"]);
            LogMarshalError();
            return result;
        }

        internal static bool SystemParametersInfoWrapper(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni)
        {
            Logging.Log([$">PINVOKE< Setting System Cursor, uiAction: {uiAction}, uiParam: {uiParam}, pvParam: {pvParam}, fWinIni: {fWinIni}"]);
            bool b = SystemParametersInfo(uiAction, uiParam, pvParam, fWinIni);
            Logging.Log([$">PINVOKE< SystemParametersInfo returned {b}"]);
            LogMarshalError();
            return b;
        }

        #endregion Internal Methods Wrapping P/Invoke

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void LogMarshalError(
            [CallerMemberName] string cname = "",
            [CallerLineNumber] int ln = 0
            )
        {
            int errorCode = Marshal.GetLastWin32Error();
            Logging.Log([$"Marshal Erroring check called by {cname} @ {ln}"], false);
            if (errorCode != 0)
                Logging.Log([$">PINVOKE< Error: {errorCode} - {new System.ComponentModel.Win32Exception(errorCode).Message}"]);
            else Logging.Log(["Marshal operation successful"]);
        }

        [Flags]
        internal enum CopyPixelOperation : uint
        {
            Blackness = 0x00000042,
            NotMergePen = 0x00000002,
            MaskNotPen = 0x00000003,
            NotCopyPen = 0x00000004,
            MaskPenNot = 0x00000005,
            Not = 0x00000006,
            XorPen = 0x00000007,
            NotMaskPen = 0x00000008,
            MaskPen = 0x00000009,
            NotXorPen = 0x0000000A,
            Nop = 0x0000000B,
            MergeNotPen = 0x0000000C,
            CopyPen = 0x0000000D,
            MergePenNot = 0x0000000E,
            MergePen = 0x0000000F,
            White = 0x00000010,
            SourceErase = 0x00000044,
            DestinationInvert = 0x00000055,
            PatInvert = 0x0000005A,
            SourceInvert = 0x00000066,
            SourceAnd = 0x00000088,
            MergePaint = 0x000000BB,
            SourceCopy = 0x00CC0020,
            SourcePaint = 0x00EE0086,
            PatCopy = 0x00F00021,
            PatPaint = 0x00FB0A09,
            Whiteness = 0x00FF0062,
            CaptureBlt = 0x40000000
        }
    }
}