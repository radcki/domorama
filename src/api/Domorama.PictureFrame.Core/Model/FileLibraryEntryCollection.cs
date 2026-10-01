using Newtonsoft.Json;

namespace Domorama.PictureFrame.Core.Model;

public class FileLibraryEntryCollection
{
    public FileLibraryEntryCollection()
    {
    }

    public FileLibraryEntryCollection(IEnumerable<FileLibraryEntry> fileInstances)
    {
        Items = fileInstances.ToList();
    }

    public Guid FileLibraryEntryCollectionId { get; init; } = Guid.CreateVersion7();
    public List<FileLibraryEntry> Items { get; init; } = [];
    public DateTime LastUpdateDate { get; private set; } = DateTime.UtcNow;

    public bool Contains(FileLibraryEntry file) => Items.Contains(file);

    public bool IsEmpty()
    {
        return Items.Count == 0;
    }

    public void Add(FileLibraryEntry newFileLibraryEntry)
    {
        Items.Add(newFileLibraryEntry);
        LastUpdateDate = DateTime.UtcNow;
    }

    public bool TryRemove(FileLibraryEntry file)
    {
        if (!Items.Remove(file))
            return false;

        LastUpdateDate = DateTime.UtcNow;
        return true;
    }
}