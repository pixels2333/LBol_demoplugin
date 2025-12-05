namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 敌人意图快照
/// </summary>
public class IntentionSnapshot
{
    /// <summary>
    /// 意图类型（攻击/防御/特殊等）
    /// </summary>
    public string IntentionType { get; set; } = "None";

    /// <summary>
    /// 意图名称
    /// </summary>
    public string IntentionName { get; set; } = "";

    /// <summary>
    /// 意图值（伤害量/格挡量等）
    /// </summary>
    public int Value { get; set; } = 0;

    /// <summary>
    /// 目标ID
    /// </summary>
    public string? TargetId { get; set; }

    /// <summary>
    /// 意图描述
    /// </summary>
    public string Description { get; set; } = "";
}
