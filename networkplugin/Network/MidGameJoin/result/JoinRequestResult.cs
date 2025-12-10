namespace NetworkPlugin.Network.MidGameJoin;

// Result classes
public class JoinRequestResult : BaseResult
{
    public bool HaveApproved { get; set; }
    public string? RequestId { get; set; }
    public PlayerBootstrappedState? BootstrapState { get; set; }

    public static JoinRequestResult Pending(string requestId) => new() { RequestId = requestId, HaveApproved = false };
    public static JoinRequestResult Denied(string errorMessage) => new() { ErrorMessage = errorMessage, HaveApproved = false };
    public static JoinRequestResult Approved(PlayerBootstrappedState state) => new() { BootstrapState = state, HaveApproved = true };
}
