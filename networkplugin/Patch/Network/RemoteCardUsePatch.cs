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
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.Patch.EnemyUnits;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 远端玩家代理目标用牌：
/// - 发送端：当卡牌/工具牌以 <see cref="RemotePlayerProxyEnemy"/> 作为单体目标时，不在本地结算，改为发送网络事件。
/// - 接收端：收到事件且目标为自己时，先做 UI 提示（后续可扩展为“按 payload 结算/播放动画”）。
/// </summary>
public static partial class RemoteCardUsePatch
{
    /// <summary>
    /// 获取服务提供者实例
    /// </summary>
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 指示是否处于远程卡牌处理管道中，用于抑制其他动作级别的同步补丁
    /// </summary>
    internal static bool IsInRemoteCardPipeline => Volatile.Read(ref _remotePipelineDepth) > 0;

    /// <summary>
    /// 远程管道深度计数器，用于嵌套处理
    /// </summary>
    private static int _remotePipelineDepth;

    /// <summary>
    /// 远程管道作用域类，实现IDisposable接口用于资源管理
    /// </summary>
    private sealed class RemotePipelineScope : IDisposable
    {
        /// <summary>
        /// 指示是否已释放资源
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 构造函数，增加远程管道深度
        /// </summary>
        public RemotePipelineScope()
        {
            Interlocked.Increment(ref _remotePipelineDepth); // 增加管道深度
        }

        /// <summary>
        /// 释放资源，减少远程管道深度
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return; // 已释放则直接返回
            }

            _disposed = true; // 标记为已释放
            Interlocked.Decrement(ref _remotePipelineDepth); // 减少管道深度
        }
    }

    /// <summary>
    /// 进入远程管道作用域
    /// </summary>
    /// <returns>远程管道作用域实例</returns>
    private static IDisposable EnterRemotePipelineScope() => new RemotePipelineScope();

    /// <summary>
    /// 已解析序列号，用于消息去重
    /// </summary>
    private static long _resolvedSeq;
    /// <summary>
    /// 解析锁，用于线程安全访问共享字段
    /// </summary>
    private static readonly object _resolvedLock = new();
    /// <summary>
    /// 按目标玩家ID存储的最后解析序列号
    /// </summary>
    private static readonly Dictionary<string, long> _lastResolvedSeqByTarget = new(StringComparer.Ordinal);
    /// <summary>
    /// 按目标玩家ID存储的最后解析时间戳
    /// </summary>
    private static readonly Dictionary<string, long> _lastResolvedTimestampByTarget = new(StringComparer.Ordinal);
    /// <summary>
    /// 按目标玩家ID存储的已处理请求ID集合
    /// </summary>
    private static readonly Dictionary<string, HashSet<string>> _processedResolvedRequestIdsByTarget = new(StringComparer.Ordinal);

    /// <summary>
    /// Harmony反向补丁，获取Card.GetActions的原始实现
    /// </summary>
    /// <remarks>
    /// 注意：该方法是 Harmony ReversePatch 必需桩方法（白名单保留），
    /// 即使方法体抛出异常也不能按“幽灵方法”删除。
    /// </remarks>
    [HarmonyReversePatch(HarmonyReversePatchType.Original)]
    [HarmonyPatch(typeof(Card), "GetActions")]
    private static IEnumerable<BattleAction> Card_GetActions_Original(
        Card __instance,
        UnitSelector selector,
        ManaGroup consumingMana,
        Interaction precondition,
        bool kicker,
        bool summoning,
        IList<DamageAction> damageActions)
        // TODO: 当前为 Harmony reverse patch 占位桩，需实现原始方法逻辑。
        => throw new NotImplementedException("Harmony reverse patch stub");

    /// <summary>
    /// 尝试获取网络客户端实例
    /// </summary>
    /// <returns>网络客户端实例，如果获取失败则返回null</returns>
    private static INetworkClient TryGetClient()
        => ServiceProvider?.GetService<INetworkClient>();

    /// <summary>
    /// 检查是否已连接网络客户端
    /// </summary>
    /// <param name="client">输出的网络客户端实例</param>
    /// <returns>是否已连接</returns>
    private static bool IsConnected(out INetworkClient client)
    {
        client = TryGetClient(); // 尝试获取客户端
        return client != null && client.IsConnected; // 检查客户端是否存在且已连接
    }

    /// <summary>
    /// 失败回退动作，在网络连接失败时提供基本处理
    /// </summary>
    /// <param name="card">卡牌实例</param>
    /// <param name="consumingMana">消耗的法力值</param>
    /// <param name="message">显示的消息</param>
    /// <returns>回退动作序列</returns>
    private static IEnumerable<BattleAction> FailFallbackActions(Card card, ManaGroup consumingMana, string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            ShowTopMessage(message); // 显示顶部消息
        }

        BattleController battle = card?.Battle;

        if (card != null)
        {
            card.PendingManaUsage = null;
            card.PendingTarget = null;
            card.KickerPlaying = false;
        }

            // 退还金钱（在 PlayCardAction/UseCardAction 中，金钱可能已在 GetActions 之前被扣除）
            BattleAction refundMoneyAction = null;
            try
            {
                int? moneyCost = card?.Config?.MoneyCost;

                if (battle != null && card != null && moneyCost != null && moneyCost.Value > 0 &&
                    (card.Zone == CardZone.PlayArea || card.Zone == CardZone.FollowArea))
                {
                    refundMoneyAction = new GainMoneyAction(moneyCost.Value);
                }
            }
            catch
            {
                // ignored
            }

            if (refundMoneyAction != null)
            {
                yield return refundMoneyAction;
            }

            // 仅当卡牌已移动到PlayArea时退还法力（UseCardAction路径）      
            BattleAction refundAction = null;
            try
            {
                if (battle != null && card != null && card.Zone == CardZone.PlayArea && consumingMana.Total > 0)
            {
                refundAction = new GainManaAction(consumingMana); // 退还法力
            }
        }
        catch
        {
            // ignored
        }

        if (refundAction != null)
        {
            yield return refundAction;
        }

        // 尽力返回卡牌，避免玩家因网络故障丢失卡牌
        BattleAction moveAction = null;
        try
        {
            if (battle != null && card != null)
            {
                bool canReturnToHand = false;
                try
                {
                    canReturnToHand = battle.HandZone != null && battle.HandZone.Count < battle.MaxHand; // 检查手牌区是否已满
                }
                catch
                {
                    canReturnToHand = false; // 异常时设置为false
                }

                moveAction = new MoveCardAction(card, canReturnToHand ? CardZone.Hand : CardZone.Discard); // 移动卡牌到手牌或弃牌堆
            }
        }
        catch
        {
            // ignored
        }

        if (moveAction != null)
        {
            yield return moveAction;
        }
    }

    /// <summary>
    /// 快照法力组信息
    /// </summary>
    /// <param name="mana">法力组实例</param>
    /// <returns>序列化的法力组信息</returns>
    private static object SnapshotMana(ManaGroup mana)
        => new
        {
            Red = mana.Red,
            Blue = mana.Blue,
            Green = mana.Green,
            White = mana.White,
            Colorless = mana.Colorless,
            Philosophy = mana.Philosophy,
            Any = mana.Any,
            Hybrid = mana.Hybrid,
            HybridColor = mana.HybridColor.ToString(),
            Total = mana.Total
        };

    /// <summary>
    /// 快照状态效果信息
    /// </summary>
    /// <param name="unit">单位实例</param>
    /// <returns>序列化的状态效果信息</returns>
    private static object SnapshotStatusEffects(Unit unit)
    {
        try
        {
            if (unit?.StatusEffects == null)
            {
                return Array.Empty<object>(); // 无状态效果时返回空数组
            }

            List<object> list = new List<object>();
            foreach (StatusEffect se in unit.StatusEffects)
            {
                if (se == null)
                {
                    continue; // 跳过空状态效果
                }

                int? level = null;
                int? count = null;
                int? duration = null;
                int? limit = null;
                try
                {
                    if (se.HasLevel) level = se.Level;
                    if (se.HasCount) count = se.Count;
                    if (se.HasDuration) duration = se.Duration;
                    limit = se.Limit;
                }
                catch (Exception ex) { Plugin.Logger?.LogDebug($"[RemoteCardUse] 获取 StatusEffect 属性失败: Id={se?.Id}, {ex.Message}"); }

                list.Add(new
                {
                    Id = se.Id, // 状态效果ID
                    TypeName = se.GetType().FullName, // 类型全名
                    Name = se.Name, // 状态效果名称
                    SeType = se.Type.ToString(), // 状态效果类型
                    IsAutoDecreasing = SafeGetAutoDecreasing(se), // 是否自动减少
                    HasLevel = SafeBool(() => se.HasLevel), // 是否有等级
                    Level = level, // 等级值
                    HasCount = SafeBool(() => se.HasCount), // 是否有计数
                    Count = count, // 计数值
                    HasDuration = SafeBool(() => se.HasDuration), // 是否有持续时间
                    Duration = duration, // 持续时间值
                    Limit = limit // 限制值
                });
            }

            return list.ToArray(); // 返回数组
        }
        catch
        {
            return Array.Empty<object>(); // 异常时返回空数组
        }
    }

    /// <summary>
    /// 安全获取布尔值，避免异常
    /// </summary>
    /// <param name="getter">获取布尔值的函数</param>
    /// <returns>布尔值，异常时返回false</returns>
    private static bool SafeBool(Func<bool> getter)
    {
        try
        {
            return getter(); // 尝试获取值
        }
        catch
        {
            return false; // 异常时返回false
        }
    }

    /// <summary>
    /// 安全获取状态效果是否自动减少
    /// </summary>
    /// <param name="effect">状态效果实例</param>
    /// <returns>是否自动减少，异常时返回true</returns>
    private static bool SafeGetAutoDecreasing(StatusEffect effect)
    {
        try
        {
            return Traverse.Create(effect).Property("IsAutoDecreasing").GetValue<bool>(); // 使用Traverse获取属性
        }
        catch
        {
            return true; // 异常时返回true
        }
    }

    /// <summary>
    /// 显示顶部消息
    /// </summary>
    /// <param name="message">要显示的消息</param>
    private static void ShowTopMessage(string message)
    {
        try
        {
            if (!UiManager.IsInitialized)
            {
                return; // UI管理器未初始化时返回
            }

            UiManager.GetPanel<TopMessagePanel>().ShowMessage(message); // 显示消息
        }
        catch
        {
            // ignored
        }
    }

        #region Send (patch Card.GetActions)

        [HarmonyPatch(typeof(BattleController), "RequestUseCard", typeof(Card), typeof(UnitSelector), typeof(ManaGroup), typeof(bool))]
        [HarmonyPrefix]
        private static bool BattleController_RequestUseCard_Prefix(Card card, UnitSelector selector)
        {
            try
            {
                if (card == null || selector == null)
                {
                    return true;
                }

                if (selector.Type != TargetType.SingleEnemy)
                {
                    return true;
                }

                if (selector.SelectedEnemy is not RemotePlayerProxyEnemy proxy || string.IsNullOrWhiteSpace(proxy.RemotePlayerId))
                {
                    return true;
                }

                if (IsConnected(out _))
                {
                    return true;
                }

                ShowTopMessage("未连接，无法对队友出牌。");
                return false;
            }
            catch
            {
                return true;
            }
        }

    /// <summary>
    /// Card.GetActions的前缀补丁，拦截对远程玩家代理目标的卡牌使用
    /// </summary>
    [HarmonyPatch(typeof(Card), "GetActions")]
    [HarmonyPrefix]
    private static bool Card_GetActions_Prefix(
        Card __instance,
        UnitSelector selector,
        ManaGroup consumingMana,
        Interaction precondition,
        bool kicker,
        bool summoning,
        IList<DamageAction> damageActions,
        ref IEnumerable<BattleAction> __result)
    {
        try
        {
            if (selector == null)
            {
                return true;
            }

            if (selector.Type != TargetType.SingleEnemy)
            {
                return true; // 不是单体敌人目标，执行原始方法
            }

            if (selector.SelectedEnemy is not RemotePlayerProxyEnemy proxy || string.IsNullOrWhiteSpace(proxy.RemotePlayerId))
            {
                return true; // 不是远程玩家代理目标，执行原始方法
            }

            if (!IsConnected(out INetworkClient client))
            {
                __result = FailFallbackActions(__instance, consumingMana, "未连接，无法对队友出牌。"); // 未连接时使用回退动作
                return false; // 跳过原始方法
            }

            __result = RemoteOnlyActions(__instance, selector, proxy, consumingMana, precondition, kicker, summoning, client); // 执行远程专用动作
            return false; // 跳过原始方法
        }
        catch
        {
            return true; // 异常时执行原始方法
        }
    }

    /// <summary>
    /// 远程专用动作处理，发送网络事件并执行本地动画
    /// </summary>
    private static IEnumerable<BattleAction> RemoteOnlyActions(Card card, UnitSelector selector, RemotePlayerProxyEnemy proxy, ManaGroup consumingMana, Interaction precondition, bool kicker, bool summoning, INetworkClient client)
    {
        bool hasDamage = false;
        bool hasHeal = false;
        bool hasStatus = false;
        object[] actionBlueprint = Array.Empty<object>();
        bool sendOk = false;
        // 1) 发送“远端用牌”事件（仅传输，不在本地结算）
        try
        {
            string senderNetworkId;
            lock (_syncLock)
            {
                senderNetworkId = _selfPlayerId; // 获取发送者网络ID
            }
            senderNetworkId ??= "unknown"; // 默认值

            string senderName = GameStateUtils.GetCurrentPlayer()?.Name; // 获取发送者名称
            string senderCharacterId = card?.Battle?.Player?.Id;

            try
            {
                using (EnterRemotePipelineScope())
                {
                    IEnumerable<BattleAction> original = Card_GetActions_Original(card, selector, consumingMana, precondition, kicker, summoning, new List<DamageAction>()); // 获取原始动作
                    actionBlueprint = BuildActionBlueprint(original, out hasDamage, out hasHeal, out hasStatus); // 构建动作蓝图
                }
            }
            catch
            {
                actionBlueprint = Array.Empty<object>(); // 异常时使用空数组
            }

            string requestId = Guid.NewGuid().ToString("N");

            var payload = new
            {
                Timestamp = DateTime.Now.Ticks,
                RequestId = requestId,
                EventType = NetworkMessageTypes.OnRemoteCardUse,
                SenderPlayerId = senderNetworkId,
                SenderName = senderName,
                SenderCharacterId = senderCharacterId,
                TargetPlayerId = proxy.RemotePlayerId,
                TargetName = proxy.RemotePlayerName,
                Card = new
                {
                    CardId = card.Id,
                    InstanceId = card.InstanceId,
                    CardName = card.Name,
                    CardType = card.CardType.ToString(),
                    IsUpgraded = card.IsUpgraded,
                    UpgradeCounter = card.UpgradeCounter,
                },
                ConsumingMana = SnapshotMana(consumingMana),
                Kicker = kicker,
                SenderStatusEffects = SnapshotStatusEffects(card?.Battle?.Player),
                Actions = actionBlueprint
            };

            client.SendGameEventData(NetworkMessageTypes.OnRemoteCardUse, payload); // 发送网络事件
            sendOk = true; // 标记发送成功
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RemoteCardUse] Send failed: {ex.Message}"); // 记录错误
        }

        if (!sendOk)
        {
            foreach (BattleAction a in FailFallbackActions(card, consumingMana, "网络发送失败，已取消对队友出牌。"))
            {
                yield return a;
            }
            yield break;
        }

        // 2) 本地仅做最小“出牌动画/状态刷新”，不执行卡牌效果
        card.PendingManaUsage = consumingMana;
        card.PendingTarget = proxy;
        card.KickerPlaying = kicker;

        try
        {
            if (card.Battle != null && actionBlueprint != null && actionBlueprint.Length > 0)
            {
                string serializedActions = JsonSerializer.Serialize(actionBlueprint);
                using (JsonDocument doc = JsonDocument.Parse(serializedActions))
                {
                    Singleton<GameDirector>.Instance?.StartCoroutine(PlayVisualsCoroutine(doc.RootElement.Clone(), card.Battle, false));
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RemoteCardUse] Caster play visuals failed: {ex.Message}");
        }

        yield return PerformAction.Wait(0.4f);

        card.PendingManaUsage = null;
        card.PendingTarget = null;
        card.KickerPlaying = false;
    }

    #endregion

    #region Shared Blueprint + JSON Helpers

    private static object SerializeUnit(Unit unit)
    {
        if (unit == null) return null;
        if (unit is PlayerUnit pu)
        {
            string pid = OtherPlayersOverlayPatch.GetPlayerIdFromUnit(pu);
            if (pid == null)
            {
                pid = NetworkIdentityTracker.GetSelfPlayerId() ?? "__local__";
            }
            return new { Kind = "Player", PlayerId = pid };
        }
        if (unit is EnemyUnit eu)
        {
            return new { Kind = "Enemy", EnemyId = eu.Id, RootIndex = eu.RootIndex };
        }
        return new { Kind = "Unknown", Id = unit.Id };
    }

    private static object[] BuildActionBlueprint(IEnumerable<BattleAction> actions)
        => BuildActionBlueprint(actions, out _, out _, out _);

    private static object[] BuildActionBlueprint(IEnumerable<BattleAction> actions, out bool hasDamage, out bool hasHeal, out bool hasStatus)
    {
        hasDamage = false;
        hasHeal = false;
        hasStatus = false;

        if (actions == null)
        {
            return Array.Empty<object>();
        }

        try
        {
            List<object> list = new List<object>();
            foreach (BattleAction action in actions)
            {
                if (action == null)
                {
                    continue;
                }

                switch (action)
                {
                    case DamageAction da:
                        {
                            hasDamage = true;
                            DamageInfo info = da.DealingArgs.DamageInfo;
                            var targetsList = new List<object>();
                            if (da.DealingArgs.Targets != null)
                            {
                                foreach (var t in da.DealingArgs.Targets)
                                {
                                    targetsList.Add(SerializeUnit(t));
                                }
                            }
                            list.Add(new
                            {
                                Kind = "Damage",
                                Caster = SerializeUnit(da.DealingArgs.Source),
                                Targets = targetsList.ToArray(),
                                Damage = info.Damage,
                                DamageType = info.DamageType.ToString(),
                                IsAccuracy = info.IsAccuracy,
                                DontBreakPerfect = info.DontBreakPerfect,
                                GunName = da.GunName,
                                GunType = da.GunType.ToString(),
                            });
                            break;
                        }
                    case HealAction ha:
                        {
                            hasHeal = true;
                            list.Add(new
                            {
                                Kind = "Heal",
                                Caster = SerializeUnit(ha.Args.Source),
                                Target = SerializeUnit(ha.Args.Target),
                                Amount = ha.Args.Amount,
                                HealType = ha.Args.HealType.ToString(),
                                WaitTime = ha.WaitTime
                            });
                            break;
                        }
                    case ApplyStatusEffectAction sa:
                        {
                            StatusEffectApplyEventArgs args = sa.Args;
                            StatusEffect effect = args?.Effect;
                            if (effect == null)
                            {
                                break;
                            }

                            hasStatus = true;
                            list.Add(new
                            {
                                Kind = "ApplyStatusEffect",
                                Target = SerializeUnit(args.Unit),
                                EffectId = effect.Id,
                                Level = args.Level,
                                Duration = args.Duration,
                                Count = args.Count,
                                Limit = effect.Limit,
                                WaitTime = args.WaitTime,
                                StartAutoDecreasing = SafeGetAutoDecreasing(effect)
                            });
                            break;
                        }
                    case PerformAction pa:
                        {
                            var paArgs = pa.Args;
                            if (paArgs is PerformAction.ViewCardArgs vca)
                            {
                                list.Add(new {
                                    Kind = "PerformAction",
                                    Type = "ViewCard",
                                    CardId = vca.Card?.Id,
                                    Zone = vca.Zone.ToString()
                                });
                            }
                            else if (paArgs is PerformAction.GunArgs ga)
                            {
                                list.Add(new {
                                    Kind = "PerformAction",
                                    Type = "Gun",
                                    Source = SerializeUnit(ga.Source),
                                    Target = SerializeUnit(ga.Target),
                                    GunId = ga.GunId,
                                    WaitTime = ga.WaitTime
                                });
                            }
                            else if (paArgs is PerformAction.DollArgs daArg)
                            {
                                list.Add(new {
                                    Kind = "PerformAction",
                                    Type = "Doll",
                                    Target = SerializeUnit(daArg.Target),
                                    GunId = daArg.GunId,
                                    WaitTime = daArg.WaitTime,
                                    DebugString = daArg.DebugString
                                });
                            }
                            else if (paArgs is PerformAction.AnimationArgs aa)
                            {
                                list.Add(new {
                                    Kind = "PerformAction",
                                    Type = "Animation",
                                    Source = SerializeUnit(aa.Source),
                                    AnimationName = aa.AnimationName,
                                    WaitTime = aa.WaitTime,
                                    SfxId = aa.SfxId,
                                    SfxDelay = aa.SfxDelay,
                                    ShakeLevel = aa.ShakeLevel
                                });
                            }
                            else if (paArgs is PerformAction.SfxArgs sfx)
                            {
                                list.Add(new {
                                    Kind = "PerformAction",
                                    Type = "Sfx",
                                    Id = sfx.Id,
                                    Delay = sfx.Delay
                                });
                            }
                            else if (paArgs is PerformAction.UiSoundArgs uiSound)
                            {
                                list.Add(new {
                                    Kind = "PerformAction",
                                    Type = "UiSound",
                                    Id = uiSound.Id
                                });
                            }
                            else if (paArgs is PerformAction.ChatArgs chat)
                            {
                                list.Add(new {
                                    Kind = "PerformAction",
                                    Type = "Chat",
                                    Source = SerializeUnit(chat.Source),
                                    Content = chat.Content,
                                    ChatTime = chat.ChatTime,
                                    Delay = chat.Delay,
                                    WaitTime = chat.WaitTime,
                                    Talk = chat.Talk
                                });
                            }
                            else if (paArgs is PerformAction.SpellArgs spell)
                            {
                                list.Add(new {
                                    Kind = "PerformAction",
                                    Type = "Spell",
                                    Source = SerializeUnit(spell.Source),
                                    SpellName = spell.SpellName
                                });
                            }
                            else if (paArgs is PerformAction.EffectArgs effect)
                            {
                                list.Add(new {
                                    Kind = "PerformAction",
                                    Type = "Effect",
                                    Source = SerializeUnit(effect.Source),
                                    EffectName = effect.EffectName,
                                    Delay = effect.Delay,
                                    WaitTime = effect.WaitTime,
                                    SfxId = effect.SfxId,
                                    SfxDelay = effect.SfxDelay,
                                    EffectType = effect.EffectType.ToString()
                                });
                            }
                            else if (paArgs is PerformAction.EffectMessageArgs ema)
                            {
                                list.Add(new {
                                    Kind = "PerformAction",
                                    Type = "EffectMessage",
                                    Source = SerializeUnit(ema.Source),
                                    EffectName = ema.EffectName,
                                    Message = ema.Message
                                });
                            }
                            else if (paArgs is PerformAction.SePopArgs sePop)
                            {
                                list.Add(new {
                                    Kind = "PerformAction",
                                    Type = "SePop",
                                    Source = SerializeUnit(sePop.Source),
                                    PopContent = sePop.PopContent
                                });
                            }
                            else if (paArgs is PerformAction.SummonFriendArgs sfa)
                            {
                                list.Add(new {
                                    Kind = "PerformAction",
                                    Type = "SummonFriend",
                                    CardId = sfa.Card?.Id
                                });
                            }
                            else if (paArgs is PerformAction.TransformModelArgs tma)
                            {
                                list.Add(new {
                                    Kind = "PerformAction",
                                    Type = "TransformModel",
                                    Source = SerializeUnit(tma.Source),
                                    ModelName = tma.ModelName
                                });
                            }
                            else if (paArgs is PerformAction.DeathAnimationArgs daa)
                            {
                                list.Add(new {
                                    Kind = "PerformAction",
                                    Type = "DeathAnimation",
                                    Source = SerializeUnit(daa.Source)
                                });
                            }
                            else if (paArgs is PerformAction.WaitArgs wait)
                            {
                                list.Add(new {
                                    Kind = "PerformAction",
                                    Type = "Wait",
                                    Time = wait.Time,
                                    Unscale = wait.Unscale
                                });
                            }
                            break;
                        }
                }
            }

            return list.ToArray();
        }
        catch
        {
            hasDamage = false;
            hasHeal = false;
            hasStatus = false;
            return Array.Empty<object>();
        }
    }

    private static bool TryGetJsonElement(object payload, out JsonElement root)
        => NetworkEventHelper.TryGetJsonElement(payload, out root);

    private static string GetString(JsonElement root, string name)
        => NetworkEventHelper.GetString(root, name);

    private static int? GetInt(JsonElement elem, string property)
    {
        if (NetworkEventHelper.TryGetInt(elem, property, out int v))
            return v;
        return null;
    }

    private static long? GetLong(JsonElement elem, string property)
    {
        if (NetworkEventHelper.TryGetLong(elem, property, out long v))
            return v;
        return null;
    }

    private static float? GetFloat(JsonElement elem, string property)
    {
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            return null;
        if (p.ValueKind == JsonValueKind.Number && p.TryGetSingle(out float f))
            return f;
        if (p.ValueKind == JsonValueKind.Number && p.TryGetDouble(out double d))
            return (float)d;
        if (p.ValueKind == JsonValueKind.String && float.TryParse(p.GetString(), out float s))
            return s;
        return null;
    }

    private static bool? GetBool(JsonElement elem, string property)
    {
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            return null;
        return p.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(p.GetString(), out bool b) ? b : null,
            _ => null,
        };
    }

    #endregion
}
