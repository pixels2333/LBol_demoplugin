using System;
using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot
{
    /// <summary>
    /// 玩家状态快照（用于断线重连与状态同步）。
    /// </summary>
    public class PlayerStateSnapshot
    {
        public string PlayerId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Block { get; set; }
        public int Shield { get; set; }

        public int[] ManaGroup { get; set; } = [0, 0, 0, 0];
        public int MaxMana { get; set; }
        public int Gold { get; set; }

        public List<CardStateSnapshot> Cards { get; set; } = [];
        public List<ExhibitStateSnapshot> Exhibits { get; set; } = [];
        public Dictionary<string, int> Potions { get; set; } = [];
        public List<StatusEffectStateSnapshot> StatusEffects { get; set; } = [];

        public LocationSnapshot GameLocation { get; set; } = new LocationSnapshot();

        public bool IsInBattle { get; set; }
        public bool IsAlive { get; set; } = true;
        public bool IsPlayersTurn { get; set; }
        public bool IsInTurn { get; set; }
        public bool IsExtraTurn { get; set; }

        public string CharacterType { get; set; } = string.Empty;
        public string ReconnectToken { get; set; } = string.Empty;

        public long DisconnectTime { get; set; }
        public long LastUpdateTime { get; set; }

        public bool IsAIControlled { get; set; }

        public int TurnCounter { get; set; }
        public int TurnNumber { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

