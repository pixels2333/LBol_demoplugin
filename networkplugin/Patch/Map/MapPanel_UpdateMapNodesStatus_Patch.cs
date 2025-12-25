using System;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.Map;

/// <summary>
/// 地图节点同步补丁 - 同步玩家位置变化
/// </summary>
[HarmonyPatch]
public class MapPanel_UpdateMapNodesStatus_Patch
{
    private static IServiceProvider serviceProvider = ModService.ServiceProvider;

    /// <summary>
    /// 当地图节点状态更新时同步玩家位置
    /// </summary>
    [HarmonyPatch(typeof(MapPanel), "UpdateMapNodesStatus")]
    [HarmonyPostfix]
    public static void Postfix(MapPanel __instance)
    {
        try
        {
            if (serviceProvider == null)
            {
                Plugin.Logger?.LogWarning("[MapSyncPatch] serviceProvider is null");
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
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

            var locationData = new
            {
                LocationX = visitingNode.X,
                LocationY = visitingNode.Y,
                LocationName = visitingNode.Name ?? "",
                LocationType = visitingNode.GetType().Name,
                Stage = gameMap.CurrentStage?.Id ?? "Unknown"
            };

            string json = JsonSerializer.Serialize(locationData);
            networkClient.SendRequest("UpdatePlayerLocation", json);

            Plugin.Logger?.LogInfo($"[MapSyncPatch] Player location updated: ({locationData.LocationX}, {locationData.LocationY}) - {locationData.LocationName}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MapSyncPatch] Error in Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
