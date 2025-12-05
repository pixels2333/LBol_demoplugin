using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 地图状态快照
/// </summary>
public class MapStateSnapshot
{
    /// <summary>
    /// 当前地图种子
    /// </summary>
    public int MapSeed { get; set; } = 0;

    /// <summary>
    /// 已访问的节点
    /// </summary>
    public List<string> VisitedNodes { get; set; } = [];

    /// <summary>
    /// 当前位置
    /// </summary>
    public LocationSnapshot CurrentLocation { get; set; } = new();

    /// <summary>
    /// 已揭示的节点
    /// </summary>
    public List<string> RevealedNodes { get; set; } = [];

    /// <summary>
    /// 路径历史
    /// </summary>
    public List<LocationSnapshot> PathHistory { get; set; } = [];

    /// <summary>
    /// 地图是否完成
    /// </summary>
    public bool MapComplete { get; set; } = false;
}
