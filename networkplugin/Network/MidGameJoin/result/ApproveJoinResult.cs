namespace NetworkPlugin.Network.MidGameJoin.Result;

/// <summary>
/// 批准加入结果，包含加入令牌和引导状态
/// </summary>
public class ApproveJoinResult : BaseResult
{
    /// <summary>批准时颁发的加入令牌</summary>
    public string? JoinToken { get; set; }
    /// <summary>批准时提供的引导状态</summary>
    public PlayerBootstrappedState? BootstrapState { get; set; }

    /// <summary>创建成功的批准结果</summary>
    public static ApproveJoinResult Success(string joinToken, PlayerBootstrappedState state) => new() { JoinToken = joinToken, BootstrapState = state };
    /// <summary>创建失败的批准结果</summary>
    public static ApproveJoinResult Failed(string errorMessage) => new() { ErrorMessage = errorMessage };
}
