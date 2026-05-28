using System;
using System.Collections.Generic;

namespace NetworkPlugin.Network.Room;

/// <summary>
/// 房间状态信息，用于向客户端展示房间列表
/// </summary>
public class RoomStatus
{
    /// <summary>房间ID</summary>
    public string RoomId { get; set; } = string.Empty;
    /// <summary>当前玩家数量</summary>
    public int PlayerCount { get; set; }
    /// <summary>最大玩家数</summary>
    public int MaxPlayers { get; set; }
    /// <summary>房主玩家ID</summary>
    public string HostPlayerId { get; set; } = string.Empty;
    /// <summary>是否在游戏中</summary>
    public bool IsInGame { get; set; }
    /// <summary>房间创建时间</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>房间内玩家ID列表</summary>
    public List<string> PlayerIds { get; set; } = [];
    /// <summary>房间延迟（毫秒）</summary>
    public int Ping { get; set; }
}
