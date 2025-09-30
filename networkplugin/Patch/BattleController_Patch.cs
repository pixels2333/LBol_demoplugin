using System;
using HarmonyLib;
using LBoL.Core.Battle;

namespace NetworkPlugin.Patch;

[HarmonyPatch]
public class BattleController_Patch
{
    //TODO:可能需要在伤害结算前添加给予伤害的action
    [HarmonyPatch(typeof(BattleController), "Damage")]
    [HarmonyPostfix]
    public static void Postfix(BattleController __instance, int damage)
    {
        //向服务器上传player的血量信息
    }
}
