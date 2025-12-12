using OpenCvSharp;

namespace Domorama.PictureFrame.ImageProcessing.Model;

public record struct Pixel(int X, int Y)
{
    public static Pixel FromPoint(Point point) => new(point.X, point.Y);
}