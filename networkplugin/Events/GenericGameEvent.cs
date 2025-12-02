using System;
using NetworkPlugin.Events;

namespace NetworkPlugin.Events;

/// <summary>
/// 通用游戏事件类
/// 用于处理未明确定义或通用的事件类型
/// 提供基本的事件结构和网络序列化能力
/// </summary>
/// <remarks>
/// <para>
/// 通用事件特点：
/// - 适用于所有未明确定义的事件类型
/// - 提供基本的事件字段和功能
/// - 支持网络序列化和反序列化
/// - 可扩展的事件处理框架
/// </para>
///
/// <para>
/// 使用场景：
/// - 新增事件类型未实现专门处理类时
/// - 通用事件的快速处理和调试
/// - 测试和原型开发阶段的事件
/// - 网络协议扩展支持
/// </para>
///
/// <para>
/// 事件字段：
/// - EventType: 事件类型字符串
/// - SourcePlayerId: 来源玩家ID
/// - Data: 事件数据载荷
/// - Timestamp: 事件时间戳
/// </para>
/// </remarks>
public class GenericGameEvent(string eventType, object data, DateTime timestamp) : GameEvent(ParseEventType(eventType), "unknown_player", data)
{
    /// <summary>
    /// 重写基类方法，提供通用的事件序列化
    /// 将事件转换为网络传输的数据格式
    /// </summary>
    /// <returns>网络传输格式的事件数据</returns>
    public override object ToNetworkData()
    {
        // 创建网络传输格式的事件数据
        return new
        {
            EventType = EventType.ToString(),    // 事件类型字符串
            Timestamp = Timestamp.Ticks,          // 时间戳（ticks格式）
            SourcePlayerId,                      // 来源玩家ID
            Data                              // 事件载荷数据
        };
    }

    /// <summary>
    /// 解析事件类型字符串为枚举值
    /// 支持事件类型的字符串到枚举的安全转换
    /// </summary>
    /// <param name="eventType">事件类型字符串</param>
    /// <returns>对应的游戏事件类型枚举</returns>
    /// <remarks>
    /// 解析规则：
    /// - 优先尝试解析为精确的枚举值
    /// - 解析失败时返回Error类型作为安全默认值
    /// - 确保类型安全性和处理意外事件类型
    /// </remarks>
    private static GameEventType ParseEventType(string eventType)
    {
        // 尝试将字符串解析为事件类型枚举
        return Enum.TryParse<GameEventType>(eventType, out var result) ? result : GameEventType.Error;
    }

    /// <summary>
    /// 对于通用事件，直接返回当前实例
    /// 因为通用事件不需要额外处理
    /// </summary>
    /// <param name="data">网络数据</param>
    /// <returns>恢复的事件实例</returns>
    public override GameEvent FromNetworkData(object data)
    {
        // 通用事件直接返回当前实例，数据已在构造函数中处理
        return this;
    }
}