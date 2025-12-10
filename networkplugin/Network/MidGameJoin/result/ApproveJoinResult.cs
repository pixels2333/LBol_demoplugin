namespace NetworkPlugin.Network.MidGameJoin;

public class ApproveJoinResult : BaseResult
{
    public string? JoinToken { get; set; }
    public PlayerBootstrappedState? BootstrapState { get; set; }

    public static ApproveJoinResult Success(string joinToken, PlayerBootstrappedState state) => new() { JoinToken = joinToken, BootstrapState = state };
    public static ApproveJoinResult Failed(string errorMessage) => new() { ErrorMessage = errorMessage };
}
