namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 位置快照
/// </summary>
public class LocationSnapshot
{
    /// <summary>
    /// X坐标
    /// </summary>
    public int X { get; set; } = 0;

    /// <summary>
    /// Y坐标
    /// </summary>
    public int Y { get; set; } = 0;

    /// <summary>
    /// 节点ID
    /// </summary>
    public string NodeId { get; set; } = "";

    /// <summary>
    /// 节点类型（Battle/Shop/Rest/Event等）
    /// </summary>
    public string NodeType { get; set; } = "Unknown";

    /// <summary>
    /// 是否是分支点
    /// </summary>
    public bool IsBranch { get; set; } = false;

    /// <summary>
    /// 访问时间
    /// </summary>
    public long VisitTime { get; set; } = 0;

    public override string ToString()
    {
        return $"Location({X}, {Y}): {NodeType}";
    }
}
