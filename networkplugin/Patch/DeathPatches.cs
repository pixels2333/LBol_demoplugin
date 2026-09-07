using System;
using System.Linq;
using HarmonyLib;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.UI.Models;
using NetworkPlugin.UI.State;

namespace NetworkPlugin.Patch;

[HarmonyPatch]
public class DeathPatches
{
    #region 全局开关与依赖注入

        public static bool AllowRealDeath = false;

        private static IServiceProvider ServiceProvider = ModService.ServiceProvider;

        private static INetworkClient NetworkClient => ServiceProvider?.GetRequiredService<INetworkClient>();

        private static INetworkManager NetworkManager => ServiceProvider?.GetService<INetworkManager>();

        private static ConfigManager ConfigManager => ServiceProvider?.GetService<ConfigManager>() ?? Plugin.ConfigManager;

        public static bool SuppressNetworkSync { get; set; }

    #endregion

    #region 核心状态判断

        private static bool IsPlayerInFakeDeath(PlayerUnit player)
    {

        if (NetworkClient == null || !NetworkClient.IsConnected)
        {
            return false;
        }

        return player != null && player.IsDead && !AllowRealDeath;
    }

        private static string ResolveLocalPlayerRegistryId(PlayerUnit player)
    {
        return GameStateUtils.GetCurrentPlayerId();
    }

        private static bool AreAllKnownPlayersDead(PlayerUnit localPlayer)
    {
        if (localPlayer == null)
        {
            return false;
        }

        int totalPlayers = Math.Max(1, NetworkManager?.GetPlayerCount() ?? 1);
        var deadPlayerIds = DeathRegistry
            .GetDeadPlayersSnapshot()
            .Select(entry => entry.PlayerId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        if (localPlayer.IsDead)
        {
            string localPlayerId = ResolveLocalPlayerRegistryId(localPlayer);
            if (!string.IsNullOrWhiteSpace(localPlayerId))
            {
                deadPlayerIds.Add(localPlayerId);
            }
        }

        return deadPlayerIds.Count >= totalPlayers;
    }

        private static bool AreAllEnemiesResolved(BattleController battle)
    {
        return battle?.EnemyGroup != null && battle.EnemyGroup.All(enemy => enemy.IsDead || enemy.IsEscaped || enemy.IsServant);
    }

    #endregion

    #region 假死/复活处理

        private static void HandleFakeDeath(PlayerUnit player)
    {
        if (player == null)
        {
            return;
        }

        try
        {

            int hpToRecover = Math.Max(1, 1 - player.Hp);
            if (hpToRecover > 0)
            {
                Traverse traverse = Traverse.Create(player);
                traverse.Method("Heal", hpToRecover).GetValue();
            }

            SyncPlayerDeathStatus(player, true);

            Plugin.Logger?.LogInfo($"[DeathPatch] 玩家 {player.Id} 进入假死状态 (Hp: {player.Hp})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] HandleFakeDeath 异常: {ex.Message}\n{ex.StackTrace}");
        }
    }

        private static void HandleResurrection(PlayerUnit player, int? resurrectionHp = null)
    {
        if (player == null)
        {
            return;
        }

        try
        {

            int finalHp = resurrectionHp ?? (player.MaxHp / 2);
            if (finalHp <= 0)
            {
                finalHp = 1;
            }

            int hpToRecover = finalHp - player.Hp;
            if (hpToRecover > 0)
            {
                Traverse traverse = Traverse.Create(player);
                traverse.Method("Heal", hpToRecover).GetValue();
            }

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

            string playerId = null;
            try
            {
                NetworkIdentityTracker.EnsureSubscribed(NetworkClient);
                playerId = NetworkIdentityTracker.GetSelfPlayerId();
            }
            catch
            {

            }

            if (string.IsNullOrWhiteSpace(playerId))
            {
                playerId = GameStateUtils.GetCurrentPlayerId();
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

            if (!string.IsNullOrWhiteSpace(playerId))
            {
                if (isFakeDeath)
                {
                    DeathRegistry.UpsertDeadPlayer(new DeadPlayerEntry
                    {
                        PlayerId = playerId,
                        PlayerName = OtherPlayersOverlayPatch.ResolveDisplayName(playerId, null, isLocal: true),
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

            }

            if (string.IsNullOrWhiteSpace(playerId))
            {
                playerId = GameStateUtils.GetCurrentPlayerId();
            }

            var resurrectionData = new
            {

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

            if (NetworkClient == null || !NetworkClient.IsConnected)
            {
                return;
            }

            if (player.Hp <= 0 && !AllowRealDeath)
            {

                Traverse traverse = Traverse.Create(player);
                traverse.Method("Heal", 1).GetValue();

                HandleFakeDeath(player);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] Damage_Postfix 异常: {ex.Message}\n{ex.StackTrace}");
        }
    }

        [HarmonyPatch(typeof(Unit), "Die")]
    [HarmonyPrefix]
    public static bool Die_Prefix(Unit __instance)
    {
        try
        {

            if (__instance is not PlayerUnit player)
            {
                return true;
            }

            if (NetworkClient == null || !NetworkClient.IsConnected)
            {
                return true;
            }

            if (!AllowRealDeath && player.IsDead)
            {
                HandleFakeDeath(player);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] Die_Prefix 异常: {ex.Message}\n{ex.StackTrace}");

            return true;
        }
    }

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

            if (NetworkClient == null || !NetworkClient.IsConnected)
            {
                return;
            }

            var player = __instance.Player;

            if (player.IsDead && !AllowRealDeath)
            {
                if (AreAllKnownPlayersDead(player))
                {
                    SetAllowRealDeath(true);
                    __result = true;
                    Plugin.Logger?.LogInfo("[DeathPatch] 已判定全员死亡，放行原版失败结算");
                    return;
                }

                if (AreAllEnemiesResolved(__instance))
                {
                    __result = true;
                    Plugin.Logger?.LogDebug("[DeathPatch] 敌人已全部解决，放行战斗结束以执行战后自动复活");
                    return;
                }

                __result = false;
                Plugin.Logger?.LogDebug("[DeathPatch] 玩家处于假死且队友仍在战斗，继续战斗");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] BattleShouldEnd_Postfix 异常: {ex.Message}");
        }
    }

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

        public static void ResurrectPlayer(PlayerUnit player, int? resurrectionHp = null)
    {
        HandleResurrection(player, resurrectionHp);
    }

        public static void SetAllowRealDeath(bool allow)
    {
        if (AllowRealDeath == allow)
        {
            return;
        }

        AllowRealDeath = allow;
        Plugin.Logger?.LogInfo($"[DeathPatch] AllowRealDeath 设置为: {allow}");
    }

        public static bool ShouldAutoReviveAfterBattle(PlayerUnit player)
    {
        if (player == null || !player.IsDead)
        {
            return false;
        }

        if (NetworkClient == null || !NetworkClient.IsConnected)
        {
            return false;
        }

        bool allPlayersDead = AreAllKnownPlayersDead(player);
        SetAllowRealDeath(allPlayersDead);
        return !allPlayersDead;
    }

        public static int CalculateConfiguredAutoReviveHp(PlayerUnit player)
    {
        if (player == null)
        {
            return 1;
        }

        return ConfigManager?.CalculateBattleAutoReviveHp(player.MaxHp)
            ?? Math.Max(1, (int)Math.Ceiling(player.MaxHp * 0.1d));
    }

        public static bool IsPlayerDead(PlayerUnit player)
    {
        return IsPlayerInFakeDeath(player);
    }

    #endregion
}
