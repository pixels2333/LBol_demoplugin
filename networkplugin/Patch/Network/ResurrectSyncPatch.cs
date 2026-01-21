using System;
using System.Text.Json;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.UI.Panels;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// Gap 复活同步补丁：
/// - 客户端发起 OnResurrectRequest。
/// - Host 校验并广播 OnPlayerResurrected（或 OnResurrectFailed）。
/// - 所有客户端收到广播后：仅 TargetPlayerId == self.playerId 的客户端执行本地复活落地。
/// - 发起者扣费：仅在成功广播后本地扣费；失败则提示（等价退款）。
/// </summary>
public static class ResurrectSyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static readonly object SyncLock = new();
    private static bool _subscribed;
    private static INetworkClient _subscribedClient;

    // 本地 UI 等待结果用（不强依赖 UI 层引用，避免循环依赖）。
    public static event Action<string, bool, string> OnResurrectResult; // requestId, success, reason

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
            {
                _subscribedClient.OnGameEventReceived -= OnGameEventReceived;
            }
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

        if (eventType != NetworkMessageTypes.OnResurrectRequest &&
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
            case NetworkMessageTypes.OnResurrectRequest:
                HandleResurrectRequest(root);
                return;
            case NetworkMessageTypes.OnPlayerResurrected:
                HandleResurrected(root);
                return;
            case NetworkMessageTypes.OnResurrectFailed:
                HandleResurrectFailed(root);
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

        var entry = new DeadPlayerEntry
        {
            PlayerId = playerId,
            PlayerName = playerId, // v1：暂无稳定昵称字段；后续可用 PlayerListUpdate 的 PlayerName
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

    private static void HandleResurrectRequest(JsonElement root)
    {
        // 仅 Host 处理请求并广播结果。
        if (!NetworkIdentityTracker.GetSelfIsHost())
        {
            return;
        }

        string requestId = GetString(root, "RequestId") ?? string.Empty;
        string requesterPlayerId = GetString(root, "RequesterPlayerId");
        string targetPlayerId = GetString(root, "TargetPlayerId");
        int cost = GetInt(root, "Cost", 0);

        if (string.IsNullOrWhiteSpace(requesterPlayerId) || string.IsNullOrWhiteSpace(targetPlayerId))
        {
            BroadcastFailed(requestId, requesterPlayerId, "InvalidRequest");
            return;
        }

        // 校验：目标必须仍在死亡登记册中。
        if (!DeathRegistry.TryGetDeadPlayer(targetPlayerId, out DeadPlayerEntry dead) || dead == null || !dead.CanResurrect)
        {
            BroadcastFailed(requestId, requesterPlayerId, "TargetNotDeadOrCannotResurrect");
            return;
        }

        // 复活 HP = Cost/2（至少 1，且不超过 MaxHp；MaxHp 不存在时不做上限）。
        int maxHp = dead.MaxHp;
        int hp = Math.Max(1, cost / 2);
        if (maxHp > 0)
        {
            hp = Math.Min(hp, maxHp);
        }

        // Host 广播结果：所有客户端都会收到，但只有目标自己会落地复活。
        try
        {
            INetworkClient client = ServiceProvider?.GetService<INetworkClient>();
            if (client == null)
            {
                BroadcastFailed(requestId, requesterPlayerId, "NoNetworkClient");
                return;
            }

            client.SendGameEventData(NetworkMessageTypes.OnPlayerResurrected, new
            {
                RequestId = requestId,
                RequesterPlayerId = requesterPlayerId,
                TargetPlayerId = targetPlayerId,
                Cost = cost,
                ResurrectionHp = hp,
                Timestamp = DateTime.UtcNow.Ticks,
            });

            // 同时把登记册里该目标移除，避免重复复活。
            DeathRegistry.MarkAlive(targetPlayerId);
        }
        catch
        {
            BroadcastFailed(requestId, requesterPlayerId, "BroadcastFailed");
        }
    }

    private static void HandleResurrected(JsonElement root)
    {
        string requestId = GetString(root, "RequestId") ?? string.Empty;
        string requesterPlayerId = GetString(root, "RequesterPlayerId");
        // 兼容：旧 payload 可能只有 PlayerId。
        string targetPlayerId = GetString(root, "TargetPlayerId") ?? GetString(root, "PlayerId");
        int cost = GetInt(root, "Cost", 0);
        int hp = GetInt(root, "ResurrectionHp", 1);

        // 任何客户端都可以据此更新“死亡登记册”。
        if (!string.IsNullOrWhiteSpace(targetPlayerId))
        {
            DeathRegistry.MarkAlive(targetPlayerId);
        }

        string selfId = NetworkIdentityTracker.GetSelfPlayerId();

        // 目标本人：执行复活落地。
        if (!string.IsNullOrWhiteSpace(selfId) && string.Equals(selfId, targetPlayerId, StringComparison.Ordinal))
        {
            try
            {
                // 复活落地：直接使用 DeathPatches 的复活入口（它会走 Heal 并触发状态补丁）。
                // 注意：DeathPatches 的 PlayerId 目前是 PlayerUnit.Id；此处落地仅针对本地 PlayerUnit。
                bool prev = NetworkPlugin.Patch.DeathPatches.SuppressNetworkSync;
                NetworkPlugin.Patch.DeathPatches.SuppressNetworkSync = true;
                try
                {
                    NetworkPlugin.Patch.DeathPatches.ResurrectPlayer(GameStateUtils.GetCurrentPlayer(), hp);
                }
                finally
                {
                    NetworkPlugin.Patch.DeathPatches.SuppressNetworkSync = prev;
                }
            }
            catch
            {
                // ignored
            }
        }

        // 发起者：成功后扣费（延迟扣费，失败等价退款）。
        if (!string.IsNullOrWhiteSpace(selfId) && string.Equals(selfId, requesterPlayerId, StringComparison.Ordinal))
        {
            OnResurrectResult?.Invoke(requestId, true, null);

            try
            {
                // 扣费由 UI 侧执行更合理；这里仅发事件。
                // ResurrectPanel 会在回调中 ConsumeMoney(cost)。
            }
            catch
            {
                // ignored
            }
        }
    }

    private static void HandleResurrectFailed(JsonElement root)
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

    private static void BroadcastFailed(string requestId, string requesterPlayerId, string reason)
    {
        try
        {
            INetworkClient client = ServiceProvider?.GetService<INetworkClient>();
            if (client == null)
            {
                return;
            }

            client.SendGameEventData(NetworkMessageTypes.OnResurrectFailed, new
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

    private static bool TryGetJsonElement(object payload, out JsonElement root)
    {
        try
        {
            if (payload is JsonElement je)
            {
                root = je;
                return true;
            }

            if (payload is string s)
            {
                root = JsonDocument.Parse(s).RootElement;
                return true;
            }
        }
        catch
        {
            // ignored
        }

        root = default;
        return false;
    }

    private static string GetString(JsonElement elem, string property)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return null;
            }

            return p.ValueKind == JsonValueKind.String ? p.GetString() : p.GetRawText();
        }
        catch
        {
            return null;
        }
    }

    private static int GetInt(JsonElement elem, string property, int defaultValue)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return defaultValue;
            }

            return p.ValueKind switch
            {
                JsonValueKind.Number => p.TryGetInt32(out int i) ? i : defaultValue,
                JsonValueKind.String => int.TryParse(p.GetString(), out int i) ? i : defaultValue,
                _ => defaultValue,
            };
        }
        catch
        {
            return defaultValue;
        }
    }

    private static bool GetBool(JsonElement elem, string property)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return false;
            }

            return p.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(p.GetString(), out bool b) && b,
                _ => false,
            };
        }
        catch
        {
            return false;
        }
    }
}
