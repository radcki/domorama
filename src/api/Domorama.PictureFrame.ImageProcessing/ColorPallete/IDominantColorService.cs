using System.Globalization;
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
        public required ColorRgb RgbColor { get; init; }
        public required ColorxyY XyYColor { get; init; }
        public required ColorLChab LChabColor { get; init; }

        public Scalar ToBgrScalar() => new(RgbColor.B, RgbColor.G, RgbColor.R);
    }
}

public class ColorLChab
{
    public double L { get; set; }

    /// <summary>
    /// C* (chroma).
    /// Ranges usually from 0 to 100.
    /// </summary>
    public double C { get; set; }

    /// <summary>
    /// h° (hue in degrees).
    /// Ranges usually from 0 to 360.
    /// </summary>
    public double H { get; set; }
    public LChabColor ToLChabColor() => new LChabColor(L, C, H);
}

public class ColorRgb
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public RGBColor ToRGBColor() => RGBColor.FromRGB8Bit(R, G, B);

    /// <inheritdoc />
    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "RGB [R={0:0}, G={1:0}, B={2:0}]", R, G, B);
}

public class ColorxyY
{
    public double X { get; set; }

    /// <summary>
    /// Chromaticity y.
    /// Ranges usually from 0 to 1.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Y channel (luminance).
    /// Ranges usually from 0 to 1.
    /// </summary>
    public double Luminance { get; set; }

    public xyYColor ToXyYColor() => new(X, Y, Luminance);

    /// <inheritdoc />
    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "xyY [x={0:0.##}, y={1:0.##}, Y={2:0.##}]", X, Y, Luminance);
}