using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    private readonly MagickFormat[] _opencvSupportedFormats =
    [
        MagickFormat.Jpg, MagickFormat.Jpeg, MagickFormat.Jpe,
        MagickFormat.Bmp, MagickFormat.Dib,
        MagickFormat.Png,
        MagickFormat.Tif, MagickFormat.Tiff,
        MagickFormat.WebP
    ];

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

                var filesystemRecord = new FilesystemRecord(fileInfo.DirectoryName ?? "", fileInfo.Name, fileInfo.Length);

                yield return filesystemRecord;
            }
        }
    }

    public async Task<BasicFileInfo> ReadBasicFileInfo(FilesystemRecord filesystemRecord)
    {
        var path = filesystemRecord.GetPath();
        await using var fileStream = new FileStream(path, FileMode.Open);
        var info = GetImageInfo(fileStream);
        var isPicture = info != null;
        if (!isPicture)
        {
            return BasicFileInfo.NonPicture(filesystemRecord);
        }

        var isOpenCvReadable = _opencvSupportedFormats.Contains(info.Format);
        fileStream.Position = 0;
        var md5 = CheckMd5(fileStream);

        // var bytes = await File.ReadAllBytesAsync(path);
        // var md5 = CheckMd5(bytes);
        // var isOpenCvReadable = IsBufferOpenCvReadable(bytes);
        // var isPicture = isOpenCvReadable || IsBufferImageMagickReadable(bytes);

        // if (isPicture && !isOpenCvReadable)
        // {
        //     var picture = new MagickImage(bytes);
        //     using var stream = new MemoryStream();
        //     await picture.WriteAsync(stream, MagickFormat.Bmp);
        //     stream.Position = 0;
        //     Mat.FromStream(stream, ImreadModes.Unchanged);
        // }

        return BasicFileInfo.Picture(filesystemRecord, isOpenCvReadable, md5);
    }

    private MagickImage? GetImageInfo(Stream stream)
    {
        try
        {
            var info = new MagickImage(stream);
            return info;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private bool IsBufferOpenCvReadable(byte[] bytes)
    {
        try
        {
            var info = new MagickImageInfo(bytes);

            return _opencvSupportedFormats.Contains(info.Format);
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

    private string CheckMd5(Stream stream)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}

public class Watch(string id) : IDisposable
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    public void Dispose()
    {
        _stopwatch.Stop();
        Console.WriteLine($"Watch {id} ended after: {Elapsed.TotalSeconds:f4} s / {Elapsed.TotalMilliseconds:f2} ms");
    }
}