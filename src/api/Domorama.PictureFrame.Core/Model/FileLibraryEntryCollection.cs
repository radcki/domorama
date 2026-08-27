namespace Domorama.PictureFrame.Core.Model;

public class FileLibraryEntryCollection(IEnumerable<FileLibraryEntry> fileInstances)
{
    public FileLibraryEntryCollection() : this([])
    {
    }

    public Guid FileLibraryEntryCollectionId { get; init; } = Guid.CreateVersion7();
    private readonly List<FileLibraryEntry> _items = fileInstances.ToList();
    public IEnumerable<FileLibraryEntry> Items => _items.AsEnumerable();

    public DateTime LastUpdateDate { get; private set; } = DateTime.UtcNow;

    public bool Contains(FileLibraryEntry file) => _items.Contains(file);

    public bool IsEmpty()
    {
        return _items.Count == 0;
    }

    public void Add(FileLibraryEntry newFileLibraryEntry)
    {
        _items.Add(newFileLibraryEntry);
        LastUpdateDate = DateTime.UtcNow;
    }

    public bool TryRemove(FileLibraryEntry file)
    {
        if (!_items.Remove(file))
            return false;

        LastUpdateDate = DateTime.UtcNow;
        return true;
    }
}