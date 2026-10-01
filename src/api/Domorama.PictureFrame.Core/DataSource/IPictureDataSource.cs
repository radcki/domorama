using Domorama.PictureFrame.Core.Model;
using OpenCvSharp;

namespace Domorama.PictureFrame.Core.DataSource;

public interface IPictureDataSource
{
    public IAsyncEnumerable<FilesystemRecord> ScanAsync(CancellationToken cancellationToken = default);
    public Task<Mat?> LoadImage(FileLibraryEntry fileLibraryEntry);
}

