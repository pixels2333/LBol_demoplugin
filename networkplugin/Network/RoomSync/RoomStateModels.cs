using System.Collections.Generic;
using NetworkPlugin.Network.Snapshot;

namespace NetworkPlugin.Network.RoomSync;

public enum RoomPhase
{
        NotVisited,
        InBattle,
        BattleFinished,
}

public sealed class RoomStateSnapshot
{
        public string RoomKey { get; set; } = string.Empty;

        public long RoomVersion { get; set; } = 0;

        public RoomPhase Phase { get; set; } = RoomPhase.NotVisited;

        public string OwnerPlayerId { get; set; } = string.Empty;

        public long UpdatedAtUtcTicks { get; set; } = 0;

        public int Act { get; set; } = 0;

        public int X { get; set; } = 0;

        public int Y { get; set; } = 0;

        public string StationType { get; set; } = string.Empty;

        public string BattleId { get; set; } = string.Empty;

        public List<EnemyStateSnapshot> Enemies { get; set; } = [];

        public BattleRewardSnapshot Rewards { get; set; } = new();

        public List<GapOptionsEventSnapshot> GapOptionsEvents { get; set; } = [];
}

public sealed class GapOptionsEventSnapshot
{
        public string ActionId { get; set; } = string.Empty;

        public string EventType { get; set; } = string.Empty;

        public string PlayerId { get; set; } = string.Empty;

        public string CardId { get; set; } = string.Empty;

        public string CardName { get; set; } = string.Empty;

        public string RoomKey { get; set; } = string.Empty;

        public long TimestampUtcTicks { get; set; } = 0;
}
