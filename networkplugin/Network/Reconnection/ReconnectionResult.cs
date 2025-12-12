using NetworkPlugin.Network.Snapshot;

namespace NetworkPlugin.Network.Reconnection;

/// <summary>
/// 重连结果
/// </summary>
public class ReconnectionResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public PlayerStateSnapshot? Snapshot { get; set; }

    public static ReconnectionResult Success(PlayerStateSnapshot snapshot)
    {
        return new ReconnectionResult
        {
            IsSuccess = true,
            Snapshot = snapshot
        };
    }

    public static ReconnectionResult Failed(string errorMessage)
    {
        return new ReconnectionResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}
