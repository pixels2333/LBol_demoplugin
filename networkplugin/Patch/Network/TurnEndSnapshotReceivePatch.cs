using System;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
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
/// OnTurnEnd（回合结束快照）接收落地：
/// - 订阅 <see cref="INetworkClient.OnGameEventReceived"/>，接收并解析 <see cref="NetworkMessageTypes.OnTurnEnd"/>；
/// - 将回合结束后的关键字段写入远端玩家对象（<see cref="RemoteNetworkPlayer"/>）以便 UI/调试对齐。
/// </summary>
/// <remarks>
/// 设计边界：
/// - EndTurnRequest/Confirm 的协商与 UI 锁定由 <see cref="EndTurnSyncPatch"/> 负责；
/// - 本补丁仅处理“回合已实际结束后”的快照事件，不参与协商，也不直接改动 LBoL 的战斗状态。
/// </remarks>
[HarmonyPatch]
public static class TurnEndSnapshotReceivePatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static bool _subscribed;
    private static INetworkClient _subscribedClient;
    private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;

    // 最小侵入：缓存最近一次收到的 TurnEnd 快照，便于 UI/诊断。
    private static readonly object _cacheLock = new();
    private static readonly System.Collections.Generic.Dictionary<string, TurnEndStateSnapshot> _lastTurnEndByPlayer
        = new(StringComparer.Ordinal);

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
        if (!string.Equals(eventType, NetworkMessageTypes.OnTurnEnd, StringComparison.Ordinal))
        {
            return;
        }

        try
        {
            if (!TryDeserialize(payload, out TurnEndStateSnapshot snapshot))
            {
                Plugin.Logger?.LogWarning("[TurnEndRecv] Failed to parse OnTurnEnd payload");
                return;
            }

            string senderId = snapshot?.playerStateSnapshot?.UserName;
            if (string.IsNullOrWhiteSpace(senderId))
            {
                senderId = snapshot?.playerStateSnapshot?.PlayerId;
            }
            if (string.IsNullOrWhiteSpace(senderId))
            {
                senderId = "unknown";
            }

            lock (_cacheLock)
            {
                _lastTurnEndByPlayer[senderId] = snapshot;
            }

            // 尝试将快照落地到远端玩家对象（如果存在）。
            INetworkManager mgr = TryGetNetworkManager();
            INetworkPlayer p = mgr?.GetPlayer(senderId);
            if (p != null)
            {
                ApplyToPlayer(p, snapshot);
            }

            Plugin.Logger?.LogDebug($"[TurnEndRecv] OnTurnEnd received: player={senderId}, battle={snapshot.BattleId}, round={snapshot.Round}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TurnEndRecv] Error handling OnTurnEnd: {ex.Message}");
        }
    }

    private static void ApplyToPlayer(INetworkPlayer player, TurnEndStateSnapshot snapshot)
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

            // mana 的接口成员在历史代码中以 lowerCamelCase 使用（Remote/LocalNetworkPlayer 也实现了）。
            // 这里按 4 位数组落地，避免长度不一致。
            int[] mana = ps.ManaGroup ?? new[] { 0, 0, 0, 0 };
            if (mana.Length != 4)
            {
                int[] fixedMana = new[] { 0, 0, 0, 0 };
                for (int i = 0; i < Math.Min(4, mana.Length); i++)
                {
                    fixedMana[i] = mana[i];
                }
                mana = fixedMana;
            }

            // 通过动态/反射避免对接口增加新成员的破坏性修改。
            // RemoteNetworkPlayer/LocalNetworkPlayer 已有 public int[] mana {get;set;}。
            var t = player.GetType();
            var prop = t.GetProperty("mana");
            if (prop != null && prop.PropertyType == typeof(int[]))
            {
                prop.SetValue(player, mana);
            }

            player.endturn = true;
        }
        catch
        {
            // ignored
        }
    }

    private static bool TryDeserialize(object payload, out TurnEndStateSnapshot snapshot)
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
                // Dictionary/anonymous object fallback
                string json = JsonCompat.Serialize(payload);
                root = JsonSerializer.Deserialize<JsonElement>(json);
            }

            if (root.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            // TurnEndStateSnapshot 的属性命名与 JSON 字段一致（含 lowerCamelCase + PascalCase 混用）。
            snapshot = JsonSerializer.Deserialize<TurnEndStateSnapshot>(root.GetRawText());
            return snapshot != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取最近一次收到的回合结束快照（用于 UI/调试）。
    /// </summary>
    public static bool TryGetLastTurnEnd(string playerId, out TurnEndStateSnapshot snapshot)
    {
        snapshot = null;
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return false;
        }

        lock (_cacheLock)
        {
            return _lastTurnEndByPlayer.TryGetValue(playerId, out snapshot);
        }
    }
}
