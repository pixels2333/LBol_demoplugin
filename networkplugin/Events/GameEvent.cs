using System;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace NetworkPlugin.Events;

/// <summary>
/// 游戏事件类型枚举
/// 定义LBoL联机MOD中所有可能的游戏事件类型，用于事件分类和处理
/// </summary>
public enum GameEventType
{
    // ========================================
    // 卡牌相关事件
    // 记录玩家卡牌操作的完整生命周期
    // ========================================

    /// <summary>
    /// 卡牌使用开始事件
    /// 玩家开始使用卡牌时触发，记录卡牌基本信息和目标选择
    /// </summary>
    CardPlayStart,

    /// <summary>
    /// 卡牌使用完成事件
    /// 卡牌效果完全执行完毕时触发，标志使用流程结束
    /// </summary>
    CardPlayComplete,

    /// <summary>
    /// 卡牌抽取事件
    /// 玩家从牌库抽取卡牌时触发，记录抽牌操作
    /// </summary>
    CardDraw,

    /// <summary>
    /// 卡牌丢弃事件
    /// 玩家丢弃手牌时触发，记录弃牌操作和原因
    /// </summary>
    CardDiscard,

    /// <summary>
    /// 卡牌放逐事件
    /// 卡牌被放逐（从游戏中移除）时触发
    /// </summary>
    CardExile,

    /// <summary>
    /// 卡牌升级事件
    /// 卡牌获得升级效果时触发，记录升级前后的变化
    /// </summary>
    CardUpgrade,

    /// <summary>
    /// 卡牌移除事件
    /// 卡牌从牌组中永久移除时触发
    /// </summary>
    CardRemove,

    // ========================================
    // 法力相关事件
    // 记录法力值的变化和消耗情况
    // ========================================

    /// <summary>
    /// 法力消耗开始事件
    /// 开始消耗法力支付卡牌费用时触发
    /// </summary>
    ManaConsumeStart,

    /// <summary>
    /// 法力消耗完成事件
    /// 法力消耗完全支付完毕时触发
    /// </summary>
    ManaConsumeComplete,

    /// <summary>
    /// 法力回复事件
    /// 玩家获得法力回复时触发，记录回复量和来源
    /// </summary>
    ManaRegain,

    /// <summary>
    /// 回合法力重置事件
    /// 新回合开始法力值重置时触发
    /// </summary>
    TurnManaReset,

    // ========================================
    // 战斗相关事件
    // 记录战斗过程中的数值变化和状态效果
    // ========================================

    /// <summary>
    /// 造成伤害事件
    /// 单位对目标造成伤害时触发，记录伤害详情
    /// </summary>
    DamageDealt,

    /// <summary>
    /// 受到伤害事件
    /// 单位受到伤害时触发，记录受伤情况
    /// </summary>
    DamageReceived,

    /// <summary>
    /// 获得格挡事件
    /// 单位获得格挡值时触发，记录格挡量
    /// </summary>
    BlockGained,

    /// <summary>
    /// 获得护盾事件
    /// 单位获得护盾值时触发，记录护盾量
    /// </summary>
    ShieldGained,

    /// <summary>
    /// 接受治疗事件
    /// 单位恢复生命值时触发，记录治疗量
    /// </summary>
    HealingReceived,

    /// <summary>
    /// 状态效果应用事件
    /// 单位获得新的状态效果时触发
    /// </summary>
    StatusEffectApplied,

    /// <summary>
    /// 状态效果移除事件
    /// 单位的状态效果消失时触发
    /// </summary>
    StatusEffectRemoved,

    // ========================================
    // 回合相关事件
    // 记录游戏流程中的关键时间节点
    // ========================================

    /// <summary>
    /// 回合开始事件
    /// 玩家回合开始时触发，标志回合切换
    /// </summary>
    TurnStart,

    /// <summary>
    /// 回合结束事件
    /// 玩家回合结束时触发，记录回合结算
    /// </summary>
    TurnEnd,

    /// <summary>
    /// 战斗开始事件
    /// 进入战斗场景时触发，标志战斗流程开始
    /// </summary>
    BattleStart,

    /// <summary>
    /// 战斗结束事件
    /// 战斗场景结束时触发，记录战斗结果
    /// </summary>
    BattleEnd,

    /// <summary>
    /// 轮次开始事件
    /// 新的游戏轮次开始时触发
    /// </summary>
    RoundStart,

    /// <summary>
    /// 轮次结束事件
    /// 当前游戏轮次结束时触发
    /// </summary>
    RoundEnd,

    // ========================================
    // 地图/节点相关事件
    // 记录大地图探索和节点交互
    // ========================================

    /// <summary>
    /// 地图节点进入事件
    /// 玩家移动到新的地图节点时触发
    /// </summary>
    MapNodeEnter,

    /// <summary>
    /// 地图节点完成事件
    /// 玩家完成当前节点的事件时触发
    /// </summary>
    MapNodeComplete,

    /// <summary>
    /// 间隙站进入事件
    /// 玩家进入间隙站（商店/休息点）时触发
    /// </summary>
    GapStationEnter,

    /// <summary>
    /// 间隙选项选择事件
    /// 玩家在间隙站选择特定选项时触发
    /// </summary>
    GapOptionSelected,

    // ========================================
    // 物品/道具相关事件
    // 记录道具和药品的获取与使用
    // ========================================

    /// <summary>
    /// 道具获取事件
    /// 玩家获得新的道具时触发，记录道具信息
    /// </summary>
    ExhibitObtained,

    /// <summary>
    /// 道具移除事件
    /// 玩家失去道具时触发，记录移除原因
    /// </summary>
    ExhibitRemoved,

    /// <summary>
    /// 药品使用事件
    /// 玩家使用药品时触发，记录使用效果
    /// </summary>
    PotionUsed,

    // ========================================
    // 玩家相关事件
    // 记录玩家状态变化和网络连接
    // ========================================

    /// <summary>
    /// 玩家加入事件
    /// 新玩家加入游戏房间时触发
    /// </summary>
    PlayerJoin,

    /// <summary>
    /// 玩家离开事件
    /// 玩家离开游戏房间时触发，记录离开原因
    /// </summary>
    PlayerLeave,

    /// <summary>
    /// 玩家准备事件
    /// 玩家点击准备开始游戏时触发
    /// </summary>
    PlayerReady,

    /// <summary>
    /// 玩家状态更新事件
    /// 玩家游戏状态发生变化时触发
    /// </summary>
    PlayerStatusUpdate,

    // ========================================
    // 网络相关事件
    // 记录网络连接和同步状态
    // ========================================

    /// <summary>
    /// 连接建立事件
    /// 与其他玩家建立网络连接时触发
    /// </summary>
    ConnectionEstablished,

    /// <summary>
    /// 连接丢失事件
    /// 与其他玩家的网络连接断开时触发
    /// </summary>
    ConnectionLost,

    /// <summary>
    /// 重连尝试事件
    /// 尝试重新建立断开的连接时触发
    /// </summary>
    ReconnectionAttempt,

    /// <summary>
    /// 状态同步请求事件
    /// 向其他玩家请求游戏状态同步时触发
    /// </summary>
    StateSyncRequest,

    /// <summary>
    /// 状态同步完成事件
    /// 游戏状态同步操作完成时触发
    /// </summary>
    StateSyncComplete,

    // ========================================
    // 系统相关事件
    // 记录游戏整体状态和异常情况
    // ========================================

    /// <summary>
    /// 游戏开始事件
    /// 整个游戏流程正式开始时触发
    /// </summary>
    GameStart,

    /// <summary>
    /// 游戏结束事件
    /// 游戏流程完全结束时触发，记录游戏结果
    /// </summary>
    GameEnd,

    /// <summary>
    /// 保存游戏事件
    /// 游戏存档操作执行时触发
    /// </summary>
    SaveGame,

    /// <summary>
    /// 加载游戏事件
    /// 游戏读档操作执行时触发
    /// </summary>
    LoadGame,

    /// <summary>
    /// 错误事件
    /// 系统发生错误或异常情况时触发
    /// </summary>
    Error
}

/// <summary>
/// 游戏事件基类
/// 定义LBoL联机MOD中所有游戏事件的通用属性和行为
/// 这是整个事件系统的基础，为具体事件类型提供统一的接口
/// </summary>
/// <param name="eventType">事件类型，标识具体是哪种游戏事件</param>
/// <param name="sourcePlayerId">触发事件的玩家ID，用于事件来源追踪</param>
/// <param name="data">事件的附加数据，可包含事件相关的具体信息</param>
public abstract class GameEvent(GameEventType eventType, string sourcePlayerId, object data = null)
{
    /// <summary>
    /// 事件类型
    /// 标识具体的事件种类，用于事件分类和处理路由
    /// </summary>
    public GameEventType EventType { get; protected set; } = eventType;

    /// <summary>
    /// 事件时间戳
    /// 记录事件发生的精确时间，用于事件排序和调试分析
    /// </summary>
    public DateTime Timestamp { get; protected set; } = DateTime.Now;

    /// <summary>
    /// 事件来源玩家ID
    /// 标识触发事件的玩家，用于权限检查和操作追踪
    /// </summary>
    public string SourcePlayerId { get; protected set; } = sourcePlayerId ?? "unknown";

    /// <summary>
    /// 事件附加数据
    /// 存储事件相关的详细信息，具体内容由子类定义
    /// </summary>
    public object Data { get; protected set; } = data;

    /// <summary>
    /// 将事件转换为网络传输格式
    /// 将事件对象序列化为适合网络传输的数据结构
    /// </summary>
    /// <returns>包含事件所有必要信息的网络数据对象</returns>
    public abstract object ToNetworkData();

    /// <summary>
    /// 从网络数据重建事件实例
    /// 将接收到的网络数据反序列化为事件对象
    /// </summary>
    /// <param name="data">来自网络的序列化事件数据</param>
    /// <returns>重建后的事件实例，失败时返回null</returns>
    public abstract GameEvent FromNetworkData(object data);
}