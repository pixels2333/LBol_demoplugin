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
    /// <summary>
    /// 伤害同步补丁 - 在伤害应用后同步本地玩家的状态
    /// 重要: 只同步本地玩家的状态变化，避免同步所有玩家造成冲突
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "Damage")]
    [HarmonyPostfix]
    public static void Damage_Postfix(BattleController __instance, DamageInfo damageinfo, Unit target)
    {
        try
        {
            // 参数验证 - DamageInfo是struct，不能直接与null比较
            if (target == null || __instance == null)
            {
                Plugin.Logger?.LogWarning("[DamageSync] Damage_Postfix received null parameter");
                return;
            }

            // 服务验证
            if (serviceProvider == null)
            {
                Plugin.Logger?.LogDebug("[DamageSync] ServiceProvider not initialized");
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                Plugin.Logger?.LogDebug("[DamageSync] Network client not available");
                return;
            }

            // 只同步玩家单位的伤害 - 使用模式匹配减少嵌套
            if (target is not PlayerUnit playerTarget)
            {
                return;
            }

            // 确保是本地玩家
            var battle = __instance;
            if (battle.Player == null || battle.Player != playerTarget)
            {
                return;
            }

            // 构建伤害信息 - 直接访问damageinfo属性
            var damageData = new
            {
                TotalDamage = damageinfo.Amount, // 总伤害值
                HpDamage = damageinfo.Damage,    // 实际HP伤害
                BlockedDamage = damageinfo.DamageBlocked, // 被格挡
                ShieldedDamage = damageinfo.DamageShielded, // 被护盾吸收
                damageinfo.DamageType, // 简化成员名称
                damageinfo.IsGrazed,
                damageinfo.IsAccuracy,
                damageinfo.OverDamage,
                TargetState = new
                {
                    playerTarget.Id,
                    playerTarget.Hp,
                    playerTarget.MaxHp,
                    playerTarget.Block,
                    playerTarget.Shield,
                    Status = playerTarget.Status.ToString(),
                    playerTarget.IsAlive
                },
                Timestamp = DateTime.Now.Ticks
            };

            var json = JsonSerializer.Serialize(damageData);
            networkClient.SendRequest("OnPlayerDamage", json);

            Plugin.Logger?.LogInfo($"[DamageSync] Player took {damageinfo.Amount:F1} damage (HP: {damageinfo.Damage:F1}, Block: {damageinfo.DamageBlocked:F1}, Shield: {damageinfo.DamageShielded:F1}). Remaining HP: {playerTarget.Hp}/{playerTarget.MaxHp}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DamageSync] Error in Damage_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
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

    [HarmonyPatch(typeof(BattleController), "Heal")]
    [HarmonyPostfix]
    public static void Heal_Postfix(BattleController __instance, Unit target)
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
            Hp = target.Hp.ToString(),
            Block = target.Block.ToString(),
            Shield = target.Shield.ToString(),
            Status = target.Status.ToString()
        });
        //TODO:请求应该添加用户id
        networkClient.SendRequest("UpdateHealthAfterHeal", json);
    }
}
