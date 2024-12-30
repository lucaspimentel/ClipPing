using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;

namespace Kookiz.ClipPing;

public partial class BorderOverlay : Window, IOverlay
{
    public BorderOverlay()
    {
        InitializeComponent();
    }

    public async Task ShowAsync(Rect area)
    {
        Position = new PixelPoint((int)area.Left, (int)area.Top);
        Width = area.Width;
        Height = Math.Min(area.Height, 500);

        Show();

        var appear = (Animation)Resources["Appear"];
        var disappear = (Animation)Resources["Disappear"];

        await appear.RunAsync(this);
        await disappear.RunAsync(this);

        Close();
    }
}