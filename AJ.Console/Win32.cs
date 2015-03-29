// Source: https://github.com/ajdotnet/AJ.Console
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace AJ.Console
{
    /// <summary>console colors</summary>
    public enum Color
    {
        /// <summary></summary>
        Black = 0 | 0 | 0,
        /// <summary></summary>
        Navy = 0 | 0 | 1,
        /// <summary></summary>
        Blue = 0 | 0 | 1 | 8,
        /// <summary></summary>
        Green = 0 | 2 | 0,
        /// <summary></summary>
        Lime = 0 | 2 | 0 | 8,
        /// <summary></summary>
        Maroon = 4 | 0 | 0,
        /// <summary></summary>
        Red = 4 | 0 | 0 | 8,
        /// <summary></summary>
        Teal = 0 | 2 | 1,
        /// <summary></summary>
        Cyan = 0 | 2 | 1 | 8,
        /// <summary></summary>
        Olive = 4 | 2 | 0,
        /// <summary></summary>
        Yellow = 4 | 2 | 0 | 8,
        /// <summary></summary>
        Purple = 4 | 0 | 1,
        /// <summary></summary>
        Magenta = 4 | 0 | 1 | 8,
        /// <summary></summary>
        Gray = 4 | 2 | 1,
        /// <summary></summary>
        White = 4 | 2 | 1 | 8,
    };

    /// <summary>
    /// Native functions.
    /// </summary>
    static class Win32
    {
        /// <summary></summary>
        public enum StdHandle
        {
            /// <summary></summary>
            Input = -10,	// ((ulong)-10),	// STD_INPUT_HANDLE ,
            /// <summary></summary>
            Output = -11,	// ((ulong)-11),	// STD_OUTPUT_HANDLE ,  
            /// <summary></summary>
            Error = -12   // ((ulong)-12)		// STD_ERROR_HANDLE    
        };

        /// <summary>
        /// Sets the color of the console text.
        /// </summary>
        /// <param name="stdHandle">The standard handle.</param>
        /// <param name="foreground">The foreground color.</param>
        /// <param name="background">The background color.</param>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "AJ.Console.Win32+NativeMethods.SetConsoleTextAttribute(System.IntPtr,System.UInt16)")]
        static public void SetConsoleTextColor(StdHandle stdHandle, Color foreground, Color background)
        {
            ushort f = (ushort)foreground;
            ushort b = (ushort)(((ushort)background) << 4);
            ushort a = (ushort)(f | b);
            IntPtr h = NativeMethods.GetStdHandle(stdHandle);
            NativeMethods.SetConsoleTextAttribute((IntPtr)h, a);
        }

        /// <summary>
        /// Gets the color of the console text.
        /// </summary>
        /// <param name="stdHandle">The standard handle.</param>
        /// <param name="foreground">The foreground color.</param>
        /// <param name="background">The background color.</param>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "AJ.Console.Win32+NativeMethods.GetConsoleScreenBufferInfo(System.IntPtr,AJ.Console.Win32+NativeMethods+CONSOLE_SCREEN_BUFFER_INFO@)")]
        static public void GetConsoleTextColor(StdHandle stdHandle, out Color foreground, out Color background)
        {
            NativeMethods.CONSOLE_SCREEN_BUFFER_INFO info = new NativeMethods.CONSOLE_SCREEN_BUFFER_INFO();
            IntPtr h = NativeMethods.GetStdHandle(stdHandle);
            NativeMethods.GetConsoleScreenBufferInfo(h, ref info);
            foreground = (Color)(info.wAttributes & 0x0f);
            background = (Color)((info.wAttributes & 0xf0) >> 4);
        }

        static class NativeMethods
        {
            /// <summary>
            /// HANDLE GetStdHandle( DWORD nStdHandle );
            /// </summary>
            /// <param name="stdHandle">The standard handle.</param>
            /// <returns></returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetStdHandle")]
            static public extern System.IntPtr GetStdHandle(StdHandle stdHandle);

            /// <summary>
            /// BOOL SetConsoleTextAttribute( HANDLE hConsoleOutput, WORD wAttributes );
            /// </summary>
            /// <param name="hConsoleOutput">The console output hanlde.</param>
            /// <param name="wAttributes">The output attributes.</param>
            /// <returns></returns>
            [DllImport("Kernel32.dll", EntryPoint = "SetConsoleTextAttribute")]
            public static extern Int32 SetConsoleTextAttribute(IntPtr hConsoleOutput, ushort wAttributes);

            /// <summary>
            /// BOOL GetConsoleScreenBufferInfo( HANDLE hConsoleOutput, PCONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo );
            /// </summary>
            /// <param name="hConsoleOutput">The console output.</param>
            /// <param name="lpConsoleScreenBufferInfo">The console screen buffer information.</param>
            /// <returns></returns>
            [DllImport("Kernel32.dll", EntryPoint = "GetConsoleScreenBufferInfo")]
            public static extern Int32 GetConsoleScreenBufferInfo(IntPtr hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

            [StructLayout(LayoutKind.Sequential)]
            public struct COORD
            {
                public short X;
                public short Y;
            };

            [StructLayout(LayoutKind.Sequential)]
            public struct SMALL_RECT
            {
                public short Left;
                public short Top;
                public short Right;
                public short Bottom;
            };

            [StructLayout(LayoutKind.Sequential)]
            public struct CONSOLE_SCREEN_BUFFER_INFO
            {
                public COORD dwSize;
                public COORD dwCursorPosition;
                public ushort wAttributes;
                public SMALL_RECT srWindow;
                public COORD dwMaximumWindowSize;
            };
        }
    }
}
