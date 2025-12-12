using Domorama.PictureFrame.ImageProcessing.ColorPallete;
using Domorama.PictureFrame.ImageProcessing.FaceDetection.Model;
using Domorama.PictureFrame.ImageProcessing.Geocoding.Model;

namespace Domorama.PictureFrame.Core.Model;

public class PictureFrameEntry(BasicFileInfoCollection fileInfoCollection)
{
    private BasicFileInfoCollection FileInfoCollection { get; init; } = fileInfoCollection;
}

public class PictureInfo(BasicFileInfo fileInfo)
{
    public BasicFileInfo FileInfo { get; set; } = fileInfo;

    public bool IsCaptureDateExtractionDon { get; set; } = false;
    public bool IsGeolocationExtractionDone { get; set; } = false;
    public bool IsDominantColorExtractionDone { get; set; } = false;
    public bool IsFaceDetectionDone { get; set; } = false;

    public DateTime? CaptureDate { get; set; } = null;
    public List<DetectedFace> DetectedFaces { get; init; } = [];
    public List<Color> DominantColors { get; init; } = [];
    public GeoLocation? GeoLocation { get; init; } = null;
}