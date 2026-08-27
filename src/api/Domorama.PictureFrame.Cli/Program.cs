using System.Collections.Concurrent;
using System.Diagnostics;
using Domorama.PictureFrame.Core.DataSource;
using Domorama.PictureFrame.Core.DataSource.Filesystem;
using Domorama.PictureFrame.Core.Library;
using Domorama.PictureFrame.Core.Library.DataStorage;
using Domorama.PictureFrame.Core.Library.Files;
using Domorama.PictureFrame.Core.Library.Files.Services;
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
                          .AddSingleton<FileGroupingServiceConfiguration>()
                          .AddTransient<FileGroupingService>()
                          .AddTransient(typeof(IDataStorage<>), typeof(JsonDataStorage<>))
                          .AddSingleton<FileLibrary>()
                          .AddLogging()
                          .BuildServiceProvider();

            Console.WriteLine("Initialized");
            var dataSource = services.GetRequiredService<FilesystemPictureDataSource>();
            var metadataExtractor = services.GetRequiredService<MetadataExtractionService>();
            var geoLocationExtractor = services.GetRequiredService<GeoLocationService>();
            var faceDetectionService = services.GetRequiredService<IFaceDetectionService>();
            var dominantColorService = services.GetRequiredService<IDominantColorService>();
            var fileGroupingService = services.GetRequiredService<FileGroupingService>();
            var fileLibrary = services.GetRequiredService<FileLibrary>();

            await fileLibrary.Load();
            fileLibrary.EnableAutosave();
            
            var sw = Stopwatch.StartNew();
            var files = await dataSource.ScanAsync().ToListAsync();
            sw.Stop();
            var changesCheckProgress = new Progress<FileLibraryTaskProgress>();
            changesCheckProgress.ProgressChanged += (sender, progress) => { Console.WriteLine($"Changes check progress: {progress.Done}/{progress.Total} (Elapsed: {progress.Elapsed:g})"); };

            await fileLibrary.FindChanges(files, changesCheckProgress);

            var compatibilityCheckProgress = new Progress<FileLibraryTaskProgress>();
            compatibilityCheckProgress.ProgressChanged += (sender, progress) => { Console.WriteLine($"Compatibility check progress: {progress.Done}/{progress.Total} (Elapsed: {progress.Elapsed:g})"); };

            await fileLibrary.CheckMissingFileCompatibility(compatibilityCheckProgress);

            var groupingProgress = new Progress<FileLibraryTaskProgress>();
            groupingProgress.ProgressChanged += (sender, progress) => { Console.WriteLine($"Grouping progress: {progress.Done}/{progress.Total} (Elapsed: {progress.Elapsed:g})"); };

            await fileLibrary.PerformPendingFileGrouping(groupingProgress);


            var scanTime
                = sw.Elapsed.TotalSeconds;
            Console.WriteLine($"{files.Count} files to index");
            var sw2 = Stopwatch.StartNew();
            ConcurrentBag<FileLibraryEntry> fileInfos = [];
            await Parallel.ForEachAsync(files,
                                        new ParallelOptions() { MaxDegreeOfParallelism = 5 },
                                        async (filesystemRecord, token) =>
                                        {
                                            var info = await dataSource.ReadBasicFileInfo(filesystemRecord);
                                            fileInfos.Add(info);
                                            Console.WriteLine($"{fileInfos.Count} / {files.Count} done");
                                        });


            // foreach (var filesystemRecord in files)
            // {
            //     var info = await dataSource.ReadBaseFileInfo(filesystemRecord);
            //     fileInfos.Add(info);
            //     Console.WriteLine($"{fileInfos.Count} / {files.Count} done");
            // }

            sw2.Stop();
            var infoReadTime = sw2.Elapsed.TotalSeconds;

            // Console.WriteLine($"{fileInfos.Count} files to group");
            // var sw3 = Stopwatch.StartNew();
            // var fileGroups = fileGroupingService.GroupFiles(fileInfos);
            // sw3.Stop();
            // Console.WriteLine($"Found {fileGroups.Count} file groups");
            // var groupingTime = sw3.Elapsed.TotalMilliseconds;

            ConcurrentDictionary<string, FileCompatibilityCheckResult> fileCompatibilityCheckResults = new();
            Console.WriteLine($"{files.Count} files to check for compatibility");
            await Parallel.ForEachAsync(fileInfos,
                                        new ParallelOptions() { MaxDegreeOfParallelism = 5 },
                                        async (basicFileInfo, token) =>
                                        {
                                            if (basicFileInfo.CompatibilityCheckResult != null)
                                            {
                                                fileCompatibilityCheckResults[basicFileInfo.Crc32Checksum] = basicFileInfo.CompatibilityCheckResult;
                                            }
                                            else
                                            {
                                                if (!fileCompatibilityCheckResults.TryGetValue(basicFileInfo.Crc32Checksum, out var fileCompatibility))
                                                {
                                                    fileCompatibility = await dataSource.CheckFileCompatibility(basicFileInfo.FilesystemRecord);
                                                    fileCompatibilityCheckResults[basicFileInfo.Crc32Checksum] = fileCompatibility;
                                                }

                                                basicFileInfo.CompatibilityCheckResult = fileCompatibility;
                                            }

                                            Console.WriteLine($"{fileInfos.Count} / {files.Count} done");
                                        });
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