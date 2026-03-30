using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Network.Snapshot;

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
    private static readonly object _cacheLock = new();
    private static readonly Dictionary<string, TurnStartStateSnapshot> _lastTurnStartByPlayer = new(StringComparer.Ordinal);

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
        => ServiceProvider?.GetService<INetworkClient>();

    private static void EnsureSubscribed(INetworkClient client)
    {
        if (_subscribed && ReferenceEquals(_subscribedClient, client))
        {
            return;
        }

        try
        {
            _subscribedClient?.OnGameEventReceived -= _onGameEventReceived;
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

            lock (_cacheLock)
            {
                _lastTurnStartByPlayer[senderId] = snapshot;
            }

            // 尝试将快照落地到远端玩家对象（如果存在）。
            INetworkManager networkManager = ServiceProvider?.GetService<INetworkManager>();
            INetworkPlayer player = networkManager?.GetPlayer(senderId);
            if (player != null)
            {
                try
                {
                    PlayerStateSnapshot playerState = snapshot.playerStateSnapshot;
                    if (playerState != null)
                    {
                        player.HP = playerState.Health;
                        player.maxHP = playerState.MaxHealth;
                        player.block = playerState.Block;
                        player.shield = playerState.Shield;
                        player.coins = playerState.Gold;

                        int[] mana = playerState.ManaGroup ?? new[] { 0, 0, 0, 0 };
                        player.SetManaArraySafe(mana);

                        // 回合开始：默认视为该玩家尚未结束回合。
                        player.endturn = false;

                        if (playerState.GameLocation != null)
                        {
                            player.location_X = playerState.GameLocation.X;
                            player.location_Y = playerState.GameLocation.Y;
                            if (!string.IsNullOrWhiteSpace(playerState.GameLocation.NodeType))
                            {
                                player.location = playerState.GameLocation.NodeType;
                            }
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }

            Plugin.Logger?.LogDebug($"[TurnStartRecv] OnTurnStart received: player={senderId}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TurnStartRecv] Error handling OnTurnStart: {ex.Message}");
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
                string json = JsonCompat.Serialize(payload);
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

    /// <summary>
    /// 获取最近一次收到的回合开始快照（用于 UI/调试）。
    /// </summary>
    public static bool TryGetLastTurnStart(string playerId, out TurnStartStateSnapshot snapshot)
    {
        snapshot = null;
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return false;
        }

        lock (_cacheLock)
        {
            return _lastTurnStartByPlayer.TryGetValue(playerId, out snapshot);
        }
    }
}
