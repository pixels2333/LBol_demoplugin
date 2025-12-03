using System;
using System.Collections.Generic;
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
    public NetPeer Peer { get; set; } = null!; // LiteNetLib网络对等节点：表示与客户端的网络连接

    /// <summary>
    /// 玩家唯一ID
    /// </summary>
    public string PlayerId { get; set; } = string.Empty; // 玩家唯一标识符：用于识别网络中的特定玩家

    /// <summary>
    /// 玩家显示名称
    /// </summary>
    public string PlayerName { get; set; } = string.Empty; // 玩家显示名称：在游戏界面中显示的玩家名称

    /// <summary>
    /// 当前所在的房间ID
    /// </summary>
    public string CurrentRoomId { get; set; } = string.Empty; // 当前房间ID：玩家所属的游戏房间标识符

    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime ConnectedAt { get; set; } // 连接时间：玩家加入网络的时间戳

    /// <summary>
    /// 最后心跳时间
    /// </summary>
    public DateTime LastHeartbeat { get; set; } // 最后心跳时间：用于检测玩家连接状态的最后活跃时间

    /// <summary>
    /// 最后接收消息时间
    /// </summary>
    public DateTime LastMessageAt { get; set; } // 最后消息时间：最后接收网络消息的时间戳

    /// <summary>
    /// 延迟（毫秒）
    /// </summary>
    public int Ping { get; set; } // 网络延迟：玩家与服务器之间的ping时间，用于网络质量评估

    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected { get; set; } // 连接状态：表示玩家当前是否保持网络连接

    /// <summary>
    /// 是否为房主
    /// </summary>
    public bool IsHost { get; set; } // 房主标识：表示玩家是否为当前房间的房主

    /// <summary>
    /// 玩家元数据（可用于存储额外信息）
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = []; // 玩家元数据：存储玩家的自定义扩展信息

    /// <summary>
    /// 获取远程终结点
    /// </summary>
    public string RemoteEndPoint => Peer.EndPoint?.ToString() ?? "unknown"; // 远程网络地址：获取客户端的IP地址和端口号

    /// <summary>
    /// 检查连接是否超时
    /// </summary>
    /// <param name="timeoutSeconds">超时时间（秒）</param>
    /// <returns></returns>
    public bool IsTimeout(int timeoutSeconds = 30)
    {
        return (DateTime.UtcNow - LastHeartbeat).TotalSeconds > timeoutSeconds;
    } // 检查连接超时：判断玩家心跳是否超过指定时间，用于自动断开超时连接

    /// <summary>
    /// 更新心跳
    /// </summary>
    public void UpdateHeartbeat()
    {
        LastHeartbeat = DateTime.UtcNow;
    } // 更新心跳：刷新玩家的最后心跳时间，用于保持连接活跃状态

    /// <summary>
    /// 更新消息时间
    /// </summary>
    public void UpdateMessageTime()
    {
        LastMessageAt = DateTime.UtcNow;
    } // 更新消息时间：刷新最后接收消息的时间戳，用于消息同步状态追踪
} // 玩家会话类：管理单个玩家的网络连接信息、心跳状态和会话数据
