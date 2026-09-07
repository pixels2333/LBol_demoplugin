namespace NetworkPlugin.Network.Snapshot;

public class LocationSnapshot
{
        public int X { get; set; } = 0;

        public int Y { get; set; } = 0;

        public string NodeId { get; set; } = "";

        public string NodeType { get; set; } = "Unknown";

        public bool IsBranch { get; set; } = false;

        public long VisitTime { get; set; } = 0;

        public override string ToString()
    {
        return $"Location({X}, {Y}): {NodeType}";
    }
}
