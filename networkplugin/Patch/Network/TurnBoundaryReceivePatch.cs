using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Network.Snapshot;

namespace NetworkPlugin.Patch.Network;

[HarmonyPatch]
public static class TurnBoundaryReceivePatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static bool _subscribed;
    private static INetworkClient _subscribedClient;
    private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;
    private static readonly object _cacheLock = new();

    private static readonly Dictionary<string, TurnBoundarySnapshot> _lastSnapshotByPlayer = new(StringComparer.Ordinal);

        [HarmonyPatch(typeof(GameDirector), "Update")]
    private static class SubscribeHook
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            INetworkClient client = NetworkEventHelper.TryGetNetworkClient();
            if (client == null)
                return;

            EnsureSubscribed(client);
        }
    }

    private static void EnsureSubscribed(INetworkClient client)
    {
        if (_subscribed && ReferenceEquals(_subscribedClient, client))
            return;

        try
        {
            if (_subscribedClient != null)
                _subscribedClient.OnGameEventReceived -= _onGameEventReceived;
        }
        catch (Exception ex) { Plugin.Logger?.LogWarning($"[TurnBoundaryRecv] 取消订阅事件失败: {ex.Message}"); }

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
        if (eventType != NetworkMessageTypes.OnTurnStart && eventType != NetworkMessageTypes.OnTurnEnd)
            return;

        try
        {
            if (!TryDeserialize(payload, out TurnBoundarySnapshot snapshot))
            {
                Plugin.Logger?.LogWarning($"[TurnBoundaryRecv] Failed to parse {eventType} payload");
                return;
            }

            string senderId = snapshot?.PlayerState?.PlayerId;
            if (string.IsNullOrWhiteSpace(senderId))
                senderId = snapshot?.PlayerState?.UserName;
            if (string.IsNullOrWhiteSpace(senderId))
                senderId = "unknown";

            lock (_cacheLock)
            {
                _lastSnapshotByPlayer[senderId] = snapshot;
            }

            INetworkManager networkManager = ServiceProvider?.GetService<INetworkManager>();
            INetworkPlayer player = networkManager?.GetPlayer(senderId);
            if (player != null && snapshot?.PlayerState != null)
            {
                try
                {
                    var ps = snapshot.PlayerState;
                    player.HP = ps.Health;
                    player.maxHP = ps.MaxHealth;
                    player.block = ps.Block;
                    player.shield = ps.Shield;
                    player.coins = ps.Gold;

                    int[] mana = ps.ManaGroup ?? new[] { 0, 0, 0, 0 };
                    player.SetManaArraySafe(mana);

                    player.endturn = snapshot.BoundaryType == BoundaryType.End;

                    if (ps.GameLocation != null)
                    {
                        player.location_X = ps.GameLocation.X;
                        player.location_Y = ps.GameLocation.Y;
                        if (!string.IsNullOrWhiteSpace(ps.GameLocation.NodeType))
                            player.location = ps.GameLocation.NodeType;
                    }
                }
                catch (Exception ex) { Plugin.Logger?.LogWarning($"[TurnBoundaryRecv] 更新远程玩家位置失败: senderId={senderId}, {ex.Message}"); }
            }

            Plugin.Logger?.LogDebug($"[TurnBoundaryRecv] {eventType} received: player={senderId}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TurnBoundaryRecv] Error handling {eventType}: {ex.Message}");
        }
    }

    private static bool TryDeserialize(object payload, out TurnBoundarySnapshot snapshot)
    {
        snapshot = null;
        try
        {
            if (!NetworkEventHelper.TryGetJsonElement(payload, out JsonElement root))
                return false;

            string raw = root.GetRawText();
            var boundary = JsonSerializer.Deserialize<TurnBoundarySnapshot>(raw);
            if (boundary == null)
                return false;

            snapshot = boundary;
            return true;
        }
        catch
        {
            return false;
        }
    }

        public static bool TryGetLastSnapshot(string playerId, out TurnBoundarySnapshot snapshot)
    {
        snapshot = null;
        if (string.IsNullOrWhiteSpace(playerId))
            return false;
        lock (_cacheLock)
            return _lastSnapshotByPlayer.TryGetValue(playerId, out snapshot);
    }
}
