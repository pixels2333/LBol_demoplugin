using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Presentation.Units;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.UI;

namespace NetworkPlugin.Patch.Network;

public static partial class RemoteCardUsePatch
{
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
            INetworkClient client = TryGetClient();
            if (client == null)
            {
                return;
            }

            EnsureSubscribed(client);
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
            // ignored
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

    /// <summary>
    /// 连接状态变化回调处理
    /// </summary>
    /// <param name="connected">是否已连接</param>
    private static void OnConnectionStateChanged(bool connected)
    {
        if (connected)
        {
            return;
        }

        lock (_syncLock)
        {
            _selfPlayerId = null;
        }

        lock (_resolvedLock)
        {
            _lastResolvedSeqByTarget.Clear();
            _lastResolvedTimestampByTarget.Clear();
            _processedResolvedRequestIdsByTarget.Clear();
        }
    }

    /// <summary>
    /// 游戏事件接收回调处理
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="payload">事件负载</param>
    private static void OnGameEventReceived(string eventType, object payload)
    {
        if (!TryGetJsonElement(payload, out JsonElement root))
        {
            return;
        }

        switch (eventType)
        {
            case NetworkMessageTypes.Welcome:
                HandleWelcome(root);
                return;
            case NetworkMessageTypes.OnRemoteCardUse:
                HandleRemoteCardUse(root);
                return;
            case NetworkMessageTypes.OnRemoteCardResolved:
                HandleRemoteCardResolved(root);
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
                return;
            }

            string pid = idEl.GetString();
            lock (_syncLock)
            {
                _selfPlayerId = pid;
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
            string targetId = GetString(root, "TargetPlayerId");
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return;
            }

            string selfId;
            lock (_syncLock)
            {
                selfId = _selfPlayerId;
            }

            string senderName = GetString(root, "SenderName") ?? "Remote";

            string cardName = null;
            if (root.TryGetProperty("Card", out JsonElement cardEl) && cardEl.ValueKind == JsonValueKind.Object)
            {
                cardName = GetString(cardEl, "CardName") ?? GetString(cardEl, "CardId");
            }

            TryPlayRemoteCardUseAnimation(root);

            if (string.IsNullOrWhiteSpace(selfId) || !string.Equals(selfId, targetId, StringComparison.Ordinal))
            {
                return;
            }

            ShowTopMessage($"{senderName} used {cardName ?? "a card"} on you.");
            TryExecuteRemoteCardUse(root);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RemoteCardUse] Receive failed: {ex.Message}");
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
            string playerId = GetString(root, "TargetPlayerId");
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return;
            }

            if (!ShouldApplyRemoteResolved(playerId, root))
            {
                return;
            }

            ApplyRemoteResolvedStateToView(playerId, root);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RemoteCardUse] Resolve receive failed: {ex.Message}");
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
            string senderId = GetString(root, "SenderPlayerId");
            string targetId = GetString(root, "TargetPlayerId");

            string cardType = null;
            if (root.TryGetProperty("Card", out JsonElement cardEl) && cardEl.ValueKind == JsonValueKind.Object)
            {
                cardType = GetString(cardEl, "CardType");
            }

            string anim = MapCardTypeToAnimation(cardType);

            string selfId;
            lock (_syncLock)
            {
                selfId = _selfPlayerId;
            }

            if (!string.IsNullOrWhiteSpace(senderId) && OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView(senderId, out UnitView casterView))
            {
                casterView.PlayAnimation(anim);
            }
            else if (!string.IsNullOrWhiteSpace(selfId) && !string.IsNullOrWhiteSpace(senderId) && string.Equals(selfId, senderId, StringComparison.Ordinal))
            {
                try
                {
                    Singleton<GameDirector>.Instance?.PlayerUnitView?.PlayAnimation(anim);
                }
                catch
                {
                    // ignored
                }
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
                            continue;
                        }

                        if (GetString(item, "Kind") == "Damage")
                        {
                            hasDamage = true;
                            break;
                        }
                    }
                }
            }
            catch
            {
                hasDamage = false;
            }

            if (!hasDamage || string.IsNullOrWhiteSpace(targetId))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(selfId) && string.Equals(selfId, targetId, StringComparison.Ordinal))
            {
                try
                {
                    Singleton<GameDirector>.Instance?.PlayerUnitView?.PlayAnimation("hit");
                }
                catch
                {
                    // ignored
                }
                return;
            }

            if (OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView(targetId, out UnitView targetView))
            {
                targetView.PlayAnimation("hit");
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
            nameof(CardType.Attack) => "shoot1",
            nameof(CardType.Defense) => "defend",
            nameof(CardType.Skill) => "skill",
            nameof(CardType.Ability) => "spell",
            nameof(CardType.Tool) => "spell",
            _ => "spell"
        };
    }

    #endregion
}
