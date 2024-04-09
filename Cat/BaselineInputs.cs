// -----------------------------------------------------------------------
// BaselineInputs.cs
// Contains utility methods for manipulating audio settings, cursor appearance, 
// keyboard inputs, and mouse movements.
// Author: Nexus
// -----------------------------------------------------------------------


using NAudio.CoreAudioApi;
using System.IO;

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
        [LoggingAspects.Logging]
        [LoggingAspects.ConsumeException]
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
            internal static CursorType CurrentCursor { get; private set; }

            /// <summary>
            /// Changes the cursor to a specified file.
            /// </summary>
            /// <param name="filename">The path to the cursor file.</param>
            /// <returns>True if the cursor was successfully changed; otherwise, false.</returns>
            [LoggingAspects.ConsumeException]
            [LoggingAspects.Logging]
            private static bool ChangeCursor(string filename)
            {
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
                    SetSystemCursorWrapper(cursorHandle, OCR_NORMAL);
                    return true;
                }
                else
                {
                    throw new InvalidOperationException($"Failed to load cursor from file.");
                }
            }

            /// <summary>
            /// Sets the cursor to a black point.
            /// </summary>
            [LoggingAspects.ConsumeException]
            [LoggingAspects.Logging]
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
            [LoggingAspects.ConsumeException]
            [LoggingAspects.Logging]
            internal static void Reset()
            {
                Logging.Log("Resetting system cursors...");
                if (!SystemParametersInfoWrapper(SPI_SETCURSORS, 0, IntPtr.Zero, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE))
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

        /// <summary>
        /// Simulates keyboard input for a specified virtual key code.
        /// </summary>
        /// <param name="virtualKeyCode">The virtual key code of the key.</param>
        internal static void SendKeyboardInput(ushort virtualKeyCode)
        {
            Logging.Log($"Sending KEYUP KEYDOWN for VK {virtualKeyCode}");
            var inputs = new Structs.INPUT[2];

            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].U.ki.wVk = virtualKeyCode;
            inputs[0].U.ki.dwFlags = KEYEVENTF_EXTENDEDKEY;

            inputs[1].type = INPUT_KEYBOARD;
            inputs[1].U.ki.wVk = virtualKeyCode;
            inputs[1].U.ki.dwFlags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP;

            SendInputWrapper((uint)inputs.Length, inputs);
        }

        /// <summary>
        /// Toggles the mute status of the default audio endpoint device.
        /// </summary>
        [LoggingAspects.Logging]
        internal static void ToggleMuteSound()
        {
            Logging.Log($"Toggling mute...");
            SendKeyboardInput(VK_VOLUME_MUTE);
            Logging.Log($"Mute toggle operation complete.");
        }

        /// <summary>
        /// Toggles the mute status of the default audio endpoint device based on a given condition.
        /// </summary>
        /// <param name="on">Indicates whether to mute (true) or unmute (false) the device.</param>
        [LoggingAspects.Logging]
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
        [LoggingAspects.Logging]
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
        [LoggingAspects.Logging]
        internal static int ShowCursor()
        {
            Logging.Log("Attempting to show cursor. (WARNING: This operation may fail without error)");
            int curs = PInvoke.ShowCursorWrapper(true);
            while (curs < 0)
                curs = PInvoke.ShowCursorWrapper(true);
            return curs;
        }

        /// <summary>
        /// Simulates typing "HELLO" using keyboard input.
        /// </summary>
        [LoggingAspects.Logging]
        [LoggingAspects.ConsumeException]
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
        /// Causes the mouse to move erratically on the screen.
        /// </summary>
        /// <param name="smooth">Indicates whether the movement should be smooth.</param>
        [LoggingAspects.Logging]
        [LoggingAspects.AsyncExceptionSwallower]
        internal static async void CauseMouseToHaveSpasticAttack(bool smooth = false)
        {
            Logging.Log($"Causing mouse to have a spastic attack, smoothing: {smooth}");
            for (int i = 0; i < (smooth ? 1000 : 100); i++)
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
        [LoggingAspects.Logging]
        [LoggingAspects.AsyncExceptionSwallower]
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