using System;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Network.Snapshot;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// OnTurnStart（回合开始快照）接收落地：
/// - 订阅 <see cref="INetworkClient.OnGameEventReceived"/>，接收并解析 <see cref="NetworkMessageTypes.OnTurnStart"/>；
/// - 将回合开始后的关键字段写入远端玩家对象（<see cref="RemoteNetworkPlayer"/>）以便 UI/调试对齐。
/// </summary>
/// <remarks>
/// 设计边界：
/// - 本补丁只更新 <see cref="INetworkPlayer"/> 的字段，不直接驱动 LBoL 的战斗逻辑；
/// - 回合协商/锁定仍由 EndTurnSyncPatch 负责。
/// </remarks>
[HarmonyPatch]
public static class TurnStartSnapshotReceivePatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static bool _subscribed;
    private static INetworkClient _subscribedClient;
    private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;

    [HarmonyPatch(typeof(GameDirector), "Update")]
    private static class SubscribeHook
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            INetworkClient client = TryGetNetworkClient();
            if (client == null)
            {
                return;
            }

            EnsureSubscribed(client);
        }
    }

    private static INetworkClient TryGetNetworkClient()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkClient>();
        }
        catch
        {
            return null;
        }
    }

    private static INetworkManager TryGetNetworkManager()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkManager>();
        }
        catch
        {
            return null;
        }
    }

    private static void EnsureSubscribed(INetworkClient client)
    {
        if (_subscribed && ReferenceEquals(_subscribedClient, client))
        {
            return;
        }

        try
        {
            if (_subscribedClient != null)
            {
                _subscribedClient.OnGameEventReceived -= _onGameEventReceived;
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            client.OnGameEventReceived += _onGameEventReceived;
            _subscribedClient = client;
            _subscribed = true;
        }
        catch
        {
            _subscribedClient = null;
            _subscribed = false;
        }
    }

    private static void OnGameEventReceived(string eventType, object payload)
    {
        if (!string.Equals(eventType, NetworkMessageTypes.OnTurnStart, StringComparison.Ordinal))
        {
            return;
        }

        try
        {
            if (!TryDeserialize(payload, out TurnStartStateSnapshot snapshot))
            {
                Plugin.Logger?.LogWarning("[TurnStartRecv] Failed to parse OnTurnStart payload");
                return;
            }

            string senderId = snapshot?.playerStateSnapshot?.PlayerId;
            if (string.IsNullOrWhiteSpace(senderId))
            {
                senderId = snapshot?.playerStateSnapshot?.UserName;
            }
            if (string.IsNullOrWhiteSpace(senderId))
            {
                senderId = "unknown";
            }

            // 尝试将快照落地到远端玩家对象（如果存在）。
            INetworkManager mgr = TryGetNetworkManager();
            INetworkPlayer p = mgr?.GetPlayer(senderId);
            if (p != null)
            {
                ApplyToPlayer(p, snapshot);
            }

            Plugin.Logger?.LogDebug($"[TurnStartRecv] OnTurnStart received: player={senderId}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TurnStartRecv] Error handling OnTurnStart: {ex.Message}");
        }
    }

    private static void ApplyToPlayer(INetworkPlayer player, TurnStartStateSnapshot snapshot)
    {
        try
        {
            PlayerStateSnapshot ps = snapshot.playerStateSnapshot;
            if (ps == null)
            {
                return;
            }

            player.HP = ps.Health;
            player.maxHP = ps.MaxHealth;
            player.block = ps.Block;
            player.shield = ps.Shield;
            player.coins = ps.Gold;

            int[] mana = ps.ManaGroup ?? new[] { 0, 0, 0, 0 };
            player.SetManaArraySafe(mana);

            // 回合开始：默认视为该玩家尚未结束回合。
            player.endturn = false;

            if (ps.GameLocation != null)
            {
                player.location_X = ps.GameLocation.X;
                player.location_Y = ps.GameLocation.Y;
                if (!string.IsNullOrWhiteSpace(ps.GameLocation.NodeType))
                {
                    player.location = ps.GameLocation.NodeType;
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private static bool TryDeserialize(object payload, out TurnStartStateSnapshot snapshot)
    {
        snapshot = null;

        try
        {
            JsonElement root;
            if (payload is JsonElement je)
            {
                root = je;
            }
            else if (payload is string s)
            {
                root = JsonSerializer.Deserialize<JsonElement>(s);
            }
            else
            {
                string json = JsonSerializer.Serialize(payload);
                root = JsonSerializer.Deserialize<JsonElement>(json);
            }

            if (root.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            snapshot = JsonSerializer.Deserialize<TurnStartStateSnapshot>(root.GetRawText());
            return snapshot != null;
        }
        catch
        {
            return false;
        }
    }
}
