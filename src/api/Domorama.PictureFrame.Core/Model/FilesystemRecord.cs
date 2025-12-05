namespace Domorama.PictureFrame.Core.Model;

public record FilesystemRecord(string Directory, string Filename, long ByteCount)
{
    public string GetPath() => Path.Combine(Directory, Filename);
}

public record BaseFileInfo(FilesystemRecord FilesystemRecord, string Md5Checksum, bool IsPicture, bool IsOpenCvReadable)
{
}