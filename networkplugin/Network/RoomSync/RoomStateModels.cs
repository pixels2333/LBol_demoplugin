using System.Collections.Generic;
using NetworkPlugin.Network.Snapshot;

namespace NetworkPlugin.Network.RoomSync;

/// <summary>
/// 房间状态阶段：用于决定“进入房间时”该怎么处理。
/// </summary>
public enum RoomPhase
{
    /// <summary>尚未访问</summary>
    NotVisited,
    /// <summary>战斗中</summary>
    InBattle,
    /// <summary>战斗已结束</summary>
    BattleFinished,
}

/// <summary>
/// 主机缓存的房间状态快照（供其他客户端请求/应用）。
/// </summary>
public sealed class RoomStateSnapshot
{
    /// <summary>房间键值（格式：Act:X:Y:StationType）</summary>
    public string RoomKey { get; set; } = string.Empty;

    /// <summary>房间版本号，用于冲突检测</summary>
    public long RoomVersion { get; set; } = 0;

    /// <summary>房间状态阶段</summary>
    public RoomPhase Phase { get; set; } = RoomPhase.NotVisited;

    /// <summary>
    /// 先进入/权威上传者。
    /// </summary>
    public string OwnerPlayerId { get; set; } = string.Empty;

    /// <summary>更新时间（UTC刻度）</summary>
    public long UpdatedAtUtcTicks { get; set; } = 0;

    /// <summary>当前章节</summary>
    public int Act { get; set; } = 0;

    /// <summary>X坐标</summary>
    public int X { get; set; } = 0;

    /// <summary>Y坐标</summary>
    public int Y { get; set; } = 0;

    /// <summary>站点类型</summary>
    public string StationType { get; set; } = string.Empty;

    /// <summary>战斗ID</summary>
    public string BattleId { get; set; } = string.Empty;

    /// <summary>敌人状态快照列表</summary>
    public List<EnemyStateSnapshot> Enemies { get; set; } = [];

    /// <summary>战斗奖励快照</summary>
    public BattleRewardSnapshot Rewards { get; set; } = new();

    /// <summary>GapOptions 事件快照列表</summary>
    public List<GapOptionsEventSnapshot> GapOptionsEvents { get; set; } = [];
}

/// <summary>
    /// GapOptions/GAP 关键事件快照（用于中途加入最小追赶）。
/// </summary>
public sealed class GapOptionsEventSnapshot
{
    /// <summary>动作ID</summary>
    public string ActionId { get; set; } = string.Empty;

    /// <summary>事件类型</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>触发玩家ID</summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>涉及的卡牌ID</summary>
    public string CardId { get; set; } = string.Empty;

    /// <summary>涉及的卡牌名称</summary>
    public string CardName { get; set; } = string.Empty;

    /// <summary>房间键值</summary>
    public string RoomKey { get; set; } = string.Empty;

    /// <summary>事件时间戳（UTC刻度）</summary>
    public long TimestampUtcTicks { get; set; } = 0;
}
