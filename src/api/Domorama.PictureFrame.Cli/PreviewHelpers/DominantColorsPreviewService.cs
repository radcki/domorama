using Domorama.PictureFrame.Core.DataSource;
using Domorama.PictureFrame.Core.Library.Files;
using Domorama.PictureFrame.Core.Model;
using Domorama.PictureFrame.ImageProcessing.Utils;
using OpenCvSharp;

namespace Domorama.PictureFrame.Cli.PreviewHelpers;

public class DominantColorsPreviewService(FileLibrary fileLibrary, IPictureDataSource pictureDataSource)
{
    public async Task Show(PictureFrameEntry pictureFrameEntry)
    {
        var collection = await fileLibrary.TryGetPictureFileCollectionsAsync((pictureFrameEntry.FileLibraryEntryCollectionId));

        var picture = await pictureDataSource.LoadImage(collection.Items.OrderBy(x => x.FilesystemRecord.ByteCount).First());
        var height = 500;
        var width = (height / picture.Height) * picture.Width;
        var resized = MatUtils.ResizeToCoverToNew(picture, new Size(width, height));
        var colorViewHeight = 100;
        
        var display = Mat.Zeros(resized.Height + colorViewHeight, resized.Width, resized.Type()).ToMat();
        display[new Rect(0, 0, resized.Width, resized.Height)] = resized;

        var dominantColors = pictureFrameEntry.PictureInfo.DominantColors;
        var dominantColorCount = dominantColors.Count;
        var tileWidth = display.Width / dominantColorCount;
        var currentX = 0;
        foreach (var dominantColor in dominantColors)
        {
            var color = dominantColor.ToBgrScalar();
            display[new Rect(currentX, resized.Height, tileWidth, colorViewHeight)].SetTo(color);
            currentX += tileWidth;
        }
        
        Cv2.ImShow("Colors", display);
        Cv2.WaitKey();
        Cv2.DestroyAllWindows();
    }
}