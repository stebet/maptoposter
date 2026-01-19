using System.Text.Json;
using System.Text.Json.Serialization;
using MapToPoster.Models;

namespace MapToPoster.Services;

public class ThemeService
{
    private const string ThemesDir = "themes";

    public static Theme LoadTheme(string themeName = "feature_based")
    {
        var themeFile = Path.Combine(ThemesDir, $"{themeName}.json");

        if (!File.Exists(themeFile))
        {
            Console.WriteLine($"⚠ Theme file '{themeFile}' not found. Using default feature_based theme.");
            return GetDefaultTheme();
        }

        try
        {
            var json = File.ReadAllText(themeFile);
            var theme = JsonSerializer.Deserialize<ThemeJson>(json);
            if (theme != null)
            {
                Console.WriteLine($"✓ Loaded theme: {theme.Name}");
                if (!string.IsNullOrEmpty(theme.Description))
                {
                    Console.WriteLine($"  {theme.Description}");
                }
                return new Theme
                {
                    Name = theme.Name,
                    Description = theme.Description,
                    Bg = theme.Bg,
                    Text = theme.Text,
                    GradientColor = theme.GradientColor,
                    Water = theme.Water,
                    Parks = theme.Parks,
                    RoadMotorway = theme.RoadMotorway,
                    RoadPrimary = theme.RoadPrimary,
                    RoadSecondary = theme.RoadSecondary,
                    RoadTertiary = theme.RoadTertiary,
                    RoadResidential = theme.RoadResidential,
                    RoadDefault = theme.RoadDefault
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Error loading theme: {ex.Message}");
        }

        return GetDefaultTheme();
    }

    public static List<string> GetAvailableThemes()
    {
        if (!Directory.Exists(ThemesDir))
        {
            Directory.CreateDirectory(ThemesDir);
            return new List<string>();
        }

        return Directory.GetFiles(ThemesDir, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrEmpty(name))
            .OrderBy(name => name)
            .ToList()!;
    }

    public static void ListThemes()
    {
        var themes = GetAvailableThemes();
        if (themes.Count == 0)
        {
            Console.WriteLine("No themes found in 'themes/' directory.");
            return;
        }

        Console.WriteLine("\nAvailable Themes:");
        Console.WriteLine(new string('-', 60));

        foreach (var themeName in themes)
        {
            var theme = LoadTheme(themeName);
            Console.WriteLine($"  {themeName}");
            Console.WriteLine($"    {theme.Name}");
            if (!string.IsNullOrEmpty(theme.Description))
            {
                Console.WriteLine($"    {theme.Description}");
            }
            Console.WriteLine();
        }
    }

    private static Theme GetDefaultTheme()
    {
        return new Theme
        {
            Name = "Feature-Based Shading",
            Bg = "#FFFFFF",
            Text = "#000000",
            GradientColor = "#FFFFFF",
            Water = "#C0C0C0",
            Parks = "#F0F0F0",
            RoadMotorway = "#0A0A0A",
            RoadPrimary = "#1A1A1A",
            RoadSecondary = "#2A2A2A",
            RoadTertiary = "#3A3A3A",
            RoadResidential = "#4A4A4A",
            RoadDefault = "#3A3A3A"
        };
    }

    // Helper class for JSON deserialization with snake_case properties
    private class ThemeJson
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("bg")]
        public string Bg { get; set; } = "#FFFFFF";

        [JsonPropertyName("text")]
        public string Text { get; set; } = "#000000";

        [JsonPropertyName("gradient_color")]
        public string GradientColor { get; set; } = "#FFFFFF";

        [JsonPropertyName("water")]
        public string Water { get; set; } = "#C0C0C0";

        [JsonPropertyName("parks")]
        public string Parks { get; set; } = "#F0F0F0";

        [JsonPropertyName("road_motorway")]
        public string RoadMotorway { get; set; } = "#0A0A0A";

        [JsonPropertyName("road_primary")]
        public string RoadPrimary { get; set; } = "#1A1A1A";

        [JsonPropertyName("road_secondary")]
        public string RoadSecondary { get; set; } = "#2A2A2A";

        [JsonPropertyName("road_tertiary")]
        public string RoadTertiary { get; set; } = "#3A3A3A";

        [JsonPropertyName("road_residential")]
        public string RoadResidential { get; set; } = "#4A4A4A";

        [JsonPropertyName("road_default")]
        public string RoadDefault { get; set; } = "#3A3A3A";
    }
}
