using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.MidGameJoin;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Map;

/// <summary>
/// 地图节点同步补丁 - 同步玩家位置变化
/// </summary>
[HarmonyPatch]
public class MapPanelUpdateMapNodesStatusPatch
{
    private static readonly object _locationSendLock = new();
    private static bool _wasConnected;
    private static ulong _lastSentFp;
    private static long _lastSentAtTicks;

    /// <summary>
    /// MapPanel 每帧更新：以小预算推进追赶，避免在 UpdateMapNodesStatus 中集中做大量工作导致卡顿。
    /// </summary>
    [HarmonyPatch(typeof(MapPanel), "Update")]
    [HarmonyPostfix]
    public static void MapPanel_Update_Postfix(MapPanel __instance)
    {
        try
        {
            IServiceProvider sp = ModService.ServiceProvider;
            sp?.GetService<MapCatchUpOrchestrator>()?.TryApplyPendingToCurrentRun(pathStepsBudget: 1, nodeStatesBudget: 10);
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// 当地图节点状态更新时同步玩家位置
    /// </summary>
    [HarmonyPatch(typeof(MapPanel), "UpdateMapNodesStatus")]
    [HarmonyPrefix]
    public static void Prefix(MapPanel __instance)
    {
        try
        {
            // Apply pending catch-up *before* the UI reads node statuses.
            IServiceProvider sp = ModService.ServiceProvider;
            sp?.GetService<MapCatchUpOrchestrator>()?.TryApplyPendingToCurrentRun(pathStepsBudget: 2, nodeStatesBudget: 60);
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// 当地图节点状态更新后同步玩家位置
    /// </summary>
    [HarmonyPatch(typeof(MapPanel), "UpdateMapNodesStatus")]
    [HarmonyPostfix]
    public static void Postfix(MapPanel __instance)
    {
        try
        {
            IServiceProvider sp = ModService.ServiceProvider;
            if (sp == null)
            {
                Plugin.Logger?.LogWarning("[MapSyncPatch] serviceProvider is null");
                return;
            }

            var networkClient = sp.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                Plugin.Logger?.LogDebug("[MapSyncPatch] Network client not available");
                return;
            }

            // 获取GameMap实例
            var gameMapField = Traverse.Create(__instance).Field("_map");
            if (!gameMapField.FieldExists())
            {
                Plugin.Logger?.LogWarning("[MapSyncPatch] _map field not found");
                return;
            }

            var gameMap = gameMapField.GetValue<GameMap>();
            if (gameMap == null)
            {
                Plugin.Logger?.LogWarning("[MapSyncPatch] GameMap is null");
                return;
            }

            var visitingNode = gameMap.VisitingNode;
            if (visitingNode == null)
            {
                Plugin.Logger?.LogWarning("[MapSyncPatch] VisitingNode is null");
                return;
            }

            string characterId = null;
            try
            {
                characterId = GameStateUtils.GetCurrentPlayer()?.ModelName;
            }
            catch
            {
                // ignored
            }

            var locationData = new
            {
                LocationX = visitingNode.X,
                LocationY = visitingNode.Y,
                LocationName = visitingNode.StationType.ToString(),
                LocationType = visitingNode.GetType().Name,
                Stage = visitingNode.Act,
                CharacterId = characterId
            };

            string json = JsonCompat.Serialize(locationData);

            // 去重/限流：UpdateMapNodesStatus 可能在短时间内被频繁调用；
            // 只在位置 payload 变化或超过一定时间后才发送，避免刷屏并减少网络流量。
            bool shouldSend;
            bool isReconnectFirstSend = false;
            long nowTicks = DateTime.UtcNow.Ticks;
            ulong fp = NetLogHelper.ComputeFnv1a64(json);
            lock (_locationSendLock)
            {
                bool connected = networkClient.IsConnected;
                if (connected && !_wasConnected)
                {
                    // 刚刚从断线恢复：允许立刻发送一次当前位置。
                    _lastSentFp = 0;
                    _lastSentAtTicks = 0;
                    isReconnectFirstSend = true;
                }

                _wasConnected = connected;

                long minIntervalTicks = TimeSpan.FromMilliseconds(250).Ticks;
                long refreshIntervalTicks = TimeSpan.FromSeconds(5).Ticks;

                bool samePayload = fp == _lastSentFp;
                bool tooSoon = (nowTicks - _lastSentAtTicks) >= 0 && (nowTicks - _lastSentAtTicks) < minIntervalTicks;
                bool needsRefresh = (nowTicks - _lastSentAtTicks) >= refreshIntervalTicks;

                shouldSend = (!samePayload) || needsRefresh || isReconnectFirstSend;
                if (samePayload && tooSoon)
                {
                    shouldSend = false;
                }

                if (shouldSend)
                {
                    _lastSentFp = fp;
                    _lastSentAtTicks = nowTicks;
                }
            }

            if (!shouldSend)
            {
                return;
            }

            networkClient.SendRequest("UpdatePlayerLocation", json);
            string summary = NetLogHelper.BuildSummary("UpdatePlayerLocation", json);
            Plugin.Logger?.LogInfo($"[MapSyncPatch] 已发送位置同步: ({locationData.LocationX}, {locationData.LocationY}) - {locationData.LocationName} ({summary})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MapSyncPatch] Error in Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 主动上报当前玩家位置（进入新关卡/节点时调用，跳过防抖动限制）
    /// </summary>
    public static void SendLocationUpdateActive(bool force = false)
    {
        try
        {
            if (force)
            {
                lock (_locationSendLock)
                {
                    _lastSentFp = 0;
                    _lastSentAtTicks = 0;
                }
            }

            var gameRun = GameStateUtils.GetCurrentGameRun();
            var visitingNode = gameRun?.CurrentMap?.VisitingNode;
            if (visitingNode == null)
            {
                return;
            }

            IServiceProvider sp = ModService.ServiceProvider;
            var networkClient = sp?.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            string characterId = null;
            try { characterId = GameStateUtils.GetCurrentPlayer()?.ModelName; } catch { }

            var locationData = new
            {
                LocationX = visitingNode.X,
                LocationY = visitingNode.Y,
                LocationName = visitingNode.StationType.ToString(),
                LocationType = visitingNode.GetType().Name,
                Stage = visitingNode.Act,
                CharacterId = characterId
            };

            string json = JsonCompat.Serialize(locationData);
            networkClient.SendRequest("UpdatePlayerLocation", json);
            Plugin.Logger?.LogInfo($"[MapSyncPatch] 主动发送位置更新: ({visitingNode.X}, {visitingNode.Y}) - {visitingNode.StationType}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[MapSyncPatch] SendLocationUpdateActive 失败: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(GameRunController), "EnterStation")]
internal static class GameRunController_EnterStation_ActiveLocationPatch
{
    [HarmonyPostfix]
    public static void Postfix(GameRunController __instance, Station station)
    {
        try
        {
            MapPanelUpdateMapNodesStatusPatch.SendLocationUpdateActive(force: true);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ActiveLocationPatch] EnterStation 后置位置主动上传失败: {ex.Message}");
        }
    }
}
