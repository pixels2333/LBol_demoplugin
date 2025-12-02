using System;
using NetworkPlugin.Events;

namespace NetworkPlugin.Events.Mana;

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