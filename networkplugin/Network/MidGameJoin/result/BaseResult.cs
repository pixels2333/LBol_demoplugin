namespace NetworkPlugin.Network.MidGameJoin.Result;

/// <summary>
/// 基础结果类，包含操作是否成功和错误消息
/// </summary>
public class BaseResult
{
    /// <summary>操作是否成功</summary>
    public bool IsSuccess { get; set; }
    /// <summary>失败时的错误消息</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>创建成功结果</summary>
    public static BaseResult Success() => new() { IsSuccess = true };
    /// <summary>创建失败结果</summary>
    /// <param name="errorMessage">错误描述</param>
    public static BaseResult Failed(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
}
