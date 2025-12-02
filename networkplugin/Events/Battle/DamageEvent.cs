using System;
using NetworkPlugin.Events;

namespace NetworkPlugin.Events.Battle;

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