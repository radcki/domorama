using Domorama.PictureFrame.ImageProcessing.Geocoding.Model;
using Domorama.PictureFrame.ImageProcessing.Geocoding.Nominatim;
using Domorama.PictureFrame.ImageProcessing.Geocoding.Nominatim.Model;

namespace Domorama.PictureFrame.ImageProcessing.Geocoding
{
    public class GeoLocationService(NominatimApiClient nominatimApiClient)
    {
        public async Task<Dictionary<string, string>> GetAddressForLocation(GeoCoordinates coordinates)
        {
            var response = await nominatimApiClient.ReversePlaceSearch(new ReversePlaceSearch.Request(coordinates.Latitude, coordinates.Longitude));
            var address = response.Address;
            address["display_name"] = response.DisplayName;
            return response.Address;
        }
    }
}