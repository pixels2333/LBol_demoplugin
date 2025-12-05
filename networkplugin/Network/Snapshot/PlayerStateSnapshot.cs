using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 玩家状态快照
/// </summary>
public class PlayerStateSnapshot
{
    /// <summary>
    /// 玩家ID
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>
    /// 玩家名称
    /// </summary>
    public string PlayerName { get; set; } = "Player";

    /// <summary>
    /// 当前生命值
    /// </summary>
    public int Health { get; set; } = 80;

    /// <summary>
    /// 最大生命值
    /// </summary>
    public int MaxHealth { get; set; } = 80;

    /// <summary>
    /// 当前格挡
    /// </summary>
    public int Block { get; set; } = 0;

    /// <summary>
    /// 当前护盾
    /// </summary>
    public int Shield { get; set; } = 0;

    /// <summary>
    /// 法力组（红、蓝、绿、白）
    /// </summary>
    public int[] ManaGroup { get; set; } = [3, 3, 3, 3];

    /// <summary>
    /// 最大法力
    /// </summary>
    public int MaxMana { get; set; } = 3;

    /// <summary>
    /// 金币
    /// </summary>
    public int Gold { get; set; } = 100;

    /// <summary>
    /// 卡牌状态
    /// </summary>
    public List<CardStateSnapshot> Cards { get; set; } = [];

    /// <summary>
    /// 宝物状态
    /// </summary>
    public List<ExhibitStateSnapshot> Exhibits { get; set; } = [];

    /// <summary>
    /// 药水状态（药水ID -> 数量）
    /// </summary>
    public Dictionary<string, int> Potions { get; set; } = [];

    /// <summary>
    /// 状态效果
    /// </summary>
    public List<StatusEffectStateSnapshot> StatusEffects { get; set; } = [];

    /// <summary>
    /// 游戏内位置
    /// </summary>
    public LocationSnapshot GameLocation { get; set; } = new();

    /// <summary>
    /// 是否在战斗中
    /// </summary>
    public bool IsInBattle { get; set; } = false;

    /// <summary>
    /// 是否存活
    /// </summary>
    public bool IsAlive { get; set; } = true;

    /// <summary>
    /// 是否是当前回合
    /// </summary>
    public bool IsPlayersTurn { get; set; } = false;

    /// <summary>
    /// 角色类型（主角/辅助）
    /// </summary>
    public string CharacterType { get; set; } = "Main";

    /// <summary>
    /// 重连令牌（用于验证重连）
    /// </summary>
    public string ReconnectToken { get; set; } = string.Empty;

    /// <summary>
    /// 断线时间（如果已断线）
    /// </summary>
    public long DisconnectTime { get; set; } = 0;

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public long LastUpdateTime { get; set; } = 0;

    /// <summary>
    /// 是否是AI托管
    /// </summary>
    public bool IsAIControlled { get; set; } = false;

    /// <summary>
    /// 统计信息
    /// </summary>
    public PlayerStatsSnapshot Stats { get; set; } = new();
}
