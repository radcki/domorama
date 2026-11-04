namespace Domorama.PictureFrame.Core.Model;

public record BasicFileInfo
{
    public FilesystemRecord FilesystemRecord { get; init; }
    public bool IsPicture { get; init; }
    public bool IsOpenCvReadable { get; init; }
    public string? Md5Checksum { get; init; }

    private BasicFileInfo(FilesystemRecord FilesystemRecord, bool IsPicture, bool IsOpenCvReadable, string? Md5Checksum)
    {
        this.FilesystemRecord = FilesystemRecord;
        this.IsPicture = IsPicture;
        this.IsOpenCvReadable = IsOpenCvReadable;
        this.Md5Checksum = Md5Checksum;
    }

    public static BasicFileInfo NonPicture(FilesystemRecord filesystemRecord) => new(filesystemRecord, false, false, null);
    public static BasicFileInfo Picture(FilesystemRecord filesystemRecord, bool isOpenCvReadable, string md5Checksum) => new(filesystemRecord, true, isOpenCvReadable, md5Checksum);
}