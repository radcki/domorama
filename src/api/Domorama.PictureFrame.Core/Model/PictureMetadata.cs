using System.Drawing;
using Domorama.PictureFrame.ImageProcessing.FaceDetection.Model;
using Domorama.PictureFrame.ImageProcessing.Geocoding.Model;

namespace Domorama.PictureFrame.Core.Model;

public class PictureMetadata
{
    public GeoLocation? GeoLocation { get; set; }
    public DateTime? CaptureTime { get; set; }
    public List<DetectedFace>? DetectedFaces { get; set; }
    public List<Color>? DominantColors { get; set; }
}