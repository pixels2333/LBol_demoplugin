using System;
using HarmonyLib;
using LBoL.Core;

namespace NetworkPlugin.Patch;

[HarmonyPatch]
public class GameRunController_Patch
{
    [HarmonyPatch(typeof(GameRunController), "Damage")]
    [HarmonyPostfix]
    public static void Postfix(GameRunController __instance, int damage)
    {
        //TODO:可能需要删除
    }
}
