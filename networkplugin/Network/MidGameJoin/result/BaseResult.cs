namespace NetworkPlugin.Network.MidGameJoin.Result;

public abstract class BaseResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
