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
        public GameEventType EventType { get; protected set; } = eventType;    // 事件的具体类型枚举值
        public DateTime Timestamp { get; protected set; } = DateTime.Now;      // 事件发生的时间戳
        public string SourcePlayerId { get; protected set; } = sourcePlayerId ?? "unknown";      // 触发事件的玩家ID
        public object Data { get; protected set; } = data;                      // 事件相关的附加数据

        /// <summary>
        /// 将事件转换为网络传输格式
        /// </summary>
        /// <returns>事件数据对象</returns>
        public abstract object ToNetworkData();     // 将事件序列化为网络传输数据格式

        /// <summary>
        /// 从网络数据创建事件实例
        /// </summary>
        /// <param name="data">网络数据</param>
        /// <returns>事件实例</returns>
        public abstract GameEvent FromNetworkData(object data);     // 从网络数据反序列化为事件对象
    }

    /// <summary>
    /// 卡牌使用事件
    /// </summary>
    public class CardPlayEvent(string sourcePlayerId, string cardId, string cardName, string cardType,
        int[] manaCost, string targetSelector, object additionalData = null) : GameEvent(GameEventType.CardPlayStart, sourcePlayerId, additionalData)
    {
        public string CardId { get; private set; } = cardId;                // 卡牌的唯一标识符
        public string CardName { get; private set; } = cardName;              // 卡牌的显示名称
        public string CardType { get; private set; } = cardType;              // 卡牌的类型分类
        public int[] ManaCost { get; private set; } = manaCost ?? [0, 0, 0, 0];      // 卡牌消耗的法力值数组
        public string TargetSelector { get; private set; } = targetSelector ?? "Nobody";  // 目标选择器名称

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
        }    // 将卡牌使用事件序列化为网络传输对象

        public override GameEvent FromNetworkData(object data)
        {
            // TODO: 实现从网络数据重建事件
            return this;
        }    // 从网络数据反序列化重建卡牌使用事件
    }

    /// <summary>
    /// 法力消耗事件
    /// </summary>
    public class ManaConsumeEvent(string sourcePlayerId, int[] manaBefore, int[] manaConsumed,
        string source, object additionalData = null) : GameEvent(GameEventType.ManaConsumeStart, sourcePlayerId, additionalData)
    {
        public int[] ManaBefore { get; private set; } = manaBefore ?? [0, 0, 0, 0];     // 消耗前的法力值数组
        public int[] ManaConsumed { get; private set; } = manaConsumed ?? [0, 0, 0, 0];     // 实际消耗的法力值数组
        public string Source { get; private set; } = source ?? "Unknown";               // 消耗法力的来源描述

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
        }    // 将法力消耗事件序列化为网络传输对象

        public override GameEvent FromNetworkData(object data)
        {
            // TODO: 实现从网络数据重建事件
            return this;
        }    // 从网络数据反序列化重建法力消耗事件
    }

    /// <summary>
    /// 伤害事件
    /// </summary>
    public class DamageEvent(string sourcePlayerId, string sourceId, string targetId,
        int damageAmount, string damageType, object additionalData = null) : GameEvent(GameEventType.DamageDealt, sourcePlayerId, additionalData)
    {
        public string SourceId { get; private set; } = sourceId;                 // 伤害来源的单位ID
        public string TargetId { get; private set; } = targetId;                 // 伤害目标单位的ID
        public int DamageAmount { get; private set; } = damageAmount;            // 伤害的具体数值
        public string DamageType { get; private set; } = damageType ?? "Unknown";   // 伤害的类型分类

        // 将伤害事件序列化为网络传输对象
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

        // 从网络数据反序列化重建伤害事件
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