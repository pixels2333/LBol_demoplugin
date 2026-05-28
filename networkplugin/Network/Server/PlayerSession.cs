using System;
using System.Collections.Generic;
using LiteNetLib;

namespace NetworkPlugin.Network.Server;

/// <summary>
/// 表示一个已连接玩家的会话状态，封装 LiteNetLib 对等端、身份、心跳及元数据。
/// </summary>
/// <remarks>
/// 由 <see cref="BaseGameServer"/> 在连接建立时创建，断线后保留于 <c>DisconnectedAtByPlayerId</c> 优雅期内，
/// 以支持断线重连时恢复原有 PlayerId 与 Metadata。
/// </remarks>
public class PlayerSession
{
    #region 会话核心属性

    /// <summary>
    /// LiteNetLib 对等端实例，表示与客户端的底层 UDP 连接。
    /// </summary>
    /// <remarks>
    /// 断连后此属性可能被清空，但会话对象本身会继续保留在优雅期内。
    /// </remarks>
    public NetPeer Peer { get; set; } = null!;

    /// <summary>
    /// 服务器分配的玩家唯一标识符。
    /// </summary>
    /// <remarks>
    /// 在首次连接时由 <see cref="BaseGameServer.CreatePlayerId"/> 生成；断线重连时保持不变。
    /// </remarks>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>玩家显示名称，供 UI 展示使用。</summary>
    public string PlayerName { get; set; } = string.Empty;

    /// <summary>当前所属房间 ID，空字符串表示未加入任何房间。</summary>
    public string CurrentRoomId { get; set; } = string.Empty;

    /// <summary>会话建立时间（UTC）。</summary>
    public DateTime ConnectedAt { get; set; }

    /// <summary>
    /// 最后一次收到心跳的时间（UTC），用于超时检测。
    /// </summary>
    public DateTime LastHeartbeat { get; set; }

    /// <summary>
    /// 最后一次收到任意消息的时间（UTC），用于活跃度评估。
    /// </summary>
    public DateTime LastMessageAt { get; set; }

    #endregion

    #region 网络质量与状态

    /// <summary>
    /// 往返延迟（RTT，毫秒），由 LiteNetLib 根据 ACK 时间自动计算。
    /// </summary>
    public int Ping { get; set; }

    /// <summary>会话是否仍保持活跃连接。</summary>
    /// <remarks>断连后被设为 false，但会话对象可能仍在优雅期内保留。</remarks>
    public bool IsConnected { get; set; }

    /// <summary>是否为当前房间的房主（Host）。</summary>
    /// <remarks>
    /// 房主是房间状态的中枢：负责处理 RoomStateRequest、FullStateSyncRequest 等定向路由。
    /// </remarks>
    public bool IsHost { get; set; }

    #endregion

    #region 扩展数据

    /// <summary>
    /// 玩家扩展元数据字典，用于存储 CharacterId、Location、Stage 等动态属性。
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = [];

    #endregion

    #region 便捷属性与方法

    /// <summary>远程端点地址字符串，未连接时返回 "unknown"。</summary>
    public string RemoteEndPoint => Peer.EndPoint?.ToString() ?? "unknown";

    /// <summary>检查心跳是否超过指定超时阈值。</summary>
    /// <param name="timeoutSeconds">超时阈值（秒），默认 30 秒。</param>
    /// <returns>超过阈值返回 true，表示连接可能已失效。</returns>
    public bool IsTimeout(int timeoutSeconds = 30)
    {
        return (DateTime.UtcNow - LastHeartbeat).TotalSeconds > timeoutSeconds;
    }

    /// <summary>刷新心跳时间戳到当前 UTC 时间。</summary>
    public void UpdateHeartbeat()
    {
        LastHeartbeat = DateTime.UtcNow;
    }

    /// <summary>刷新最后消息时间戳到当前 UTC 时间。</summary>
    public void UpdateMessageTime()
    {
        LastMessageAt = DateTime.UtcNow;
    }

    #endregion
}
