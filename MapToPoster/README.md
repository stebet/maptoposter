# City Map Poster Generator (C# Edition)

Generate beautiful, minimalist map posters for any city in the world using .NET 10.

<img src="posters/singapore_neon_cyberpunk_20260108_184503.png" width="250">
<img src="posters/dubai_midnight_blue_20260108_174920.png" width="250">

## Features

- **17 Beautiful Themes** - From minimalist noir to vibrant neon cyberpunk
- **OpenStreetMap Data** - Accurate, up-to-date map data for any city worldwide
- **Road Hierarchy** - Different styling for motorways, primary roads, and residential streets
- **Custom Fonts** - Roboto font family included for professional typography
- **High Resolution** - 300 DPI PNG output suitable for printing

## Requirements

- .NET 10 SDK
- Internet connection (for downloading map data)

## Installation

### Build from Source

```bash
cd MapToPoster
dotnet build
```

## Usage

```bash
dotnet run --project MapToPoster -- --city <city> --country <country> [options]
```

### Options

| Option | Short | Description | Default |
|--------|-------|-------------|---------|
| `--city` | `-c` | City name | required |
| `--country` | `-C` | Country name | required |
| `--theme` | `-t` | Theme name | feature_based |
| `--distance` | `-d` | Map radius in meters | 29000 |
| `--list-themes` | | List all available themes | |
| `--help` | `-h` | Show help message | |

### Examples

```bash
# Iconic grid patterns
dotnet run -- -c "New York" -C "USA" -t noir -d 12000           # Manhattan grid
dotnet run -- -c "Barcelona" -C "Spain" -t warm_beige -d 8000   # Eixample district

# Waterfront & canals
dotnet run -- -c "Venice" -C "Italy" -t blueprint -d 4000       # Canal network
dotnet run -- -c "Amsterdam" -C "Netherlands" -t ocean -d 6000  # Concentric canals
dotnet run -- -c "Dubai" -C "UAE" -t midnight_blue -d 15000     # Palm & coastline

# Radial patterns
dotnet run -- -c "Paris" -C "France" -t pastel_dream -d 10000   # Haussmann boulevards
dotnet run -- -c "Moscow" -C "Russia" -t noir -d 12000          # Ring roads

# Organic old cities
dotnet run -- -c "Tokyo" -C "Japan" -t japanese_ink -d 15000    # Dense organic streets
dotnet run -- -c "Marrakech" -C "Morocco" -t terracotta -d 5000 # Medina maze
dotnet run -- -c "Rome" -C "Italy" -t warm_beige -d 8000        # Ancient layout

# Coastal cities
dotnet run -- -c "San Francisco" -C "USA" -t sunset -d 10000    # Peninsula grid
dotnet run -- -c "Sydney" -C "Australia" -t ocean -d 12000      # Harbor city
dotnet run -- -c "Mumbai" -C "India" -t contrast_zones -d 18000 # Coastal peninsula

# River cities
dotnet run -- -c "London" -C "UK" -t noir -d 15000              # Thames curves
dotnet run -- -c "Budapest" -C "Hungary" -t copper_patina -d 8000  # Danube split

# List available themes
dotnet run -- --list-themes
```

### Publishing

To create a standalone executable:

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained

# macOS
dotnet publish -c Release -r osx-x64 --self-contained
```

The executable will be in `bin/Release/net10.0/<runtime>/publish/`

### Distance Guide

| Distance | Best for |
|----------|----------|
| 4000-6000m | Small/dense cities (Venice, Amsterdam center) |
| 8000-12000m | Medium cities, focused downtown (Paris, Barcelona) |
| 15000-20000m | Large metros, full city view (Tokyo, Mumbai) |

## Themes

17 themes available in `themes/` directory:

| Theme | Style |
|-------|-------|
| `feature_based` | Classic black & white with road hierarchy |
| `gradient_roads` | Smooth gradient shading |
| `contrast_zones` | High contrast urban density |
| `noir` | Pure black background, white roads |
| `midnight_blue` | Navy background with gold roads |
| `blueprint` | Architectural blueprint aesthetic |
| `neon_cyberpunk` | Dark with electric pink/cyan |
| `warm_beige` | Vintage sepia tones |
| `pastel_dream` | Soft muted pastels |
| `japanese_ink` | Minimalist ink wash style |
| `forest` | Deep greens and sage |
| `ocean` | Blues and teals for coastal cities |
| `terracotta` | Mediterranean warmth |
| `sunset` | Warm oranges and pinks |
| `autumn` | Seasonal burnt oranges and reds |
| `copper_patina` | Oxidized copper aesthetic |
| `monochrome_blue` | Single blue color family |

## Output

Posters are saved to `posters/` directory with format:
```
{city}_{theme}_{YYYYMMDD_HHMMSS}.png
```

## Adding Custom Themes

Create a JSON file in `themes/` directory:

```json
{
  "name": "My Theme",
  "description": "Description of the theme",
  "bg": "#FFFFFF",
  "text": "#000000",
  "gradient_color": "#FFFFFF",
  "water": "#C0C0C0",
  "parks": "#F0F0F0",
  "road_motorway": "#0A0A0A",
  "road_primary": "#1A1A1A",
  "road_secondary": "#2A2A2A",
  "road_tertiary": "#3A3A3A",
  "road_residential": "#4A4A4A",
  "road_default": "#3A3A3A"
}
```

## Project Structure

```
MapToPoster/
├── MapToPoster.csproj        # Project file
├── Program.cs                 # Main entry point with CLI
├── Models/
│   ├── Theme.cs              # Theme model
│   ├── GeoCoordinate.cs      # Coordinate record
│   └── OsmElement.cs         # OSM data models
├── Services/
│   ├── ThemeService.cs       # Theme loading and management
│   ├── GeocodingService.cs   # Nominatim geocoding
│   ├── OsmDataService.cs     # OSM Overpass API data fetching
│   └── MapRenderer.cs        # SkiaSharp rendering
├── themes/                    # Theme JSON files
├── fonts/                     # Roboto font files
└── posters/                   # Generated posters
```

## Technology Stack

- **.NET 10** - Latest .NET runtime
- **System.CommandLine** - Command-line argument parsing
- **SkiaSharp** - Cross-platform 2D graphics library
- **Nominatim** - OpenStreetMap geocoding service
- **Overpass API** - OpenStreetMap data query service

## Architecture

### Data Flow

```
CLI Parser → Geocoding → OSM Data Fetch → Rendering → PNG Output
```

### Services

1. **ThemeService** - Loads and manages color themes from JSON files
2. **GeocodingService** - Converts city names to coordinates using Nominatim
3. **OsmDataService** - Fetches street, water, and park data from Overpass API
4. **MapRenderer** - Renders the map using SkiaSharp with layered composition

### Rendering Layers (z-order)

```
z=11  Text labels (city, country, coords)
z=10  Gradient fades (top & bottom)
z=3   Roads (with hierarchy styling)
z=2   Parks (green polygons)
z=1   Water (blue polygons)
z=0   Background color
```

### OSM Highway Types → Road Hierarchy

```
motorway, motorway_link     → Thickest (1.2px), darkest
trunk, primary              → Thick (1.0px)
secondary                   → Medium (0.8px)
tertiary                    → Thin (0.6px)
residential, living_street  → Thinnest (0.4px), lightest
```

## Migration from Python

This project was originally written in Python. Key differences in the C# version:

### What's the Same
- Same 17 themes with identical JSON format
- Same command-line interface and options
- Same output format and quality
- Same OpenStreetMap data sources

### What's Different
- Uses SkiaSharp instead of matplotlib for rendering
- Uses System.CommandLine for CLI (simple manual parsing)
- Async/await for all I/O operations
- Strongly typed models instead of dictionaries
- Cross-platform native executables via dotnet publish

### Performance
- C# version is generally faster at rendering
- Similar network I/O times (both limited by OSM API)
- Lower memory usage due to compiled code

## Contributing

When adding new features:

1. **New map layer**: Add to `OsmDataService.cs` and `MapRenderer.cs`
2. **New theme property**: Update `Theme.cs` and `ThemeService.ThemeJson`
3. **New CLI option**: Add to `Program.cs` parsing logic

## License

See LICENSE file for details.

## Acknowledgments

- OpenStreetMap contributors for map data
- Nominatim for geocoding service
- Overpass API for OSM data queries
- SkiaSharp team for the graphics library
- Roboto font by Google Fonts
