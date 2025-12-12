using OpenCvSharp;

namespace Domorama.PictureFrame.ImageProcessing.Model;

public record PixelBox(Pixel Origin, PixelSize Size)
{
    public Rect ToRect() => new(Origin.X, Origin.Y, Size.Width, Size.Height);
    public static PixelBox FromRect(Rect rect) => new(Pixel.FromPoint(rect.TopLeft), new PixelSize(rect.Width, rect.Height));
}