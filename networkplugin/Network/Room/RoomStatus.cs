using System;
using System.Collections.Generic;

namespace NetworkPlugin.Network.Room;

/// <summary>
/// 房间状态信息 DTO，用于向客户端大厅列表展示房间概要。
/// </summary>
public class RoomStatus
{
    /// <summary>房间唯一标识。</summary>
    public string RoomId { get; set; } = string.Empty;

    /// <summary>当前玩家数量。</summary>
    public int PlayerCount { get; set; }

    /// <summary>最大玩家容量。</summary>
    public int MaxPlayers { get; set; }

    /// <summary>当前房主 PlayerId。</summary>
    public string HostPlayerId { get; set; } = string.Empty;

    /// <summary>房间是否已开局。</summary>
    public bool IsInGame { get; set; }

    /// <summary>房间创建时间（UTC）。</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>房间内所有玩家 ID 列表。</summary>
    public List<string> PlayerIds { get; set; } = [];

    /// <summary>房间延迟（毫秒），通常取房内玩家平均 Ping。</summary>
    public int Ping { get; set; }
}
