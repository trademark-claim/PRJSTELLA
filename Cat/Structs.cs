﻿//-------------------------------------------------------------------------------------
// <summary>
//     Structs.cs
//     Contains structures used for simulating input events for keyboard, mouse,
//     and hardware devices.
// </summary>
//-------------------------------------------------------------------------------------

using System.Runtime.InteropServices;

/// <summary>
/// Contains structures used for simulating input events.
/// </summary>
namespace Cat
{
    internal static class Structs
    {
        #region Structs

        /// <summary>
        /// Represents a point in 2-dimensional space.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int X;
            public int Y;
        }

        /// <summary>
        /// Represents an input event.
        /// </summary>
        /// <remarks>
        /// This structure can represent a mouse, keyboard, or hardware input event.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            public uint type;
            public InputUnion U;
            public static int Size => Marshal.SizeOf(typeof(INPUT));
        }

        /// <summary>
        /// Union of input structures representing different types of input.
        /// </summary>
        /// <remarks>
        /// This union allows a single input to represent either mouse, keyboard, or hardware input.
        /// </remarks>
        [StructLayout(LayoutKind.Explicit)]
        internal struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        /// <summary>
        /// Contains information about a simulated mouse event.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            public POINT pt;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        /// <summary>
        /// Contains information about a simulated keyboard event.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        /// <summary>
        /// Contains information about a simulated message generated by an input device other than a keyboard or mouse.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        #endregion Structs
    }
}