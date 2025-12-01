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

/// <summary>
/// 战斗控制器网络同步补丁
/// 通过Harmony框架修改BattleController类的行为，实现战斗状态的网络同步
/// 确保多玩家游戏中的战斗状态一致性，包括伤害、治疗和状态效果同步
/// </summary>
[HarmonyPatch]
public class BattleController_Patch
{
    /// <summary>
    /// 服务提供者访问器，用于获取依赖注入的组件
    /// 通过模块服务获取网络客户端和其他服务
    /// </summary>
    private static IServiceProvider serviceProvider = ModService.ServiceProvider;
    
    /// <summary>
    /// 网络客户端属性，用于发送网络请求
    /// 采用延迟加载模式，避免初始化时的依赖问题
    /// </summary>
    private static INetworkClient networkClient => serviceProvider?.GetRequiredService<INetworkClient>();

    /// <summary>
    /// 伤害同步补丁 - 在伤害应用后同步本地玩家的状态
    /// 重要: 只同步本地玩家的状态变化，避免同步所有玩家造成冲突
    /// 确保伤害计算在网络环境中的一致性，防止作弊和状态不同步
    /// </summary>
    /// <param name="__instance">BattleController实例，通过Harmony自动注入</param>
    /// <param name="damageinfo">伤害信息对象，包含伤害数值和类型</param>
    /// <param name="target">受到伤害的目标单位</param>
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

            // 服务验证 - 确保依赖注入服务已正确初始化
            if (serviceProvider == null)
            {
                Plugin.Logger?.LogDebug("[DamageSync] ServiceProvider not initialized");
                return;
            }

            // 网络客户端验证 - 检查网络连接状态
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

            // 确保是本地玩家 - 避免同步其他玩家的状态造成冲突
            var battle = __instance;
            if (battle.Player == null || battle.Player != playerTarget)
            {
                return;
            }

            // 构建伤害信息 - 直接访问damageinfo属性
            var damageData = new
            {
                TotalDamage = damageinfo.Amount, // 总伤害值，包含所有类型的伤害
                HpDamage = damageinfo.Damage,    // 实际HP伤害，对玩家血量有直接影响
                BlockedDamage = damageinfo.DamageBlocked, // 被格挡的伤害量
                ShieldedDamage = damageinfo.DamageShielded, // 被护盾吸收的伤害量
                damageinfo.DamageType, // 伤害类型（物理、魔法等）
                damageinfo.IsGrazed,    // 是否是擦伤伤害
                damageinfo.IsAccuracy,  // 是否是精准命中
                damageinfo.OverDamage,  // 溢出伤害量
                TargetState = new       // 目标单位当前状态
                {
                    playerTarget.Id,      // 玩家单位唯一标识符
                    playerTarget.Hp,      // 当前生命值
                    playerTarget.MaxHp,   // 最大生命值
                    playerTarget.Block,   // 当前格挡值
                    playerTarget.Shield,  // 当前护盾值
                    Status = playerTarget.Status.ToString(), // 状态枚举值
                    playerTarget.IsAlive  // 是否存活状态
                },
                Timestamp = DateTime.Now.Ticks // 时间戳，用于时序同步
            };

            // 序列化伤害数据并发送到服务器
            var json = JsonSerializer.Serialize(damageData);
            networkClient.SendRequest("OnPlayerDamage", json);

            // 记录详细的伤害同步日志，便于调试和监控
            Plugin.Logger?.LogInfo($"[DamageSync] Player took {damageinfo.Amount:F1} damage (HP: {damageinfo.Damage:F1}, Block: {damageinfo.DamageBlocked:F1}, Shield: {damageinfo.DamageShielded:F1}). Remaining HP: {playerTarget.Hp}/{playerTarget.MaxHp}");
        }
        catch (Exception ex)
        {
            // 异常处理，记录错误堆栈信息便于问题排查
            Plugin.Logger?.LogError($"[DamageSync] Error in Damage_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 尝试添加状态效果补丁 - 在添加状态效果后同步状态信息
    /// 实现状态效果的网络同步，确保所有玩家看到一致的状态效果
    /// </summary>
    /// <param name="__instance">BattleController实例</param>
    /// <param name="target">接收状态效果的单位</param>
    [HarmonyPatch(typeof(BattleController), "TryAddStatusEffect")]
    [HarmonyPostfix]
    public static void TryAddStatusEffect_Postfix(BattleController __instance, Unit target)
    {
        // 向服务器上传player的状态信息，确保状态同步

        // 服务依赖检查 - 防止服务未初始化导致的空引用异常
        if (serviceProvider == null)
        {
            // 在这里可以添加日志或错误处理，以防服务未被正确初始化
            return;
        }
        
        // 通过反射获取单位的状态效果列表
        // Traverse.Create提供了安全的反射访问方式
        var _statusEffects = Traverse.Create(target)
                                   .Field("_statusEffects")?
                                   .GetValue<OrderedList<StatusEffect>>();

        // 将所有StatusEffect对象转为字符串列表（可根据实际需求选择ToString或Name等属性）
        var _statusEffectList = new List<string>();
        foreach (var se in _statusEffects)
        {
            _statusEffectList.Add(se.ToString());
        }
        
        // 获取网络客户端实例
        var json = JsonSerializer.Serialize(new
        {
            statusEffects = _statusEffectList
        });

        //TODO: 请求应该添加用户id，用于标识具体的状态变化来源
        networkClient.SendRequest("UpdateAfterTryAddStatusEffects", json);
    }

    /// <summary>
    /// 移除状态效果补丁 - 在移除状态效果后同步状态信息
    /// 保持状态效果列表的一致性，防止状态显示不同步
    /// </summary>
    /// <param name="__instance">BattleController实例</param>
    /// <param name="target">移除状态效果的单位</param>
    [HarmonyPatch(typeof(BattleController), "RemoveStatusEffect")]
    [HarmonyPostfix]
    public static void RemoveStatusEffect_Postfix(BattleController __instance, Unit target)
    {
        // 向服务器上传player的状态信息，确保移除操作在网络中同步

        // 服务依赖检查 - 确保服务正确初始化
        if (serviceProvider == null)
        {
            // 在这里可以添加日志或错误处理，以防服务未被正确初始化
            return;
        }
        
        // 通过反射获取单位当前的状态效果列表
        var _statusEffects = Traverse.Create(target)
                                   .Field("_statusEffects")?
                                   .GetValue<OrderedList<StatusEffect>>();

        // 将所有StatusEffect对象转为字符串列表，用于网络传输
        var _statusEffectList = new List<string>();
        foreach (var se in _statusEffects)
        {
            _statusEffectList.Add(se.ToString());
        }
        
        // 序列化状态效果列表并发送到服务器
        var json = JsonSerializer.Serialize(new
        {
            statusEffects = _statusEffectList
        });
        
        //TODO: 请求应该添加用户id，用于标识具体的移除操作来源
        networkClient.SendRequest("UpdateAfterTryRemoveStatusEffects", json);
    }

    /// <summary>
    /// 治疗补丁 - 在治疗应用后同步生命值信息
    /// 确保治疗操作在所有客户端的一致性，防止血量显示不同步
    /// </summary>
    /// <param name="__instance">BattleController实例</param>
    /// <param name="target">接受治疗的单位</param>
    [HarmonyPatch(typeof(BattleController), "Heal")]
    [HarmonyPostfix]
    public static void Heal_Postfix(BattleController __instance, Unit target)
    {
        // 向服务器上传player的血量信息，确保治疗同步

        // 服务依赖检查 - 防止空引用异常
        if (serviceProvider == null)
        {
            // 在这里可以添加日志或错误处理，以防服务未被正确初始化
            return;
        }
        
        // 网络客户端检查 - 确保网络通信可用
        if (networkClient == null)
        {
            // 在这里可以添加日志或错误处理，以防网络客户端未被正确初始化
            return;
        }
        
        // 构建治疗状态数据，包含生命值、格挡值、护盾值和状态信息
        var json = JsonSerializer.Serialize(new
        {
            Hp = target.Hp.ToString(),      // 当前生命值（转换为字符串确保序列化兼容性）
            Block = target.Block.ToString(), // 当前格挡值
            Shield = target.Shield.ToString(), // 当前护盾值
            Status = target.Status.ToString()  // 单位状态
        });
        
        //TODO: 请求应该添加用户id，用于标识具体的治疗操作来源
        networkClient.SendRequest("UpdateHealthAfterHeal", json);
    }
}
