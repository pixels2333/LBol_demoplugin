using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.UI.Factories;
using NetworkPlugin.UI.Panels;
using NetworkPlugin.UI.Payloads;
using NetworkPlugin.UI.Rules;
using NetworkPlugin.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Dialogs;

// 运行时创建的 dialog，不依赖 prefab 预绑定，因此在 mod 环境里也能工作。
public sealed class TradeDetailDialog : UiDialog<TradeDetailPayload>, IInputActionHandler
{
    private const int MaxMoneyOffer = 99999;

    private static GameRunController CurrentGameRun
        => GameStateUtils.GetCurrentGameRun();

    private CanvasGroup _canvasGroup;

    private RectTransform _panelRoot;
    private CommonButtonWidget _buttonTemplate;
    private TextMeshProUGUI _textTemplate;
    private GameObject _rowTemplate;
    private RecordCardCell _cardCellTemplate;
    private ExhibitWidget _exhibitTemplate;

    // 头部区域
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _statusText;

    // 本地报价控件
    private TextMeshProUGUI _localCardsTitle;
    private Transform _localCardsList;
    private Transform _localExhibitsList;
    private TextMeshProUGUI _localMoneyText;
    private TextMeshProUGUI _localExhibitsText;

    private CommonButtonWidget _addCardButton;
    private CommonButtonWidget _editExhibitButton;
    private CommonButtonWidget _moneyMinusButton;
    private CommonButtonWidget _moneyPlusButton;

    // 对方报价展示
    private TextMeshProUGUI _remoteCardsTitle;
    private Transform _remoteCardsList;
    private Transform _remoteExhibitsList;
    private TextMeshProUGUI _remoteMoneyText;
    private TextMeshProUGUI _remoteExhibitsText;

    // 底部按钮
    private CommonButtonWidget _confirmButton;
    private CommonButtonWidget _cancelButton;

    // Payload / 会话状态
    private TradeDetailPayload _payload;
    private string _tradeId;
    private string _selfId;
    private string _partnerId;
    private bool _subscribed;
    private bool _isApplyingState;
    private int _maxSlots = 5;
    private bool _completionApplied;
    private bool _cancelRequested;
    private bool _actionHandlerPushed;
    private long _lastReturnToTradePanelTimestamp;
    private long _lastPreparingHandledTimestamp;
    private TradeSyncPatch.TradeStatus? _lastStatus;
    private readonly List<Card> _initialDeckCards = new List<Card>();

    // 本地报价状态
    private readonly List<Card> _localCards = new List<Card>();
    private int _localMoney;
    private readonly HashSet<string> _localExhibitIds = new HashSet<string>(StringComparer.Ordinal);

    // 选择器弹层
    private GameObject _cardPickerRoot;
    private GameObject _exhibitPickerRoot;
    private readonly List<Card> _cardPickerOriginalCards = new List<Card>();
    private bool _cardPickerEditing;

    internal void BindRuntime(
        CommonButtonWidget buttonTemplate,
        TextMeshProUGUI textTemplate,
        GameObject rowTemplate,
        RecordCardCell cardCellTemplate,
        ExhibitWidget exhibitTemplate,
        RectTransform panelRoot)
    {
        _buttonTemplate = buttonTemplate;
        _textTemplate = textTemplate;
        _rowTemplate = rowTemplate;
        _cardCellTemplate = cardCellTemplate;
        _exhibitTemplate = exhibitTemplate;
        _panelRoot = panelRoot;
    }

    public void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        EnsureUi();
    }

    protected override void OnShowing(TradeDetailPayload payload)
    {
        _payload = payload;
        _cancelRequested = false;
        _actionHandlerPushed = false;

        if (!TryEnsureNetworkConnected())
        {
            Hide(false);
            return;
        }

        _tradeId = string.IsNullOrWhiteSpace(payload?.TradeId) ? Guid.NewGuid().ToString("N") : payload.TradeId;
        _selfId = payload?.SelfPlayerId;
        _partnerId = payload?.PartnerPlayerId;
        _maxSlots = payload?.MaxTradeSlots > 0 ? payload.MaxTradeSlots : 5;
        _completionApplied = false;
        _lastPreparingHandledTimestamp = 0;
        _lastStatus = null;
        _initialDeckCards.Clear();
        if (payload?.InitialDeckCards != null)
        {
            _initialDeckCards.AddRange(payload.InitialDeckCards.Where(c => c != null));
        }

        bool hasRun = GameStateUtils.TryGetCurrentGameRun(out GameRunController currentRun, out string runSource);
        int currentDeckCount = currentRun?.BaseDeck?.Count ?? 0;
        Plugin.Logger?.LogInfo(
            $"[TradeDetailDialog] OnShowing: tradeId={_tradeId ?? payload?.TradeId ?? "<null>"}, self={payload?.SelfPlayerId ?? "<null>"}, partner={payload?.PartnerPlayerId ?? "<null>"}, partnerName={payload?.PartnerPlayerName ?? "<null>"}, maxSlots={payload?.MaxTradeSlots ?? 0}, payloadDeckCount={payload?.InitialDeckCards?.Count ?? 0}, cachedInitialDeckCount={_initialDeckCards.Count}, hasRun={hasRun}, runSource={runSource ?? "<null>"}, currentDeckCount={currentDeckCount}");

        if (string.IsNullOrWhiteSpace(_selfId) || string.IsNullOrWhiteSpace(_partnerId))
        {
            TryShowTopMessage("交易对象无效。");
            Hide(false);
            return;
        }

        ResetLocalOffer();
        EnsureSubscribed();

        _canvasGroup.interactable = true;

        if (UiManager.IsInitialized)
        {
            UiManager.PushActionHandler(this);
            _actionHandlerPushed = true;
        }

        // 启动会话并请求快照，这样两侧报价都能立即渲染出来。
        TradeSyncPatch.RequestStartTrade(_tradeId, _selfId, _partnerId, _maxSlots);
        TradeSyncPatch.RequestSnapshot(_tradeId, _selfId);

        _titleText.text = string.IsNullOrWhiteSpace(payload?.PartnerPlayerName)
            ? "交易"
            : $"交易 - {payload.PartnerPlayerName}";

        _statusText.text = "请选择交易内容";

        RefreshLocalUi();
        RefreshRemoteUi(TradeSyncPatch.GetLastKnown(_tradeId));
    }

    protected override void OnHiding()
    {
        _canvasGroup.interactable = false;

        if (_actionHandlerPushed)
        {
            try
            {
                if (UiManager.IsInitialized)
                {
                    UiManager.PopActionHandler(this);
                }
            }
            catch
            {
                // 忽略
            }
            finally
            {
                _actionHandlerPushed = false;
            }
        }

        // 如果 dialog 不是在明确取消/完成的情况下关闭，补发一次取消请求，避免服务端会话悬空。
        if (!_completionApplied
            && !_cancelRequested
            && !string.IsNullOrWhiteSpace(_tradeId)
            && !string.IsNullOrWhiteSpace(_selfId)
            && _lastStatus != TradeSyncPatch.TradeStatus.Canceled
            && _lastStatus != TradeSyncPatch.TradeStatus.Completed)
        {
            try
            {
                _cancelRequested = true;
                TradeSyncPatch.RequestCancel(_tradeId, _selfId);
            }
            catch
            {
                // ignored
            }
        }

        TryUnsubscribe();

        // 如果选择器还开着，一并关掉。
        CloseCardPickerOverlay(applyChanges: false, closeOnly: true);
        _exhibitPickerRoot?.SetActive(false);

        ScheduleReturnToTradePanel();
    }

    public void OnConfirm()
    {
        OnConfirmClick();
    }

    public void OnCancel()
    {
        // 如果选择器弹层还开着，先只关闭弹层，不取消整场交易。
        if (_cardPickerRoot != null && _cardPickerRoot.activeSelf)
        {
            CloseCardPickerOverlay(applyChanges: false, closeOnly: false);
            return;
        }

        if (_exhibitPickerRoot != null && _exhibitPickerRoot.activeSelf)
        {
            _exhibitPickerRoot.SetActive(false);
            _canvasGroup.interactable = true;
            return;
        }

        // Back / Escape 关闭 dialog，并向服务端发取消请求。
        if (_completionApplied)
        {
            Hide();
            return;
        }

        try
        {
            _cancelRequested = true;
            TradeSyncPatch.RequestCancel(_tradeId, _selfId);
        }
        catch
        {
            // ignored
        }

        Hide();
    }

    private bool TryEnsureNetworkConnected()
    {
        var client = ModService.ServiceProvider.GetService<INetworkClient>();
        if (client == null || !client.IsConnected)
        {
            TryShowTopMessage("交易仅在联机模式下可用。");
            return false;
        }

        TradeSyncPatch.EnsureSubscribed(client);
        return true;
    }

    private void TryShowTopMessage(string message)
    {
        if (!UiManager.IsInitialized)
        {
            return;
        }

        UiManager.GetPanel<TopMessagePanel>().ShowMessage(message);
    }

    private void EnsureSubscribed()
    {
        if (_subscribed)
        {
            return;
        }

        TradeSyncPatch.OnTradeStateUpdated += OnTradeStateUpdated;
        _subscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!_subscribed)
        {
            return;
        }

        TradeSyncPatch.OnTradeStateUpdated -= OnTradeStateUpdated;
        _subscribed = false;
    }

    private void ScheduleReturnToTradePanel()
    {
        // 按当前需求，dialog 关闭后返回 TradePanel，并重新显示 partner picker。
        // 延后一帧执行，避免和 dialog 的 OnHiding 产生输入栈/布局冲突。
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (_lastReturnToTradePanelTimestamp > 0 && now - _lastReturnToTradePanelTimestamp < 450)
        {
            return;
        }
        _lastReturnToTradePanelTimestamp = now;

        // 仅在仍处于联机状态时回到 TradePanel，避免不断弹“交易不可用”。
        var client = ModService.ServiceProvider.GetService<INetworkClient>();
        if (client == null || !client.IsConnected)
        {
            return;
        }

        UniTask.Void(async () =>
        {
            try
            {
                await UniTask.NextFrame();

                if (!UiManager.IsInitialized || !TryGetReturnContextParent(out Transform contextParent))
                {
                    return;
                }

                TradePanel panel = UnityEngine.Object.FindAnyObjectByType<TradePanel>()
                    ?? TradePanelRuntimeFactory.GetOrCreate(contextParent);
                if (panel == null)
                {
                    return;
                }

                if (contextParent != null)
                {
                    // 已存在面板时尽量挂回当前上下文。
                    panel.transform.SetParent(contextParent, false);
                }

                panel.Show(new TradePayload());
            }
            catch
            {
                // 忽略
            }
        });
    }

    private static bool TryGetReturnContextParent(out Transform parent)
    {
        parent = null;

        // 优先回到商店上下文。
        var shop = UiManager.GetPanel<ShopPanel>();
        if (shop != null && shop.IsVisible && shop.transform != null)
        {
            parent = shop.transform.parent != null ? shop.transform.parent : shop.transform;
            return parent != null;
        }

        // 回退到 Gap 上下文。
        var gap = UiManager.GetPanel<GapOptionsPanel>();
        if (gap != null && gap.IsVisible && gap.transform != null)
        {
            parent = gap.transform.parent != null ? gap.transform.parent : gap.transform;
            return parent != null;
        }

        return false;
    }

    private void OnTradeStateUpdated(TradeSyncPatch.TradeSessionState state)
    {
        try
        {
            if (state == null || !string.Equals(state.TradeId, _tradeId, StringComparison.Ordinal))
            {
                return;
            }

            if (!state.IsParticipant(_selfId))
            {
                return;
            }

            using (new ApplyingStateScope(this))
            {
                if (!_cardPickerEditing)
                {
                    SyncLocalFromState(state);
                }
                RefreshRemoteUi(state);
                UpdateConfirmInteractable(state);
            }

            if (state.Status == TradeSyncPatch.TradeStatus.Completed)
            {
                _statusText.text = "交易完成";
                if (!_completionApplied)
                {
                    _completionApplied = true;
                    ApplyCompletedTradeAndClose(state).Forget();
                }
            }
            else if (state.Status == TradeSyncPatch.TradeStatus.Canceled)
            {
                _statusText.text = "交易已取消";
                Hide();
            }
            else if (state.Status == TradeSyncPatch.TradeStatus.Preparing)
            {
                _statusText.text = "确认中...";
                TryHandlePreparing(state);
            }
            else if (state.Status == TradeSyncPatch.TradeStatus.Failed)
            {
                _statusText.text = string.IsNullOrWhiteSpace(state.Reason)
                    ? "交易失败"
                    : $"交易失败: {state.Reason}";
            }
            else
            {
                if (_lastStatus == TradeSyncPatch.TradeStatus.Preparing && !string.IsNullOrWhiteSpace(state.Reason))
                {
                    _statusText.text = $"确认失败: {state.Reason}";
                }
                else
                {
                    _statusText.text = "请选择交易内容";
                }
            }

            _lastStatus = state.Status;
        }
        catch
        {
            // ignored
        }
    }

    private async UniTaskVoid ApplyCompletedTradeAndClose(TradeSyncPatch.TradeSessionState state)
    {
        _canvasGroup.interactable = false;

        if (!TryApplyCompletedTrade(state, out string reason))
        {
            _statusText.text = reason;
            await UniTask.Delay(TimeSpan.FromSeconds(1.6));

            Hide();
            return;
        }

        await UniTask.Delay(TimeSpan.FromSeconds(1.2));

        Hide();
    }

    private bool TryApplyCompletedTrade(TradeSyncPatch.TradeSessionState state, out string reason)
    {
        reason = "交易结算失败。";

        if (state == null)
        {
            reason = "交易结算失败：状态为空。";
            return false;
        }

        var run = CurrentGameRun;
        if (run == null)
        {
            reason = "交易结算失败：游戏状态不可用。";
            return false;
        }

        bool localIsA = string.Equals(state.PlayerAId, _selfId, StringComparison.Ordinal);

        List<TradeSyncPatch.CardRef> mine = localIsA ? state.OfferA : state.OfferB;
        List<TradeSyncPatch.CardRef> theirs = localIsA ? state.OfferB : state.OfferA;

        int myMoney = localIsA ? state.MoneyA : state.MoneyB;
        int theirMoney = localIsA ? state.MoneyB : state.MoneyA;

        List<TradeSyncPatch.ExhibitRef> myExhibits = localIsA ? state.ExhibitsA : state.ExhibitsB;
        List<TradeSyncPatch.ExhibitRef> theirExhibits = localIsA ? state.ExhibitsB : state.ExhibitsA;

        using (TradeSyncPatch.EnterApplyingTradeScope())
        {
            if (mine != null)
            {
                foreach (var cardRef in mine)
                {
                    if (cardRef == null)
                    {
                        continue;
                    }

                    Card owned = TryFindDeckCard(cardRef);
                    if (owned == null)
                    {
                        reason = "交易失败：找不到要移除的卡牌。";
                        return false;
                    }

                    run.RemoveDeckCard(owned, false);
                }
            }

            if (myMoney > 0)
            {
                try
                {
                    run.ConsumeMoney(myMoney);
                }
                catch
                {
                    reason = "交易失败：金币不足。";
                    return false;
                }
            }

            if (theirMoney > 0)
            {
                run.GainMoney(theirMoney, true, new VisualSourceData { SourceType = VisualSourceType.CardSelect });
            }

            if (myExhibits != null)
            {
                foreach (var ex in myExhibits)
                {
                    string id = ex?.ExhibitId;
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        continue;
                    }

                    Exhibit ownedEx = null;
                    try
                    {
                        ownedEx = run.Player?.Exhibits?.FirstOrDefault(e => e != null && string.Equals(e.Id, id, StringComparison.Ordinal));
                    }
                    catch
                    {
                        ownedEx = null;
                    }

                    if (ownedEx == null)
                    {
                        reason = "交易失败：找不到要交易的遗物。";
                        return false;
                    }

                    if (!TradeExhibitRules.IsTradable(ownedEx))
                    {
                        reason = "交易失败：遗物不可交易。";
                        return false;
                    }

                    try
                    {
                        run.LoseExhibit(ownedEx, true, true);
                    }
                    catch
                    {
                        reason = "交易失败：无法移除遗物。";
                        return false;
                    }
                }
            }

            if (theirExhibits != null)
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
                        reason = "交易失败：对方遗物在黑名单内。";
                        return false;
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

                    if (created == null)
                    {
                        reason = "交易失败：无法创建获得的遗物。";
                        return false;
                    }

                    try
                    {
                        run.GainExhibitInstantly(created, true, new VisualSourceData { SourceType = VisualSourceType.CardSelect });
                    }
                    catch
                    {
                        reason = "交易失败：无法获得遗物。";
                        return false;
                    }
                }
            }

            if (theirs != null)
            {
                foreach (var cardRef in theirs)
                {
                    if (cardRef == null || string.IsNullOrWhiteSpace(cardRef.CardId))
                    {
                        continue;
                    }

                    Card created = Library.TryCreateCard(cardRef.CardId, cardRef.IsUpgraded, cardRef.UpgradeCounter);
                    if (created != null)
                    {
                        run.AddDeckCard(created, true, new VisualSourceData { SourceType = VisualSourceType.CardSelect });
                    }
                }
            }
        }

        reason = null;
        return true;
    }

    private void UpdateConfirmInteractable(TradeSyncPatch.TradeSessionState state)
    {
        if (_confirmButton?.button == null)
        {
            return;
        }

        if (state == null)
        {
            _confirmButton.button.interactable = false;
            return;
        }

        bool localIsA = string.Equals(state.PlayerAId, _selfId, StringComparison.Ordinal);
        bool localHasOffer = (localIsA ? (state.OfferA?.Count ?? 0) : (state.OfferB?.Count ?? 0)) > 0
                          || (localIsA ? state.MoneyA : state.MoneyB) > 0
                          || (localIsA ? (state.ExhibitsA?.Count ?? 0) : (state.ExhibitsB?.Count ?? 0)) > 0;
        bool remoteHasOffer = (localIsA ? (state.OfferB?.Count ?? 0) : (state.OfferA?.Count ?? 0)) > 0
                           || (localIsA ? state.MoneyB : state.MoneyA) > 0
                           || (localIsA ? (state.ExhibitsB?.Count ?? 0) : (state.ExhibitsA?.Count ?? 0)) > 0;

        _confirmButton.button.interactable = state.Status == TradeSyncPatch.TradeStatus.Open && localHasOffer && remoteHasOffer;
    }

    private void SyncLocalFromState(TradeSyncPatch.TradeSessionState state)
    {
        if (state == null)
        {
            return;
        }

        bool localIsA = string.Equals(state.PlayerAId, _selfId, StringComparison.Ordinal);

        _localMoney = Mathf.Max(0, localIsA ? state.MoneyA : state.MoneyB);

        _localExhibitIds.Clear();
        foreach (var ex in localIsA ? state.ExhibitsA : state.ExhibitsB)
        {
            if (ex != null && !string.IsNullOrWhiteSpace(ex.ExhibitId))
            {
                _localExhibitIds.Add(ex.ExhibitId);
            }
        }

        _localCards.Clear();
        var localOffer = localIsA ? state.OfferA : state.OfferB;
        if (localOffer != null)
        {
            foreach (var cardRef in localOffer)
            {
                Card owned = TryFindDeckCard(cardRef);
                if (owned != null)
                {
                    _localCards.Add(owned);
                }
            }
        }

        RefreshLocalUi();
    }

    private Card TryFindDeckCard(TradeSyncPatch.CardRef cardRef)
    {
        if (cardRef == null || cardRef.InstanceId < 0)
        {
            return null;
        }

        return CurrentGameRun?.GetDeckCardByInstanceId(cardRef.InstanceId);
    }

    private void TryHandlePreparing(TradeSyncPatch.TradeSessionState state)
    {
        if (state == null)
        {
            return;
        }

        if (state.Timestamp > 0 && _lastPreparingHandledTimestamp == state.Timestamp)
        {
            return;
        }

        _lastPreparingHandledTimestamp = state.Timestamp;

        bool localIsA = string.Equals(state.PlayerAId, _selfId, StringComparison.Ordinal);

        List<TradeSyncPatch.CardRef> mine = localIsA ? state.OfferA : state.OfferB;
        int myMoney = localIsA ? state.MoneyA : state.MoneyB;
        List<TradeSyncPatch.ExhibitRef> myExhibits = localIsA ? state.ExhibitsA : state.ExhibitsB;

        if ((mine?.Count ?? 0) == 0 && myMoney <= 0 && (myExhibits?.Count ?? 0) == 0)
        {
            TradeSyncPatch.RequestPrepareResult(_tradeId, _selfId, false, "EmptyOffer");
            return;
        }

        if (mine != null)
        {
            foreach (var c in mine)
            {
                if (c == null)
                {
                    continue;
                }

                if (c.InstanceId < 0)
                {
                    TradeSyncPatch.RequestPrepareResult(_tradeId, _selfId, false, "InvalidInstanceId");
                    return;
                }

                if (TryFindDeckCard(c) == null)
                {
                    TradeSyncPatch.RequestPrepareResult(_tradeId, _selfId, false, "MissingCard");
                    return;
                }
            }
        }

        int currentMoney = CurrentGameRun?.Money ?? 0;

        if (myMoney < 0 || myMoney > currentMoney)
        {
            TradeSyncPatch.RequestPrepareResult(_tradeId, _selfId, false, "InsufficientMoney");
            return;
        }

        if (myExhibits != null)
        {
            var run = CurrentGameRun;
            foreach (var ex in myExhibits)
            {
                string id = ex?.ExhibitId;
                if (string.IsNullOrWhiteSpace(id))
                {
                    TradeSyncPatch.RequestPrepareResult(_tradeId, _selfId, false, "InvalidExhibitId");
                    return;
                }

                Exhibit owned = run?.Player?.Exhibits?.FirstOrDefault(e => e != null && string.Equals(e.Id, id, StringComparison.Ordinal));

                if (owned == null)
                {
                    TradeSyncPatch.RequestPrepareResult(_tradeId, _selfId, false, "MissingExhibit");
                    return;
                }

                if (!TradeExhibitRules.IsTradable(owned))
                {
                    TradeSyncPatch.RequestPrepareResult(_tradeId, _selfId, false, "ExhibitNotTradable");
                    return;
                }
            }
        }

        TradeSyncPatch.RequestPrepareResult(_tradeId, _selfId, true, null);
    }

    private void ResetLocalOffer()
    {
        _localCards.Clear();
        _localMoney = 0;
        _localExhibitIds.Clear();
    }

    private void RefreshLocalUi()
    {
        _localMoneyText.text = $"金币: {_localMoney}";
        _localExhibitsText.text = $"遗物: {_localExhibitIds.Count}";

        RebuildCardList(_localCardsList, _localCards, isLocal: true);
        RebuildExhibitList(_localExhibitsList, _localExhibitIds.ToList(), isLocal: true);
    }

    private void RefreshRemoteUi(TradeSyncPatch.TradeSessionState state)
    {
        if (state == null)
        {
            _remoteMoneyText.text = "金币: 0";
            _remoteExhibitsText.text = "遗物: 0";
            RebuildCardList(_remoteCardsList, (List<TradeSyncPatch.CardRef>)null, isLocal: false);
            RebuildExhibitList(_remoteExhibitsList, (List<string>)null, isLocal: false);
            return;
        }

        bool localIsA = string.Equals(state.PlayerAId, _selfId, StringComparison.Ordinal);

        int theirMoney = localIsA ? state.MoneyB : state.MoneyA;
        List<TradeSyncPatch.ExhibitRef> theirEx = localIsA ? state.ExhibitsB : state.ExhibitsA;

        _remoteMoneyText.text = $"金币: {Mathf.Max(0, theirMoney)}";
        _remoteExhibitsText.text = $"遗物: {theirEx?.Count ?? 0}";

        var theirCards = localIsA ? state.OfferB : state.OfferA;
        RebuildCardList(_remoteCardsList, theirCards, isLocal: false);
        RebuildExhibitList(_remoteExhibitsList, theirEx, isLocal: false);
    }

    private void RebuildExhibitList(Transform listRoot, List<string> exhibitIds, bool isLocal)
    {
        if (listRoot == null) return;
        foreach (Transform c in listRoot) Destroy(c.gameObject);

        if (exhibitIds == null || exhibitIds.Count == 0) return;

        foreach (var exId in exhibitIds)
        {
            if (string.IsNullOrEmpty(exId)) continue;
            
            var label = exId;

            var row = CloneListItem(listRoot, $"Ex_{exId}", label, false);
            if (isLocal)
            {
                row.button.onClick.AddListener(() =>
                {
                    AudioManager.Button(0);
                    _localExhibitIds.Remove(exId);
                    RefreshLocalUi();
                    TrySendOfferUpdate();
                });
            }
            else
            {
                row.button.interactable = false;
            }
        }
    }

    private void RebuildExhibitList(Transform listRoot, List<TradeSyncPatch.ExhibitRef> exhibits, bool isLocal)
    {
        if (listRoot == null) return;
        foreach (Transform c in listRoot) Destroy(c.gameObject);

        if (exhibits == null || exhibits.Count == 0) return;

        foreach (var ex in exhibits)
        {
            var exId = ex?.ExhibitId;
            if (string.IsNullOrEmpty(exId)) continue;

            var row = CloneListItem(listRoot, $"Ex_{exId}", exId, false);
            row.button.interactable = false;
        }
    }

    private void EnsureUi()
    {
        if (_panelRoot == null)
        {
            RectTransform panel = transform.Find("Panel") as RectTransform;
            _panelRoot = panel != null ? panel : GetComponent<RectTransform>();
        }

        var bg = _panelRoot.GetComponent<Image>();
        if (bg == null) bg = _panelRoot.gameObject.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.04f, 0.04f, 0.85f); 

        _titleText = CloneText(_panelRoot, "Title", 30, TextAlignmentOptions.Center);
        SetRect(_titleText.rectTransform, 0.06f, 0.90f, 0.94f, 0.98f);

        _statusText = CloneText(_panelRoot, "Status", 18, TextAlignmentOptions.Center);
        SetRect(_statusText.rectTransform, 0.06f, 0.85f, 0.94f, 0.90f);

        // --- Left Side (Local) ---
        _localCardsTitle = CloneText(_panelRoot, "LocalTitle", 22, TextAlignmentOptions.Left);
        _localCardsTitle.text = "我的报价";
        SetRect(_localCardsTitle.rectTransform, 0.06f, 0.80f, 0.46f, 0.85f);

        _localCardsList = CreateScrollList(_panelRoot, "LocalCards", 0.06f, 0.55f, 0.46f, 0.78f);
        AddListBackground(_localCardsList);

        _localExhibitsList = CreateScrollList(_panelRoot, "LocalExhibits", 0.06f, 0.35f, 0.46f, 0.53f);
        AddListBackground(_localExhibitsList);

        // 操作按钮放到报价区域附近，减少来回移动视线。
        _addCardButton = CloneButton(_panelRoot, "AddCard", "+ 卡牌");
        SetRect(_addCardButton.GetComponent<RectTransform>(), 0.06f, 0.28f, 0.18f, 0.33f);
        _addCardButton.button.onClick.RemoveAllListeners();
        _addCardButton.button.onClick.AddListener(new UnityAction(ShowCardPicker));

        _editExhibitButton = CloneButton(_panelRoot, "EditEx", "+ 遗物");
        SetRect(_editExhibitButton.GetComponent<RectTransform>(), 0.19f, 0.28f, 0.31f, 0.33f);
        _editExhibitButton.button.onClick.RemoveAllListeners();
        _editExhibitButton.button.onClick.AddListener(new UnityAction(ShowExhibitPicker));

        _localMoneyText = CloneText(_panelRoot, "LocalMoney", 18, TextAlignmentOptions.Left);
        SetRect(_localMoneyText.rectTransform, 0.06f, 0.23f, 0.22f, 0.27f);

        _moneyMinusButton = CloneButton(_panelRoot, "MoneyMinus", "-");
        SetRect(_moneyMinusButton.GetComponent<RectTransform>(), 0.22f, 0.23f, 0.26f, 0.27f);
        _moneyMinusButton.button.onClick.RemoveAllListeners();
        _moneyMinusButton.button.onClick.AddListener(new UnityAction(() => ChangeMoney(-10)));

        _moneyPlusButton = CloneButton(_panelRoot, "MoneyPlus", "+");
        SetRect(_moneyPlusButton.GetComponent<RectTransform>(), 0.27f, 0.23f, 0.31f, 0.27f);
        _moneyPlusButton.button.onClick.RemoveAllListeners();
        _moneyPlusButton.button.onClick.AddListener(new UnityAction(() => ChangeMoney(+10)));

        _localExhibitsText = CloneText(_panelRoot, "LocalEx", 18, TextAlignmentOptions.Left);
        SetRect(_localExhibitsText.rectTransform, 0.34f, 0.23f, 0.46f, 0.27f);


        // --- Right Side (Remote) ---
        _remoteCardsTitle = CloneText(_panelRoot, "RemoteTitle", 22, TextAlignmentOptions.Left);
        _remoteCardsTitle.text = "对方报价";
        SetRect(_remoteCardsTitle.rectTransform, 0.54f, 0.80f, 0.94f, 0.85f);

        _remoteCardsList = CreateScrollList(_panelRoot, "RemoteCards", 0.54f, 0.55f, 0.94f, 0.78f);
        AddListBackground(_remoteCardsList);

        _remoteExhibitsList = CreateScrollList(_panelRoot, "RemoteExhibits", 0.54f, 0.35f, 0.94f, 0.53f);
        AddListBackground(_remoteExhibitsList);

        _remoteMoneyText = CloneText(_panelRoot, "RemoteMoney", 18, TextAlignmentOptions.Left);
        SetRect(_remoteMoneyText.rectTransform, 0.54f, 0.23f, 0.78f, 0.27f);

        _remoteExhibitsText = CloneText(_panelRoot, "RemoteEx", 18, TextAlignmentOptions.Left);
        SetRect(_remoteExhibitsText.rectTransform, 0.78f, 0.23f, 0.94f, 0.27f);

        // --- Bottom Controls ---
        _confirmButton = CloneButton(_panelRoot, "Confirm", "确认交易");
        SetRect(_confirmButton.GetComponent<RectTransform>(), 0.32f, 0.08f, 0.48f, 0.16f);
        _confirmButton.button.onClick.RemoveAllListeners();
        _confirmButton.button.onClick.AddListener(new UnityAction(OnConfirmClick));

        _cancelButton = CloneButton(_panelRoot, "Cancel", "取消交易");
        SetRect(_cancelButton.GetComponent<RectTransform>(), 0.52f, 0.08f, 0.68f, 0.16f);
        _cancelButton.button.onClick.RemoveAllListeners();
        _cancelButton.button.onClick.AddListener(new UnityAction(OnCancelClick));

        DisableTooltipBehaviours(gameObject);
    }

    private void AddListBackground(Transform listRoot)
    {
        if (listRoot == null || listRoot.parent == null) return;
        var parent = listRoot.parent;
        
        // 在 ScrollRect 背后补一个背景对象，没有就创建。
        var bgName = "ListBg_" + listRoot.name;
        var bgTransform = parent.Find(bgName);
        if (bgTransform == null)
        {
            GameObject go = new GameObject(bgName);
            go.transform.SetParent(parent, false);
            go.transform.SetAsFirstSibling();
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-4, -4);
            rt.offsetMax = new Vector2(4, 4);
            
            var img = go.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.35f); // 简单的半透明深色底。
        }
    }

    private void OnConfirmClick()
    {
        try
        {
            TradeSyncPatch.TradeSessionState state = TradeSyncPatch.GetLastKnown(_tradeId);
            if (state == null)
            {
                return;
            }

            UpdateConfirmInteractable(state);
            if (_confirmButton?.button != null && !_confirmButton.button.interactable)
            {
                return;
            }

            // 播放确认音效。
            AudioManager.Button(0);

            TradeSyncPatch.RequestConfirm(_tradeId, _selfId);
            _statusText.text = "已确认，等待对方...";
        }
        catch
        {
            // 忽略
        }
    }

    private void OnCancelClick()
    {
        AudioManager.Button(0);
        OnCancel();
    }

    private void ChangeMoney(int delta)
    {
        if (_isApplyingState)
        {
            return;
        }

        int current = CurrentGameRun?.Money ?? 0;

        int next = Mathf.Clamp(_localMoney + delta, 0, Mathf.Min(MaxMoneyOffer, current));
        if (next != _localMoney)
        {
            AudioManager.Button(0);
            _localMoney = next;
            RefreshLocalUi();
            TrySendOfferUpdate();
        }
    }

    private void ShowCardPicker()
    {
        if (_isApplyingState)
        {
            return;
        }

        bool hasRun = GameStateUtils.TryGetCurrentGameRun(out GameRunController currentRun, out string runSource);
        int currentDeckCount = currentRun?.BaseDeck?.Count ?? 0;
        Plugin.Logger?.LogInfo(
            $"[TradeDetailDialog] ShowCardPicker: tradeId={_tradeId ?? "<null>"}, localCards={_localCards.Count}, initialDeckCount={_initialDeckCards.Count}, hasRun={hasRun}, runSource={runSource ?? "<null>"}, currentDeckCount={currentDeckCount}, pickerExists={(_cardPickerRoot != null)}");

        AudioManager.Card(3);
        EnsureCardPicker();
        if (_cardPickerRoot == null)
        {
            TryShowTopMessage("卡牌选择界面不可用。");
            return;
        }
        BeginCardPickerEdit();
        RebuildCardPicker();
        _cardPickerRoot.SetActive(true);
        _canvasGroup.interactable = false;
        StartCoroutine(CoRefreshCardPickerNextFrame());
    }

    private System.Collections.IEnumerator CoRefreshCardPickerNextFrame()
    {
        yield return null;

        if (_cardPickerRoot == null || !_cardPickerRoot.activeSelf)
        {
            Plugin.Logger?.LogInfo($"[TradeDetailDialog] CoRefreshCardPickerNextFrame skipped: tradeId={_tradeId ?? "<null>"}, pickerExists={(_cardPickerRoot != null)}, pickerActive={(_cardPickerRoot != null && _cardPickerRoot.activeSelf)}");
            yield break;
        }

        Plugin.Logger?.LogInfo($"[TradeDetailDialog] CoRefreshCardPickerNextFrame rebuilding: tradeId={_tradeId ?? "<null>"}");
        RebuildCardPicker();
    }

    private void ShowExhibitPicker()
    {
        if (_isApplyingState)
        {
            return;
        }

        AudioManager.Card(3);
        EnsureExhibitPicker();
        RebuildExhibitPicker();
        _exhibitPickerRoot.SetActive(true);
        _canvasGroup.interactable = false;
    }

    private void EnsureCardPicker()
    {
        if (_cardPickerRoot != null)
        {
            return;
        }

        _cardPickerRoot = CreateCardSelectionOverlay();
        Plugin.Logger?.LogInfo($"[TradeDetailDialog] EnsureCardPicker created: tradeId={_tradeId ?? "<null>"}, pickerExists={(_cardPickerRoot != null)}");
    }

    private void BeginCardPickerEdit()
    {
        _cardPickerOriginalCards.Clear();
        _cardPickerOriginalCards.AddRange(_localCards.Where(c => c != null));
        _cardPickerEditing = true;
    }

    private void CloseCardPickerOverlay(bool applyChanges, bool closeOnly)
    {
        if (_cardPickerRoot == null || !_cardPickerRoot.activeSelf)
        {
            _cardPickerEditing = false;
            return;
        }

        if (!applyChanges)
        {
            _localCards.Clear();
            _localCards.AddRange(_cardPickerOriginalCards.Where(c => c != null));
        }

        _cardPickerRoot.SetActive(false);
        _canvasGroup.interactable = true;
        _cardPickerEditing = false;

        if (!closeOnly)
        {
            RefreshLocalUi();
            RefreshRemoteUi(TradeSyncPatch.GetLastKnown(_tradeId));
        }
    }

    private void ConfirmCardPickerSelection()
    {
        if (_cardPickerRoot == null)
        {
            return;
        }

        CardPickerPanelTag tag = _cardPickerRoot.GetComponent<CardPickerPanelTag>();
        int targetCount = tag?.TargetSelectCount ?? 0;
        if (targetCount > 0 && _localCards.Count != targetCount)
        {
            if (tag?.HintText != null)
            {
                tag.HintText.text = $"请选择 {targetCount} 张卡牌（当前 {_localCards.Count}/{targetCount}）";
            }
            return;
        }

        CloseCardPickerOverlay(applyChanges: true, closeOnly: false);
        TrySendOfferUpdate();
    }

    private GameObject CreateCardSelectionOverlay()
    {
        // 独立交易确认面板：上半显示对方卡牌，下半显示我方卡牌，底部为可选卡牌。
        try
        {
            GameObject root = new GameObject("CardPicker");
            root.transform.SetParent(transform, false);
            root.SetActive(false);

            RectTransform rt = root.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image blocker = root.AddComponent<Image>();
            blocker.color = new Color(0f, 0f, 0f, 0f);
            blocker.raycastTarget = true;

            GameObject prefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (prefab == null)
            {
                return CreateFullOverlayFallback(root, rt, "交易确认面板");
            }

            GameObject frame = Instantiate(prefab, root.transform, false);
            frame.name = "Frame";
            frame.SetActive(true);

            RectTransform frameRt = frame.GetComponent<RectTransform>();
            if (frameRt != null)
            {
                frameRt.anchorMin = new Vector2(0.05f, 0.05f);
                frameRt.anchorMax = new Vector2(0.95f, 0.95f);
                frameRt.offsetMin = Vector2.zero;
                frameRt.offsetMax = Vector2.zero;
            }

            MessageDialog dialog = frame.GetComponentInChildren<MessageDialog>(true);
            if (dialog == null)
            {
                return CreateFullOverlayFallback(root, rt, "交易确认面板");
            }

            TextMeshProUGUI mainText = GetDialogField<TextMeshProUGUI>(dialog, "mainText");
            TextMeshProUGUI subText = GetDialogField<TextMeshProUGUI>(dialog, "subText");
            Button singleConfirm = GetDialogField<Button>(dialog, "singleConfirmButton");
            Button confirm = GetDialogField<Button>(dialog, "confirmButton");
            Button cancel = GetDialogField<Button>(dialog, "cancelButton");

            HideDialogButton(singleConfirm);

            if (mainText != null)
            {
                mainText.gameObject.SetActive(true);
                mainText.text = "交易确认面板";
                mainText.alignment = TextAlignmentOptions.Center;
                mainText.raycastTarget = false;
                Color c = mainText.color;
                c.a = 1f;
                mainText.color = c;
            }

            RectTransform panelRect = TryFindCommonAncestorRect(mainText?.rectTransform, cancel?.GetComponent<RectTransform>())
                ?? (frameRt != null ? frameRt : frame.GetComponent<RectTransform>());
            if (panelRect == null)
            {
                panelRect = rt;
            }

            if (subText != null)
            {
                subText.gameObject.SetActive(false);
            }

            TextMeshProUGUI hint = CloneText(panelRect, "CardPickerHint", 18, TextAlignmentOptions.Center);
            SetRect(hint.rectTransform, 0.08f, 0.84f, 0.92f, 0.89f);

            TextMeshProUGUI remoteTitle = CloneText(panelRect, "RemoteSelectedTitle", 20, TextAlignmentOptions.Left);
            remoteTitle.text = "对方已选卡牌";
            SetRect(remoteTitle.rectTransform, 0.08f, 0.77f, 0.92f, 0.82f);

            Transform remoteList = CreateScrollList(panelRect, "RemoteSelectedList", 0.08f, 0.60f, 0.92f, 0.76f);
            AddListBackground(remoteList);

            TextMeshProUGUI localTitle = CloneText(panelRect, "LocalSelectedTitle", 20, TextAlignmentOptions.Left);
            localTitle.text = "我方已选卡牌";
            SetRect(localTitle.rectTransform, 0.08f, 0.53f, 0.92f, 0.58f);

            Transform localList = CreateScrollList(panelRect, "LocalSelectedList", 0.08f, 0.36f, 0.92f, 0.52f);
            AddListBackground(localList);

            TextMeshProUGUI availableTitle = CloneText(panelRect, "AvailableCardTitle", 20, TextAlignmentOptions.Left);
            availableTitle.text = "可选卡牌";
            SetRect(availableTitle.rectTransform, 0.08f, 0.29f, 0.92f, 0.34f);

            Transform candidateList = CreateScrollList(panelRect, "AvailableCardList", 0.08f, 0.14f, 0.92f, 0.28f);
            AddListBackground(candidateList);

            CommonButtonWidget backButton = CloneButton(panelRect, "CardPickerBack", "返回");
            SetRect(backButton.GetComponent<RectTransform>(), 0.26f, 0.05f, 0.44f, 0.11f);
            backButton.button.onClick.RemoveAllListeners();
            backButton.button.onClick.AddListener(() => CloseCardPickerOverlay(applyChanges: false, closeOnly: false));

            CommonButtonWidget confirmButtonWidget = CloneButton(panelRect, "CardPickerConfirm", "确认选牌");
            SetRect(confirmButtonWidget.GetComponent<RectTransform>(), 0.56f, 0.05f, 0.74f, 0.11f);
            confirmButtonWidget.button.onClick.RemoveAllListeners();
            confirmButtonWidget.button.onClick.AddListener(ConfirmCardPickerSelection);

            CardPickerPanelTag panelTag = root.AddComponent<CardPickerPanelTag>();
            panelTag.HintText = hint;
            panelTag.RemoteSelectedList = remoteList;
            panelTag.LocalSelectedList = localList;
            panelTag.CandidateList = candidateList;
            panelTag.ConfirmButton = confirmButtonWidget;

            dialog.enabled = false;
            return root;
        }
        catch
        {
            return null;
        }
    }

    private void EnsureExhibitPicker()
    {
        if (_exhibitPickerRoot != null)
        {
            return;
        }

        _exhibitPickerRoot = CreateFullOverlay("ExhibitPicker", "选择要交易的遗物");
    }

    private GameObject CreateFullOverlay(string name, string titleText)
    {
        // 优先复用游戏内 MessageDialog prefab，保证视觉风格一致。
        try
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(transform, false);
            root.SetActive(false);

            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // 透明遮罩层，避免 prefab 的 mask 失效时点击穿透到底层。
            var blocker = root.AddComponent<Image>();
            blocker.color = new Color(0f, 0f, 0f, 0f);
            blocker.raycastTarget = true;

            var prefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (prefab == null)
            {
                return CreateFullOverlayFallback(root, rt, titleText);
            }

            var frame = Instantiate(prefab, root.transform, false);
            frame.name = "Frame";
            frame.SetActive(true);

            var frameRt = frame.GetComponent<RectTransform>();
            if (frameRt != null)
            {
                frameRt.anchorMin = new Vector2(0.06f, 0.06f);
                frameRt.anchorMax = new Vector2(0.94f, 0.94f);
                frameRt.offsetMin = Vector2.zero;
                frameRt.offsetMax = Vector2.zero;
            }

            var dialog = frame.GetComponentInChildren<MessageDialog>(true);
            if (dialog == null)
            {
                return CreateFullOverlayFallback(root, rt, titleText);
            }

            var mainText = GetDialogField<TextMeshProUGUI>(dialog, "mainText");
            var subText = GetDialogField<TextMeshProUGUI>(dialog, "subText");
            var singleConfirm = GetDialogField<Button>(dialog, "singleConfirmButton");
            var confirm = GetDialogField<Button>(dialog, "confirmButton");
            var cancel = GetDialogField<Button>(dialog, "cancelButton");

            HideDialogButton(singleConfirm);

            if (mainText != null)
            {
                mainText.gameObject.SetActive(true);
                mainText.text = titleText;
                mainText.alignment = TextAlignmentOptions.Center;
                mainText.raycastTarget = false;
                var c = mainText.color;
                c.a = 1f;
                mainText.color = c;
            }

            // 保留 subText 参与布局，但把它隐藏掉，避免 prefab 排版塌掉。
            RectTransform panelRect = null;
            RectTransform subTextRect = subText?.rectTransform;
            if (subText != null)
            {
                subText.gameObject.SetActive(true);
                subText.text = string.Empty;
                subText.raycastTarget = false;
                var c = subText.color;
                c.a = 0f;
                subText.color = c;
            }

            panelRect = TryFindCommonAncestorRect(mainText?.rectTransform,
                cancel?.GetComponent<RectTransform>())
                ?? (frameRt != null ? frameRt : frame.GetComponent<RectTransform>());

            if (panelRect == null)
            {
                panelRect = rt;
            }

            // 把列表放到中间区域。
            var list = CreateScrollList(panelRect, "List", 0.10f, 0.18f, 0.90f, 0.86f);
            _ = list.gameObject.AddComponent<PickerListTag>();

            if (cancel != null)
            {
                cancel.onClick.RemoveAllListeners();
                cancel.gameObject.SetActive(true);
                SetButtonLabel(cancel, "取消");
                cancel.onClick.AddListener(() =>
                {
                    root.SetActive(false);
                    _canvasGroup.interactable = true;
                });
            }

            if (confirm != null)
            {
                confirm.onClick.RemoveAllListeners();
                confirm.gameObject.SetActive(true);
                SetButtonLabel(confirm, "确定");
                confirm.onClick.AddListener(() =>
                {
                    root.SetActive(false);
                    _canvasGroup.interactable = true;
                    RefreshLocalUi();
                    TrySendOfferUpdate();
                });
            }

            // 确保列表不会压在按钮上面。
            if (cancel != null)
            {
                cancel.transform.SetAsLastSibling();
            }
            if (confirm != null)
            {
                confirm.transform.SetAsLastSibling();
            }

            dialog.enabled = false;
            return root;
        }
        catch
        {
            // 如果构建原生风格 overlay 失败，就回退到旧的运行时实现。
            GameObject root = new GameObject(name);
            root.transform.SetParent(transform, false);
            root.SetActive(false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return CreateFullOverlayFallback(root, rt, titleText);
        }
    }

    private GameObject CreateFullOverlayFallback(GameObject root, RectTransform rt, string titleText)
    {
        try
        {
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);
            bg.raycastTarget = true;

            var title = CloneText(rt, "Title", 26, TextAlignmentOptions.Center);
            title.text = titleText;
            SetRect(title.rectTransform, 0.10f, 0.88f, 0.90f, 0.97f);

            var list = CreateScrollList(rt, "List", 0.10f, 0.18f, 0.90f, 0.86f);
            _ = list.gameObject.AddComponent<PickerListTag>();

            var cancel = CloneButton(rt, "Cancel", "取消");
            SetRect(cancel.GetComponent<RectTransform>(), 0.25f, 0.06f, 0.45f, 0.14f);
            cancel.button.onClick.RemoveAllListeners();
            cancel.button.onClick.AddListener(new UnityAction(() =>
            {
                root.SetActive(false);
                _canvasGroup.interactable = true;
            }));

            var ok = CloneButton(rt, "OK", "确定");
            SetRect(ok.GetComponent<RectTransform>(), 0.55f, 0.06f, 0.75f, 0.14f);
            ok.button.onClick.RemoveAllListeners();
            ok.button.onClick.AddListener(new UnityAction(() =>
            {
                root.SetActive(false);
                _canvasGroup.interactable = true;
                RefreshLocalUi();
                TrySendOfferUpdate();
            }));
        }
        catch
        {
            // ignored
        }

        return root;
    }

    private static void HideDialogButton(Button b)
    {
        if (b == null)
        {
            return;
        }

        b.onClick.RemoveAllListeners();
        b.gameObject.SetActive(false);
    }

    private static T GetDialogField<T>(MessageDialog dialog, string fieldName) where T : class
    {
        if (dialog == null || string.IsNullOrWhiteSpace(fieldName))
        {
            return null;
        }

        var fi = typeof(MessageDialog).GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (fi == null)
        {
            return null;
        }

        return fi.GetValue(dialog) as T;
    }

    private static void SetButtonLabel(Button button, string label)
    {
        if (button == null)
        {
            return;
        }

        var tmp = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp != null)
        {
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
        }
    }

    private static RectTransform TryFindCommonAncestorRect(RectTransform a, RectTransform b)
    {
        if (a == null || b == null)
        {
            return null;
        }

        HashSet<Transform> ancestors = new HashSet<Transform>();
        Transform t = a;
        while (t != null)
        {
            ancestors.Add(t);
            t = t.parent;
        }

        Transform u = b;
        while (u != null)
        {
            if (ancestors.Contains(u))
            {
                return u as RectTransform;
            }

            u = u.parent;
        }

        return null;
    }

    private sealed class PickerListTag : MonoBehaviour
    {
    }

    private sealed class CardPickerPanelTag : MonoBehaviour
    {
        public TextMeshProUGUI HintText;
        public Transform RemoteSelectedList;
        public Transform LocalSelectedList;
        public Transform CandidateList;
        public CommonButtonWidget ConfirmButton;
        public int TargetSelectCount;
    }

    private void RebuildCardPicker()
    {
        if (_cardPickerRoot == null)
        {
            Plugin.Logger?.LogWarning($"[TradeDetailDialog] RebuildCardPicker aborted: picker root is null, tradeId={_tradeId ?? "<null>"}");
            return;
        }

        CardPickerPanelTag tag = _cardPickerRoot.GetComponent<CardPickerPanelTag>();
        if (tag == null)
        {
            Plugin.Logger?.LogWarning($"[TradeDetailDialog] RebuildCardPicker aborted: CardPickerPanelTag missing, tradeId={_tradeId ?? "<null>"}");
            return;
        }

        Transform candidateContainer = tag.CandidateList;
        Transform localContainer = tag.LocalSelectedList;
        Transform remoteContainer = tag.RemoteSelectedList;
        if (candidateContainer == null || localContainer == null || remoteContainer == null)
        {
            return;
        }

        // 可选卡牌列表改用 Grid Layout，更接近游戏原生卡库的观看方式。
        var vlg = candidateContainer.GetComponent<VerticalLayoutGroup>();
        if (vlg != null) DestroyImmediate(vlg);
        
        var glg = candidateContainer.GetComponent<GridLayoutGroup>();
        if (glg == null) glg = candidateContainer.gameObject.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(160, 100);
        glg.spacing = new Vector2(10, 10);
        glg.padding = new RectOffset(10, 10, 10, 10);
        glg.childAlignment = TextAnchor.UpperLeft;

        foreach (Transform c in candidateContainer)
        {
            Destroy(c.gameObject);
        }

        foreach (Transform c in localContainer)
        {
            Destroy(c.gameObject);
        }

        foreach (Transform c in remoteContainer)
        {
            Destroy(c.gameObject);
        }

        bool hasRun = GameStateUtils.TryGetCurrentGameRun(out GameRunController currentRun, out string runSource);
        List<Card> deck = currentRun?.BaseDeck?
            .Where(c => c != null)
            .OrderBy(c => c.Name)
            .ToList();

        string deckSource = "CurrentGameRun";

        if (deck == null || deck.Count == 0)
        {
            deck = _initialDeckCards
                .Where(c => c != null)
                .OrderBy(c => c.Name)
                .ToList();
            deckSource = "InitialDeckCards";
        }

        deck ??= new List<Card>();

        HashSet<int> selected = new HashSet<int>(_localCards.Where(c => c != null).Select(c => c.InstanceId));
        int candidateCount = deck.Count(card => card != null && !selected.Contains(card.InstanceId));

        int desiredCount = Mathf.Max(1, Mathf.Min(3, _maxSlots));
        int maxSelectableNow = Mathf.Max(1, Mathf.Min(desiredCount, _localCards.Count + candidateCount));
        tag.TargetSelectCount = maxSelectableNow;

        while (_localCards.Count > maxSelectableNow)
        {
            _localCards.RemoveAt(_localCards.Count - 1);
        }

        if (tag.HintText != null)
        {
            tag.HintText.text = $"请选择 {maxSelectableNow} 张卡牌（当前 {_localCards.Count}/{maxSelectableNow}）";
        }

        if (tag.ConfirmButton?.button != null)
        {
            tag.ConfirmButton.button.interactable = _localCards.Count == maxSelectableNow;
        }

        Plugin.Logger?.LogInfo(
            $"[TradeDetailDialog] RebuildCardPicker: tradeId={_tradeId ?? "<null>"}, hasRun={hasRun}, runSource={runSource ?? "<null>"}, currentDeckCount={(currentRun?.BaseDeck?.Count ?? 0)}, initialDeckCount={_initialDeckCards.Count}, selectedCount={selected.Count}, chosenDeckSource={deckSource}, chosenDeckCount={deck.Count}, candidateCount={candidateCount}, target={maxSelectableNow}, cardCellTemplate={(_cardCellTemplate != null)}, pickerActive={(_cardPickerRoot != null && _cardPickerRoot.activeSelf)}");

        // 先渲染“我方已选卡牌”（可点击移除）。
        if (_localCards.Count == 0)
        {
            CommonButtonWidget empty = CloneListItem(localContainer, "LocalEmpty", "(无)", false);
            empty.button.interactable = false;
        }
        else
        {
            for (int i = 0; i < _localCards.Count; i++)
            {
                Card selectedCard = _localCards[i];
                if (selectedCard == null)
                {
                    continue;
                }

                string label = $"{selectedCard.Name}{(selectedCard.IsUpgraded ? "+" : string.Empty)}  (点击移除)";
                CommonButtonWidget row = CloneListItem(localContainer, $"LocalSelected_{selectedCard.InstanceId}_{i}", label, false);
                row.button.onClick.RemoveAllListeners();
                row.button.onClick.AddListener(new UnityAction(() =>
                {
                    _localCards.Remove(selectedCard);
                    RebuildCardPicker();
                }));
            }
        }

        // 渲染“对方已选卡牌”（只读）。
        TradeSyncPatch.TradeSessionState state = TradeSyncPatch.GetLastKnown(_tradeId);
        bool localIsA = state != null && string.Equals(state.PlayerAId, _selfId, StringComparison.Ordinal);
        List<TradeSyncPatch.CardRef> remoteCards = state == null
            ? null
            : (localIsA ? state.OfferB : state.OfferA);

        if (remoteCards == null || remoteCards.Count == 0)
        {
            CommonButtonWidget empty = CloneListItem(remoteContainer, "RemoteEmpty", "(无)", false);
            empty.button.interactable = false;
        }
        else
        {
            for (int i = 0; i < remoteCards.Count && i < 3; i++)
            {
                TradeSyncPatch.CardRef remoteCard = remoteCards[i];
                if (remoteCard == null)
                {
                    continue;
                }

                string label = $"{remoteCard.CardName}{(remoteCard.IsUpgraded ? "+" : string.Empty)}";
                CommonButtonWidget row = CloneListItem(remoteContainer, $"RemoteSelected_{remoteCard.InstanceId}_{i}", label, false);
                row.button.interactable = false;
            }
        }

        foreach (var card in deck)
        {
            if (card == null || selected.Contains(card.InstanceId))
            {
                continue;
            }

            if (_cardCellTemplate != null)
            {
                var cell = Instantiate(_cardCellTemplate, candidateContainer, false);
                cell.gameObject.SetActive(true);
                cell.Card = card;
                cell.SetNum(1);

                var btn = cell.gameObject.GetComponent<Button>();
                if (btn == null) btn = cell.gameObject.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(new UnityAction(() =>
                {
                    if (_localCards.Count >= maxSelectableNow)
                    {
                        return;
                    }

                    AudioManager.Card(3); // 卡牌点击音效。
                    _localCards.Add(card);
                    RebuildCardPicker(); // 立即刷新，把刚选中的卡牌移出列表。
                }));
            }
            else
            {
                var row = CloneListItem(candidateContainer, $"Card_{card.InstanceId}", card.Name, false);
                row.button.onClick.AddListener(new UnityAction(() =>
                {
                    if (_localCards.Count >= maxSelectableNow)
                    {
                        return;
                    }

                    _localCards.Add(card);
                    RebuildCardPicker();
                }));
            }
        }

        if (deck.Count == 0)
        {
            Plugin.Logger?.LogWarning($"[TradeDetailDialog] RebuildCardPicker empty deck: tradeId={_tradeId ?? "<null>"}, hasRun={hasRun}, runSource={runSource ?? "<null>"}, initialDeckCount={_initialDeckCards.Count}");
            var empty = CloneText(candidateContainer as RectTransform, "Empty", 20, TextAlignmentOptions.Center);
            empty.text = "没有可交易的卡牌";
        }
    }

    private void RebuildExhibitPicker()
    {
        if (_exhibitPickerRoot == null)
        {
            return;
        }

        var tag = _exhibitPickerRoot.GetComponentInChildren<PickerListTag>(true);
        if (tag == null)
        {
            return;
        }

        var container = tag.transform;
        
        // 遗物列表也使用 Grid Layout，和原生展示方式更接近。
        var vlg = container.GetComponent<VerticalLayoutGroup>();
        if (vlg != null) DestroyImmediate(vlg);
        
        var glg = container.GetComponent<GridLayoutGroup>();
        if (glg == null) glg = container.gameObject.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(100, 100); // 遗物格子保持方形。
        glg.spacing = new Vector2(20, 20);
        glg.padding = new RectOffset(20, 20, 20, 20);
        glg.childAlignment = TextAnchor.UpperLeft;

        foreach (Transform c in container)
        {
            Destroy(c.gameObject);
        }

        List<Exhibit> tradable = CurrentGameRun?.Player?.Exhibits?
            .Where(TradeExhibitRules.IsTradable)
            .OrderBy(e => e.Name)
            .ToList()
            ?? new List<Exhibit>();

        if (tradable.Count == 0)
        {
            var empty = CloneText(container as RectTransform, "Empty", 20, TextAlignmentOptions.Center);
            empty.text = "没有可交易的遗物";
            return;
        }

        foreach (var ex in tradable)
        {
            if (ex == null || string.IsNullOrWhiteSpace(ex.Id))
            {
                continue;
            }

            if (_exhibitTemplate != null)
            {
                var widget = Instantiate(_exhibitTemplate, container, false);
                widget.gameObject.SetActive(true);
                widget.Exhibit = ex;
                widget.ShowCounter = false;

                var isOn = _localExhibitIds.Contains(ex.Id);
                var img = widget.MainImage;
                if (img != null) img.color = isOn ? new Color(0.4f, 1f, 0.4f, 1f) : Color.white;

                var btn = widget.gameObject.GetComponent<Button>();
                if (btn == null) btn = widget.gameObject.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(new UnityAction(() =>
                {
                    AudioManager.Button(2);
                    if (_localExhibitIds.Contains(ex.Id))
                    {
                        _localExhibitIds.Remove(ex.Id);
                    }
                    else
                    {
                        _localExhibitIds.Add(ex.Id);
                    }
                    RebuildExhibitPicker();
                }));
            }
            else
            {
                var isOn = _localExhibitIds.Contains(ex.Id);
                var row = CloneListItem(container, $"Ex_{ex.Id}", ex.Name, isOn);
                row.button.onClick.AddListener(new UnityAction(() =>
                {
                    if (_localExhibitIds.Contains(ex.Id))
                    {
                        _localExhibitIds.Remove(ex.Id);
                    }
                    else
                    {
                        _localExhibitIds.Add(ex.Id);
                    }

                    RefreshLocalUi();
                    TrySendOfferUpdate();
                    RebuildExhibitPicker();
                }));
            }
        }
    }

    private void TrySendOfferUpdate()
    {
        if (_isApplyingState)
        {
            return;
        }

        try
        {
            List<TradeSyncPatch.CardRef> refs = _localCards
                .Where(c => c != null)
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

            TradeSyncPatch.RequestOfferUpdate(_tradeId, _selfId, refs, _localMoney, _localExhibitIds.ToList());

            UpdateConfirmInteractable(TradeSyncPatch.GetLastKnown(_tradeId));
        }
        catch
        {
            // ignored
        }
    }

    private void RebuildCardList(Transform listRoot, List<Card> cards, bool isLocal)
    {
        if (listRoot == null) return;
        foreach (Transform c in listRoot) Destroy(c.gameObject);

        if (cards == null || cards.Count == 0)
        {
            var empty = CloneText(listRoot as RectTransform, "Empty", 18, TextAlignmentOptions.Center);
            empty.text = "(无)";
            return;
        }

        foreach (var card in cards)
        {
            if (card == null) continue;
            var label = card.Name + (card.IsUpgraded ? "+" : "");
            var row = CloneListItem(listRoot, $"Card_{card.InstanceId}", label, false);
            if (isLocal)
            {
                row.button.onClick.AddListener(() =>
                {
                    AudioManager.Button(0);
                    _localCards.Remove(card);
                    RefreshLocalUi();
                    TrySendOfferUpdate();
                });
            }
            else
            {
                row.button.interactable = false;
            }
        }
    }

    private void RebuildCardList(Transform listRoot, List<TradeSyncPatch.CardRef> cardRefs, bool isLocal)
    {
        if (listRoot == null) return;
        foreach (Transform c in listRoot) Destroy(c.gameObject);

        if (cardRefs == null || cardRefs.Count == 0)
        {
            var empty = CloneText(listRoot as RectTransform, "Empty", 18, TextAlignmentOptions.Center);
            empty.text = "(无)";
            return;
        }

        foreach (var cardRef in cardRefs)
        {
            if (cardRef == null) continue;
            var label = cardRef.CardName + (cardRef.IsUpgraded ? "+" : "");
            var row = CloneListItem(listRoot, $"RemoteCard_{cardRef.InstanceId}", label, false);
            row.button.interactable = false;
        }
    }

    private CommonButtonWidget CloneListItem(Transform parent, string name, string label, bool isActive = false)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, 44f); 

        var img = go.AddComponent<Image>();
        img.color = isActive ? new Color(0.12f, 0.36f, 0.12f, 0.7f) : new Color(0.1f, 0.1f, 0.1f, 0.5f);
        
        var text = CloneText(rt, "Label", 18, TextAlignmentOptions.Left);
        text.text = label;
        SetRect(text.rectTransform, 0.05f, 0f, 0.85f, 1f);

        var status = CloneText(rt, "Status", 16, TextAlignmentOptions.Right);
        status.text = isActive ? "●" : ""; // Symbolic indicator
        SetRect(status.rectTransform, 0.85f, 0f, 0.95f, 1f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var cbw = go.AddComponent<CommonButtonWidget>();
        cbw.button = btn;
        
        DisableTooltipBehaviours(go);
        return cbw;
    }

    private CommonButtonWidget CloneButton(Transform parent, string name, string label)
    {
        CommonButtonWidget w = null;
        try
        {
            if (_buttonTemplate != null)
            {
                // Ensure we clone the TEMPLATE, not the active instance if it's already modified.
                w = UnityEngine.Object.Instantiate(_buttonTemplate, parent, false);
                w.name = name;
            }
        }
        catch { w = null; }

        if (w == null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            w = go.AddComponent<CommonButtonWidget>();
            w.button = btn;
        }

        try
        {
            SetButtonText(w, label);
            Traverse traverse = HarmonyLib.Traverse.Create(w);
            if (label.Contains("取消") || label.Contains("Cancel") || label.Contains("Back"))
            {
                traverse.Field("buttonBehavior").SetValue(1); // Close behavior
            }
            else
            {
                traverse.Field("buttonBehavior").SetValue(0); // Normal/Open behavior
            }
        }
        catch { }

        DisableTooltipBehaviours(w.gameObject);
        return w;
    }

    private void SetButtonText(CommonButtonWidget button, string label)
    {
        if (button == null) return;
        var tmp = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp == null)
        {
            tmp = CloneText(button.transform as RectTransform, "Label", 18, TextAlignmentOptions.Center);
            SetRect(tmp.rectTransform, 0f, 0f, 1f, 1f);
        }
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    private TextMeshProUGUI CloneText(RectTransform parent, string name, int fontSize, TextAlignmentOptions align)
    {
        TextMeshProUGUI t = null;

        try
        {
            if (_textTemplate != null)
            {
                t = UnityEngine.Object.Instantiate(_textTemplate, parent, false);
                t.name = name;
            }
        }
        catch
        {
            t = null;
        }

        if (t == null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            t = go.AddComponent<TextMeshProUGUI>();
        }

        t.fontSize = Mathf.Max(t.fontSize, fontSize);
        t.alignment = align;
        t.raycastTarget = false;
        _ = t.rectTransform;
        return t;
    }

    private Transform CreateScrollList(RectTransform parent, string name, float minX, float minY, float maxX, float maxY)
    {
        // Prefer an in-game authored scroll view (HistoryPanel) so visuals match vanilla.
        if (TryCreateInGameScrollList(parent, name, minX, minY, maxX, maxY, out Transform content))
        {
            return content;
        }

        // Fallback: lightweight runtime scroll list.
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);

        var rootRt = root.AddComponent<RectTransform>();
        SetRect(rootRt, minX, minY, maxX, maxY);

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(root.transform, false);
        var vpRt = viewport.AddComponent<RectTransform>();
        SetRect(vpRt, 0f, 0f, 1f, 1f);

        var vpImg = viewport.AddComponent<Image>();
        vpImg.color = new Color(0f, 0f, 0f, 0.25f);

        var mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject contentGo = new GameObject("Content");
        contentGo.transform.SetParent(viewport.transform, false);
        var cRt = contentGo.AddComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0f, 1f);
        cRt.anchorMax = new Vector2(1f, 1f);
        cRt.pivot = new Vector2(0.5f, 1f);
        cRt.anchoredPosition = Vector2.zero;
        cRt.sizeDelta = new Vector2(0f, 0f);

        var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.spacing = 6f;
        vlg.padding = new RectOffset(8, 8, 8, 8);

        var fitter = contentGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scroll = root.AddComponent<ScrollRect>();
        scroll.viewport = vpRt;
        scroll.content = cRt;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        return contentGo.transform;
    }

    private bool TryCreateInGameScrollList(
        RectTransform parent,
        string name,
        float minX,
        float minY,
        float maxX,
        float maxY,
        out Transform content)
    {
        content = null;
        GameObject historyInstance = null;

        try
        {
            var historyPrefab = Resources.Load<GameObject>("UI/Panels/HistoryPanel");
            if (historyPrefab == null)
            {
                return false;
            }

            historyInstance = Instantiate(historyPrefab);
            historyInstance.SetActive(false);

            ScrollRect listScrollRect = null;
            try
            {
                var historyPanel = historyInstance.GetComponentInChildren<HistoryPanel>(true);
                if (historyPanel != null)
                {
                    listScrollRect = GetPrivateFieldValue<ScrollRect>(historyPanel, "listScrollRect");
                }
            }
            catch
            {
                listScrollRect = null;
            }

            if (listScrollRect == null)
            {
                // Heuristic: pick a ScrollRect whose content contains a RecordRow template.
                foreach (var sr in historyInstance.GetComponentsInChildren<ScrollRect>(true))
                {
                    if (sr == null || sr.content == null)
                    {
                        continue;
                    }

                    var rr = sr.content.GetComponentInChildren<RecordRow>(true);
                    if (rr == null)
                    {
                        continue;
                    }

                    listScrollRect = sr;
                    break;
                }
            }

            if (listScrollRect == null)
            {
                return false;
            }

            // Detach the ScrollRect (we only need its visuals: viewport/mask/scrollbars).
            var scrollGo = listScrollRect.gameObject;
            scrollGo.name = name;
            listScrollRect.transform.SetParent(parent, false);
            listScrollRect.gameObject.SetActive(true);

            var scrollRt = listScrollRect.GetComponent<RectTransform>();
            if (scrollRt != null)
            {
                SetRect(scrollRt, minX, minY, maxX, maxY);
            }

            // Replace content with a simple vertical layout that we own.
            var oldContent = listScrollRect.content;
            if (oldContent != null)
            {
                try
                {
                    oldContent.gameObject.SetActive(false);
                }
                catch
                {
                    // ignored
                }
            }

            if (listScrollRect.viewport == null)
            {
                // Try to recover viewport.
                try
                {
                    var mask = listScrollRect.GetComponentInChildren<Mask>(true);
                    if (mask != null)
                    {
                        listScrollRect.viewport = mask.GetComponent<RectTransform>();
                    }
                }
                catch
                {
                    // ignored
                }
            }

            var viewport = listScrollRect.viewport != null ? listScrollRect.viewport : scrollRt;
            if (viewport == null)
            {
                return false;
            }

            GameObject contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewport, false);

            var cRt = contentGo.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 1f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(0f, 0f);

            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 6f;
            vlg.padding = new RectOffset(8, 8, 8, 8);

            var fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            listScrollRect.content = cRt;
            listScrollRect.horizontal = false;
            listScrollRect.vertical = true;
            listScrollRect.movementType = ScrollRect.MovementType.Clamped;

            // Destroy the rest of the instantiated HistoryPanel (we already detached ScrollRect).
            try
            {
                if (historyInstance != null)
                {
                    Destroy(historyInstance);
                }
            }
            catch
            {
                // ignored
            }

            content = contentGo.transform;
            return true;
        }
        catch
        {
            try
            {
                if (historyInstance != null)
                {
                    Destroy(historyInstance);
                }
            }
            catch
            {
                // ignored
            }

            content = null;
            return false;
        }
    }

    private static T GetPrivateFieldValue<T>(object target, string fieldName) where T : class
    {
        try
        {
            if (target == null || string.IsNullOrWhiteSpace(fieldName))
            {
                return null;
            }

            var t = target.GetType();
            while (t != null)
            {
                var fi = t.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (fi != null)
                {
                    return fi.GetValue(target) as T;
                }

                t = t.BaseType;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private void SetRect(RectTransform rt, float minX, float minY, float maxX, float maxY)
    {
        rt.anchorMin = new Vector2(minX, minY);
        rt.anchorMax = new Vector2(maxX, maxY);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void DisableExtraButtons(CommonButtonWidget widget)
    {
        try
        {
            if (widget == null)
            {
                return;
            }

            Button keep = widget.button;
            var buttons = widget.GetComponentsInChildren<Button>(true);
            if (buttons == null || buttons.Length <= 1)
            {
                return;
            }

            foreach (var b in buttons)
            {
                if (b == null || b == keep)
                {
                    continue;
                }

                b.enabled = false;
                b.interactable = false;
            }
        }
        catch
        {
            // ignored
        }
    }

    private void DisableTooltipBehaviours(GameObject root)
    {
        try
        {
            if (root == null)
            {
                return;
            }

            foreach (var behaviour in root.GetComponentsInChildren<Behaviour>(true))
            {
                if (behaviour == null)
                {
                    continue;
                }

                var n = behaviour.GetType().Name;
                if (n != null && n.IndexOf("Tooltip", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    behaviour.enabled = false;
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private readonly struct ApplyingStateScope : IDisposable
    {
        private readonly TradeDetailDialog _d;
        private readonly bool _prev;

        public ApplyingStateScope(TradeDetailDialog d)
        {
            _d = d;
            _prev = d._isApplyingState;
            d._isApplyingState = true;
        }

        public void Dispose()
        {
            if (_d != null) _d._isApplyingState = _prev;
        }
    }
}
