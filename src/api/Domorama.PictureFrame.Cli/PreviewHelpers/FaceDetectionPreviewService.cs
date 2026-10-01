using Domorama.PictureFrame.Core.DataSource;
using Domorama.PictureFrame.Core.Library.Files;
using Domorama.PictureFrame.Core.Model;
using Domorama.PictureFrame.ImageProcessing.Model;
using Domorama.PictureFrame.ImageProcessing.Utils;
using OpenCvSharp;

namespace Domorama.PictureFrame.Cli.PreviewHelpers;

public class FaceDetectionPreviewService(FileLibrary fileLibrary, IPictureDataSource pictureDataSource)
{
    public async Task Show(PictureFrameEntry pictureFrameEntry)
    {
        var collection = await fileLibrary.TryGetPictureFileCollectionsAsync((pictureFrameEntry.FileLibraryEntryCollectionId));

        var picture = await pictureDataSource.LoadImage(collection.Items.OrderBy(x => x.FilesystemRecord.ByteCount).First());
        var height = 500;
        var width = (height / picture.Height) * picture.Width;
        var display = MatUtils.ResizeToCoverToNew(picture, new Size(width, height));
        
        foreach (var detectedFace in pictureFrameEntry.PictureInfo.DetectedFaces)
        {
            var faceRect = detectedFace.BoundingBox.ToRect(PixelSize.FromSize(display.Size()));
            Cv2.Rectangle(display, faceRect, Scalar.Green, 1);
        }
        
        Cv2.ImShow("Faces", display);
        Cv2.WaitKey();
        Cv2.DestroyAllWindows();
    }
}