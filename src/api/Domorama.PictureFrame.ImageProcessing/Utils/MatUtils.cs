using OpenCvSharp;

namespace Domorama.PictureFrame.ImageProcessing.Utils
{
    public static class MatUtils
    {
        public static void ResizeToCover(Mat image, Size size)
        {
            if (size == default)
            {
                return;
            }

            if (image.Rows == 0 || image.Cols == 0)
            {
                var emptyImage = new Mat(size, MatType.CV_8UC3, new Scalar(255, 255, 255));
                emptyImage.CopyTo(image);
                return;
            }

            var wScalingRatio = (float)size.Width / image.Cols;
            var hScalingRatio = (float)size.Height / image.Rows;

            var noScalingRequired = Math.Abs(wScalingRatio - 1) < 0.001f && Math.Abs(hScalingRatio - 1) < 0.001f;
            if (noScalingRequired)
            {
                return;
            }

            var scalingRatio = Math.Max(wScalingRatio, hScalingRatio);

            var enlargeRequired = scalingRatio > 0;
            var interpolation = enlargeRequired ? InterpolationFlags.Cubic : InterpolationFlags.Area;
            Cv2.Resize(image, image, new(), scalingRatio, scalingRatio, interpolation);
        }

        public static Mat ResizeToCoverToNew(Mat image, Size size)
        {
            if (size == default)
            {
                return image.Clone();
            }

            if (image.Rows == 0 || image.Cols == 0)
            {
                var emptyImage = new Mat(size, MatType.CV_8UC3, new Scalar(255, 255, 255));
                return emptyImage;
            }

            var wScalingRatio = (float)size.Width / image.Cols;
            var hScalingRatio = (float)size.Height / image.Rows;

            var noScalingRequired = Math.Abs(wScalingRatio - 1) < 0.001f && Math.Abs(hScalingRatio - 1) < 0.001f;
            if (noScalingRequired)
            {
                return image.Clone();
            }

            var scalingRatio = Math.Max(wScalingRatio, hScalingRatio);

            var enlargeRequired = scalingRatio > 0;
            var interpolation = enlargeRequired ? InterpolationFlags.Cubic : InterpolationFlags.Area;
            var newMat = new Mat();
            Cv2.Resize(image, newMat, new(), scalingRatio, scalingRatio, interpolation);
            return newMat;
        }

        public static void CropToSizeFromCenter(Mat image, Size size)
        {
            if (size.Width >= image.Cols && size.Height >= image.Rows)
            {
                return;
            }

            var centerX = image.Cols / 2;
            var centerY = image.Rows / 2;

            var rowStart = Math.Clamp(centerY - size.Height / 2, 0, image.Rows);
            var rowEnd = Math.Clamp(rowStart + size.Height, 0, image.Rows);
            var colStart = Math.Clamp(centerX - size.Width / 2, 0, image.Cols);
            var colEnd = Math.Clamp(colStart + size.Width, 0, image.Cols);

            image[rowStart, rowEnd, colStart, colEnd].CopyTo(image);
        }

        public static byte[] EncodeToJpg(Mat image, int quality = 90)
        {
            quality = Math.Clamp(quality, 0, 100);
            return image.ImEncode(".jpg", new ImageEncodingParam(ImwriteFlags.JpegQuality, quality));
        }

        public static Rect ScaleRect(Rect rect, double factor)
        {
            return new Rect((int)(rect.X * factor), (int)(rect.Y * factor), (int)(rect.Width * factor), (int)(rect.Height * factor));
        }

        public static Rect MoveRect(Rect rect, Point vector)
        {
            return new Rect(rect.Location + vector, rect.Size);
        }

        public static Mat RotateImage(Mat image, double angle)
        {
            if (angle == 0)
                return image;
            var height = image.Rows;
            var width = image.Cols;

            var rotationMatrix = Cv2.GetRotationMatrix2D(new((float)width / 2, (float)height / 2), angle, 1);
            var dest = new Mat();
            Cv2.WarpAffine(image, dest, rotationMatrix, new(width, height), InterpolationFlags.Linear);
            return dest;
        }

        public static IEnumerable<Point> TransformPoints(this IEnumerable<Point> points, Mat rotMat)
        {
            Mat transformedPoints = new Mat();
            Cv2.Transform(InputArray.Create(points), transformedPoints, rotMat);
            transformedPoints.GetArray(out Point[] results);
            return results;
        }
    }
}