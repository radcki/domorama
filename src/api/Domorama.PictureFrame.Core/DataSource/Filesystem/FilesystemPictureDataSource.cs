using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Domorama.PictureFrame.Core.Model;

namespace Domorama.PictureFrame.Core.DataSource.Filesystem;

public class FilesystemPictureDataSourceConfiguration()
{
    public List<string> Directories { get; set; } = [];

    public List<string> SupportedFileExtensions { get; set; } =
    [
        ".jpg", ".jpeg", ".png"
    ];
}

public class FilesystemPictureDataSource(FilesystemPictureDataSourceConfiguration configuration) : IPictureDataSource
{
    /// <inheritdoc />
    public async IAsyncEnumerable<PictureMetadata> ScanAsync(CancellationToken cancellationToken = default)
    {
        foreach (var sourceDirectory in configuration.Directories)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            IEnumerable<string> directoryFiles = [];
            try
            {
                directoryFiles = Directory.EnumerateFiles(sourceDirectory, "*.*", SearchOption.AllDirectories);
            }
            catch (Exception e)
            {
                //todo logging
                Console.WriteLine(e);
            }

            var supportedFilesEnumerable = directoryFiles.Where(f => configuration.SupportedFileExtensions
                                                                                  .Any(x => string.Compare(Path.GetExtension(f), x, StringComparison.InvariantCultureIgnoreCase) == 0));
            foreach (var path in supportedFilesEnumerable)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var pictureMetadata = new PictureMetadata()
                                      {
                                          Path = path
                                      };
                yield return pictureMetadata;
            }
        }
    }
}