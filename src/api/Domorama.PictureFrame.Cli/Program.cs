using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenCvSharp;
using Domorama.PictureFrame.ImageProcessing.ColorPallete;
using Domorama.PictureFrame.ImageProcessing.Geocoding;
using Domorama.PictureFrame.ImageProcessing.Geocoding.Nominatim;
using Domorama.PictureFrame.ImageProcessing.Utils;

namespace Domorama.PictureFrame.Cli
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection()
                          .AddHttpClient()
                          .AddSingleton<GeoLocationExtractor>()
                          .AddSingleton<NominatimApiClient>()
                          .AddTransient<IDominantColorService, DominantColorService>()
                          .BuildServiceProvider();

            Console.WriteLine("Hello, World!");
            var geoLocationExtractor = services.GetRequiredService<GeoLocationExtractor>();
            var dominantColorService = services.GetRequiredService<IDominantColorService>();
            var geoFilePath = @"E:\2025\2025-08-09\DSCF4178.JPG";

            var geolocation = geoLocationExtractor.GetGeolocationFromFile(geoFilePath);
            if (geolocation != null)
            {
                //var address = await geoLocationExtractor.GetAddressForLocation(geolocation.Value);
            }

            var files = Directory.GetFiles(@"E:\2025\2025-08-09\", "*.jpg").OrderBy(x => Guid.NewGuid()).ToList();
            
            foreach (var path in files)
            {
                var imgMat = Cv2.ImRead(path, ImreadModes.Unchanged);
                MatUtils.ResizeToCover(imgMat, new Size(500, 500));

                var dominantColors = dominantColorService.GetDominantColorsFromMat(imgMat);
                var colorTileSize = new Size(imgMat.Width / dominantColors.Count, 100);
                for (var i = 0; i < dominantColors.Count; i++)
                {
                    var color = dominantColors[i];
                    var tileEndY = imgMat.Rows;
                    var tileStartY = tileEndY - colorTileSize.Height;
                    var tileStartX = i * colorTileSize.Width;
                    var tileEndX = Math.Min(imgMat.Cols, tileStartX + colorTileSize.Width);
                    imgMat[tileStartY, tileEndY, tileStartX, tileEndX].SetTo(color.ToBgrScalar());
                }

                Cv2.ImShow("Preview", imgMat);
                Cv2.WaitKey();
            }

            // Console.WriteLine(geolocation);
        }
    }
}