using Colourful;
using OpenCvSharp;

namespace Domorama.PictureFrame.ImageProcessing.ColorPallete
{
    public interface IDominantColorService
    {
        List<Color> GetDominantColorsFromMat(Mat source);
    }

    public class Color
    {
        public RGBColor RgbColor { get; init; }
        public xyYColor XyYColor { get; init; }
        public LChabColor LChabColor { get; init; }

        public Scalar ToBgrScalar() => new(255 * RgbColor.B, 255 * RgbColor.G, 255 * RgbColor.R);
    }
}