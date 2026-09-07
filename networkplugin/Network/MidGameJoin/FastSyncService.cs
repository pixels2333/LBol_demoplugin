using BepInEx.Logging;

namespace NetworkPlugin.Network.MidGameJoin;

public class FastSyncService(ManualLogSource logger)
{
    private readonly ManualLogSource _logger = logger;

        public void SyncPlayerState(string playerId, PlayerBootstrappedState state)
    {

        _logger.LogDebug($"[FastSyncService] Syncing state for {playerId}: progress {state.GameProgress}%");
    }
}
