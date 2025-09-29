using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.NetworkPlayer;

namespace NetworkPlugin.Patch;


[HarmonyPatch]
public class MapPanel_UpdateMapNodesStatus_Patch()
{
    // 这里可以添加你的补丁逻辑
    // 例如，使用 HarmonyPatch 特性来指定要补丁的方法
    [HarmonyPatch(typeof(MapPanel), "UpdateMapNodesStatus")]
    [HarmonyPostfix]
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

