using System;
using System.Collections.Generic;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace NetworkPlugin.Events
{
    /// <summary>
    /// 游戏事件类型枚举
    /// </summary>
    public enum GameEventType
    {
        // 卡牌相关事件
        CardPlayStart,
        CardPlayComplete,
        CardDraw,
        CardDiscard,
        CardExile,
        CardUpgrade,
        CardRemove,

        // 法力相关事件
        ManaConsumeStart,
        ManaConsumeComplete,
        ManaRegain,
        TurnManaReset,

        // 战斗相关事件
        DamageDealt,
        DamageReceived,
        BlockGained,
        ShieldGained,
        HealingReceived,
        StatusEffectApplied,
        StatusEffectRemoved,

        // 回合相关事件
        TurnStart,
        TurnEnd,
        BattleStart,
        BattleEnd,
        RoundStart,
        RoundEnd,

        // 地图/节点相关事件
        MapNodeEnter,
        MapNodeComplete,
        GapStationEnter,
        GapOptionSelected,

        // 物品/道具相关事件
        ExhibitObtained,
        ExhibitRemoved,
        PotionUsed,

        // 玩家相关事件
        PlayerJoin,
        PlayerLeave,
        PlayerReady,
        PlayerStatusUpdate,

        // 网络相关事件
        ConnectionEstablished,
        ConnectionLost,
        ReconnectionAttempt,
        StateSyncRequest,
        StateSyncComplete,

        // 系统相关事件
        GameStart,
        GameEnd,
        SaveGame,
        LoadGame,
        Error
    }

    /// <summary>
    /// 游戏事件基类
    /// </summary>
    public abstract class GameEvent(GameEventType eventType, string sourcePlayerId, object data = null)
    {
        public GameEventType EventType { get; protected set; } = eventType;
        public DateTime Timestamp { get; protected set; } = DateTime.Now;
        public string SourcePlayerId { get; protected set; } = sourcePlayerId ?? "unknown";
        public object Data { get; protected set; } = data;

        /// <summary>
        /// 将事件转换为网络传输格式
        /// </summary>
        /// <returns>事件数据对象</returns>
        public abstract object ToNetworkData();

        /// <summary>
        /// 从网络数据创建事件实例
        /// </summary>
        /// <param name="data">网络数据</param>
        /// <returns>事件实例</returns>
        public abstract GameEvent FromNetworkData(object data);
    }

    /// <summary>
    /// 卡牌使用事件
    /// </summary>
    public class CardPlayEvent(string sourcePlayerId, string cardId, string cardName, string cardType,
        int[] manaCost, string targetSelector, object additionalData = null) : GameEvent(GameEventType.CardPlayStart, sourcePlayerId, additionalData)
    {
        public string CardId { get; private set; } = cardId;
        public string CardName { get; private set; } = cardName;
        public string CardType { get; private set; } = cardType;
        public int[] ManaCost { get; private set; } = manaCost ?? [0, 0, 0, 0];
        public string TargetSelector { get; private set; } = targetSelector ?? "Nobody";

        public override object ToNetworkData()
        {
            return new
            {
                EventType = EventType.ToString(),
                Timestamp = Timestamp.Ticks,
                SourcePlayerId,
                CardId,
                CardName,
                CardType,
                ManaCost,
                TargetSelector,
                Data
            };
        }

        public override GameEvent FromNetworkData(object data)
        {
            // TODO: 实现从网络数据重建事件
            return this;
        }
    }

    /// <summary>
    /// 法力消耗事件
    /// </summary>
    public class ManaConsumeEvent(string sourcePlayerId, int[] manaBefore, int[] manaConsumed,
        string source, object additionalData = null) : GameEvent(GameEventType.ManaConsumeStart, sourcePlayerId, additionalData)
    {
        public int[] ManaBefore { get; private set; } = manaBefore ?? [0, 0, 0, 0];
        public int[] ManaConsumed { get; private set; } = manaConsumed ?? [0, 0, 0, 0];
        public string Source { get; private set; } = source ?? "Unknown";

        public override object ToNetworkData()
        {
            return new
            {
                EventType = EventType.ToString(),
                Timestamp = Timestamp.Ticks,
                SourcePlayerId,
                ManaBefore,
                ManaConsumed,
                Source,
                Data
            };
        }

        public override GameEvent FromNetworkData(object data)
        {
            // TODO: 实现从网络数据重建事件
            return this;
        }
    }

    /// <summary>
    /// 伤害事件
    /// </summary>
    public class DamageEvent(string sourcePlayerId, string sourceId, string targetId,
        int damageAmount, string damageType, object additionalData = null) : GameEvent(GameEventType.DamageDealt, sourcePlayerId, additionalData)
    {
        public string SourceId { get; private set; } = sourceId;
        public string TargetId { get; private set; } = targetId;
        public int DamageAmount { get; private set; } = damageAmount;
        public string DamageType { get; private set; } = damageType ?? "Unknown";

        public override object ToNetworkData()
        {
            return new
            {
                EventType = EventType.ToString(),
                Timestamp = Timestamp.Ticks,
                SourcePlayerId,
                SourceId,
                TargetId,
                DamageAmount,
                DamageType,
                Data
            };
        }

        public override GameEvent FromNetworkData(object data)
        {
            // TODO: 实现从网络数据重建事件
            return this;
        }
    }

    /// <summary>
    /// 事件工厂类 - 用于创建各种游戏事件
    /// </summary>
    public static class GameEventFactory
    {
        /// <summary>
        /// 创建卡牌使用事件
        /// </summary>
        public static CardPlayEvent CreateCardPlayEvent(string playerId, string cardId, string cardName,
            string cardType, int[] manaCost, string targetSelector)
        {
            return new CardPlayEvent(playerId, cardId, cardName, cardType, manaCost, targetSelector);
        }

        /// <summary>
        /// 创建法力消耗事件
        /// </summary>
        public static ManaConsumeEvent CreateManaConsumeEvent(string playerId, int[] manaBefore,
            int[] manaConsumed, string source)
        {
            return new ManaConsumeEvent(playerId, manaBefore, manaConsumed, source);
        }

        /// <summary>
        /// 创建伤害事件
        /// </summary>
        public static DamageEvent CreateDamageEvent(string playerId, string sourceId, string targetId,
            int damageAmount, string damageType)
        {
            return new DamageEvent(playerId, sourceId, targetId, damageAmount, damageType);
        }

        /// <summary>
        /// 从网络数据创建事件
        /// </summary>
        public static GameEvent CreateEventFromNetworkData(object data)
        {
            // TODO: 实现从网络数据创建相应事件的逻辑
            return null;
        }
    }
}