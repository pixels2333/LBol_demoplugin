using System.Collections.Generic;
using BepInEx.Logging;

namespace NetworkPlugin.Network.MidGameJoin;
//TODO:ai角色,低优先级
// AI Controller stub
public class AIPlayerController(ManualLogSource logger)
{
    private readonly ManualLogSource _logger = logger;
    private readonly HashSet<string> _controlledPlayers = [];

    public void StartControlling(string playerId)
    {
        _controlledPlayers.Add(playerId);
        _logger.LogDebug($"[AIController] Started controlling {playerId}");
    }

    public void StopControlling(string playerId)
    {
        _controlledPlayers.Remove(playerId);
        _logger.LogDebug($"[AIController] Stopped controlling {playerId}");
    }
}
