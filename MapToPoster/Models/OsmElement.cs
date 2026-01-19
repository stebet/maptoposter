namespace MapToPoster.Models;

public class OsmNode
{
    public long Id { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
    public Dictionary<string, string> Tags { get; set; } = new();
}

public class OsmWay
{
    public long Id { get; set; }
    public List<long> Nodes { get; set; } = new();
    public Dictionary<string, string> Tags { get; set; } = new();
}

public class OsmRelation
{
    public long Id { get; set; }
    public List<OsmMember> Members { get; set; } = new();
    public Dictionary<string, string> Tags { get; set; } = new();
}

public class OsmMember
{
    public string Type { get; set; } = string.Empty;
    public long Ref { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class OsmData
{
    public List<OsmNode> Nodes { get; set; } = new();
    public List<OsmWay> Ways { get; set; } = new();
    public List<OsmRelation> Relations { get; set; } = new();
}
