using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

public class MapStateSnapshot
{
        public int MapSeed { get; set; } = 0;

        public ulong? MapSeedUlong { get; set; }

        public List<string> VisitedNodes { get; set; } = [];

        public Dictionary<string, string> NodeStates { get; set; } = [];

        public List<string> ClearedNodes { get; set; } = [];

        public string LastCheckpointId { get; set; } = "";

        public long LastCheckpointAtUtcTicks { get; set; } = 0;

        public LocationSnapshot CurrentLocation { get; set; } = new();

        public List<string> RevealedNodes { get; set; } = [];

        public List<LocationSnapshot> PathHistory { get; set; } = [];

        public bool MapComplete { get; set; } = false;
}
