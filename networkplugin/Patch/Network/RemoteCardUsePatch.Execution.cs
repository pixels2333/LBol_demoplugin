using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.Presentation.Units;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

public static partial class RemoteCardUsePatch
{
    #region Execution (replay + resolved state)

    private static void ApplyRemoteResolvedStateToView(string playerId, JsonElement root)
    {
        if (!OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView(playerId, out UnitView view))
        {
            return;
        }

        Unit unit = view.Unit;
        if (unit == null)
        {
            return;
        }

        try
        {
            int? maxHp = GetInt(root, "MaxHp");
            int? hp = GetInt(root, "Hp");
            int? block = GetInt(root, "Block");
            int? shield = GetInt(root, "Shield");

            if (maxHp != null) TrySetProperty(unit, "MaxHp", maxHp.Value);
            if (hp != null) TrySetProperty(unit, "Hp", hp.Value);
            if (block != null) TrySetProperty(unit, "Block", block.Value);
            if (shield != null) TrySetProperty(unit, "Shield", shield.Value);
        }
        catch
        {
            // ignored
        }

        try
        {
            if (root.TryGetProperty("StatusEffects", out JsonElement effectsEl) && effectsEl.ValueKind == JsonValueKind.Array)
            {
                ReplaceStatusEffects(unit, effectsEl);
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            unit.NotifyChanged();
        }
        catch
        {
            // ignored
        }

        try
        {
            if (root.TryGetProperty("Effects", out JsonElement effectsEl) && effectsEl.ValueKind == JsonValueKind.Object)
            {
                bool hasDamage = GetBool(effectsEl, "HasDamage") ?? false;
                bool hasStatus = GetBool(effectsEl, "HasStatus") ?? false;
                bool hasHeal = GetBool(effectsEl, "HasHeal") ?? false;

                if (hasDamage)
                {
                    view.PlayAnimation("hit");
                }
                else if (hasStatus || hasHeal)
                {
                    view.PlayAnimation("spell");
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void ReplaceStatusEffects(Unit owner, JsonElement effectsEl)
    {
        try
        {
            OrderedList<StatusEffect> list = Traverse.Create(owner).Field("_statusEffects").GetValue<OrderedList<StatusEffect>>();
            list?.Clear();

            foreach (JsonElement item in effectsEl.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                string effectId = GetString(item, "Id");
                if (string.IsNullOrWhiteSpace(effectId))
                {
                    continue;
                }

                StatusEffect effect = Library.TryCreateStatusEffect(effectId);
                if (effect == null)
                {
                    continue;
                }

                bool hasLevel = GetBool(item, "HasLevel") ?? false;
                bool hasDuration = GetBool(item, "HasDuration") ?? false;
                bool hasCount = GetBool(item, "HasCount") ?? false;

                int level = GetInt(item, "Level") ?? 0;
                int duration = GetInt(item, "Duration") ?? 0;
                int count = GetInt(item, "Count") ?? 0;
                int limit = GetInt(item, "Limit") ?? 0;

                bool autoDecreasing = GetBool(item, "IsAutoDecreasing") ?? true;
                TrySetProperty(effect, "IsAutoDecreasing", autoDecreasing);

                if (hasLevel)
                {
                    TryCall(effect, "SetInitLevel", level);
                }

                if (hasDuration)
                {
                    TryCall(effect, "SetInitDuration", duration);
                }

                if (hasCount)
                {
                    TryCall(effect, "SetInitCount", count);
                }

                if (limit > 0)
                {
                    TrySetProperty(effect, "Limit", limit);
                }

                AddStatusEffectNoStack(owner, effect);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void TryExecuteRemoteCardUse(JsonElement root)
    {
        try
        {
            GameRunController run = GameStateUtils.GetCurrentGameRun();
            BattleController battle = run?.Battle;
            if (battle == null || battle.Player == null)
            {
                return;
            }

            if (!TryGetActionSourceCard(root, battle, out Card actionSourceCard))
            {
                actionSourceCard = null;
            }

            PlayerUnit caster = TryCreateRemoteCaster(root, battle) ?? battle.Player;
            using (EnterRemotePipelineScope())
            {
                List<BattleAction> actions = BuildReplayActions(root, battle, caster);
                if (actions.Count == 0)
                {
                    return;
                }

                InvokeBattleReact(battle, actions, actionSourceCard);
                TryBroadcastResolvedState(root, battle);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RemoteCardUse] Execute failed: {ex.Message}");
        }
    }

    private static void InvokeBattleReact(BattleController battle, List<BattleAction> actions, Card actionSourceCard)
    {
        if (battle == null || actions == null || actions.Count == 0)
        {
            return;
        }

        try
        {
            Reactor reactor = new Reactor(actions);
            GameEntity source = actionSourceCard;

            MethodInfo react = battle.GetType().GetMethod(
                "React",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: new[] { typeof(Reactor), typeof(GameEntity), typeof(ActionCause) },
                modifiers: null);

            if (react != null)
            {
                react.Invoke(battle, new object[] { reactor, source, ActionCause.Card });
                return;
            }

            react = battle.GetType().GetMethod(
                "React",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: new[] { typeof(Reactor) },
                modifiers: null);

            if (react != null)
            {
                react.Invoke(battle, new object[] { reactor });
                return;
            }

            Plugin.Logger?.LogWarning("[RemoteCardUse] BattleController.React not found; remote actions skipped.");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RemoteCardUse] BattleController.React invoke failed: {ex.Message}");
        }
    }

    private static void TryBroadcastResolvedState(JsonElement originalRoot, BattleController battle)
    {
        try
        {
            if (battle?.Player == null)
            {
                return;
            }

            string requestId = GetString(originalRoot, "RequestId");
            long resolveSeq = Interlocked.Increment(ref _resolvedSeq);

            string selfId;
            lock (_syncLock)
            {
                selfId = _selfPlayerId;
            }

            if (string.IsNullOrWhiteSpace(selfId))
            {
                return;
            }

            if (!IsConnected(out INetworkClient client))
            {
                return;
            }

            bool hasDamage = false;
            bool hasHeal = false;
            bool hasStatus = false;
            try
            {
                if (originalRoot.TryGetProperty("Actions", out JsonElement actionsEl) && actionsEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in actionsEl.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Object)
                        {
                            continue;
                        }

                        string kind = GetString(item, "Kind");
                        if (kind == "Damage")
                        {
                            hasDamage = true;
                        }
                        else if (kind == "Heal")
                        {
                            hasHeal = true;
                        }
                        else if (kind == "ApplyStatusEffect")
                        {
                            hasStatus = true;
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            var payload = new
            {
                Timestamp = DateTime.Now.Ticks,
                RequestId = requestId,
                ResolveSeq = resolveSeq,
                EventType = NetworkMessageTypes.OnRemoteCardResolved,
                TargetPlayerId = selfId,
                Hp = battle.Player.Hp,
                MaxHp = battle.Player.MaxHp,
                Block = battle.Player.Block,
                Shield = battle.Player.Shield,
                Status = battle.Player.Status.ToString(),
                StatusEffects = SnapshotStatusEffects(battle.Player),
                Effects = new
                {
                    HasDamage = hasDamage,
                    HasHeal = hasHeal,
                    HasStatus = hasStatus
                }
            };

            client.SendGameEventData(NetworkMessageTypes.OnRemoteCardResolved, payload);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RemoteCardUse] Resolve send failed: {ex.Message}");
        }
    }

    private static bool TryGetActionSourceCard(JsonElement root, BattleController battle, out Card card)
    {
        card = null;
        try
        {
            if (!root.TryGetProperty("Card", out JsonElement cardEl) || cardEl.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            string cardId = GetString(cardEl, "CardId");
            if (string.IsNullOrWhiteSpace(cardId))
            {
                return false;
            }

            bool upgraded = GetBool(cardEl, "IsUpgraded") ?? false;
            int? upgradeCounter = GetInt(cardEl, "UpgradeCounter");

            Card created = Library.TryCreateCard(cardId, upgraded, upgradeCounter);
            if (created == null)
            {
                return false;
            }

            TrySetGameRun(created, battle.GameRun);
            TryEnterBattle(created, battle);

            try
            {
                created.PendingTarget = battle.Player;
            }
            catch
            {
                // ignored
            }

            card = created;
            return true;
        }
        catch
        {
            card = null;
            return false;
        }
    }

    private static PlayerUnit TryCreateRemoteCaster(JsonElement root, BattleController battle)
    {
        try
        {
            string characterId = GetString(root, "SenderCharacterId");
            if (string.IsNullOrWhiteSpace(characterId))
            {
                characterId = battle.Player.Id;
            }

            PlayerUnit unit = Library.TryCreatePlayerUnit(characterId);
            if (unit == null)
            {
                return null;
            }

            TrySetGameRun(unit, battle.GameRun);
            TryEnterBattle(unit, battle);

            if (root.TryGetProperty("SenderStatusEffects", out JsonElement effectsEl) && effectsEl.ValueKind == JsonValueKind.Array)
            {
                ApplyStatusEffectSnapshot(unit, battle, effectsEl);
            }

            return unit;
        }
        catch
        {
            return null;
        }
    }

    private static List<BattleAction> BuildReplayActions(JsonElement root, BattleController battle, PlayerUnit caster)
    {
        List<BattleAction> list = new List<BattleAction>();
        try
        {
            if (!root.TryGetProperty("Actions", out JsonElement actionsEl) || actionsEl.ValueKind != JsonValueKind.Array)
            {
                return list;
            }

            foreach (JsonElement item in actionsEl.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                string kind = GetString(item, "Kind");
                if (string.IsNullOrWhiteSpace(kind))
                {
                    continue;
                }

                switch (kind)
                {
                    case "Damage":
                        TryAddReplayDamage(list, item, caster, battle.Player);
                        break;
                    case "Heal":
                        TryAddReplayHeal(list, item, caster, battle.Player);
                        break;
                    case "ApplyStatusEffect":
                        TryAddReplayStatus(list, item, battle.Player);
                        break;
                }
            }
        }
        catch
        {
            // ignored
        }

        return list;
    }

    private static void TryAddReplayDamage(List<BattleAction> list, JsonElement item, Unit caster, Unit target)
    {
        try
        {
            float? dmg = GetFloat(item, "Damage") ?? GetFloat(item, "Amount");
            if (dmg == null)
            {
                return;
            }

            string typeStr = GetString(item, "DamageType");
            if (!Enum.TryParse(typeStr, out DamageType damageType))
            {
                return;
            }

            bool isAccuracy = GetBool(item, "IsAccuracy") ?? false;
            bool dontBreakPerfect = GetBool(item, "DontBreakPerfect") ?? false;

            DamageInfo info = damageType switch
            {
                DamageType.Attack => DamageInfo.Attack(dmg.Value, isAccuracy),
                DamageType.HpLose => DamageInfo.HpLose(dmg.Value, dontBreakPerfect),
                DamageType.Reaction => DamageInfo.Reaction(dmg.Value, dontBreakPerfect),
                _ => DamageInfo.Attack(dmg.Value, isAccuracy)
            };

            string gunName = GetString(item, "GunName") ?? "Instant";
            string gunTypeStr = GetString(item, "GunType");
            GunType gunType = Enum.TryParse(gunTypeStr, out GunType parsed) ? parsed : GunType.Single;

            list.Add(new DamageAction(caster, target, info, gunName, gunType));
        }
        catch
        {
            // ignored
        }
    }

    private static void TryAddReplayHeal(List<BattleAction> list, JsonElement item, Unit caster, Unit target)
    {
        try
        {
            int? amount = GetInt(item, "Amount");
            if (amount == null)
            {
                float? f = GetFloat(item, "Amount");
                if (f != null)
                {
                    amount = (int)Math.Round(f.Value);
                }
            }

            if (amount == null)
            {
                return;
            }

            string typeStr = GetString(item, "HealType");
            HealType healType = Enum.TryParse(typeStr, out HealType parsed) ? parsed : HealType.Normal;

            float waitTime = GetFloat(item, "WaitTime") ?? 0.2f;
            list.Add(new HealAction(caster, target, amount.Value, healType, waitTime));
        }
        catch
        {
            // ignored
        }
    }

    private static void TryAddReplayStatus(List<BattleAction> list, JsonElement item, Unit target)
    {
        try
        {
            string effectId = GetString(item, "EffectId");
            if (string.IsNullOrWhiteSpace(effectId))
            {
                return;
            }

            StatusEffect effect = Library.TryCreateStatusEffect(effectId);
            if (effect == null)
            {
                return;
            }

            int? level = GetInt(item, "Level");
            int? duration = GetInt(item, "Duration");
            int? count = GetInt(item, "Count");
            int? limit = GetInt(item, "Limit");
            float waitTime = GetFloat(item, "WaitTime") ?? 0f;
            bool startAutoDecreasing = GetBool(item, "StartAutoDecreasing") ?? true;

            list.Add(new ApplyStatusEffectAction(effect.GetType(), target, level, duration, count, limit, waitTime, startAutoDecreasing));
        }
        catch
        {
            // ignored
        }
    }

    private static void ApplyStatusEffectSnapshot(Unit owner, BattleController battle, JsonElement effectsEl)
    {
        try
        {
            foreach (JsonElement item in effectsEl.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                string effectId = GetString(item, "Id");
                if (string.IsNullOrWhiteSpace(effectId))
                {
                    continue;
                }

                StatusEffect effect = Library.TryCreateStatusEffect(effectId);
                if (effect == null)
                {
                    continue;
                }

                TrySetGameRun(effect, battle.GameRun);

                bool hasLevel = GetBool(item, "HasLevel") ?? false;
                bool hasDuration = GetBool(item, "HasDuration") ?? false;
                bool hasCount = GetBool(item, "HasCount") ?? false;

                int level = GetInt(item, "Level") ?? 0;
                int duration = GetInt(item, "Duration") ?? 0;
                int count = GetInt(item, "Count") ?? 0;
                int limit = GetInt(item, "Limit") ?? 0;

                bool autoDecreasing = GetBool(item, "IsAutoDecreasing") ?? true;
                TrySetProperty(effect, "IsAutoDecreasing", autoDecreasing);

                if (hasLevel)
                {
                    TryCall(effect, "SetInitLevel", level);
                }

                if (hasDuration)
                {
                    TryCall(effect, "SetInitDuration", duration);
                }

                if (hasCount)
                {
                    TryCall(effect, "SetInitCount", count);
                }

                if (limit > 0)
                {
                    TrySetProperty(effect, "Limit", limit);
                }

                AddStatusEffectNoStack(owner, effect);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void AddStatusEffectNoStack(Unit owner, StatusEffect effect)
    {
        try
        {
            if (owner == null || effect == null)
            {
                return;
            }

            if (owner.HasStatusEffect(effect.GetType()))
            {
                return;
            }

            TrySetProperty(effect, "Owner", owner);
            TryCall(effect, "TriggerAdding", owner);
            TryCall(effect, "ClampMax");

            OrderedList<StatusEffect> list = Traverse.Create(owner).Field("_statusEffects").GetValue<OrderedList<StatusEffect>>();
            list?.Add(effect);

            TryCall(effect, "TriggerAdded", owner);
        }
        catch
        {
            // ignored
        }
    }

    private static void TrySetGameRun(GameEntity entity, GameRunController gameRun)
    {
        try
        {
            Traverse.Create(entity).Property("GameRun").SetValue(gameRun);
        }
        catch
        {
            // ignored
        }
    }

    private static void TryEnterBattle(object entity, BattleController battle)
    {
        try
        {
            Traverse.Create(entity).Method("EnterBattle", battle).GetValue();
        }
        catch
        {
            // ignored
        }
    }

    private static void TryCall(object instance, string methodName, params object[] args)
    {
        try
        {
            Traverse.Create(instance).Method(methodName, args).GetValue();
        }
        catch
        {
            // ignored
        }
    }

    private static void TrySetProperty(object instance, string propertyName, object value)
    {
        try
        {
            Traverse.Create(instance).Property(propertyName).SetValue(value);
        }
        catch
        {
            // ignored
        }
    }

    #endregion
}
