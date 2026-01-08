using System;
using System.Collections.Generic;
using System.Linq;
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
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 远端玩家代理目标用牌：
/// - 发送端：当卡牌/工具牌以 <see cref="RemotePlayerProxyEnemy"/> 作为单体目标时，不在本地结算，改为发送网络事件。
/// - 接收端：收到事件且目标为自己时，先做 UI 提示（后续可扩展为“按 payload 结算/播放动画”）。
/// </summary>
[HarmonyPatch]
public static class RemoteCardUsePatch
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
        => throw new NotImplementedException("Harmony reverse patch stub");

    /// <summary>
    /// 尝试获取网络客户端实例
    /// </summary>
    /// <returns>网络客户端实例，如果获取失败则返回null</returns>
    private static INetworkClient TryGetClient()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkClient>(); // 从服务提供者获取网络客户端
        }
        catch
        {
            return null; // 异常时返回null
        }
    }

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

        BattleController battle = null;
        try { battle = card?.Battle; } catch { battle = null; } // 尝试获取战斗控制器

        try
        {
            if (card != null)
            {
                card.PendingManaUsage = null; // 清空待处理法力使用
                card.PendingTarget = null; // 清空待处理目标
                card.KickerPlaying = false; // 重置踢球状态
            }
        }
        catch
        {
            // ignored
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

            var list = new List<object>();
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
                try { if (se.HasLevel) level = se.Level; } catch { /* ignored */ } // 获取等级
                try { if (se.HasCount) count = se.Count; } catch { /* ignored */ } // 获取计数
                try { if (se.HasDuration) duration = se.Duration; } catch { /* ignored */ } // 获取持续时间
                try { limit = se.Limit; } catch { /* ignored */ } // 获取限制

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
            if (__instance == null || selector == null)
            {
                return true; // 参数无效，执行原始方法
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
            string senderCharacterId = null;
            try { senderCharacterId = card?.Battle?.Player?.Id; } catch { senderCharacterId = null; } // 获取发送者角色ID

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

            string requestId = null;
            try { requestId = Guid.NewGuid().ToString("N"); } catch { requestId = null; } // 生成请求ID

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
                ConsumingMana = SnapshotMana(consumingMana), // 快照法力消耗
                Kicker = kicker, // 踢球状态
                SenderStatusEffects = SnapshotStatusEffects(card?.Battle?.Player), // 快照发送者状态效果
                Actions = actionBlueprint // 动作蓝图
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
        try
        {
            card.PendingManaUsage = consumingMana; // 设置待处理法力使用
            card.PendingTarget = proxy; // 设置待处理目标
            card.KickerPlaying = kicker; // 设置踢球状态
        }
        catch
        {
            // ignored
        }

        // 简化：仅按卡牌类型播放基础动画（不尝试复刻 Config.Perform）
        BattleAction playAnimation = null;
        try
        {
            if (card.Battle?.Player != null)
            {
                string anim = card.CardType switch
                {
                    CardType.Attack => "shoot1", // 攻击类型动画
                    CardType.Defense => "defend", // 防御类型动画
                    CardType.Skill => "skill", // 技能类型动画
                    CardType.Ability => "spell", // 能力类型动画
                    CardType.Tool => "spell", // 工具类型动画
                    _ => "spell" // 默认动画
                };

                playAnimation = PerformAction.Animation(card.Battle.Player, anim, 0.2f, null, 0f, -1); // 播放动画动作
            }
        }
        catch
        {
            // ignored
        }

        if (playAnimation != null)
        {
            yield return playAnimation;
        }

        // 3) 发送者不会收到服务器转发的 OnRemoteCardUse，所以在本地补一次对“被选中目标”的动画预播放
        try
        {
            if ((hasDamage || hasHeal || hasStatus) &&
                !string.IsNullOrWhiteSpace(proxy.RemotePlayerId) &&
                OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView(proxy.RemotePlayerId, out UnitView targetView))
            {
                targetView.PlayAnimation(hasDamage ? "hit" : "spell"); // 播放目标动画（伤害用hit，其他用spell）
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            card.PendingManaUsage = null; // 清空待处理法力使用
            card.PendingTarget = null; // 清空待处理目标
            card.KickerPlaying = false; // 重置踢球状态
        }
        catch
        {
            // ignored
        }
    }

    #endregion

    #region Receive (subscribe + UI hint)

    /// <summary>
    /// 是否已订阅网络事件
    /// </summary>
    private static bool _subscribed;
    /// <summary>
    /// 已订阅的网络客户端实例
    /// </summary>
    private static INetworkClient _subscribedClient;
    /// <summary>
    /// 游戏事件接收回调
    /// </summary>
    private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;
    /// <summary>
    /// 连接状态变化回调
    /// </summary>
    private static readonly Action<bool> _onConnectionStateChanged = OnConnectionStateChanged;

    /// <summary>
    /// 同步锁，用于线程安全访问共享字段
    /// </summary>
    private static readonly object _syncLock = new();
    /// <summary>
    /// 自身玩家ID
    /// </summary>
    private static string _selfPlayerId;

    /// <summary>
    /// 订阅钩子类，用于在GameDirector.Update中订阅网络事件
    /// </summary>
    [HarmonyPatch(typeof(GameDirector), "Update")]
    private static class SubscribeHook
    {
        /// <summary>
        /// GameDirector.Update的后缀补丁，确保订阅网络事件
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix()
        {
            INetworkClient client = TryGetClient(); // 尝试获取网络客户端
            if (client == null)
            {
                return; // 客户端为空时返回
            }

            EnsureSubscribed(client); // 确保已订阅
        }
    }

    /// <summary>
    /// 确保已订阅网络客户端事件
    /// </summary>
    /// <param name="client">网络客户端实例</param>
    private static void EnsureSubscribed(INetworkClient client)
    {
        if (_subscribed && ReferenceEquals(_subscribedClient, client))
        {
            return; // 已订阅相同客户端时返回
        }

        try
        {
            if (_subscribedClient != null)
            {
                _subscribedClient.OnGameEventReceived -= _onGameEventReceived; // 取消订阅旧客户端事件
                _subscribedClient.OnConnectionStateChanged -= _onConnectionStateChanged; // 取消订阅连接状态变化
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            client.OnGameEventReceived += _onGameEventReceived; // 订阅游戏事件
            client.OnConnectionStateChanged += _onConnectionStateChanged; // 订阅连接状态变化
            _subscribedClient = client; // 更新已订阅客户端
            _subscribed = true; // 标记为已订阅
        }
        catch
        {
            _subscribedClient = null; // 异常时重置客户端
            _subscribed = false; // 标记为未订阅
        }
    }

    /// <summary>
    /// 连接状态变化回调处理
    /// </summary>
    /// <param name="connected">是否已连接</param>
    private static void OnConnectionStateChanged(bool connected)
    {
        if (connected)
        {
            return; // 连接时不需要处理
        }

        lock (_syncLock)
        {
            _selfPlayerId = null; // 断开连接时清空自身玩家ID
        }
    }

    /// <summary>
    /// 游戏事件接收回调处理
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="payload">事件负载</param>
    private static void OnGameEventReceived(string eventType, object payload)
    {
        if (!TryGetJsonElement(payload, out JsonElement root)) // 尝试解析JSON
        {
            return; // 解析失败时返回
        }

        switch (eventType)
        {
            case "Welcome":
                HandleWelcome(root); // 处理欢迎消息
                return;
            case NetworkMessageTypes.OnRemoteCardUse:
                HandleRemoteCardUse(root); // 处理远程卡牌使用
                return;
            case NetworkMessageTypes.OnRemoteCardResolved:
                HandleRemoteCardResolved(root); // 处理远程卡牌解析
                return;
        }
    }

    /// <summary>
    /// 处理欢迎消息，获取自身玩家ID
    /// </summary>
    /// <param name="root">JSON根元素</param>
    private static void HandleWelcome(JsonElement root)
    {
        try
        {
            if (!root.TryGetProperty("PlayerId", out JsonElement idEl) || idEl.ValueKind != JsonValueKind.String)
            {
                return; // 无法获取玩家ID时返回
            }

            string pid = idEl.GetString(); // 获取玩家ID
            lock (_syncLock)
            {
                _selfPlayerId = pid; // 存储自身玩家ID
            }
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// 处理远程卡牌使用事件
    /// </summary>
    /// <param name="root">JSON根元素</param>
    private static void HandleRemoteCardUse(JsonElement root)
    {
        try
        {
            string targetId = GetString(root, "TargetPlayerId"); // 获取目标玩家ID
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return; // 目标ID为空时返回
            }

            string selfId;
            lock (_syncLock)
            {
                selfId = _selfPlayerId; // 获取自身玩家ID
            }

            string senderName = GetString(root, "SenderName") ?? "Remote"; // 获取发送者名称

            string cardName = null;
            if (root.TryGetProperty("Card", out JsonElement cardEl) && cardEl.ValueKind == JsonValueKind.Object)
            {
                cardName = GetString(cardEl, "CardName") ?? GetString(cardEl, "CardId"); // 获取卡牌名称
            }

            // 全部客户端播放基础视觉动画
            TryPlayRemoteCardUseAnimation(root);

            // 只有被选中的目标客户端才进行结算
            if (string.IsNullOrWhiteSpace(selfId) || !string.Equals(selfId, targetId, StringComparison.Ordinal))
            {
                return; // 不是目标玩家时返回
            }

            ShowTopMessage($"{senderName} used {cardName ?? "a card"} on you."); // 显示顶部消息
            TryExecuteRemoteCardUse(root); // 尝试执行远程卡牌使用
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RemoteCardUse] Receive failed: {ex.Message}"); // 记录错误
        }
    }

    /// <summary>
    /// 处理远程卡牌解析事件
    /// </summary>
    /// <param name="root">JSON根元素</param>
    private static void HandleRemoteCardResolved(JsonElement root)
    {
        try
        {
            string playerId = GetString(root, "TargetPlayerId"); // 获取玩家ID
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return; // 玩家ID为空时返回
            }

            if (!ShouldApplyRemoteResolved(playerId, root))
            {
                return;
            }

            ApplyRemoteResolvedStateToView(playerId, root); // 应用远程解析状态到视图
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RemoteCardUse] Resolve receive failed: {ex.Message}"); // 记录错误
        }
    }

    private static bool ShouldApplyRemoteResolved(string targetPlayerId, JsonElement root)
    {
        try
        {
            long resolveSeq = GetLong(root, "ResolveSeq") ?? 0;
            long timestamp = GetLong(root, "Timestamp") ?? 0;
            string requestId = GetString(root, "RequestId");

            lock (_resolvedLock)
            {
                if (!string.IsNullOrWhiteSpace(requestId))
                {
                    if (!_processedResolvedRequestIdsByTarget.TryGetValue(targetPlayerId, out HashSet<string> set))
                    {
                        set = new HashSet<string>(StringComparer.Ordinal);
                        _processedResolvedRequestIdsByTarget[targetPlayerId] = set;
                    }

                    if (set.Contains(requestId))
                    {
                        return false;
                    }

                    set.Add(requestId);
                    if (set.Count > 256)
                    {
                        set.Clear();
                        set.Add(requestId);
                    }
                }

                if (resolveSeq > 0)
                {
                    if (_lastResolvedSeqByTarget.TryGetValue(targetPlayerId, out long lastSeq) && resolveSeq <= lastSeq)
                    {
                        return false;
                    }

                    _lastResolvedSeqByTarget[targetPlayerId] = resolveSeq;
                    if (timestamp > 0)
                    {
                        _lastResolvedTimestampByTarget[targetPlayerId] = timestamp;
                    }
                    return true;
                }

                if (timestamp > 0)
                {
                    if (_lastResolvedTimestampByTarget.TryGetValue(targetPlayerId, out long lastTs) && timestamp <= lastTs)
                    {
                        return false;
                    }

                    _lastResolvedTimestampByTarget[targetPlayerId] = timestamp;
                    return true;
                }
            }

            return true;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// 尝试播放远程卡牌使用动画
    /// </summary>
    /// <param name="root">JSON根元素</param>
    private static void TryPlayRemoteCardUseAnimation(JsonElement root)
    {
        try
        {
            string senderId = GetString(root, "SenderPlayerId"); // 获取发送者ID
            string targetId = GetString(root, "TargetPlayerId"); // 获取目标ID

            string cardType = null;
            if (root.TryGetProperty("Card", out JsonElement cardEl) && cardEl.ValueKind == JsonValueKind.Object)
            {
                cardType = GetString(cardEl, "CardType"); // 获取卡牌类型
            }

            string anim = MapCardTypeToAnimation(cardType); // 映射卡牌类型到动画

            if (!string.IsNullOrWhiteSpace(senderId) && OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView(senderId, out UnitView casterView))
            {
                casterView.PlayAnimation(anim); // 播放施法者动画
            }

            bool hasDamage = false;
            try
            {
                if (root.TryGetProperty("Actions", out JsonElement actionsEl) && actionsEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in actionsEl.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Object)
                        {
                            continue; // 跳过非对象元素
                        }

                        if (GetString(item, "Kind") == "Damage")
                        {
                            hasDamage = true; // 标记有伤害动作
                            break;
                        }
                    }
                }
            }
            catch
            {
                hasDamage = false; // 异常时设置为false
            }

            if (!hasDamage || string.IsNullOrWhiteSpace(targetId))
            {
                return; // 没有伤害或目标ID为空时返回
            }

            string selfId;
            lock (_syncLock)
            {
                selfId = _selfPlayerId; // 获取自身ID
            }

            if (!string.IsNullOrWhiteSpace(selfId) && string.Equals(selfId, targetId, StringComparison.Ordinal))
            {
                try
                {
                    Singleton<GameDirector>.Instance?.PlayerUnitView?.PlayAnimation("hit"); // 播放自身受击动画
                }
                catch
                {
                    // ignored
                }
                return;
            }

            if (OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView(targetId, out UnitView targetView))
            {
                targetView.PlayAnimation("hit"); // 播放目标受击动画
            }
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// 映射卡牌类型到对应的动画名称
    /// </summary>
    /// <param name="cardType">卡牌类型字符串</param>
    /// <returns>动画名称</returns>
    private static string MapCardTypeToAnimation(string cardType)
    {
        return cardType switch
        {
            nameof(CardType.Attack) => "shoot1", // 攻击卡牌使用射击动画
            nameof(CardType.Defense) => "defend", // 防御卡牌使用防御动画
            nameof(CardType.Skill) => "skill", // 技能卡牌使用技能动画
            nameof(CardType.Ability) => "spell", // 能力卡牌使用法术动画
            nameof(CardType.Tool) => "spell", // 工具卡牌使用法术动画
            _ => "spell" // 默认使用法术动画
        };
    }

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
        var list = new List<BattleAction>();
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
            var list = new List<object>();
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
                            list.Add(new
                            {
                                Kind = "Damage",
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
    {
        try
        {
            if (payload is JsonElement je)
            {
                root = je;
                return true;
            }

            if (payload is string s)
            {
                root = JsonDocument.Parse(s).RootElement;
                return true;
            }
        }
        catch
        {
            // ignored
        }

        root = default;
        return false;
    }

    private static string GetString(JsonElement elem, string property)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return null;
            }

            return p.ValueKind switch
            {
                JsonValueKind.String => p.GetString(),
                JsonValueKind.Number => p.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => null,
            };
        }
        catch
        {
            return null;
        }
    }

    private static int? GetInt(JsonElement elem, string property)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return null;
            }

            if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out int i))
            {
                return i;
            }

            if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out int s))
            {
                return s;
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static long? GetLong(JsonElement elem, string property)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return null;
            }

            if (p.ValueKind == JsonValueKind.Number && p.TryGetInt64(out long i))
            {
                return i;
            }

            if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out int i32))
            {
                return i32;
            }

            if (p.ValueKind == JsonValueKind.String && long.TryParse(p.GetString(), out long s))
            {
                return s;
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static float? GetFloat(JsonElement elem, string property)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return null;
            }

            if (p.ValueKind == JsonValueKind.Number && p.TryGetSingle(out float f))
            {
                return f;
            }

            if (p.ValueKind == JsonValueKind.Number && p.TryGetDouble(out double d))
            {
                return (float)d;
            }

            if (p.ValueKind == JsonValueKind.String && float.TryParse(p.GetString(), out float s))
            {
                return s;
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static bool? GetBool(JsonElement elem, string property)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return null;
            }

            return p.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(p.GetString(), out bool b) ? b : null,
                _ => null,
            };
        }
        catch
        {
            return null;
        }
    }

    #endregion
}
