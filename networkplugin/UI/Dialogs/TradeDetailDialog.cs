using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Core;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Dialogs;

// Runtime-created dialog; we avoid relying on prefabs so this works in mod context.
public sealed class TradeDetailDialog : UiDialog<TradeDetailPayload>, IInputActionHandler
{
    private const int MaxMoneyOffer = 99999;

    private static GameRunController CurrentGameRun
    {
        get
        {
            try
            {
                return Singleton<GameMaster>.Instance?.CurrentGameRun;
            }
            catch
            {
                return null;
            }
        }
    }

    private CanvasGroup _canvasGroup;

    private RectTransform _panelRoot;
    private CommonButtonWidget _buttonTemplate;
    private TextMeshProUGUI _textTemplate;

    // Header
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _statusText;

    // Local offer controls
    private TextMeshProUGUI _localCardsTitle;
    private Transform _localCardsList;
    private TextMeshProUGUI _localMoneyText;
    private TextMeshProUGUI _localExhibitsText;

    private CommonButtonWidget _addCardButton;
    private CommonButtonWidget _editExhibitButton;
    private CommonButtonWidget _moneyMinusButton;
    private CommonButtonWidget _moneyPlusButton;

    // Remote offer display
    private TextMeshProUGUI _remoteCardsTitle;
    private Transform _remoteCardsList;
    private TextMeshProUGUI _remoteMoneyText;
    private TextMeshProUGUI _remoteExhibitsText;

    // Footer
    private CommonButtonWidget _confirmButton;
    private CommonButtonWidget _cancelButton;

    // Payload/session
    private TradeDetailPayload _payload;
    private string _tradeId;
    private string _selfId;
    private string _partnerId;
    private bool _subscribed;
    private bool _isApplyingState;
    private int _maxSlots = 5;
    private bool _completionApplied;
    private long _lastPreparingHandledTimestamp;
    private TradeSyncPatch.TradeStatus? _lastStatus;

    // Local offer state
    private readonly List<Card> _localCards = new List<Card>();
    private int _localMoney;
    private readonly HashSet<string> _localExhibitIds = new HashSet<string>(StringComparer.Ordinal);

    // Pickers
    private GameObject _cardPickerRoot;
    private GameObject _exhibitPickerRoot;

    internal void BindRuntime(CommonButtonWidget buttonTemplate, TextMeshProUGUI textTemplate, RectTransform panelRoot)
    {
        _buttonTemplate = buttonTemplate;
        _textTemplate = textTemplate;
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

        if (string.IsNullOrWhiteSpace(_selfId) || string.IsNullOrWhiteSpace(_partnerId))
        {
            TryShowTopMessage("交易对象无效。");
            Hide(false);
            return;
        }

        ResetLocalOffer();
        EnsureSubscribed();

        // Start session and request a snapshot so we can render both sides.
        TradeSyncPatch.RequestStartTrade(_tradeId, _selfId, _partnerId, _maxSlots);
        TradeSyncPatch.RequestSnapshot(_tradeId, _selfId);

        _titleText.text = string.IsNullOrWhiteSpace(payload?.PartnerPlayerName)
            ? "交易"
            : $"交易 - {payload.PartnerPlayerName}";

        _statusText.text = "请选择交易内容";

        RefreshLocalUi();
        RefreshRemoteUi(TradeSyncPatch.GetLastKnown(_tradeId));

        _canvasGroup.interactable = true;
        UiManager.PushActionHandler(this);
    }

    protected override void OnHiding()
    {
        _canvasGroup.interactable = false;
        UiManager.PopActionHandler(this);
        TryUnsubscribe();

        // Hide pickers if open.
        try { if (_cardPickerRoot != null) _cardPickerRoot.SetActive(false); } catch { }
        try { if (_exhibitPickerRoot != null) _exhibitPickerRoot.SetActive(false); } catch { }
    }

    public void OnCancel()
    {
        // Back/escape closes the dialog and requests cancel.
        if (_completionApplied)
        {
            Hide();
            return;
        }

        try
        {
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
        try
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
        catch
        {
            return false;
        }
    }

    private void TryShowTopMessage(string message)
    {
        try
        {
            if (!UiManager.IsInitialized)
            {
                return;
            }

            UiManager.GetPanel<TopMessagePanel>().ShowMessage(message);
        }
        catch
        {
            // ignored
        }
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
                SyncLocalFromState(state);
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
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1.6));
            }
            catch
            {
                // ignored
            }

            try { Hide(); } catch { }
            return;
        }

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1.2));
        }
        catch
        {
            // ignored
        }

        try { Hide(); } catch { }
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
        try
        {
            if (cardRef == null || cardRef.InstanceId < 0)
            {
                return null;
            }

            return CurrentGameRun?.GetDeckCardByInstanceId(cardRef.InstanceId);
        }
        catch
        {
            return null;
        }
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

        int currentMoney;
        try
        {
            currentMoney = CurrentGameRun?.Money ?? 0;
        }
        catch
        {
            TradeSyncPatch.RequestPrepareResult(_tradeId, _selfId, false, "MoneyCheckFailed");
            return;
        }

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

                Exhibit owned = null;
                try
                {
                    owned = run?.Player?.Exhibits?.FirstOrDefault(e => e != null && string.Equals(e.Id, id, StringComparison.Ordinal));
                }
                catch
                {
                    owned = null;
                }

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
        try
        {
            _localMoneyText.text = $"金币: {_localMoney}";
            _localExhibitsText.text = $"遗物: {_localExhibitIds.Count}";

            RebuildCardList(_localCardsList, _localCards, isLocal: true);
        }
        catch
        {
            // ignored
        }
    }

    private void RefreshRemoteUi(TradeSyncPatch.TradeSessionState state)
    {
        try
        {
            if (state == null)
            {
                _remoteMoneyText.text = "金币: 0";
                _remoteExhibitsText.text = "遗物: 0";
                RebuildCardList(_remoteCardsList, new List<string>(), isLocal: false);
                return;
            }

            bool localIsA = string.Equals(state.PlayerAId, _selfId, StringComparison.Ordinal);

            int theirMoney = localIsA ? state.MoneyB : state.MoneyA;
            int theirEx = localIsA ? (state.ExhibitsB?.Count ?? 0) : (state.ExhibitsA?.Count ?? 0);

            _remoteMoneyText.text = $"金币: {Mathf.Max(0, theirMoney)}";
            _remoteExhibitsText.text = $"遗物: {Mathf.Max(0, theirEx)}";

            var theirCards = localIsA ? state.OfferB : state.OfferA;
            var names = new List<string>();
            if (theirCards != null)
            {
                foreach (var c in theirCards)
                {
                    if (c == null)
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(c.CardName))
                    {
                        names.Add(c.CardName);
                    }
                    else if (!string.IsNullOrWhiteSpace(c.CardId))
                    {
                        names.Add(c.CardId);
                    }
                }
            }

            RebuildCardList(_remoteCardsList, names, isLocal: false);
        }
        catch
        {
            // ignored
        }
    }

    private void EnsureUi()
    {
        if (_panelRoot == null)
        {
            // Fallback for scene-created dialog.
            var panel = transform.Find("Panel") as RectTransform;
            _panelRoot = panel != null ? panel : GetComponent<RectTransform>();
        }

        // Use best-effort templates.
        if (_textTemplate == null)
        {
            try { _textTemplate = GetComponentInChildren<TextMeshProUGUI>(true); } catch { _textTemplate = null; }
        }

        // Header
        _titleText = CloneText(_panelRoot, "Title", 30, TextAlignmentOptions.Center);
        SetRect(_titleText.rectTransform, 0.06f, 0.90f, 0.94f, 0.98f);

        _statusText = CloneText(_panelRoot, "Status", 18, TextAlignmentOptions.Center);
        SetRect(_statusText.rectTransform, 0.06f, 0.85f, 0.94f, 0.90f);

        // Local side
        _localCardsTitle = CloneText(_panelRoot, "LocalTitle", 20, TextAlignmentOptions.Left);
        _localCardsTitle.text = "我的报价";
        SetRect(_localCardsTitle.rectTransform, 0.06f, 0.80f, 0.46f, 0.85f);

        _remoteCardsTitle = CloneText(_panelRoot, "RemoteTitle", 20, TextAlignmentOptions.Left);
        _remoteCardsTitle.text = "对方报价";
        SetRect(_remoteCardsTitle.rectTransform, 0.54f, 0.80f, 0.94f, 0.85f);

        _localCardsList = CreateScrollList(_panelRoot, "LocalCards", 0.06f, 0.36f, 0.46f, 0.78f);
        _remoteCardsList = CreateScrollList(_panelRoot, "RemoteCards", 0.54f, 0.36f, 0.94f, 0.78f);

        _localMoneyText = CloneText(_panelRoot, "LocalMoney", 18, TextAlignmentOptions.Left);
        SetRect(_localMoneyText.rectTransform, 0.06f, 0.30f, 0.30f, 0.35f);

        _localExhibitsText = CloneText(_panelRoot, "LocalEx", 18, TextAlignmentOptions.Left);
        SetRect(_localExhibitsText.rectTransform, 0.30f, 0.30f, 0.46f, 0.35f);

        _remoteMoneyText = CloneText(_panelRoot, "RemoteMoney", 18, TextAlignmentOptions.Left);
        SetRect(_remoteMoneyText.rectTransform, 0.54f, 0.30f, 0.78f, 0.35f);

        _remoteExhibitsText = CloneText(_panelRoot, "RemoteEx", 18, TextAlignmentOptions.Left);
        SetRect(_remoteExhibitsText.rectTransform, 0.78f, 0.30f, 0.94f, 0.35f);

        // Controls
        _addCardButton = CloneButton(_panelRoot, "AddCard", "添加卡牌");
        SetRect(_addCardButton.GetComponent<RectTransform>(), 0.06f, 0.24f, 0.22f, 0.29f);
        _addCardButton.button.onClick.RemoveAllListeners();
        _addCardButton.button.onClick.AddListener(new UnityAction(ShowCardPicker));

        _editExhibitButton = CloneButton(_panelRoot, "EditEx", "选择遗物");
        SetRect(_editExhibitButton.GetComponent<RectTransform>(), 0.24f, 0.24f, 0.40f, 0.29f);
        _editExhibitButton.button.onClick.RemoveAllListeners();
        _editExhibitButton.button.onClick.AddListener(new UnityAction(ShowExhibitPicker));

        _moneyMinusButton = CloneButton(_panelRoot, "MoneyMinus", "-1");
        SetRect(_moneyMinusButton.GetComponent<RectTransform>(), 0.42f, 0.24f, 0.46f, 0.29f);
        _moneyMinusButton.button.onClick.RemoveAllListeners();
        _moneyMinusButton.button.onClick.AddListener(new UnityAction(() => ChangeMoney(-1)));

        _moneyPlusButton = CloneButton(_panelRoot, "MoneyPlus", "+1");
        SetRect(_moneyPlusButton.GetComponent<RectTransform>(), 0.47f, 0.24f, 0.51f, 0.29f);
        _moneyPlusButton.button.onClick.RemoveAllListeners();
        _moneyPlusButton.button.onClick.AddListener(new UnityAction(() => ChangeMoney(+1)));

        // Footer
        _confirmButton = CloneButton(_panelRoot, "Confirm", "确认");
        SetRect(_confirmButton.GetComponent<RectTransform>(), 0.28f, 0.08f, 0.46f, 0.16f);
        _confirmButton.button.onClick.RemoveAllListeners();
        _confirmButton.button.onClick.AddListener(new UnityAction(OnConfirm));

        _cancelButton = CloneButton(_panelRoot, "Cancel", "取消");
        SetRect(_cancelButton.GetComponent<RectTransform>(), 0.54f, 0.08f, 0.72f, 0.16f);
        _cancelButton.button.onClick.RemoveAllListeners();
        _cancelButton.button.onClick.AddListener(new UnityAction(OnCancelClick));

        DisableTooltipBehaviours(gameObject);
    }

    private void OnConfirm()
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

            TradeSyncPatch.RequestConfirm(_tradeId, _selfId);
            _statusText.text = "已确认，等待对方...";
        }
        catch
        {
            // ignored
        }
    }

    private void OnCancelClick()
    {
        if (_completionApplied)
        {
            Hide();
            return;
        }

        try
        {
            TradeSyncPatch.RequestCancel(_tradeId, _selfId);
        }
        catch
        {
            // ignored
        }

        Hide();
    }

    private void ChangeMoney(int delta)
    {
        if (_isApplyingState)
        {
            return;
        }

        int current = 0;
        try { current = CurrentGameRun?.Money ?? 0; } catch { current = 0; }

        _localMoney = Mathf.Clamp(_localMoney + delta, 0, Mathf.Min(MaxMoneyOffer, current));
        RefreshLocalUi();
        TrySendOfferUpdate();
    }

    private void ShowCardPicker()
    {
        if (_isApplyingState)
        {
            return;
        }

        EnsureCardPicker();
        RebuildCardPicker();
        _cardPickerRoot.SetActive(true);
        _canvasGroup.interactable = false;
    }

    private void ShowExhibitPicker()
    {
        if (_isApplyingState)
        {
            return;
        }

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

        _cardPickerRoot = CreateFullOverlay("CardPicker", "选择要交易的卡牌");
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
        var root = new GameObject(name);
        root.transform.SetParent(transform, false);
        root.SetActive(false);

        var rt = root.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var bg = root.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.65f);
        bg.raycastTarget = true;

        var title = CloneText(rt, "Title", 26, TextAlignmentOptions.Center);
        title.text = titleText;
        SetRect(title.rectTransform, 0.10f, 0.88f, 0.90f, 0.97f);

        var list = CreateScrollList(rt, "List", 0.10f, 0.18f, 0.90f, 0.86f);
        var tag = list.gameObject.AddComponent<PickerListTag>();

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

        return root;
    }

    private sealed class PickerListTag : MonoBehaviour
    {
    }

    private void RebuildCardPicker()
    {
        if (_cardPickerRoot == null)
        {
            return;
        }

        var tag = _cardPickerRoot.GetComponentInChildren<PickerListTag>(true);
        if (tag == null)
        {
            return;
        }

        var container = tag.transform;
        foreach (Transform c in container)
        {
            Destroy(c.gameObject);
        }

        var deck = new List<Card>();
        try
        {
            if (CurrentGameRun?.BaseDeck != null)
            {
                deck = CurrentGameRun.BaseDeck.Where(c => c != null).OrderBy(c => c.Name).ToList();
            }
        }
        catch
        {
            deck = new List<Card>();
        }

        var selected = new HashSet<int>(_localCards.Where(c => c != null).Select(c => c.InstanceId));

        foreach (var card in deck)
        {
            if (card == null || selected.Contains(card.InstanceId))
            {
                continue;
            }

            var row = CloneButton(container, $"Card_{card.InstanceId}", card.Name);
            row.button.onClick.RemoveAllListeners();
            row.button.onClick.AddListener(new UnityAction(() =>
            {
                if (_localCards.Count >= _maxSlots)
                {
                    return;
                }

                _localCards.Add(card);
                RefreshLocalUi();
                TrySendOfferUpdate();
            }));
        }

        if (deck.Count == 0)
        {
            var empty = CloneText(container as RectTransform, "Empty", 20, TextAlignmentOptions.Center);
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
        foreach (Transform c in container)
        {
            Destroy(c.gameObject);
        }

        List<Exhibit> tradable = new List<Exhibit>();
        try
        {
            if (CurrentGameRun?.Player?.Exhibits != null)
            {
                tradable = CurrentGameRun.Player.Exhibits
                    .Where(TradeExhibitRules.IsTradable)
                    .OrderBy(e => e.Name)
                    .ToList();
            }
        }
        catch
        {
            tradable = new List<Exhibit>();
        }

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

            var row = CloneButton(container, $"Ex_{ex.Id}", ex.Name);
            var isOn = _localExhibitIds.Contains(ex.Id);
            TintRow(row.gameObject, isOn);

            row.button.onClick.RemoveAllListeners();
            row.button.onClick.AddListener(new UnityAction(() =>
            {
                if (_localExhibitIds.Contains(ex.Id))
                {
                    _localExhibitIds.Remove(ex.Id);
                    TintRow(row.gameObject, false);
                }
                else
                {
                    _localExhibitIds.Add(ex.Id);
                    TintRow(row.gameObject, true);
                }

                RefreshLocalUi();
                TrySendOfferUpdate();
            }));
        }
    }

    private void TintRow(GameObject row, bool selected)
    {
        try
        {
            var img = row != null ? row.GetComponent<Image>() : null;
            if (img != null)
            {
                img.color = selected ? new Color(0.4f, 0.8f, 0.4f, 0.18f) : new Color(1f, 1f, 1f, 0.10f);
            }
        }
        catch
        {
            // ignored
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
            var refs = _localCards
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
        if (listRoot == null)
        {
            return;
        }

        foreach (Transform c in listRoot)
        {
            Destroy(c.gameObject);
        }

        if (cards == null || cards.Count == 0)
        {
            var empty = CloneText(listRoot as RectTransform, "Empty", 18, TextAlignmentOptions.Center);
            empty.text = "(无)";
            return;
        }

        foreach (var card in cards)
        {
            if (card == null)
            {
                continue;
            }

            var row = CloneButton(listRoot, $"Card_{card.InstanceId}", card.Name);
            if (isLocal)
            {
                row.button.onClick.RemoveAllListeners();
                row.button.onClick.AddListener(new UnityAction(() =>
                {
                    _localCards.Remove(card);
                    RefreshLocalUi();
                    TrySendOfferUpdate();
                }));
            }
            else
            {
                row.button.interactable = false;
            }
        }
    }

    private void RebuildCardList(Transform listRoot, List<string> names, bool isLocal)
    {
        if (listRoot == null)
        {
            return;
        }

        foreach (Transform c in listRoot)
        {
            Destroy(c.gameObject);
        }

        if (names == null || names.Count == 0)
        {
            var empty = CloneText(listRoot as RectTransform, "Empty", 18, TextAlignmentOptions.Center);
            empty.text = "(无)";
            return;
        }

        int i = 0;
        foreach (var n in names)
        {
            var row = CloneButton(listRoot, $"Remote_{i++}", n);
            row.button.interactable = false;
        }
    }

    private CommonButtonWidget CloneButton(Transform parent, string name, string label)
    {
        CommonButtonWidget w = null;

        try
        {
            if (_buttonTemplate != null)
            {
                w = UnityEngine.Object.Instantiate(_buttonTemplate, parent, false);
                w.name = name;
            }
        }
        catch
        {
            w = null;
        }

        if (w == null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.10f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            w = go.AddComponent<CommonButtonWidget>();
            w.button = btn;
        }

        try
        {
            var tmp = w.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp == null)
            {
                tmp = CloneText(w.transform as RectTransform, "Label", 18, TextAlignmentOptions.Center);
                SetRect(tmp.rectTransform, 0f, 0f, 1f, 1f);
            }

            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
        }
        catch
        {
            // ignored
        }

        DisableExtraButtons(w);
        DisableTooltipBehaviours(w.gameObject);

        return w;
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
            var go = new GameObject(name);
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
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);

        var rootRt = root.AddComponent<RectTransform>();
        SetRect(rootRt, minX, minY, maxX, maxY);

        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(root.transform, false);
        var vpRt = viewport.AddComponent<RectTransform>();
        SetRect(vpRt, 0f, 0f, 1f, 1f);

        var vpImg = viewport.AddComponent<Image>();
        vpImg.color = new Color(0f, 0f, 0f, 0.10f);

        var mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var cRt = content.AddComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0f, 1f);
        cRt.anchorMax = new Vector2(1f, 1f);
        cRt.pivot = new Vector2(0.5f, 1f);
        cRt.anchoredPosition = Vector2.zero;
        cRt.sizeDelta = new Vector2(0f, 0f);

        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.spacing = 6f;
        vlg.padding = new RectOffset(8, 8, 8, 8);

        var fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scroll = root.AddComponent<ScrollRect>();
        scroll.viewport = vpRt;
        scroll.content = cRt;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        return content.transform;
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
            if (_d != null)
            {
                _d._isApplyingState = _prev;
            }
        }
    }
}
