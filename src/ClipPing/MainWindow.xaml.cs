using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace Kookiz.ClipPing;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        ComponentDispatcher.ThreadFilterMessage += ComponentDispatcher_ThreadFilterMessage;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var handle = new WindowInteropHelper(this).Handle;
        var extendedStyle = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_EXSTYLE);

        // Add WS_EX_TRANSPARENT style to allow clicks to pass through
        _ = NativeMethods.SetWindowLong(
            handle,
            NativeMethods.GWL_EXSTYLE,
            extendedStyle | NativeMethods.WS_EX_TRANSPARENT);
    }

    private void ShowOnActiveWindow()
    {
        var hwnd = NativeMethods.GetForegroundWindow();

        if (hwnd == IntPtr.Zero)
        {
            // No active window
            return;
        }

        // 2) Get bounding rectangle in *device* coordinates
        if (!NativeMethods.GetWindowRect(hwnd, out var rect))
        {
            return;
        }

        int windowWidthPx = rect.Right - rect.Left;
        int windowHeightPx = rect.Bottom - rect.Top;

        if (windowWidthPx <= 0 || windowHeightPx <= 0)
        {
            return;
        }

        // 2) Determine which monitor this window is on
        var monitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);

        // 3) Convert device coordinates => WPF coordinates (DIPs)
        int hr = NativeMethods.GetDpiForMonitor(
            monitor,
            NativeMethods.MonitorDpiType.MDT_Effective_DPI,
            out var dpiX,
            out var dpiY);

        if (hr != 0)
        {
            // Fallback if GetDpiForMonitor failed. Assume 96 DPI (100% scaling)
            dpiX = 96;
            dpiY = 96;
        }

        // Convert device pixels -> WPF device-independent pixels (DIPs)
        // 1 DIP = 1 px at 96 DPI. So the scale factor is (96 / actualDPI)
        double scaleX = 96.0 / dpiX;
        double scaleY = 96.0 / dpiY;

        double leftDIPs = rect.Left * scaleX;
        double topDIPs = rect.Top * scaleY;
        double widthDIPs = windowWidthPx * scaleX;
        double heightDIPs = windowHeightPx * scaleY;

        // 4) Position window
        Width = widthDIPs;
        Height = heightDIPs;

        Top = topDIPs;
        Left = leftDIPs;

        // 5) Show the overlay window
        DoShow();
    }

    private void DoShow()
    {
        Show();

        var storyboard = (Storyboard)Resources["ShowStoryboard"];
        storyboard.Begin();
    }

    private void ComponentDispatcher_ThreadFilterMessage(ref MSG msg, ref bool handled)
    {
        if (msg.message == 0x031D /* WM_CLIPBOARDUPDATE */)
        {
            ShowOnActiveWindow();
        }
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Hide();

        var result = NativeMethods.AddClipboardFormatListener(new WindowInteropHelper(this).Handle);

        if (!result)
        {
            MessageBox.Show($"Failed to add clipboard listener: {result:x2}. Exiting.");
            Close();
        }
    }

    private void Storyboard_Completed(object sender, EventArgs e) => Hide();

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();
}