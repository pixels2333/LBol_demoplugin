using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

public class GameStateSnapshot
{
        public string GamePhase { get; set; } = "Unknown";

        public int CurrentAct { get; set; } = 1;

        public int CurrentFloor { get; set; } = 0;

        public bool GameStarted { get; set; } = false;

        public bool GameEnded { get; set; } = false;

        public string? GameResult { get; set; }

        public string? ActivePlayerId { get; set; }

        public int TurnCount { get; set; } = 0;

        public int GameSeed { get; set; } = 0;

        public ulong? RootSeed { get; set; }

        public ulong? UISeed { get; set; }

        public int? StageIndex { get; set; }

        public int? Difficulty { get; set; }

        public int? Puzzles { get; set; }

        public int? GameMode { get; set; }

        public bool? ShowRandomResult { get; set; }

        public List<string> StageTypeNames { get; set; } = [];

        public string? DebutAdventureTypeName { get; set; }

        public string RoomId { get; set; } = "unknown";

        public string HostPlayerId { get; set; } = "unknown";
}
