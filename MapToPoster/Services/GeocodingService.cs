using System.Text.Json;
using System.Text.Json.Serialization;
using MapToPoster.Models;

namespace MapToPoster.Services;

public class GeocodingService
{
    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://nominatim.openstreetmap.org/"),
        DefaultRequestHeaders = { { "User-Agent", "MapToPoster/1.0" } }
    };

    public static async Task<GeoCoordinate?> GetCoordinatesAsync(string city, string country)
    {
        Console.WriteLine("Looking up coordinates...");

        try
        {
            // Rate limiting - respect Nominatim's usage policy
            await Task.Delay(1000);

            var query = $"{city}, {country}";
            var url = $"search?q={Uri.EscapeDataString(query)}&format=json&limit=1";

            var json = await _httpClient.GetStringAsync(url);
            var results = JsonSerializer.Deserialize<List<NominatimResult>>(json);

            if (results != null && results.Count > 0)
            {
                var result = results[0];
                var lat = double.Parse(result.Lat);
                var lon = double.Parse(result.Lon);
                
                Console.WriteLine($"✓ Found: {result.DisplayName}");
                Console.WriteLine($"✓ Coordinates: {lat}, {lon}");
                return new GeoCoordinate(lat, lon);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Error geocoding: {ex.Message}");
        }

        throw new InvalidOperationException($"Could not find coordinates for {city}, {country}");
    }

    private class NominatimResult
    {
        [JsonPropertyName("lat")]
        public string Lat { get; set; } = "0";

        [JsonPropertyName("lon")]
        public string Lon { get; set; } = "0";

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;
    }
}
