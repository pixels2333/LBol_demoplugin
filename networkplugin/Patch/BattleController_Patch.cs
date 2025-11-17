using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using Mono.Cecil;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch;

[HarmonyPatch]
public class BattleController_Patch
{

    private static IServiceProvider serviceProvider = ModService.ServiceProvider;
    private static INetworkClient networkClient => serviceProvider?.GetRequiredService<INetworkClient>();
    //TODO:可能需要在伤害结算前添加给予伤害的action
    [HarmonyPatch(typeof(BattleController), "Damage")]
    [HarmonyPostfix]
    public static void Damage_Postfix(BattleController __instance, Unit target)
    {
        //向服务器上传player的血量信息

        // if (serviceProvider == null)
        // {
        //     // 在这里可以添加日志或错误处理，以防服务未被正确初始化
        //     return;
        // }
        // if (networkClient == null)
        // {
        //     // 在这里可以添加日志或错误处理，以防网络客户端未被正确初始化
        //     return;
        // }
        // var json = JsonSerializer.Serialize(new
        // {
        //     Hp = target.Hp.ToString(),
        //     Block = target.Block.ToString(),
        //     Shield = target.Shield.ToString(),
        //     Status = target.Status.ToString()
        // });
        // //TODO:请求应该添加用户id
        // networkClient.SendRequest("UpdateHealthAfterDamage", json);

    }

    [HarmonyPatch(typeof(BattleController), "TryAddStatusEffect")]
    [HarmonyPostfix]
    public static void TryAddStatusEffect_Postfix(BattleController __instance, Unit target)
    {
        // 向服务器上传player的状态信息

        if (serviceProvider == null)
        {
            // 在这里可以添加日志或错误处理，以防服务未被正确初始化
            return;
        }
        var _statusEffects = Traverse.Create(target)
                                   .Field("_statusEffects")?
                                   .GetValue<OrderedList<StatusEffect>>()
                                   ;
        // 将所有StatusEffect对象转为字符串（可根据实际需求选择ToString或Name等属性）
        var _statusEffectList = new List<string>();
        foreach (var se in _statusEffects)
        {
            _statusEffectList.Add(se.ToString());
        }
        // var networkClient = serviceProvider.GetRequiredService<INetworkClient>();
        var json = JsonSerializer.Serialize(new
        {
            statusEffects = _statusEffectList
        });

        //TODO:请求应该添加用户id
        networkClient.SendRequest("UpdateAfterTryAddStatusEffects", json);

    }

    [HarmonyPatch(typeof(BattleController), "RemoveStatusEffect")]
    [HarmonyPostfix]
    public static void RemoveStatusEffect_Postfix(BattleController __instance, Unit target)
    {
        //向服务器上传player的状态信息

        if (serviceProvider == null)
        {
            // 在这里可以添加日志或错误处理，以防服务未被正确初始化
            return;
        }
        var _statusEffects = Traverse.Create(target)
                                   .Field("_statusEffects")?
                                   .GetValue<OrderedList<StatusEffect>>()
                                   ;
        // 将所有StatusEffect对象转为字符串（可根据实际需求选择ToString或Name等属性）
        var _statusEffectList = new List<string>();
        foreach (var se in _statusEffects)
        {
            _statusEffectList.Add(se.ToString());
        }
        // var networkClient = serviceProvider.GetRequiredService<INetworkClient>();
        var json = JsonSerializer.Serialize(new
        {
            statusEffects = _statusEffectList
        });
        //TODO:请求应该添加用户id
        networkClient.SendRequest("UpdateAfterTryRemoveStatusEffects", json);
    }

    



}
