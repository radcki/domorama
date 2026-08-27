using Domorama.PictureFrame.Core.Model;
using ImageMagick;
using OpenCvSharp;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

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

                var subdirectory = fileInfo.DirectoryName?.Remove(0, sourceDirectory.Length);
                var filesystemRecord = new FilesystemRecord(sourceDirectory, subdirectory ?? "", fileInfo.Name, fileInfo.Length);

                yield return filesystemRecord;
            }
        }
    }

    public async Task<FileLibraryEntry> ReadBasicFileInfo(FilesystemRecord filesystemRecord)
    {
        var path = filesystemRecord.GetFullPath();
        await using var fileStream = new FileStream(path, FileMode.Open);
        
        fileStream.Position = 0;

        var crc32 = CheckCrc32(fileStream);
        // var md5 = CheckMd5(fileStream);

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

        return new FileLibraryEntry(filesystemRecord, crc32);
    }

    public async Task<FileCompatibilityCheckResult> CheckFileCompatibility(FilesystemRecord filesystemRecord)
    {
        var path = filesystemRecord.GetFullPath();
        await using var fileStream = new FileStream(path, FileMode.Open);
        var imageInfo = GetImageInfo(fileStream);
        var isPicture = imageInfo != null;
        var isOpenCvReadable = imageInfo != null && _opencvSupportedFormats.Contains(imageInfo.Format);
        return new FileCompatibilityCheckResult()
               {
                   IsOpenCvReadable = isOpenCvReadable,
                   IsPicture = isPicture
               };
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
        return Convert.ToHexString(hash);
    }

    private string CheckMd5(Stream stream)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }

    private string CheckCrc32(Stream stream)
    {
        var crc32 = new System.IO.Hashing.Crc32();
        crc32.Append(stream);
        return Convert.ToHexString(crc32.GetHashAndReset());
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