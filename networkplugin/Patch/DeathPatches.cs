using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.UI.Panels;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch;

/// <summary>
/// 玩家死亡处理补丁：在联机模式下实现“假死（Fake Death）”机制，并同步死亡/复活状态。
/// </summary>
/// <remarks>
/// 核心思路：
/// - 单人模式：沿用原版死亡流程。
/// - 联机模式：当玩家“死亡”时不立即结束游戏，而进入假死状态，允许其他玩家继续。
/// - 当满足条件（例如所有玩家都死亡或强制结束）时，才允许真死。
/// </remarks>
[HarmonyPatch]
public class DeathPatches
{
    #region 全局开关与依赖注入

    /// <summary>
    /// 是否允许玩家进入“真正死亡”流程。
    /// </summary>
    /// <remarks>
    /// - false：联机中默认不允许真死（进入假死）。
    /// - true：允许游戏走原版死亡/结束流程（通常用于“所有玩家都死亡”等终局条件）。
    /// </remarks>
    public static bool AllowRealDeath = false;

    /// <summary>
    /// 依赖注入服务提供者。
    /// </summary>
    private static IServiceProvider ServiceProvider = ModService.ServiceProvider;

    /// <summary>
    /// 网络客户端（用于判断联机状态与发送同步消息）。
    /// </summary>
    private static INetworkClient NetworkClient => ServiceProvider?.GetRequiredService<INetworkClient>();

    /// <summary>
    /// 在“从网络落地”的场景下抑制再次向网络广播，避免回环/重复消息。
    /// </summary>
    public static bool SuppressNetworkSync { get; set; }

    #endregion

    #region 核心状态判断

    /// <summary>
    /// 判断玩家是否处于“假死”状态。
    /// </summary>
    /// <param name="player">玩家单位。</param>
    /// <returns>处于联机模式，且玩家已死且不允许真死时返回 true，否则 false。</returns>
    private static bool IsPlayerInFakeDeath(PlayerUnit player)
    {
        // 未联机时不启用假死机制。
        if (NetworkClient == null || !NetworkClient.IsConnected)
        {
            return false;
        }

        // 联机：已死 + 不允许真死 => 假死。
        return player != null && player.IsDead && !AllowRealDeath;
    }

    #endregion

    #region 假死/复活处理

    /// <summary>
    /// 处理玩家假死：将 HP 拉回到可存活的最小值，并同步状态。
    /// </summary>
    /// <param name="player">玩家单位。</param>
    private static void HandleFakeDeath(PlayerUnit player)
    {
        if (player == null)
        {
            return;
        }

        try
        {
            // 通过反射调用内部 Heal：确保 HP 至少为 1，避免触发真死。
            int hpToRecover = Math.Max(1, 1 - player.Hp);
            if (hpToRecover > 0)
            {
                var traverse = Traverse.Create(player);
                traverse.Method("Heal", hpToRecover).GetValue();
            }

            // 同步死亡状态（标记为假死）。
            SyncPlayerDeathStatus(player, true);

            Plugin.Logger?.LogInfo($"[DeathPatch] 玩家 {player.Id} 进入假死状态 (Hp: {player.Hp})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] HandleFakeDeath 异常: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 处理玩家复活：恢复 HP，并同步状态。
    /// </summary>
    /// <param name="player">玩家单位。</param>
    /// <param name="resurrectionHp">复活后的目标 HP；为 null 时默认取最大生命的一半。</param>
    private static void HandleResurrection(PlayerUnit player, int? resurrectionHp = null)
    {
        if (player == null)
        {
            return;
        }

        try
        {
            // 计算复活目标生命值。
            int finalHp = resurrectionHp ?? (player.MaxHp / 2);
            if (finalHp <= 0)
            {
                finalHp = 1;
            }

            // 计算需要恢复的量，并通过 Heal 补回。
            int hpToRecover = finalHp - player.Hp;
            if (hpToRecover > 0)
            {
                var traverse = Traverse.Create(player);
                traverse.Method("Heal", hpToRecover).GetValue();
            }

            // 同步复活状态。
            SyncPlayerResurrectionStatus(player, finalHp);

            Plugin.Logger?.LogInfo($"[DeathPatch] 玩家 {player.Id} 已复活 (Hp: {player.Hp}/{player.MaxHp})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] HandleResurrection 异常: {ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion

    #region 网络同步

    /// <summary>
    /// 同步玩家死亡状态（真死/假死）到服务器。
    /// </summary>
    /// <param name="player">玩家单位。</param>
    /// <param name="isFakeDeath">是否假死。</param>
    private static void SyncPlayerDeathStatus(PlayerUnit player, bool isFakeDeath)
    {
        if (NetworkClient == null || !NetworkClient.IsConnected)
        {
            return;
        }

        if (SuppressNetworkSync)
        {
            return;
        }

        try
        {
            // 需求：以服务端分配的 PlayerId 作为唯一标识。
            string playerId = null;
            try
            {
                NetworkIdentityTracker.EnsureSubscribed(NetworkClient);
                playerId = NetworkIdentityTracker.GetSelfPlayerId();
            }
            catch
            {
                // ignored
            }

            // 兜底：极少数情况下 Welcome 尚未到达，先退回本地 Id。
            if (string.IsNullOrWhiteSpace(playerId))
            {
                playerId = player?.Id;
            }

            var deathData = new
            {
                PlayerId = playerId,
                IsFakeDeath = isFakeDeath,
                Hp = player.Hp,
                MaxHp = player.MaxHp,
                Status = player.Status.ToString(),
                Timestamp = DateTime.UtcNow.Ticks,
            };

            string json = JsonCompat.Serialize(deathData);
            NetworkClient.SendRequest(NetworkMessageTypes.OnPlayerDeathStatusChanged, json);

            // 重要：服务端广播会排除发送方，因此本地也同步更新登记册，避免 Host 无法校验“复活 Host”请求。
            if (!string.IsNullOrWhiteSpace(playerId))
            {
                if (isFakeDeath)
                {
                    DeathRegistry.UpsertDeadPlayer(new DeadPlayerEntry
                    {
                        PlayerId = playerId,
                        PlayerName = playerId,
                        DeadCause = "FakeDeath",
                        CanResurrect = true,
                        MaxHp = player.MaxHp,
                        DeathTime = DateTime.UtcNow,
                        ResurrectionCost = Math.Max(0, player.MaxHp),
                        Level = 0,
                    });
                }
                else
                {
                    DeathRegistry.MarkAlive(playerId);
                }
            }

            Plugin.Logger?.LogDebug($"[DeathPatch] 已同步死亡状态（IsFakeDeath: {isFakeDeath}, Hp: {player.Hp}）");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] 同步死亡状态失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 同步玩家复活状态到服务器。
    /// </summary>
    /// <param name="player">玩家单位。</param>
    /// <param name="resurrectionHp">复活后目标生命值。</param>
    private static void SyncPlayerResurrectionStatus(PlayerUnit player, int resurrectionHp)
    {
        if (NetworkClient == null || !NetworkClient.IsConnected)
        {
            return;
        }

        if (SuppressNetworkSync)
        {
            return;
        }

        try
        {
            string playerId = null;
            try
            {
                NetworkIdentityTracker.EnsureSubscribed(NetworkClient);
                playerId = NetworkIdentityTracker.GetSelfPlayerId();
            }
            catch
            {
                // ignored
            }

            if (string.IsNullOrWhiteSpace(playerId))
            {
                playerId = player?.Id;
            }

            var resurrectionData = new
            {
                // 兼容字段：PlayerId / TargetPlayerId 都指向被复活者（自己）。
                PlayerId = playerId,
                TargetPlayerId = playerId,
                ResurrectionHp = resurrectionHp,
                MaxHp = player.MaxHp,
                Status = player.Status.ToString(),
                Timestamp = DateTime.UtcNow.Ticks,
            };

            string json = JsonCompat.Serialize(resurrectionData);
            NetworkClient.SendRequest(NetworkMessageTypes.OnPlayerResurrected, json);

            if (!string.IsNullOrWhiteSpace(playerId))
            {
                // 本地同步清理登记册（同理：广播不会回到发送方）。
                DeathRegistry.MarkAlive(playerId);
            }

            Plugin.Logger?.LogDebug($"[DeathPatch] 已同步复活状态（Hp: {resurrectionHp}）");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] 同步复活状态失败: {ex.Message}");
        }
    }

    #endregion

    #region Harmony 补丁点

    /// <summary>
    /// 战斗伤害结算后置：当伤害将本地玩家打到 0 HP 时，在不允许真死的联机模式下触发假死。
    /// </summary>
    /// <param name="__instance">战斗控制器实例。</param>
    [HarmonyPatch(typeof(BattleController), "Damage")]
    [HarmonyPostfix]
    public static void Damage_Postfix(BattleController __instance)
    {
        try
        {
            if (__instance?.Player == null)
            {
                return;
            }

            PlayerUnit player = __instance.Player;

            // 只处理联机模式。
            if (NetworkClient == null || !NetworkClient.IsConnected)
            {
                return;
            }

            // HP <= 0 且不允许真死 => 进入假死。
            if (player.Hp <= 0 && !AllowRealDeath)
            {
                // 先通过 Heal 把 HP 拉回到 1，避免原版死亡流程继续推进。
                Traverse traverse = Traverse.Create(player);
                traverse.Method("Heal", 1).GetValue();

                // 再执行统一的假死处理（包含同步）。
                HandleFakeDeath(player);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] Damage_Postfix 异常: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 单位死亡前置：在联机且不允许真死时，阻止玩家进入原版死亡流程并触发假死。
    /// </summary>
    /// <param name="__instance">即将死亡的单位。</param>
    /// <returns>返回 false 表示跳过原方法（阻止真死）。</returns>
    [HarmonyPatch(typeof(Unit), "Die")]
    [HarmonyPrefix]
    public static bool Die_Prefix(Unit __instance)
    {
        try
        {
            // 只处理玩家单位。
            if (__instance is not PlayerUnit player)
            {
                return true;
            }

            // 未联机时沿用原版流程。
            if (NetworkClient == null || !NetworkClient.IsConnected)
            {
                return true;
            }

            // 不允许真死且已判定死亡：转为假死并阻止原死亡流程。
            if (!AllowRealDeath && player.IsDead)
            {
                HandleFakeDeath(player);
                return false;
            }

            // 允许真死时继续原流程。
            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] Die_Prefix 异常: {ex.Message}\n{ex.StackTrace}");
            // 出错时保守起见，放行原流程。
            return true;
        }
    }

    /// <summary>
    /// 回合开始前置：若玩家处于假死状态，则跳过玩家回合开始逻辑。
    /// </summary>
    /// <param name="__instance">战斗控制器实例。</param>
    /// <returns>返回 false 表示阻止原方法。</returns>
    [HarmonyPatch(typeof(BattleController), "StartPlayerTurn")]
    [HarmonyPrefix]
    public static bool StartPlayerTurn_Prefix(BattleController __instance)
    {
        try
        {
            if (__instance?.Player == null)
            {
                return true;
            }

            // 假死状态下不应进入玩家回合（避免卡牌/输入等流程）。
            if (IsPlayerInFakeDeath(__instance.Player))
            {
                Plugin.Logger?.LogDebug("[DeathPatch] 玩家处于假死，跳过回合开始");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] StartPlayerTurn_Prefix 异常: {ex.Message}");
            return true;
        }
    }

    /// <summary>
    /// 出牌请求前置：若玩家处于假死状态，则阻止出牌。
    /// </summary>
    /// <param name="__instance">战斗控制器实例。</param>
    /// <param name="card">请求使用的卡牌。</param>
    /// <returns>返回 false 表示阻止原方法。</returns>
    [HarmonyPatch(typeof(BattleController), "RequestUseCard")]
    [HarmonyPrefix]
    public static bool RequestUseCard_Prefix(BattleController __instance, Card card)
    {
        try
        {
            if (__instance?.Player == null || card == null)
            {
                return true;
            }

            // 假死状态下不能出牌。
            if (IsPlayerInFakeDeath(__instance.Player))
            {
                Plugin.Logger?.LogDebug("[DeathPatch] 玩家处于假死，阻止出牌");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] RequestUseCard_Prefix 异常: {ex.Message}");
            return true;
        }
    }

    /// <summary>
    /// 结束回合请求前置：若玩家处于假死状态，则阻止手动结束回合。
    /// </summary>
    /// <param name="__instance">战斗控制器实例。</param>
    /// <returns>返回 false 表示阻止原方法。</returns>
    [HarmonyPatch(typeof(BattleController), "RequestEndPlayerTurn")]
    [HarmonyPrefix]
    public static bool RequestEndPlayerTurn_Prefix(BattleController __instance)
    {
        try
        {
            if (__instance?.Player == null)
            {
                return true;
            }

            // 假死状态下不应等待玩家手动结束回合。
            if (IsPlayerInFakeDeath(__instance.Player))
            {
                Plugin.Logger?.LogDebug("[DeathPatch] 玩家处于假死，阻止结束回合请求");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] RequestEndPlayerTurn_Prefix 异常: {ex.Message}");
            return true;
        }
    }

    /// <summary>
    /// 战斗结束条件后置：若玩家处于假死状态，则强制让战斗继续（不结束）。
    /// </summary>
    /// <param name="__instance">战斗控制器实例。</param>
    /// <param name="__result">原方法计算结果（Harmony 注入）。</param>
    [HarmonyPatch(typeof(BattleController), "BattleShouldEnd", MethodType.Getter)]
    [HarmonyPostfix]
    public static void BattleShouldEnd_Postfix(BattleController __instance, ref bool __result)
    {
        try
        {
            if (__instance?.Player == null)
            {
                return;
            }

            // 只处理联机模式。
            if (NetworkClient == null || !NetworkClient.IsConnected)
            {
                return;
            }

            var player = __instance.Player;

            // 假死（已死但不允许真死）时，战斗不应结束。
            if (player.IsDead && !AllowRealDeath)
            {
                __result = false;
                Plugin.Logger?.LogDebug("[DeathPatch] 玩家处于假死，战斗继续");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] BattleShouldEnd_Postfix 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 是否等待玩家输入前置：假死状态下不等待输入（避免卡住回合）。
    /// </summary>
    /// <param name="__instance">战斗控制器实例。</param>
    /// <param name="__result">返回值（Harmony 注入）。</param>
    /// <returns>返回 false 表示跳过原 getter，并使用 __result。</returns>
    [HarmonyPatch(typeof(BattleController), "IsWaitingPlayerInput", MethodType.Getter)]
    [HarmonyPrefix]
    public static bool IsWaitingPlayerInput_Prefix(BattleController __instance, ref bool __result)
    {
        try
        {
            if (__instance?.Player == null)
            {
                return true;
            }

            // 假死状态下不等待输入。
            if (IsPlayerInFakeDeath(__instance.Player))
            {
                __result = false;
                Plugin.Logger?.LogDebug("[DeathPatch] 玩家处于假死，不等待输入");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] IsWaitingPlayerInput_Prefix 异常: {ex.Message}");
            return true;
        }
    }

    #endregion

    #region 对外接口

    /// <summary>
    /// 触发玩家复活（供网络回调或特殊事件调用）。
    /// </summary>
    /// <param name="player">要复活的玩家。</param>
    /// <param name="resurrectionHp">复活生命值（null 表示最大生命的 50%）。</param>
    public static void ResurrectPlayer(PlayerUnit player, int? resurrectionHp = null)
    {
        HandleResurrection(player, resurrectionHp);
    }

    /// <summary>
    /// 设置是否允许真死。
    /// </summary>
    /// <param name="allow">是否允许真死。</param>
    public static void SetAllowRealDeath(bool allow)
    {
        AllowRealDeath = allow;
        Plugin.Logger?.LogInfo($"[DeathPatch] AllowRealDeath 设置为: {allow}");
    }

    /// <summary>
    /// 判断玩家是否处于假死状态（对外暴露）。
    /// </summary>
    /// <param name="player">玩家单位。</param>
    /// <returns>处于假死返回 true，否则 false。</returns>
    public static bool IsPlayerDead(PlayerUnit player)
    {
        return IsPlayerInFakeDeath(player);
    }

    #endregion
}
