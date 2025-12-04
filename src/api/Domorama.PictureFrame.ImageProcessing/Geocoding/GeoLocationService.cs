using Domorama.PictureFrame.ImageProcessing.Geocoding.Nominatim;
using Domorama.PictureFrame.ImageProcessing.Geocoding.Nominatim.Model;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace Domorama.PictureFrame.ImageProcessing.Geocoding
{
    public class GeoLocationService(NominatimApiClient nominatimApiClient)
    {
        public async Task<Dictionary<string, string>> GetAddressForLocation(GeoLocation geoLocation)
        {
            var response = await nominatimApiClient.ReversePlaceSearch(new ReversePlaceSearch.Request(geoLocation.Latitude, geoLocation.Longitude));
            var address = response.Address;
            address["display_name"] = response.DisplayName;
            return response.Address;
        }
    }
}