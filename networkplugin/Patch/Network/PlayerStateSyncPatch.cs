using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

[HarmonyPatch]
public static class PlayerStateSyncPatch
{
    #region 依赖注入与发送

        private static INetworkClient TryGetClient()
        => SendSyncHelper.TryGetClient();

        private static bool ShouldSend()
    {
        return SendSyncHelper.IsReady();
    }

    private static void Send(string eventType, object payload)
    {
        var client = SendSyncHelper.TryGetClient();
        if (client == null) return;
        client.SendGameEventData(eventType, payload);
    }

    #endregion

    #region 本地战斗判断与快照

        private static bool ShouldSyncLocalBattle(BattleController battle)
        => SendSyncHelper.ShouldSyncBattle(battle);

        private static object SnapshotPlayer(PlayerUnit player)
    {
        var statusEffects = player?.StatusEffects == null ? new List<object>() :
            player.StatusEffects.Where(se => se != null).Select(se => new
            {
                Id = se.Id,
                Type = se.GetType().Name,
                Level = se.HasLevel ? se.Level : (se.HasCount ? se.Count : 0),
                Duration = se.HasDuration ? se.Duration : 0
            }).ToList<object>();

        return new
        {
            PlayerUnitId = player?.Id?.ToString(),
            player?.Hp,
            player?.MaxHp,
            player?.Block,
            player?.Shield,
            player?.IsAlive,
            Status = player?.Status.ToString(),
            Power = player?.Power,
            PowerPerLevel = player?.Us != null ? (int?)player.PowerPerLevel : null,
            MaxPowerLevel = player?.Us != null ? (int?)player.Us.MaxPowerLevel : null,
            StatusEffects = statusEffects,
        };
    }

    #endregion

    #region 格挡/护盾变化同步

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

                Send(NetworkMessageTypes.OnPlayerStateUpdate, new
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
                Plugin.Logger?.LogError($"[PlayerStateSync] CastBlockShield 后置同步失败: {ex.Message}");
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

                Send(NetworkMessageTypes.OnPlayerStateUpdate, new
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
                Plugin.Logger?.LogError($"[PlayerStateSync] LoseBlockShield 后置同步失败: {ex.Message}");
            }
        }
    }

    #endregion

    #region 最大生命变化同步

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

                Send(NetworkMessageTypes.OnPlayerStateUpdate, new
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
                Plugin.Logger?.LogError($"[PlayerStateSync] set_MaxHp 后置同步失败: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(PlayerUnit), "set_Power")]
    private static class PlayerUnit_SetPower_Sync
    {
        [HarmonyPrefix]
        public static void Prefix(PlayerUnit __instance, ref int __state)
        {
            __state = int.MinValue;
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                if (__instance != GameStateUtils.GetCurrentPlayer())
                {
                    return;
                }

                __state = __instance.Power;
            }
            catch
            {

            }
        }

        [HarmonyPostfix]
        public static void Postfix(PlayerUnit __instance, int __state)
        {
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                if (__instance != GameStateUtils.GetCurrentPlayer())
                {
                    return;
                }

                if (__state == int.MinValue || __state == __instance.Power)
                {
                    return;
                }

                Send(NetworkMessageTypes.OnPlayerStateUpdate, new
                {
                    Timestamp = DateTime.Now.Ticks,
                    UpdateType = "PowerChanged",
                    PlayerId = NetworkIdentityTracker.GetSelfPlayerId(),
                    Before = new { Power = __state },
                    After = new { Power = __instance.Power },
                    Player = SnapshotPlayer(__instance),
                });
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PlayerStateSync] set_Power 后置同步失败: {ex.Message}");
            }
        }
    }

    #endregion

    #region 金钱变化同步

    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.GainMoney))]
    internal static class GameRun_GainMoney_Sync
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

                Send(NetworkMessageTypes.OnPlayerStateUpdate, new
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
                Plugin.Logger?.LogError($"[PlayerStateSync] GainMoney 后置同步失败: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.ConsumeMoney))]
    internal static class GameRun_ConsumeMoney_Sync
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

                Send(NetworkMessageTypes.OnPlayerStateUpdate, new
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
                Plugin.Logger?.LogError($"[PlayerStateSync] ConsumeMoney 后置同步失败: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.LoseMoney))]
    internal static class GameRun_LoseMoney_Sync
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

                Send(NetworkMessageTypes.OnPlayerStateUpdate, new
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
                Plugin.Logger?.LogError($"[PlayerStateSync] LoseMoney 后置同步失败: {ex.Message}");
            }
        }
    }

    #endregion

    #region 开局即刻上报与 0.5s 定时脉冲同步

    private static float _lastPeriodicStateSentTime;

    public static void SendFullPlayerStateSnapshot(BattleController battle, string reason)
    {
        SendFullPlayerStateSnapshot(battle?.Player ?? GameStateUtils.GetCurrentPlayer(), reason);
    }

    public static void SendFullPlayerStateSnapshot(PlayerUnit player, string reason)
    {
        try
        {
            if (!ShouldSend()) return;
            player ??= GameStateUtils.GetCurrentPlayer();
            if (player == null) return;

            string selfId = NetworkIdentityTracker.GetSelfPlayerId();
            if (string.IsNullOrWhiteSpace(selfId)) return;

            Send(NetworkMessageTypes.OnPlayerStateUpdate, new
            {
                Timestamp = DateTime.Now.Ticks,
                UpdateType = reason,
                PlayerId = selfId,
                Player = new
                {
                    Hp = player.Hp,
                    MaxHp = player.MaxHp,
                    Block = player.Block,
                    Shield = player.Shield,
                    IsAlive = player.IsAlive,
                    Status = player.Status.ToString(),
                    Power = player.Power,
                    PowerPerLevel = player.Us != null ? (int?)player.PowerPerLevel : 1,
                    MaxPowerLevel = player.Us != null ? (int?)player.Us.MaxPowerLevel : 3,
                },
                PlayerUnitId = player.Id,
            });
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[PlayerStateSync] SendFullPlayerStateSnapshot 异常: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(BattleController), "StartBattle")]
    internal static class BattleController_StartBattle_SyncInitialState
    {
        [HarmonyPostfix]
        public static void Postfix(BattleController __instance)
        {
            try
            {
                SendFullPlayerStateSnapshot(__instance, "StartBattle");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PlayerStateSync] StartBattle 后置初入状态上报失败: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(GameDirector), "Update")]
    internal static class GameDirector_PeriodicHeartbeat_Sync
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            try
            {
                if (!ShouldSend() || RemoteCardUsePatch.IsInRemoteCardPipeline)
                {
                    return;
                }

                var battle = GameStateUtils.GetCurrentGameRun()?.Battle;
                if (battle == null || !ShouldSyncLocalBattle(battle))
                {
                    return;
                }

                if (UnityEngine.Time.unscaledTime - _lastPeriodicStateSentTime >= 0.5f)
                {
                    _lastPeriodicStateSentTime = UnityEngine.Time.unscaledTime;
                    SendFullPlayerStateSnapshot(battle, "Periodic0.5sHeartbeat");
                }
            }
            catch
            {
            }
        }
    }

    #endregion
}
