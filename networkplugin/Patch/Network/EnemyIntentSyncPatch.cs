using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Intentions;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 敌人意图同步补丁（参考 Together in Spire: Monster_IntentPatches.java）
/// <para/>
/// 对应关系：
/// - StS: MonsterGroup.showIntent() -> LBoL: StartRoundAction.MainPhase()（回合开始生成/展示下一回合意图）
/// - StS: AbstractMonster.createIntent() -> LBoL: EnemyUnit.UpdateTurnMoves()（刷新敌人回合行动并重建 Intentions）
/// </summary>
[HarmonyPatch]
public static class EnemyIntentSyncPatch
{
    /// <summary>
    /// 与 TogetherInSpire 一致：在应用远端同步/批量重放时可临时关闭意图同步，防止回环。
    /// </summary>
    public static bool PauseIntentSync { get; set; }

    /// <summary>
    /// 标记当前是否处于“回合切换时生成下一回合意图”的阶段（类似 StS 的 EOT intents）。
    /// </summary>
    private static bool _generatingRoundStartIntentions;

    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetNetworkClient()
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

    [HarmonyPatch(typeof(StartRoundAction), "MainPhase")]
    [HarmonyPrefix]
    public static void StartRoundAction_MainPhase_Prefix()
    {
        _generatingRoundStartIntentions = true;
    }

    [HarmonyPatch(typeof(StartRoundAction), "MainPhase")]
    [HarmonyPostfix]
    public static void StartRoundAction_MainPhase_Postfix()
    {
        _generatingRoundStartIntentions = false;
    }

    [HarmonyPatch(typeof(EnemyUnit), nameof(EnemyUnit.UpdateTurnMoves))]
    [HarmonyPostfix]
    public static void EnemyUnit_UpdateTurnMoves_Postfix(EnemyUnit __instance)
    {
        try
        {
            if (PauseIntentSync)
            {
                return;
            }

            if (__instance?.Battle == null)
            {
                return;
            }

            INetworkClient networkClient = TryGetNetworkClient();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            object payload = new
            {
                Timestamp = DateTime.Now.Ticks,
                EnemyGroupId = __instance.Battle.EnemyGroup?.Id,
                Round = __instance.Battle.RoundCounter,
                GeneratingEotIntents = _generatingRoundStartIntentions,
                Enemy = new
                {
                    __instance.RootIndex,
                    __instance.Id,
                    __instance.Name,
                    __instance.ModelName,
                },
                Intentions = BuildIntentionsSnapshot(__instance.Intentions),
            };

            string json = JsonSerializer.Serialize(payload);

            // 用 Battle* 前缀走 GameEvent 通道，避免被 NetworkClient/Server 当作“未知系统消息”丢弃。
            networkClient.SendRequest("BattleEnemyIntentChanged", json);

            Plugin.Logger?.LogDebug(
                $"[EnemyIntentSync] Intentions updated. Enemy={__instance.Name} RootIndex={__instance.RootIndex} Round={__instance.Battle.RoundCounter} EOT={_generatingRoundStartIntentions}"
            );
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemyIntentSync] Error syncing enemy intentions: {ex.Message}");
        }
    }

    private static List<object> BuildIntentionsSnapshot(IEnumerable<Intention> intentions)
    {
        if (intentions == null)
        {
            return [];
        }

        List<object> list = [];
        foreach (Intention intention in intentions.Where(i => i != null))
        {
            list.Add(SerializeIntention(intention));
        }

        return list;
    }

    private static object SerializeIntention(Intention intention)
    {
        try
        {
            return new
            {
                Class = intention.GetType().Name,
                Type = intention.Type.ToString(),
                intention.MoveName,
                intention.Name,
                intention.Description,
                intention.HiddenByEnemy,
                intention.ShowByEnemyTurn,
                intention.HiddenFinal,
                Specific = GetSpecificIntentionData(intention),
            };
        }
        catch (Exception ex)
        {
            return new
            {
                Class = intention?.GetType().Name,
                Error = ex.Message,
            };
        }
    }

    private static object GetSpecificIntentionData(Intention intention)
    {
        return intention switch
        {
            AttackIntention atk => new
            {
                Damage = atk.Damage.Damage,
                DamageType = atk.Damage.DamageType.ToString(),
                atk.Times,
                atk.IsAccuracy,
            },
            SpellCardIntention spell => new
            {
                Damage = spell.Damage?.Damage,
                DamageType = spell.Damage?.DamageType.ToString(),
                spell.Times,
                spell.IsAccuracy,
                spell.IconName,
            },
            CountDownIntention cd => new { cd.Counter },
            KokoroDarkIntention kd => new
            {
                Damage = kd.Damage.Damage,
                DamageType = kd.Damage.DamageType.ToString(),
                kd.Count,
            },
            ExplodeIntention ex => new
            {
                Damage = ex.Damage.Damage,
                DamageType = ex.Damage.DamageType.ToString(),
            },
            _ => null,
        };
    }
}

