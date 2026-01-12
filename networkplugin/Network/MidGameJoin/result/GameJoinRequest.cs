namespace NetworkPlugin.Network.MidGameJoin.Result;

/// <summary>
/// 加入请求
/// </summary>
public class GameJoinRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string ClientPlayerId { get; set; } = string.Empty;
    public long RequestTime { get; set; }
    public JoinRequestStatus Status { get; set; }
}
