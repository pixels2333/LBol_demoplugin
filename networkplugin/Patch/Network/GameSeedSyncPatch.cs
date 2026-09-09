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

[HarmonyPatch]
public static class GameSeedSyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetClient()
        => ServiceProvider?.GetService<INetworkClient>();

    private static readonly object CacheLock = new();
    private static HostGameStartConfig? _cachedConfig;

    private static bool _subscribed;
    private static INetworkClient? _subscribedClient;
    private static readonly Action<string, object> OnGameEventReceivedHandler = HandleGameEventReceived;

    public static bool TryGetCachedHostSeed(out ulong rootSeed)
    {
        lock (CacheLock)
        {
            if (_cachedConfig != null && _cachedConfig.RootSeed != 0)
            {
                rootSeed = _cachedConfig.RootSeed;
                return true;
            }
        }
        rootSeed = 0;
        return false;
    }

    public static bool TryGetCachedHostConfig(out HostGameStartConfig? config)
    {
        lock (CacheLock)
        {
            if (_cachedConfig != null && _cachedConfig.RootSeed != 0)
            {
                config = _cachedConfig.Clone();
                return true;
            }
        }
        config = null;
        return false;
    }

    public static bool TryGetCachedHostConfig(
        out ulong rootSeed,
        out GameDifficulty difficulty,
        out PuzzleFlag puzzles,
        out GameMode gameMode,
        out bool showRandomResult,
        out List<string> stageTypeNames,
        out string? debutAdventureTypeName,
        out List<string> jadeBoxIds)
    {
        if (TryGetCachedHostConfig(out var config) && config != null)
        {
            rootSeed = config.RootSeed;
            difficulty = config.Difficulty;
            puzzles = config.Puzzles;
            gameMode = config.GameMode;
            showRandomResult = config.ShowRandomResult;
            stageTypeNames = config.StageTypeNames;
            debutAdventureTypeName = config.DebutAdventureTypeName;
            jadeBoxIds = config.JadeBoxIds;
            return true;
        }

        rootSeed = default;
        difficulty = default;
        puzzles = default;
        gameMode = default;
        showRandomResult = default;
        stageTypeNames = new();
        debutAdventureTypeName = null;
        jadeBoxIds = new();
        return false;
    }

    public static void SetCachedResumeConfig(JsonElement root)
    {
        lock (CacheLock)
        {
            _cachedConfig ??= new HostGameStartConfig();

            if (root.TryGetProperty("RootSeed", out var sElem) && sElem.TryGetUInt64(out var sVal))
                _cachedConfig.RootSeed = sVal;
            if (root.TryGetProperty("Difficulty", out var dElem) && dElem.TryGetInt32(out var dVal))
                _cachedConfig.Difficulty = (GameDifficulty)dVal;
            if (root.TryGetProperty("GameMode", out var mElem) && mElem.TryGetInt32(out var mVal))
                _cachedConfig.GameMode = (GameMode)mVal;
            if (root.TryGetProperty("Puzzles", out var pElem) && pElem.TryGetInt32(out var pVal))
                _cachedConfig.Puzzles = (PuzzleFlag)pVal;
            else
                _cachedConfig.Puzzles = PuzzleFlag.None;
            if (root.TryGetProperty("ShowRandomResult", out var rElem) && (rElem.ValueKind == JsonValueKind.True || rElem.ValueKind == JsonValueKind.False))
                _cachedConfig.ShowRandomResult = rElem.GetBoolean();

            if (_cachedConfig.StageTypeNames == null || _cachedConfig.StageTypeNames.Count == 0)
            {
                _cachedConfig.StageTypeNames = new List<string>
                {
                    "BambooForest",
                    "XuanwuRavine",
                    "WindGodLake",
                    "FinalStage"
                };
            }
            if (string.IsNullOrWhiteSpace(_cachedConfig.DebutAdventureTypeName))
            {
                _cachedConfig.DebutAdventureTypeName = "Debut";
            }
        }
    }

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

            GameMasterSaveHookPatch.IsMultiplayerRunActive = true;
            NetworkIdentityTracker.EnsureSubscribed(client);

            if (!NetworkIdentityTracker.GetSelfIsHost())
            {
                return;
            }

            BroadcastHostGameStart(__result, parameters);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[GameSeedSync] GameRunController_Create_Postfix 广播失败: {ex.Message}");
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
        catch (Exception ex)
        {
            Plugin.Logger?.LogDebug($"[GameSeedSync] 获取关卡阶段名称异常: {ex.Message}");
        }

        string? debutAdventureTypeName = null;
        try
        {
            Type? debutType = run.Stages.Count > 0 ? run.Stages[0].DebutAdventureType : null;
            debutAdventureTypeName = debutType?.Name;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogDebug($"[GameSeedSync] 获取首发事件类型异常: {ex.Message}");
        }

        List<string> jadeBoxIds = new();
        try
        {
            foreach (var jb in run.JadeBoxes)
            {
                if (!string.IsNullOrWhiteSpace(jb.Id))
                    jadeBoxIds.Add(jb.Id);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogDebug($"[GameSeedSync] 获取玉匣列表异常: {ex.Message}");
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
            JadeBoxIds = jadeBoxIds,
        };

        client.BroadcastState(NetworkMessageTypes.OnGameStart, payload);
        Plugin.Logger?.LogInfo($"[GameSeedSync] 广播房主种子: RootSeed={run.RootSeed}, Difficulty={run.Difficulty}, Mode={run.Mode}");
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
                _subscribedClient.OnGameEventReceived -= OnGameEventReceivedHandler;
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogDebug($"[GameSeedSync] 退订网络事件异常: {ex.Message}");
        }

        try
        {
            client.OnGameEventReceived += OnGameEventReceivedHandler;
            _subscribedClient = client;
            _subscribed = true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[GameSeedSync] 订阅网络事件异常: {ex.Message}");
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
            List<string> jadeBoxIds = GetStringList(root, "JadeBoxIds");

            lock (CacheLock)
            {
                _cachedConfig = new HostGameStartConfig
                {
                    RootSeed = rootSeed.Value,
                    Difficulty = (GameDifficulty)(difficulty ?? 0),
                    Puzzles = (PuzzleFlag)(puzzles ?? 0),
                    GameMode = (GameMode)(gameMode ?? 0),
                    ShowRandomResult = showRandomResult ?? false,
                    StageTypeNames = stageTypeNames ?? new List<string>(),
                    DebutAdventureTypeName = debutAdventureTypeName,
                    HostPlayerId = hostPlayerId,
                    JadeBoxIds = jadeBoxIds ?? new List<string>()
                };
            }

            Plugin.Logger?.LogInfo($"[GameSeedSync] 已缓存房主种子: RootSeed={rootSeed}, HostId={hostPlayerId}");

            Plugin.RunOnMainThread(() => NetworkPlugin.Patch.UI.MainMenuMultiplayerEntryPatch.OnLobbyGameStartedReceived());
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GameSeedSync] 处理 OnGameStart 失败: {ex.Message}");
        }
    }

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

/// <summary>
/// 房主开局配置数据对象，消除配置字段散落的数据泥团（Data Clumps）。
/// </summary>
public class HostGameStartConfig
{
    public ulong RootSeed { get; set; }
    public GameDifficulty Difficulty { get; set; }
    public PuzzleFlag Puzzles { get; set; }
    public GameMode GameMode { get; set; }
    public bool ShowRandomResult { get; set; }
    public List<string> StageTypeNames { get; set; } = new();
    public string? DebutAdventureTypeName { get; set; }
    public string? HostPlayerId { get; set; }
    public List<string> JadeBoxIds { get; set; } = new();

    public HostGameStartConfig Clone()
    {
        return new HostGameStartConfig
        {
            RootSeed = RootSeed,
            Difficulty = Difficulty,
            Puzzles = Puzzles,
            GameMode = GameMode,
            ShowRandomResult = ShowRandomResult,
            StageTypeNames = new List<string>(StageTypeNames),
            DebutAdventureTypeName = DebutAdventureTypeName,
            HostPlayerId = HostPlayerId,
            JadeBoxIds = new List<string>(JadeBoxIds)
        };
    }
}
