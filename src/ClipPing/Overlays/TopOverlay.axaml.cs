using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;

namespace Kookiz.ClipPing;

public partial class TopOverlay : Window, IOverlay
{
    public TopOverlay()
    {
        InitializeComponent();
    }

    public async Task ShowAsync(Rect area)
    {
        Position = new PixelPoint((int)area.Left, (int)area.Top);
        Width = area.Width;
        Height = area.Height;

        Show();

        var appear = (Animation)Resources["Appear"];
        var disappear = (Animation)Resources["Disappear"];

        await appear.RunAsync(this);
        await disappear.RunAsync(this);

        Close();
    }
}