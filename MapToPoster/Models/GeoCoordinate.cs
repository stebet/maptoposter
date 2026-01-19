namespace MapToPoster.Models;

public record GeoCoordinate(double Latitude, double Longitude)
{
    public override string ToString()
    {
        var latDir = Latitude >= 0 ? "N" : "S";
        var lonDir = Longitude >= 0 ? "E" : "W";
        return $"{Math.Abs(Latitude):F4}° {latDir} / {Math.Abs(Longitude):F4}° {lonDir}";
    }
}
