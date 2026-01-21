using System;
using System.Collections.Generic;
using NetworkPlugin.Network.Snapshot;

namespace NetworkPlugin.Network.RoomSync;

/// <summary>
/// 房间状态阶段：用于决定“进入房间时”该怎么处理。
/// </summary>
public enum RoomPhase
{
    NotVisited,
    InBattle,
    BattleFinished,
}

/// <summary>
/// 主机缓存的房间状态快照（供其他客户端请求/应用）。
/// </summary>
public sealed class RoomStateSnapshot
{
    public string RoomKey { get; set; } = string.Empty;

    public long RoomVersion { get; set; } = 0;

    public RoomPhase Phase { get; set; } = RoomPhase.NotVisited;

    /// <summary>
    /// 先进入/权威上传者。
    /// </summary>
    public string OwnerPlayerId { get; set; } = string.Empty;

    public long UpdatedAtUtcTicks { get; set; } = 0;

    public int Act { get; set; } = 0;

    public int X { get; set; } = 0;

    public int Y { get; set; } = 0;

    public string StationType { get; set; } = string.Empty;

    public string BattleId { get; set; } = string.Empty;

    public List<EnemyStateSnapshot> Enemies { get; set; } = [];

    public BattleRewardSnapshot Rewards { get; set; } = new();
}
