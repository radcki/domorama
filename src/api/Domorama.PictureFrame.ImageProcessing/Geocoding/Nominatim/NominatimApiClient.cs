using System.Globalization;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using Domorama.PictureFrame.ImageProcessing.Geocoding.Nominatim.Model;
using Newtonsoft.Json;

namespace Domorama.PictureFrame.ImageProcessing.Geocoding.Nominatim;

public class NominatimApiClient(IHttpClientFactory httpClientFactory)
{
    public async Task<ReversePlaceSearch.Response> ReversePlaceSearch(ReversePlaceSearch.Request request)
    {
        var url = @"https://nominatim.openstreetmap.org/reverse";

        var response = await GetRequest<ReversePlaceSearch.Response>(url, new()
                                                                     {
                                                                         { "lat", request.Latitude.ToString(CultureInfo.InvariantCulture) },
                                                                         { "lon", request.Longitude.ToString(CultureInfo.InvariantCulture) },
                                                                         { "format", "json" },
                                                                     });
        return response;
    }

    private async Task<T> GetRequest<T>(string url, Dictionary<string, string> parameters)
    {
        var queryString = AddQueryStringToUrl(url, parameters);

        var httpClient = httpClientFactory.CreateClient();
        AddUserAgent(httpClient);
        var result = await httpClient.GetStringAsync(queryString).ConfigureAwait(false);

        return JsonConvert.DeserializeObject<T>(result);
    }

    private static string AddQueryStringToUrl(string url, IDictionary<string, string> parameters)
    {
        if ((parameters?.Keys.Count ?? 0) == 0)
        {
            return url;
        }

        var op = url.IndexOf('?') != -1;
        var sb = new StringBuilder();
        sb.Append(url);
        foreach (var kvp in parameters)
        {
            sb.Append(op ? '&' : '?');
            sb.Append($"{UrlEncoder.Default.Encode(kvp.Key)}={UrlEncoder.Default.Encode(kvp.Value)}");
            op = true;
        }

        return sb.ToString();
    }

    private static void AddUserAgent(HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.UserAgent.Clear();
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("raDom", Assembly.GetExecutingAssembly().GetName().Version.ToString()));
    }
}