using System.Diagnostics;
using Domorama.PictureFrame.Core.DataSource.Filesystem;
using Domorama.PictureFrame.Core.Library.DataStorage;
using Domorama.PictureFrame.Core.Library.Files.Services;
using Domorama.PictureFrame.Core.Model;
using Microsoft.Extensions.Logging;

namespace Domorama.PictureFrame.Core.Library.Files;

public class FileLibrary(FileGroupingService fileGroupingService, FilesystemPictureDataSource filesystemPictureDataSource, IDataStorage<FileLibraryData> storage, ILogger<FileLibrary> logger)
{
    private readonly Dictionary<Guid, FileLibraryEntry> _knownFiles = [];
    private readonly Dictionary<Guid, FileLibraryEntryCollection> _pictureFileCollections = [];
    private readonly Dictionary<Guid, FileLibraryEntryCollection> _fileCollectionsIndex = [];
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
        foreach (var fileInfo in data.KnownFiles)
        {
            _knownFiles.Add(fileInfo.FileId, fileInfo);
        }

        foreach (var dataPictureFileInfoCollection in data.PictureFileInfoCollections)
        {
            _pictureFileCollections.Add(dataPictureFileInfoCollection.FileLibraryEntryCollectionId, dataPictureFileInfoCollection);
            foreach (var basicFileInfo in dataPictureFileInfoCollection.Items)
            {
                _fileCollectionsIndex[basicFileInfo.FileId] = dataPictureFileInfoCollection;
            }
        }

        _isLoaded = true;
    }

    private async Task EnsureLoaded()
    {
        if (!_isLoaded)
        {
            await LoadAsync();
        }
    }

    public async Task<IEnumerable<FileLibraryEntryCollection>> GetPictureFileCollectionsAsync()
    {
        await EnsureLoaded();
        return _pictureFileCollections.Values;
    }
    
    public async Task<FileLibraryEntry?> TryGetFileByIdAsync(Guid fileId)
    {
        await EnsureLoaded();
        _knownFiles.TryGetValue(fileId, out FileLibraryEntry? file);
        return file;
    }
    public async Task<FileLibraryEntryCollection?> TryGetPictureFileCollectionsAsync(Guid fileLibraryEntryCollectionId)
    {
        await EnsureLoaded();
        _pictureFileCollections.TryGetValue(fileLibraryEntryCollectionId, out var fileLibraryEntryCollection);
        return fileLibraryEntryCollection;
    }

    public void EnableAutosave()
    {
        _ = ChangesSavingTask();
    }

    public async Task ChangesSavingTask()
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (await timer.WaitForNextTickAsync())
        {
            if (HasPendingChanges())
            {
                logger.LogInformation($"Saving library changes...");
                var data = new FileLibraryData();
                data.KnownFiles.AddRange(_knownFiles.Values);
                data.PictureFileInfoCollections.AddRange(_pictureFileCollections.Values);
                await storage.Save(data);
                NotifyLibrarySaved();
                logger.LogInformation($"Library saved");
            }
        }
    }

    public async Task FindChanges(List<FilesystemRecord> filesystemRecords, IProgress<FileLibraryTaskProgress> progress)
    {
        var stopwatch = Stopwatch.StartNew();

        var knownFilesToRemove = _knownFiles.Values.Where(knownFile => filesystemRecords.All(newFile => newFile.GetFullPath() != knownFile.FilesystemRecord.GetFullPath())).ToList();
        foreach (var fileToRemove in knownFilesToRemove)
        {
            RemoveFile(fileToRemove);
        }

        var changedFiles = _knownFiles.Values.Where(knownFile => filesystemRecords.Any(newFile => knownFile.FilesystemRecord.GetFullPath() == newFile.GetFullPath()
                                                                                                  && knownFile.FilesystemRecord.ByteCount != newFile.ByteCount));
        foreach (var outdatedFile in changedFiles)
        {
            RemoveFile(outdatedFile);
        }

        var newFiles = filesystemRecords.Where(newFile => _knownFiles.Values.All(knownFile => knownFile.FilesystemRecord.GetFullPath() != newFile.GetFullPath())).ToList();
        var todo = newFiles.Count;
        var done = 0;

        async ValueTask ReadBasicFileInfo(FilesystemRecord newFile, CancellationToken token)
        {
            try
            {
                var newFileInfo = await filesystemPictureDataSource.ReadBasicFileInfo(newFile);
                _knownFiles.Add(newFileInfo.FileId, newFileInfo);
                NotifyLibraryChanged();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to read basic file info. Path: {FullPath}", newFile.GetFullPath());
            }

            done++;
            progress.Report(new(done, todo, stopwatch.Elapsed));
        }

        await Parallel.ForEachAsync(newFiles, FileProcessingParallelOptions(), ReadBasicFileInfo);
        //
        // foreach (var newFile in newFiles)
        // {
        //     await ReadBasicFileInfo(newFile, CancellationToken.None);
        // }
    }

    public async Task CheckMissingFileCompatibility(IProgress<FileLibraryTaskProgress> progress)
    {
        var stopwatch = Stopwatch.StartNew();
        var filesWithUnknownCompatibility = _knownFiles.Values.Where(x => !x.CompatibilityCheckDone).ToList();
        var todo = filesWithUnknownCompatibility.Count;
        var done = 0;

        progress.Report(new(done, todo, stopwatch.Elapsed));

        async ValueTask CheckCompatibility(FileLibraryEntry fileLibraryEntry, CancellationToken token)
        {
            try
            {
                fileLibraryEntry.CompatibilityCheckResult = await filesystemPictureDataSource.CheckFileCompatibility(fileLibraryEntry.FilesystemRecord);
                fileLibraryEntry.CompatibilityCheckDone = true;
                NotifyLibraryChanged();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to check file compatibility. Path: {FullPath}", fileLibraryEntry.FilesystemRecord.GetFullPath());
            }

            done++;
            progress.Report(new(done, todo, stopwatch.Elapsed));
        }

        await Parallel.ForEachAsync(filesWithUnknownCompatibility, FileProcessingParallelOptions(), CheckCompatibility);

        // foreach (var fileInfo in filesWithUnknownCompatibility)
        // {
        //     await CheckCompatibility(fileInfo, CancellationToken.None);
        // }
    }

    private ParallelOptions FileProcessingParallelOptions() => new() { MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 5) };

    public async Task PerformPendingFileGrouping(IProgress<FileLibraryTaskProgress> progress)
    {
        var stopwatch = Stopwatch.StartNew();
        var filesWithoutGroupingDone = _knownFiles.Values.Where(x => !x.GroupingDone).ToList();
        var todo = filesWithoutGroupingDone.Count;
        var done = 0;

        progress.Report(new(done, todo, stopwatch.Elapsed));

        foreach (var fileInfo in filesWithoutGroupingDone)
        {
            try
            {
                var collection = fileGroupingService.FindMatchingFileCollection(fileInfo, _pictureFileCollections.Values);
                if (collection == null)
                {
                    collection = new FileLibraryEntryCollection([fileInfo]);
                    _pictureFileCollections.Add(collection.FileLibraryEntryCollectionId, collection);
                    _fileCollectionsIndex[fileInfo.FileId] = collection;
                }
                else
                {
                    collection.Add(fileInfo);
                }

                fileInfo.GroupingDone = true;
                NotifyLibraryChanged();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to perform file grouping. Path: {FullPath}", fileInfo.FilesystemRecord.GetFullPath());
            }

            done++;
            progress.Report(new(done, todo, stopwatch.Elapsed));
        }
    }


    public void RemoveFile(FileLibraryEntry fileLibraryEntry)
    {
        _knownFiles.Remove(fileLibraryEntry.FileId);

        if (!_fileCollectionsIndex.TryGetValue(fileLibraryEntry.FileId, out var fileInfoCollection))
            return;

        fileInfoCollection.TryRemove(fileLibraryEntry);
        if (fileInfoCollection.IsEmpty())
        {
            _pictureFileCollections.Remove(fileInfoCollection.FileLibraryEntryCollectionId);
        }

        _fileCollectionsIndex.Remove(fileLibraryEntry.FileId);
        NotifyLibraryChanged();
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

public record FileLibraryTaskProgress(int Done, int Total, TimeSpan Elapsed);