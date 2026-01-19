namespace MapToPoster.Models;

public class Theme
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Bg { get; set; } = "#FFFFFF";
    public string Text { get; set; } = "#000000";
    public string GradientColor { get; set; } = "#FFFFFF";
    public string Water { get; set; } = "#C0C0C0";
    public string Parks { get; set; } = "#F0F0F0";
    public string RoadMotorway { get; set; } = "#0A0A0A";
    public string RoadPrimary { get; set; } = "#1A1A1A";
    public string RoadSecondary { get; set; } = "#2A2A2A";
    public string RoadTertiary { get; set; } = "#3A3A3A";
    public string RoadResidential { get; set; } = "#4A4A4A";
    public string RoadDefault { get; set; } = "#3A3A3A";
}
