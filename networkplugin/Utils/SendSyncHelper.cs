using LBoL.Core.Battle;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Utils;

/// <summary>
/// 发送端补丁的通用辅助方法。
/// </summary>
public static class SendSyncHelper
{
    public static INetworkClient TryGetClient()
        => ModService.ServiceProvider?.GetService<INetworkClient>();

    public static INetworkClient TryGetHostClient()
    {
        var client = TryGetClient();
        if (client?.IsConnected != true) return null;
        if (!NetworkIdentityTracker.GetSelfIsHost()) return null;
        return client;
    }

    public static bool IsReady()
    {
        var client = TryGetClient();
        if (client?.IsConnected != true) return false;
        NetworkIdentityTracker.EnsureSubscribed(client);
        return !string.IsNullOrWhiteSpace(NetworkIdentityTracker.GetSelfPlayerId());
    }

    public static bool ShouldSyncBattle(BattleController battle)
    {
        if (battle == null) return false;
        return battle.Player != null && battle.Player == GameStateUtils.GetCurrentPlayer();
    }
}
