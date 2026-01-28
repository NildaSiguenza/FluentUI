using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
    public static class NativeMethods
    {
        public const int WM_SETREDRAW = 0x000B;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
