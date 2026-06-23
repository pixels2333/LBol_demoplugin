using System;
using System.Collections.Generic;
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
                InvokeBattleReact(battle, actions, actionSourceCard);
                TryBroadcastResolvedState(root, battle);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RemoteCardUse] Execute failed: {ex.Message}");
        }
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

    private static void InvokeBattleReact(BattleController battle, List<BattleAction> actions, Card actionSourceCard)
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
                        TryAddReplayDamage(list, item, caster, targetUnit);
                        break;
                    case "Heal":
                        TryAddReplayHeal(list, item, caster, targetUnit);
                        break;
                    case "ApplyStatusEffect":
                        TryAddReplayStatus(list, item, targetUnit);
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
