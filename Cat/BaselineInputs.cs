// -----------------------------------------------------------------------
// BaselineInputs.cs
// Contains utility methods for manipulating audio settings, cursor appearance,
// keyboard inputs, and mouse movements.
// Author: Nexus
// -----------------------------------------------------------------------

using NAudio.CoreAudioApi;
using System.IO;
using System.Runtime.InteropServices;

namespace Cat
{
    /// <summary>
    /// Provides methods for manipulating system baseline inputs such as audio, cursor,
    /// keyboard, and mouse.
    /// </summary>
    internal static class BaselineInputs
    {
        /// <summary>
        /// Checks if the default audio endpoint device is muted.
        /// </summary>
        /// <returns>True if the device is muted; otherwise, false.</returns>
        [CAspects.Logging]
        [CAspects.ConsumeException]
        internal static bool IsMute()
        {
            Logging.Log("Checking Mute status of device...");
            using (var enumerator = new MMDeviceEnumerator())
            {
                var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                Logging.Log($"Mute Status: {device.AudioEndpointVolume.Mute}");
                return device.AudioEndpointVolume.Mute;
            }
        }

        /// <summary>
        /// Handles cursor-related functionalities.
        /// </summary>
        internal static class Cursor
        {
            internal static readonly HashSet<(string, string)> validEntries =
            [
                ("OCR_APPSTARTING", "The cursor indicating that an application is starting or loading."),
                ("OCR_NORMAL", "The normal cursor, usually a pointer for selection."),
                ("OCR_CROSS", "A crosshair cursor, often used for precise alignment or selection."),
                ("OCR_HAND", "A hand cursor, typically used to indicate a clickable link or object."),
                ("OCR_HELP", "A help cursor, usually a question mark or pointer with a question mark, indicating help or more information is available."),
                ("OCR_IBEAM", "An I-beam cursor, used to indicate that text can be edited or selected."),
                ("OCR_UNAVAILABLE", "A cursor indicating that an action cannot be taken, often shown as a circle with a line through it."),
                ("OCR_SIZEALL", "A sizing cursor, indicating that an object can be resized in any direction."),
                ("OCR_SIZENESW", "A diagonal sizing cursor, indicating resizing from the northeast to southwest or vice versa."),
                ("OCR_SIZENS", "A vertical sizing cursor, indicating resizing in the north-south direction."),
                ("OCR_SIZENWSE", "A diagonal sizing cursor, indicating resizing from the northwest to southeast or vice versa."),
                ("OCR_SIZEWE", "A horizontal sizing cursor, indicating resizing in the west-east direction."),
                ("OCR_UP", "An up arrow cursor, often used to indicate an upward action or movement."),
                ("OCR_WAIT", "A wait cursor, typically shown as an hourglass or spinning circle, indicating a process is ongoing and the user must wait.")
            ];

            internal static CursorType CurrentCursor { get; private set; }

            [CAspects.Logging]
            [CAspects.ConsumeException]
            internal static void LoadPresetByIndex(int num)
            {
                var dirs = Directory.EnumerateDirectories(CursorsFilePath).ToArray().Select(x => x.Replace(CursorsFilePath, "")).ToArray();
                --num;
                if (num < dirs.Count())
                {
                    Commands.commandstruct = new("", "", [[dirs[num],], [false,]]);
                    Commands.LoadCursorPreset();
                }
                Logging.Log("");
            }

            /// <summary>
            /// Changes the cursor to a specified file.
            /// </summary>
            /// <param name="filename">The path to the cursor file.</param>
            /// <returns>True if the cursor was successfully changed; otherwise, false.</returns>
            [CAspects.ConsumeException]
            [CAspects.Logging]
            internal static bool ChangeCursor(string filename, uint id = OCR_NORMAL, bool persistent = false)
            {
                bool allg = true;
                Logging.Log("Changing cursor to: " + filename);
                bool isValid = ValidateFile(filename);
                if (!isValid)
                {
                    Logging.Log($"{filename} invalid, returning.");
                    return false;
                }
                Logging.Log("Loading cursor handling...");
                IntPtr cursorHandle = LoadCursorFromFileWrapper(filename);
                Logging.Log($"Loaded handle: {cursorHandle}");
                if (cursorHandle != IntPtr.Zero)
                {
                    SetSystemCursorWrapper(cursorHandle, id);
                    if (persistent && Helpers.BackendHelping.CheckIfAdmin())
                    {
                        string cursorRegistryKey = GetCursorRegistryKeyName(id);
                        if (!string.IsNullOrEmpty(cursorRegistryKey))
                        {
                            allg = SetPersistentCursor(filename, cursorRegistryKey);
                        }
                    }
                    return allg;
                }
                else
                {
                    throw new InvalidOperationException($"Failed to load cursor from file.");
                }
            }

            [CAspects.Logging]
            [CAspects.ConsumeException]
            private static bool SetPersistentCursor(string cursorPath, string cursorRegistryKey)
            {
                var cursorsRegistryPath = @"Control Panel\Cursors";
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(cursorsRegistryPath, true))
                {
                    if (key != null)
                    {
                        key.SetValue(cursorRegistryKey, cursorPath);
                        SystemParametersInfoWrapper(SPI_SETCURSORS, 0, IntPtr.Zero, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                    }
                    else
                        return false;
                }
                return true;
            }

            /// <summary>
            /// Gets the registry key name for a given system cursor ID.
            /// </summary>
            /// <param name="id">The system cursor ID.</param>
            /// <returns>The registry key name corresponding to the system cursor ID.</returns>
            private static string GetCursorRegistryKeyName(uint id)
            {
                switch (id)
                {
                    case OCR_NORMAL: return "Arrow"; // Standard arrow cursor
                    case OCR_IBEAM: return "IBeam"; // Text select cursor
                    case OCR_WAIT: return "Wait"; // Hourglass or spinning circle cursor
                    case OCR_CROSS: return "Crosshair"; // Crosshair cursor
                    case OCR_UP: return "UpArrow"; // Up arrow cursor
                    case OCR_SIZENWSE: return "SizeNWSE"; // Diagonal resize cursor (/)
                    case OCR_SIZENESW: return "SizeNESW"; // Diagonal resize cursor (\)
                    case OCR_SIZEWE: return "SizeWE"; // Horizontal resize cursor
                    case OCR_SIZENS: return "SizeNS"; // Vertical resize cursor
                    case OCR_SIZEALL: return "SizeAll"; // Move cursor
                    case OCR_UNAVAILABLE: return "No"; // Unavailable cursor (circle with a slash)
                    case OCR_HAND: return "Hand"; // Hand cursor
                                                  // Default case for unidentified or custom cursors.
                    default: return null; // Returns null if the ID doesn't match known values
                }
            }

            /// <summary>
            /// Sets the cursor to a black point.
            /// </summary>
            [CAspects.ConsumeException]
            [CAspects.Logging]
            internal static void BlackPoint()
            {
                string path = Path.Combine(RunningPath, BCURPATH);
                //System.Windows.MessageBox.Show(path);
                CurrentCursor = CursorType.BlackPoint;
                ChangeCursor(path);
            }

            /// <summary>
            /// Resets the cursor to the default system cursor.
            /// </summary>
            [CAspects.ConsumeException]
            [CAspects.Logging]
            internal static void Reset()
            {
                Logging.Log("Resetting system cursors...");
                if (!SystemParametersInfoWrapper(SPI_SETCURSORS, 0, IntPtr.Zero, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE) && Marshal.GetLastWin32Error() != 0)
                {
                    throw new InvalidOperationException("Failed to restore default system cursors.");
                }
                CurrentCursor = CursorType.Default;
                Logging.Log("Complete.");
            }

            internal enum CursorType : byte
            {
                Default,
                BlackPoint
            }
        }

        internal readonly record struct ExtendedInput(ushort Key, byte DownFor = 0, bool up = false);

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task SendKeyboardInput(int delayms, params ExtendedInput[] vks)
        {
            var lst = vks.ToList();
            List<(int, ExtendedInput)> eexis = new();
            for (int i = 0; i < vks.Length; i++)
            {
                ExtendedInput v = vks[i];
                if (v.DownFor > 0)
                    eexis.Add((i + v.DownFor + 1 + eexis.Count, new(v.Key, 0, true)));
            }
            foreach (var v in eexis)
                lst.Insert(v.Item1, v.Item2);
            Logging.Log("New List: ", lst);
            for (int i = 0; i < lst.Count; i++)
            {
                ExtendedInput v = lst[i];
                SendKeyboardInput(v.Key, v.DownFor > 0 ? false : v.up ? true : null);
                await Task.Delay(TimeSpan.FromMilliseconds(delayms));
            }
            
        }

        /// <summary>
        /// Simulates keyboard input for a specified virtual key code.
        /// </summary>
        /// <param name="virtualKeyCode">The virtual key code of the key.</param>
        [CAspects.Logging]
        [CAspects.ConsumeException]
        internal static void SendKeyboardInput(ushort virtualKeyCode, bool? onlyup = null)
        {
            Logging.Log($"Sending {(onlyup == true ? "KEYUP" : onlyup == false? "KEYDOWN" : "KEYDOWN and KEYUP")} for VK {virtualKeyCode}");
            var inputs = new Structs.INPUT[onlyup == null? 2 : 1];

            if (onlyup == null || onlyup == false)
            {
                inputs[0].type = INPUT_KEYBOARD;
                inputs[0].U.ki.wVk = virtualKeyCode;
                inputs[0].U.ki.dwFlags = KEYEVENTF_EXTENDEDKEY;
            }

            if (onlyup == null || onlyup == true)
            {
                inputs[onlyup == null ? 1 : 0].type = INPUT_KEYBOARD;
                inputs[onlyup == null ? 1 : 0].U.ki.wVk = virtualKeyCode;
                inputs[onlyup == null ? 1 : 0].U.ki.dwFlags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP;
            }
            SendInputWrapper((uint)inputs.Length, inputs);
        }

        /// <summary>
        /// Simulates typing "HELLO" using keyboard input.
        /// </summary>
        [CAspects.Logging]
        [CAspects.ConsumeException]
        internal static void SendHello()
        {
            Logging.Log($"Sending fake hello VIn");
            List<INPUT> inputs = new List<INPUT>();
            var vkCodes = new ushort[] { 0x48, 0x45, 0x4C, 0x4C, 0x4F };

            foreach (var vkCode in vkCodes)
            {
                inputs.Add(new INPUT
                {
                    type = 1,
                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = vkCode,
                            dwFlags = 0,
                        }
                    }
                });
                inputs.Add(new INPUT
                {
                    type = 1,
                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = vkCode,
                            dwFlags = 2,
                        }
                    }
                });
            }

            SendInputWrapper((uint)inputs.Count, inputs.ToArray());
        }

        /// <summary>
        /// Toggles the mute status of the default audio endpoint device.
        /// </summary>
        [CAspects.Logging]
        internal static void ToggleMuteSound()
        {
            Logging.Log($"Toggling mute...");
            SendKeyboardInput(VK_VOLUME_MUTE);
            Logging.Log($"Mute toggle operation complete.");
        }

        [CAspects.Logging]
        internal static void IncrVol20()
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                if (defaultDevice.AudioEndpointVolume.Mute)
                {
                    defaultDevice.AudioEndpointVolume.Mute = false;
                }
                float currentVolume = defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
                float newVolume = currentVolume + 0.2f;
                newVolume = Math.Min(newVolume, 1.0f);
                defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = newVolume;
            }
        }

        [CAspects.Logging]
        internal static void DecrVol20()
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                if (defaultDevice.AudioEndpointVolume.Mute)
                {
                    defaultDevice.AudioEndpointVolume.Mute = false;
                }
                float currentVolume = defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
                float newVolume = currentVolume - 0.2f;
                newVolume = Math.Max(newVolume, 0.0f);
                defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = newVolume;
            }
        }

        /// <summary>
        /// Toggles the mute status of the default audio endpoint device based on a given condition.
        /// </summary>
        /// <param name="on">Indicates whether to mute (true) or unmute (false) the device.</param>
        [CAspects.Logging]
        internal static void ToggleMuteSound(bool on)
        {
            bool muted = IsMute();
            if (on && muted)
            {
                ToggleMuteSound();
            }
            else if (!on && !muted)
            {
                ToggleMuteSound();
            }
        }

        /// <summary>
        /// Hides the cursor.
        /// </summary>
        /// <returns>The new display count of the cursor.</returns>
        [CAspects.Logging]
        internal static int HideCursor()
        {
            Logging.Log("Attempting to hide cursor. (WARNING: This operation may fail without error)");
            int curs = PInvoke.ShowCursorWrapper(false);
            while (curs >= 0)
                curs = PInvoke.ShowCursorWrapper(false);
            return curs;
        }

        /// <summary>
        /// Shows the cursor.
        /// </summary>
        /// <returns>The new display count of the cursor.</returns>
        [CAspects.Logging]
        internal static int ShowCursor()
        {
            Logging.Log("Attempting to show cursor. (WARNING: This operation may fail without error)");
            int curs = PInvoke.ShowCursorWrapper(true);
            while (curs < 0)
                curs = PInvoke.ShowCursorWrapper(true);
            return curs;
        }

        /// <summary>
        /// Causes the mouse to move erratically on the screen.
        /// </summary>
        /// <param name="smooth">Indicates whether the movement should be smooth.</param>
        [CAspects.Logging]
        [CAspects.ConsumeException]
        internal static async void CauseMouseToHaveSpasticAttack(bool smooth = false)
        {
            Logging.Log($"Causing mouse to have a spastic attack, smoothing: {smooth}");
            for (int i = 0; i < (smooth ? 20 : 1000); i++)
            {
                if (smooth)
                {
                    POINT point;
                    GetCursorPosWrapper(out point);
                    var scre = Catowo.GetScreen();
                    await MoveMouseSmoothly(point.X, point.Y, (int)(scre.Bounds.Width), random.Next(0, (int)scre.Bounds.Height), random.Next(10, 400));
                }
                else
                {
                    var scre = Catowo.GetScreen();
                    SetCursorPosWrapper(random.Next(0, (int)(scre.Bounds.Width)), random.Next(0, (int)scre.Bounds.Height));
                }
                await Task.Delay(5);
            }
            Logging.Log($"Spastic Attack complete.");
        }

        /// <summary>
        /// Smoothly moves the mouse cursor from one point to another.
        /// </summary>
        /// <param name="startX">The starting X coordinate.</param>
        /// <param name="startY">The starting Y coordinate.</param>
        /// <param name="endX">The ending X coordinate.</param>
        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task MoveMouseSmoothly(int startX, int startY, int endX, int endY, int duration)
        {
            Logging.Log($"Moving mouse smoothly from {startX}:{startY} to {endX}:{endY} over {duration} milliseconds. (100 steps)");
            const int steps = 100;
            int deltaX = (endX - startX) / steps;
            int deltaY = (endY - startY) / steps;
            int delay = duration / steps;

            for (int i = 0; i <= steps; i++)
            {
                SetCursorPosWrapper(startX + deltaX * i, startY + deltaY * i);
                await Task.Delay(delay);
            }
        }
    }
}