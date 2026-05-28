#nullable enable
namespace NetworkPlugin.Network.Server;

/// <summary>
/// 表示加入房间操作的结果，包含成功/失败状态及可选错误信息。
/// </summary>
public class JoinResult
{
    /// <summary>操作是否成功。</summary>
    public bool IsSuccess { get; set; }

    /// <summary>失败时的错误消息；成功时通常为 null。</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>创建成功结果。</summary>
    /// <returns>IsSuccess = true 的实例。</returns>
    public static JoinResult Success()
    {
        return new JoinResult { IsSuccess = true };
    }

    /// <summary>创建失败结果。</summary>
    /// <param name="errorMessage">错误描述，供 UI 展示或日志记录。</param>
    /// <returns>IsSuccess = false 且携带错误信息的实例。</returns>
    public static JoinResult Failed(string errorMessage)
    {
        return new JoinResult { IsSuccess = false, ErrorMessage = errorMessage };
    }
}
