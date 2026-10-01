namespace Domorama.PictureFrame.Core.Model;

public class PictureFrameEntry()
{
    public Guid FileLibraryEntryCollectionId { get; init; }

    public PictureInfo PictureInfo { get; set; } = new();
}