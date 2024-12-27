using System.Runtime.InteropServices;

namespace Kookiz.ClipPing;

internal static class NativeMethods
{
    public const int MONITOR_DEFAULTTONEAREST = 0x00000002;
    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_TRANSPARENT = 0x00000020;

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    public static extern IntPtr MonitorFromWindow(IntPtr handle, Int32 flags);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, out RECT lpRect, int cPoints);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool IsZoomed(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetMonitorInfo(IntPtr hmonitor, MONITORINFO info);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("Shcore.dll")]
    public static extern int GetDpiForMonitor(
        IntPtr hMonitor,
        MonitorDpiType dpiType,
        out uint dpiX,
        out uint dpiY);

    public enum MonitorDpiType : int
    {
        MDT_Effective_DPI = 0,
        MDT_Angular_DPI = 1,
        MDT_Raw_DPI = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    public class MONITORINFO
    {
        internal int Size = Marshal.SizeOf(typeof(MONITORINFO));
        internal RECT Monitor = new();
        internal RECT Work = new();
        internal int Flags = 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct RECT(int left, int top, int right, int bottom)
    {
        public readonly int Left = left;
        public readonly int Top = top;
        public readonly int Right = right;
        public readonly int Bottom = bottom;
    }
}