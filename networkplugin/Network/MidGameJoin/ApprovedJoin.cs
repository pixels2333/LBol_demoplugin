namespace NetworkPlugin.Network.MidGameJoin;

/// <summary>
/// 批准加入的信息
/// </summary>
public class ApprovedJoin
{
    public string RequestId { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string HostPlayerId { get; set; } = string.Empty;
    public string ClientPlayerId { get; set; } = string.Empty;
    public string JoinToken { get; set; } = string.Empty;
    public long ApprovedAt { get; set; }
    public long ExpiresAt { get; set; }
    public PlayerBootstrappedState BootstrappedState { get; set; } = new();
}
