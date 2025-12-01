using System;
using System.Collections.Generic;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace NetworkPlugin.Events
{
    /// <summary>
    /// 游戏事件类型枚举
    /// 定义LBoL联机MOD中所有可能的游戏事件类型，用于事件分类和处理
    /// </summary>
    /// <remarks>
    /// <para>
    /// 事件分类说明：
    /// - 卡牌相关：记录卡牌的使用、抽取、丢弃等操作
    /// - 法力相关：记录法力消耗、回复等状态变化
    /// - 战斗相关：记录伤害、格挡、护盾等战斗数值变化
    /// - 回合相关：记录游戏流程中的关键时间点
    /// - 地图相关：记录地图探索和节点操作
    /// - 物品相关：记录道具和药品的使用情况
    /// - 玩家相关：记录玩家状态和网络连接
    /// - 系统相关：记录游戏整体状态和异常情况
    /// </para>
    ///
    /// <para>
    /// 设计参考: 杀戮尖塔Together in Spire的事件系统架构
    /// 适配了LBoL游戏的事件机制和多用户同步需求
    /// </para>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// 基类设计原则：
    /// - 提供所有事件的通用属性（类型、时间戳、来源等）
    /// - 定义网络序列化的标准接口
    /// - 支持事件的创建和重建机制
    /// - 为事件处理系统提供统一的数据格式
    /// </para>
    ///
    /// <para>
    /// 事件生命周期：
    /// 1. 游戏内发生特定动作 → 触发事件创建
    /// 2. 事件对象被创建并初始化
    /// 3. 事件通过网络协议发送给其他玩家
    /// 4. 接收方重建事件对象并执行相应处理
    /// 5. 事件被记录到历史日志中
    /// </para>
    ///
    /// <para>
    /// 设计模式：模板方法模式
    /// 抽象方法定义了网络处理的框架，具体事件类实现细节
    /// </para>
    /// </remarks>
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
        /// <remarks>
        /// <para>
        /// 序列化要求：
        /// - 包含事件的所有关键属性（类型、时间戳、来源等）
        /// - 数据格式必须能够在网络中可靠传输
        /// - 支持跨平台的数据格式（JSON、二进制等）
        /// - 保持数据的完整性和一致性
        /// </para>
        ///
        /// <para>
        /// 实现注意事项：
        /// - 时间戳建议转换为Ticks以确保精度
        /// - 复杂对象需要适当的序列化处理
        /// - 考虑数据压缩以减少网络传输量
        /// </para>
        /// </remarks>
        public abstract object ToNetworkData();

        /// <summary>
        /// 从网络数据重建事件实例
        /// 将接收到的网络数据反序列化为事件对象
        /// </summary>
        /// <param name="data">来自网络的序列化事件数据</param>
        /// <returns>重建后的事件实例，失败时返回null</returns>
        /// <remarks>
        /// <para>
        /// 重建要求：
        /// - 验证网络数据的完整性和有效性
        /// - 正确恢复事件的所有属性和状态
        /// - 处理数据格式不匹配或损坏的情况
        /// - 确保重建的事件与原始事件功能一致
        /// </para>
        ///
        /// <para>
        /// 安全考虑：
        /// - 验证数据来源的可信度
        /// - 防止恶意数据的注入攻击
        /// - 处理版本兼容性问题
        /// </para>
        /// </remarks>
        public abstract GameEvent FromNetworkData(object data);
    }

    /// <summary>
    /// 卡牌使用事件类
    /// 记录玩家使用卡牌的详细信息，包括卡牌属性、消耗和目标选择
    /// 这是游戏中最核心和最频繁的事件类型之一
    /// </summary>
    /// <remarks>
    /// <para>
    /// 卡牌使用流程：
    /// 1. 玩家选择手牌中的卡牌
    /// 2. 系统检查使用条件（法力、目标等）
    /// 3. 玩家确认使用，触发CardPlayStart事件
    /// 4. 系统执行卡牌效果
    /// 5. 效果执行完毕，触发CardPlayComplete事件
    /// </para>
    ///
    /// <para>
    /// 同步重要性：
    /// - 卡牌使用影响战斗结果和游戏平衡
    /// - 多玩家环境下需要精确同步使用顺序
    /// - 法力消耗和目标选择需要实时验证
    /// - 为其他玩家提供游戏进度信息
    /// </para>
    /// </remarks>
    /// <param name="sourcePlayerId">使用卡牌的玩家ID</param>
    /// <param name="cardId">卡牌的唯一标识符</param>
    /// <param name="cardName">卡牌的显示名称</param>
    /// <param name="cardType">卡牌的类型分类（攻击/技能/能力等）</param>
    /// <param name="manaCost">卡牌的四色法力消耗数组</param>
    /// <param name="targetSelector">目标选择器，标识卡牌的作用目标</param>
    /// <param name="additionalData">附加数据，可包含特殊效果或元数据</param>
    public class CardPlayEvent(string sourcePlayerId, string cardId, string cardName, string cardType,
        int[] manaCost, string targetSelector, object additionalData = null) : GameEvent(GameEventType.CardPlayStart, sourcePlayerId, additionalData)
    {
        /// <summary>
        /// 卡牌唯一标识符
        /// 系统内部用于识别卡牌的ID，通常与卡牌数据库对应
        /// </summary>
        public string CardId { get; private set; } = cardId;

        /// <summary>
        /// 卡牌显示名称
        /// 在UI界面显示给玩家的卡牌名称，可能包含本地化文本
        /// </summary>
        public string CardName { get; private set; } = cardName;

        /// <summary>
        /// 卡牌类型
        /// 标识卡牌的战斗作用分类（攻击/技能/能力/诅咒等）
        /// </summary>
        public string CardType { get; private set; } = cardType;

        /// <summary>
        /// 法力消耗数组
        /// 包含四种颜色法力的消耗量 [红,白,黑,无]
        /// </summary>
        public int[] ManaCost { get; private set; } = manaCost ?? [0, 0, 0, 0];

        /// <summary>
        /// 目标选择器
        /// 描述卡牌的作用目标或效果范围，支持复杂的目标逻辑
        /// </summary>
        public string TargetSelector { get; private set; } = targetSelector ?? "Nobody";

        /// <summary>
        /// 将卡牌使用事件转换为网络数据格式
        /// 序列化卡牌使用相关的所有信息用于网络传输
        /// </summary>
        /// <returns>包含卡牌使用信息的匿名对象</returns>
        /// <remarks>
        /// <para>
        /// 网络数据包含：
        /// - 事件基础信息（类型、时间戳、来源）
        /// - 卡牌基本信息（ID、名称、类型）
        /// - 游戏机制信息（法力消耗、目标选择）
        /// - 附加数据（特殊效果、元数据等）
        /// </para>
        ///
        /// <para>
        /// 时间戳处理：
        /// 转换为Ticks确保网络传输中的精度保持，避免时区差异问题
        /// </para>
        /// </remarks>
        public override object ToNetworkData()
        {
            // 创建匿名对象包含所有网络传输所需的信息
            return new
            {
                // 事件基础信息
                EventType = EventType.ToString(),          // 事件类型转换为字符串
                Timestamp = Timestamp.Ticks,               // 时间戳转换为Tick确保精度
                SourcePlayerId,                            // 事件来源玩家ID

                // 卡牌核心信息
                CardId,                                    // 卡牌唯一标识
                CardName,                                  // 卡牌显示名称
                CardType,                                  // 卡牌类型分类
                ManaCost,                                  // 四色法力消耗数组
                TargetSelector,                            // 目标选择器

                // 附加数据和元数据
                Data                                       // 事件附加数据对象
            };
        }

        /// <summary>
        /// 从网络数据重建卡牌使用事件
        /// 将接收到的网络数据反序列化为CardPlayEvent对象
        /// </summary>
        /// <param name="data">来自网络的序列化卡牌事件数据</param>
        /// <returns>重建的CardPlayEvent实例</returns>
        /// <remarks>
        /// <para>
        /// TODO: 需要实现完整的反序列化逻辑：
        /// - 解析网络数据并提取各个字段
        /// - 验证数据的有效性和完整性
        /// - 重建CardPlayEvent对象的所有属性
        /// - 处理数据格式不匹配的情况
        /// </para>
        ///
        /// <para>
        /// 当前实现为临时返回，实际使用时需要：
        /// 1. 将data转换为具体的数据结构
        /// 2. 验证必需字段的存在和有效性
        /// 3. 创建新的CardPlayEvent实例
        /// 4. 返回重建的事件对象
        /// </para>
        /// </remarks>
        public override GameEvent FromNetworkData(object data)
        {
            // TODO: 实现完整的网络数据重建逻辑
            // 1. 解析data对象中的各个字段
            // 2. 验证数据格式和内容
            // 3. 重建CardPlayEvent实例
            // 4. 处理异常和错误情况

            return this; // 临时返回当前实例，实际需要实现重建逻辑
        }
    }

    /// <summary>
    /// 法力消耗事件类
    /// 记录法力值消耗的详细信息，包括消耗前后的状态变化和消耗来源
    /// 用于同步玩家法力状态和验证卡牌使用的合法性
    /// </summary>
    /// <remarks>
    /// <para>
    /// 法力系统说明：
    /// LBoL使用四色法力系统：
    /// - 红色法力：攻击类卡牌的主要消耗
    /// - 白色法力：技能类卡牌的主要消耗
    /// - 黑色法力：能力类卡牌的主要消耗
    /// - 无色法力：通用法力，可用于任何类型卡牌
    /// </para>
    ///
    /// <para>
    /// 消耗流程：
    /// 1. 玩家选择要使用的卡牌
    /// 2. 系统计算所需法力消耗
    /// 3. 检查当前法力是否足够
    /// 4. 触发ManaConsumeStart事件
    /// 5. 扣除相应法力值
    /// 6. 触发ManaConsumeComplete事件
    /// </para>
    /// </remarks>
    /// <param name="sourcePlayerId">消耗法力的玩家ID</param>
    /// <param name="manaBefore">消耗前的四色法力值数组</param>
    /// <param name="manaConsumed">本次消耗的四色法力值数组</param>
    /// <param name="source">法力消耗的来源说明</param>
    /// <param name="additionalData">附加数据，可能包含特殊效果或触发条件</param>
    public class ManaConsumeEvent(string sourcePlayerId, int[] manaBefore, int[] manaConsumed,
        string source, object additionalData = null) : GameEvent(GameEventType.ManaConsumeStart, sourcePlayerId, additionalData)
    {
        /// <summary>
        /// 消耗前的法力值数组
        /// 记录消耗前玩家拥有的四色法力值 [红,白,黑,无]
        /// 用于验证消耗的合法性和计算剩余法力
        /// </summary>
        public int[] ManaBefore { get; private set; } = manaBefore ?? [0, 0, 0, 0];

        /// <summary>
        /// 消耗的法力值数组
        /// 记录本次操作消耗的四色法力值 [红,白,黑,无]
        /// 与卡牌消耗值对应，用于状态同步和验证
        /// </summary>
        public int[] ManaConsumed { get; private set; } = manaConsumed ?? [0, 0, 0, 0];

        /// <summary>
        /// 法力消耗来源
        /// 描述导致法力消耗的具体原因或操作
        /// </summary>
        public string Source { get; private set; } = source ?? "Unknown";

        /// <summary>
        /// 将法力消耗事件转换为网络数据格式
        /// 序列化法力消耗状态变化信息用于网络传输
        /// </summary>
        /// <returns>包含法力消耗信息的匿名对象</returns>
        /// <remarks>
        /// <para>
        /// 网络数据特点：
        /// - 包含完整的前后状态对比信息
        /// - 支持四色法力系统的精确同步
        /// - 提供消耗来源的详细说明
        /// - 便于客户端进行状态验证和UI更新
        /// </para>
        ///
        /// <para>
        /// 数据验证：
        /// - ManaBefore数组长度必须为4
        /// - ManaConsumed数组长度必须为4
        /// - 各颜色法力值不能为负数
        /// - 消耗量不能超过原有法力量
        /// </para>
        /// </remarks>
        public override object ToNetworkData()
        {
            // 创建匿名对象包含法力消耗的所有相关信息
            return new
            {
                // 事件基础信息
                EventType = EventType.ToString(),          // 事件类型字符串
                Timestamp = Timestamp.Ticks,               // 精确时间戳
                SourcePlayerId,                            // 消耗来源玩家

                // 法力状态信息
                ManaBefore,                                // 消耗前法力数组
                ManaConsumed,                              // 实际消耗法力数组
                Source,                                    // 消耗来源说明

                // 附加数据
                Data                                       // 事件附加数据
            };
        }

        /// <summary>
        /// 从网络数据重建法力消耗事件
        /// 将接收到的网络数据反序列化为ManaConsumeEvent对象
        /// </summary>
        /// <param name="data">来自网络的序列化法力事件数据</param>
        /// <returns>重建的ManaConsumeEvent实例</returns>
        /// <remarks>
        /// <para>
        /// TODO: 需要实现完整的反序列化逻辑：
        /// - 解析法力数组并验证数据完整性
        /// - 验证法力值的合理性和范围
        /// - 重建事件的时间戳和来源信息
        /// - 处理版本差异和数据格式变更
        /// </para>
        ///
        /// <para>
        /// 数据验证要求：
        /// - 确保数组长度为4（四色法力）
        /// - 验证法力值非负
        /// - 检查消耗量的合理性
        /// - 验证时间戳的有效性
        /// </para>
        /// </remarks>
        public override GameEvent FromNetworkData(object data)
        {
            // TODO: 实现完整的网络数据重建逻辑
            // 1. 解析并验证法力数组数据
            // 2. 重建ManaConsumeEvent的所有属性
            // 3. 验证数据的合法性和一致性
            // 4. 处理数据异常和错误情况

            return this; // 临时返回当前实例，需要实现完整的重建逻辑
        }
    }

    /// <summary>
    /// 伤害事件类
    /// 记录战斗中伤害发生的详细信息，包括来源、目标、数值和类型
    /// 这是战斗系统中最重要的同步事件，直接影响战斗结果
    /// </summary>
    /// <remarks>
    /// <para>
    /// 伤害类型说明：
    /// - 物理伤害：普通攻击造成的伤害
    /// - 魔法伤害：技能和法术造成的伤害
    /// - 真实伤害：无视格挡和护盾的伤害
    /// - 反弹伤害：攻击敌人时受到的反伤
    /// - 状态伤害：中毒、灼烧等持续状态效果
    /// </para>
    ///
    /// <para>
    /// 伤害计算流程：
    /// 1. 确定伤害来源和目标
    /// 2. 计算基础伤害数值
    /// 3. 应用伤害修正（加成、减伤等）
    /// 4. 处理格挡和护盾减免
    /// 5. 扣除最终伤害值
    /// 6. 触发相应的事件和效果
    /// </para>
    /// </remarks>
    /// <param name="sourcePlayerId">造成伤害的玩家ID</param>
    /// <param name="sourceId">伤害来源单位的ID（玩家、敌人、卡牌等）</param>
    /// <param name="targetId">承受伤害的目标单位ID</param>
    /// <param name="damageAmount">实际造成的伤害数值</param>
    /// <param name="damageType">伤害的类型分类</param>
    /// <param name="additionalData">附加数据，可能包含伤害计算的详细过程</param>
    public class DamageEvent(string sourcePlayerId, string sourceId, string targetId,
        int damageAmount, string damageType, object additionalData = null) : GameEvent(GameEventType.DamageDealt, sourcePlayerId, additionalData)
    {
        /// <summary>
        /// 伤害来源单位ID
        /// 标识造成伤害的具体单位，可能是玩家、敌人、卡牌效果等
        /// </summary>
        public string SourceId { get; private set; } = sourceId;

        /// <summary>
        /// 伤害目标单位ID
        /// 标识承受伤害的目标单位，可能是玩家、敌人、召唤物等
        /// </summary>
        public string TargetId { get; private set; } = targetId;

        /// <summary>
        /// 伤害数值
        /// 实际造成的基础伤害量，不包含格挡和护盾减免
        /// </summary>
        public int DamageAmount { get; private set; } = damageAmount;

        /// <summary>
        /// 伤害类型
        /// 标识伤害的分类，影响伤害计算和特效触发
        /// </summary>
        public string DamageType { get; private set; } = damageType ?? "Unknown";

        /// <summary>
        /// 将伤害事件转换为网络数据格式
        /// 序列化伤害相关的所有信息用于网络传输
        /// </summary>
        /// <returns>包含伤害信息的匿名对象</returns>
        /// <remarks>
        /// <para>
        /// 网络数据包含：
        /// - 伤害来源和目标的完整标识
        /// - 伤害数值和类型的详细信息
        /// - 事件发生的时间和玩家信息
        /// - 可能的伤害计算过程数据
        /// </para>
        ///
        /// <para>
        /// 同步重要性：
        /// - 伤害数值影响战斗平衡和游戏结果
        /// - 伤害类型影响后续效果触发
        /// - 来源和目标关系影响战斗逻辑
        /// - 时间戳确保事件顺序的正确性
        /// </para>
        /// </remarks>
        public override object ToNetworkData()
        {
            // 创建匿名对象包含伤害事件的所有关键信息
            return new
            {
                // 事件基础信息
                EventType = EventType.ToString(),          // 事件类型标识
                Timestamp = Timestamp.Ticks,               // 精确发生时间
                SourcePlayerId,                            // 触发事件的玩家

                // 伤害核心信息
                SourceId,                                  // 伤害来源单位ID
                TargetId,                                  // 伤害目标单位ID
                DamageAmount,                              // 实际伤害数值
                DamageType,                                // 伤害类型分类

                // 附加数据
                Data                                       // 事件附加信息
            };
        }

        /// <summary>
        /// 从网络数据重建伤害事件
        /// 将接收到的网络数据反序列化为DamageEvent对象
        /// </summary>
        /// <param name="data">来自网络的序列化伤害事件数据</param>
        /// <returns>重建的DamageEvent实例</returns>
        /// <remarks>
        /// <para>
        /// TODO: 需要实现完整的反序列化逻辑：
        /// - 解析伤害数值并验证有效性
        /// - 验证来源和目标ID的合法性
        /// - 重建伤害类型和相关属性
        /// - 处理数据版本兼容性问题
        /// </para>
        ///
        /// <para>
        /// 数据验证重点：
        /// - 伤害数值必须为正整数
        /// - 来源和目标ID不能为空
        /// - 伤害类型必须是已知类型
        /// - 时间戳应该在合理范围内
        /// </para>
        /// </remarks>
        public override GameEvent FromNetworkData(object data)
        {
            // TODO: 实现完整的网络数据重建逻辑
            // 1. 解析并验证伤害数值和类型
            // 2. 重建来源和目标的标识信息
            // 3. 验证数据的完整性和一致性
            // 4. 处理异常情况和错误数据

            return this; // 临时返回当前实例，需要实现完整的重建逻辑
        }
    }

    /// <summary>
    /// 事件工厂类
    /// 提供统一的游戏事件创建接口，封装事件对象的创建逻辑
    /// 支持从游戏状态创建事件，以及从网络数据重建事件
    /// </summary>
    /// <remarks>
    /// <para>
    /// 工厂模式的优势：
    /// - 统一事件创建接口，简化调用代码
    /// - 封装复杂的创建逻辑和参数验证
    /// - 支持事件的批量和条件创建
    /// - 便于事件类型的扩展和管理
    /// </para>
    ///
    /// <para>
    /// 设计原则：
    /// - 单一职责：专注于事件对象的创建
    /// - 开闭原则：便于添加新的事件类型
    /// - 依赖倒置：依赖抽象而非具体实现
    /// - 接口隔离：提供简洁清晰的工厂接口
    /// </para>
    ///
    /// <para>
    /// TODO: 待完善的功能：
    /// - 添加更多事件类型的创建方法
    /// - 实现从网络数据的智能重建
    /// - 添加事件创建的参数验证
    /// - 支持事件模板和预设配置
    /// </para>
    /// </remarks>
    public static class GameEventFactory
    {
        /// <summary>
        /// 创建卡牌使用事件
        /// 根据提供的卡牌信息创建CardPlayEvent对象
        /// </summary>
        /// <param name="playerId">使用卡牌的玩家ID</param>
        /// <param name="cardId">卡牌的唯一标识符</param>
        /// <param name="cardName">卡牌的显示名称</param>
        /// <param name="cardType">卡牌的类型分类</param>
        /// <param name="manaCost">卡牌的法力消耗数组</param>
        /// <param name="targetSelector">目标选择器字符串</param>
        /// <returns>创建的CardPlayEvent对象</returns>
        /// <remarks>
        /// <para>
        /// 参数验证建议：
        /// - 确保playerId不为空或null
        /// - 验证cardId的格式和有效性
        /// - 检查manaCost数组长度和内容
        /// - 验证cardType是否为已知类型
        /// </para>
        ///
        /// <para>
        /// 使用场景：
        /// - 玩家在UI中选择并使用卡牌
        /// - 系统自动触发卡牌效果
        /// - 网络同步重建卡牌使用事件
        /// - 游戏回放和测试场景
        /// </para>
        /// </remarks>
        public static CardPlayEvent CreateCardPlayEvent(string playerId, string cardId, string cardName,
            string cardType, int[] manaCost, string targetSelector)
        {
            // 直接创建并返回新的CardPlayEvent实例
            return new CardPlayEvent(playerId, cardId, cardName, cardType, manaCost, targetSelector);
        }

        /// <summary>
        /// 创建法力消耗事件
        /// 根据法力变化信息创建ManaConsumeEvent对象
        /// </summary>
        /// <param name="playerId">消耗法力的玩家ID</param>
        /// <param name="manaBefore">消耗前的法力值数组</param>
        /// <param name="manaConsumed">消耗的法力值数组</param>
        /// <param name="source">法力消耗的来源说明</param>
        /// <returns>创建的ManaConsumeEvent对象</returns>
        /// <remarks>
        /// <para>
        /// 参数检查建议：
        /// - 验证playerId的有效性
        /// - 确保法力数组长度为4
        /// - 检查法力值的合理性
        /// - 验证source描述的清晰性
        /// </para>
        ///
        /// <para>
        /// 数据完整性：
        /// - manaBefore应反映真实的状态
        /// - manaConsumed应与实际消耗匹配
        /// - source应清楚说明消耗原因
        /// - 数组顺序应为[红,白,黑,无]
        /// </para>
        /// </remarks>
        public static ManaConsumeEvent CreateManaConsumeEvent(string playerId, int[] manaBefore,
            int[] manaConsumed, string source)
        {
            // 直接创建并返回新的ManaConsumeEvent实例
            return new ManaConsumeEvent(playerId, manaBefore, manaConsumed, source);
        }

        /// <summary>
        /// 创建伤害事件
        /// 根据伤害信息创建DamageEvent对象
        /// </summary>
        /// <param name="playerId">造成伤害的玩家ID</param>
        /// <param name="sourceId">伤害来源单位ID</param>
        /// <param name="targetId">伤害目标单位ID</param>
        /// <param name="damageAmount">伤害数值</param>
        /// <param name="damageType">伤害类型</param>
        /// <returns>创建的DamageEvent对象</returns>
        /// <remarks>
        /// <para>
        /// 参数验证建议：
        /// - 确保damageAmount为正整数
        /// - 验证sourceId和targetId不为空
        /// - 检查damageType的有效性
        /// - 确认playerId的合法性
        /// </para>
        ///
        /// <para>
        /// 伤害类型标准化：
        /// - 建议使用预定义的伤害类型枚举
        /// - 保持大小写一致性
        /// - 支持多语言本地化
        /// - 考虑扩展新的伤害类型
        /// </para>
        /// </remarks>
        public static DamageEvent CreateDamageEvent(string playerId, string sourceId, string targetId,
            int damageAmount, string damageType)
        {
            // 直接创建并返回新的DamageEvent实例
            return new DamageEvent(playerId, sourceId, targetId, damageAmount, damageType);
        }

        /// <summary>
        /// 从网络数据创建事件
        /// 根据网络传输的数据智能重建对应的游戏事件对象
        /// </summary>
        /// <param name="data">来自网络的序列化事件数据</param>
        /// <returns>重建的GameEvent对象，失败时返回null</returns>
        /// <remarks>
        /// <para>
        /// TODO: 需要实现的智能重建逻辑：
        /// - 解析数据中的事件类型标识
        /// - 根据类型选择对应的重建方法
        /// - 验证网络数据的完整性和格式
        /// - 处理版本兼容和数据迁移
        /// </para>
        ///
        /// <para>
        /// 重建策略：
        /// 1. 解析EventType字段确定事件类型
        /// 2. 根据类型调用相应的构造函数
        /// 3. 填充事件的所有属性和字段
        /// 4. 验证重建事件的有效性
        /// 5. 返回完整的事件对象
        /// </para>
        ///
        /// <para>
        /// 错误处理：
        /// - 网络数据格式错误时返回null
        /// - 事件类型未知时记录警告
        /// - 缺少必要字段时抛出异常
        /// - 数据验证失败时提供详细错误信息
        /// </para>
        /// </remarks>
        public static GameEvent CreateEventFromNetworkData(object data)
        {
            // TODO: 实现完整的网络数据事件重建逻辑
            // 1. 解析数据中的事件类型信息
            // 2. 根据事件类型选择重建策略
            // 3. 验证数据完整性并重建事件
            // 4. 处理各种异常和错误情况
            // 5. 返回重建完成的事件对象

            return null; // 临时返回null，需要实现完整的重建逻辑
        }
    }
}