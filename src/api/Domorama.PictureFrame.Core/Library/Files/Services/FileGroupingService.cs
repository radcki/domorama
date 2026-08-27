using Domorama.PictureFrame.Core.Model;

namespace Domorama.PictureFrame.Core.Library.Files.Services;

public class FileGroupingServiceConfiguration
{
    public bool MatchByChecksum { get; init; } = true;
    public bool MatchByFilenameInSameDirectory { get; init; } = true;
    public bool MatchByFilenameGlobally { get; init; } = false;
}

public class FileGroupingService(FileGroupingServiceConfiguration configuration)
{
    // public List<BasicFileInfoCollection> GroupFiles(IEnumerable<BasicFileInfo> items)
    // {
    //     var open = items.ToList();
    //     var closed = new List<BasicFileInfoCollection>();
    //     while (open.Count > 0)
    //     {
    //         var matches = new List<BasicFileInfo>();
    //         var current = open.First();
    //         open.Remove(current);
    //         matches.Add(current);
    //
    //         bool matchFound;
    //         do
    //         {
    //             matchFound = false;
    //             if (configuration.MatchByFilenameInSameDirectory)
    //             {
    //                 foreach (var grouping in matches.GroupBy(x => x.FilesystemRecord.Subdirectory).ToList())
    //                 {
    //                     var directory = grouping.Key;
    //                     var filenames = grouping.Select(x => Path.GetFileNameWithoutExtension(x.FilesystemRecord.Filename))
    //                                             .ToList();
    //                     var filenameMatches = open.Where(x => x.FilesystemRecord.Subdirectory == directory
    //                                                           && filenames.Contains(Path.GetFileNameWithoutExtension(x.FilesystemRecord.Filename)))
    //                                               .ToList();
    //                     foreach (var match in filenameMatches)
    //                     {
    //                         matchFound = true;
    //                         open.Remove(match);
    //                         matches.Add(match);
    //                     }
    //                 }
    //             }
    //
    //             if (configuration.MatchByFilenameGlobally)
    //             {
    //                 var filenames = matches.Select(x => Path.GetFileNameWithoutExtension(x.FilesystemRecord.Filename))
    //                                        .ToList();
    //                 var filenameMatches = open.Where(x => filenames.Contains(Path.GetFileNameWithoutExtension(x.FilesystemRecord.Filename)))
    //                                           .ToList();
    //                 foreach (var match in filenameMatches)
    //                 {
    //                     matchFound = true;
    //                     open.Remove(match);
    //                     matches.Add(match);
    //                 }
    //             }
    //
    //             if (configuration.MatchByChecksum)
    //             {
    //                 var md5s = matches.Select(x => x.Crc32Checksum).OfType<string>().Distinct().ToArray();
    //                 var match = open.FirstOrDefault(x => x.Crc32Checksum != null && md5s.Contains(x.Crc32Checksum));
    //                 if (match != null)
    //                 {
    //                     open.Remove(match);
    //                     matches.Add(match);
    //                     matchFound = true;
    //                 }
    //             }
    //         }
    //         while (matchFound);
    //
    //
    //         closed.Add(new BasicFileInfoCollection(matches));
    //     }
    //
    //     return closed;
    // }

    public FileLibraryEntryCollection? FindMatchingFileCollection(FileLibraryEntry newFile, List<FileLibraryEntryCollection> pictureFileCollections)
    {
        if (configuration.MatchByFilenameInSameDirectory)
        {
            var collection = pictureFileCollections.FirstOrDefault(x => x.Items.Any(s => s.FilesystemRecord.BasePath == newFile.FilesystemRecord.BasePath 
                                                                                         && s.FilesystemRecord.Subdirectory == newFile.FilesystemRecord.Subdirectory
                                                                                         && Path.GetFileNameWithoutExtension(s.FilesystemRecord.Filename) == Path.GetFileNameWithoutExtension(newFile.FilesystemRecord.Filename)));
            if (collection != null)
            {
                return collection;
            }
        }

        if (configuration.MatchByFilenameGlobally)
        {
            var collection = pictureFileCollections.FirstOrDefault(x => x.Items.Any(s => Path.GetFileNameWithoutExtension(s.FilesystemRecord.Filename) == Path.GetFileNameWithoutExtension(newFile.FilesystemRecord.Filename)));
            if (collection != null)
            {
                return collection;
            }
        }

        if (configuration.MatchByChecksum)
        {
            var collection = pictureFileCollections.FirstOrDefault(x => x.Items.Any(s => s.Crc32Checksum == newFile.Crc32Checksum));
            if (collection != null)
            {
                return collection;
            }
        }

        return null;
    }
}