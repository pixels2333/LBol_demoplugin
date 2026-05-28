namespace NetworkPlugin.Network.MidGameJoin.Result;

/// <summary>
/// 加入请求
/// </summary>
public class GameJoinRequest
{
    /// <summary>请求ID</summary>
    public string RequestId { get; set; } = string.Empty;
    /// <summary>房间ID</summary>
    public string RoomId { get; set; } = string.Empty;
    /// <summary>玩家名称</summary>
    public string PlayerName { get; set; } = string.Empty;
    /// <summary>客户端玩家ID</summary>
    public string ClientPlayerId { get; set; } = string.Empty;
    /// <summary>请求时间（UTC刻度）</summary>
    public long RequestTime { get; set; }
    /// <summary>请求状态</summary>
    public JoinRequestStatus Status { get; set; }
}
