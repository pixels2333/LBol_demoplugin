using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch;

/// <summary>
/// 战斗控制器相关补丁。
/// 通过 Harmony 拦截 <see cref="BattleController"/> 的关键流程，并将本地玩家的战斗状态变化同步到联机层。
/// </summary>
/// <remarks>
/// 说明：这里的同步以“本地玩家”为中心，避免在同一房间中对其他玩家的状态进行重复广播造成冲突。
/// </remarks>
[HarmonyPatch]
public class BattleController_Patch
{
    #region 依赖注入与客户端

    /// <summary>
    /// 服务提供者（依赖注入入口），用于解析网络客户端等服务。
    /// </summary>
    private static IServiceProvider serviceProvider = ModService.ServiceProvider;

    /// <summary>
    /// 网络客户端（通过依赖注入获取）。
    /// </summary>
    private static INetworkClient networkClient => serviceProvider?.GetRequiredService<INetworkClient>();

    #endregion

    #region 伤害同步

    /// <summary>
    /// 伤害应用完成后同步（仅同步本地玩家）。
    /// </summary>
    /// <param name="__instance">被补丁的 <see cref="BattleController"/> 实例（Harmony 注入）。</param>
    /// <param name="damageinfo">本次伤害信息（结构体）。</param>
    /// <param name="target">伤害目标单位。</param>
    [HarmonyPatch(typeof(BattleController), "Damage")]
    [HarmonyPostfix]
    public static void Damage_Postfix(BattleController __instance, DamageInfo damageinfo, Unit target)
    {
        try
        {
            // 参数校验：DamageInfo 是 struct，不能与 null 比较；这里只检查引用类型参数。
            if (target == null || __instance == null)
            {
                Plugin.Logger?.LogWarning("[DamageSync] Damage_Postfix received null parameter");
                return;
            }

            // 依赖注入服务未就绪时直接跳过。
            if (serviceProvider == null)
            {
                Plugin.Logger?.LogDebug("[DamageSync] ServiceProvider not initialized");
                return;
            }

            // 获取网络客户端并确认联机状态。
            var client = serviceProvider.GetService<INetworkClient>();
            if (client == null || !client.IsConnected)
            {
                Plugin.Logger?.LogDebug("[DamageSync] Network client not available");
                return;
            }

            // 只同步玩家单位的伤害（敌人伤害在其他补丁中处理）。
            if (target is not PlayerUnit playerTarget)
            {
                return;
            }

            // 只同步本地玩家：避免同步其他玩家的状态导致彼此覆盖。
            var battle = __instance;
            if (battle.Player == null || battle.Player != playerTarget)
            {
                return;
            }

            // 构建“伤害 + 目标快照”的同步数据。
            var damageData = new
            {
                TotalDamage = damageinfo.Amount, // 总伤害（含溢出等统计）
                HpDamage = damageinfo.Damage, // 实际扣血
                BlockedDamage = damageinfo.DamageBlocked, // 被格挡的伤害
                ShieldedDamage = damageinfo.DamageShielded, // 被护盾吸收的伤害
                damageinfo.DamageType, // 伤害类型
                damageinfo.IsGrazed, // 是否擦伤
                damageinfo.IsAccuracy, // 是否精准
                damageinfo.OverDamage, // 溢出伤害
                TargetState = new
                {
                    playerTarget.Id,
                    playerTarget.Hp,
                    playerTarget.MaxHp,
                    playerTarget.Block,
                    playerTarget.Shield,
                    Status = playerTarget.Status.ToString(),
                    playerTarget.IsAlive,
                },
                Timestamp = DateTime.Now.Ticks,
            };

            // 序列化并发送事件。
            string json = JsonSerializer.Serialize(damageData);
            client.SendRequest("OnPlayerDamage", json);

            // 记录日志，便于排查同步问题。
            Plugin.Logger?.LogInfo(
                $"[DamageSync] Player took {damageinfo.Amount:F1} damage (HP: {damageinfo.Damage:F1}, " +
                $"Block: {damageinfo.DamageBlocked:F1}, Shield: {damageinfo.DamageShielded:F1}). " +
                $"Remaining HP: {playerTarget.Hp}/{playerTarget.MaxHp}");
        }
        catch (Exception ex)
        {
            // 捕获异常：防止补丁异常影响游戏主流程。
            Plugin.Logger?.LogError($"[DamageSync] Error in Damage_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion

    #region 状态效果同步

    /// <summary>
    /// 添加状态效果完成后，同步目标单位的完整状态列表。
    /// </summary>
    /// <param name="__instance">被补丁的 <see cref="BattleController"/> 实例（Harmony 注入）。</param>
    /// <param name="target">被添加状态效果的单位。</param>
    [HarmonyPatch(typeof(BattleController), "TryAddStatusEffect")]
    [HarmonyPostfix]
    public static void TryAddStatusEffect_Postfix(BattleController __instance, Unit target)
    {
        try
        {
            // 依赖注入服务未就绪时跳过。
            if (serviceProvider == null)
            {
                Plugin.Logger?.LogDebug("[StatusSync] ServiceProvider not initialized for TryAddStatusEffect");
                return;
            }

            // 通过 Traverse 访问私有字段 _statusEffects 以获取完整列表。
            var statusEffects = Traverse.Create(target)
                .Field("_statusEffects")?
                .GetValue<OrderedList<StatusEffect>>();

            // 将状态效果对象转换为字符串，保证序列化简单可靠。
            List<string> statusEffectList = [];
            foreach (var se in statusEffects)
            {
                statusEffectList.Add(se.ToString());
            }

            // 构建并发送同步数据。
            string json = JsonSerializer.Serialize(new
            {
                statusEffects = statusEffectList,
                TargetId = target?.Id,
                Timestamp = DateTime.Now.Ticks,
            });

            // TODO：后续可补充用户 ID（服务端侧更容易定位来源）。
            networkClient.SendRequest("UpdateAfterTryAddStatusEffects", json);

            Plugin.Logger?.LogInfo($"[StatusSync] Status effects updated after addition for unit {target?.Id}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[StatusSync] Error in TryAddStatusEffect_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 移除状态效果完成后，同步目标单位的完整状态列表。
    /// </summary>
    /// <param name="__instance">被补丁的 <see cref="BattleController"/> 实例（Harmony 注入）。</param>
    /// <param name="target">被移除状态效果的单位。</param>
    [HarmonyPatch(typeof(BattleController), "RemoveStatusEffect")]
    [HarmonyPostfix]
    public static void RemoveStatusEffect_Postfix(BattleController __instance, Unit target)
    {
        try
        {
            // 依赖注入服务未就绪时跳过。
            if (serviceProvider == null)
            {
                Plugin.Logger?.LogDebug("[StatusSync] ServiceProvider not initialized for RemoveStatusEffect");
                return;
            }

            // 通过 Traverse 访问私有字段 _statusEffects 以获取完整列表。
            var statusEffects = Traverse.Create(target)
                .Field("_statusEffects")?
                .GetValue<OrderedList<StatusEffect>>();

            // 将状态效果对象转换为字符串，保证序列化简单可靠。
            List<string> statusEffectList = [];
            foreach (var se in statusEffects)
            {
                statusEffectList.Add(se.ToString());
            }

            // 构建并发送同步数据。
            string json = JsonSerializer.Serialize(new
            {
                statusEffects = statusEffectList,
                TargetId = target?.Id,
                Timestamp = DateTime.Now.Ticks,
            });

            // TODO：后续可补充用户 ID（服务端侧更容易定位来源）。
            networkClient.SendRequest("UpdateAfterTryRemoveStatusEffects", json);

            Plugin.Logger?.LogInfo($"[StatusSync] Status effects updated after removal for unit {target?.Id}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[StatusSync] Error in RemoveStatusEffect_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion

    #region 治疗同步

    /// <summary>
    /// 治疗完成后同步目标单位的生命相关数据。
    /// </summary>
    /// <param name="__instance">被补丁的 <see cref="BattleController"/> 实例（Harmony 注入）。</param>
    /// <param name="target">接受治疗的目标单位。</param>
    [HarmonyPatch(typeof(BattleController), "Heal")]
    [HarmonyPostfix]
    public static void Heal_Postfix(BattleController __instance, Unit target)
    {
        try
        {
            // 依赖注入服务未就绪时跳过。
            if (serviceProvider == null)
            {
                Plugin.Logger?.LogDebug("[HealSync] ServiceProvider not initialized");
                return;
            }

            // 网络客户端不可用时跳过。
            if (networkClient == null)
            {
                Plugin.Logger?.LogDebug("[HealSync] Network client not available");
                return;
            }

            // 构建“治疗后目标快照”。
            string json = JsonSerializer.Serialize(new
            {
                Hp = target.Hp.ToString(),
                Block = target.Block.ToString(),
                Shield = target.Shield.ToString(),
                Status = target.Status.ToString(),
                TargetId = target?.Id,
                MaxHp = target.MaxHp.ToString(),
                IsAlive = target.IsAlive,
                Timestamp = DateTime.Now.Ticks,
            });

            // TODO：后续可补充用户 ID（服务端侧更容易定位来源）。
            networkClient.SendRequest("UpdateHealthAfterHeal", json);

            Plugin.Logger?.LogInfo(
                $"[HealSync] Health updated after heal for unit {target?.Id}: " +
                $"HP {target.Hp}/{target.MaxHp}, Block {target.Block}, Shield {target.Shield}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[HealSync] Error in Heal_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion
}
