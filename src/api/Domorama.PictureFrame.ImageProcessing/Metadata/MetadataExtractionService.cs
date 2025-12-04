using System.Globalization;
using Domorama.PictureFrame.ImageProcessing.Utils;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace Domorama.PictureFrame.ImageProcessing.Metadata;

public class MetadataExtractionService
{
    public GeoLocation? ExtractGeolocationFromFile(string path)
    {
        var fileMetadata = ImageMetadataReader.ReadMetadata(path);
        var gpsData = fileMetadata.OfType<GpsDirectory>().FirstOrDefault();
        GeoLocation? geoLocation = null;
        if (gpsData != null && gpsData.TryGetGeoLocation(out var fileGeoLocation))
        {
            geoLocation = fileGeoLocation;
        }

        return geoLocation;
    }

    public DateTime? ExtractCaptureDateFromFile(string path)
    {
        var fileMetadata = ImageMetadataReader.ReadMetadata(path);
        var directory = fileMetadata.OfType<ExifSubIfdDirectory>().FirstOrDefault();

        if (directory is null)
            return null;

        var dateTime = directory.GetString(ExifDirectoryBase.TagDateTimeOriginal);
        var dateTimeFormat = "yyyy:MM:dd HH:mm:ss";
        var timeZone = directory.GetString(ExifDirectoryBase.TagTimeZoneOriginal);

        if (!string.IsNullOrWhiteSpace(timeZone))
        {
            dateTime += $" {timeZone}";
            dateTimeFormat += " zzz";
        }

        if (DateTime.TryParseExact(dateTime, dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var originalCaptureDate))
            return originalCaptureDate;

        return null;
    }
}