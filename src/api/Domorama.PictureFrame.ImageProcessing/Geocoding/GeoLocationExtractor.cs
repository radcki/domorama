using Domorama.PictureFrame.ImageProcessing.Geocoding.Nominatim;
using Domorama.PictureFrame.ImageProcessing.Geocoding.Nominatim.Model;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace Domorama.PictureFrame.ImageProcessing.Geocoding
{
    public class GeoLocationExtractor(NominatimApiClient nominatimApiClient)
    {
        public GeoLocation? GetGeolocationFromFile(string path)
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

        public async Task<Dictionary<string, string>> GetAddressForLocation(GeoLocation geoLocation)
        {
            var response = await nominatimApiClient.ReversePlaceSearch(new ReversePlaceSearch.Request(geoLocation.Latitude, geoLocation.Longitude));
            var address = response.Address;
            address["display_name"] = response.DisplayName;
            return response.Address;
        }
    }
}