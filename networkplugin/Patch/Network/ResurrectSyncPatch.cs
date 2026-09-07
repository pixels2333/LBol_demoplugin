using System;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.UI.Models;
using NetworkPlugin.UI.State;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

public static class ResurrectSyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static readonly object SyncLock = new();
    private static bool _subscribed;
    private static INetworkClient _subscribedClient;

        public static event Action<string, bool, string> OnResurrectResult;

        public static void EnsureSubscribed(INetworkClient client)
    {
        if (client == null)
        {
            return;
        }

        lock (SyncLock)
        {
            if (_subscribed && ReferenceEquals(_subscribedClient, client))
            {
                return;
            }
        }

        try
        {
            if (_subscribedClient != null)
                _subscribedClient.OnGameEventReceived -= OnGameEventReceived;
        }
        catch
        {

        }

        try
        {
            client.OnGameEventReceived += OnGameEventReceived;
            lock (SyncLock)
            {
                _subscribedClient = client;
                _subscribed = true;
            }
        }
        catch
        {
            lock (SyncLock)
            {
                _subscribedClient = null;
                _subscribed = false;
            }
        }
    }

    private static void OnGameEventReceived(string eventType, object payload)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            return;
        }

        if (eventType != NetworkMessageTypes.OnGapHealRequest &&
            eventType != NetworkMessageTypes.OnGapPlayerHealed &&
            eventType != NetworkMessageTypes.OnGapHealFailed &&
            eventType != NetworkMessageTypes.OnResurrectRequest &&
            eventType != NetworkMessageTypes.OnPlayerResurrected &&
            eventType != NetworkMessageTypes.OnResurrectFailed &&
            eventType != NetworkMessageTypes.OnPlayerDeathStatusChanged)
        {
            return;
        }

        if (!TryGetJsonElement(payload, out JsonElement root))
        {
            return;
        }

        switch (eventType)
        {
            case NetworkMessageTypes.OnPlayerDeathStatusChanged:
                HandleDeathStatusChanged(root);
                return;
            case NetworkMessageTypes.OnGapHealRequest:
            case NetworkMessageTypes.OnResurrectRequest:
                HandleGapHealRequest(root);
                return;
            case NetworkMessageTypes.OnGapPlayerHealed:
            case NetworkMessageTypes.OnPlayerResurrected:
                HandleGapHealed(root);
                return;
            case NetworkMessageTypes.OnGapHealFailed:
            case NetworkMessageTypes.OnResurrectFailed:
                HandleGapHealFailed(root);
                return;
        }
    }

    private static void HandleDeathStatusChanged(JsonElement root)
    {

        string playerId = GetString(root, "PlayerId");
        bool isFakeDeath = GetBool(root, "IsFakeDeath");
        int maxHp = GetInt(root, "MaxHp", 0);

        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        if (!isFakeDeath)
        {
            DeathRegistry.MarkAlive(playerId);
            return;
        }

        DeadPlayerEntry entry = new DeadPlayerEntry
        {
            PlayerId = playerId,
            PlayerName = OtherPlayersOverlayPatch.ResolveDisplayName(playerId),
            DeadCause = "FakeDeath",
            CanResurrect = true,
            MaxHp = maxHp,
            DeathTime = DateTime.UtcNow,

            ResurrectionCost = Math.Max(0, maxHp),
            Level = 0,
        };

        DeathRegistry.UpsertDeadPlayer(entry);
    }

    private static void HandleGapHealRequest(JsonElement root)
    {

        if (!NetworkIdentityTracker.GetSelfIsHost())
        {
            return;
        }

        string requestId = GetString(root, "RequestId") ?? string.Empty;
        string requesterPlayerId = GetString(root, "RequesterPlayerId");
        string targetPlayerId = GetString(root, "TargetPlayerId");

        if (string.IsNullOrWhiteSpace(requesterPlayerId) || string.IsNullOrWhiteSpace(targetPlayerId))
        {
            BroadcastHealFailed(requestId, requesterPlayerId, "InvalidRequest");
            return;
        }

        if (!TryResolveTargetVitals(targetPlayerId, out int currentHp, out int maxHp))
        {
            BroadcastHealFailed(requestId, requesterPlayerId, "TargetNotFound");
            return;
        }

        int healAmount = CalculateHealingAmount(maxHp);
        int finalHp = Math.Min(maxHp, Math.Max(0, currentHp) + healAmount);
        if (finalHp <= currentHp)
        {
            BroadcastHealFailed(requestId, requesterPlayerId, "TargetAlreadyFullHealth");
            return;
        }

        try
        {
            INetworkClient client = ServiceProvider?.GetService<INetworkClient>();
            if (client == null)
            {
                BroadcastHealFailed(requestId, requesterPlayerId, "NoNetworkClient");
                return;
            }

            var payload = new
            {
                RequestId = requestId,
                RequesterPlayerId = requesterPlayerId,
                TargetPlayerId = targetPlayerId,
                HealAmount = healAmount,
                ResultHp = finalHp,
                MaxHp = maxHp,
                Timestamp = DateTime.UtcNow.Ticks,
            };

            Plugin.Logger?.LogInfo($"[ResurrectSyncPatch] Broadcasting OnGapPlayerHealed: target={targetPlayerId}, healAmount={healAmount}, resultHp={finalHp}/{maxHp}");
            client.BroadcastState(NetworkMessageTypes.OnGapPlayerHealed, payload);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ResurrectSyncPatch] HandleGapHealRequest broadcast failed: {ex.Message}");
            BroadcastHealFailed(requestId, requesterPlayerId, "BroadcastFailed");
        }
    }

    private static void HandleGapHealed(JsonElement root)
    {
        string requestId = GetString(root, "RequestId") ?? string.Empty;
        string requesterPlayerId = GetString(root, "RequesterPlayerId");
        string targetPlayerId = GetString(root, "TargetPlayerId") ?? GetString(root, "PlayerId");
        int resultHp = GetInt(root, "ResultHp", GetInt(root, "ResurrectionHp", 1));
        int maxHp = GetInt(root, "MaxHp", 0);

        Plugin.RunOnMainThread(() =>
        {
            if (!string.IsNullOrWhiteSpace(targetPlayerId))
            {
                UpdateKnownPlayerVitals(targetPlayerId, resultHp, maxHp);
            }

            string selfId = NetworkIdentityTracker.GetSelfPlayerId();

            if (!string.IsNullOrWhiteSpace(selfId) && string.Equals(selfId, targetPlayerId, StringComparison.Ordinal))
            {
                try
                {
                    ApplyHealToLocalPlayer(resultHp, maxHp);
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogError($"[ResurrectSyncPatch] ApplyHealToLocalPlayer failed: {ex.Message}");
                }
            }

            if (!string.IsNullOrWhiteSpace(selfId) && string.Equals(selfId, requesterPlayerId, StringComparison.Ordinal))
            {
                OnResurrectResult?.Invoke(requestId, true, null);
            }
        });
    }

    private static void HandleGapHealFailed(JsonElement root)
    {
        string requestId = GetString(root, "RequestId") ?? string.Empty;
        string requesterPlayerId = GetString(root, "RequesterPlayerId");
        string reason = GetString(root, "Reason") ?? "Failed";

        Plugin.RunOnMainThread(() =>
        {
            string selfId = NetworkIdentityTracker.GetSelfPlayerId();
            if (!string.IsNullOrWhiteSpace(selfId) && string.Equals(selfId, requesterPlayerId, StringComparison.Ordinal))
            {
                OnResurrectResult?.Invoke(requestId, false, reason);
            }
        });
    }

    private static void BroadcastHealFailed(string requestId, string requesterPlayerId, string reason)
    {
        try
        {
            INetworkClient client = ServiceProvider?.GetService<INetworkClient>();
            if (client == null)
            {
                return;
            }

            var payload = new
            {
                RequestId = requestId,
                RequesterPlayerId = requesterPlayerId,
                Reason = reason,
                Timestamp = DateTime.UtcNow.Ticks,
            };

            Plugin.Logger?.LogWarning($"[ResurrectSyncPatch] Broadcasting OnGapHealFailed: requester={requesterPlayerId}, reason={reason}");
            client.BroadcastState(NetworkMessageTypes.OnGapHealFailed, payload);
        }
        catch
        {

        }
    }

    private static bool TryResolveTargetVitals(string playerId, out int currentHp, out int maxHp)
    {
        currentHp = 0;
        maxHp = 0;

        if (string.IsNullOrWhiteSpace(playerId))
        {
            return false;
        }

        INetworkManager networkManager = ServiceProvider?.GetService<INetworkManager>();
        INetworkPlayer networkPlayer = networkManager?.GetPlayer(playerId);
        if (networkPlayer == null && networkManager != null)
        {
            var self = networkManager.GetSelf();
            if (self != null && string.Equals(self.playerId, playerId, StringComparison.Ordinal))
            {
                networkPlayer = self;
            }
        }

        return PlayerVitalsHelper.TryResolveVitals(playerId, networkPlayer, out currentHp, out maxHp);
    }

    private static void UpdateKnownPlayerVitals(string playerId, int hp, int maxHp)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        INetworkManager networkManager = ServiceProvider?.GetService<INetworkManager>();
        if (networkManager == null)
        {
            return;
        }

        var self = networkManager.GetSelf();
        if (self != null && string.Equals(self.playerId, playerId, StringComparison.Ordinal))
        {
            return;
        }

        var target = networkManager.GetPlayer(playerId);
        if (target == null)
        {
            return;
        }

        target.HP = Math.Max(0, hp);
        if (maxHp > 0)
        {
            target.maxHP = Math.Max(target.HP, maxHp);
        }
    }

    private static void ApplyHealToLocalPlayer(int resultHp, int maxHp)
    {
        var localPlayer = GameStateUtils.GetCurrentPlayer();
        if (localPlayer == null)
        {
            return;
        }

        int finalMaxHp = maxHp > 0 ? Math.Max(maxHp, localPlayer.MaxHp) : localPlayer.MaxHp;
        int finalHp = Math.Min(finalMaxHp, Math.Max(0, resultHp));
        int healDelta = finalHp - localPlayer.Hp;
        if (healDelta <= 0)
        {
            return;
        }

        Plugin.Logger?.LogInfo($"[ResurrectSyncPatch] ApplyHealToLocalPlayer: healDelta={healDelta}, newHp={finalHp}/{finalMaxHp}");

        try
        {
            var gameRun = GameMaster.Instance?.CurrentGameRun;
            if (gameRun != null)
            {
                gameRun.Heal(healDelta, true, null);
                return;
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[ResurrectSyncPatch] gameRun.Heal failed: {ex.Message}, falling back to player.Heal");
        }

        try
        {
            Traverse.Create(localPlayer).Method("Heal", healDelta).GetValue();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ResurrectSyncPatch] localPlayer.Heal failed: {ex.Message}");
        }
    }

    private static int CalculateHealingAmount(int maxHp)
    {
        if (maxHp <= 0)
        {
            return 1;
        }

        return Math.Max(1, (int)Math.Ceiling(maxHp * 0.2d));
    }

    private static bool TryGetJsonElement(object payload, out JsonElement root)
        => NetworkEventHelper.TryGetJsonElement(payload, out root);

    private static string GetString(JsonElement root, string name)
        => NetworkEventHelper.GetString(root, name);

    private static int GetInt(JsonElement elem, string property, int defaultValue)
    {
        if (NetworkEventHelper.TryGetInt(elem, property, out int v))
            return v;
        return defaultValue;
    }

    private static bool GetBool(JsonElement root, string name)
        => NetworkEventHelper.GetBool(root, name);
}
