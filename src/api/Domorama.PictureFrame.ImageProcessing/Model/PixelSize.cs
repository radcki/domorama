using OpenCvSharp;

namespace Domorama.PictureFrame.ImageProcessing.Model;

public record struct PixelSize(int Width, int Height)
{
    public static PixelSize FromSize(Size size)
    {
        return new PixelSize(Width: size.Width, Height: size.Height);
    }
}