namespace Domorama.PictureFrame.ImageProcessing.Model;

public record struct RelativePoint(double PercentageX, double PercentageY)
{
    public Pixel ToPixel(PixelSize size) => new((int)(size.Width * PercentageX), (int)(size.Height * PercentageY));

    public static RelativePoint FromPixel(Pixel pixelBoxOrigin, PixelSize fullSize)
    {
        var x = pixelBoxOrigin.X / (double)fullSize.Width;
        var y = pixelBoxOrigin.Y / (double)fullSize.Height;
        return new(x, y);
    }
}