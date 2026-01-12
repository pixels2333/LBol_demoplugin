// ============================================================================
// 服务器内核会话类
// ============================================================================
// 说明：仅包含网络层的连接与活跃信息，避免把游戏层状态耦合进内核。
// 职责：管理网络对等连接的基本信息，包括连接时间、最后活动时间和心跳检测。
// ============================================================================

using System;
using LiteNetLib;

namespace NetworkPlugin.Network.Server.Core;

/// <summary>
/// 核心对等会话类
/// </summary>
/// <remarks>
/// 该类封装了网络对等连接的基本信息，用于服务器内核管理网络连接。
/// 设计原则：保持轻量级，仅包含网络层状态，避免业务逻辑耦合。
/// </remarks>
public sealed class CorePeerSession
{
    #region 属性定义
    
    /// <summary>
    /// 网络对等连接实例
    /// </summary>
    public NetPeer Peer { get; set; }
    
    /// <summary>
    /// 连接建立时间（UTC）
    /// </summary>
    public DateTime ConnectedAt { get; set; }
    
    /// <summary>
    /// 最后活动时间（UTC）
    /// </summary>
    public DateTime LastSeenAt { get; private set; }
    
    /// <summary>
    /// 当前网络延迟（毫秒）
    /// </summary>
    public int Ping { get; internal set; }

    #endregion

    #region 公共方法
    
    /// <summary>
    /// 标记会话为活动状态
    /// </summary>
    /// <param name="nowUtc">当前UTC时间</param>
    public void MarkSeen(DateTime nowUtc)
    {
        LastSeenAt = nowUtc;
    }

    /// <summary>
    /// 检查会话是否超时
    /// </summary>
    /// <param name="timeoutSeconds">超时时间（秒）</param>
    /// <param name="nowUtc">当前UTC时间</param>
    /// <returns>如果超时返回true，否则返回false</returns>
    public bool IsTimeout(int timeoutSeconds, DateTime nowUtc)
    {
        return (nowUtc - LastSeenAt).TotalSeconds > timeoutSeconds;
    }
    
    #endregion

    #region 私有方法（如有需要可在此区域添加）
    // 私有方法区域 - 当前为空
    #endregion
}

