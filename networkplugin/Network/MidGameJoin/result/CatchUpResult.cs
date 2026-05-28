namespace NetworkPlugin.Network.MidGameJoin.Result;

/// <summary>
/// 追赶同步结果，包含成功应用的事件数量
/// </summary>
public class CatchUpResult : BaseResult
{
    /// <summary>成功应用的事件数量</summary>
    public int EventsApplied { get; set; }

    /// <summary>创建成功的追赶结果</summary>
    public static CatchUpResult Success(int eventsApplied) => new() { IsSuccess = true, EventsApplied = eventsApplied };
    /// <summary>创建失败的追赶结果</summary>
    public static CatchUpResult Failed(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
}
