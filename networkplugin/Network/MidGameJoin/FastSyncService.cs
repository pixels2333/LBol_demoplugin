using BepInEx.Logging;

namespace NetworkPlugin.Network.MidGameJoin;

/// <summary>
/// 快速同步服务，用于中途加入时的玩家状态快速同步
/// </summary>
/// <param name="logger">BepInEx 日志记录器</param>
public class FastSyncService(ManualLogSource logger)
{
    private readonly ManualLogSource _logger = logger;

    /// <summary>
    /// 同步指定玩家的状态
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    /// <param name="state">玩家引导状态</param>
    public void SyncPlayerState(string playerId, PlayerBootstrappedState state)
    {
        _logger.LogDebug($"[FastSyncService] Syncing state for {playerId}: progress {state.GameProgress}%");
    }
}
