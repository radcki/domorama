using OpenCvSharp;

namespace Domorama.PictureFrame.ImageProcessing.Model;

public record RelativeBox(RelativePoint Origin, RelativeSize RelativeSize)
{
    public Rect ToRect(PixelSize fullSize) => ToPixelBox(fullSize).ToRect();

    public PixelBox ToPixelBox(PixelSize fullSize)
    {
        var boxOrigin = Origin.ToPixel(fullSize);
        var boxSize = RelativeSize.ToSize(fullSize);
        return new PixelBox(boxOrigin, boxSize);
    }

    public static RelativeBox FromPixelBox(PixelBox pixelBox, PixelSize fullSize)
    {
        var relativeOrigin = RelativePoint.FromPixel(pixelBox.Origin, fullSize);
        var relativeSize = RelativeSize.FromPixelSize(pixelBox.Size, fullSize);
        return new RelativeBox(relativeOrigin, relativeSize);
    }
}