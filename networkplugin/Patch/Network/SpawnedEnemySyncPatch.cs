using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core.Units;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 敌人“生成/召唤”同步补丁（参考 Together in Spire: SpawnedMonsterManager.java）。
/// <para/>
/// 目标：
/// - 为“战斗中途生成的 EnemyUnit”分配一个可网络对齐的 SpawnId（本项目内置的 Unit.Id 仅为类型名，无法区分多只同类型敌人）。
/// - 主机在 EnemyGroup.Add 后广播 SpawnId；客户端在本地发生相同 spawn 时，将 SpawnId 绑定到对应 EnemyUnit。
/// <para/>
/// 说明：
/// - 本补丁不负责“远程创建敌人对象”，仅用于在双方都会生成该敌人的前提下，确保后续同步可以稳定定位到该敌人。
/// </summary>
public static class SpawnedEnemySyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static bool _subscribed;
    private static INetworkClient _subscribedClient;
    private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;
    private static readonly Action<bool> _onConnectionStateChanged = OnConnectionStateChanged;

    private static readonly object _syncLock = new();
    private static string _selfPlayerId;
    private static bool _selfIsHost;
    private static int _localSpawnCount;
    private static readonly Queue<PendingSpawn> _pendingSpawns = new();

    private sealed class PendingSpawn
    {
        public string EnemyGroupId { get; set; }
        public int SpawnCount { get; set; }
        public int RootIndex { get; set; }
        public int Index { get; set; }
        public string EnemyId { get; set; }
        public string SpawnId { get; set; }
        public string SenderPlayerId { get; set; }
    }

    private sealed class SpawnIdBox
    {
        public string SpawnId;
    }

    private static readonly ConditionalWeakTable<EnemyUnit, SpawnIdBox> _spawnIdTable = new();

    public static bool TryGetSpawnId(EnemyUnit enemy, out string spawnId)
    {
        spawnId = null;
        if (enemy == null)
        {
            return false;
        }

        if (_spawnIdTable.TryGetValue(enemy, out SpawnIdBox box) && !string.IsNullOrEmpty(box.SpawnId))
        {
            spawnId = box.SpawnId;
            return true;
        }

        return false;
    }

    private static void SetSpawnId(EnemyUnit enemy, string spawnId)
    {
        if (enemy == null || string.IsNullOrEmpty(spawnId))
        {
            return;
        }

        SpawnIdBox box = _spawnIdTable.GetOrCreateValue(enemy);
        box.SpawnId = spawnId;
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
                _subscribedClient.OnConnectionStateChanged -= _onConnectionStateChanged;
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            client.OnGameEventReceived += _onGameEventReceived;
            client.OnConnectionStateChanged += _onConnectionStateChanged;
            _subscribedClient = client;
            _subscribed = true;
        }
        catch
        {
            _subscribedClient = null;
            _subscribed = false;
        }
    }

    private static void OnConnectionStateChanged(bool connected)
    {
        if (connected)
        {
            return;
        }

        lock (_syncLock)
        {
            _selfPlayerId = null;
            _selfIsHost = false;
            _localSpawnCount = 0;
            _pendingSpawns.Clear();
        }
    }

    private static void OnGameEventReceived(string eventType, object payload)
    {
        if (!TryGetJsonElement(payload, out JsonElement root))
        {
            return;
        }

        switch (eventType)
        {
            case "Welcome":
                HandleWelcome(root);
                return;
            case "HostChanged":
                HandleHostChanged(root);
                return;
            case "BattleEnemySpawned":
                HandleBattleEnemySpawned(root);
                return;
        }
    }

    private static void HandleWelcome(JsonElement root)
    {
        try
        {
            string playerId = root.TryGetProperty("PlayerId", out JsonElement idEl) && idEl.ValueKind == JsonValueKind.String
                ? idEl.GetString()
                : null;
            bool isHost = root.TryGetProperty("IsHost", out JsonElement hostEl) &&
                          (hostEl.ValueKind == JsonValueKind.True || hostEl.ValueKind == JsonValueKind.False) &&
                          hostEl.GetBoolean();

            lock (_syncLock)
            {
                _selfPlayerId = playerId;
                _selfIsHost = isHost;
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void HandleHostChanged(JsonElement root)
    {
        try
        {
            string newHostId = root.TryGetProperty("NewHostId", out JsonElement idEl) && idEl.ValueKind == JsonValueKind.String
                ? idEl.GetString()
                : null;

            lock (_syncLock)
            {
                if (!string.IsNullOrEmpty(_selfPlayerId) && !string.IsNullOrEmpty(newHostId))
                {
                    _selfIsHost = string.Equals(_selfPlayerId, newHostId, StringComparison.Ordinal);
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void HandleBattleEnemySpawned(JsonElement root)
    {
        try
        {
            PendingSpawn pending = new()
            {
                EnemyGroupId = root.TryGetProperty("EnemyGroupId", out JsonElement groupEl) && groupEl.ValueKind == JsonValueKind.String
                    ? groupEl.GetString()
                    : null,
                SpawnCount = TryGetInt(root, "SpawnCount", out int sc) ? sc : -1,
                RootIndex = TryGetInt(root, "RootIndex", out int ri) ? ri : -1,
                Index = TryGetInt(root, "Index", out int idx) ? idx : -1,
                EnemyId = root.TryGetProperty("EnemyId", out JsonElement enemyIdEl) && enemyIdEl.ValueKind == JsonValueKind.String
                    ? enemyIdEl.GetString()
                    : null,
                SpawnId = root.TryGetProperty("SpawnId", out JsonElement spawnIdEl) && spawnIdEl.ValueKind == JsonValueKind.String
                    ? spawnIdEl.GetString()
                    : null,
                SenderPlayerId = root.TryGetProperty("PlayerId", out JsonElement senderEl) && senderEl.ValueKind == JsonValueKind.String
                    ? senderEl.GetString()
                    : null,
            };

            if (string.IsNullOrEmpty(pending.SpawnId) || pending.SpawnCount < 0)
            {
                return;
            }

            lock (_syncLock)
            {
                _pendingSpawns.Enqueue(pending);
            }
        }
        catch
        {
            // ignored
        }
    }

    [HarmonyPatch(typeof(EnemyGroup), "Add")]
    private static class EnemyGroup_Add_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(EnemyGroup __instance, EnemyUnit enemy)
        {
            try
            {
                if (enemy == null || __instance == null)
                {
                    return;
                }

                INetworkClient networkClient = TryGetNetworkClient();
                if (networkClient == null || !networkClient.IsConnected)
                {
                    return;
                }

                bool isHost;
                lock (_syncLock)
                {
                    isHost = _selfIsHost;
                }

                if (isHost)
                {
                    int spawnCount;
                    lock (_syncLock)
                    {
                        spawnCount = _localSpawnCount++;
                    }

                    string spawnId = $"{__instance.Id}:{spawnCount}:{enemy.RootIndex}:{enemy.Id}";
                    SetSpawnId(enemy, spawnId);

                    object payload = new
                    {
                        Timestamp = DateTime.Now.Ticks,
                        PlayerId = _selfPlayerId,
                        EnemyGroupId = __instance.Id,
                        SpawnCount = spawnCount,
                        enemy.RootIndex,
                        Index = GetEnemyIndex(enemy),
                        EnemyId = enemy.Id,
                        SpawnId = spawnId,
                    };

                    networkClient.SendRequest("BattleEnemySpawned", JsonCompat.Serialize(payload));
                    Plugin.Logger?.LogDebug($"[SpawnedEnemySync] Host spawned enemy. Enemy={enemy.Name} Index={GetEnemyIndex(enemy)} RootIndex={enemy.RootIndex} SpawnId={spawnId}");
                    return;
                }

                PendingSpawn pendingSpawn = null;
                lock (_syncLock)
                {
                    if (_pendingSpawns.Count > 0)
                    {
                        pendingSpawn = _pendingSpawns.Dequeue();
                        _localSpawnCount = Math.Max(_localSpawnCount, pendingSpawn.SpawnCount + 1);
                    }
                }

                if (pendingSpawn == null)
                {
                    return;
                }

                if (!string.Equals(pendingSpawn.EnemyGroupId, __instance.Id, StringComparison.Ordinal))
                {
                    Plugin.Logger?.LogWarning($"[SpawnedEnemySync] Pending spawn group mismatch. Local={__instance.Id} Remote={pendingSpawn.EnemyGroupId}");
                }

                SetSpawnId(enemy, pendingSpawn.SpawnId);

                Plugin.Logger?.LogDebug(
                    $"[SpawnedEnemySync] Client bound SpawnId. Enemy={enemy.Name} Index={GetEnemyIndex(enemy)} RootIndex={enemy.RootIndex} SpawnId={pendingSpawn.SpawnId}"
                );
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SpawnedEnemySync] Error in EnemyGroup.Add postfix: {ex.Message}");
            }
        }
    }

    private static bool TryGetJsonElement(object payload, out JsonElement root)
    {
        root = default;

        try
        {
            if (payload is JsonElement el)
            {
                root = el;
                return true;
            }

            if (payload is string s && !string.IsNullOrWhiteSpace(s))
            {
                using JsonDocument doc = JsonDocument.Parse(s);
                root = doc.RootElement.Clone();
                return true;
            }
        }
        catch
        {
            // ignored
        }

        return false;
    }

    private static bool TryGetInt(JsonElement root, string name, out int value)
    {
        value = default;
        if (!root.TryGetProperty(name, out JsonElement el) || el.ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        return el.TryGetInt32(out value);
    }

    private static int GetEnemyIndex(EnemyUnit enemy)
    {
        try
        {
            return Traverse.Create(enemy).Property("Index")?.GetValue<int>() ?? 0;
        }
        catch
        {
            return 0;
        }
    }
}
