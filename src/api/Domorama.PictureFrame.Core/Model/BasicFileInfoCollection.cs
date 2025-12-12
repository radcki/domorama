namespace Domorama.PictureFrame.Core.Model;

public class BasicFileInfoCollection(IEnumerable<BasicFileInfo> fileInstances)
{
    private readonly List<BasicFileInfo> _items = fileInstances.ToList();
    public IEnumerable<BasicFileInfo> Items => _items.AsEnumerable();

    public DateTime LastUpdateDate { get; private set; } = DateTime.UtcNow;

    public bool Contains(BasicFileInfo file) => _items.Contains(file);

    public bool IsEmpty()
    {
        return _items.Count == 0;
    }

    public void Add(BasicFileInfo newFileInfo)
    {
        _items.Add(newFileInfo);
        LastUpdateDate = DateTime.UtcNow;
    }

    public bool TryRemove(BasicFileInfo file)
    {
        if (!_items.Remove(file))
            return false;

        LastUpdateDate = DateTime.UtcNow;
        return true;
    }
}