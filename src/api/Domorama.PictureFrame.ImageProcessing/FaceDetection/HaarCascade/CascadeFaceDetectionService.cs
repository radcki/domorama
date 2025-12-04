using System.Diagnostics;
using Domorama.PictureFrame.ImageProcessing.Utils;
using OpenCvSharp;

namespace Domorama.PictureFrame.ImageProcessing.FaceDetection.HaarCascade
{
    public class CascadeFaceDetectionService : IFaceDetectionService
    {
        private readonly CascadeClassifier _faceClassifier;
        private readonly CascadeClassifier _eyeClassifier;

        public CascadeFaceDetectionService()
        {
            _eyeClassifier = new CascadeClassifier(".\\FaceDetection\\HaarCascade\\haarcascade_eye_tree_eyeglasses.xml");
            _faceClassifier = new CascadeClassifier(".\\FaceDetection\\HaarCascade\\haarcascade_frontalface_alt.xml");
        }

        public IEnumerable<DetectedFace> DetectFaces(Mat image)
        {
            using var src = image.Clone();
            var expectedWidth = 800;
            var faceScaling = 1 / (src.Cols / (double)expectedWidth);
            var grey = new Mat();
            Cv2.CvtColor(src, grey, ColorConversionCodes.BGR2GRAY);
            var small = new Mat();
            Cv2.Resize(grey, small, new Size(0, 0), faceScaling, faceScaling);
            
            var faces = new List<Rect>();
            foreach (var angle in new[] { 0, -25, 25 })
            {
                var rotatedSmall = MatUtils.RotateImage(small, angle);
                var detectedFaces = _faceClassifier.DetectMultiScale(rotatedSmall, 1.1, 5);
   
                if (angle != 0 && detectedFaces.Any())
                {
                    foreach (var detectedFace in detectedFaces)
                    {
                        var mask = new Mat(rotatedSmall.Size(), MatType.CV_8UC1, new Scalar(0));
                        mask[detectedFace].SetTo(new Scalar(255));
                        var a = new Mat();;
                        var unrotatedMask = MatUtils.RotateImage(mask, -angle);

                        faces.Add(Cv2.BoundingRect(unrotatedMask));
                    }
                }
                else
                {
                    faces.AddRange(detectedFaces);
                }

                if (detectedFaces.Any())
                {
                    break;
                }
            }

            foreach (var face in faces)
            {
                var faceArea = MatUtils.ScaleRect(face, 1 / faceScaling);

                yield return new DetectedFace(faceArea);
            }
        }

        public DetectedEyes? DetectEyes(Mat image, DetectedFace detectedFace)
        {
            using var src = image[detectedFace.FaceArea].Clone();
            var expectedWidth = 300;
            var faceScaling = 1 / (src.Cols / (double)expectedWidth);
            var grey = new Mat();
            Cv2.CvtColor(src, grey, ColorConversionCodes.BGR2GRAY);
            var small = new Mat();
            Cv2.Resize(grey, small, new Size(0, 0), faceScaling, faceScaling);

            var eyes = _eyeClassifier.DetectMultiScale(small, 1.1, 3);

            var twoLargestEyes = eyes.OrderByDescending(x => x.Width * x.Height).Take(2).ToList();
            if (twoLargestEyes.Count != 2)
            {
                return null;
            }

            var leftEye = MatUtils.MoveRect(MatUtils.ScaleRect(twoLargestEyes.MinBy(x => x.X), 1 / faceScaling), detectedFace.FaceArea.Location);
            var rightEye = MatUtils.MoveRect(MatUtils.ScaleRect(twoLargestEyes.MaxBy(x => x.X), 1 / faceScaling), detectedFace.FaceArea.Location);


            return new DetectedEyes(new DetectedEye(leftEye), new DetectedEye(rightEye));
        }
    }
}