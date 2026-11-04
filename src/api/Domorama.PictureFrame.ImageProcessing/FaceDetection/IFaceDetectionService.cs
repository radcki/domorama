using Domorama.PictureFrame.ImageProcessing.FaceDetection.Model;
using OpenCvSharp;

namespace Domorama.PictureFrame.ImageProcessing.FaceDetection;

public interface IFaceDetectionService
{
    IEnumerable<DetectedFace> DetectFaces(Mat image);
}



public record DetectedEyes(DetectedEye LeftEye, DetectedEye RightEye);

public record DetectedEye(Rect EyeArea)
{
    public Point GetCenter() => new(EyeArea.X + EyeArea.Width / 2, EyeArea.Y + EyeArea.Height / 2);
};