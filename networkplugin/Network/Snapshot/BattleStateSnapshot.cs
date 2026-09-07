using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

public class BattleStateSnapshot
{
        public bool IsInBattle { get; set; } = false;

        public string BattleId { get; set; } = "";

        public int CurrentTurn { get; set; } = 1;

        public string CurrentTurnPlayerId { get; set; } = "unknown";

        public string TurnPhase { get; set; } = "Player";

        public List<EnemyStateSnapshot> Enemies { get; set; } = [];

        public long BattleStartTime { get; set; } = 0;

        public bool IsBossBattle { get; set; } = false;

        public BattleRewardSnapshot Rewards { get; set; } = new();

        public string BattleType { get; set; } = "Normal";
}
