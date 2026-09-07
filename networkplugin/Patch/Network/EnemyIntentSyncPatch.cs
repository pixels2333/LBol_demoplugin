using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Intentions;
using LBoL.Core.Units;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

[HarmonyPatch]
public static class EnemyIntentSyncPatch
{
        public static bool PauseIntentSync { get; set; }

        private static bool _generatingRoundStartIntentions;

    private static bool _subscribed;
    private static INetworkClient _subscribedClient;
    private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;
    private static readonly Action<bool> _onConnectionStateChanged = OnConnectionStateChanged;

    private static long _lastJoinBroadcastTicks;

    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetNetworkClient()
        => ServiceProvider?.GetService<INetworkClient>();

        [HarmonyPatch(typeof(GameDirector), "Update")]
    private static class SubscribeHook
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            INetworkClient client = TryGetNetworkClient();
            if (client == null)
            {
                return;
            }

            EnsureSubscribed(client);
            NetworkIdentityTracker.EnsureSubscribed(client);
        }
    }

    private static void EnsureSubscribed(INetworkClient client)
    {
        if (_subscribed && ReferenceEquals(_subscribedClient, client))
        {
            return;
        }

        try
        {
            if (_subscribedClient != null)
            {
                _subscribedClient.OnGameEventReceived -= _onGameEventReceived;
                _subscribedClient.OnConnectionStateChanged -= _onConnectionStateChanged;
            }
        }
        catch
        {

        }

        try
        {
            client.OnGameEventReceived += _onGameEventReceived;
            client.OnConnectionStateChanged += _onConnectionStateChanged;
            _subscribedClient = client;
            _subscribed = true;
        }
        catch
        {
            _subscribedClient = null;
            _subscribed = false;
        }
    }

    private static void OnConnectionStateChanged(bool connected)
    {
        if (connected)
        {
            return;
        }

        _lastJoinBroadcastTicks = 0;
    }

    private static void OnGameEventReceived(string eventType, object payload)
    {

        if (!string.Equals(eventType, NetworkMessageTypes.PlayerJoined, StringComparison.Ordinal) &&
            !string.Equals(eventType, NetworkMessageTypes.Welcome, StringComparison.Ordinal) &&
            !string.Equals(eventType, NetworkMessageTypes.PlayerListUpdate, StringComparison.Ordinal))
        {
            return;
        }

        if (!NetworkIdentityTracker.GetSelfIsHost())
        {
            return;
        }

        if (!ShouldBroadcastAgain(ref _lastJoinBroadcastTicks, TimeSpan.FromSeconds(1)))
        {
            return;
        }

        TryBroadcastCurrentBattleIntentions();
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

            NetworkIdentityTracker.EnsureSubscribed(networkClient);
            if (!NetworkIdentityTracker.GetSelfIsHost())
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
                    SpawnId = SpawnedEnemySyncPatch.TryGetSpawnId(__instance, out string spawnId) ? spawnId : null,
                    __instance.Name,
                    __instance.ModelName,
                },
                Intentions = BuildIntentionsSnapshot(__instance.Intentions),
            };

            string json = JsonCompat.Serialize(payload);

            networkClient.SendRequest(NetworkMessageTypes.BattleEnemyIntentChanged, json);

            Plugin.Logger?.LogDebug(
                $"[EnemyIntentSync] Intentions updated. Enemy={__instance.Name} RootIndex={__instance.RootIndex} Round={__instance.Battle.RoundCounter} EOT={_generatingRoundStartIntentions}"
            );
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemyIntentSync] Error syncing enemy intentions: {ex.Message}");
        }
    }

    private static bool ShouldBroadcastAgain(ref long lastTicks, TimeSpan cooldown)
    {
        long now = DateTime.Now.Ticks;
        long prev = lastTicks;
        if (prev != 0 && now - prev < cooldown.Ticks)
        {
            return false;
        }

        lastTicks = now;
        return true;
    }

    private static BattleController TryGetCurrentBattle()
    {
        try
        {
            var playBoard = UiManager.GetPanel<PlayBoard>();
            if (playBoard == null)
            {
                return null;
            }

            return Traverse.Create(playBoard).Property("Battle").GetValue<BattleController>();
        }
        catch
        {
            return null;
        }
    }

    private static void TryBroadcastCurrentBattleIntentions()
    {
        try
        {
            INetworkClient client = TryGetNetworkClient();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            BattleController battle = TryGetCurrentBattle();
            if (battle?.EnemyGroup == null)
            {
                return;
            }

            foreach (EnemyUnit enemy in battle.EnemyGroup.Alives)
            {
                if (enemy == null)
                {
                    continue;
                }

                object payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EnemyGroupId = battle.EnemyGroup.Id,
                    Round = battle.RoundCounter,
                    GeneratingEotIntents = _generatingRoundStartIntentions,
                    Enemy = new
                    {
                        enemy.RootIndex,
                        enemy.Id,
                        SpawnId = SpawnedEnemySyncPatch.TryGetSpawnId(enemy, out string spawnId) ? spawnId : null,
                        enemy.Name,
                        enemy.ModelName,
                    },
                    Intentions = BuildIntentionsSnapshot(enemy.Intentions),
                };

                client.SendRequest(NetworkMessageTypes.BattleEnemyIntentChanged, JsonCompat.Serialize(payload));
            }

            Plugin.Logger?.LogDebug("[EnemyIntentSync] Broadcasted current battle intentions for join/reconnect.");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemyIntentSync] Failed to broadcast current battle intentions: {ex.Message}");
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
