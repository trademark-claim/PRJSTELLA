using System.Runtime.InteropServices;

namespace Cat
{
    internal static class PInvoke
    {
        #region P/Invoke Declarations

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int ShowCursor(bool bShow);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursorFromFile(string path);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetSystemCursor(IntPtr hcur, uint id);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDesktopWindow();
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);
        
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern uint BitBlt(IntPtr hDestDC, int xDest, int yDest, int wDest, int hDest, IntPtr hSrcDC, int xSrc, int ySrc, CopyPixelOperation rop);
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int width, int height);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteDC(IntPtr hDC);


        #endregion P/Invoke Declarations

        #region Internal Methods Wrapping P/Invoke

        internal static IntPtr GetDesktopWindowWrapper()
        {
            Logging.Log(">PINVOKE< Getting Desktop Window...");
            IntPtr hWnd = GetDesktopWindow();
            Logging.Log($">PINVOKE< GetDesktopWindow returned hWnd: {hWnd}");
            return hWnd;
        }

        internal static IntPtr GetWindowDCWrapper(IntPtr hWnd)
        {
            Logging.Log(">PINVOKE< Getting Window DC...");
            IntPtr hDC = GetWindowDC(hWnd);
            Logging.Log($">PINVOKE< GetWindowDC for hWnd: {hWnd} returned hDC: {hDC}");
            if (hDC == IntPtr.Zero)
                LogMarshalError();
            return hDC;
        }

        internal static bool ReleaseDCWrapper(IntPtr hWnd, IntPtr hDC)
        {
            Logging.Log(">PINVOKE< Releasing DC...");
            bool result = ReleaseDC(hWnd, hDC);
            Logging.Log($">PINVOKE< ReleaseDC for hWnd: {hWnd} and hDC: {hDC} returned {result}");
            if (!result)
                LogMarshalError();
            return result;
        }

        internal static bool DeleteDCWrapper(IntPtr hDC)
        {
            Logging.Log(">PINVOKE< Deleting DC...");
            bool result = DeleteDC(hDC);
            Logging.Log($">PINVOKE< DeleteDC for hDC: {hDC} returned {result}");
            if (!result)
                LogMarshalError();
            return result;
        }

        internal static IntPtr CreateCompatibleDCWrapper(IntPtr hDC)
        {
            Logging.Log(">PINVOKE< Creating Compatible DC...");
            IntPtr compatibleDC = CreateCompatibleDC(hDC);
            Logging.Log($">PINVOKE< CreateCompatibleDC for hDC: {hDC} returned compatibleDC: {compatibleDC}");
            if (compatibleDC == IntPtr.Zero)
                LogMarshalError();
            return compatibleDC;
        }

        internal static uint BitBltWrapper(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, CopyPixelOperation dwRop)
        {
            Logging.Log(">PINVOKE< Performing BitBlt...");
            uint result = BitBlt(hdcDest, nXDest, nYDest, nWidth, nHeight, hdcSrc, nXSrc, nYSrc, dwRop);
            Logging.Log($">PINVOKE< BitBlt operation returned {result}");
            LogMarshalError();
            return result;
        }

        internal static bool DeleteObjectWrapper(IntPtr hObject)
        {
            Logging.Log(">PINVOKE< Deleting GDI Object...");
            bool result = DeleteObject(hObject);
            Logging.Log($">PINVOKE< DeleteObject operation returned {result} for object handle: {hObject}");
            if (!result)
                LogMarshalError();
            return result;
        }

        internal static IntPtr CreateCompatibleBitmapWrapper(IntPtr hdc, int nWidth, int nHeight)
        {
            Logging.Log(">PINVOKE< Creating Compatible Bitmap...");
            IntPtr bitmap = CreateCompatibleBitmap(hdc, nWidth, nHeight);
            Logging.Log($">PINVOKE< CreateCompatibleBitmap returned bitmap handle: {bitmap}");
            if (bitmap == IntPtr.Zero)
                LogMarshalError();
            return bitmap;
        }

        internal static IntPtr SelectObjectWrapper(IntPtr hdc, IntPtr hgdiobj)
        {
            Logging.Log(">PINVOKE< Selecting GDI Object...");
            IntPtr prevObject = SelectObject(hdc, hgdiobj);
            Logging.Log($">PINVOKE< SelectObject operation returned previous object handle: {prevObject}");
            if (prevObject == IntPtr.Zero || prevObject == new IntPtr(-1))
                LogMarshalError();
            return prevObject;
        }

        internal static bool GetCursorPosWrapper(out POINT lpPoint)
        {
            Logging.Log(">PINVOKE< Getting Cursor Position...");
            bool b = GetCursorPos(out lpPoint);
            Logging.Log($">PINVOKE< GetCursorPos returned {b} with position {lpPoint.X}, {lpPoint.Y}");
            if (!b)
                LogMarshalError();
            return b;
        }

        internal static bool SetCursorPosWrapper(int X, int Y)
        {
            Logging.Log($">PINVOKE< Setting Cursor Position to X: {X}, Y: {Y}...");
            bool result = SetCursorPos(X, Y);
            Logging.Log($">PINVOKE< SetCursorPos returned {result}");
            if (!result)
                LogMarshalError();
            return result;
        }

        internal static uint SendInputWrapper(uint nInputs, INPUT[] pInputs)
        {
            Logging.Log($">PINVOKE< Sending {nInputs} Inputs...");
            uint result = SendInput(nInputs, pInputs, INPUT.Size);
            Logging.Log($">PINVOKE< SendInput processed {result} inputs");
            return result;
        }

        internal static int GetWindowLongWrapper(IntPtr hWnd, int nIndex)
        {
            Logging.Log($">PINVOKE< Getting Window Long, hWnd: {hWnd}, nIndex: {nIndex}...");
            int result = GetWindowLong(hWnd, nIndex);
            Logging.Log($">PINVOKE< GetWindowLong returned {result}");
            return result;
        }

        internal static int SetWindowLongWrapper(IntPtr hWnd, int nIndex, int dwNewLong)
        {
            Logging.Log($">PINVOKE< Setting Window Long, hWnd: {hWnd}, nIndex: {nIndex}, NewLong: {dwNewLong}...");
            int result = SetWindowLong(hWnd, nIndex, dwNewLong);
            Logging.Log($">PINVOKE< SetWindowLong returned {result}");
            return result;
        }

        internal static IntPtr SetWindowsHookExWrapper(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId)
        {
            Logging.Log($">PINVOKE< Setting Windows Hook, idHook: {idHook}, Thread ID: {dwThreadId}...");
            IntPtr result = SetWindowsHookEx(idHook, lpfn, hMod, dwThreadId);
            Logging.Log($">PINVOKE< SetWindowsHookEx returned {result}");
            return result;
        }

        internal static bool UnhookWindowsHookExWrapper(IntPtr hhk)
        {
            Logging.Log($">PINVOKE< Unhooking Windows Hook, hhk: {hhk}...");
            bool result = UnhookWindowsHookEx(hhk);
            Logging.Log($">PINVOKE< UnhookWindowsHookEx returned {result}");
            if (!result)
                LogMarshalError();
            return result;
        }

        internal static IntPtr CallNextHookExWrapper(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam)
        {
            IntPtr result = CallNextHookEx(hhk, nCode, wParam, lParam);
            return result;
        }

        internal static IntPtr GetModuleHandleWrapper(string lpModuleName)
        {
            Logging.Log($">PINVOKE< Getting Module Handle, Module Name: {lpModuleName}...");
            IntPtr result = GetModuleHandle(lpModuleName);
            Logging.Log($">PINVOKE< GetModuleHandle returned {result}");
            return result;
        }

        internal static short GetAsyncKeyStateWrapper(int vKey)
        {
            short result = GetAsyncKeyState(vKey);
            return result;
        }

        internal static int ShowCursorWrapper(bool bShow)
        {
            Logging.Log($">PINVOKE< {(bShow ? "Showing" : "Hiding")} cursor...");
            int result = ShowCursor(bShow);
            Logging.Log($">PINVOKE< ShowCursor action resulted in cursor count {result}");
            return result;
        }

        internal static IntPtr LoadCursorFromFileWrapper(string path)
        {
            Logging.Log($">PINVOKE< Loading Cursor from file, Path: {path}...");
            IntPtr result = LoadCursorFromFile(path);
            Logging.Log($">PINVOKE< LoadCursorFromFile returned {result}");
            return result;
        }

        internal static bool SetSystemCursorWrapper(IntPtr hcur, uint id)
        {
            Logging.Log($">PINVOKE< Setting System Cursor, Cursor Handle: {hcur}, ID: {id}...");
            bool result = SetSystemCursor(hcur, id);
            Logging.Log($">PINVOKE< SetSystemCursor returned {result}");
            if (!result)
                LogMarshalError();
            return result;
        }

        internal static bool SystemParametersInfoWrapper(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni)
        {
            Logging.Log($">PINVOKE< Setting System Cursor, uiAction: {uiAction}, uiParam: {uiParam}, pvParam: {pvParam}, fWinIni: {fWinIni}");
            bool b = SystemParametersInfo(uiAction, uiParam, pvParam, fWinIni);
            Logging.Log($">PINVOKE< SystemParametersInfo returned {b}");
            if (!b)
                LogMarshalError();
            return b;
        }

        #endregion Internal Methods Wrapping P/Invoke

        private static void LogMarshalError()
        {
            int errorCode = Marshal.GetLastWin32Error();
            Logging.Log($">PINVOKE< Error: {errorCode} - {new System.ComponentModel.Win32Exception(errorCode).Message}");
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