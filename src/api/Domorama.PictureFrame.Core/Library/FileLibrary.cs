using Domorama.PictureFrame.Core.DataSource.Filesystem;
using Domorama.PictureFrame.Core.Library.Services;
using Domorama.PictureFrame.Core.Model;

namespace Domorama.PictureFrame.Core.Library;

public class FileLibrary(FileGroupingService fileGroupingService, FilesystemPictureDataSource filesystemPictureDataSource)
{
    private readonly List<BasicFileInfo> _knownFiles = [];
    private readonly List<BasicFileInfoCollection> _pictureFileCollections = [];
    private readonly Dictionary<BasicFileInfo, BasicFileInfoCollection> _fileCollectionsIndex = [];

    public async Task Update(List<FilesystemRecord> filesystemRecords)
    {
        var knownFilesToRemove = _knownFiles.Where(knownFile => filesystemRecords.All(newFile => newFile.GetPath() != knownFile.FilesystemRecord.GetPath())).ToList();
        foreach (var fileToRemove in knownFilesToRemove)
        {
            RemoveFile(fileToRemove);
        }

        var changedFiles = _knownFiles.Where(knownFile => filesystemRecords.Any(newFile => knownFile.FilesystemRecord.GetPath() == newFile.GetPath()
                                                                                           && knownFile.FilesystemRecord.ByteCount != newFile.ByteCount));
        foreach (var outdatedFile in changedFiles)
        {
            RemoveFile(outdatedFile);
        }

        var newFiles = filesystemRecords.Where(newFile => _knownFiles.All(knownFile => knownFile.FilesystemRecord.GetPath() != newFile.GetPath()));
        List<BasicFileInfo> filesToGroup = [];
        foreach (var newFile in newFiles)
        {
            var newFileInfo = await filesystemPictureDataSource.ReadBasicFileInfo(newFile);
            if (!newFileInfo.IsPicture)
            {
                _knownFiles.Add(newFileInfo);
                continue;
            }

            var collection = fileGroupingService.FindMatchingFileCollection(newFileInfo, _pictureFileCollections);
            if (collection != null)
            {
                collection.Add(newFileInfo);
            }
            else
            {
                filesToGroup.Add(newFileInfo);
            }
        }

        var newGroups = fileGroupingService.GroupFiles(filesToGroup);
        foreach (var group in newGroups)
        {
            _pictureFileCollections.Add(group);
            foreach (var groupItem in group.Items)
            {
                _fileCollectionsIndex[groupItem] = group;
            }
        }
    }

    public void RemoveFile(BasicFileInfo fileInfo)
    {
        if (!_fileCollectionsIndex.TryGetValue(fileInfo, out var fileInfoCollection))
            return;

        fileInfoCollection.TryRemove(fileInfo);
        if (fileInfoCollection.IsEmpty())
        {
            _pictureFileCollections.Remove(fileInfoCollection);
        }

        _fileCollectionsIndex.Remove(fileInfo);
    }
}