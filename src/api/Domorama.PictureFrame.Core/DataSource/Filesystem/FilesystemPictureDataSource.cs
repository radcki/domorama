using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using Domorama.PictureFrame.Core.Model;
using ImageMagick;
using OpenCvSharp;

namespace Domorama.PictureFrame.Core.DataSource.Filesystem;

public class FilesystemPictureDataSourceConfiguration()
{
    public List<string> Directories { get; set; } = [];

    // public List<string> SupportedFileExtensions { get; set; } =
    // [
    //     ".jpg", ".jpeg", ".png"
    // ];
}

public class FilesystemPictureDataSource(FilesystemPictureDataSourceConfiguration configuration) : IPictureDataSource
{
    /// <inheritdoc />
    public async IAsyncEnumerable<FilesystemRecord> ScanAsync(CancellationToken cancellationToken = default)
    {
        foreach (var sourceDirectory in configuration.Directories)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var directoryInfo = new DirectoryInfo(sourceDirectory);
            var files = directoryInfo.GetFiles("*.*", new EnumerationOptions()
                                                      {
                                                          RecurseSubdirectories = true,
                                                          IgnoreInaccessible = true,
                                                          ReturnSpecialDirectories = false,
                                                      });
            foreach (var fileInfo in files)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var pictureMetadata = new FilesystemRecord(fileInfo.DirectoryName ?? "", fileInfo.Name, fileInfo.Length);

                yield return pictureMetadata;
            }
        }
    }

    public async Task<BaseFileInfo> ReadBaseFileInfo(FilesystemRecord filesystemRecord)
    {
        var path = filesystemRecord.GetPath();
        var bytes = await File.ReadAllBytesAsync(path);
        var md5 = CheckMd5(bytes);
        var isOpenCvReadable = IsBufferOpenCvReadable(bytes);
        var isPicture = isOpenCvReadable || IsBufferImageMagickReadable(bytes);

        return new BaseFileInfo(filesystemRecord, md5, isPicture, isOpenCvReadable);
    }

    private bool IsBufferOpenCvReadable(byte[] bytes)
    {
        try
        {
            var mat = Cv2.ImDecode(bytes, ImreadModes.Unchanged);

            return mat.Size() != new Size();
        }
        catch (Exception)
        {
            return false;
        }
    }

    private bool IsBufferImageMagickReadable(byte[] bytes)
    {
        try
        {
            _ = new MagickImageInfo(bytes);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private string CheckMd5(byte[] bytes)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}