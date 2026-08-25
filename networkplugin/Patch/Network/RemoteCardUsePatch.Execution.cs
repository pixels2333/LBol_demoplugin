using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
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

            // 根据 TargetUnitKind 选择伤害目标：Enemy 时从 EnemyGroup 按 Id 查找，否则用本地玩家。
            Unit targetUnit = ResolveTargetUnit(root, battle);

            using (EnterRemotePipelineScope())
            {
                List<BattleAction> actions = BuildReplayActions(root, battle, caster, targetUnit);
                if (actions.Count == 0)
                {
                    Plugin.Logger?.LogWarning("[RemoteCardUse] BuildReplayActions 返回空列表，跳过执行");
                    return;
                }

                Plugin.Logger?.LogInfo($"[RemoteCardUse] 准备执行: caster={(caster == null ? "null" : caster.GetType().Name)}, target={(targetUnit == null ? "null" : targetUnit.GetType().Name)}, actions={actions.Count}, card={(actionSourceCard == null ? "null" : actionSourceCard.Id)}");
                InvokeBattleReact(battle, actions, actionSourceCard, () =>
                {
                    TryBroadcastResolvedState(root, battle);
                });
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RemoteCardUse] Execute failed: {ex.Message}");
        }
    }

    private sealed class CallbackAction : LBoL.Core.Battle.SimpleAction
    {
        private readonly Action _callback;
        public CallbackAction(Action callback) => _callback = callback;
        protected override void ResolvePhase() => _callback?.Invoke();
    }

    // 根据载荷中 TargetUnitKind 选择效果目标。
    // "Enemy"：按 TargetPlayerId 从 EnemyGroup 查找匹配 EnemyUnit（先按 Id，再按 RootIndex）。
    // 默认（"Player" 或缺失）：返回 battle.Player（本地玩家自己承受效果）。
    private static Unit ResolveTargetUnit(JsonElement root, BattleController battle)
    {
        string targetUnitKind = NetworkEventHelper.GetString(root, "TargetUnitKind");
        if (!string.Equals(targetUnitKind, "Enemy", StringComparison.OrdinalIgnoreCase))
        {
            return battle.Player;
        }

        string targetId = NetworkEventHelper.GetString(root, "TargetPlayerId");
        if (string.IsNullOrWhiteSpace(targetId) || battle.EnemyGroup == null)
        {
            return battle.Player;
        }

        // 优先按 Id 精确匹配。
        foreach (EnemyUnit enemy in battle.EnemyGroup)
        {
            if (enemy != null && !string.IsNullOrWhiteSpace(enemy.Id) &&
                string.Equals(enemy.Id, targetId, StringComparison.Ordinal))
            {
                return enemy;
            }
        }

        // 兜底：按 RootIndex（数字字符串）匹配。
        if (int.TryParse(targetId, out int rootIndex))
        {
            EnemyUnit byIndex = battle.GetEnemyByRootIndex(rootIndex);
            if (byIndex != null)
            {
                return byIndex;
            }
        }

        Plugin.Logger?.LogWarning($"[RemoteCardUse] Enemy target not found: {targetId}, fallback to battle.Player.");
        return battle.Player;
    }

    private static void InvokeBattleReact(BattleController battle, List<BattleAction> actions, Card actionSourceCard, Action onComplete = null)
    {
        if (battle == null || actions == null || actions.Count == 0)
        {
            return;
        }

        try
        {
            // BattleController.React(Reactor, GameEntity, ActionCause) 要求当前处于 action 解析上下文
            //（_resolver._reactors != null），在网络回调中直接调用会抛 InvalidOperationException:
            // "Reacting out of action-resolving status"。
            // 正确入口是 public RequestDebugAction(BattleAction, string)：将 action 入队 _debugActionQueue，
            // 由战斗协程在 ResolveAction→ResolveDebugActions 中消费执行。
            // 逐个入队：Queue 是 FIFO，ResolveDebugActions 会依次 resolve 每个 action。
            string recordPrefix = "RemoteCard";
            int i = 0;
            foreach (BattleAction action in actions)
            {
                if (action == null)
                {
                    continue;
                }

                // 绑定卡牌来源（可能为 null，SetSource(null) 安全）和出牌原因。
                action.SetSource(actionSourceCard).SetCause(ActionCause.Card);

                string recordName = $"{recordPrefix}:{action.GetType().Name}:{i}";
                battle.RequestDebugAction(action, recordName);
                i++;
            }

            if (onComplete != null)
            {
                battle.RequestDebugAction(new CallbackAction(onComplete), $"{recordPrefix}:ResolvedCallback");
            }

            Plugin.Logger?.LogInfo($"[RemoteCardUse] 已入队 {i} 个 debug action 等待战斗协程执行");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RemoteCardUse] RequestDebugAction enqueue failed: {ex.Message}\n{ex.StackTrace}");
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
            string senderId = NetworkEventHelper.GetString(root, "SenderPlayerId");
            if (!string.IsNullOrWhiteSpace(senderId))
            {
                if (OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView(senderId, out UnitView casterView) && casterView?.Unit is PlayerUnit playerUnit)
                {
                    if (root.TryGetProperty("SenderStatusEffects", out JsonElement effectsEl2) && effectsEl2.ValueKind == JsonValueKind.Array)
                    {
                        ApplyStatusEffectSnapshot(playerUnit, battle, effectsEl2);
                    }
                    return playerUnit;
                }
            }

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

    private static List<BattleAction> BuildReplayActions(JsonElement root, BattleController battle, PlayerUnit caster, Unit targetUnit)
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
                        TryAddReplayDamage(list, item, caster, targetUnit, battle);
                        break;
                    case "Heal":
                        TryAddReplayHeal(list, item, caster, targetUnit, battle);
                        break;
                    case "ApplyStatusEffect":
                        TryAddReplayStatus(list, item, targetUnit, battle);
                        break;
                    case "PerformAction":
                        var pa = ReconstructPerformAction(item, battle);
                        if (pa != null)
                        {
                            list.Add(pa);
                        }
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

    private static void TryAddReplayDamage(List<BattleAction> list, JsonElement item, Unit caster, Unit target, BattleController battle)
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

            Unit actionCaster = ResolveUnit(item.TryGetProperty("Caster", out JsonElement cEl) ? cEl : default, battle) ?? caster;
            var targets = new List<Unit>();
            if (item.TryGetProperty("Targets", out JsonElement targetsEl) && targetsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement tEl in targetsEl.EnumerateArray())
                {
                    Unit t = ResolveUnit(tEl, battle);
                    if (t != null)
                    {
                        targets.Add(t);
                    }
                }
            }
            if (targets.Count == 0)
            {
                targets.Add(target);
            }

            list.Add(new DamageAction(actionCaster, targets, info, gunName, gunType));
        }
        catch
        {
            // ignored
        }
    }

    private static void TryAddReplayHeal(List<BattleAction> list, JsonElement item, Unit caster, Unit target, BattleController battle)
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

            Unit actionCaster = ResolveUnit(item.TryGetProperty("Caster", out JsonElement cEl) ? cEl : default, battle) ?? caster;
            Unit actionTarget = ResolveUnit(item.TryGetProperty("Target", out JsonElement tEl) ? tEl : default, battle) ?? target;

            list.Add(new HealAction(actionCaster, actionTarget, amount.Value, healType, waitTime));
        }
        catch
        {
            // ignored
        }
    }

    private static void TryAddReplayStatus(List<BattleAction> list, JsonElement item, Unit target, BattleController battle)
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

            Unit actionTarget = ResolveUnit(item.TryGetProperty("Target", out JsonElement tEl) ? tEl : default, battle) ?? target;

            list.Add(new ApplyStatusEffectAction(effect.GetType(), actionTarget, level, duration, count, limit, waitTime, startAutoDecreasing));
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

    // Helper to resolve a Unit from serialized JSON representation
    private static Unit ResolveUnit(JsonElement elem, BattleController battle)
    {
        if (elem.ValueKind != JsonValueKind.Object) return null;
        string kind = GetString(elem, "Kind");
        if (kind == "Player")
        {
            string playerId = GetString(elem, "PlayerId");
            if (string.IsNullOrWhiteSpace(playerId)) return battle.Player;

            string selfId;
            lock (_syncLock)
            {
                selfId = _selfPlayerId;
            }
            if (string.Equals(playerId, selfId, StringComparison.Ordinal) || playerId == "__local__")
            {
                return battle.Player;
            }

            if (OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView(playerId, out UnitView view) && view?.Unit != null)
            {
                return view.Unit;
            }

            return battle.Player;
        }
        else if (kind == "Enemy")
        {
            string enemyId = GetString(elem, "EnemyId");
            int? rootIndex = GetInt(elem, "RootIndex");
            if (battle.EnemyGroup == null) return null;

            foreach (EnemyUnit enemy in battle.EnemyGroup)
            {
                if (enemy != null && string.Equals(enemy.Id, enemyId, StringComparison.Ordinal))
                {
                    if (rootIndex == null || enemy.RootIndex == rootIndex.Value)
                    {
                        return enemy;
                    }
                }
            }
            if (rootIndex != null)
            {
                EnemyUnit byIndex = battle.GetEnemyByRootIndex(rootIndex.Value);
                if (byIndex != null) return byIndex;
            }
            return null;
        }
        return null;
    }

    // Helper to reconstruct PerformAction subclasses from JSON
    private static PerformAction ReconstructPerformAction(JsonElement elem, BattleController battle)
    {
        string type = GetString(elem, "Type");
        switch (type)
        {
            case "ViewCard":
                {
                    string cardId = GetString(elem, "CardId");
                    string zoneStr = GetString(elem, "Zone");
                    Card card = Library.TryCreateCard(cardId, false, null);
                    if (card != null && Enum.TryParse(zoneStr, out CardZone zone))
                    {
                        TrySetGameRun(card, battle.GameRun);
                        TryEnterBattle(card, battle);
                        return PerformAction.ViewCard(card);
                    }
                    break;
                }
            case "Gun":
                {
                    Unit source = ResolveUnit(elem.GetProperty("Source"), battle);
                    Unit target = ResolveUnit(elem.GetProperty("Target"), battle);
                    string gunId = GetString(elem, "GunId");
                    float waitTime = GetFloat(elem, "WaitTime") ?? 0f;
                    if (source != null && target != null)
                    {
                        return PerformAction.Gun(source, target, gunId, waitTime);
                    }
                    break;
                }
            case "Doll":
                {
                    Unit target = ResolveUnit(elem.GetProperty("Target"), battle);
                    string gunId = GetString(elem, "GunId");
                    float waitTime = GetFloat(elem, "WaitTime") ?? 0f;
                    string debugString = GetString(elem, "DebugString") ?? "";
                    if (target != null)
                    {
                        return PerformAction.Doll(null, target, gunId, waitTime, debugString);
                    }
                    break;
                }
            case "Animation":
                {
                    Unit source = ResolveUnit(elem.GetProperty("Source"), battle);
                    string animationName = GetString(elem, "AnimationName");
                    float waitTime = GetFloat(elem, "WaitTime") ?? 0f;
                    string sfxId = GetString(elem, "SfxId");
                    float sfxDelay = GetFloat(elem, "SfxDelay") ?? 0f;
                    int shakeLevel = GetInt(elem, "ShakeLevel") ?? -1;
                    if (source != null)
                    {
                        return PerformAction.Animation(source, animationName, waitTime, sfxId, sfxDelay, shakeLevel);
                    }
                    break;
                }
            case "Sfx":
                {
                    string id = GetString(elem, "Id");
                    float delay = GetFloat(elem, "Delay") ?? 0f;
                    return PerformAction.Sfx(id, delay);
                }
            case "UiSound":
                {
                    string id = GetString(elem, "Id");
                    return PerformAction.UiSound(id);
                }
            case "Chat":
                {
                    Unit source = ResolveUnit(elem.GetProperty("Source"), battle);
                    string content = GetString(elem, "Content");
                    float chatTime = GetFloat(elem, "ChatTime") ?? 2f;
                    float delay = GetFloat(elem, "Delay") ?? 0f;
                    float waitTime = GetFloat(elem, "WaitTime") ?? 0f;
                    bool talk = GetBool(elem, "Talk") ?? true;
                    if (source != null)
                    {
                        return PerformAction.Chat(source, content, chatTime, delay, waitTime, talk);
                    }
                    break;
                }
            case "Spell":
                {
                    Unit source = ResolveUnit(elem.GetProperty("Source"), battle);
                    string spellName = GetString(elem, "SpellName");
                    if (source != null)
                    {
                        return PerformAction.Spell(source, spellName);
                    }
                    break;
                }
            case "Effect":
                {
                    Unit source = ResolveUnit(elem.GetProperty("Source"), battle);
                    string effectName = GetString(elem, "EffectName");
                    float delay = GetFloat(elem, "Delay") ?? 0f;
                    float waitTime = GetFloat(elem, "WaitTime") ?? 0f;
                    string sfxId = GetString(elem, "SfxId");
                    float sfxDelay = GetFloat(elem, "SfxDelay") ?? 0f;
                    string effectTypeStr = GetString(elem, "EffectType");
                    if (source != null && Enum.TryParse(effectTypeStr, out PerformAction.EffectBehavior effectType))
                    {
                        return PerformAction.Effect(source, effectName, delay, sfxId, sfxDelay, effectType, waitTime);
                    }
                    break;
                }
            case "EffectMessage":
                {
                    Unit source = ResolveUnit(elem.GetProperty("Source"), battle);
                    string effectName = GetString(elem, "EffectName");
                    string message = GetString(elem, "Message");
                    if (source != null)
                    {
                        return PerformAction.EffectMessage(source, effectName, message, null);
                    }
                    break;
                }
            case "SePop":
                {
                    Unit source = ResolveUnit(elem.GetProperty("Source"), battle);
                    string popContent = GetString(elem, "PopContent");
                    if (source != null)
                    {
                        return PerformAction.SePop(source, popContent);
                    }
                    break;
                }
            case "SummonFriend":
                {
                    string cardId = GetString(elem, "CardId");
                    Card card = Library.TryCreateCard(cardId, false, null);
                    if (card != null)
                    {
                        TrySetGameRun(card, battle.GameRun);
                        TryEnterBattle(card, battle);
                        return PerformAction.SummonFriend(card);
                    }
                    break;
                }
            case "TransformModel":
                {
                    Unit source = ResolveUnit(elem.GetProperty("Source"), battle);
                    string modelName = GetString(elem, "ModelName");
                    if (source != null)
                    {
                        return PerformAction.TransformModel(source, modelName);
                    }
                    break;
                }
            case "DeathAnimation":
                {
                    Unit source = ResolveUnit(elem.GetProperty("Source"), battle);
                    if (source != null)
                    {
                        return PerformAction.DeathAnimation(source);
                    }
                    break;
                }
            case "Wait":
                {
                    float time = GetFloat(elem, "Time") ?? 0f;
                    bool unscale = GetBool(elem, "Unscale") ?? false;
                    return PerformAction.Wait(time, unscale);
                }
        }
        return null;
    }

    // Helper to resolve a UnitView from Unit or serialized JSON representation, supporting remote player UnitViews
    private static UnitView ResolveUnitView(Unit unit, JsonElement elem, BattleController battle, string defaultPlayerId = null)
    {
        try
        {
            // 1. 如果有 Unit 实例
            if (unit != null)
            {
                if (Singleton<GameDirector>.Instance?.PlayerUnitView != null && unit == Singleton<GameDirector>.Instance.PlayerUnitView.Unit)
                {
                    return Singleton<GameDirector>.Instance.PlayerUnitView;
                }

                if (unit is EnemyUnit eu)
                {
                    UnitView ev = GameDirector.GetEnemy(eu);
                    if (ev != null) return ev;
                }

                foreach (UnitView remote in OtherPlayersOverlayPatch.SnapshotRemoteCharacterUnitViews())
                {
                    if (remote != null && remote.Unit == unit)
                    {
                        return remote;
                    }
                }
            }

            // 2. 从 JSON 载荷解析
            if (elem.ValueKind == JsonValueKind.Object)
            {
                string kind = GetString(elem, "Kind");
                if (kind == "Player")
                {
                    string playerId = GetString(elem, "PlayerId");
                    if (string.IsNullOrWhiteSpace(playerId))
                    {
                        playerId = defaultPlayerId;
                    }

                    if (!string.IsNullOrWhiteSpace(playerId))
                    {
                        string selfId;
                        lock (_syncLock)
                        {
                            selfId = _selfPlayerId;
                        }
                        if (string.Equals(playerId, selfId, StringComparison.Ordinal) || playerId == "__local__")
                        {
                            return Singleton<GameDirector>.Instance?.PlayerUnitView;
                        }

                        if (OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView(playerId, out UnitView remoteView) && remoteView != null)
                        {
                            return remoteView;
                        }
                    }
                }
                else if (kind == "Enemy")
                {
                    string enemyId = GetString(elem, "EnemyId");
                    int? rootIndex = GetInt(elem, "RootIndex");
                    if (rootIndex != null)
                    {
                        UnitView ev = GameDirector.GetEnemyByRootIndex(rootIndex.Value);
                        if (ev != null) return ev;
                    }
                    if (battle?.EnemyGroup != null && !string.IsNullOrWhiteSpace(enemyId))
                    {
                        foreach (EnemyUnit e in battle.EnemyGroup)
                        {
                            if (e != null && e.Id == enemyId)
                            {
                                UnitView ev = GameDirector.GetEnemy(e);
                                if (ev != null) return ev;
                            }
                        }
                    }
                }
            }

            // 3. Fallback: 使用 defaultPlayerId 查找远程玩家
            if (!string.IsNullOrWhiteSpace(defaultPlayerId))
            {
                if (OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView(defaultPlayerId, out UnitView remoteView) && remoteView != null)
                {
                    return remoteView;
                }
            }

            return Singleton<GameDirector>.Instance?.PlayerUnitView;
        }
        catch
        {
            return Singleton<GameDirector>.Instance?.PlayerUnitView;
        }
    }

    private static UnitView ResolveTargetUnitView(Unit targetUnit, JsonElement targetElem, BattleController battle, UnitView sourceView)
    {
        try
        {
            UnitView tv = ResolveUnitView(targetUnit, targetElem, battle, null);
            if (tv != null && tv != sourceView)
            {
                return tv;
            }

            // 智能敌方目标兜底：如果本地战斗有敌方单位，选择第一个存活的敌人
            if (GameDirector.Enemies != null)
            {
                foreach (UnitView enemyView in GameDirector.Enemies)
                {
                    if (enemyView != null && enemyView.Unit != null && enemyView.Unit.IsAlive && enemyView.gameObject.activeInHierarchy)
                    {
                        return enemyView;
                    }
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    // Play visuals sequentially on non-executing client via a coroutine
    public static System.Collections.IEnumerator PlayVisualsCoroutine(JsonElement actionsEl, BattleController battle, bool skipStateVisuals, string defaultSenderPlayerId = null)
    {
        if (actionsEl.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (JsonElement actionEl in actionsEl.EnumerateArray())
        {
            if (actionEl.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            string kind = GetString(actionEl, "Kind");
            if (kind == "PerformAction")
            {
                string type = GetString(actionEl, "Type");
                yield return PlayPerformActionVisual(type, actionEl, battle, defaultSenderPlayerId);
            }
            else if (kind == "Damage" && !skipStateVisuals)
            {
                yield return PlayDamageVisual(actionEl, battle, defaultSenderPlayerId);
            }
            else if (kind == "Heal" && !skipStateVisuals)
            {
                yield return PlayHealVisual(actionEl, battle, defaultSenderPlayerId);
            }
            else if (kind == "ApplyStatusEffect" && !skipStateVisuals)
            {
                yield return PlayStatusEffectVisual(actionEl, battle, defaultSenderPlayerId);
            }
        }
    }

    private static System.Collections.IEnumerator PlayPerformActionVisual(string type, JsonElement elem, BattleController battle, string defaultSenderPlayerId = null)
    {
        switch (type)
        {
            case "ViewCard":
                {
                    string cardId = GetString(elem, "CardId");
                    string zoneStr = GetString(elem, "Zone");
                    Card card = Library.TryCreateCard(cardId, false, null);
                    if (card != null && Enum.TryParse(zoneStr, out CardZone zone))
                    {
                        yield return LBoL.Presentation.UI.UiManager.GetPanel<LBoL.Presentation.UI.Panels.PlayBoard>().CardUi.ViewCardFromZone(card, zone);
                    }
                    break;
                }
            case "Gun":
                {
                    Unit source = elem.TryGetProperty("Source", out JsonElement sEl) ? ResolveUnit(sEl, battle) : null;
                    Unit target = elem.TryGetProperty("Target", out JsonElement tEl) ? ResolveUnit(tEl, battle) : null;
                    string gunId = GetString(elem, "GunId");
                    float waitTime = GetFloat(elem, "WaitTime") ?? 0f;
                    UnitView sourceView = ResolveUnitView(source, sEl, battle, defaultSenderPlayerId);
                    UnitView targetView = ResolveTargetUnitView(target, tEl, battle, sourceView);
                    if (sourceView != null && !string.IsNullOrEmpty(gunId) && gunId != "Empty")
                    {
                        if (targetView != null)
                        {
                            sourceView.Target = targetView;
                            sourceView.Targets.Clear();
                            sourceView.Targets.Add(targetView);
                            targetView.ComingDamage = DamageInfo.Attack(0f, false);
                        }
                        sourceView.PerformShoot(gunId);
                        yield return new UnityEngine.WaitForSeconds(waitTime);
                    }
                    break;
                }
            case "Doll":
                {
                    float waitTime = GetFloat(elem, "WaitTime") ?? 0f;
                    yield return new UnityEngine.WaitForSeconds(waitTime);
                    break;
                }
            case "Animation":
                {
                    Unit source = elem.TryGetProperty("Source", out JsonElement sEl) ? ResolveUnit(sEl, battle) : null;
                    string animationName = GetString(elem, "AnimationName");
                    float waitTime = GetFloat(elem, "WaitTime") ?? 0f;
                    string sfxId = GetString(elem, "SfxId");
                    float sfxDelay = GetFloat(elem, "SfxDelay") ?? 0f;
                    int shakeLevel = GetInt(elem, "ShakeLevel") ?? -1;

                    if (!string.IsNullOrEmpty(sfxId))
                    {
                        LBoL.Presentation.AudioManager.PlaySfxDelay(sfxId, sfxDelay);
                    }
                    if (shakeLevel >= 0)
                    {
                        GameDirector.Shake(shakeLevel, true);
                    }
                    UnitView sourceView = ResolveUnitView(source, sEl, battle, defaultSenderPlayerId);
                    if (sourceView != null && !string.IsNullOrEmpty(animationName))
                    {
                        try
                        {
                            sourceView.PlayAnimation(animationName);
                        }
                        catch
                        {
                            try { sourceView.PlayAnimation("spell"); } catch { }
                        }
                        yield return new UnityEngine.WaitForSeconds(waitTime);
                    }
                    break;
                }
            case "Sfx":
                {
                    string sfxId = GetString(elem, "Id");
                    float delay = GetFloat(elem, "Delay") ?? 0f;
                    if (!string.IsNullOrEmpty(sfxId))
                    {
                        LBoL.Presentation.AudioManager.PlaySfxDelay(sfxId, delay);
                    }
                    break;
                }
            case "UiSound":
                {
                    string sfxId = GetString(elem, "Id");
                    if (!string.IsNullOrEmpty(sfxId))
                    {
                        LBoL.Presentation.AudioManager.PlayUi(sfxId, false);
                    }
                    break;
                }
            case "Chat":
                {
                    Unit source = elem.TryGetProperty("Source", out JsonElement sEl) ? ResolveUnit(sEl, battle) : null;
                    string content = GetString(elem, "Content");
                    float chatTime = GetFloat(elem, "ChatTime") ?? 2f;
                    float delay = GetFloat(elem, "Delay") ?? 0f;
                    float waitTime = GetFloat(elem, "WaitTime") ?? 0f;
                    bool talk = GetBool(elem, "Talk") ?? true;
                    UnitView sourceView = ResolveUnitView(source, sEl, battle, defaultSenderPlayerId);
                    if (sourceView != null && sourceView.Unit != null && sourceView.Unit.IsAlive && !sourceView.IsHidden)
                    {
                        sourceView.Chat(content, chatTime, talk ? ((sourceView == GameDirector.Player) ? LBoL.Presentation.UI.Widgets.ChatWidget.CloudType.LeftTalk : LBoL.Presentation.UI.Widgets.ChatWidget.CloudType.RightTalk) : ((sourceView == GameDirector.Player) ? LBoL.Presentation.UI.Widgets.ChatWidget.CloudType.LeftThink : LBoL.Presentation.UI.Widgets.ChatWidget.CloudType.RightThink), delay);
                        yield return new UnityEngine.WaitForSecondsRealtime(waitTime);
                    }
                    break;
                }
            case "Spell":
                {
                    Unit source = elem.TryGetProperty("Source", out JsonElement sEl) ? ResolveUnit(sEl, battle) : null;
                    string spellName = GetString(elem, "SpellName");
                    UnitView sourceView = ResolveUnitView(source, sEl, battle, defaultSenderPlayerId);
                    if (sourceView != null && !string.IsNullOrEmpty(spellName))
                    {
                        yield return sourceView.SpellDeclare(spellName);
                    }
                    break;
                }
            case "Effect":
                {
                    Unit source = elem.TryGetProperty("Source", out JsonElement sEl) ? ResolveUnit(sEl, battle) : null;
                    string effectName = GetString(elem, "EffectName");
                    float delay = GetFloat(elem, "Delay") ?? 0f;
                    float waitTime = GetFloat(elem, "WaitTime") ?? 0f;
                    string sfxId = GetString(elem, "SfxId");
                    float sfxDelay = GetFloat(elem, "SfxDelay") ?? 0f;
                    string effectTypeStr = GetString(elem, "EffectType");
                    UnitView sourceView = ResolveUnitView(source, sEl, battle, defaultSenderPlayerId);
                    if (sourceView != null && !string.IsNullOrEmpty(effectName))
                    {
                        if (Enum.TryParse(effectTypeStr, out PerformAction.EffectBehavior effectType))
                        {
                            switch (effectType)
                            {
                                case PerformAction.EffectBehavior.PlayOneShot:
                                    sourceView.PlayEffectOneShot(effectName, delay);
                                    break;
                                case PerformAction.EffectBehavior.Add:
                                    sourceView.PlayEffectLoop(effectName);
                                    break;
                                case PerformAction.EffectBehavior.Remove:
                                    sourceView.EndEffectLoop(effectName, true);
                                    break;
                                case PerformAction.EffectBehavior.DieOut:
                                    sourceView.EndEffectLoop(effectName, false);
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(sfxId))
                        {
                            LBoL.Presentation.AudioManager.PlaySfxDelay(sfxId, sfxDelay);
                        }
                        yield return new UnityEngine.WaitForSeconds(waitTime);
                    }
                    break;
                }
            case "EffectMessage":
                {
                    Unit source = elem.TryGetProperty("Source", out JsonElement sEl) ? ResolveUnit(sEl, battle) : null;
                    string effectName = GetString(elem, "EffectName");
                    string message = GetString(elem, "Message");
                    UnitView sourceView = ResolveUnitView(source, sEl, battle, defaultSenderPlayerId);
                    if (sourceView != null && !string.IsNullOrEmpty(effectName))
                    {
                        sourceView.SendEffectMessage(effectName, message, null);
                    }
                    break;
                }
            case "SePop":
                {
                    Unit source = elem.TryGetProperty("Source", out JsonElement sEl) ? ResolveUnit(sEl, battle) : null;
                    string popContent = GetString(elem, "PopContent");
                    UnitView sourceView = ResolveUnitView(source, sEl, battle, defaultSenderPlayerId);
                    if (sourceView != null && sourceView.Unit != null && sourceView.Unit.IsAlive && !string.IsNullOrEmpty(popContent))
                    {
                        sourceView.ShowSePopup(popContent, StatusEffectType.Positive, 0, LBoL.Presentation.UI.Widgets.UnitInfoWidget.SePopType.Amulet);
                    }
                    break;
                }
            case "SummonFriend":
                {
                    string cardId = GetString(elem, "CardId");
                    Card card = Library.TryCreateCard(cardId, false, null);
                    if (card != null)
                    {
                        LBoL.Presentation.UI.UiManager.GetPanel<LBoL.Presentation.UI.Panels.PlayBoard>().CardUi.PlaySummonEffect(card);
                        yield return new UnityEngine.WaitForSeconds(0.5f);
                    }
                    break;
                }
            case "TransformModel":
                {
                    Unit source = elem.TryGetProperty("Source", out JsonElement sEl) ? ResolveUnit(sEl, battle) : null;
                    string modelName = GetString(elem, "ModelName");
                    UnitView sourceView = ResolveUnitView(source, sEl, battle, defaultSenderPlayerId);
                    if (sourceView != null && !string.IsNullOrEmpty(modelName))
                    {
                        yield return sourceView.LoadUnitModelAsync(modelName, false, null);
                    }
                    break;
                }
            case "DeathAnimation":
                {
                    Unit source = elem.TryGetProperty("Source", out JsonElement sEl) ? ResolveUnit(sEl, battle) : null;
                    UnitView sourceView = ResolveUnitView(source, sEl, battle, defaultSenderPlayerId);
                    if (sourceView != null)
                    {
                        sourceView.DeathAnimation();
                    }
                    break;
                }
            case "Wait":
                {
                    float time = GetFloat(elem, "Time") ?? 0f;
                    bool unscale = GetBool(elem, "Unscale") ?? false;
                    if (unscale)
                    {
                        yield return new UnityEngine.WaitForSecondsRealtime(time);
                    }
                    else
                    {
                        yield return new UnityEngine.WaitForSeconds(time);
                    }
                    break;
                }
        }
    }

    private static System.Collections.IEnumerator PlayDamageVisual(JsonElement actionEl, BattleController battle, string defaultSenderPlayerId = null)
    {
        Unit source = actionEl.TryGetProperty("Caster", out JsonElement cEl) ? ResolveUnit(cEl, battle) : null;
        UnitView sourceView = ResolveUnitView(source, cEl, battle, defaultSenderPlayerId);
        if (sourceView == null)
        {
            yield break;
        }

        var targets = new List<Unit>();
        if (actionEl.TryGetProperty("Targets", out JsonElement targetsEl) && targetsEl.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement tEl in targetsEl.EnumerateArray())
            {
                Unit t = ResolveUnit(tEl, battle);
                if (t != null) targets.Add(t);
            }
        }

        string gunName = GetString(actionEl, "GunName") ?? "Instant";
        string gunTypeStr = GetString(actionEl, "GunType");
        GunType gunType = Enum.TryParse(gunTypeStr, out GunType parsed) ? parsed : GunType.Single;

        var targetViews = new List<UnitView>();
        foreach (Unit t in targets)
        {
            UnitView tv = ResolveUnitView(t, default, battle, null);
            if (tv != null) targetViews.Add(tv);
        }

        if (targetViews.Count == 0)
        {
            UnitView fallbackTarget = ResolveTargetUnitView(null, default, battle, sourceView);
            if (fallbackTarget != null)
            {
                targetViews.Add(fallbackTarget);
            }
        }

        if (targetViews.Count > 0 && !string.IsNullOrEmpty(gunName) && gunName != "Empty" && gunName != "Instant")
        {
            var pairs = new List<ValueTuple<UnitView, DamageInfo>>();
            foreach (UnitView tv in targetViews)
            {
                pairs.Add(new ValueTuple<UnitView, DamageInfo>(tv, DamageInfo.Attack(0f, false)));
            }

            var method = Traverse.Create(typeof(GameDirector)).Method("GunShootAction", sourceView, pairs, gunName, gunType);
            if (method.MethodExists())
            {
                yield return (System.Collections.IEnumerator)method.GetValue();
            }
            else
            {
                sourceView.PerformShoot(gunName);
            }
        }
        else if (!string.IsNullOrEmpty(gunName) && gunName != "Empty" && gunName != "Instant")
        {
            sourceView.PerformShoot(gunName);
        }
        else
        {
            foreach (UnitView tv in targetViews)
            {
                tv?.PlayAnimation("hit");
            }
        }
    }

    private static System.Collections.IEnumerator PlayHealVisual(JsonElement actionEl, BattleController battle, string defaultSenderPlayerId = null)
    {
        Unit target = actionEl.TryGetProperty("Target", out JsonElement tEl) ? ResolveUnit(tEl, battle) : null;
        UnitView targetView = ResolveUnitView(target, tEl, battle, defaultSenderPlayerId);
        if (targetView == null)
        {
            yield break;
        }

        int amount = GetInt(actionEl, "Amount") ?? 0;
        bool large = amount > 12;

        try
        {
            LBoL.Presentation.Effect.EffectManager.CreateEffect(large ? "UnitHealLarge" : "UnitHeal", targetView.EffectRoot, true);
            LBoL.Presentation.AudioManager.PlayUi(large ? "HealLarge" : "Heal", false);
        }
        catch
        {
            // ignored
        }
        yield return new UnityEngine.WaitForSeconds(0.2f);
    }

    private static System.Collections.IEnumerator PlayStatusEffectVisual(JsonElement actionEl, BattleController battle, string defaultSenderPlayerId = null)
    {
        Unit target = actionEl.TryGetProperty("Target", out JsonElement tEl) ? ResolveUnit(tEl, battle) : null;
        UnitView targetView = ResolveUnitView(target, tEl, battle, defaultSenderPlayerId);
        if (targetView == null)
        {
            yield break;
        }

        string effectId = GetString(actionEl, "EffectId");
        if (string.IsNullOrEmpty(effectId))
        {
            yield break;
        }

        StatusEffect effect = Library.TryCreateStatusEffect(effectId);
        if (effect == null)
        {
            yield break;
        }

        int level = GetInt(actionEl, "Level") ?? 0;
        int duration = GetInt(actionEl, "Duration") ?? 0;
        int count = GetInt(actionEl, "Count") ?? 0;
        int num = 0;
        if (effect.HasLevel) num = level;
        else if (effect.HasDuration) num = duration;

        try
        {
            targetView.ShowSePopup(effect.Name, effect.Type, num, LBoL.Presentation.UI.Widgets.UnitInfoWidget.SePopType.Add);

            if (string.IsNullOrEmpty(effect.Config.SFX) || effect.Config.SFX == "Default")
            {
                switch (effect.Config.Type)
                {
                    case StatusEffectType.Positive:
                        LBoL.Presentation.AudioManager.PlaySfx("Buff", -1f);
                        break;
                    case StatusEffectType.Negative:
                        LBoL.Presentation.AudioManager.PlaySfx("Debuff", -1f);
                        break;
                }
            }
            else
            {
                LBoL.Presentation.AudioManager.PlaySfx(effect.Config.SFX, -1f);
            }

            if (!string.IsNullOrEmpty(effect.Config.VFX) && effect.Config.VFX != "Default")
            {
                LBoL.Presentation.Effect.EffectManager.CreateEffect(effect.Config.VFX, targetView.EffectRoot, 0f, null, false, true);
            }
        }
        catch
        {
            // ignored
        }

        yield return new UnityEngine.WaitForSeconds(0.2f);
    }

    #endregion
}
