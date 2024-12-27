using System.Windows;
using System.Windows.Media.Animation;

namespace Kookiz.ClipPing.Overlays;

public partial class BorderOverlay : Window, IOverlay
{
    public BorderOverlay()
    {
        InitializeComponent();
    }

    public void Show(Rect area)
    {
        Left = area.Left;
        Top = area.Top;
        Width = area.Width;
        Height = area.Height;

        Show();

        var storyboard = (Storyboard)Resources["ShowStoryboard"];
        storyboard.Begin();
    }

    private void Storyboard_Completed(object sender, EventArgs e) => Hide();
}
