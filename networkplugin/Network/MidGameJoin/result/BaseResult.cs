namespace NetworkPlugin.Network.MidGameJoin;

public abstract class BaseResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
