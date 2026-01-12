using BepInEx.Logging;

namespace NetworkPlugin.Network.MidGameJoin;

// Fast sync service stub
public class FastSyncService(ManualLogSource logger)
{
    private readonly ManualLogSource _logger = logger;

    public void SyncPlayerState(string playerId, PlayerBootstrappedState state)
    {
        _logger.LogDebug($"[FastSyncService] Syncing state for {playerId}: progress {state.GameProgress}%");
    }
}
