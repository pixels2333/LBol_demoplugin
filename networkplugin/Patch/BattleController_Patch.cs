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
/// 战斗控制器补丁类
/// 使用Harmony框架拦截和修改LBoL游戏的核心战斗逻辑
/// 实现战斗状态的网络同步，包括伤害、状态效果、治疗等关键战斗事件
/// </summary>
/// <remarks>
/// 这个类包含多个Harmony后置补丁，用于在游戏战斗系统执行关键操作后
/// 将相关的状态变化同步到网络中的其他玩家，确保多人游戏的一致性
/// </remarks>
[HarmonyPatch]
public class BattleController_Patch
{
    /// <summary>
    /// 服务提供者实例，用于获取依赖注入的服务
    /// 主要用于获取网络客户端实例进行状态同步
    /// </summary>
    private static IServiceProvider serviceProvider = ModService.ServiceProvider;

    /// <summary>
    /// 获取网络客户端实例的便捷属性
    /// 用于发送网络同步请求到服务器
    /// </summary>
    private static INetworkClient networkClient => serviceProvider?.GetRequiredService<INetworkClient>();

    /// <summary>
    /// 伤害同步补丁方法
    /// 在BattleController.Damage方法执行完成后被调用
    /// 将本地玩家的伤害结果同步到网络中的其他玩家
    /// </summary>
    /// <param name="__instance">被补丁的BattleController实例（Harmony自动注入）</param>
    /// <param name="damageinfo">包含伤害详细信息的DamageInfo结构体</param>
    /// <param name="target">受到伤害的目标单位</param>
    /// <remarks>
    /// 重要说明：此补丁只同步本地玩家的伤害状态，避免同步所有玩家状态造成冲突
    ///
    /// 同步的伤害信息包括：
    /// - 总伤害值（包括溢出伤害）
    /// - 实际HP伤害
    /// - 被格挡的伤害
    /// - 被护盾吸收的伤害
    /// - 伤害类型和其他属性
    /// - 目标受伤后的完整状态
    /// </remarks>
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

            // 服务验证 - 检查依赖注入服务是否已初始化
            if (serviceProvider == null)
            {
                Plugin.Logger?.LogDebug("[DamageSync] ServiceProvider not initialized");
                return;
            }

            // 获取网络客户端实例并验证连接状态
            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                Plugin.Logger?.LogDebug("[DamageSync] Network client not available");
                return;
            }

            // 只同步玩家单位的伤害 - 使用模式匹配减少嵌套层级
            if (target is not PlayerUnit playerTarget)
            {
                return; // 非玩家单位（如敌人）不进行网络同步
            }

            // 确保是本地玩家 - 避免同步其他玩家的状态
            var battle = __instance;
            if (battle.Player == null || battle.Player != playerTarget)
            {
                return; // 不是本地控制的玩家，跳过同步
            }

            // 构建详细的伤害同步数据 - 包含完整的伤害信息和目标状态
            var damageData = new
            {
                TotalDamage = damageinfo.Amount,         // 总伤害值（包含所有修正）
                HpDamage = damageinfo.Damage,            // 实际HP伤害
                BlockedDamage = damageinfo.DamageBlocked, // 被格挡的伤害
                ShieldedDamage = damageinfo.DamageShielded, // 被护盾吸收的伤害
                damageinfo.DamageType,                   // 伤害类型
                damageinfo.IsGrazed,                     // 是否被擦过（部分伤害）
                damageinfo.IsAccuracy,                   // 是否命中判定
                damageinfo.OverDamage,                   // 溢出伤害
                TargetState = new                        // 目标受伤后的完整状态
                {
                    playerTarget.Id,                     // 玩家唯一标识
                    playerTarget.Hp,                     // 当前HP
                    playerTarget.MaxHp,                  // 最大HP
                    playerTarget.Block,                  // 当前格挡值
                    playerTarget.Shield,                 // 当前护盾值
                    Status = playerTarget.Status.ToString(), // 状态效果摘要
                    playerTarget.IsAlive                 // 存活状态
                },
                Timestamp = DateTime.Now.Ticks          // 事件时间戳
            };

            // 序列化伤害数据并发送到网络服务器
            var json = JsonSerializer.Serialize(damageData);
            networkClient.SendRequest("OnPlayerDamage", json);

            // 记录详细的伤害同步日志 - 便于调试和问题追踪
            Plugin.Logger?.LogInfo(
                $"[DamageSync] Player took {damageinfo.Amount:F1} damage (HP: {damageinfo.Damage:F1}, " +
                $"Block: {damageinfo.DamageBlocked:F1}, Shield: {damageinfo.DamageShielded:F1}). " +
                $"Remaining HP: {playerTarget.Hp}/{playerTarget.MaxHp}");
        }
        catch (Exception ex)
        {
            // 捕获并记录异常，防止补丁错误影响游戏正常运行
            Plugin.Logger?.LogError($"[DamageSync] Error in Damage_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 状态效果添加同步补丁方法
    /// 在BattleController.TryAddStatusEffect方法执行完成后被调用
    /// 将角色添加状态效果后的完整状态列表同步到网络
    /// </summary>
    /// <param name="__instance">被补丁的BattleController实例（Harmony自动注入）</param>
    /// <param name="target">添加状态效果的目标单位</param>
    /// <remarks>
    /// 此补丁使用Harmony的Traverse工具访问私有的_statusEffects字段
    /// 获取角色的完整状态效果列表并同步到服务器
    /// </remarks>
    [HarmonyPatch(typeof(BattleController), "TryAddStatusEffect")]
    [HarmonyPostfix]
    public static void TryAddStatusEffect_Postfix(BattleController __instance, Unit target)
    {
        try
        {
            // 向服务器上传玩家添加状态效果后的完整状态信息

            // 验证服务提供者是否已初始化
            if (serviceProvider == null)
            {
                Plugin.Logger?.LogDebug("[StatusSync] ServiceProvider not initialized for TryAddStatusEffect");
                return;
            }

            // 使用Harmony的Traverse工具访问私有的_statusEffects字段
            // 这里需要访问私有字段来获取完整的状态效果列表
            var _statusEffects = Traverse.Create(target)
                                       .Field("_statusEffects")?
                                       .GetValue<OrderedList<StatusEffect>>();

            // 将所有StatusEffect对象转换为字符串形式
            // 使用ToString()方法确保序列化兼容性
            var _statusEffectList = new List<string>();
            foreach (var se in _statusEffects)
            {
                _statusEffectList.Add(se.ToString());
            }

            // 构建状态效果同步数据
            var json = JsonSerializer.Serialize(new
            {
                statusEffects = _statusEffectList,
                TargetId = target?.Id, // 添加目标ID用于服务器端识别
                Timestamp = DateTime.Now.Ticks
            });

            // TODO: 请求应该添加用户ID - 需要在后续版本中完善
            networkClient.SendRequest("UpdateAfterTryAddStatusEffects", json);

            Plugin.Logger?.LogInfo($"[StatusSync] Status effects updated after addition for unit {target?.Id}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[StatusSync] Error in TryAddStatusEffect_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 状态效果移除同步补丁方法
    /// 在BattleController.RemoveStatusEffect方法执行完成后被调用
    /// 将角色移除状态效果后的完整状态列表同步到网络
    /// </summary>
    /// <param name="__instance">被补丁的BattleController实例（Harmony自动注入）</param>
    /// <param name="target">移除状态效果的目标单位</param>
    /// <remarks>
    /// 与添加状态效果的补丁逻辑相同，获取移除后的完整状态效果列表进行同步
    /// </remarks>
    [HarmonyPatch(typeof(BattleController), "RemoveStatusEffect")]
    [HarmonyPostfix]
    public static void RemoveStatusEffect_Postfix(BattleController __instance, Unit target)
    {
        try
        {
            // 向服务器上传玩家移除状态效果后的完整状态信息

            // 验证服务提供者是否已初始化
            if (serviceProvider == null)
            {
                Plugin.Logger?.LogDebug("[StatusSync] ServiceProvider not initialized for RemoveStatusEffect");
                return;
            }

            // 使用Harmony的Traverse工具访问私有的_statusEffects字段
            var _statusEffects = Traverse.Create(target)
                                       .Field("_statusEffects")?
                                       .GetValue<OrderedList<StatusEffect>>();

            // 将所有StatusEffect对象转换为字符串形式
            var _statusEffectList = new List<string>();
            foreach (var se in _statusEffects)
            {
                _statusEffectList.Add(se.ToString());
            }

            // 构建状态效果同步数据
            var json = JsonSerializer.Serialize(new
            {
                statusEffects = _statusEffectList,
                TargetId = target?.Id, // 添加目标ID用于服务器端识别
                Timestamp = DateTime.Now.Ticks
            });

            // TODO: 请求应该添加用户ID - 需要在后续版本中完善
            networkClient.SendRequest("UpdateAfterTryRemoveStatusEffects", json);

            Plugin.Logger?.LogInfo($"[StatusSync] Status effects updated after removal for unit {target?.Id}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[StatusSync] Error in RemoveStatusEffect_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 治疗同步补丁方法
    /// 在BattleController.Heal方法执行完成后被调用
    /// 将角色治疗后的生命值、护盾、屏障等信息同步到网络
    /// </summary>
    /// <param name="__instance">被补丁的BattleController实例（Harmony自动注入）</param>
    /// <param name="target">接受治疗的目标单位</param>
    /// <remarks>
    /// 同步治疗后的完整生命状态，包括HP、格挡值、护盾值和状态效果
    /// 确保网络中所有玩家看到的治疗结果一致
    /// </remarks>
    [HarmonyPatch(typeof(BattleController), "Heal")]
    [HarmonyPostfix]
    public static void Heal_Postfix(BattleController __instance, Unit target)
    {
        try
        {
            // 向服务器上传玩家治疗后的完整血量信息

            // 验证服务提供者是否已初始化
            if (serviceProvider == null)
            {
                Plugin.Logger?.LogDebug("[HealSync] ServiceProvider not initialized");
                return;
            }

            // 验证网络客户端是否可用
            if (networkClient == null)
            {
                Plugin.Logger?.LogDebug("[HealSync] Network client not available");
                return;
            }

            // 构建治疗后的状态同步数据
            var json = JsonSerializer.Serialize(new
            {
                Hp = target.Hp.ToString(),           // 当前HP
                Block = target.Block.ToString(),     // 当前格挡值
                Shield = target.Shield.ToString(),   // 当前护盾值
                Status = target.Status.ToString(),   // 状态效果摘要
                TargetId = target?.Id,               // 目标ID
                MaxHp = target.MaxHp.ToString(),     // 最大HP
                IsAlive = target.IsAlive,            // 存活状态
                Timestamp = DateTime.Now.Ticks       // 治疗时间戳
            });

            // TODO: 请求应该添加用户ID - 需要在后续版本中完善
            networkClient.SendRequest("UpdateHealthAfterHeal", json);

            Plugin.Logger?.LogInfo($"[HealSync] Health updated after heal for unit {target?.Id}: " +
                                  $"HP {target.Hp}/{target.MaxHp}, Block {target.Block}, Shield {target.Shield}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[HealSync] Error in Heal_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }
}