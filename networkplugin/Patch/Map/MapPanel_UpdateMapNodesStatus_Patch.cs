using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.NetworkPlayer;

namespace NetworkPlugin.Patch.Map;


[HarmonyPatch]
/// <summary>
/// Harmony 补丁类，拦截 MapPanel.UpdateMapNodesStatus 方法，在地图节点状态更新后将当前玩家所在节点位置同步至服务器。
/// </summary>
public class MapPanel_UpdateMapNodesStatus_Patch()
{
    // 这里可以添加你的补丁逻辑
    // 例如，使用 HarmonyPatch 特性来指定要补丁的方法
    [HarmonyPatch(typeof(MapPanel), "UpdateMapNodesStatus")]
    [HarmonyPostfix]
    /// <summary>
    /// MapPanel.UpdateMapNodesStatus 后缀补丁，读取当前地图的访问节点并调用网络管理器更新本地玩家位置。
    /// </summary>
    /// <param name="__instance">被补丁的 MapPanel 实例。</param>
    public static void Postfix(MapPanel __instance)
    {
        var serviceProvider = ModService.ServiceProvider;
        if (serviceProvider == null)
        {
            // 在这里可以添加日志或错误处理，以防服务未被正确初始化
            return;
        }
        var NetWorkManager = serviceProvider.GetRequiredService<INetworkManager>();
        MapNode visitingNode = Traverse.Create(__instance).Field("_map").GetValue<GameMap>().VisitingNode;
        NetWorkManager.GetSelf().UpdateLocation(visitingNode);
        Console.WriteLine("MapPanel_UpdateMapNodesStatus_Patch executed.");
    }
}
