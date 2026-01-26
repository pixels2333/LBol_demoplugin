// 存档/读档同步补丁：在保存与加载流程中广播必要的同步事件与快照。
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.RoomSync;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 存档/读档同步补丁。
/// </summary>
/// <remarks>
/// 主要目标：
/// - 主机在保存时广播必要的快照，供客户端尽力对齐进度。
/// - 客户端在加载后可请求主机发送一次最新快照。
/// 注意：该模块以“诊断/尽力同步”为主，不承诺能完全覆盖游戏内部存档细节。
/// </remarks>
public class SaveLoadSyncPatch
{
    #region 字段与同步缓存

    /// <summary>
    /// 依赖注入服务提供者。
    /// </summary>
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    private static bool IsEnabled()
    {
        try
        {
            return Plugin.ConfigManager?.EnableSaveLoadSync?.Value == true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 按玩家缓存最近一次同步的快照。
    /// </summary>
    private static readonly Dictionary<string, GameStateSnapshot> _syncedSnapshots = [];

    /// <summary>
    /// 同步锁，保护快照缓存。
    /// </summary>
    private static readonly object _syncLock = new();

    private static bool IsPatchable(MethodInfo method)
    {
        // Harmony detour 需要 IL 方法体；abstract / extern / runtime 特殊方法会在运行时抛 "Method has no body"。
        if (method == null)
        {
            return false;
        }

        if (method.IsAbstract)
        {
            return false;
        }

        if (method.IsGenericMethodDefinition)
        {
            return false;
        }

        if (method.ContainsGenericParameters)
        {
            return false;
        }

        if (method.GetMethodBody() == null)
        {
            return false;
        }

        return true;
    }

    private static IEnumerable<Assembly> EnumerateGameAssemblies()
    {
        // 只扫描 LBoL 自身程序集，避免误命中 Unity/MonoMod/Harmony 等第三方或动态程序集。
        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            string name = null;
            try
            {
                name = asm.GetName().Name;
            }
            catch
            {
                // ignored
            }

            if (string.IsNullOrWhiteSpace(name) || !name.StartsWith("LBoL", StringComparison.Ordinal))
            {
                continue;
            }

            yield return asm;
        }
    }

    #endregion

    #region 保存同步

    /// <summary>
    /// 游戏保存同步：在保存流程前后记录状态并广播。
    /// </summary>
    [HarmonyPatch]
    public static class GameSaveSync
    {
        /// <summary>
        /// 目标方法集合：通过反射扫描可能的 Save 入口。
        /// </summary>
        /// <returns>可能的保存方法列表。</returns>
        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            if (!IsEnabled())
            {
                return Array.Empty<MethodBase>();
            }

            string[] methodNames =
            [
                "SaveGame",
                "SaveToFile",
                "WriteSaveData",
                "CreateSave",
            ];

            List<MethodBase> methods = [];

            foreach (var assembly in EnumerateGameAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        foreach (var method in type.GetMethods())
                        {
                            if (!IsPatchable(method))
                            {
                                continue;
                            }

                            bool nameMatch = methodNames.Any(name => method.Name.Contains(name));
                            bool paramMatch = method.ReturnType == typeof(void)
                                             && method.GetParameters().Any(p => p.ParameterType.Name.Contains("Save"));

                            if (nameMatch || paramMatch)
                            {
                                methods.Add(method);
                            }
                        }
                    }
                }
                catch
                {
                    // 忽略无法访问的程序集。
                }
            }

            return methods;
        }

        [HarmonyPrepare]
        private static bool Prepare()
        {
            // 避免因为扫描不到目标或目标不可 patch 导致 PatchAll 崩溃。
            return TargetMethods().Any();
        }

        /// <summary>
        /// 保存前置：记录保存类型与保存前快照。
        /// </summary>
        /// <param name="__instance">保存调用实例。</param>
        /// <param name="__args">保存调用参数。</param>
        /// <param name="__state">用于保存前置信息。</param>
        [HarmonyPrefix]
        private static void GameSave_Prefix(object __instance, object[] __args, out SaveSyncState __state)
        {
            __state = new SaveSyncState();
            try
            {
                if (!IsEnabled())
                {
                    return;
                }

                __state.SaveStartTime = DateTime.Now;
                __state.SaveType = DetermineSaveType(__instance, __args);
                __state.PlayerId = GetCurrentPlayerId();
                __state.GameStateBefore = CaptureGameState();

                Plugin.Logger?.LogInfo($"[SaveLoadSync] 开始保存: {__state.SaveType}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SaveLoadSync] GameSave_Prefix 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存后置：主机广播保存后的快照。
        /// </summary>
        /// <param name="__instance">保存调用实例。</param>
        /// <param name="__args">保存调用参数。</param>
        /// <param name="__state">前置保存的信息。</param>
        [HarmonyPostfix]
        private static void GameSave_Postfix(object __instance, object[] __args, SaveSyncState __state)
        {
            try
            {
                if (!IsEnabled())
                {
                    return;
                }

                if (serviceProvider == null)
                {
                    return;
                }

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                {
                    return;
                }

                // 只同步主机保存，避免客户端重复广播。
                if (!IsHostPlayer())
                {
                    Plugin.Logger?.LogDebug("[SaveLoadSync] 非主机保存，跳过同步");
                    return;
                }

                var saveTime = DateTime.Now - __state.SaveStartTime;
                object gameStateAfter = CaptureGameState();

                var syncData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.OnGameSave,
                    PlayerId = __state.PlayerId,
                    SaveType = __state.SaveType,
                    SaveDurationMs = saveTime.TotalMilliseconds,
                    GameStateSnapshot = gameStateAfter,
                    SaveSlot = DetermineSaveSlot(__args),
                    IsAutoSave = __state.SaveType.Contains("Auto"),
                    MultiplayerSync = true,
                };

                // 缓存快照，供后续查询/响应。
                GameStateSnapshot snapshot = new GameStateSnapshot
                {
                    PlayerId = __state.PlayerId,
                    SaveType = __state.SaveType,
                    Timestamp = DateTime.Now.Ticks,
                    GameState = gameStateAfter,
                };

                lock (_syncLock)
                {
                    _syncedSnapshots[__state.PlayerId] = snapshot;
                }

                SendGameEvent(NetworkMessageTypes.OnGameSave, syncData);

                Plugin.Logger?.LogInfo($"[SaveLoadSync] 保存同步完成: {__state.SaveType} (耗时 {saveTime.TotalMilliseconds:F1}ms)");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SaveLoadSync] GameSave_Postfix 异常: {ex.Message}");
            }
        }
    }

    #endregion

    #region 加载同步

    /// <summary>
    /// 游戏加载同步：在加载流程前后记录状态并广播。
    /// </summary>
    [HarmonyPatch]
    public static class GameLoadSync
    {
        /// <summary>
        /// 目标方法集合：通过反射扫描可能的 Load 入口。
        /// </summary>
        /// <returns>可能的加载方法列表。</returns>
        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            if (!IsEnabled())
            {
                return Array.Empty<MethodBase>();
            }

            string[] methodNames =
            [
                "LoadGame",
                "LoadFromFile",
                "ReadSaveData",
                "LoadSave",
            ];

            List<MethodBase> methods = [];

            foreach (var assembly in EnumerateGameAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        foreach (var method in type.GetMethods())
                        {
                            if (!IsPatchable(method))
                            {
                                continue;
                            }

                            bool nameMatch = methodNames.Any(name => method.Name.Contains(name));
                            bool paramMatch = method.ReturnType != typeof(void)
                                             && method.GetParameters().Any(p => p.ParameterType.Name.Contains("Save"));

                            if (nameMatch || paramMatch)
                            {
                                methods.Add(method);
                            }
                        }
                    }
                }
                catch
                {
                    // 忽略无法访问的程序集。
                }
            }

            return methods;
        }

        [HarmonyPrepare]
        private static bool Prepare()
        {
            return TargetMethods().Any();
        }

        /// <summary>
        /// 加载前置：记录加载类型与加载前快照。
        /// </summary>
        /// <param name="__instance">加载调用实例。</param>
        /// <param name="__args">加载调用参数。</param>
        /// <param name="__state">用于保存前置信息。</param>
        [HarmonyPrefix]
        private static void GameLoad_Prefix(object __instance, object[] __args, out LoadSyncState __state)
        {
            __state = new LoadSyncState();
            try
            {
                if (!IsEnabled())
                {
                    return;
                }

                __state.LoadStartTime = DateTime.Now;
                __state.LoadType = DetermineLoadType(__args);
                __state.PlayerId = GetCurrentPlayerId();
                __state.GameStateBefore = CaptureGameState();
                __state.SaveSlot = DetermineSaveSlot(__args);

                Plugin.Logger?.LogInfo($"[SaveLoadSync] 开始加载: {__state.LoadType}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SaveLoadSync] GameLoad_Prefix 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载后置：广播加载后的快照；客户端可额外请求主机同步。
        /// </summary>
        /// <param name="__instance">加载调用实例。</param>
        /// <param name="__args">加载调用参数。</param>
        /// <param name="__state">前置保存的信息。</param>
        [HarmonyPostfix]
        private static void GameLoad_Postfix(object __instance, object[] __args, LoadSyncState __state)
        {
            try
            {
                if (!IsEnabled())
                {
                    return;
                }

                if (serviceProvider == null)
                {
                    return;
                }

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                {
                    return;
                }

                var loadTime = DateTime.Now - __state.LoadStartTime;
                object gameStateAfter = CaptureGameState();

                // 客户端加载后可请求主机发送一次最新快照。
                if (!IsHostPlayer())
                {
                    SaveSyncManager.RequestHostSaveSync(__state.SaveSlot);
                }

                var syncData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.OnGameLoad,
                    PlayerId = __state.PlayerId,
                    LoadType = __state.LoadType,
                    LoadDurationMs = loadTime.TotalMilliseconds,
                    GameStateSnapshot = gameStateAfter,
                    SaveSlot = __state.SaveSlot,
                    IsAutoLoad = __state.LoadType.Contains("Auto"),
                    RequestingHostSync = !IsHostPlayer(),
                };

                SendGameEvent(NetworkMessageTypes.OnGameLoad, syncData);

                Plugin.Logger?.LogInfo($"[SaveLoadSync] 加载同步完成: {__state.LoadType} (耗时 {loadTime.TotalMilliseconds:F1}ms)");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SaveLoadSync] GameLoad_Postfix 异常: {ex.Message}");
            }
        }
    }

    #endregion

    #region 自动保存/快速保存同步

    /// <summary>
    /// 快速保存同步：对自动保存类方法做节流同步。
    /// </summary>
    [HarmonyPatch]
    public static class QuickSaveSync
    {
        private static DateTime _lastQuickSave = DateTime.MinValue;
        private static readonly TimeSpan QUICK_SAVE_COOLDOWN = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 目标方法集合：通过反射扫描 QuickSave/AutoSave/CheckpointSave。
        /// </summary>
        /// <returns>可能的快速保存方法列表。</returns>
        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            if (!IsEnabled())
            {
                return Array.Empty<MethodBase>();
            }

            string[] methodNames =
            [
                "QuickSave",
                "AutoSave",
                "CheckpointSave",
            ];

            List<MethodBase> methods = [];

            foreach (var assembly in EnumerateGameAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        foreach (var method in type.GetMethods())
                        {
                            if (!IsPatchable(method))
                            {
                                continue;
                            }

                            if (methodNames.Any(name => method.Name.Contains(name)))
                            {
                                methods.Add(method);
                            }
                        }
                    }
                }
                catch
                {
                    // 忽略无法访问的程序集。
                }
            }

            return methods;
        }

        [HarmonyPrepare]
        private static bool Prepare()
        {
            return TargetMethods().Any();
        }

        /// <summary>
        /// 快速保存后置：主机按冷却时间节流后广播快照。
        /// </summary>
        /// <param name="__instance">调用实例。</param>
        /// <param name="__args">调用参数。</param>
        [HarmonyPostfix]
        public static void QuickSave_Postfix(object __instance, object[] __args)
        {
            try
            {
                if (!IsEnabled())
                {
                    return;
                }

                var now = DateTime.Now;
                if (now - _lastQuickSave < QUICK_SAVE_COOLDOWN)
                {
                    // 节流：避免过于频繁广播。
                    return;
                }

                if (serviceProvider == null)
                {
                    return;
                }

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                {
                    return;
                }

                // 只有主机执行快速保存同步。
                if (!IsHostPlayer())
                {
                    return;
                }

                object gameState = CaptureGameState();
                var syncData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = "QuickSaveSync",
                    PlayerId = GetCurrentPlayerId(),
                    GameStateSnapshot = gameState,
                    SaveFrequency = "Throttled",
                };

                SendGameEvent("QuickSaveSync", syncData);

                _lastQuickSave = now;

                Plugin.Logger?.LogDebug("[SaveLoadSync] 快速保存已同步");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SaveLoadSync] QuickSave_Postfix 异常: {ex.Message}");
            }
        }
    }

    #endregion

    #region 存档同步管理器

    /// <summary>
    /// 存档同步管理器：处理“向主机请求快照”与“主机响应快照”。
    /// </summary>
    public static class SaveSyncManager
    {
        /// <summary>
        /// 请求主机发送存档同步。
        /// </summary>
        /// <param name="saveSlot">存档槽标识。</param>
        public static void RequestHostSaveSync(string saveSlot)
        {
            try
            {
                if (serviceProvider == null)
                {
                    return;
                }

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                {
                    return;
                }

                var requestData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = "SaveSyncRequest",
                    RequesterId = GetCurrentPlayerId(),
                    SaveSlot = saveSlot,
                    RequestType = "FullSync",
                };

                SendGameEvent("SaveSyncRequest", requestData);

                Plugin.Logger?.LogInfo($"[SaveLoadSync] 已请求主机同步存档槽: {saveSlot}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SaveLoadSync] 请求主机存档同步失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 主机响应存档同步请求：发送一次完整快照。
        /// </summary>
        /// <param name="requesterId">请求方玩家 Id。</param>
        public static void RespondToSaveSyncRequest(string requesterId)
        {
            try
            {
                if (!IsHostPlayer())
                {
                    return;
                }

                if (serviceProvider == null)
                {
                    return;
                }

                var networkClient = serviceProvider.GetService<INetworkClient>();
                if (networkClient == null || !networkClient.IsConnected)
                {
                    return;
                }

                object gameState = CaptureGameState();
                var responseData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = "SaveSyncResponse",
                    RequesterId = requesterId,
                    HostId = GetCurrentPlayerId(),
                    GameStateSnapshot = gameState,
                    SyncType = "FullSync",
                    SaveTime = DateTime.Now.Ticks,
                };

                SendGameEvent("SaveSyncResponse", responseData);

                Plugin.Logger?.LogInfo($"[SaveLoadSync] 已响应存档同步请求: {requesterId}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SaveLoadSync] 响应存档同步请求失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 应用来自主机的存档同步数据（预留）。
        /// </summary>
        /// <param name="saveSyncData">同步数据。</param>
        public static void ApplyHostSaveSync(object saveSyncData)
        {
            try
            {
                // 兼容：原 TODO 计划用于“主机存档/快照应用”。
                // 目前项目的同步目标已转向“房间/战斗残局同步”，这里把可识别的房间快照透传给 RoomSyncManager。
                if (saveSyncData == null)
                {
                    return;
                }

                // 如果 payload 已经是 RoomStateResponse/RoomStateBroadcast 形态（包含 RoomKey），则直接注入给 RoomSyncManager。
                // RoomSyncManager 通过订阅 OnGameEventReceived 处理 JsonElement；这里走一次本地注入，确保 Host/Client 都可用。
                if (serviceProvider != null)
                {
                    var client = serviceProvider.GetService<INetworkClient>();
                    if (client != null)
                    {
                        NetworkIdentityTracker.EnsureSubscribed(client);
                        RoomSyncManager.EnsureSubscribed(client);

                        // 仅当底层实现支持本地注入时才注入；否则依赖正常网络通道。
                        if (client is NetworkClient liteNetClient)
                        {
                            liteNetClient.InjectLocalGameEvent(NetworkMessageTypes.RoomStateResponse, saveSyncData);
                            Plugin.Logger?.LogInfo("[SaveLoadSync] 已将主机快照交由 RoomSyncManager 处理。");
                            return;
                        }
                    }
                }

                Plugin.Logger?.LogInfo("[SaveLoadSync] 已收到主机同步数据，但客户端未就绪，已跳过应用。");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SaveLoadSync] 应用主机存档同步失败: {ex.Message}");
            }
        }
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 发送游戏事件（优先使用 SendGameEventData，失败则回落到 SendRequest）。
    /// </summary>
    /// <param name="eventType">事件类型。</param>
    /// <param name="eventData">事件数据。</param>
    private static void SendGameEvent(string eventType, object eventData)
    {
        try
        {
            var networkClient = serviceProvider?.GetService<INetworkClient>();
            if (networkClient is NetworkClient liteNetClient)
            {
                liteNetClient.SendGameEventData(eventType, eventData);
            }
            else
            {
                networkClient?.SendRequest(eventType, JsonCompat.Serialize(eventData));
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SaveLoadSync] 发送事件 {eventType} 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取当前玩家 ID。
    /// </summary>
    /// <returns>玩家标识字符串。</returns>
    private static string GetCurrentPlayerId()
    {
        try
        {
            string id = NetworkIdentityTracker.GetSelfPlayerId();
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }

            id = GameStateUtils.GetCurrentPlayerId();
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }

            return "unknown_player";
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SaveLoadSync] 获取当前玩家 ID 失败: {ex.Message}");
            return "unknown_player";
        }
    }

    /// <summary>
    /// 判断当前玩家是否主机。
    /// </summary>
    /// <returns>是主机返回 true，否则 false。</returns>
    private static bool IsHostPlayer()
    {
        try
        {
            // 优先使用 NetworkIdentityTracker（Welcome/PlayerListUpdate 会刷新），否则回退到 GameStateUtils 的判断。
            if (NetworkIdentityTracker.GetSelfIsHost())
            {
                return true;
            }

            return GameStateUtils.IsHost();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SaveLoadSync] 判断主机状态失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 抓取最小可用游戏状态快照（用于诊断与尽力同步）。
    /// </summary>
    /// <returns>可序列化对象。</returns>
    private static object CaptureGameState()
    {
        try
        {
            var player = GameStateUtils.GetCurrentPlayer();
            return new
            {
                Timestamp = DateTime.Now.Ticks,
                PlayerState = new
                {
                    PlayerId = GetCurrentPlayerId(),
                    IsHost = IsHostPlayer(),
                    ModelName = player?.ModelName,
                    Hp = player?.Hp,
                    MaxHp = player?.MaxHp,
                },
                BattleState = new
                {
                    InBattle = player?.Battle != null,
                },
                GameProgress = new
                {
                    GameStarted = player != null,
                    RunTimestamp = GameStateUtils.GetCurrentGameRun() != null ? DateTime.Now.Ticks : 0,
                },
            };
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SaveLoadSync] 抓取快照失败: {ex.Message}");
            return new { Error = ex.Message, Timestamp = DateTime.Now.Ticks };
        }
    }

    /// <summary>
    /// 根据调用实例/参数推断保存类型。
    /// </summary>
    /// <param name="instance">调用实例。</param>
    /// <param name="args">调用参数。</param>
    /// <returns>保存类型字符串。</returns>
    private static string DetermineSaveType(object instance, object[] args)
    {
        try
        {
            if (instance?.GetType().Name.Contains("Auto") == true)
            {
                return "AutoSave";
            }

            if (args.Any(arg => arg?.ToString().Contains("Quick") == true))
            {
                return "QuickSave";
            }

            if (args.Any(arg => arg?.ToString().Contains("Manual") == true))
            {
                return "ManualSave";
            }

            return "UnknownSave";
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SaveLoadSync] 推断保存类型失败: {ex.Message}");
            return "UnknownSave";
        }
    }

    /// <summary>
    /// 根据调用参数推断加载类型。
    /// </summary>
    /// <param name="args">调用参数。</param>
    /// <returns>加载类型字符串。</returns>
    private static string DetermineLoadType(object[] args)
    {
        try
        {
            if (args.Any(arg => arg?.ToString().Contains("Auto") == true))
            {
                return "AutoLoad";
            }

            if (args.Any(arg => arg?.ToString().Contains("Quick") == true))
            {
                return "QuickLoad";
            }

            if (args.Any(arg => arg?.ToString().Contains("Manual") == true))
            {
                return "ManualLoad";
            }

            return "UnknownLoad";
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SaveLoadSync] 推断加载类型失败: {ex.Message}");
            return "UnknownLoad";
        }
    }

    /// <summary>
    /// 根据参数推断存档槽。
    /// </summary>
    /// <param name="args">调用参数。</param>
    /// <returns>存档槽字符串。</returns>
    private static string DetermineSaveSlot(object[] args)
    {
        try
        {
            object slotArg = args.FirstOrDefault(arg => arg?.ToString().Contains("Slot") == true);
            return slotArg?.ToString() ?? "DefaultSlot";
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SaveLoadSync] 推断存档槽失败: {ex.Message}");
            return "DefaultSlot";
        }
    }

    #endregion

    #region 数据结构

    /// <summary>
    /// 保存同步前置状态。
    /// </summary>
    public class SaveSyncState
    {
        /// <summary>保存开始时间。</summary>
        public DateTime SaveStartTime { get; set; }

        /// <summary>保存类型。</summary>
        public string SaveType { get; set; }

        /// <summary>玩家 Id。</summary>
        public string PlayerId { get; set; }

        /// <summary>保存前快照。</summary>
        public object GameStateBefore { get; set; }
    }

    /// <summary>
    /// 加载同步前置状态。
    /// </summary>
    public class LoadSyncState
    {
        /// <summary>加载开始时间。</summary>
        public DateTime LoadStartTime { get; set; }

        /// <summary>加载类型。</summary>
        public string LoadType { get; set; }

        /// <summary>玩家 Id。</summary>
        public string PlayerId { get; set; }

        /// <summary>加载前快照。</summary>
        public object GameStateBefore { get; set; }

        /// <summary>存档槽标识。</summary>
        public string SaveSlot { get; set; }
    }

    /// <summary>
    /// 缓存用的游戏状态快照（按玩家保存最后一次快照）。
    /// </summary>
    private class GameStateSnapshot
    {
        /// <summary>玩家 Id。</summary>
        public string PlayerId { get; set; }

        /// <summary>保存类型。</summary>
        public string SaveType { get; set; }

        /// <summary>时间戳。</summary>
        public long Timestamp { get; set; }

        /// <summary>快照数据。</summary>
        public object GameState { get; set; }
    }

    #endregion
}
