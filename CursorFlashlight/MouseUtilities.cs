using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

/*
  Found this on google somewhere, was not the original author.
  Does a really good job of getting a WPF point for Cursor position  
*/

namespace CursorFlashlight
{
    public static class MouseUtilities
    {
        public static Point CorrectGetPosition(Visual relativeTo)
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            try
            {
                return relativeTo.PointFromScreen(new Point(w32Mouse.X, w32Mouse.Y));
            }
            catch (Exception e)
            { return new Point(); }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);
    }
}