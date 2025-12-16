namespace NetworkPlugin.Network.MidGameJoin.Result;

public  class BaseResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    public static BaseResult Success() => new() { IsSuccess = true };
    public static BaseResult Failed(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
}
