using Domorama.PictureFrame.Core.Model;

namespace Domorama.PictureFrame.Core.Library.Files;

public class FileLibraryData
{
    public List<FileLibraryEntry> KnownFiles { get; init; } = [];
    public List<FileLibraryEntryCollection> PictureFileInfoCollections { get; init; } = [];
}