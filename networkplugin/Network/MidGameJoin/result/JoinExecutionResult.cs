namespace NetworkPlugin.Network.MidGameJoin.Result;

/// <summary>
/// 加入执行结果，包含分配的玩家ID和引导状态
/// </summary>
public class JoinExecutionResult : BaseResult
{
    /// <summary>分配的玩家ID</summary>
    public string? PlayerId { get; set; }
    /// <summary>引导状态</summary>
    public PlayerBootstrappedState? BootstrapState { get; set; }

    /// <summary>创建成功的加入执行结果</summary>
    public static JoinExecutionResult Success(string playerId, PlayerBootstrappedState state)
        => new() { IsSuccess = true, PlayerId = playerId, BootstrapState = state };

    /// <summary>创建失败的加入执行结果</summary>
    public static JoinExecutionResult Failed(string errorMessage)
        => new() { ErrorMessage = errorMessage, IsSuccess = false };
}
