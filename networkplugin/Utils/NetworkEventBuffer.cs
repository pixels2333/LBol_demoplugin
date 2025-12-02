using System;
using System.Collections.Generic;

namespace NetworkPlugin.Utils;

/// <summary>
/// 网络事件缓冲区项
/// 用于存储远程事件的包装器，包含时间戳验证和状态跟踪
/// </summary>
internal class NetworkEventBuffer
{
    /// <summary>
    /// 事件的时间戳
    /// 用于排序和时序验证
    /// </summary>
    public long Timestamp { get; }

    /// <summary>
    /// 原始网络事件数据
    /// 从网络接收的原始事件字典
    /// </summary>
    public Dictionary<string, object> OriginalData { get; }

    /// <summary>
    /// 事件接收时间
    /// 用于计算事件延迟和超时判断
    /// </summary>
    public DateTime ReceivedAt { get; }

    /// <summary>
    /// 事件处理状态
    /// 标记事件是否已被处理或需要特殊处理
    /// </summary>
    public ProcessingStatus Status { get; set; }

    /// <summary>
    /// 主构造函数
    /// </summary>
    /// <param name="timestamp">事件时间戳</param>
    /// <param name="originalData">原始网络数据</param>
    public NetworkEventBuffer(long timestamp, Dictionary<string, object> originalData)
    {
        Timestamp = timestamp;
        OriginalData = originalData ?? throw new ArgumentNullException(nameof(originalData));
        ReceivedAt = DateTime.Now;
        Status = ProcessingStatus.Pending;
    }

    /// <summary>
    /// 检查事件是否已超时
    /// </summary>
    /// <param name="timeout">超时时间阈值</param>
    /// <returns>如果事件超时返回true</returns>
    public bool IsTimeout(TimeSpan timeout)
    {
        return DateTime.Now - ReceivedAt > timeout;
    }

    /// <summary>
    /// 事件处理状态枚举
    /// </summary>
    public enum ProcessingStatus
    {
        /// <summary>
        /// 等待处理
        /// </summary>
        Pending,

        /// <summary>
        /// 正在处理中
        /// </summary>
        Processing,

        /// <summary>
        /// 处理完成
        /// </summary>
        Completed,

        /// <summary>
        /// 已丢弃（无效或超时）
        /// </summary>
        Discarded
    }
}