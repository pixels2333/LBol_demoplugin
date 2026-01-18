using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Core;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Patch.Network;

namespace NetworkPlugin.Patch.Actions;

/// <summary>
/// 伤害动作同步补丁。
/// </summary>
/// <remarks>
/// 目标：拦截 <see cref="DamageAction"/> 的构造与部分静态工厂方法，
/// 将“本地玩家造成的伤害/失去生命/反应伤害”等事件广播到联机层。
/// </remarks>
public class DamageAction_Patch
{
    #region 依赖注入

    /// <summary>
    /// 依赖注入服务提供者（用于解析网络相关服务）。
    /// </summary>
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    #endregion

    #region 构造函数补丁（伤害动作）

    /// <summary>
    /// 构造函数补丁(1)：多目标伤害。
    /// </summary>
    /// <param name="__instance">动作实例（Harmony 注入）。</param>
    /// <param name="source">伤害来源单位。</param>
    /// <param name="targets">伤害目标集合。</param>
    /// <param name="damageInfo">伤害信息。</param>
    /// <param name="gunName">武器/弹幕名称。</param>
    /// <param name="gunType">武器类型。</param>
    [HarmonyPatch(typeof(DamageAction), MethodType.Constructor, typeof(Unit), typeof(IEnumerable<Unit>), typeof(DamageInfo), typeof(string), typeof(GunType))]
    [HarmonyPostfix]
    public static void Constructor1_Postfix(DamageAction __instance, Unit source, IEnumerable<Unit> targets, DamageInfo damageInfo, string gunName, GunType gunType)
    {
        try
        {
            // 远程出牌管线中的动作由远端驱动，本地不应再次广播。
            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
            {
                return;
            }

            // 解析同步管理器。
            ISynchronizationManager syncManager = GetSyncManager();
            if (syncManager == null)
            {
                return;
            }

            // 解析网络管理器。
            INetworkManager networkManager = GetNetworkManager();
            if (networkManager == null)
            {
                return;
            }

            INetworkPlayer player = networkManager.GetSelf();

            // 只同步“玩家造成的伤害”，避免把敌人内部结算也广播出去。
            if (source is not PlayerUnit)
            {
                return;
            }

            // 构建伤害同步数据。
            Dictionary<string, object> damageData = new()
            {
                ["UserName"] = player.userName,
                ["Timestamp"] = DateTime.Now.Ticks,
                ["ActionType"] = "Damage",
                ["GunName"] = gunName,
                ["GunType"] = gunType.ToString(),
                ["Damage"] = damageInfo.Amount,
                ["DamageAmount"] = damageInfo.Amount,
                ["DamageType"] = damageInfo.DamageType.ToString(),
                ["DamageShielded"] = damageInfo.DamageShielded,
                ["DamageBlocked"] = damageInfo.DamageBlocked,
                ["ZeroDamage"] = damageInfo.ZeroDamage,
                ["DontBreakPerfect"] = damageInfo.DontBreakPerfect,
                ["IsAccuracy"] = damageInfo.IsAccuracy,
                ["IsGrazed"] = damageInfo.IsGrazed,
                ["SourceId"] = source.Id,
                ["SourceName"] = source.Name,
                ["TargetCount"] = 1,
            };

            // 补充目标列表信息，便于远端重放或校验。
            if (targets != null)
            {
                List<Dictionary<string, object>> targetList = new();
                foreach (Unit target in targets)
                {
                    targetList.Add(new Dictionary<string, object>
                    {
                        ["TargetId"] = target.Id,
                        ["TargetName"] = target.Name,
                        ["TargetType"] = target.GetType().Name,
                        ["CurrentHp"] = target.Hp,
                    });
                }

                damageData["Targets"] = targetList;
            }

            // 组装事件并发送。
            GameEvent gameEvent = GameEventManager.CreateEvent(
                NetworkMessageTypes.OnDamageDealt.ToString(),
                player.userName,
                damageData
            );

            syncManager.SendGameEvent(gameEvent);

            Plugin.Logger?.LogInfo($"[DamageSync] 伤害动作: {source.Name} -> 多个目标 (伤害: {damageInfo.Amount}, 武器: {gunName})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DamageSync] Constructor1_Postfix 错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 构造函数补丁(2)：单目标伤害。
    /// </summary>
    /// <param name="__instance">动作实例（Harmony 注入）。</param>
    /// <param name="source">伤害来源单位。</param>
    /// <param name="unit">伤害目标单位。</param>
    /// <param name="damageInfo">伤害信息。</param>
    /// <param name="gunName">武器/弹幕名称。</param>
    /// <param name="gunType">武器类型。</param>
    [HarmonyPatch(typeof(DamageAction), MethodType.Constructor, typeof(Unit), typeof(Unit), typeof(DamageInfo), typeof(string), typeof(GunType))]
    [HarmonyPostfix]
    public static void Constructor2_Postfix(DamageAction __instance, Unit source, Unit unit, DamageInfo damageInfo, string gunName, GunType gunType)
    {
        try
        {
            // 远程出牌管线中的动作由远端驱动，本地不应再次广播。
            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
            {
                return;
            }

            // 解析同步管理器。
            ISynchronizationManager syncManager = GetSyncManager();
            if (syncManager == null)
            {
                return;
            }

            // 解析网络管理器。
            INetworkManager networkManager = GetNetworkManager();
            if (networkManager == null)
            {
                return;
            }

            INetworkPlayer player = networkManager.GetSelf();

            // 只同步“玩家造成的伤害”。
            if (source is not PlayerUnit)
            {
                return;
            }

            // 构建伤害同步数据。
            Dictionary<string, object> damageData = new()
            {
                ["UserName"] = player.userName,
                ["Timestamp"] = DateTime.Now.Ticks,
                ["ActionType"] = "Damage",
                ["GunName"] = gunName,
                ["GunType"] = gunType.ToString(),
                ["Damage"] = damageInfo.Amount,
                ["DamageAmount"] = damageInfo.Amount,
                ["DamageType"] = damageInfo.DamageType.ToString(),
                ["DamageShielded"] = damageInfo.DamageShielded,
                ["DamageBlocked"] = damageInfo.DamageBlocked,
                ["ZeroDamage"] = damageInfo.ZeroDamage,
                ["DontBreakPerfect"] = damageInfo.DontBreakPerfect,
                ["IsAccuracy"] = damageInfo.IsAccuracy,
                ["IsGrazed"] = damageInfo.IsGrazed,
                ["SourceId"] = source.Id,
                ["SourceName"] = source.Name,
                ["TargetCount"] = 1,
            };

            // 组装事件并发送。
            GameEvent gameEvent = GameEventManager.CreateEvent(
                NetworkMessageTypes.OnDamageDealt.ToString(),
                player.userName,
                damageData
            );

            syncManager.SendGameEvent(gameEvent);

            Plugin.Logger?.LogInfo($"[DamageSync] 伤害动作: {source.Name} -> {unit.Name} (伤害: {damageInfo.Amount}, 武器: {gunName})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DamageSync] Constructor2_Postfix 错误: {ex.Message}");
        }
    }

    #endregion

    #region 静态方法补丁（失去生命/反应）

    /// <summary>
    /// <see cref="DamageAction.LoseLife"/> 后置：同步“失去生命”事件。
    /// </summary>
    /// <param name="__result">生成的伤害动作（Harmony 注入）。</param>
    /// <param name="target">失去生命的目标。</param>
    /// <param name="life">失去生命数值。</param>
    [HarmonyPatch(typeof(DamageAction), nameof(DamageAction.LoseLife))]
    [HarmonyPostfix]
    public static void LoseLife_Postfix(DamageAction __result, Unit target, int life)
    {
        try
        {
            // 远程出牌管线中的动作由远端驱动，本地不应再次广播。
            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
            {
                return;
            }

            // 解析同步管理器。
            ISynchronizationManager syncManager = GetSyncManager();
            if (syncManager == null)
            {
                return;
            }

            // 解析网络管理器。
            INetworkManager networkManager = GetNetworkManager();
            if (networkManager == null)
            {
                return;
            }

            INetworkPlayer player = networkManager.GetSelf();

            // 构建“失去生命”同步数据。
            Dictionary<string, object> damageData = new()
            {
                ["Timestamp"] = DateTime.Now.Ticks,
                ["ActionType"] = "LoseLife",
                ["GunName"] = __result.GunName,
                ["DamageAmount"] = life,
                ["DamageType"] = "HpLose",
                ["SourceId"] = target.Id,
                ["SourceName"] = target.Name,
                ["TargetCount"] = 1,
                ["UserName"] = player.userName,
                ["TargetId"] = target.Id,
                ["TargetName"] = target.Name,
                ["TargetType"] = target.GetType().Name,
                ["TargetCurrentHp"] = target.Hp,
            };

            GameEvent gameEvent = GameEventManager.CreateEvent(
                NetworkMessageTypes.OnDamageDealt.ToString(),
                player.userName,
                damageData
            );

            syncManager.SendGameEvent(gameEvent);

            Plugin.Logger?.LogInfo($"[DamageSync] 失去生命: {target.Name} 失去 {life} 点生命");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DamageSync] LoseLife_Postfix 错误: {ex.Message}");
        }
    }

    /// <summary>
    /// <see cref="DamageAction.Reaction"/> 后置：同步“反应伤害”事件。
    /// </summary>
    /// <param name="__result">生成的伤害动作（Harmony 注入）。</param>
    /// <param name="target">受到反应伤害的目标。</param>
    /// <param name="damage">反应伤害数值。</param>
    [HarmonyPatch(typeof(DamageAction), nameof(DamageAction.Reaction))]
    [HarmonyPostfix]
    public static void Reaction_Postfix(DamageAction __result, Unit target, int damage)
    {
        try
        {
            // 远程出牌管线中的动作由远端驱动，本地不应再次广播。
            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
            {
                return;
            }

            // 解析同步管理器。
            ISynchronizationManager syncManager = GetSyncManager();
            if (syncManager == null)
            {
                return;
            }

            // 解析网络管理器。
            INetworkManager networkManager = GetNetworkManager();
            if (networkManager == null)
            {
                return;
            }

            INetworkPlayer player = networkManager.GetSelf();

            // 构建反应伤害同步数据。
            Dictionary<string, object> damageData = new()
            {
                ["Timestamp"] = DateTime.Now.Ticks,
                ["ActionType"] = "Reaction",
                ["GunName"] = __result.GunName,
                ["DamageAmount"] = damage,
                ["DamageType"] = "Reaction",
                ["SourceId"] = target.Id,
                ["SourceName"] = target.Name,
                ["TargetCount"] = 1,
                ["UserName"] = player.userName,
                ["TargetId"] = target.Id,
                ["TargetName"] = target.Name,
                ["TargetType"] = target.GetType().Name,
                ["TargetCurrentHp"] = target.Hp,
            };

            GameEvent gameEvent = GameEventManager.CreateEvent(
                NetworkMessageTypes.OnDamageDealt.ToString(),
                player.userName,
                damageData
            );

            syncManager.SendGameEvent(gameEvent);

            Plugin.Logger?.LogInfo($"[DamageSync] 反应伤害: {target.Name} 受到 {damage} 点反应伤害");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DamageSync] Reaction_Postfix 错误: {ex.Message}");
        }
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 获取同步管理器。
    /// </summary>
    /// <returns>解析成功返回实例，否则返回 null。</returns>
    private static ISynchronizationManager GetSyncManager()
    {
        try
        {
            return serviceProvider?.GetService<ISynchronizationManager>();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取网络管理器。
    /// </summary>
    /// <returns>解析成功返回实例，否则返回 null。</returns>
    private static INetworkManager GetNetworkManager()
    {
        try
        {
            return serviceProvider?.GetService<INetworkManager>();
        }
        catch
        {
            return null;
        }
    }

    #endregion
}
