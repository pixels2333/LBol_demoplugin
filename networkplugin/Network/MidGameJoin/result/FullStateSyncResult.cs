using System.Collections.Generic;
using NetworkPlugin.Network.Event;

namespace NetworkPlugin.Network.MidGameJoin.Result;

/// <summary>
/// 完整状态同步结果，包含同步的游戏事件列表
/// </summary>
public class FullStateSyncResult : BaseResult
{
    /// <summary>同步的游戏事件列表</summary>
    public List<GameEvent> Events { get; set; } = [];
}
