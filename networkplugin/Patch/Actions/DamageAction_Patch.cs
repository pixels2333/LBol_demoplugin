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
/// 伤害动作同步补丁类
/// 同步游戏中的伤害事件，包括玩家攻击、技能伤害、反击等
/// </summary>
public class DamageAction_Patch
{
    /// <summary>
    /// 服务提供者实例
    /// </summary>
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    // ========================================
    // 构造函数补丁
    // ========================================

    /// <summary>
    /// DamageAction构造函数(1)同步补丁
    /// 拦截：DamageAction(Unit source, IEnumerable<Unit> targets, DamageInfo damageInfo, ...)
    /// </summary>
    [HarmonyPatch(typeof(DamageAction), MethodType.Constructor, typeof(Unit), typeof(IEnumerable<Unit>), typeof(DamageInfo), typeof(string), typeof(GunType))]
    [HarmonyPostfix]
    public static void Constructor1_Postfix(DamageAction __instance, Unit source, IEnumerable<Unit> targets, DamageInfo damageInfo, string gunName, GunType gunType)
    {
        try
        {
            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
                return;

            ISynchronizationManager syncManager = GetSyncManager();
            if (syncManager == null)
                return;

            INetworkManager networkManager = GetNetworkManager();
            if (networkManager == null)
                return;

            INetworkPlayer player = networkManager.GetSelf();

            // 只同步玩家造成的伤害
            if (source is not PlayerUnit)
                return;

            // 构建伤害同步数据
            Dictionary<string, object> damageData = new()

            {
                ["UserName"] = player.userName,
                ["Timestamp"] = DateTime.Now.Ticks,
                ["ActionType"] = "Damage",
                ["GunName"] = gunName,
                ["GunType"] = gunType.ToString(),
                ["Damage"]= damageInfo.Amount,
                ["DamageAmount"] = damageInfo.Amount,
                ["DamageType"] = damageInfo.DamageType.ToString(), // 使用DamageType而不是GetType()
                ["DamageShielded"]= damageInfo.DamageShielded,
                ["DamageBlocked"] = damageInfo.DamageBlocked,
                ["ZeroDamage"] = damageInfo.ZeroDamage,
                ["DontBreakPerfect"] = damageInfo.DontBreakPerfect,
                ["IsAccuracy"] = damageInfo.IsAccuracy,
                ["IsGrazed"]=damageInfo.IsGrazed,
                ["SourceId"] = source.Id,
                ["SourceName"] = source.Name, 
                ["TargetCount"] = 1,
            };

            // 添加目标信息
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
                        ["CurrentHp"] = target.Hp
                    });
                }
                damageData["Targets"] = targetList;
            }

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
            Plugin.Logger?.LogError($"[DamageSync] Constructor1_Postfix错误: {ex.Message}");
        }
    }

    /// <summary>
    /// DamageAction构造函数(2)同步补丁
    /// 拦截：DamageAction(Unit source, Unit unit, DamageInfo damageInfo, ...)
    /// </summary>
    [HarmonyPatch(typeof(DamageAction), MethodType.Constructor, typeof(Unit), typeof(Unit), typeof(DamageInfo), typeof(string), typeof(GunType))]
    [HarmonyPostfix]
    public static void Constructor2_Postfix(DamageAction __instance, Unit source, Unit unit, DamageInfo damageInfo, string gunName, GunType gunType)
    {
        try
        {
            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
                return;

            ISynchronizationManager syncManager = GetSyncManager();
            if (syncManager == null)
                return;

            INetworkManager networkManager = GetNetworkManager();
            if (networkManager == null)
                return;

            INetworkPlayer player = networkManager.GetSelf();

            // 只同步玩家造成的伤害
            if (source is not PlayerUnit)
                return;

            // 构建伤害同步数据
            Dictionary<string, object> damageData = new()

            {
                ["UserName"] = player.userName,
                ["Timestamp"] = DateTime.Now.Ticks,
                ["ActionType"] = "Damage",
                ["GunName"] = gunName,
                ["GunType"] = gunType.ToString(),
                ["Damage"]= damageInfo.Amount,
                ["DamageAmount"] = damageInfo.Amount,
                ["DamageType"] = damageInfo.DamageType.ToString(), // 使用DamageType而不是GetType()
                ["DamageShielded"]= damageInfo.DamageShielded,
                ["DamageBlocked"] = damageInfo.DamageBlocked,
                ["ZeroDamage"] = damageInfo.ZeroDamage,
                ["DontBreakPerfect"] = damageInfo.DontBreakPerfect,
                ["IsAccuracy"] = damageInfo.IsAccuracy,
                ["IsGrazed"]=damageInfo.IsGrazed,
                ["SourceId"] = source.Id,
                ["SourceName"] = source.Name, 
                ["TargetCount"] = 1,
            };

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
            Plugin.Logger?.LogError($"[DamageSync] Constructor2_Postfix错误: {ex.Message}");
        }
    }

    /// <summary>
    /// DamageAction.LoseLife静态方法同步补丁
    /// </summary>
    [HarmonyPatch(typeof(DamageAction), nameof(DamageAction.LoseLife))]
    [HarmonyPostfix]
    public static void LoseLife_Postfix(DamageAction __result, Unit target, int life)
    {
        try
        {
            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
                return;

            ISynchronizationManager syncManager = GetSyncManager();
            if (syncManager == null)
                return;

            INetworkManager networkManager = GetNetworkManager();
            if (networkManager == null)
                return;

            INetworkPlayer player = networkManager.GetSelf();

            // 构建失去生命同步数据
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
                ["TargetCurrentHp"] = target.Hp
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
            Plugin.Logger?.LogError($"[DamageSync] LoseLife_Postfix错误: {ex.Message}");
        }
    }

    /// <summary>
    /// DamageAction.Reaction静态方法同步补丁
    /// </summary>
    [HarmonyPatch(typeof(DamageAction), nameof(DamageAction.Reaction))]
    [HarmonyPostfix]
    public static void Reaction_Postfix(DamageAction __result, Unit target, int damage)
    {
        try
        {
            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
                return;

            ISynchronizationManager syncManager = GetSyncManager();
            if (syncManager == null)
                return;

            INetworkManager networkManager = GetNetworkManager();
            if (networkManager == null)
                return;

            INetworkPlayer player = networkManager.GetSelf();

            // 构建反应伤害同步数据
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
                ["TargetCurrentHp"] = target.Hp
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
            Plugin.Logger?.LogError($"[DamageSync] Reaction_Postfix错误: {ex.Message}");
        }
    }

    // ========================================
    // 辅助方法
    // ========================================

    /// <summary>
    /// 获取同步管理器
    /// </summary>
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
    /// 获取网络管理器
    /// </summary>
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
}
