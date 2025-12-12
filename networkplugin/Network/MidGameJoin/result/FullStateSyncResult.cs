using System.Collections.Generic;
using NetworkPlugin.Network.Event;

namespace NetworkPlugin.Network.MidGameJoin.Result;

public class FullStateSyncResult : BaseResult
{
    public List<GameEvent> Events { get; set; } = [];
}
