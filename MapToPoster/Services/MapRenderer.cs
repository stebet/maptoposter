using SkiaSharp;
using MapToPoster.Models;

namespace MapToPoster.Services;

public class MapRenderer
{
    private const int Width = 1200;
    private const int Height = 1600;
    private const int Dpi = 300;
    private const string FontsDir = "fonts";

    public static void RenderPoster(
        OsmData osmData,
        Theme theme,
        string city,
        string country,
        GeoCoordinate coordinates,
        string outputPath)
    {
        Console.WriteLine("Rendering map...");

        // Create bitmap and canvas
        var imageInfo = new SKImageInfo(Width, Height);
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;

        // Parse background color
        var bgColor = ParseColor(theme.Bg);
        canvas.Clear(bgColor);

        // Calculate bounds from OSM data
        var (minLat, minLon, maxLat, maxLon) = CalculateBounds(osmData);
        
        if (minLat == double.MaxValue || minLon == double.MaxValue)
        {
            Console.WriteLine("⚠ No valid geographical data to render");
            minLat = coordinates.Latitude - 0.1;
            maxLat = coordinates.Latitude + 0.1;
            minLon = coordinates.Longitude - 0.1;
            maxLon = coordinates.Longitude + 0.1;
        }

        var latRange = maxLat - minLat;
        var lonRange = maxLon - minLon;

        // Transform functions
        float TransformX(double lon) => (float)((lon - minLon) / lonRange * Width);
        float TransformY(double lat) => (float)(Height - (lat - minLat) / latRange * Height);

        // Create node lookup for ways
        var nodeDict = osmData.Nodes.ToDictionary(n => n.Id);

        // Layer 1: Water polygons
        RenderPolygons(canvas, osmData, nodeDict, "natural", "water", theme.Water, TransformX, TransformY);
        RenderPolygons(canvas, osmData, nodeDict, "waterway", "riverbank", theme.Water, TransformX, TransformY);

        // Layer 2: Parks polygons
        RenderPolygons(canvas, osmData, nodeDict, "leisure", "park", theme.Parks, TransformX, TransformY);
        RenderPolygons(canvas, osmData, nodeDict, "landuse", "grass", theme.Parks, TransformX, TransformY);

        // Layer 3: Roads with hierarchy
        RenderRoads(canvas, osmData, nodeDict, theme, TransformX, TransformY);

        // Layer 4: Gradient fades
        RenderGradient(canvas, theme.GradientColor, isTop: false);
        RenderGradient(canvas, theme.GradientColor, isTop: true);

        // Layer 5: Typography
        RenderText(canvas, city, country, coordinates, theme);

        // Save image
        Console.WriteLine($"Saving to {outputPath}...");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "posters");

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(outputPath);
        data.SaveTo(stream);

        Console.WriteLine($"✓ Done! Poster saved as {outputPath}");
    }

    private static void RenderPolygons(
        SKCanvas canvas,
        OsmData osmData,
        Dictionary<long, OsmNode> nodeDict,
        string tagKey,
        string tagValue,
        string colorHex,
        Func<double, float> transformX,
        Func<double, float> transformY)
    {
        var color = ParseColor(colorHex);
        using var paint = new SKPaint
        {
            Color = color,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        foreach (var way in osmData.Ways)
        {
            if (way.Tags.TryGetValue(tagKey, out var value) && value == tagValue && way.Nodes.Count > 2)
            {
                using var path = new SKPath();
                bool first = true;

                foreach (var nodeId in way.Nodes)
                {
                    if (nodeDict.TryGetValue(nodeId, out var node))
                    {
                        var x = transformX(node.Lon);
                        var y = transformY(node.Lat);

                        if (first)
                        {
                            path.MoveTo(x, y);
                            first = false;
                        }
                        else
                        {
                            path.LineTo(x, y);
                        }
                    }
                }

                path.Close();
                canvas.DrawPath(path, paint);
            }
        }
    }

    private static void RenderRoads(
        SKCanvas canvas,
        OsmData osmData,
        Dictionary<long, OsmNode> nodeDict,
        Theme theme,
        Func<double, float> transformX,
        Func<double, float> transformY)
    {
        Console.WriteLine("Applying road hierarchy colors...");

        foreach (var way in osmData.Ways)
        {
            if (way.Tags.TryGetValue("highway", out var highway) && way.Nodes.Count > 1)
            {
                var (color, width) = GetRoadStyle(highway, theme);

                using var paint = new SKPaint
                {
                    Color = ParseColor(color),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = width,
                    IsAntialias = true,
                    StrokeCap = SKStrokeCap.Round
                };

                using var path = new SKPath();
                bool first = true;

                foreach (var nodeId in way.Nodes)
                {
                    if (nodeDict.TryGetValue(nodeId, out var node))
                    {
                        var x = transformX(node.Lon);
                        var y = transformY(node.Lat);

                        if (first)
                        {
                            path.MoveTo(x, y);
                            first = false;
                        }
                        else
                        {
                            path.LineTo(x, y);
                        }
                    }
                }

                canvas.DrawPath(path, paint);
            }
        }
    }

    private static (string color, float width) GetRoadStyle(string highway, Theme theme)
    {
        return highway switch
        {
            "motorway" or "motorway_link" => (theme.RoadMotorway, 1.2f),
            "trunk" or "trunk_link" or "primary" or "primary_link" => (theme.RoadPrimary, 1.0f),
            "secondary" or "secondary_link" => (theme.RoadSecondary, 0.8f),
            "tertiary" or "tertiary_link" => (theme.RoadTertiary, 0.6f),
            "residential" or "living_street" or "unclassified" => (theme.RoadResidential, 0.4f),
            _ => (theme.RoadDefault, 0.4f)
        };
    }

    private static void RenderGradient(SKCanvas canvas, string colorHex, bool isTop)
    {
        var color = ParseColor(colorHex);
        var colors = isTop
            ? new[] { SKColors.Transparent, color }
            : new[] { color, SKColors.Transparent };

        var startY = isTop ? Height * 0.75f : 0;
        var endY = isTop ? Height : Height * 0.25f;

        using var shader = SKShader.CreateLinearGradient(
            new SKPoint(0, startY),
            new SKPoint(0, endY),
            colors,
            null,
            SKShaderTileMode.Clamp);

        using var paint = new SKPaint
        {
            Shader = shader,
            Style = SKPaintStyle.Fill
        };

        canvas.DrawRect(0, startY, Width, endY - startY, paint);
    }

    private static void RenderText(
        SKCanvas canvas,
        string city,
        string country,
        GeoCoordinate coordinates,
        Theme theme)
    {
        var textColor = ParseColor(theme.Text);

        // Try to load custom font
        SKTypeface? typefaceBold = null;
        SKTypeface? typefaceLight = null;
        SKTypeface? typefaceRegular = null;

        var boldPath = Path.Combine(FontsDir, "Roboto-Bold.ttf");
        var lightPath = Path.Combine(FontsDir, "Roboto-Light.ttf");
        var regularPath = Path.Combine(FontsDir, "Roboto-Regular.ttf");

        if (File.Exists(boldPath))
            typefaceBold = SKTypeface.FromFile(boldPath);
        if (File.Exists(lightPath))
            typefaceLight = SKTypeface.FromFile(lightPath);
        if (File.Exists(regularPath))
            typefaceRegular = SKTypeface.FromFile(regularPath);

        // City name (spaced)
        var spacedCity = string.Join("  ", city.ToUpper().ToCharArray());
        using (var paint = new SKPaint
        {
            Color = textColor,
            TextSize = 60,
            Typeface = typefaceBold ?? SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold),
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        })
        {
            canvas.DrawText(spacedCity, Width / 2, Height * 0.86f, paint);
        }

        // Decorative line
        using (var paint = new SKPaint
        {
            Color = textColor,
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke
        })
        {
            canvas.DrawLine(Width * 0.4f, Height * 0.875f, Width * 0.6f, Height * 0.875f, paint);
        }

        // Country
        using (var paint = new SKPaint
        {
            Color = textColor,
            TextSize = 22,
            Typeface = typefaceLight ?? SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal),
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        })
        {
            canvas.DrawText(country.ToUpper(), Width / 2, Height * 0.90f, paint);
        }

        // Coordinates
        using (var paint = new SKPaint
        {
            Color = textColor.WithAlpha(179), // 0.7 alpha
            TextSize = 14,
            Typeface = typefaceRegular ?? SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal),
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        })
        {
            canvas.DrawText(coordinates.ToString(), Width / 2, Height * 0.93f, paint);
        }

        // Attribution
        using (var paint = new SKPaint
        {
            Color = textColor.WithAlpha(128), // 0.5 alpha
            TextSize = 8,
            Typeface = typefaceLight ?? SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal),
            IsAntialias = true,
            TextAlign = SKTextAlign.Right
        })
        {
            canvas.DrawText("© OpenStreetMap contributors", Width * 0.98f, Height * 0.98f, paint);
        }

        // Cleanup
        typefaceBold?.Dispose();
        typefaceLight?.Dispose();
        typefaceRegular?.Dispose();
    }

    private static (double minLat, double minLon, double maxLat, double maxLon) CalculateBounds(OsmData osmData)
    {
        if (osmData.Nodes.Count == 0)
            return (double.MaxValue, double.MaxValue, double.MinValue, double.MinValue);

        var minLat = osmData.Nodes.Min(n => n.Lat);
        var maxLat = osmData.Nodes.Max(n => n.Lat);
        var minLon = osmData.Nodes.Min(n => n.Lon);
        var maxLon = osmData.Nodes.Max(n => n.Lon);

        return (minLat, minLon, maxLat, maxLon);
    }

    private static SKColor ParseColor(string hex)
    {
        if (string.IsNullOrEmpty(hex) || !hex.StartsWith('#'))
            return SKColors.White;

        try
        {
            return SKColor.Parse(hex);
        }
        catch
        {
            return SKColors.White;
        }
    }

    public static string GenerateOutputFilename(string city, string themeName)
    {
        const string postersDir = "posters";
        Directory.CreateDirectory(postersDir);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var citySlug = city.ToLower().Replace(' ', '_');
        var filename = $"{citySlug}_{themeName}_{timestamp}.png";

        return Path.Combine(postersDir, filename);
    }
}
