#nullable enable
namespace NetworkPlugin.Network.Server;

/// <summary>
/// 加入房间结果
/// </summary>
/// <summary>
/// 加入房间操作的结果
/// </summary>
public class JoinResult
{
    /// <summary>操作是否成功</summary>
    public bool IsSuccess { get; set; }
    /// <summary>失败时的错误消息</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 创建成功结果
    /// </summary>
    /// <returns>成功结果实例</returns>
    public static JoinResult Success()
    {
        return new JoinResult { IsSuccess = true };
    }

    /// <summary>
    /// 创建失败结果
    /// </summary>
    /// <param name="errorMessage">错误描述</param>
    /// <returns>失败结果实例</returns>
    public static JoinResult Failed(string errorMessage)
    {
        return new JoinResult { IsSuccess = false, ErrorMessage = errorMessage };
    }
}
