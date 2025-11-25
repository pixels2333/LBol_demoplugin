using System;
using LiteNetLib;

namespace NetworkPlugin.Network.Server;

/// <summary>
/// 玩家会话 - 管理单个玩家的会话信息
/// </summary>
public class PlayerSession
{
    /// <summary>
    /// LiteNetLib对等节点
    /// </summary>
    public NetPeer Peer { get; set; } = null!;

    /// <summary>
    /// 玩家唯一ID
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>
    /// 玩家显示名称
    /// </summary>
    public string PlayerName { get; set; } = string.Empty;

    /// <summary>
    /// 当前所在的房间ID
    /// </summary>
    public string CurrentRoomId { get; set; } = string.Empty;

    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime ConnectedAt { get; set; }

    /// <summary>
    /// 最后心跳时间
    /// </summary>
    public DateTime LastHeartbeat { get; set; }

    /// <summary>
    /// 最后接收消息时间
    /// </summary>
    public DateTime LastMessageAt { get; set; }

    /// <summary>
    /// 延迟（毫秒）
    /// </summary>
    public int Ping { get; set; }

    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// 是否为房主
    /// </summary>
    public bool IsHost { get; set; }

    /// <summary>
    /// 玩家元数据（可用于存储额外信息）
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// 获取远程终结点
    /// </summary>
    public string RemoteEndPoint => Peer.EndPoint?.ToString() ?? "unknown";

    /// <summary>
    /// 检查连接是否超时
    /// </summary>
    /// <param name="timeoutSeconds">超时时间（秒）</param>
    /// <returns></returns>
    public bool IsTimeout(int timeoutSeconds = 30)
    {
        return (DateTime.UtcNow - LastHeartbeat).TotalSeconds > timeoutSeconds;
    }

    /// <summary>
    /// 更新心跳
    /// </summary>
    public void UpdateHeartbeat()
    {
        LastHeartbeat = DateTime.UtcNow;
    }

    /// <summary>
    /// 更新消息时间
    /// </summary>
    public void UpdateMessageTime()
    {
        LastMessageAt = DateTime.UtcNow;
    }
}
