using Domorama.PictureFrame.Core.Model;

namespace Domorama.PictureFrame.Core.DataSource;

public interface IPictureDataSource
{
    public IAsyncEnumerable<FilesystemRecord> ScanAsync(CancellationToken cancellationToken = default);
}