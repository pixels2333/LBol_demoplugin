using System;
using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 游戏事件基类 - 用于断线重连和中途加入的事件回放
/// </summary>
public class GameEvent
{
    /// <summary>
    /// 事件唯一ID
    /// </summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 事件类型
    /// </summary>
    public string EventType { get; set; } = "Unknown";

    /// <summary>
    /// 事件时间戳（用于排序和追赶）
    /// </summary>
    public long Timestamp { get; set; } = DateTime.Now.Ticks;

    /// <summary>
    /// 事件索引（递增，用于确定事件顺序）
    /// </summary>
    public long EventIndex { get; set; } = 0;

    /// <summary>
    /// 触发事件的玩家ID
    /// </summary>
    public string PlayerId { get; set; } = "unknown";

    /// <summary>
    /// 影响的目标ID
    /// </summary>
    public string? TargetId { get; set; }

    /// <summary>
    /// 事件数据
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = [];

    /// <summary>
    /// 事件是否已处理
    /// </summary>
    public bool IsProcessed { get; set; } = false;

    /// <summary>
    /// 事件来源（Client/Server/System）
    /// </summary>
    public string Source { get; set; } = "Unknown";

    /// <summary>
    /// 构造函数
    /// </summary>
    public GameEvent() { }

    /// <summary>
    /// 构造函数
    /// </summary>
    public GameEvent(string eventType, string playerId, Dictionary<string, object> data)
    {
        EventType = eventType;
        PlayerId = playerId;
        Data = data;
        Timestamp = DateTime.Now.Ticks;
    }
}

/// <summary>
/// 卡��使用事件
/// </summary>
public class CardPlayedEvent : GameEvent
{
    public CardPlayedEvent() : base()
    {
        EventType = "CardPlayed";
    }

    public CardPlayedEvent(string playerId, string cardId, string cardName, string? targetId = null)
        : base("CardPlayed", playerId, new Dictionary<string, object>
        {
            { "CardId", cardId },
            { "CardName", cardName },
            { "TargetId", targetId ?? "" }
        })
    {
    }
}

/// <summary>
/// 抽牌事件
/// </summary>
public class CardDrawnEvent : GameEvent
{
    public CardDrawnEvent() : base()
    {
        EventType = "CardDrawn";
    }

    public CardDrawnEvent(string playerId, int cardsDrawn, int cardsInHand)
        : base("CardDrawn", playerId, new Dictionary<string, object>
        {
            { "CardsDrawn", cardsDrawn },
            { "CardsInHand", cardsInHand }
        })
    {
    }
}

/// <summary>
/// 药水使用事件
/// </summary>
public class PotionUsedEvent : GameEvent
{
    public PotionUsedEvent() : base()
    {
        EventType = "PotionUsed";
    }

    public PotionUsedEvent(string playerId, string potionId, string potionName, int quantity = 1)
        : base("PotionUsed", playerId, new Dictionary<string, object>
        {
            { "PotionId", potionId },
            { "PotionName", potionName },
            { "Quantity", quantity }
        })
    {
    }
}

/// <summary>
/// 受到伤害事件
/// </summary>
public class DamageTakenEvent : GameEvent
{
    public DamageTakenEvent() : base()
    {
        EventType = "DamageTaken";
    }

    public DamageTakenEvent(string playerId, string targetId, int damage, int remainingHealth)
        : base("DamageTaken", playerId, new Dictionary<string, object>
        {
            { "TargetId", targetId },
            { "Damage", damage },
            { "RemainingHealth", remainingHealth },
            { "IsPlayer", true }
        })
    {
    }
}

/// <summary>
/// 获得金币事件
/// </summary>
public class GoldGainedEvent : GameEvent
{
    public GoldGainedEvent() : base()
    {
        EventType = "GoldGained";
    }

    public GoldGainedEvent(string playerId, int amount, string source)
        : base("GoldGained", playerId, new Dictionary<string, object>
        {
            { "Amount", amount },
            { "Source", source },
            { "TotalGold", GetTotalGold(playerId) }
        })
    {
    }

    private static int GetTotalGold(string playerId) { return 100; } // Stub
}

/// <summary>
/// 宝物获得事件
/// </summary>
public class ExhibitGainedEvent : GameEvent
{
    public ExhibitGainedEvent() : base()
    {
        EventType = "ExhibitGained";
    }

    public ExhibitGainedEvent(string playerId, string exhibitId, string exhibitName, string rarity)
        : base("ExhibitGained", playerId, new Dictionary<string, object>
        {
            { "ExhibitId", exhibitId },
            { "ExhibitName", exhibitName },
            { "Rarity", rarity }
        })
    {
    }
}

/// <summary>
/// 回合结束事件
/// </summary>
public class TurnEndedEvent : GameEvent
{
    public TurnEndedEvent() : base()
    {
        EventType = "TurnEnded";
    }

    public TurnEndedEvent(string playerId, int turnNumber, int cardsPlayed)
        : base("TurnEnded", playerId, new Dictionary<string, object>
        {
            { "TurnNumber", turnNumber },
            { "CardsPlayed", cardsPlayed }
        })
    {
    }
}

/// <summary>
/// 房间进入事件
/// </summary>
public class RoomEnteredEvent : GameEvent
{
    public RoomEnteredEvent() : base()
    {
        EventType = "RoomEntered";
    }

    public RoomEnteredEvent(string playerId, string roomId, string roomType, int x, int y)
        : base("RoomEntered", playerId, new Dictionary<string, object>
        {
            { "RoomId", roomId },
            { "RoomType", roomType },
            { "X", x },
            { "Y", y }
        })
    {
    }
}

/// <summary>
/// 战斗开始事件
/// </summary>
public class BattleStartedEvent : GameEvent
{
    public BattleStartedEvent() : base()
    {
        EventType = "BattleStarted";
    }

    public BattleStartedEvent(string playerId, string battleId, List<string> enemyIds, bool isBossBattle)
        : base("BattleStarted", playerId, new Dictionary<string, object>
        {
            { "BattleId", battleId },
            { "EnemyIds", enemyIds },
            { "IsBossBattle", isBossBattle }
        })
    {
    }
}

/// <summary>
/// 状态效果应用事件
/// </summary>
public class StatusEffectAppliedEvent : GameEvent
{
    public StatusEffectAppliedEvent() : base()
    {
        EventType = "StatusEffectApplied";
    }

    public StatusEffectAppliedEvent(string playerId, string targetId, string effectId, string effectName, int level, bool isDebuff)
        : base("StatusEffectApplied", playerId, new Dictionary<string, object>
        {
            { "TargetId", targetId },
            { "EffectId", effectId },
            { "EffectName", effectName },
            { "Level", level },
            { "IsDebuff", isDebuff }
        })
    {
    }
}

/// <summary>
/// 玩家连接事件
/// </summary>
public class PlayerConnectedEvent : GameEvent
{
    public PlayerConnectedEvent() : base()
    {
        EventType = "PlayerConnected";
    }

    public PlayerConnectedEvent(string playerId, string playerName, bool isReconnect = false)
        : base("PlayerConnected", playerId, new Dictionary<string, object>
        {
            { "PlayerName", playerName },
            { "IsReconnect", isReconnect }
        })
    {
    }
}

/// <summary>
/// 玩家断开连接事件
/// </summary>
public class PlayerDisconnectedEvent : GameEvent
{
    public PlayerDisconnectedEvent() : base()
    {
        EventType = "PlayerDisconnected";
    }

    public PlayerDisconnectedEvent(string playerId, string disconnectReason)
        : base("PlayerDisconnected", playerId, new Dictionary<string, object>
        {
            { "Reason", disconnectReason },
            { "Timestamp", DateTime.Now.Ticks }
        })
    {
    }
}

/// <summary>
/// 同步请求事件（客户端请求状态同步）
/// </summary>
public class SyncRequestedEvent : GameEvent
{
    public SyncRequestedEvent() : base()
    {
        EventType = "SyncRequested";
    }

    public SyncRequestedEvent(string playerId, string requestId, string syncType)
        : base("SyncRequested", playerId, new Dictionary<string, object>
        {
            { "RequestId", requestId },
            { "SyncType", syncType }
        })
    {
    }
}

/// <summary>
/// 同步响应事件（服务器返回同步数据）
/// </summary>
public class SyncResponseEvent : GameEvent
{
    public SyncResponseEvent() : base()
    {
        EventType = "SyncResponse";
    }

    public SyncResponseEvent(string playerId, string requestId, string syncType, object data)
        : base("SyncResponse", playerId, new Dictionary<string, object>
        {
            { "RequestId", requestId },
            { "SyncType", syncType },
            { "Data", data }
        })
    {
    }
}

/// <summary>
/// 快照保存事件
/// </summary>
public class SnapshotSavedEvent : GameEvent
{
    public SnapshotSavedEvent() : base()
    {
        EventType = "SnapshotSaved";
    }

    public SnapshotSavedEvent(string playerId, long timestamp, string snapshotType)
        : base("SnapshotSaved", "SYSTEM", new Dictionary<string, object>
        {
            { "PlayerId", playerId },
            { "SnapshotTimestamp", timestamp },
            { "SnapshotType", snapshotType }
        })
    {
    }
}

/// <summary>
/// 游戏事件管理器 - 用于创建和管理游戏事件
/// </summary>
public static class GameEventManager
{
    private static long _nextEventIndex = 0;

    /// <summary>
    /// 创建新事件并分配索引
    /// </summary>
    public static GameEvent CreateEvent(string eventType, string playerId, Dictionary<string, object> data)
    {
        var gameEvent = new GameEvent(eventType, playerId, data)
        {
            EventIndex = Interlocked.Increment(ref _nextEventIndex)
        };
        return gameEvent;
    }

    /// <summary>
    /// 获取当前事件索引
    /// </summary>
    public static long GetCurrentEventIndex()
    {
        return _nextEventIndex;
    }

    /// <summary>
    /// 重置事件索引计数器
    /// </summary>
    public static void ResetEventIndex()
    {
        _nextEventIndex = 0;
    }

    /// <summary>
    /// 从特定索引开始
    /// </summary>
    public static void SetEventIndex(long index)
    {
        _nextEventIndex = index;
    }
}
