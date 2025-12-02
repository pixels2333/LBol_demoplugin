using System;
using NetworkPlugin.Events;

namespace NetworkPlugin.Events.Cards;

/// <summary>
/// 卡牌使用事件类
/// 记录玩家使用卡牌的详细信息，包括卡牌属性、消耗和目标选择
/// 这是游戏中最核心和最频繁的事件类型之一
/// </summary>
/// <param name="sourcePlayerId">使用卡牌的玩家ID</param>
/// <param name="cardId">卡牌的唯一标识符</param>
/// <param name="cardName">卡牌的显示名称</param>
/// <param name="cardType">卡牌的类型分类（攻击/技能/能力等）</param>
/// <param name="manaCost">卡牌的四色法力消耗数组</param>
/// <param name="targetSelector">目标选择器，标识卡牌的作用目标</param>
/// <param name="additionalData">附加数据，可包含特殊效果或元数据</param>
public class CardPlayEvent(string sourcePlayerId, string cardId, string cardName, string cardType,
                        int[] manaCost, string targetSelector, object additionalData = null)
            : GameEvent(GameEventType.CardPlayStart, sourcePlayerId, additionalData)
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
    /// TODO:实际上有五种
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