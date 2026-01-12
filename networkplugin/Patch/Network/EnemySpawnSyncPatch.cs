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
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.EnemyUnits;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 敌人生成同步补丁
/// 客机接收主机的 EnemySpawned 事件并在本地真正生成敌人。
/// - 主机：正常执行 BattleController.Spawn，并广播 EnemySpawned（由 SpawnedEnemyManager 负责）
/// - 客机：收到 EnemySpawned 后，调用 BattleController 的私有 Spawn 重放生成流程（并抑制二次广播）
/// 同时处理网络连接状态变化和消息去重，确保生成同步的可靠性。
/// </summary>
public static class EnemySpawnSyncPatch
{
    #region 字段和属性

    /// <summary>
    /// 服务提供者，用于获取网络客户端实例
    /// </summary>
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 是否已订阅网络客户端事件
    /// </summary>
    private static bool _subscribed;

    /// <summary>
    /// 当前订阅的网络客户端实例
    /// </summary>
    private static INetworkClient _subscribedClient;

    /// <summary>
    /// 游戏事件接收回调
    /// </summary>
    private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;

    /// <summary>
    /// 连接状态变化回调
    /// </summary>
    private static readonly Action<bool> _onConnectionStateChanged = OnConnectionStateChanged;

    /// <summary>
    /// 当前玩家ID
    /// </summary>
    private static string _selfPlayerId;

    /// <summary>
    /// 当前玩家是否为主机
    /// </summary>
    private static bool _selfIsHost;

    /// <summary>
    /// 同步锁，用于线程安全访问共享字段
    /// </summary>
    private static readonly object _syncLock = new();

    /// <summary>
    /// 已处理的生成事件集合，用于去重（格式：BattleId:SpawnIndex）
    /// </summary>
    private static readonly HashSet<string> _processedSpawns = new(StringComparer.Ordinal);

    #endregion

    #region Harmony补丁

    /// <summary>
    /// 订阅钩子，在GameDirector.Update后确保订阅网络客户端事件
    /// </summary>
    [HarmonyPatch(typeof(GameDirector), "Update")]
    private static class SubscribeHook
    {
        /// <summary>
        /// 后置补丁，每帧检查并订阅网络客户端事件
        /// </summary>
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

    /// <summary>
    /// 尝试获取网络客户端实例
    /// </summary>
    /// <returns>网络客户端实例，如果获取失败则返回null</returns>
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

    /// <summary>
    /// 确保订阅指定网络客户端的事件
    /// </summary>
    /// <param name="client">要订阅的网络客户端</param>
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
            // 忽略取消订阅异常
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

    /// <summary>
    /// 连接状态变化事件处理
    /// </summary>
    /// <param name="connected">是否已连接</param>
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

    /// <summary>
    /// 游戏事件接收处理
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="payload">事件负载</param>
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
            case NetworkMessageTypes.EnemySpawned:
                HandleEnemySpawned(root);
                return;
        }
    }

    /// <summary>
    /// 处理Welcome消息，更新本地玩家信息
    /// </summary>
    /// <param name="root">消息JSON根元素</param>
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
            // 忽略解析异常
        }
    }

    /// <summary>
    /// 处理主机变更消息，更新本地主机状态
    /// </summary>
    /// <param name="root">消息JSON根元素</param>
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
            // 忽略解析异常
        }
    }

    /// <summary>
    /// 处理敌人生成消息，在客机端重放生成流程
    /// </summary>
    /// <param name="root">消息JSON根元素</param>
    private static void HandleEnemySpawned(JsonElement root)
    {
        try
        {
            bool isHost;
            lock (_syncLock)
            {
                isHost = _selfIsHost;
            }

            // 主机不需要重放（否则会生成两次：本地一次 + 收到转发一次）
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
                // 调用 BattleController 私有 Spawn(spawner, enemyUnit, rootIndex, isServant) 以复用原生生成流程（EnterBattle/OnSpawn 等）
                EnemyUnit spawned = Traverse.Create(battle)
                                            .Method("Spawn", spawner, enemyUnit, rootIndex, isServant)
                                            .GetValue<EnemyUnit>();

                // 尽量把可见基础状态对齐（后续细节由 EnemySyncPatch 的增量同步覆盖）
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

    /// <summary>
    /// 查找生成者敌人单位
    /// </summary>
    /// <param name="battle">战斗控制器</param>
    /// <param name="root">消息JSON根元素</param>
    /// <returns>找到的生成者，如果未找到则返回null</returns>
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

                if (index >= 0 && e.RootIndex != index)
                {
                    continue;
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
            // 忽略查找异常
        }

        return null;
    }

    /// <summary>
    /// 应用生成快照，同步敌人单位的基础状态
    /// </summary>
    /// <param name="spawned">生成的敌人单位</param>
    /// <param name="spawnedEl">生成快照JSON元素</param>
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
                // EnemyUnit.MaxHp 仅 protected set，这里用 Traverse 兜底；失败则不强制
                try
                {
                    Traverse.Create(spawned).Property("MaxHp").SetValue(maxHp);
                }
                catch
                {
                    // 忽略设置异常
                }
            }

            // Unit.Hp/Block/Shield 的 setter 多为 internal；用 Traverse 确保插件侧可写。
            try { Traverse.Create(spawned).Property("Hp").SetValue(currentHp); } catch { /* 忽略设置异常 */ }
            try { Traverse.Create(spawned).Property("Block").SetValue(block); } catch { /* 忽略设置异常 */ }
            try { Traverse.Create(spawned).Property("Shield").SetValue(shield); } catch { /* 忽略设置异常 */ }
        }
        catch
        {
            // 忽略应用快照异常
        }
    }

    /// <summary>
    /// 尝试将负载转换为JsonElement
    /// </summary>
    /// <param name="payload">事件负载</param>
    /// <param name="root">输出的JsonElement</param>
    /// <returns>转换是否成功</returns>
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
            // 忽略转换异常
        }

        root = default;
        return false;
    }

    /// <summary>
    /// 从JsonElement获取字符串属性值
    /// </summary>
    /// <param name="elem">JSON元素</param>
    /// <param name="property">属性名</param>
    /// <returns>属性值字符串，如果获取失败则返回null</returns>
    private static string GetString(JsonElement elem, string property)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return null;
            }

            return p.ValueKind switch
            {
                JsonValueKind.String => p.GetString(),
                JsonValueKind.Number => p.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => null,
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 从JsonElement获取整数属性值
    /// </summary>
    /// <param name="elem">JSON元素</param>
    /// <param name="property">属性名</param>
    /// <param name="fallback">失败时的默认值</param>
    /// <returns>属性值整数</returns>
    private static int GetInt(JsonElement elem, string property, int fallback)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return fallback;
            }

            if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out int v))
            {
                return v;
            }

            if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out int vs))
            {
                return vs;
            }
        }
        catch
        {
            // 忽略解析异常
        }

        return fallback;
    }

    /// <summary>
    /// 从JsonElement获取布尔属性值
    /// </summary>
    /// <param name="elem">JSON元素</param>
    /// <param name="property">属性名</param>
    /// <param name="fallback">失败时的默认值</param>
    /// <returns>属性值布尔值</returns>
    private static bool GetBool(JsonElement elem, string property, bool fallback)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return fallback;
            }

            if (p.ValueKind == JsonValueKind.True || p.ValueKind == JsonValueKind.False)
            {
                return p.GetBoolean();
            }

            if (p.ValueKind == JsonValueKind.String && bool.TryParse(p.GetString(), out bool vb))
            {
                return vb;
            }
        }
        catch
        {
            // 忽略解析异常
        }

        return fallback;
    }

    #endregion
}
