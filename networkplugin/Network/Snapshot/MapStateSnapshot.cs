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
    /// 当前地图种子（LBoL: Stage.MapSeed，ulong）。
    /// 旧字段 <see cref="MapSeed"/> 保留用于兼容。
    /// </summary>
    public ulong? MapSeedUlong { get; set; }

    /// <summary>
    /// 已访问的节点
    /// </summary>
    public List<string> VisitedNodes { get; set; } = [];

    /// <summary>
    /// 每个节点的状态（nodeKey -> state）。
    /// nodeKey 口径建议与 RoomSync 一致：Act:X:Y:StationType。
    /// </summary>
    public Dictionary<string, string> NodeStates { get; set; } = [];

    /// <summary>
    /// 已清理（已结算）的节点（用于追赶时跳过战斗）。
    /// </summary>
    public List<string> ClearedNodes { get; set; } = [];

    /// <summary>
    /// 最近一次关键提交点 ID（仅主机更新）。
    /// </summary>
    public string LastCheckpointId { get; set; } = "";

    /// <summary>
    /// 最近一次关键提交点时间（UTC ticks，仅主机更新）。
    /// </summary>
    public long LastCheckpointAtUtcTicks { get; set; } = 0;

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
