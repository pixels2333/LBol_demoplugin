using System;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.Actions;

/// <summary>
/// 回合管理同步补丁 - 同步回合开始和结束
/// </summary>
public class TurnAction_Patch
{
    private static IServiceProvider serviceProvider =>
        ModService.ServiceProvider;

    /*
    /// <summary>
    /// 玩家回合开始同步
    /// </summary>
    // [HarmonyPatch(typeof(StartPlayerTurnAction), "Execute")] // Execute method doesn't exist - commented out
    [HarmonyPostfix]
    public static void StartPlayerTurn_Postfix(StartPlayerTurnAction __instance)
    {
        try
        {
            if (serviceProvider == null)
                return;

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
                return;

            var source = __instance.Source;
            var battle = source?.Battle;
            if (battle == null)
                return;

            // 只同步玩家回合
            if (!(source is PlayerUnit))
                return;

            var turnData = new
            {
                PlayerId = source.Id,
                TurnNumber = battle.TurnCounter,
                DrawCount = __instance.DrawCount, // 抽牌数量
                ExtraTurn = __instance.ExtraTurn,
                CardsInHand = battle.Player.HandZone.Count,
                CardsInDraw = battle.DrawZone.Count,
                CardsInDiscard = battle.DiscardZone.Count,
                ManaReset = true // 回合开始时法力重置
            };

            var json = JsonSerializer.Serialize(turnData);
            networkClient.SendRequest("OnPlayerTurnStart", json);

            Plugin.Logger?.LogInfo($"[TurnAction_Patch] Player turn started. Turn #{turnData.TurnNumber}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TurnAction_Patch] Error in StartPlayerTurn_Postfix: {ex.Message}");
        }
    }
    */

    /*
    /// <summary>
    /// 玩家回合结束同步
    /// </summary>
    // [HarmonyPatch(typeof(EndPlayerTurnAction), "Execute")] // Execute method doesn't exist - commented out
    [HarmonyPostfix]
    public static void EndPlayerTurn_Postfix(EndPlayerTurnAction __instance)
    {
        try
        {
            if (serviceProvider == null)
                return;

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
                return;

            var source = __instance.Source;
            var battle = source?.Battle;
            if (battle == null)
                return;

            // 只同步玩家回合
            if (!(source is PlayerUnit))
                return;

            var turnData = new
            {
                PlayerId = source.Id,
                TurnNumber = battle.TurnCounter,
                CardsInHand = battle.Player.HandZone.Count,
                CardsInDiscard = battle.DiscardZone.Count,
                Block = source.Block,
                ExtraTurn = __instance.ExtraTurn
            };

            var json = JsonSerializer.Serialize(turnData);
            networkClient.SendRequest("OnPlayerTurnEnd", json);

            Plugin.Logger?.LogInfo($"[TurnAction_Patch] Player turn ended. Turn #{turnData.TurnNumber}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TurnAction_Patch] Error in EndPlayerTurn_Postfix: {ex.Message}");
        }
    }
    */

    /*
    /// <summary>
    /// 战斗开始同步
    /// </summary>
    // [HarmonyPatch(typeof(StartBattleAction), "Execute")] // Execute method doesn't exist - commented out
    [HarmonyPostfix]
    public static void StartBattle_Postfix(StartBattleAction __instance)
    {
        try
        {
            if (serviceProvider == null)
                return;

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
                return;

            var battle = __instance.Battle;
            if (battle == null)
                return;

            var battleData = new
            {
                BattleId = battle.GetHashCode(),
                PlayerId = battle.Player.Id,
                PlayerName = battle.Player.Name,
                EnemyGroupId = battle.EnemyGroup.GetHashCode(),
                EnemyCount = battle.EnemyGroup.Count,
                InitialPlayerHP = battle.Player.Hp,
                InitialPlayerBlock = battle.Player.Block,
                InitialPlayerShield = battle.Player.Shield
            };

            var json = JsonSerializer.Serialize(battleData);
            networkClient.SendRequest("OnBattleStart", json);

            Plugin.Logger?.LogInfo($"[TurnAction_Patch] Battle started. Enemies: {battleData.EnemyCount}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TurnAction_Patch] Error in StartBattle_Postfix: {ex.Message}");
        }
    }
    */

    /*
    /// <summary>
    /// 战斗结束同步
    /// </summary>
    // [HarmonyPatch(typeof(EndBattleAction), "Execute")] // Execute method doesn't exist - commented out
    [HarmonyPostfix]
    public static void EndBattle_Postfix(EndBattleAction __instance)
    {
        try
        {
            if (serviceProvider == null)
                return;

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
                return;

            var battle = __instance.Battle;
            if (battle == null)
                return;

            var battleData = new
            {
                BattleId = battle.GetHashCode(),
                PlayerId = battle.Player.Id,
                FinalPlayerHP = battle.Player.Hp,
                FinalPlayerBlock = battle.Player.Block,
                FinalPlayerShield = battle.Player.Shield,
                Victory = __instance.Victory,
                Escape = __instance.Escape
            };

            var json = JsonSerializer.Serialize(battleData);
            networkClient.SendRequest("OnBattleEnd", json);

            Plugin.Logger?.LogInfo($"[TurnAction_Patch] Battle ended. Victory: {battleData.Victory}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TurnAction_Patch] Error in EndBattle_Postfix: {ex.Message}");
        }
    }
    */
}
