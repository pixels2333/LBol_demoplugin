using System;
using System.Collections.Generic;
using LBoL.Base;

namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 玩家状态快照
/// </summary>
public class PlayerStateSnapshot(
    string userName,
    int health,
    int maxHealth,
    int block,
    int shield,
    Dictionary<ManaColor, int> mana,
    int maxMana,
    int gold,
    int turnNumber,
    bool isInBattle,
    bool isAlive,
    bool isPlayersTurn,
    bool isInTurn,
    bool isExtraTurn,
    string characterType,
    string reconnectToken,
    long disconnectTime,
    long lastUpdateTime,
    bool isAIControlled,
    int turnCounter,
    DateTime timestamp)
{


    /// <summary>
    /// 玩家名称
    /// </summary>
    public string UserName { get; set; } = userName;

    /// <summary>
    /// 当前生命值
    /// </summary>
    public int Health { get; set; } = health;

    /// <summary>
    /// 最大生命值
    /// </summary>
    public int MaxHealth { get; set; } = maxHealth;

    /// <summary>
    /// 当前格挡
    /// </summary>
    public int Block { get; set; } = block;

    /// <summary>
    /// 当前护盾
    /// </summary>
    public int Shield { get; set; } = shield;

    /// <summary>
    /// 法力组（红、蓝、绿、白）
    /// </summary>
    public Dictionary<ManaColor, int> ManaGroup { get; set; } = mana;

    /// <summary>
    /// 最大法力
    /// </summary>
    public int MaxMana { get; set; } = maxMana;

    /// <summary>
    /// 金币
    /// </summary>
    public int Gold { get; set; } = gold;

    /// <summary>
    /// 卡牌状态
    /// </summary>
    public List<CardStateSnapshot> Cards { get; set; }

    /// <summary>
    /// 宝物状态
    /// </summary>
    public List<ExhibitStateSnapshot> Exhibits { get; set; }

    /// <summary>
    /// 药水状态（药水ID -> 数量）
    /// </summary>
    public Dictionary<string, int> Potions { get; set; }

    /// <summary>
    /// 状态效果
    /// </summary>
    public List<StatusEffectStateSnapshot> StatusEffects { get; set; }

    /// <summary>
    /// 游戏内位置
    /// </summary>
    public LocationSnapshot GameLocation { get; set; }

    /// <summary>
    /// 是否在战斗中
    /// </summary>
    public bool IsInBattle { get; set; } = isInBattle;

    /// <summary>
    /// 是否存活
    /// </summary>
    public bool IsAlive { get; set; } = isAlive;

    /// <summary>
    /// 是否是当前回合
    /// </summary>
    public bool IsPlayersTurn { get; set; } = isPlayersTurn;

    /// <summary>
    /// 是否在回合中
    /// </summary>
    public bool IsInTurn { get; set; } = isInTurn;

    public int TurnNumber { get; set; } = turnNumber;

    /// <summary>
    /// 是否是额外回合
    /// </summary>
    public bool IsExtraTurn { get; set; } = isExtraTurn;

    /// <summary>
    /// 角色类型（主角/辅助）
    /// </summary>
    public string CharacterType { get; set; } = characterType;

    /// <summary>
    /// 重连令牌（用于验证重连）
    /// </summary>
    public string ReconnectToken { get; set; } = reconnectToken;

    /// <summary>
    /// 断线时间（如果已断线）
    /// </summary>
    public long DisconnectTime { get; set; } = disconnectTime;

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public long LastUpdateTime { get; set; } = lastUpdateTime;

    /// <summary>
    /// 是否是AI托管
    /// </summary>
    public bool IsAIControlled { get; set; } = isAIControlled;

    /// <summary>
    /// 统计信息
    /// </summary>
    public PlayerPerformanceSnapshot Stats { get; set; }

    /// <summary>
    /// 快照生成时间
    /// </summary>
    public DateTime Timestamp { get; set; } = timestamp;
}
