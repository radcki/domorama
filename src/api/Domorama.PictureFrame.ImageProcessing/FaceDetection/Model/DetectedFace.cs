using Domorama.PictureFrame.ImageProcessing.Model;

namespace Domorama.PictureFrame.ImageProcessing.FaceDetection.Model;

public class DetectedFace(RelativeBox boundingBox)
{
    public RelativeBox BoundingBox { get; set; } = boundingBox;
}