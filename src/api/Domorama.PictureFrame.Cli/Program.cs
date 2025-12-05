using System.Collections.Concurrent;
using System.Diagnostics;
using Domorama.PictureFrame.Core.DataSource;
using Domorama.PictureFrame.Core.DataSource.Filesystem;
using Domorama.PictureFrame.Core.Model;
using Microsoft.Extensions.DependencyInjection;
using OpenCvSharp;
using Domorama.PictureFrame.ImageProcessing.ColorPallete;
using Domorama.PictureFrame.ImageProcessing.FaceDetection;
using Domorama.PictureFrame.ImageProcessing.FaceDetection.HaarCascade;
using Domorama.PictureFrame.ImageProcessing.FaceDetection.ONNX;
using Domorama.PictureFrame.ImageProcessing.Geocoding;
using Domorama.PictureFrame.ImageProcessing.Geocoding.Nominatim;
using Domorama.PictureFrame.ImageProcessing.Metadata;
using Domorama.PictureFrame.ImageProcessing.Utils;

namespace Domorama.PictureFrame.Cli
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection()
                          .AddHttpClient()
                          .AddTransient<MetadataExtractionService>()
                          .AddSingleton<GeoLocationService>()
                          .AddSingleton<NominatimApiClient>()
                          .AddTransient<IDominantColorService, DominantColorService>()
                          .AddSingleton<IFaceDetectionService, FaceOnnxDetectionService>()
                           // .AddSingleton<IFaceDetectionService, CascadeFaceDetectionService>()
                          .AddSingleton<FilesystemPictureDataSourceConfiguration>(x => new FilesystemPictureDataSourceConfiguration()
                                                                                       {
                                                                                           Directories =
                                                                                           [
                                                                                               @"E:\2025\2025-08-09"
                                                                                           ]
                                                                                       })
                          .AddTransient<FilesystemPictureDataSource>()
                          .BuildServiceProvider();

            Console.WriteLine("Initialized");
            var dataSource = services.GetRequiredService<FilesystemPictureDataSource>();
            var metadataExtractor = services.GetRequiredService<MetadataExtractionService>();
            var geoLocationExtractor = services.GetRequiredService<GeoLocationService>();
            var faceDetectionService = services.GetRequiredService<IFaceDetectionService>();
            var dominantColorService = services.GetRequiredService<IDominantColorService>();

            var sw = Stopwatch.StartNew();
            var files = await dataSource.ScanAsync().ToListAsync();
            sw.Stop();
            var scanTime
                = sw.Elapsed.TotalSeconds;
            Console.WriteLine($"{files.Count} files to check");
            var sw2 = Stopwatch.StartNew();
            ConcurrentBag<BaseFileInfo> fileInfos = [];
            foreach (var filesystemRecord in files)
            {
                var info = await dataSource.ReadBaseFileInfo(filesystemRecord);
                fileInfos.Add(info);
                Console.WriteLine($"{fileInfos.Count} / {files.Count} done");
            }

            sw2.Stop();
            var infoReadTime = sw2.Elapsed.TotalSeconds;

            //
            // foreach (var metadata in pictures)
            // {
            //     var captureData = metadataExtractor.ExtractCaptureDateFromFile(metadata.Path);
            //     var geolocation = metadataExtractor.ExtractGeolocationFromFile(metadata.Path);
            //     if (geolocation != null)
            //     {
            //         var address = await geoLocationExtractor.GetAddressForLocation(geolocation.Value);
            //     }
            //     var imgMat = Cv2.ImRead(metadata.Path, ImreadModes.Unchanged);
            //     MatUtils.ResizeToCover(imgMat, new Size(800, 800));
            //     var faces = faceDetectionService.DetectFaces(imgMat).ToList();
            //     foreach (var detectedFace in faces)
            //     {
            //         Cv2.Rectangle(imgMat, detectedFace.FaceArea, Scalar.Red, 2);
            //     }
            //     var dominantColors = dominantColorService.GetDominantColorsFromMat(imgMat);
            //
            //     var colorTileSize = new Size(imgMat.Width / dominantColors.Count, 100);
            //     for (var i = 0; i < dominantColors.Count; i++)
            //     {
            //         var color = dominantColors[i];
            //         var tileEndY = imgMat.Rows;
            //         var tileStartY = tileEndY - colorTileSize.Height;
            //         var tileStartX = i * colorTileSize.Width;
            //         var tileEndX = Math.Min(imgMat.Cols, tileStartX + colorTileSize.Width);
            //         imgMat[tileStartY, tileEndY, tileStartX, tileEndX].SetTo(color.ToBgrScalar());
            //     }
            //
            //     Cv2.ImShow("Preview", imgMat);
            //     Cv2.WaitKey();
            // }

            // Console.WriteLine(geolocation);
        }
    }
}