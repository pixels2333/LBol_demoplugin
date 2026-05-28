using System;
using System.Text.Json;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.UI.Models;
using NetworkPlugin.UI.State;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// Gap 治疗同步补丁：
/// - 客户端发起 OnGapHealRequest。
/// - Host 校验并广播 OnGapPlayerHealed（或 OnGapHealFailed）。
/// - 所有客户端收到广播后：更新目标玩家缓存；仅目标本人执行本地治疗落地。
/// </summary>
public static class ResurrectSyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static readonly object SyncLock = new();
    private static bool _subscribed;
    private static INetworkClient _subscribedClient;

    /// <summary>
    /// 复活操作结果事件：（requestId, success, reason）
    /// </summary>
    public static event Action<string, bool, string> OnResurrectResult;

    /// <summary>
    /// 确保已订阅网络客户端事件
    /// </summary>
    /// <param name="client">网络客户端实例</param>
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
            // ignored
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
        // 统一把“死者列表”汇总到 DeathRegistry，供 Gap 复活面板展示。
        // 注意：DeathPatches 当前发送的 PlayerId 是 PlayerUnit.Id；但需求规定应以网络 PlayerId 为准。
        // v1：若 payload 没有 Network PlayerId，则先用字符串字段 PlayerId 作为 key（实施后续可补齐映射）。

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
            // 默认经济模型：Cost = MaxHp => ResurrectionHp = Cost/2 = MaxHp/2
            ResurrectionCost = Math.Max(0, maxHp),
            Level = 0,
        };

        DeathRegistry.UpsertDeadPlayer(entry);
    }

    private static void HandleGapHealRequest(JsonElement root)
    {
        // 仅 Host 处理请求并广播结果。
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

            client.SendGameEventData(NetworkMessageTypes.OnGapPlayerHealed, new
            {
                RequestId = requestId,
                RequesterPlayerId = requesterPlayerId,
                TargetPlayerId = targetPlayerId,
                HealAmount = healAmount,
                ResultHp = finalHp,
                MaxHp = maxHp,
                Timestamp = DateTime.UtcNow.Ticks,
            });

            UpdateKnownPlayerVitals(targetPlayerId, finalHp, maxHp);
        }
        catch
        {
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

        if (!string.IsNullOrWhiteSpace(targetPlayerId))
        {
            UpdateKnownPlayerVitals(targetPlayerId, resultHp, maxHp);
        }

        string selfId = NetworkIdentityTracker.GetSelfPlayerId();

        // 目标本人：执行治疗落地。
        if (!string.IsNullOrWhiteSpace(selfId) && string.Equals(selfId, targetPlayerId, StringComparison.Ordinal))
        {
            try
            {
                ApplyHealToLocalPlayer(resultHp, maxHp);
            }
            catch
            {
                // ignored
            }
        }

        if (!string.IsNullOrWhiteSpace(selfId) && string.Equals(selfId, requesterPlayerId, StringComparison.Ordinal))
        {
            OnResurrectResult?.Invoke(requestId, true, null);
        }
    }

    private static void HandleGapHealFailed(JsonElement root)
    {
        string requestId = GetString(root, "RequestId") ?? string.Empty;
        string requesterPlayerId = GetString(root, "RequesterPlayerId");
        string reason = GetString(root, "Reason") ?? "Failed";

        string selfId = NetworkIdentityTracker.GetSelfPlayerId();
        if (!string.IsNullOrWhiteSpace(selfId) && string.Equals(selfId, requesterPlayerId, StringComparison.Ordinal))
        {
            OnResurrectResult?.Invoke(requestId, false, reason);
        }
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

            client.SendGameEventData(NetworkMessageTypes.OnGapHealFailed, new
            {
                RequestId = requestId,
                RequesterPlayerId = requesterPlayerId,
                Reason = reason,
                Timestamp = DateTime.UtcNow.Ticks,
            });
        }
        catch
        {
            // ignored
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
        if (networkManager == null)
        {
            return false;
        }

        string selfPlayerId = NetworkIdentityTracker.GetSelfPlayerId();
        if (!string.IsNullOrWhiteSpace(selfPlayerId) && string.Equals(selfPlayerId, playerId, StringComparison.Ordinal))
        {
            var localPlayer = GameStateUtils.GetCurrentPlayer();
            if (localPlayer != null)
            {
                currentHp = Math.Max(0, localPlayer.Hp);
                maxHp = Math.Max(0, localPlayer.MaxHp);
                return maxHp > 0;
            }
        }

        var networkPlayer = networkManager.GetPlayer(playerId);
        if (networkPlayer == null)
        {
            networkPlayer = networkManager.GetSelf();
            if (networkPlayer == null || !string.Equals(networkPlayer.playerId, playerId, StringComparison.Ordinal))
            {
                return false;
            }
        }

        currentHp = Math.Max(0, networkPlayer.HP);
        maxHp = Math.Max(0, networkPlayer.maxHP);
        return maxHp > 0;
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

        Traverse.Create(localPlayer).Method("Heal", healDelta).GetValue();
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
