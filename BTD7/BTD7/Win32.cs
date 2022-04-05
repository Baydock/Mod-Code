using System;
using System.Runtime.InteropServices;
using System.Text;

namespace BTD7 {
    internal static class Win32 {

        internal delegate bool EnumThreadWndProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern bool EnumThreadWindows(int dwThreadId, EnumThreadWndProc lpfn, IntPtr lParam);

        internal const int GWL_STYLE = -16;
        internal const int GWL_EXSTYLE = -20;

        [DllImport("user32.dll")]
        internal static extern int SetWindowLongPtr(IntPtr hWnd, int nIndex, long dwNewLong);

        [DllImport("user32.dll")]
        internal static extern long GetWindowLongPtr(IntPtr hWnd, int nIndex);

        internal const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        internal const int SW_MAXIMIZE = 3;

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        internal static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        internal static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowRect(IntPtr hwnd, out Rect lpRect);

        [DllImport("user32.dll")]
        internal static extern bool GetClientRect(IntPtr hwnd, out Rect lpRect);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetDesktopWindow();
    }
}
