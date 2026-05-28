namespace NetworkPlugin.Network.MidGameJoin.Result;

/// <summary>
/// 加入请求结果，包含批准状态和引导状态
/// </summary>
public class JoinRequestResult : BaseResult
{
    /// <summary>是否已获得批准</summary>
    public bool HaveApproved { get; set; }
    /// <summary>请求ID</summary>
    public string? RequestId { get; set; }
    /// <summary>批准时提供的引导状态</summary>
    public PlayerBootstrappedState? BootstrapState { get; set; }

    /// <summary>创建待处理的加入请求结果</summary>
    public static JoinRequestResult Pending(string requestId) => new() { RequestId = requestId, HaveApproved = false };
    /// <summary>创建被拒绝的加入请求结果</summary>
    public static JoinRequestResult Denied(string errorMessage) => new() { ErrorMessage = errorMessage, HaveApproved = false };
    /// <summary>创建已批准的加入请求结果</summary>
    public static JoinRequestResult Approved(PlayerBootstrappedState state) => new() { BootstrapState = state, HaveApproved = true };
}
