using System.Diagnostics;
using Domorama.PictureFrame.Core.DataSource;
using Domorama.PictureFrame.Core.DataSource.Filesystem;
using Domorama.PictureFrame.Core.Library.DataStorage;
using Domorama.PictureFrame.Core.Library.Files;
using Domorama.PictureFrame.Core.Model;
using Domorama.PictureFrame.ImageProcessing.ColorPallete;
using Domorama.PictureFrame.ImageProcessing.FaceDetection;
using Domorama.PictureFrame.ImageProcessing.Geocoding;
using Domorama.PictureFrame.ImageProcessing.Geocoding.Model;
using Domorama.PictureFrame.ImageProcessing.Metadata;
using Microsoft.Extensions.Logging;
using OpenCvSharp;

namespace Domorama.PictureFrame.Core.Library.PictureFrameEntries;

public class PictureFrameEntryLibrary(
    FileLibrary fileLibrary,
    IDataStorage<PictureFrameEntryLibraryData> storage,
    ILogger<PictureFrameEntryLibrary> logger,
    MetadataExtractionService metadataExtractionService,
    IDominantColorService dominantColorService,
    IFaceDetectionService faceDetectionService,
    IPictureDataSource filesystemPictureDataSource,
    GeoLocationService geoLocationService
)
{
    private readonly Dictionary<Guid, PictureFrameEntry> _pictureFrameEntries = [];


    private bool _isLoaded;
    private DateTime SavedAt { get; set; }
    private DateTime LastModificationAt { get; set; }
    private bool HasPendingChanges() => LastModificationAt > SavedAt;

    public async Task LoadAsync()
    {
        if (_isLoaded)
        {
            return;
        }

        var data = await storage.Load();
        foreach (var pictureFrameEntry in data.PictureFrameEntries)
        {
            _pictureFrameEntries.Add(pictureFrameEntry.FileLibraryEntryCollectionId, pictureFrameEntry);
        }

        var fileLibraryPictureFileCollections = await fileLibrary.GetPictureFileCollectionsAsync();
        foreach (var pictureFileCollection in fileLibraryPictureFileCollections)
        {
            if (!_pictureFrameEntries.ContainsKey(pictureFileCollection.FileLibraryEntryCollectionId))
            {
                var pictureFrameEntry = new PictureFrameEntry() { FileLibraryEntryCollectionId = pictureFileCollection.FileLibraryEntryCollectionId };
                _pictureFrameEntries.Add(pictureFrameEntry.FileLibraryEntryCollectionId, pictureFrameEntry);

                NotifyLibraryChanged();
            }
        }

        _isLoaded = true;
    }

    public void EnableAutosave()
    {
        _ = ChangesSavingTask();
    }

    public IEnumerable<PictureFrameEntry> GetEntries() => _pictureFrameEntries.Values;

    public async Task ChangesSavingTask()
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (await timer.WaitForNextTickAsync())
        {
            if (HasPendingChanges())
            {
                logger.LogInformation($"Saving library changes...");
                var data = new PictureFrameEntryLibraryData();
                data.PictureFrameEntries.AddRange(_pictureFrameEntries.Values);

                await storage.Save(data);
                NotifyLibrarySaved();
                logger.LogInformation($"Library saved");
            }
            else
            {
                logger.LogDebug("No changes to save.");
            }
        }
    }


    public async Task PerformCaptureDateExtraction(IProgress<PictureLibraryTaskProgress> progress, CancellationToken cancellationToken = default)
    {
        await IterateFileLibraryEntries(entry => !entry.PictureInfo.IsCaptureDateExtractionDone, progress, ExecuteExtraction, cancellationToken);

        ValueTask ExecuteExtraction(PictureFrameEntry pictureFrameEntry, FileLibraryEntryCollection collection)
        {
            foreach (var fileLibraryEntry in collection.Items)
            {
                var path = fileLibraryEntry.FilesystemRecord.GetFullPath();
                var captureDate = metadataExtractionService.ExtractCaptureDateFromFile(path);
                if (captureDate != null)
                {
                    pictureFrameEntry.PictureInfo.CaptureDate = captureDate;
                    pictureFrameEntry.PictureInfo.IsCaptureDateExtractionDone = true;
                    NotifyLibraryChanged();
                    break;
                }
            }

            return ValueTask.CompletedTask;
        }
    }

    public async Task PerformGeolocationExtraction(IProgress<PictureLibraryTaskProgress> progress, CancellationToken cancellationToken = default)
    {
        await IterateFileLibraryEntries(entry => !entry.PictureInfo.IsGeolocationExtractionDone, progress, ExecuteExtraction, cancellationToken);

        async ValueTask ExecuteExtraction(PictureFrameEntry pictureFrameEntry, FileLibraryEntryCollection collection)
        {
            foreach (var fileLibraryEntry in collection.Items)
            {
                var path = fileLibraryEntry.FilesystemRecord.GetFullPath();
                var geolocation = metadataExtractionService.ExtractGeolocationFromFile(path);
                if (geolocation != null)
                {
                    var coordinate = new GeoCoordinates(geolocation.Value.Latitude, geolocation.Value.Longitude);
                    var address = await geoLocationService.GetAddressForLocation(coordinate);
                    pictureFrameEntry.PictureInfo.GeoLocation = new GeoLocation(coordinate, address);
                    break;
                }
            }

            pictureFrameEntry.PictureInfo.IsGeolocationExtractionDone = true;
            NotifyLibraryChanged();
        }
    }


    public async Task PerformDominantColorsExtraction(IProgress<PictureLibraryTaskProgress> progress, CancellationToken cancellationToken = default)
    {
        await IterateImages(entry => !entry.PictureInfo.IsDominantColorExtractionDone, progress, ExecuteProcessing, cancellationToken);

        ValueTask ExecuteProcessing(PictureFrameEntry pictureFrameEntry, Mat image)
        {
            var dominantColorsFromMat = dominantColorService.GetDominantColorsFromMat(image);

            pictureFrameEntry.PictureInfo.DominantColors.Clear();
            pictureFrameEntry.PictureInfo.DominantColors.AddRange(dominantColorsFromMat);
            pictureFrameEntry.PictureInfo.IsDominantColorExtractionDone = true;
            NotifyLibraryChanged();
            return ValueTask.CompletedTask;
        }
    }

    public async Task PerformFaceDetection(IProgress<PictureLibraryTaskProgress> progress, CancellationToken cancellationToken = default)
    {
        await IterateImages(entry => !entry.PictureInfo.IsFaceDetectionDone, progress, ExecuteProcessing, cancellationToken);

        ValueTask ExecuteProcessing(PictureFrameEntry pictureFrameEntry, Mat image)
        {
            var dominantColorsFromMat = faceDetectionService.DetectFaces(image);

            pictureFrameEntry.PictureInfo.DetectedFaces.Clear();
            pictureFrameEntry.PictureInfo.DetectedFaces.AddRange(dominantColorsFromMat);
            pictureFrameEntry.PictureInfo.IsFaceDetectionDone = true;
            NotifyLibraryChanged();
            return ValueTask.CompletedTask;
        }
    }

    private async Task IterateFileLibraryEntries(Func<PictureFrameEntry, bool> selector,
                                                 IProgress<PictureLibraryTaskProgress> progress,
                                                 Func<PictureFrameEntry, FileLibraryEntryCollection, ValueTask> action,
                                                 CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var toProcess = _pictureFrameEntries.Values
                                            .Where(selector)
                                            .ToList();
        var todo = toProcess.Count;
        var done = 0;

        progress.Report(new(done, todo, stopwatch.Elapsed));

        async Task ProcessFrameEntry(PictureFrameEntry pictureFrameEntry, CancellationToken processingCancellationToken = default)
        {
            try
            {
                var collection = await fileLibrary.TryGetPictureFileCollectionsAsync(pictureFrameEntry.FileLibraryEntryCollectionId);
                if (collection == null)
                {
                    logger.LogError("Could not load file collection {FileLibraryEntryCollectionId}", pictureFrameEntry.FileLibraryEntryCollectionId);
                    return;
                }

                if (processingCancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await action(pictureFrameEntry, collection);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Processing failure");
            }

            done++;
            progress.Report(new(done, todo, stopwatch.Elapsed));
        }

        var processingOptions = new ParallelOptions()
                                {
                                    MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1),
                                    CancellationToken = cancellationToken
                                };

        await Parallel.ForEachAsync(toProcess,
                                    processingOptions,
                                    async (pictureFrameEntry, token) => await ProcessFrameEntry(pictureFrameEntry, token));
    }

    private async Task IterateImages(Func<PictureFrameEntry, bool> selector,
                                     IProgress<PictureLibraryTaskProgress> progress,
                                     Func<PictureFrameEntry, Mat, ValueTask> action,
                                     CancellationToken cancellationToken = default)
    {
        await IterateFileLibraryEntries(selector,
                                        progress,
                                        async (entry, collection) =>
                                        {
                                            foreach (var fileLibraryEntry in collection.Items.OrderBy(x => x.FilesystemRecord.ByteCount))
                                            {
                                                //todo: prefer small versions
                                                using var mat = await filesystemPictureDataSource.LoadImage(fileLibraryEntry);
                                                if (mat == null)
                                                {
                                                    continue;
                                                }

                                                if (cancellationToken.IsCancellationRequested)
                                                {
                                                    return;
                                                }

                                                await action(entry, mat);
                                                break;
                                            }
                                        },
                                        cancellationToken);
    }

    private void NotifyLibraryChanged()
    {
        LastModificationAt = DateTime.UtcNow;
    }

    private void NotifyLibrarySaved()
    {
        SavedAt = DateTime.UtcNow;
    }
}

public record PictureLibraryTaskProgress(int Done, int Total, TimeSpan Elapsed);