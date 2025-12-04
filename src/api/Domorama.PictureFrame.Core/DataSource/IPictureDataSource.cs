using Domorama.PictureFrame.Core.Model;

namespace Domorama.PictureFrame.Core.DataSource;

public interface IPictureDataSource
{
    public IAsyncEnumerable<PictureMetadata> ScanAsync(CancellationToken cancellationToken = default);
}