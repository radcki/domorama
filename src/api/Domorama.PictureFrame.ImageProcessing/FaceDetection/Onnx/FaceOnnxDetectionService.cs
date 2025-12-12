using System.Drawing;
using Domorama.PictureFrame.ImageProcessing.FaceDetection.Model;
using Domorama.PictureFrame.ImageProcessing.Model;
using FaceONNX;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Domorama.PictureFrame.ImageProcessing.FaceDetection.ONNX;

public class FaceOnnxDetectionService : IFaceDetectionService
{
    private readonly FaceDetector _faceDetector = new();


    /// <inheritdoc />
    public IEnumerable<DetectedFace> DetectFaces(Mat image)
    {
        var detections = _faceDetector.Forward(image.ToBitmap());
        foreach (var faceDetectionResult in detections)
        {
            yield return new DetectedFace(new PixelBox(new Pixel(faceDetectionResult.Box.X,
                                                                 faceDetectionResult.Box.Y),
                                                       new PixelSize(faceDetectionResult.Box.Width,
                                                                     faceDetectionResult.Box.Height)));
        }
    }
}