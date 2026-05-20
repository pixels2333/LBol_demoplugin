using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.UI.Factories;
using NetworkPlugin.UI.Payloads;
using NetworkPlugin.UI.Rules;
using NetworkPlugin.UI.Widgets;
using NetworkPlugin.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace NetworkPlugin.UI.Panels;
// 网络同步与交易状态
public sealed partial class TradePanel
{
private void TrySubscribeTradeEvents()
    {
        if (_subscribedToTrade) return;
        TradeSyncPatch.OnTradeStateUpdated += OnTradeStateUpdated;
        _subscribedToTrade = true;
    }

    private void TryUnsubscribeTradeEvents()
    {
        if (!_subscribedToTrade) return;
        TradeSyncPatch.OnTradeStateUpdated -= OnTradeStateUpdated;
        _subscribedToTrade = false;
    }

    private void OnTradeStateUpdated(TradeSyncPatch.TradeSessionState state)
    {
        try
        {
            if (state is null || !string.Equals(state.TradeId, _tradeId, StringComparison.Ordinal))
            {
                return;
            }

            // 只关心参与者。
            if (!state.IsParticipant(_selfPlayerId))
            {
                return;
            }

            ApplyStateToUi(state);

            // Preparing：运行严格本地预检并一次性上报结果。
            if (state.Status == TradeSyncPatch.TradeStatus.Preparing)
            {
                TryHandlePreparing(state);
            }

            if (state.Status == TradeSyncPatch.TradeStatus.Completed)
            {
                StartCoroutine(ExecuteTrade());
            }
            else if (state.Status == TradeSyncPatch.TradeStatus.Canceled)
            {
                UpdateUIStatus("Trade.Canceled".Localize());
                Hide();
            }
            else if (state.Status == TradeSyncPatch.TradeStatus.Open)
            {
        // 若 host 将状态回退到 Open（Prepare 失败），显示原因并允许重试。
                if (_lastTradeStatus == TradeSyncPatch.TradeStatus.Preparing && !string.IsNullOrWhiteSpace(state.Reason))
                {
                    UpdateUIStatus($"Prepare failed: {state.Reason}");
                }
            }

            _lastTradeStatus = state.Status;
        }
        catch
        {
            // 忽略
        }
    }

    private void ApplyStateToUi(TradeSyncPatch.TradeSessionState state)
    {
        // 以 Host 广播状态为准刷新 UI。
        using (new ApplyingStateScope(this))
        {
            bool localIsA = IsPlayerA(state);

            // 清空现有 UI
            ResetTradeData();
            _tradeId = state.TradeId;
            _selfPlayerId = NetworkIdentityTracker.GetSelfPlayerId();
            _playerAId = state.PlayerAId;
            _playerBId = state.PlayerBId;

            // 从 host 状态拉取本地金币/展品报价，保持 UI 一致。
            _localMoneyOffer = localIsA ? state.MoneyA : state.MoneyB;
            _localExhibitOfferIds.Clear();
            (localIsA ? state.ExhibitsA : state.ExhibitsB)?
                .Where(ex => ex is not null && !string.IsNullOrWhiteSpace(ex.ExhibitId))
                .Select(ex => ex.ExhibitId)
                .ToList()
                .ForEach(id => _localExhibitOfferIds.Add(id));
            RefreshOfferEditorTexts();

            // 本地报价：显示在 player1
            (localIsA ? state.OfferA : state.OfferB)
                ?.Select(c => TryFindDeckCard(c))
                .Where(real => real is not null)
                .ToList()
                .ForEach(real => AddCardToTrade(real, true));

            // 远端报价：显示在 player2（临时卡用于展示）
            (localIsA ? state.OfferB : state.OfferA)
                ?.Where(c => c != null && !string.IsNullOrWhiteSpace(c.CardId))
                .Select(c =>
                {
                    try { return Library.TryCreateCard(c.CardId, c.IsUpgraded, c.UpgradeCounter); }
                    catch (Exception ex) { Plugin.Logger?.LogWarning($"[TradePanel] 远端报价创建临时卡失败: CardId={c.CardId}, {ex.Message}"); return null; }
                })
                .Where(temp => temp != null)
                .ToList()
                .ForEach(temp => AddCardToTrade(temp, false));

            // 锁住远端槽位，避免误删
            player2Slots?.ToList().ForEach(s => s?.SetLocked(true));
        }

        // 用户需求：永不禁用确认按钮，点击时再做逻辑守卫。
        if (confirmButton?.button is not null)
        {
            confirmButton.button.interactable = true;
        }

        if (state.Status == TradeSyncPatch.TradeStatus.Open)
        {
            UpdateUIStatus((state.OfferA?.Count ?? 0) > 0 && (state.OfferB?.Count ?? 0) > 0
                ? TryLocalize("Trade.ReadyToConfirm", "可以确认交易")
                : TryLocalize("Trade.WaitingForItems", "等待放入物品..."));
        }
        else if (state.Status == TradeSyncPatch.TradeStatus.Completed)
        {
            UpdateUIStatus("Trade.Completed".Localize());
        }
        else if (state.Status == TradeSyncPatch.TradeStatus.Preparing)
        {
            UpdateUIStatus("Preparing...");
        }
    }

    private Card TryFindDeckCard(TradeSyncPatch.CardRef cardRef)
    {
        try
        {
            if (cardRef is null || cardRef.InstanceId < 0)
            {
                return null;
            }

            return ActiveGameRun?.GetDeckCardByInstanceId(cardRef.InstanceId);
        }
        catch
        {
            return null;
        }
    }

    private void TrySendOfferUpdate()
    {
        if (!TryIsNetworkTrade(out _))
        {
            return;
        }

        // 只发送本地侧(player1)报价。
        List<Card> offered = _player1OfferedCards;
        List<TradeSyncPatch.CardRef> refs = offered
            .Where(c => c is not null)
            .Select(c => new TradeSyncPatch.CardRef
            {
                CardId = c.Id,
                InstanceId = c.InstanceId,
                IsUpgraded = c.IsUpgraded,
                UpgradeCounter = c.UpgradeCounter ?? 0,
                DeckCounter = c.DeckCounter,
                CardName = c.Name,
                CardType = c.CardType.ToString(),
            })
            .ToList();

        TradeSyncPatch.RequestOfferUpdate(_tradeId, _selfPlayerId, refs, _localMoneyOffer, _localExhibitOfferIds.ToList());
    }

    private IEnumerator ApplyNetworkTradeAndClose(bool localIsA)
    {
        GameRunController run = ActiveGameRun;
        TradeSyncPatch.TradeSessionState state = TradeSyncPatch.GetLastKnown(_tradeId);
        if (state is null || state.Status != TradeSyncPatch.TradeStatus.Completed)
        {
            yield break;
        }

        if (run is null)
        {
            UpdateUIStatus("Trade failed: game state unavailable.");
            Plugin.Logger?.LogWarning($"[TradePanel] ApplyNetworkTradeAndClose aborted: ActiveGameRun is null, tradeId={_tradeId ?? "<null>"}");
            yield break;
        }

        // 禁用交互
        SetCanvasInteractable(false);

        using (TradeSyncPatch.EnterApplyingTradeScope())
        {
            // 自己移除自己报价，添加对方报价。
            List<TradeSyncPatch.CardRef> mine = localIsA ? state.OfferA : state.OfferB;
            List<TradeSyncPatch.CardRef> theirs = localIsA ? state.OfferB : state.OfferA;

            int myMoney = localIsA ? state.MoneyA : state.MoneyB;
            int theirMoney = localIsA ? state.MoneyB : state.MoneyA;

            List<TradeSyncPatch.ExhibitRef> myExhibits = localIsA ? state.ExhibitsA : state.ExhibitsB;
            List<TradeSyncPatch.ExhibitRef> theirExhibits = localIsA ? state.ExhibitsB : state.ExhibitsA;

            if (mine is not null)
            {
                foreach (var c in mine)
                {
                    if (c is null)
                    {
                        continue;
                    }

                    Card deck = TryFindDeckCard(c);
                    if (deck is null)
                    {
                        UpdateUIStatus("Trade failed: missing offered card.");
                        yield break;
                    }

                    run.RemoveDeckCard(deck, false);
                }
            }

            // 金币：严格核查（不足则失败）。
            try
            {
                if (myMoney > 0)
                {
                    run.ConsumeMoney(myMoney);
                }
            }
            catch
            {
                UpdateUIStatus("Trade failed: insufficient money.");
                yield break;
            }

            if (theirMoney > 0)
            {
                run.GainMoney(theirMoney, true, new VisualSourceData { SourceType = VisualSourceType.CardSelect });
            }

            // 展品：严格核查（未找到/不可交易/黑名单/重复 则失败）。
            if (myExhibits is not null)
            {
                foreach (var ex in myExhibits)
                {
                    string id = ex?.ExhibitId;
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        continue;
                    }

                    Exhibit owned = null;
                    try
                    {
                        owned = run.Player?.Exhibits?.FirstOrDefault(e => e is not null && string.Equals(e.Id, id, StringComparison.Ordinal));
                    }
                    catch
                    {
                        owned = null;
                    }

                    if (owned is null)
                    {
                        UpdateUIStatus("Trade failed: missing offered exhibit.");
                        yield break;
                    }

                    if (!TradeExhibitRules.IsTradable(owned))
                    {
                        UpdateUIStatus("Trade failed: exhibit not tradable.");
                        yield break;
                    }

                    try
                    {
                        run.LoseExhibit(owned, true, true);
                    }
                    catch
                    {
                        UpdateUIStatus("Trade failed: cannot lose exhibit.");
                        yield break;
                    }
                }
            }

            if (theirExhibits is not null)
            {
                foreach (var ex in theirExhibits)
                {
                    string id = ex?.ExhibitId;
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        continue;
                    }

                    if (TradeExhibitRules.IsBlacklisted(id))
                    {
                        UpdateUIStatus("Trade failed: received exhibit is blacklisted.");
                        yield break;
                    }

                    Exhibit created;
                    try
                    {
                        created = Library.TryCreateExhibit(id);
                    }
                    catch
                    {
                        created = null;
                    }

                    if (created is null)
                    {
                        UpdateUIStatus("Trade failed: cannot create received exhibit.");
                        yield break;
                    }

                    try
                    {
                        run.GainExhibitInstantly(created, true, new VisualSourceData { SourceType = VisualSourceType.CardSelect });
                    }
                    catch
                    {
                        UpdateUIStatus("Trade failed: cannot gain received exhibit.");
                        yield break;
                    }
                }
            }

            if (theirs is not null)
            {
                foreach (var c in theirs)
                {
                    if (c is null || string.IsNullOrWhiteSpace(c.CardId))
                    {
                        continue;
                    }

                    Card created = Library.TryCreateCard(c.CardId, c.IsUpgraded, c.UpgradeCounter);
                    if (created is not null)
                    {
                        run.AddDeckCard(created, true, new VisualSourceData
                        {
                            SourceType = VisualSourceType.CardSelect
                        });
                    }
                }
            }
        }

        UpdateUIStatus("Trade.Completed".Localize());
        yield return new WaitForSeconds(TradeCompleteWaitTime);
        Hide();
    }

    private void TryHandlePreparing(TradeSyncPatch.TradeSessionState state)
    {
        GameRunController run = ActiveGameRun;
        if (state is null)
        {
            return;
        }

        // 避免对同一 preparing 阶段发送多次结果。
        if (state.Timestamp > 0 && _lastPreparingHandledTimestamp == state.Timestamp)
        {
            return;
        }

        _lastPreparingHandledTimestamp = state.Timestamp;

        bool localIsA = IsPlayerA(state);

        // 仅严格验证本地自身的报价。
        List<TradeSyncPatch.CardRef> mine = localIsA ? state.OfferA : state.OfferB;
        int myMoney = localIsA ? state.MoneyA : state.MoneyB;
        List<TradeSyncPatch.ExhibitRef> myExhibits = localIsA ? state.ExhibitsA : state.ExhibitsB;

        if ((mine?.Count ?? 0) == 0 && myMoney <= 0 && (myExhibits?.Count ?? 0) == 0)
        {
            TradeSyncPatch.RequestPrepareResult(_tradeId, _selfPlayerId, false, "EmptyOffer");
            return;
        }

        // 卡牌必须已存在（按实例 ID 核查）。
        if (mine is not null)
        {
            foreach (var c in mine)
            {
                if (c is null)
                {
                    continue;
                }

                if (c.InstanceId < 0)
                {
                    TradeSyncPatch.RequestPrepareResult(_tradeId, _selfPlayerId, false, "InvalidInstanceId");
                    return;
                }

                if (TryFindDeckCard(c) is null)
                {
                    TradeSyncPatch.RequestPrepareResult(_tradeId, _selfPlayerId, false, "MissingCard");
                    return;
                }
            }
        }

        // 金币必须足够支付。
        try
        {
            int current = run?.Money ?? 0;
            if (myMoney < 0 || myMoney > current)
            {
                TradeSyncPatch.RequestPrepareResult(_tradeId, _selfPlayerId, false, "InsufficientMoney");
                return;
            }
        }
        catch
        {
            TradeSyncPatch.RequestPrepareResult(_tradeId, _selfPlayerId, false, "MoneyCheckFailed");
            return;
        }

        // 展品必须存在且可交易。
        if (myExhibits is not null)
        {
            foreach (var ex in myExhibits)
            {
                string id = ex?.ExhibitId;
                if (string.IsNullOrWhiteSpace(id))
                {
                    TradeSyncPatch.RequestPrepareResult(_tradeId, _selfPlayerId, false, "InvalidExhibitId");
                    return;
                }

                Exhibit owned = null;
                try
                {
                    owned = run?.Player?.Exhibits?.FirstOrDefault(e => e is not null && string.Equals(e.Id, id, StringComparison.Ordinal));
                }
                catch
                {
                    owned = null;
                }

                if (owned is null)
                {
                    TradeSyncPatch.RequestPrepareResult(_tradeId, _selfPlayerId, false, "MissingExhibit");
                    return;
                }

                if (!TradeExhibitRules.IsTradable(owned))
                {
                    TradeSyncPatch.RequestPrepareResult(_tradeId, _selfPlayerId, false, "ExhibitNotTradable");
                    return;
                }
            }
        }

        TradeSyncPatch.RequestPrepareResult(_tradeId, _selfPlayerId, true, null);
    }
}
