using OpenCvSharp;

namespace Domorama.PictureFrame.ImageProcessing.FaceDetection;

public interface IFaceDetectionService
{
    IEnumerable<DetectedFace> DetectFaces(Mat image);
}

public record DetectedFace(Rect FaceArea)
{
    public bool IsSameFace(DetectedFace other)
    {
        var intersection = FaceArea.Intersect(other.FaceArea);
        var union = FaceArea.Union(other.FaceArea);
        var intersectionArea = intersection.Width * intersection.Height;
        var unionArea = union.Width * union.Height;
        var overlap = (intersectionArea / (double)unionArea);
        return overlap > 0.6;
    }
};

public record DetectedEyes(DetectedEye LeftEye, DetectedEye RightEye);

public record DetectedEye(Rect EyeArea)
{
    public Point GetCenter() => new(EyeArea.X + EyeArea.Width / 2, EyeArea.Y + EyeArea.Height / 2);
};