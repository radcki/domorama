namespace Domorama.PictureFrame.Core.Model;

public class PictureMetadata
{
    public bool IsCaptureDateExtractionDon { get; set; } = false;
    public bool IsGeolocationExtractionDone { get; set; } = false;
    public bool IsDominantColorExtractionDone { get; set; } = false;
    public bool IsFaceDetectionDone { get; set; } = false;

    public DateTime? CaptureDate { get; set; }
}