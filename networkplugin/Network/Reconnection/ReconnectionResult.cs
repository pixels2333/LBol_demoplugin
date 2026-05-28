using NetworkPlugin.Network.Snapshot;

namespace NetworkPlugin.Network.Reconnection;

/// <summary>
/// 重连结果，包含是否成功、错误消息以及玩家状态快照
/// </summary>
public class ReconnectionResult
{
    /// <summary>是否成功</summary>
    public bool IsSuccess { get; set; }
    /// <summary>失败时的错误消息</summary>
    public string? ErrorMessage { get; set; }
    /// <summary>重连时的玩家状态快照</summary>
    public PlayerStateSnapshot? Snapshot { get; set; }

    /// <summary>
    /// 创建成功的重连结果
    /// </summary>
    /// <param name="snapshot">玩家状态快照</param>
    public static ReconnectionResult Success(PlayerStateSnapshot snapshot)
    {
        return new ReconnectionResult
        {
            IsSuccess = true,
            Snapshot = snapshot
        };
    }

    /// <summary>
    /// 创建失败的重连结果
    /// </summary>
    /// <param name="errorMessage">错误描述</param>
    public static ReconnectionResult Failed(string errorMessage)
    {
        return new ReconnectionResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}
