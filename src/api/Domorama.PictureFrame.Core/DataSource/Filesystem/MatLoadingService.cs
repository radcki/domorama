using ImageMagick;
using ImageMagick.Formats;
using OpenCvSharp;

namespace Domorama.PictureFrame.Core.DataSource.Filesystem;

public class MatLoadingService
{
    public Task<Mat?> LoadMat(string path)
    {
        return Task.Run(() =>
                        {
                            try
                            {
                                var mat = Cv2.ImRead(path);
                                return mat;
                            }
                            catch (Exception)
                            {
                                return null;
                            }
                        });
    }

    public async Task<Mat?> LoadMatThroughImageMagick(string path)
    {
        try
        {
            await using var fileStream = new FileStream(path, FileMode.Open);
            using var magickImage = new MagickImage(fileStream);
            magickImage.AutoOrient();
            magickImage.SetBitDepth(8);
            var bytes = magickImage.ToByteArray(MagickFormat.Bmp);
            var mat = Cv2.ImDecode(bytes, ImreadModes.Unchanged);
            return mat;
        }
        catch (Exception)
        {
            return null;
        }
    }
}