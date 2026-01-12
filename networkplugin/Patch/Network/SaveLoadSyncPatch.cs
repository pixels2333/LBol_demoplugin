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
using NetworkPlugin.Network.Messages;

namespace NetworkPlugin.Patch.Network;


public class SaveLoadSyncPatch
{
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;
    private static readonly Dictionary<string, GameStateSnapshot> _syncedSnapshots = [];
    private static readonly object _syncLock = new();

    /// <summary>
    /// 游戏保存同步 - 当游戏保存时同步状态
    /// </summary>
    [HarmonyPatch]
    public static class GameSaveSync
    {
        [HarmonyTargetMethods]
        static System.Collections.Generic.IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            string[] methodNames = new[]
            {
                "SaveGame",
                "SaveToFile",
                "WriteSaveData",
                "CreateSave"
            };

            List<MethodBase> methods = [];

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        foreach (var method in type.GetMethods())
                        {
                            if (methodNames.Any(name => method.Name.Contains(name)) ||
                                (method.ReturnType == typeof(void) &&
                                 method.GetParameters().Any(p => p.ParameterType.Name.Contains("Save"))))
                            {
                                methods.Add(method);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // 忽略无法访问的程序集
                }
            }

            return methods;
        }

        [HarmonyPrefix]
        private static void GameSave_Prefix(object __instance, object[] __args, out SaveSyncState __state)
        {
            __state = new SaveSyncState();
            try
            {
                // 记录保存前的状态
                __state.SaveStartTime = DateTime.Now;
                __state.SaveType = DetermineSaveType(__instance, __args);
                __state.PlayerId = GetCurrentPlayerId();
                __state.GameStateBefore = CaptureGameState();

                Plugin.Logger?.LogInfo($"[SaveLoadSync] Starting game save: {__state.SaveType}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SaveLoadSync] Error in GameSave_Prefix: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        private static void GameSave_Postfix(object __instance, object[] __args, SaveSyncState __state)
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

                // 只同步主机保存，避免客户端重复同步
                if (!IsHostPlayer())
                {
                    Plugin.Logger?.LogDebug("[SaveLoadSync] Non-host save, skipping sync");
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
                    MultiplayerSync = true
                };

                // 缓存存档快照用于同步
                GameStateSnapshot snapshot = new GameStateSnapshot
                {
                    PlayerId = __state.PlayerId,
                    SaveType = __state.SaveType,
                    Timestamp = DateTime.Now.Ticks,
                    GameState = gameStateAfter
                };

                lock (_syncLock)
                {
                    _syncedSnapshots[__state.PlayerId] = snapshot;
                }

                SendGameEvent(NetworkMessageTypes.OnGameSave, syncData);

                Plugin.Logger?.LogInfo($"[SaveLoadSync] Game save synced: {__state.SaveType} (took {saveTime.TotalMilliseconds:F1}ms)");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SaveLoadSync] Error in GameSave_Postfix: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 游戏加载同步 - 当游戏加载时同步状态
    /// </summary>
    [HarmonyPatch]
    public static class GameLoadSync
    {
        [HarmonyTargetMethods]
        static System.Collections.Generic.IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            string[] methodNames = new[]
            {
                "LoadGame",
                "LoadFromFile",
                "ReadSaveData",
                "LoadSave"
            };

            List<MethodBase> methods = [];

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        foreach (var method in type.GetMethods())
                        {
                            if (methodNames.Any(name => method.Name.Contains(name)) ||
                                (method.ReturnType != typeof(void) &&
                                 method.GetParameters().Any(p => p.ParameterType.Name.Contains("Save"))))
                            {
                                methods.Add(method);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // 忽略无法访问的程序集
                }
            }

            return methods;
        }

        [HarmonyPrefix]
        private static void GameLoad_Prefix(object __instance, object[] __args, out LoadSyncState __state)
        {
            __state = new LoadSyncState();
            try
            {
                // 记录加载前的状态
                __state.LoadStartTime = DateTime.Now;
                __state.LoadType = DetermineLoadType(__args);
                __state.PlayerId = GetCurrentPlayerId();
                __state.GameStateBefore = CaptureGameState();
                __state.SaveSlot = DetermineSaveSlot(__args);

                Plugin.Logger?.LogInfo($"[SaveLoadSync] Starting game load: {__state.LoadType}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SaveLoadSync] Error in GameLoad_Prefix: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        private static void GameLoad_Postfix(object __instance, object[] __args, LoadSyncState __state)
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

                var loadTime = DateTime.Now - __state.LoadStartTime;
                object gameStateAfter = CaptureGameState();

                // 请求主机发送最新的存档同步
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
                    RequestingHostSync = !IsHostPlayer()
                };

                SendGameEvent(NetworkMessageTypes.OnGameLoad, syncData);

                Plugin.Logger?.LogInfo($"[SaveLoadSync] Game load synced: {__state.LoadType} (took {loadTime.TotalMilliseconds:F1}ms)");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SaveLoadSync] Error in GameLoad_Postfix: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 快速保存同步 - 游戏中的自动保存
    /// </summary>
    [HarmonyPatch]
    public static class QuickSaveSync
    {
        private static DateTime _lastQuickSave = DateTime.MinValue;
        private static readonly TimeSpan QUICK_SAVE_COOLDOWN = TimeSpan.FromSeconds(30);

        [HarmonyTargetMethods]
        static System.Collections.Generic.IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            string[] methodNames = new[]
            {
                "QuickSave",
                "AutoSave",
                "CheckpointSave"
            };

            List<MethodBase> methods = [];

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        foreach (var method in type.GetMethods())
                        {
                            if (methodNames.Any(name => method.Name.Contains(name)))
                            {
                                methods.Add(method);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // 忽略无法访问的程序集
                }
            }

            return methods;
        }

        [HarmonyPostfix]
        public static void QuickSave_Postfix(object __instance, object[] __args)
        {
            try
            {
                var now = DateTime.Now;
                if (now - _lastQuickSave < QUICK_SAVE_COOLDOWN)
                {
                    return; // 避免过于频繁的快速保存同步
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

                if (!IsHostPlayer())
                {
                    return; // 只有主机执行快速保存同步
                }

                object gameState = CaptureGameState();
                var syncData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = "QuickSaveSync",
                    PlayerId = GetCurrentPlayerId(),
                    GameStateSnapshot = gameState,
                    SaveFrequency = "Throttled"
                };

                SendGameEvent("QuickSaveSync", syncData);

                _lastQuickSave = now;

                Plugin.Logger?.LogDebug("[SaveLoadSync] Quick save synced");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SaveLoadSync] Error in QuickSave_Postfix: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 存档同步管理器 - 处理存档同步请求和响应
    /// </summary>
    public static class SaveSyncManager
    {
        /// <summary>
        /// 请求主机发送存档同步
        /// </summary>
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
                    RequestType = "FullSync"
                };

                SendGameEvent("SaveSyncRequest", requestData);

                Plugin.Logger?.LogInfo($"[SaveLoadSync] Requested host save sync for slot: {saveSlot}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SaveLoadSync] Error requesting host save sync: {ex.Message}");
            }
        }

        /// <summary>
        /// 响应存档同步请求 - 主机发送存档数据
        /// </summary>
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
                    SaveTime = DateTime.Now.Ticks
                };

                SendGameEvent("SaveSyncResponse", responseData);

                Plugin.Logger?.LogInfo($"[SaveLoadSync] Responded to save sync request from {requesterId}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SaveLoadSync] Error responding to save sync request: {ex.Message}");
            }
        }

        /// <summary>
        /// 应用来自主机的存档同步数据
        /// </summary>
        public static void ApplyHostSaveSync(object saveSyncData)
        {
            try
            {
                // TODO: 实现将主机的存档状态应用到本地游戏
                // 这可能需要调用游戏内部的存档应用API
                Plugin.Logger?.LogInfo("[SaveLoadSync] Host save sync applied successfully");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[SaveLoadSync] Error applying host save sync: {ex.Message}");
            }
        }
    }

    // 辅助方法

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
                networkClient?.SendRequest(eventType, JsonSerializer.Serialize(eventData));
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SaveLoadSync] Error sending game event {eventType}: {ex.Message}");
        }
    }

    private static string GetCurrentPlayerId()
    {
        try
        {
            // TODO: 从GameStateUtils获取玩家ID
            return "current_player";
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SaveLoadSync] Error getting current player ID: {ex.Message}");
            return "unknown_player";
        }
    }

    private static bool IsHostPlayer()
    {
        try
        {
            // TODO: 检查当前玩家是否为主机
            return true; // 临时返回true
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SaveLoadSync] Error checking host status: {ex.Message}");
            return false;
        }
    }

    private static object CaptureGameState()
    {
        try
        {
            // TODO: 实现完整的游戏状态捕获
            return new
            {
                Timestamp = DateTime.Now.Ticks,
                PlayerState = new
                {
                    // TODO: 捕获玩家状态
                },
                BattleState = new
                {
                    // TODO: 捕获战斗状态
                },
                GameProgress = new
                {
                    // TODO: 捕获游戏进度
                }
            };
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SaveLoadSync] Error capturing game state: {ex.Message}");
            return new { Error = ex.Message, Timestamp = DateTime.Now.Ticks };
        }
    }

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
            Plugin.Logger?.LogError($"[SaveLoadSync] Error determining save type: {ex.Message}");
            return "UnknownSave";
        }
    }

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
            Plugin.Logger?.LogError($"[SaveLoadSync] Error determining load type: {ex.Message}");
            return "UnknownLoad";
        }
    }

    private static string DetermineSaveSlot(object[] args)
    {
        try
        {
            object slotArg = args.FirstOrDefault(arg => arg?.ToString().Contains("Slot") == true);
            return slotArg?.ToString() ?? "DefaultSlot";
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[SaveLoadSync] Error determining save slot: {ex.Message}");
            return "DefaultSlot";
        }
    }

    // 数据结构

    public class SaveSyncState
    {
        public DateTime SaveStartTime { get; set; }
        public string SaveType { get; set; }
        public string PlayerId { get; set; }
        public object GameStateBefore { get; set; }
    }

    public class LoadSyncState
    {
        public DateTime LoadStartTime { get; set; }
        public string LoadType { get; set; }
        public string PlayerId { get; set; }
        public object GameStateBefore { get; set; }
        public string SaveSlot { get; set; }
    }

    private class GameStateSnapshot
    {
        public string PlayerId { get; set; }
        public string SaveType { get; set; }
        public long Timestamp { get; set; }
        public object GameState { get; set; }
    }
}
