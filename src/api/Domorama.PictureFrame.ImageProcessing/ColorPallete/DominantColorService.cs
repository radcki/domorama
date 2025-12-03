using Colourful;
using OpenCvSharp;
using Size = OpenCvSharp.Size;

namespace Domorama.PictureFrame.ImageProcessing.ColorPallete
{
    public class DominantColorService : IDominantColorService
    {
        private readonly IColorConverter<RGBColor, xyYColor> _rgbToXyYColorConverter = new ConverterBuilder()
                                                                                      .FromRGB(RGBWorkingSpaces.sRGB)
                                                                                      .ToxyY(RGBWorkingSpaces.sRGB.WhitePoint)
                                                                                      .Build();

        readonly IColorConverter<RGBColor, LChabColor> _rgbToLChabColorConverter = new ConverterBuilder()
                                                                                  .FromRGB(RGBWorkingSpaces.sRGB)
                                                                                  .ToLChab()
                                                                                  .Build();

        public List<Color> GetDominantColorsFromMat(Mat source)
        {
            using var labels = new Mat();
            using var centers = new Mat();
            var data = new Mat();
            var k = 7;

            Cv2.Resize(source, data, new Size(320, 320), interpolation: InterpolationFlags.Nearest);
            data.ConvertTo(data, MatType.CV_32FC3);
            data = data.Reshape(1, (int)data.Total());

            Cv2.Kmeans(data, k, labels, new TermCriteria(CriteriaTypes.MaxIter, 1, 1.0), 3, KMeansFlags.PpCenters, centers);

            var center = centers.Reshape(3, centers.Rows);

            center.ConvertTo(center, MatType.CV_8U);
            labels.ConvertTo(labels, MatType.CV_8U);
            labels.GetArray(out byte[] labelsArray);

            var labelCounts = labelsArray.GroupBy(x => x).ToDictionary(x => (int)x.Key, x => x.Count());

            var colors = Enumerable.Range(0, k)
                                   .OrderByDescending(x => labelCounts[x])
                                   .Select(i =>
                                           {
                                               var r = center.ExtractChannel(2).Get<byte>(i);
                                               var g = center.ExtractChannel(1).Get<byte>(i);
                                               var b = center.ExtractChannel(0).Get<byte>(i);
                                               var rgbColor = RGBColor.FromRGB8Bit(r, g, b);
                                               return new Color()
                                                      {
                                                          RgbColor = rgbColor,
                                                          XyYColor = _rgbToXyYColorConverter.Convert(rgbColor),
                                                          LChabColor = _rgbToLChabColorConverter.Convert(rgbColor)
                                                      };
                                           })
                                   .Take(5)
                                   .ToList();

            return colors;
        }
    }
}