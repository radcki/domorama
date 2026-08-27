namespace Domorama.PictureFrame.Core.Model;

public class PictureFrameEntry()
{
    private Guid FileLibraryEntryCollectionId { get; init; }

    public PictureInfo PictureInfo { get; set; } = new();
}