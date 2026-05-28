using System;
using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot
{
    /// <summary>
    /// 玩家状态快照（用于断线重连与状态同步）。
    /// </summary>
    public class PlayerStateSnapshot
    {
        /// <summary>玩家唯一ID</summary>
        public string PlayerId { get; set; } = string.Empty;
        /// <summary>玩家显示名称</summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>当前生命值</summary>
        public int Health { get; set; }
        /// <summary>最大生命值</summary>
        public int MaxHealth { get; set; }
        /// <summary>当前格挡值</summary>
        public int Block { get; set; }
        /// <summary>当前护盾值</summary>
        public int Shield { get; set; }

        /// <summary>法力分组（4个槽位）</summary>
        public int[] ManaGroup { get; set; } = [0, 0, 0, 0];
        /// <summary>最大法力值</summary>
        public int MaxMana { get; set; }
        /// <summary>当前金币数量</summary>
        public int Gold { get; set; }

        /// <summary>手牌卡牌快照列表</summary>
        public List<CardStateSnapshot> Cards { get; set; } = [];
        /// <summary>遗物快照列表</summary>
        public List<ExhibitStateSnapshot> Exhibits { get; set; } = [];
        /// <summary>工具牌ID到数量的映射</summary>
        public Dictionary<string, int> ToolCards { get; set; } = [];
        /// <summary>状态效果快照列表</summary>
        public List<StatusEffectStateSnapshot> StatusEffects { get; set; } = [];

        /// <summary>当前游戏位置</summary>
        public LocationSnapshot GameLocation { get; set; } = new LocationSnapshot();

        /// <summary>是否在战斗中</summary>
        public bool IsInBattle { get; set; }
        /// <summary>是否存活</summary>
        public bool IsAlive { get; set; } = true;
        /// <summary>是否轮到该玩家的回合</summary>
        public bool IsPlayersTurn { get; set; }
        /// <summary>是否在回合中</summary>
        public bool IsInTurn { get; set; }
        /// <summary>是否为额外回合</summary>
        public bool IsExtraTurn { get; set; }

        /// <summary>角色类型标识</summary>
        public string CharacterType { get; set; } = string.Empty;
        /// <summary>重连令牌</summary>
        public string ReconnectToken { get; set; } = string.Empty;

        /// <summary>断线时间（UTC刻度）</summary>
        public long DisconnectTime { get; set; }
        /// <summary>最后更新时间（UTC刻度）</summary>
        public long LastUpdateTime { get; set; }

        /// <summary>回合计数器</summary>
        public int TurnCounter { get; set; }
        /// <summary>回合编号</summary>
        public int TurnNumber { get; set; }

        /// <summary>快照时间戳</summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
