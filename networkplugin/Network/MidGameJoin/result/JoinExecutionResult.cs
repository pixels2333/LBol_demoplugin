namespace NetworkPlugin.Network.MidGameJoin.Result;

public class JoinExecutionResult : BaseResult
{
    public string? PlayerId { get; set; }
    public PlayerBootstrappedState? BootstrapState { get; set; }

    public static JoinExecutionResult Success(string playerId, PlayerBootstrappedState state) => new() { Success = true, PlayerId = playerId, BootstrapState = state };
    public static JoinExecutionResult Failed(string errorMessage) => new() { ErrorMessage = errorMessage, Success = false };
}
