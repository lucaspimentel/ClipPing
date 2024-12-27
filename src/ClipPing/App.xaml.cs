using Hardcodet.Wpf.TaskbarNotification;
using Kookiz.ClipPing;
using Kookiz.ClipPing.Overlays;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ClipPing;

public partial class App : Application
{
    private TaskbarIcon? _taskbarIcon;
    private IOverlay? _overlay;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _taskbarIcon = new TaskbarIcon()
        {
            Visibility = Visibility.Visible,
            ToolTipText = "Clipboard notification",
            IconSource = BitmapFrame.Create(new Uri("pack://application:,,,/clipboard.ico", UriKind.Absolute)),
            ContextMenu = (ContextMenu)FindResource("TaskbarIconContextMenu")
        };

        _overlay = LoadOverlay();

        var interopHelper = new WindowInteropHelper(MainWindow);

        MainWindow.SourceInitialized += (s, e) =>
        {
            var handle = interopHelper.Handle;
            var result = NativeMethods.AddClipboardFormatListener(handle);

            if (!result)
            {
                MessageBox.Show($"Failed to add clipboard listener: {result:x2}. Exiting.");
                Shutdown();
            }
        };

        interopHelper.EnsureHandle();

        ComponentDispatcher.ThreadFilterMessage += ComponentDispatcher_ThreadFilterMessage;
    }

    private IOverlay LoadOverlay()
    {
        // TODO: Add a way to pick what overlay to use
        var overlay = new TopOverlay
        {
            ShowInTaskbar = false,
            ShowActivated = false,
            Topmost = true
        };

        overlay.SourceInitialized += (s, e) =>
        {
            var handle = new WindowInteropHelper(overlay).Handle;
            var extendedStyle = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_EXSTYLE);

            // Add WS_EX_TRANSPARENT style to allow clicks to pass through
            _ = NativeMethods.SetWindowLong(
                handle,
                NativeMethods.GWL_EXSTYLE,
                extendedStyle | NativeMethods.WS_EX_TRANSPARENT);
        };

        return overlay;
    }

    private void ComponentDispatcher_ThreadFilterMessage(ref MSG msg, ref bool handled)
    {
        if (msg.message == 0x031D /* WM_CLIPBOARDUPDATE */)
        {
            ShowOverlay();
        }
    }

    private void ShowOverlay()
    {
        var hwnd = NativeMethods.GetForegroundWindow();

        if (hwnd == IntPtr.Zero)
        {
            // No active window
            return;
        }

        // 2) Get bounding rectangle in device coordinates
        NativeMethods.RECT rect;

        if (NativeMethods.IsZoomed(hwnd))
        {
            // If the window is maximized, the title bar is offscreen.
            // Use GetClientRect to get the client area.
            if (!NativeMethods.GetClientRect(hwnd, out rect))
            {
                return;
            }

            // Client area is relative to the window, convert to absolute coordinates
            if (!NativeMethods.MapWindowPoints(hwnd, IntPtr.Zero, out rect, 2))
            {
                return;
            }
        }
        else
        {
            // If the window isn't maximize, use GetWindowRect to get the full window area
            if (!NativeMethods.GetWindowRect(hwnd, out rect))
            {
                return;
            }
        }

        int windowWidthPx = rect.Right - rect.Left;
        int windowHeightPx = rect.Bottom - rect.Top;

        if (windowWidthPx <= 0 || windowHeightPx <= 0)
        {
            return;
        }

        // 2) Determine which monitor this window is on
        var monitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);

        var monitorInfo = new NativeMethods.MONITORINFO();
        NativeMethods.GetMonitorInfo(monitor, monitorInfo);

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

        var left = rect.Left;
        var top = rect.Top;

        if (left < monitorInfo.Work.Left)
        {
            left = monitorInfo.Work.Left;
        }

        if (top < monitorInfo.Work.Top)
        {
            top = monitorInfo.Work.Top;
        }

        double leftDIPs = rect.Left * scaleX;
        double topDIPs = rect.Top * scaleY;
        double widthDIPs = windowWidthPx * scaleX;
        double heightDIPs = windowHeightPx * scaleY;

        // 4) Show the overlay window
        _overlay?.Show(new(leftDIPs, topDIPs, widthDIPs, heightDIPs));
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Shutdown();
}
