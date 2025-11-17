using System;
using HarmonyLib;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.EnemyUnits;

[HarmonyPatch]
public class EnemyUnitPatches
{
    private static IServiceProvider serviceProvider = ModService.ServiceProvider;
    private static INetworkManager networkManager => serviceProvider?.GetRequiredService<INetworkManager>();

    //TODO:人员变动发送请求后,客户端接收服务器响应后调整属性
    [HarmonyPatch(typeof(EnemyUnit), "SetMaxHpInBattle")]
    [HarmonyPostfix]
    public static bool SetMaxHpInBattle(EnemyUnit _instance, int hp, int maxHp)
    {
        //TODO:改为从config读取

        //调整系数
        int hpCoefficient = networkManager.GetPlayerCount();

        int modifiedHp = hp * hpCoefficient;
        int modifiedMaxHp = maxHp * hpCoefficient;
        // 关键点：使用 Traverse 调用基类的 SetMaxHp 方法
        // 1. 获取 MyDerivedClass 的基类类型 (MyBaseClass)
        // Type baseType = typeof(EnemyUnit).BaseType;
        Type baseType = typeof(Unit);

        // 2. 使用 Traverse.CreateWithType(baseType.FullName) 来指定在哪个类型上查找方法
        //    然后使用 .Method() 查找并准备调用该方法
        //    最后使用 .GetValue(__instance) 来在 __instance 对象上执行这个基类的方法
        Traverse.CreateWithType(baseType.FullName)
                .Method("SetMaxHp", modifiedHp, modifiedMaxHp) // 指定方法名和要传递的参数
                .GetValue(_instance); // 指定方法执行的实例
        return false; // 跳过原始方法
    }
}
