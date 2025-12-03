using Newtonsoft.Json;

namespace Domorama.PictureFrame.ImageProcessing.Geocoding.Nominatim.Model
{
    public abstract class ReversePlaceSearch
    {
        public record Request(double Latitude, double Longitude);

        public class Response
        {
            [JsonProperty("place_id")] public int PlaceId { get; set; }
            [JsonProperty("lat")] public double Latitude { get; set; }
            [JsonProperty("lon")] public double Longitude { get; set; }

            [JsonProperty("addresstype")] public string AddressType { get; set; }

            [JsonProperty("display_name")] public string DisplayName { get; set; }

            [JsonProperty("address")] public Dictionary<string, string> Address { get; set; } = new();
        }
    }
}