using System.CommandLine;
using System.CommandLine.Parsing;
using MapToPoster.Services;

namespace MapToPoster;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Handle no arguments or help request
        if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
        {
            PrintHelp();
            return 0;
        }

        // Handle list themes
        if (args.Contains("--list-themes"))
        {
            ThemeService.ListThemes();
            return 0;
        }

        // Parse arguments manually
        string? city = GetArgValue(args, "--city", "-c");
        string? country = GetArgValue(args, "--country", "-C");
        string theme = GetArgValue(args, "--theme", "-t") ?? "feature_based";
        string distanceStr = GetArgValue(args, "--distance", "-d") ?? "29000";

        if (!int.TryParse(distanceStr, out int distance))
        {
            distance = 29000;
        }

        // Validate required arguments
        if (string.IsNullOrEmpty(city) || string.IsNullOrEmpty(country))
        {
            Console.WriteLine("Error: --city and --country are required.\n");
            PrintHelp();
            return 1;
        }

        try
        {
            // Validate theme exists
            var availableThemes = ThemeService.GetAvailableThemes();
            if (!availableThemes.Contains(theme))
            {
                Console.WriteLine($"Error: Theme '{theme}' not found.");
                Console.WriteLine($"Available themes: {string.Join(", ", availableThemes)}");
                return 1;
            }

            Console.WriteLine(new string('=', 50));
            Console.WriteLine("City Map Poster Generator");
            Console.WriteLine(new string('=', 50));

            // Load theme
            var themeObj = ThemeService.LoadTheme(theme);

            // Get coordinates
            var coords = await GeocodingService.GetCoordinatesAsync(city, country);
            if (coords == null)
            {
                Console.WriteLine("Failed to get coordinates");
                return 1;
            }

            // Fetch OSM data
            var osmData = await OsmDataService.FetchDataAsync(coords, distance);

            // Generate output filename
            var outputFile = MapRenderer.GenerateOutputFilename(city, theme);

            // Render poster
            MapRenderer.RenderPoster(osmData, themeObj, city, country, coords, outputFile);

            Console.WriteLine();
            Console.WriteLine(new string('=', 50));
            Console.WriteLine("✓ Poster generation complete!");
            Console.WriteLine(new string('=', 50));

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    static string? GetArgValue(string[] args, params string[] names)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (names.Contains(args[i]))
            {
                return args[i + 1];
            }
        }
        return null;
    }

    static void PrintHelp()
    {
        Console.WriteLine(@"
City Map Poster Generator
=========================

Usage:
  MapToPoster --city <city> --country <country> [options]

Options:
  --city, -c        City name (required)
  --country, -C     Country name (required)
  --theme, -t       Theme name (default: feature_based)
  --distance, -d    Map radius in meters (default: 29000)
  --list-themes     List all available themes
  --help, -h        Show this help message

Examples:
  MapToPoster -c ""New York"" -C ""USA"" -t noir -d 12000
  MapToPoster -c Venice -C Italy -t blueprint -d 4000
  MapToPoster -c Paris -C France -t pastel_dream -d 10000
  MapToPoster --list-themes

Distance guide:
  4000-6000m   Small/dense cities (Venice, Amsterdam)
  8000-12000m  Medium cities (Paris, Barcelona)
  15000-20000m Large metros (Tokyo, Mumbai)
");
    }
}
