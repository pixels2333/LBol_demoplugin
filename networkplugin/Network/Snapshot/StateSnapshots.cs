using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 完整游戏状态快照（用于断线重连和中途加入）
/// </summary>
public class FullStateSnapshot
{
    /// <summary>
    /// 快照创建时间
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// 游戏全局状态
    /// </summary>
    public GameStateSnapshot GameState { get; set; } = new();

    /// <summary>
    /// 所有玩家的状态
    /// </summary>
    public List<PlayerStateSnapshot> PlayerStates { get; set; } = [];

    /// <summary>
    /// 战斗状态（如果在战斗中）
    /// </summary>
    public BattleStateSnapshot? BattleState { get; set; }

    /// <summary>
    /// 地图状态
    /// </summary>
    public MapStateSnapshot MapState { get; set; } = new();

    /// <summary>
    /// 事件历史索引（用于追赶）
    /// </summary>
    public long EventIndex { get; set; }

    /// <summary>
    /// 游戏版本（用于兼容性检查）
    /// </summary>
    public string GameVersion { get; set; } = "1.0.0";

    /// <summary>
    /// 模组版本（用于MOD兼容性）
    /// </summary>
    public string ModVersion { get; set; } = "1.0.0";
}

/// <summary>
/// 游戏全局状态快照
/// </summary>
public class GameStateSnapshot
{
    /// <summary>
    /// 当前游戏阶段（Menu/InGame/Combat/Rest/Event）
    /// </summary>
    public string GamePhase { get; set; } = "Unknown";

    /// <summary>
    /// 当前关卡（Act）
    /// </summary>
    public int CurrentAct { get; set; } = 1;

    /// <summary>
    /// 当前楼层
    /// </summary>
    public int CurrentFloor { get; set; } = 0;

    /// <summary>
    /// 游戏是否开始
    /// </summary>
    public bool GameStarted { get; set; } = false;

    /// <summary>
    /// 游戏是否结束
    /// </summary>
    public bool GameEnded { get; set; } = false;

    /// <summary>
    /// 胜利/失败
    /// </summary>
    public string? GameResult { get; set; }

    /// <summary>
    /// 当前活动玩家ID（回合制）
    /// </summary>
    public string? ActivePlayerId { get; set; }

    /// <summary>
    /// 回合数
    /// </summary>
    public int TurnCount { get; set; } = 0;

    /// <summary>
    /// 游戏种子（用于复现随机事件）
    /// </summary>
    public int GameSeed { get; set; } = 0;

    /// <summary>
    /// 房间ID
    /// </summary>
    public string RoomId { get; set; } = "unknown";

    /// <summary>
    /// 主机玩家ID
    /// </summary>
    public string HostPlayerId { get; set; } = "unknown";
}

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

/// <summary>
/// 卡牌状态快照
/// </summary>
public class CardStateSnapshot
{
    /// <summary>
    /// 卡牌唯一ID
    /// </summary>
    public string CardId { get; set; } = string.Empty;

    /// <summary>
    /// 卡牌名称
    /// </summary>
    public string CardName { get; set; } = string.Empty;

    /// <summary>
    /// 卡牌类型（攻击/防御/技能等）
    /// </summary>
    public string CardType { get; set; } = "Unknown";

    /// <summary>
    /// 卡牌稀有度
    /// </summary>
    public string Rarity { get; set; } = "Common";

    /// <summary>
    /// 法力消耗
    /// </summary>
    public int ManaCost { get; set; } = 1;

    /// <summary>
    /// 升级次数
    /// </summary>
    public int UpgradeCount { get; set; } = 0;

    /// <summary>
    /// 是否在手中
    /// </summary>
    public bool InHand { get; set; } = false;

    /// <summary>
    /// 是否在牌库
    /// </summary>
    public bool InDeck { get; set; } = false;

    /// <summary>
    /// 是否在弃牌堆
    /// </summary>
    public bool InDiscard { get; set; } = false;

    /// <summary>
    /// 是否被消耗
    /// </summary>
    public bool IsExhausted { get; set; } = false;

    /// <summary>
    /// 是否被放逐
    /// </summary>
    public bool IsBanished { get; set; } = false;

    /// <summary>
    /// 是否是临时卡牌（本回合有效）
    /// </summary>
    public bool IsTemporary { get; set; } = false;

    /// <summary>
    /// 卡牌所在区域的索引
    /// </summary>
    public int ZoneIndex { get; set; } = -1;

    /// <summary>
    /// 额外元数据
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = [];
}

/// <summary>
/// 宝物（圣物）状态快照
/// </summary>
public class ExhibitStateSnapshot
{
    /// <summary>
    /// 宝物ID
    /// </summary>
    public string ExhibitId { get; set; } = string.Empty;

    /// <summary>
    /// 宝物名称
    /// </summary>
    public string ExhibitName { get; set; } = string.Empty;

    /// <summary>
    /// 宝物类型
    /// </summary>
    public string ExhibitType { get; set; } = "Unknown";

    /// <summary>
    /// 稀有度
    /// </summary>
    public string Rarity { get; set; } = "Common";

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; } = false;

    /// <summary>
    /// 是否熄灯（暂时失效）
    /// </summary>
    public bool IsBlackout { get; set; } = false;

    /// <summary>
    /// 是否有计数器
    /// </summary>
    public bool HasCounter { get; set; } = false;

    /// <summary>
    /// 计数器值
    /// </summary>
    public int Counter { get; set; } = 0;

    /// <summary>
    /// 图标名称
    /// </summary>
    public string IconName { get; set; } = "";

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 是否是Boss专属宝物
    /// </summary>
    public bool IsBossExhibit { get; set; } = false;

    /// <summary>
    /// 是否是起始宝物
    /// </summary>
    public bool IsStarterExhibit { get; set; } = false;

    /// <summary>
    /// 额外配置
    /// </summary>
    public Dictionary<string, object> Config { get; set; } = [];
}

/// <summary>
/// 状态效果快照
/// </summary>
public class StatusEffectStateSnapshot
{
    /// <summary>
    /// 效果ID
    /// </summary>
    public string EffectId { get; set; } = string.Empty;

    /// <summary>
    /// 效果名称
    /// </summary>
    public string EffectName { get; set; } = string.Empty;

    /// <summary>
    /// 效果类型
    /// </summary>
    public string EffectType { get; set; } = "Unknown";

    /// <summary>
    /// 等级
    /// </summary>
    public int Level { get; set; } = 1;

    /// <summary>
    /// 持续时间（回合数）
    /// </summary>
    public int Duration { get; set; } = 0;

    /// <summary>
    /// 是否是Debuff
    /// </summary>
    public bool IsDebuff { get; set; } = false;

    /// <summary>
    /// 是否是永久性效果
    /// </summary>
    public bool IsPermanent { get; set; } = false;

    /// <summary>
    /// 效果值
    /// </summary>
    public int EffectValue { get; set; } = 0;

    /// <summary>
    /// 效果描述
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 来源（哪个玩家/敌人施加的）
    /// </summary>
    public string SourceId { get; set; } = "";
}

/// <summary>
/// 战��状态快照
/// </summary>
public class BattleStateSnapshot
{
    /// <summary>
    /// 是否在战斗中
    /// </summary>
    public bool IsInBattle { get; set; } = false;

    /// <summary>
    /// 战斗ID
    /// </summary>
    public string BattleId { get; set; } = "";

    /// <summary>
    /// 当前回合
    /// </summary>
    public int CurrentTurn { get; set; } = 1;

    /// <summary>
    /// 当前回合玩家ID
    /// </summary>
    public string CurrentTurnPlayerId { get; set; } = "unknown";

    /// <summary>
    /// 回合阶段（Player/Enemy）
    /// </summary>
    public string TurnPhase { get; set; } = "Player";

    /// <summary>
    /// 敌人状态
    /// </summary>
    public List<EnemyStateSnapshot> Enemies { get; set; } = [];

    /// <summary>
    /// 战斗开始时间
    /// </summary>
    public long BattleStartTime { get; set; } = 0;

    /// <summary>
    /// 是否是Boss战
    /// </summary>
    public bool IsBossBattle { get; set; } = false;

    /// <summary>
    /// 奖励预览（击败敌人后的奖励）
    /// </summary>
    public BattleRewardSnapshot Rewards { get; set; } = new();

    /// <summary>
    /// 战斗类型（Normal/Elite/Boss/Event）
    /// </summary>
    public string BattleType { get; set; } = "Normal";
}

/// <summary>
/// 敌人状态快照
/// </summary>
public class EnemyStateSnapshot
{
    /// <summary>
    /// 敌人ID
    /// </summary>
    public string EnemyId { get; set; } = string.Empty;

    /// <summary>
    /// 敌人名称
    /// </summary>
    public string EnemyName { get; set; } = "";

    /// <summary>
    /// 敌人类型
    /// </summary>
    public string EnemyType { get; set; } = "Normal";

    /// <summary>
    /// 当前生命值
    /// </summary>
    public int Health { get; set; } = 0;

    /// <summary>
    /// 最大生命值
    /// </summary>
    public int MaxHealth { get; set; } = 0;

    /// <summary>
    /// 格挡
    /// </summary>
    public int Block { get; set; } = 0;

    /// <summary>
    /// 护盾
    /// </summary>
    public int Shield { get; set; } = 0;

    /// <summary>
    /// 当前状态效果
    /// </summary>
    public List<StatusEffectStateSnapshot> StatusEffects { get; set; } = [];

    /// <summary>
    /// 当前意图
    /// </summary>
    public IntentionSnapshot Intention { get; set; } = new();

    /// <summary>
    /// 敌人索引（在战场中的位置）
    /// </summary>
    public int Index { get; set; } = 0;

    /// <summary>
    /// 是否存活
    /// </summary>
    public bool IsAlive { get; set; } = true;
}

/// <summary>
/// 敌人意图快照
/// </summary>
public class IntentionSnapshot
{
    /// <summary>
    /// 意图类型（攻击/防御/特殊等）
    /// </summary>
    public string IntentionType { get; set; } = "None";

    /// <summary>
    /// 意图名称
    /// </summary>
    public string IntentionName { get; set; } = "";

    /// <summary>
    /// 意图值（伤害量/格挡量等）
    /// </summary>
    public int Value { get; set; } = 0;

    /// <summary>
    /// 目标ID
    /// </summary>
    public string? TargetId { get; set; }

    /// <summary>
    /// 意图描述
    /// </summary>
    public string Description { get; set; } = "";
}

/// <summary>
/// 地图状态快照
/// </summary>
public class MapStateSnapshot
{
    /// <summary>
    /// 当前地图种子
    /// </summary>
    public int MapSeed { get; set; } = 0;

    /// <summary>
    /// 已访问的节点
    /// </summary>
    public List<string> VisitedNodes { get; set; } = [];

    /// <summary>
    /// 当前位置
    /// </summary>
    public LocationSnapshot CurrentLocation { get; set; } = new();

    /// <summary>
    /// 已揭示的节点
    /// </summary>
    public List<string> RevealedNodes { get; set; } = [];

    /// <summary>
    /// 路径历史
    /// </summary>
    public List<LocationSnapshot> PathHistory { get; set; } = [];

    /// <summary>
    /// 地图是否完成
    /// </summary>
    public bool MapComplete { get; set; } = false;
}

/// <summary>
/// 位置快照
/// </summary>
public class LocationSnapshot
{
    /// <summary>
    /// X坐标
    /// </summary>
    public int X { get; set; } = 0;

    /// <summary>
    /// Y坐标
    /// </summary>
    public int Y { get; set; } = 0;

    /// <summary>
    /// 节点ID
    /// </summary>
    public string NodeId { get; set; } = "";

    /// <summary>
    /// 节点类型（Battle/Shop/Rest/Event等）
    /// </summary>
    public string NodeType { get; set; } = "Unknown";

    /// <summary>
    /// 是否是分支点
    /// </summary>
    public bool IsBranch { get; set; } = false;

    /// <summary>
    /// 访问时间
    /// </summary>
    public long VisitTime { get; set; } = 0;

    public override string ToString()
    {
        return $"Location({X}, {Y}): {NodeType}";
    }
}

/// <summary>
/// 战斗奖励快照
/// </summary>
public class BattleRewardSnapshot
{
    /// <summary>
    /// 金币奖励
    /// </summary>
    public int GoldReward { get; set; } = 0;

    /// <summary>
    /// 卡牌奖励
    /// </summary>
    public List<string> CardRewards { get; set; } = [];

    /// <summary>
    /// 宝物奖励
    /// </summary>
    public List<string> ExhibitRewards { get; set; } = [];

    /// <summary>
    /// 药水奖励
    /// </summary>
    public List<string> PotionRewards { get; set; } = [];

    /// <summary>
    /// 奖励是否已领取
    /// </summary>
    public bool IsRewardClaimed { get; set; } = false;
}

/// <summary>
/// 玩家统计快照
/// </summary>
public class PlayerStatsSnapshot
{
    /// <summary>
    /// 总伤害
    /// </summary>
    public int TotalDamage { get; set; } = 0;

    /// <summary>
    /// 击败的敌人数量
    /// </summary>
    public int EnemiesDefeated { get; set; } = 0;

    /// <summary>
    /// 战斗胜利次数
    /// </summary>
    public int BattlesWon { get; set; } = 0;

    /// <summary>
    /// 战斗失败次数
    /// </summary>
    public int BattlesLost { get; set; } = 0;

    /// <summary>
    /// 抽牌数量
    /// </summary>
    public int CardsDrawn { get; set; } = 0;

    /// <summary>
    /// 使用卡牌数量
    /// </summary>
    public int CardsPlayed { get; set; } = 0;

    /// <summary>
    /// 获得的宝物数量
    /// </summary>
    public int ExhibitsCollected { get; set; } = 0;

    /// <summary>
    /// 击败的Boss数量
    /// </summary>
    public int BossesDefeated { get; set; } = 0;

    /// <summary>
    /// 发现的秘密房间数
    /// </summary>
    public int SecretsFound { get; set; } = 0;
}