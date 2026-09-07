using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using LBoL.Presentation;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.EnemyUnits;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

public static class EnemySpawnSyncPatch
{
    #region 字段和属性

        private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

        private static bool _subscribed;

        private static INetworkClient _subscribedClient;

        private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;

        private static readonly Action<bool> _onConnectionStateChanged = OnConnectionStateChanged;

        private static string _selfPlayerId;

        private static bool _selfIsHost;

        private static readonly object _syncLock = new();

        private static readonly HashSet<string> _processedSpawns = new(StringComparer.Ordinal);

    #endregion

    #region Harmony补丁

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

    #endregion

    #region 网络客户端管理

        private static INetworkClient TryGetNetworkClient()
        => NetworkEventHelper.TryGetNetworkClient();

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

    #endregion

    #region 事件处理

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
            _processedSpawns.Clear();
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
            case NetworkMessageTypes.Welcome:
                HandleWelcome(root);
                return;
            case NetworkMessageTypes.HostChanged:
                HandleHostChanged(root);
                return;
            case NetworkMessageTypes.BattleEnemySpawned:
            case NetworkMessageTypes.EnemySpawned:
                HandleEnemySpawned(root);
                return;
        }
    }

        private static void HandleWelcome(JsonElement root)
    {
        try
        {
            string playerId = GetString(root, "PlayerId");
            bool isHost = GetBool(root, "IsHost", fallback: false);

            lock (_syncLock)
            {
                _selfPlayerId = playerId;
                _selfIsHost = isHost;
            }
        }
        catch
        {

        }
    }

        private static void HandleHostChanged(JsonElement root)
    {
        try
        {
            string newHostId = GetString(root, "NewHostId");

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

        }
    }

        private static void HandleEnemySpawned(JsonElement root)
    {
        try
        {
            bool isHost;
            lock (_syncLock)
            {
                isHost = _selfIsHost;
            }

            if (isHost)
            {
                return;
            }

            string battleId = GetString(root, "BattleId") ?? "unknown";
            int spawnIndex = GetInt(root, "SpawnIndex", fallback: -1);
            if (spawnIndex <= 0)
            {
                return;
            }

            string dedupeKey = $"{battleId}:{spawnIndex}";
            lock (_syncLock)
            {
                if (_processedSpawns.Contains(dedupeKey))
                {
                    return;
                }
                _processedSpawns.Add(dedupeKey);
            }

            BattleController battle = GameMaster.Instance?.CurrentGameRun?.Battle;
            if (battle == null)
            {
                Plugin.Logger?.LogWarning("[EnemySpawnSync] No active battle, skipping spawn replay.");
                return;
            }

            if (!root.TryGetProperty("Spawned", out JsonElement spawnedEl) || spawnedEl.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            string spawnId = GetString(root, "SpawnId") ?? GetString(spawnedEl, "SpawnId");

            if (!root.TryGetProperty("Args", out JsonElement argsEl) || argsEl.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            string enemyTypeName = GetString(spawnedEl, "Type") ?? GetString(argsEl, "EnemyType");
            int rootIndex = GetInt(argsEl, "RootIndex", fallback: -1);
            bool isServant = GetBool(argsEl, "IsServant", fallback: true);

            if (string.IsNullOrWhiteSpace(enemyTypeName) || rootIndex < 0)
            {
                return;
            }

            EnemyUnit spawner = FindSpawner(battle, root);

            EnemyUnit enemyUnit;
            try
            {
                enemyUnit = Library.CreateEnemyUnit(enemyTypeName);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnemySpawnSync] CreateEnemyUnit failed for '{enemyTypeName}': {ex.Message}");
                return;
            }

            using (SpawnedEnemyManager.SuppressBroadcast())
            {

                EnemyUnit spawned = Traverse.Create(battle)
                                            .Method("Spawn", spawner, enemyUnit, rootIndex, isServant)
                                            .GetValue<EnemyUnit>();

                if (spawned != null && !string.IsNullOrWhiteSpace(spawnId))
                {
                    SpawnedEnemySyncPatch.BindSpawnId(spawned, spawnId);
                }

                ApplySpawnedSnapshot(spawned, spawnedEl);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemySpawnSync] HandleEnemySpawned failed: {ex.Message}");
        }
    }

    #endregion

    #region 辅助方法

        private static EnemyUnit FindSpawner(BattleController battle, JsonElement root)
    {
        try
        {
            if (!root.TryGetProperty("Spawner", out JsonElement spawnerEl) || spawnerEl.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            int rootIndex = GetInt(spawnerEl, "RootIndex", fallback: -1);
            int index = GetInt(spawnerEl, "Index", fallback: -1);
            string id = GetString(spawnerEl, "Id");

            foreach (EnemyUnit e in battle.EnemyGroup)
            {
                if (e == null)
                {
                    continue;
                }

                if (rootIndex >= 0 && e.RootIndex != rootIndex)
                {
                    continue;
                }

                if (index >= 0)
                {
                    int enemyIndex;
                    try
                    {
                        enemyIndex = Traverse.Create(e).Property("Index")?.GetValue<int>() ?? -1;
                    }
                    catch
                    {
                        enemyIndex = -1;
                    }

                    if (enemyIndex != index)
                    {
                        continue;
                    }
                }

                if (!string.IsNullOrWhiteSpace(id) && !string.Equals(e.Id, id, StringComparison.Ordinal))
                {
                    continue;
                }

                return e;
            }
        }
        catch
        {

        }

        return null;
    }
        private static void ApplySpawnedSnapshot(EnemyUnit spawned, JsonElement spawnedEl)
    {
        try
        {
            if (spawned == null)
            {
                return;
            }

            int currentHp = GetInt(spawnedEl, "CurrentHp", fallback: spawned.Hp);
            int maxHp = GetInt(spawnedEl, "MaxHp", fallback: spawned.MaxHp);
            int block = GetInt(spawnedEl, "Block", fallback: spawned.Block);
            int shield = GetInt(spawnedEl, "Shield", fallback: spawned.Shield);

            if (maxHp > 0 && spawned.MaxHp != maxHp)
            {

                try
                {
                    Traverse.Create(spawned).Property("MaxHp").SetValue(maxHp);
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogWarning($"[EnemySpawnSync] 设置属性异常（可接受）: {ex.Message}");
                }
            }

            try
            {
                Traverse.Create(spawned).Property("Hp").SetValue(currentHp);
                Traverse.Create(spawned).Property("Block").SetValue(block);
                Traverse.Create(spawned).Property("Shield").SetValue(shield);
            }
            catch (Exception ex) { Plugin.Logger?.LogWarning($"[EnemySpawnSync] Traverse 设置敌人属性失败: Hp={currentHp}, Block={block}, Shield={shield}, {ex.Message}"); }
        }
        catch (Exception ex)
        {

            Plugin.Logger?.LogWarning($"[EnemySpawnSync] 应用快照异常: {ex.Message}");
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
	            using JsonDocument doc = JsonDocument.Parse(s);
	            root = doc.RootElement.Clone();
                return true;
            }
        }
        catch
        {

        }

        root = default;
        return false;
    }

        private static string GetString(JsonElement root, string name)
        => NetworkEventHelper.GetString(root, name);

        private static int GetInt(JsonElement elem, string property, int fallback)
    {
        if (NetworkEventHelper.TryGetInt(elem, property, out int v))
            return v;
        return fallback;
    }

        private static bool GetBool(JsonElement elem, string property, bool fallback)
        => NetworkEventHelper.GetBool(elem, property, fallback);

    #endregion
}
