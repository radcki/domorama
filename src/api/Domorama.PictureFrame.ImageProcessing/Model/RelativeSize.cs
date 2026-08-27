namespace Domorama.PictureFrame.ImageProcessing.Model;

public record struct RelativeSize(double PercentageWidth, double PercentageHeight)
{
    public PixelSize ToSize(PixelSize size) => new((int)(size.Width * PercentageWidth), (int)(size.Height * PercentageHeight));

    public static RelativeSize FromPixelSize(PixelSize pixelBoxSize, PixelSize fullSize)
    {
        var width = pixelBoxSize.Width / (double)fullSize.Width;
        var height = pixelBoxSize.Height / (double)fullSize.Height;
        return new RelativeSize(width, height);
    }
}