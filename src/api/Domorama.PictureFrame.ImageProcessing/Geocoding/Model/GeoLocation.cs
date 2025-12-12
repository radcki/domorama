namespace Domorama.PictureFrame.ImageProcessing.Geocoding.Model;

public class GeoLocation(GeoCoordinates geoCoordinates, Dictionary<string, string> addressDictionary)
{
    public GeoCoordinates GeoCoordinates { get; init; } = geoCoordinates;
    public Dictionary<string, string> AddressDictionary { get; init; } = addressDictionary;
}