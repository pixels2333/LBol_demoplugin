using Microsoft.Extensions.Logging;

namespace NetworkPlugin.Network.MidGameJoin;

// Fast sync service stub
public class FastSyncService(ILogger logger)
{
    private readonly ILogger _logger = logger;

    public void SyncPlayerState(string playerId, PlayerBootstrappedState state)
    {
        _logger.LogDebug($"[FastSyncService] Syncing state for {playerId}: progress {state.GameProgress}%");
    }
}
