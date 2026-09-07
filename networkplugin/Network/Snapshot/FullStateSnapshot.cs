using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

public class FullStateSnapshot
{
        public long Timestamp { get; set; }

        public GameStateSnapshot GameState { get; set; } = new();

        public List<PlayerStateSnapshot> PlayerStates { get; set; } = [];

        public BattleStateSnapshot? BattleState { get; set; }

        public MapStateSnapshot MapState { get; set; } = new();

        public long EventIndex { get; set; }

        public string GameVersion { get; set; } = "1.0.0";

        public string ModVersion { get; set; } = "1.0.0";
}
