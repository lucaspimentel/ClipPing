using Avalonia;

namespace Kookiz.ClipPing;
internal interface IOverlay
{
    Task ShowAsync(Rect area);
}
