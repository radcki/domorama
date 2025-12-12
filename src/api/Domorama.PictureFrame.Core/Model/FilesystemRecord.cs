namespace Domorama.PictureFrame.Core.Model;

public record FilesystemRecord(string BasePath, string Subdirectory, string Filename, long ByteCount)
{
    public string GetFullPath() => Path.Combine(BasePath, Subdirectory, Filename);
}