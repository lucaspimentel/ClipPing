using System.Runtime.InteropServices;

namespace Kookiz.ClipPing;

internal static class NativeMethods
{
    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_TRANSPARENT = 0x00000020;
    public const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

    [DllImport("user32.dll")]
    public static extern uint GetDpiForWindow(IntPtr hwnd);

    [DllImport("dwmapi.dll")]
    public static extern int DwmGetWindowAttribute(
        IntPtr hwnd,
        int dwAttribute,
        out RECT pvAttribute,
        int cbAttribute);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct RECT(int left, int top, int right, int bottom)
    {
        public readonly int Left = left;
        public readonly int Top = top;
        public readonly int Right = right;
        public readonly int Bottom = bottom;
    }
}