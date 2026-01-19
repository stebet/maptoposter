using System.Net.Http.Json;
using System.Text.Json;
using MapToPoster.Models;

namespace MapToPoster.Services;

public class OsmDataService
{
    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://overpass-api.de/api/"),
        Timeout = TimeSpan.FromMinutes(5)
    };

    public static async Task<OsmData> FetchDataAsync(GeoCoordinate center, int distanceMeters)
    {
        Console.WriteLine($"\nGenerating map for coordinates: {center}");

        var osmData = new OsmData();

        // Calculate bounding box
        var (south, west, north, east) = CalculateBoundingBox(center, distanceMeters);

        // Fetch different types of data with progress updates
        Console.WriteLine("Fetching map data:");

        Console.Write("  Downloading street network... ");
        var roads = await FetchRoadsAsync(south, west, north, east);
        osmData.Ways.AddRange(roads.Ways);
        osmData.Nodes.AddRange(roads.Nodes);
        Console.WriteLine("✓");

        await Task.Delay(500); // Rate limiting

        Console.Write("  Downloading water features... ");
        var water = await FetchWaterAsync(south, west, north, east);
        osmData.Ways.AddRange(water.Ways);
        osmData.Nodes.AddRange(water.Nodes);
        osmData.Relations.AddRange(water.Relations);
        Console.WriteLine("✓");

        await Task.Delay(300); // Rate limiting

        Console.Write("  Downloading parks/green spaces... ");
        var parks = await FetchParksAsync(south, west, north, east);
        osmData.Ways.AddRange(parks.Ways);
        osmData.Nodes.AddRange(parks.Nodes);
        osmData.Relations.AddRange(parks.Relations);
        Console.WriteLine("✓");

        Console.WriteLine("✓ All data downloaded successfully!");

        return osmData;
    }

    private static async Task<OsmData> FetchRoadsAsync(double south, double west, double north, double east)
    {
        var query = $@"
[out:json][timeout:90];
(
  way[""highway""]({south},{west},{north},{east});
);
out body;
>;
out skel qt;
";
        return await ExecuteOverpassQueryAsync(query);
    }

    private static async Task<OsmData> FetchWaterAsync(double south, double west, double north, double east)
    {
        var query = $@"
[out:json][timeout:90];
(
  way[""natural""=""water""]({south},{west},{north},{east});
  way[""waterway""=""riverbank""]({south},{west},{north},{east});
  relation[""natural""=""water""]({south},{west},{north},{east});
);
out body;
>;
out skel qt;
";
        return await ExecuteOverpassQueryAsync(query);
    }

    private static async Task<OsmData> FetchParksAsync(double south, double west, double north, double east)
    {
        var query = $@"
[out:json][timeout:90];
(
  way[""leisure""=""park""]({south},{west},{north},{east});
  way[""landuse""=""grass""]({south},{west},{north},{east});
  relation[""leisure""=""park""]({south},{west},{north},{east});
);
out body;
>;
out skel qt;
";
        return await ExecuteOverpassQueryAsync(query);
    }

    private static async Task<OsmData> ExecuteOverpassQueryAsync(string query)
    {
        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "data", query }
            });

            var response = await _httpClient.PostAsync("interpreter", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return ParseOverpassJson(json);
        }
        catch (Exception)
        {
            // Return empty data on error
            return new OsmData();
        }
    }

    private static OsmData ParseOverpassJson(string json)
    {
        var osmData = new OsmData();
        
        try
        {
            using var doc = JsonDocument.Parse(json);
            var elements = doc.RootElement.GetProperty("elements");

            foreach (var element in elements.EnumerateArray())
            {
                var type = element.GetProperty("type").GetString();

                if (type == "node")
                {
                    var node = new OsmNode
                    {
                        Id = element.GetProperty("id").GetInt64(),
                        Lat = element.GetProperty("lat").GetDouble(),
                        Lon = element.GetProperty("lon").GetDouble()
                    };

                    if (element.TryGetProperty("tags", out var tags))
                    {
                        foreach (var tag in tags.EnumerateObject())
                        {
                            node.Tags[tag.Name] = tag.Value.GetString() ?? "";
                        }
                    }

                    osmData.Nodes.Add(node);
                }
                else if (type == "way")
                {
                    var way = new OsmWay
                    {
                        Id = element.GetProperty("id").GetInt64()
                    };

                    if (element.TryGetProperty("nodes", out var nodes))
                    {
                        foreach (var nodeId in nodes.EnumerateArray())
                        {
                            way.Nodes.Add(nodeId.GetInt64());
                        }
                    }

                    if (element.TryGetProperty("tags", out var tags))
                    {
                        foreach (var tag in tags.EnumerateObject())
                        {
                            way.Tags[tag.Name] = tag.Value.GetString() ?? "";
                        }
                    }

                    osmData.Ways.Add(way);
                }
                else if (type == "relation")
                {
                    var relation = new OsmRelation
                    {
                        Id = element.GetProperty("id").GetInt64()
                    };

                    if (element.TryGetProperty("members", out var members))
                    {
                        foreach (var member in members.EnumerateArray())
                        {
                            relation.Members.Add(new OsmMember
                            {
                                Type = member.GetProperty("type").GetString() ?? "",
                                Ref = member.GetProperty("ref").GetInt64(),
                                Role = member.TryGetProperty("role", out var role) 
                                    ? role.GetString() ?? "" 
                                    : ""
                            });
                        }
                    }

                    if (element.TryGetProperty("tags", out var tags))
                    {
                        foreach (var tag in tags.EnumerateObject())
                        {
                            relation.Tags[tag.Name] = tag.Value.GetString() ?? "";
                        }
                    }

                    osmData.Relations.Add(relation);
                }
            }
        }
        catch (Exception)
        {
            // Return partial data on parse error
        }

        return osmData;
    }

    private static (double south, double west, double north, double east) CalculateBoundingBox(
        GeoCoordinate center, int distanceMeters)
    {
        // Approximate degrees per meter at the given latitude
        const double metersPerDegreeLat = 111000.0;
        var metersPerDegreeLon = 111000.0 * Math.Cos(center.Latitude * Math.PI / 180.0);

        var deltaLat = distanceMeters / metersPerDegreeLat;
        var deltaLon = distanceMeters / metersPerDegreeLon;

        return (
            center.Latitude - deltaLat,
            center.Longitude - deltaLon,
            center.Latitude + deltaLat,
            center.Longitude + deltaLon
        );
    }
}
