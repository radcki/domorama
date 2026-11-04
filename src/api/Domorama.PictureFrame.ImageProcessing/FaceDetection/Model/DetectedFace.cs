using Domorama.PictureFrame.ImageProcessing.Model;

namespace Domorama.PictureFrame.ImageProcessing.FaceDetection.Model;

public class DetectedFace(PixelBox boundingBox)
{
    public PixelBox BoundingBox { get; set; } = boundingBox;
}