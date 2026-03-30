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
/// <summary>
/// Harmony 补丁类，拦截 BattleController 的战斗核心方法（伤害、状态效果、治疗），
/// 在每次事件后将最新的玩家战斗数据通过网络客户端同步至服务器。
/// </summary>
public class BattleController_Patch
{

    private static IServiceProvider serviceProvider = ModService.ServiceProvider;
    private static INetworkClient networkClient => serviceProvider?.GetRequiredService<INetworkClient>();
    //TODO:可能需要在伤害结算前添加给予伤害的action
    /// <summary>
    /// BattleController.Damage 后缀补丁，伤害结算后将目标单位的 HP、格挡、护盾等状态序列化为 JSON 上传至服务器。
    /// </summary>
    /// <param name="__instance">BattleController 实例。</param>
    /// <param name="damageinfo">本次伤害信息（包含伤害值和类型）。</param>
    /// <param name="target">受到伤害的目标单位。</param>
    [HarmonyPatch(typeof(BattleController), "Damage")]
    [HarmonyPostfix]
    public static void Damage_Postfix(BattleController __instance, DamageInfo damageinfo, Unit target)
    {
        // 向服务器上传player的血量信息

        if (serviceProvider == null)
        {
            // 在这里可以添加日志或错误处理，以防服务未被正确初始化
            return;
        }
        if (networkClient == null)
        {
            // 在这里可以添加日志或错误处理，以防网络客户端未被正确初始化
            return;
        }
        var json = JsonSerializer.Serialize(new
        {
            //TODO:传输更多的伤害信息
            Damage=damageinfo.Damage.ToString(),
            DamageType = damageinfo.DamageType.ToString(),
            Hp = target.Hp.ToString(),
            Block = target.Block.ToString(),
            Shield = target.Shield.ToString(),
            Status = target.Status.ToString()
        });
        //TODO:请求应该添加用户id
        networkClient.SendRequest("UpdateHealthAfterDamage", json);

    }



    /// <summary>
    /// BattleController.TryAddStatusEffect 后缀补丁，状态效果添加后将目标单位的全部状态效果序列化为 JSON 上传至服务器。
    /// </summary>
    /// <param name="__instance">BattleController 实例。</param>
    /// <param name="target">状态效果被添加的目标单位。</param>
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

    /// <summary>
    /// BattleController.RemoveStatusEffect 后缀补丁，状态效果移除后将目标单位的剩余状态效果序列化为 JSON 上传至服务器。
    /// </summary>
    /// <param name="__instance">BattleController 实例。</param>
    /// <param name="target">状态效果被移除的目标单位。</param>
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

    /// <summary>
    /// BattleController.Heal 后缀补丁，治疗结算后将目标单位的 HP、格挡、护盾状态序列化为 JSON（待完善上传逻辑）。
    /// </summary>
    /// <param name="__instance">BattleController 实例。</param>
    /// <param name="target">接受治疗的目标单位。</param>
    [HarmonyPatch(typeof(BattleController), "Heal")]
    [HarmonyPostfix]
    public static void Heal_Postfix(BattleController __instance, Unit target)
    {
        //向服务器上传player的血量信息

        if (serviceProvider == null)
        {
            // 在这里可以添加日志或错误处理，以防服务未被正确初始化
            return;
        }
        // var networkClient = serviceProvider.GetRequiredService<INetworkClient>();
        var json = JsonSerializer.Serialize(new
        {
            Hp = target.Hp.ToString(),
            Block = target.Block.ToString(),
            Shield = target.Shield.ToString(),
            Status = target.Status.ToString()
        });



    }}
