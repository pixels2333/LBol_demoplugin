namespace NetworkPlugin.Network.MidGameJoin;

/// <summary>
/// 中途加入配置
/// </summary>
public class MidGameJoinConfig
{
    public bool AllowMidGameJoin { get; set; } = true;
    public int JoinRequestTimeoutMinutes { get; set; } = 2;
    public int MaxJoinRequestsPerRoom { get; set; } = 5;
    public int AIControlTimeoutMinutes { get; set; } = 10;
    public bool EnableCompensation { get; set; } = true;
    public bool EnableAIPassthrough { get; set; } = true;
    public int CatchUpBatchSize { get; set; } = 50;
}
