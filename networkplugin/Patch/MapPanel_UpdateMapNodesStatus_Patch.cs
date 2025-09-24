using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation.UI.Panels;
using NetworkPlugin.Network.Client;

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

        ClientData.SharedMapData.TryAdd(ClientData.username, Traverse.Create(__instance).Field("_map").GetValue<GameMap>()); // 确保 MapName 和 Map 是 MapPanel 的属性
        Console.WriteLine("MapPanel_UpdateMapNodesStatus_Patch executed.");
    }
}

