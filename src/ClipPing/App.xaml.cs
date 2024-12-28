using Hardcodet.Wpf.TaskbarNotification;
using Kookiz.ClipPing;
using Kookiz.ClipPing.Overlays;
using System.Runtime.InteropServices;
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

        // Get bounding rectangle in device coordinates
        var hr = NativeMethods.DwmGetWindowAttribute(
            hwnd,
            NativeMethods.DWMWA_EXTENDED_FRAME_BOUNDS,
            out NativeMethods.RECT rect,
            Marshal.SizeOf(typeof(NativeMethods.RECT)));

        if (hr != 0)
        {
            return;
        }

        int windowWidthPx = rect.Right - rect.Left;
        int windowHeightPx = rect.Bottom - rect.Top;

        if (windowWidthPx <= 0 || windowHeightPx <= 0)
        {
            return;
        }

        // Get DPI of the window
        var dpi = NativeMethods.GetDpiForWindow(hwnd);

        // Convert device pixels -> WPF device-independent pixels (DIPs)
        // 1 DIP = 1 px at 96 DPI. So the scale factor is (96 / actualDPI)
        double scale = 96.0 / dpi;

        double leftDIPs = rect.Left * scale;
        double topDIPs = rect.Top * scale;
        double widthDIPs = windowWidthPx * scale;
        double heightDIPs = windowHeightPx * scale;

        // Show the overlay window
        _overlay?.Show(new(leftDIPs, topDIPs, widthDIPs, heightDIPs));
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Shutdown();
}
