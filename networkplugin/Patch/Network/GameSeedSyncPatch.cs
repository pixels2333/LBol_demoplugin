using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 游戏种子同步补丁：房主创建 GameRun 后广播 RootSeed 及开局配置，
/// 客户端收到后缓存，供 <see cref="MidGameJoin.JoinerStartGameLockPatch"/> 在无 FullStateSnapshot 时
/// 覆盖本地 seed，使 <see cref="EnemyUnits.SpawnedEnemyManager"/> 的确定性敌人生成种子与房主一致。
/// </summary>
/// <remarks>
/// 正常联机开始游戏时，OnGameStart/FullStateSync 等流程不传递 RootSeed，
/// 导致客户端各自随机生成种子、敌人生成不同步。此补丁填补该缺口：
/// 1. 房主侧：GameRunController.Create Postfix → 广播 OnGameStart（含 RootSeed+配置）
/// 2. 客户端侧：订阅网络事件 → 收到 OnGameStart → 缓存
/// 3. JoinerStartGameLockPatch：无 FullStateSnapshot 时 fallback 到缓存种子
/// </remarks>
[HarmonyPatch]
public static class GameSeedSyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetClient()
        => ServiceProvider?.GetService<INetworkClient>();

    // --- 缓存：客户端收到的房主开局配置 ---
    private static readonly object CacheLock = new();
    private static ulong? _cachedRootSeed;
    private static int? _cachedDifficulty;
    private static int? _cachedPuzzles;
    private static int? _cachedGameMode;
    private static bool? _cachedShowRandomResult;
    private static List<string>? _cachedStageTypeNames;
    private static string? _cachedDebutAdventureTypeName;
    private static string? _cachedHostPlayerId;

    private static bool _subscribed;
    private static INetworkClient? _subscribedClient;
    private static readonly Action<string, object> OnGameEventReceivedHandler = HandleGameEventReceived;

    /// <summary>
    /// 尝试获取缓存的房主 RootSeed（供 JoinerStartGameLockPatch 使用）。
    /// </summary>
    public static bool TryGetCachedHostSeed(out ulong rootSeed)
    {
        lock (CacheLock)
        {
            if (_cachedRootSeed.HasValue)
            {
                rootSeed = _cachedRootSeed.Value;
                return true;
            }
        }
        rootSeed = 0;
        return false;
    }

    /// <summary>
    /// 尝试获取缓存的房主完整开局配置（供 JoinerStartGameLockPatch fallback 使用）。
    /// </summary>
    public static bool TryGetCachedHostConfig(
        out ulong rootSeed,
        out GameDifficulty difficulty,
        out PuzzleFlag puzzles,
        out GameMode gameMode,
        out bool showRandomResult,
        out List<string> stageTypeNames,
        out string? debutAdventureTypeName)
    {
        lock (CacheLock)
        {
            rootSeed = default;
            difficulty = default;
            puzzles = default;
            gameMode = default;
            showRandomResult = default;
            stageTypeNames = new();
            debutAdventureTypeName = null;

            if (!_cachedRootSeed.HasValue ||
                _cachedDifficulty == null ||
                _cachedPuzzles == null ||
                _cachedGameMode == null ||
                _cachedStageTypeNames == null ||
                _cachedStageTypeNames.Count == 0)
            {
                return false;
            }

            rootSeed = _cachedRootSeed.Value;
            difficulty = (GameDifficulty)_cachedDifficulty.Value;
            puzzles = (PuzzleFlag)_cachedPuzzles.Value;
            gameMode = (GameMode)_cachedGameMode.Value;
            showRandomResult = _cachedShowRandomResult ?? false;
            stageTypeNames = new List<string>(_cachedStageTypeNames);
            debutAdventureTypeName = _cachedDebutAdventureTypeName;
            return true;
        }
    }

    // --- 发送端：房主创建 GameRun 后广播种子 ---

    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.Create))]
    [HarmonyPostfix]
    public static void GameRunController_Create_Postfix(GameRunController __result, GameRunStartupParameters parameters)
    {
        try
        {
            if (__result == null)
            {
                return;
            }

            INetworkClient client = TryGetClient();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            NetworkIdentityTracker.EnsureSubscribed(client);

            // 仅房主广播。
            if (!NetworkIdentityTracker.GetSelfIsHost())
            {
                return;
            }

            BroadcastHostGameStart(__result, parameters);
        }
        catch
        {
            // 广播失败不影响本地游戏启动。
        }
    }

    private static void BroadcastHostGameStart(GameRunController run, GameRunStartupParameters parameters)
    {
        INetworkClient client = TryGetClient();
        if (client == null)
        {
            return;
        }

        List<string> stageTypeNames = new();
        try
        {
            foreach (Stage stage in run.Stages)
            {
                stageTypeNames.Add(stage.GetType().Name);
            }
        }
        catch
        {
            // ignored
        }

        string? debutAdventureTypeName = null;
        try
        {
            Type? debutType = run.Stages.Count > 0 ? run.Stages[0].DebutAdventureType : null;
            debutAdventureTypeName = debutType?.Name;
        }
        catch
        {
            // ignored
        }

        string hostId = NetworkIdentityTracker.GetSelfPlayerId();
        if (string.IsNullOrWhiteSpace(hostId))
        {
            hostId = GameStateUtils.GetCurrentPlayerId() ?? "unknown";
        }

        var payload = new
        {
            Timestamp = DateTime.UtcNow.Ticks,
            HostPlayerId = hostId,
            RootSeed = run.RootSeed,
            UISeed = run.UISeed,
            Difficulty = (int)run.Difficulty,
            Puzzles = (int)run.Puzzles,
            GameMode = (int)run.Mode,
            ShowRandomResult = run.ShowRandomResult,
            StageTypeNames = stageTypeNames,
            DebutAdventureTypeName = debutAdventureTypeName,
        };

        client.SendGameEventData(NetworkMessageTypes.OnGameStart, payload);
        Plugin.Logger?.LogInfo($"[GameSeedSync] 广播房主种子: RootSeed={run.RootSeed}, Difficulty={run.Difficulty}, Mode={run.Mode}");
    }

    // --- 接收端：客户端缓存房主种子 ---

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
                _subscribedClient.OnGameEventReceived -= OnGameEventReceivedHandler;
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            client.OnGameEventReceived += OnGameEventReceivedHandler;
            _subscribedClient = client;
            _subscribed = true;
        }
        catch
        {
            _subscribedClient = null;
            _subscribed = false;
        }
    }

    private static void HandleGameEventReceived(string eventType, object payload)
    {
        if (eventType != NetworkMessageTypes.OnGameStart)
        {
            return;
        }

        if (!NetworkEventHelper.TryGetJsonElement(payload, out JsonElement root))
        {
            return;
        }

        try
        {
            // 房主自己发出的事件可能回环，跳过。
            if (NetworkIdentityTracker.GetSelfIsHost())
            {
                return;
            }

            ulong? rootSeed = GetULong(root, "RootSeed");
            if (rootSeed == null)
            {
                Plugin.Logger?.LogWarning("[GameSeedSync] 收到 OnGameStart 但 RootSeed 缺失");
                return;
            }

            int? difficulty = GetInt(root, "Difficulty");
            int? puzzles = GetInt(root, "Puzzles");
            int? gameMode = GetInt(root, "GameMode");
            bool? showRandomResult = GetBool(root, "ShowRandomResult");
            List<string> stageTypeNames = GetStringList(root, "StageTypeNames");
            string? debutAdventureTypeName = NetworkEventHelper.GetString(root, "DebutAdventureTypeName");
            string? hostPlayerId = NetworkEventHelper.GetString(root, "HostPlayerId");

            lock (CacheLock)
            {
                _cachedRootSeed = rootSeed;
                _cachedDifficulty = difficulty;
                _cachedPuzzles = puzzles;
                _cachedGameMode = gameMode;
                _cachedShowRandomResult = showRandomResult;
                _cachedStageTypeNames = stageTypeNames;
                _cachedDebutAdventureTypeName = debutAdventureTypeName;
                _cachedHostPlayerId = hostPlayerId;
            }

            Plugin.Logger?.LogInfo($"[GameSeedSync] 已缓存房主种子: RootSeed={rootSeed}, HostId={hostPlayerId}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GameSeedSync] 处理 OnGameStart 失败: {ex.Message}");
        }
    }

    // --- 订阅钩子：在 GameDirector.Update 中确保订阅 ---

    [HarmonyPatch(typeof(GameDirector), "Update")]
    private static class SubscribeHook
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            INetworkClient? client = TryGetClient();
            if (client == null || !client.IsConnected)
            {
                return;
            }
            EnsureSubscribed(client);
        }
    }

    // --- 辅助方法 ---

    private static ulong? GetULong(JsonElement elem, string name)
    {
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(name, out JsonElement p))
        {
            return null;
        }
        if (p.ValueKind == JsonValueKind.Number && p.TryGetUInt64(out ulong v))
        {
            return v;
        }
        if (p.ValueKind == JsonValueKind.String && ulong.TryParse(p.GetString(), out ulong s))
        {
            return s;
        }
        return null;
    }

    private static int? GetInt(JsonElement elem, string name)
    {
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(name, out JsonElement p))
        {
            return null;
        }
        if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out int v))
        {
            return v;
        }
        return null;
    }

    private static bool? GetBool(JsonElement elem, string name)
    {
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(name, out JsonElement p))
        {
            return null;
        }
        if (p.ValueKind == JsonValueKind.True) return true;
        if (p.ValueKind == JsonValueKind.False) return false;
        return null;
    }

    private static List<string> GetStringList(JsonElement elem, string name)
    {
        List<string> result = new();
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(name, out JsonElement p))
        {
            return result;
        }
        if (p.ValueKind != JsonValueKind.Array)
        {
            return result;
        }
        foreach (JsonElement item in p.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                string? s = item.GetString();
                if (!string.IsNullOrWhiteSpace(s))
                {
                    result.Add(s);
                }
            }
        }
        return result;
    }
}