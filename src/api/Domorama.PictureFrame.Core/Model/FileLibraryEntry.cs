namespace Domorama.PictureFrame.Core.Model;

public record FileLibraryEntry
{
    public Guid FileId { get; init; } = Guid.CreateVersion7();
    public FilesystemRecord FilesystemRecord { get; init; }
    public string Crc32Checksum { get; init; }
    public bool CompatibilityCheckDone { get; set; }
    public bool GroupingDone { get; set; }
    public FileCompatibilityCheckResult? CompatibilityCheckResult { get; set; }

    public FileLibraryEntry(FilesystemRecord FilesystemRecord, string Crc32Checksum)
    {
        this.FilesystemRecord = FilesystemRecord;
        this.Crc32Checksum = Crc32Checksum;
    }
}

public class FileCompatibilityCheckResult
{
    public bool IsPicture { get; init; }
    public bool IsOpenCvReadable { get; init; }
}