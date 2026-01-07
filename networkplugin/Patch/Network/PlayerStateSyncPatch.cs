using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 玩家基础状态同步补丁（参考 Together in Spire 的 CreatureSyncPatches 思路）：
/// - 同步本地玩家的 Block/Shield 变化（不经过 Damage/Heal 的场景）
/// - 同步本地玩家的金钱变化（Gain/Consume/Lose）
/// - 同步本地玩家的 MaxHP 变化
/// </summary>
[HarmonyPatch]
public static class PlayerStateSyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetClient()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkClient>();
        }
        catch
        {
            return null;
        }
    }

    private static bool ShouldSend()
    {
        var client = TryGetClient();
        if (client == null || !client.IsConnected)
        {
            return false;
        }

        NetworkIdentityTracker.EnsureSubscribed(client);
        return !string.IsNullOrWhiteSpace(NetworkIdentityTracker.GetSelfPlayerId());
    }

    private static void Send(string eventType, object payload)
    {
        try
        {
            var client = TryGetClient();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            NetworkIdentityTracker.EnsureSubscribed(client);
            client.SendGameEventData(eventType, payload);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PlayerStateSync] Error sending {eventType}: {ex.Message}");
        }
    }

    private static bool ShouldSyncLocalBattle(BattleController battle)
        => battle != null && battle.Player != null && battle.Player == GameStateUtils.GetCurrentPlayer();

    private static object SnapshotPlayer(PlayerUnit player)
        => new
        {
            PlayerUnitId = player?.Id?.ToString(),
            player?.Hp,
            player?.MaxHp,
            player?.Block,
            player?.Shield,
            player?.IsAlive,
            Status = player?.Status.ToString(),
        };

    [HarmonyPatch(typeof(BattleController), "CastBlockShield")]
    private static class BattleController_CastBlockShield_Sync
    {
        [HarmonyPrefix]
        public static void Prefix(BattleController __instance, Unit target, ref (int block, int shield) __state)
        {
            __state = (int.MinValue, int.MinValue);
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline || !ShouldSyncLocalBattle(__instance))
                {
                    return;
                }

                if (target is not PlayerUnit player || player != __instance.Player)
                {
                    return;
                }

                __state = (player.Block, player.Shield);
            }
            catch
            {
                // ignored
            }
        }

        [HarmonyPostfix]
        public static void Postfix(BattleController __instance, Unit target, float block, float shield, (int block, int shield) __state)
        {
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline || !ShouldSyncLocalBattle(__instance))
                {
                    return;
                }

                if (target is not PlayerUnit player || player != __instance.Player)
                {
                    return;
                }

                if (__state.block == int.MinValue)
                {
                    return;
                }

                if (player.Block == __state.block && player.Shield == __state.shield)
                {
                    return;
                }

                Send("PlayerStateUpdate", new
                {
                    Timestamp = DateTime.Now.Ticks,
                    UpdateType = "BlockShieldGained",
                    PlayerId = NetworkIdentityTracker.GetSelfPlayerId(),
                    BattleId = __instance.GetHashCode().ToString(),
                    Round = __instance.RoundCounter,
                    Requested = new { Block = block, Shield = shield },
                    Before = new { Block = __state.block, Shield = __state.shield },
                    After = new { Block = player.Block, Shield = player.Shield },
                    Player = SnapshotPlayer(player),
                });
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PlayerStateSync] Error in CastBlockShield postfix: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(BattleController), "LoseBlockShield")]
    private static class BattleController_LoseBlockShield_Sync
    {
        [HarmonyPrefix]
        public static void Prefix(BattleController __instance, Unit target, ref (int block, int shield) __state)
        {
            __state = (int.MinValue, int.MinValue);
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline || !ShouldSyncLocalBattle(__instance))
                {
                    return;
                }

                if (target is not PlayerUnit player || player != __instance.Player)
                {
                    return;
                }

                __state = (player.Block, player.Shield);
            }
            catch
            {
                // ignored
            }
        }

        [HarmonyPostfix]
        public static void Postfix(BattleController __instance, Unit target, float block, float shield, (int block, int shield) __state)
        {
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline || !ShouldSyncLocalBattle(__instance))
                {
                    return;
                }

                if (target is not PlayerUnit player || player != __instance.Player)
                {
                    return;
                }

                if (__state.block == int.MinValue)
                {
                    return;
                }

                if (player.Block == __state.block && player.Shield == __state.shield)
                {
                    return;
                }

                Send("PlayerStateUpdate", new
                {
                    Timestamp = DateTime.Now.Ticks,
                    UpdateType = "BlockShieldLost",
                    PlayerId = NetworkIdentityTracker.GetSelfPlayerId(),
                    BattleId = __instance.GetHashCode().ToString(),
                    Round = __instance.RoundCounter,
                    Requested = new { Block = block, Shield = shield },
                    Before = new { Block = __state.block, Shield = __state.shield },
                    After = new { Block = player.Block, Shield = player.Shield },
                    Player = SnapshotPlayer(player),
                });
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PlayerStateSync] Error in LoseBlockShield postfix: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(Unit), "set_MaxHp")]
    private static class Unit_SetMaxHp_Sync
    {
        [HarmonyPrefix]
        public static void Prefix(Unit __instance, ref int __state)
        {
            __state = 0;
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                if (__instance is not PlayerUnit player || player != GameStateUtils.GetCurrentPlayer())
                {
                    return;
                }

                __state = player.MaxHp;
            }
            catch
            {
                // ignored
            }
        }

        [HarmonyPostfix]
        public static void Postfix(Unit __instance, int __state)
        {
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                if (__instance is not PlayerUnit player || player != GameStateUtils.GetCurrentPlayer())
                {
                    return;
                }

                if (__state == 0 || __state == player.MaxHp)
                {
                    return;
                }

                Send("PlayerStateUpdate", new
                {
                    Timestamp = DateTime.Now.Ticks,
                    UpdateType = "MaxHpChanged",
                    PlayerId = NetworkIdentityTracker.GetSelfPlayerId(),
                    Before = new { MaxHp = __state },
                    After = new { MaxHp = player.MaxHp },
                    Player = SnapshotPlayer(player),
                });
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PlayerStateSync] Error in set_MaxHp postfix: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.GainMoney))]
    private static class GameRun_GainMoney_Sync
    {
        [HarmonyPrefix]
        public static void Prefix(GameRunController __instance, int money, out int __state)
        {
            __state = 0;
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                __state = __instance.Money;
            }
            catch
            {
                // ignored
            }
        }

        [HarmonyPostfix]
        public static void Postfix(GameRunController __instance, int money, int __state)
        {
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                if (__instance.Money == __state)
                {
                    return;
                }

                Send("PlayerStateUpdate", new
                {
                    Timestamp = DateTime.Now.Ticks,
                    UpdateType = "MoneyGained",
                    PlayerId = NetworkIdentityTracker.GetSelfPlayerId(),
                    Before = new { Money = __state },
                    After = new { Money = __instance.Money },
                    Delta = __instance.Money - __state,
                    Requested = money,
                    PlayerUnitId = GameStateUtils.GetCurrentPlayerId(),
                });
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PlayerStateSync] Error in GainMoney postfix: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.ConsumeMoney))]
    private static class GameRun_ConsumeMoney_Sync
    {
        [HarmonyPrefix]
        public static void Prefix(GameRunController __instance, int cost, out int __state)
        {
            __state = 0;
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                __state = __instance.Money;
            }
            catch
            {
                // ignored
            }
        }

        [HarmonyPostfix]
        public static void Postfix(GameRunController __instance, int cost, int __state)
        {
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                if (__instance.Money == __state)
                {
                    return;
                }

                Send("PlayerStateUpdate", new
                {
                    Timestamp = DateTime.Now.Ticks,
                    UpdateType = "MoneyConsumed",
                    PlayerId = NetworkIdentityTracker.GetSelfPlayerId(),
                    Before = new { Money = __state },
                    After = new { Money = __instance.Money },
                    Delta = __instance.Money - __state,
                    Requested = cost,
                    PlayerUnitId = GameStateUtils.GetCurrentPlayerId(),
                });
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PlayerStateSync] Error in ConsumeMoney postfix: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.LoseMoney))]
    private static class GameRun_LoseMoney_Sync
    {
        [HarmonyPrefix]
        public static void Prefix(GameRunController __instance, int money, out int __state)
        {
            __state = 0;
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                __state = __instance.Money;
            }
            catch
            {
                // ignored
            }
        }

        [HarmonyPostfix]
        public static void Postfix(GameRunController __instance, int money, int __state)
        {
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                if (__instance.Money == __state)
                {
                    return;
                }

                Send("PlayerStateUpdate", new
                {
                    Timestamp = DateTime.Now.Ticks,
                    UpdateType = "MoneyLost",
                    PlayerId = NetworkIdentityTracker.GetSelfPlayerId(),
                    Before = new { Money = __state },
                    After = new { Money = __instance.Money },
                    Delta = __instance.Money - __state,
                    Requested = money,
                    PlayerUnitId = GameStateUtils.GetCurrentPlayerId(),
                });
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PlayerStateSync] Error in LoseMoney postfix: {ex.Message}");
            }
        }
    }
}
