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
using NetworkPlugin.Network.Services;
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
public sealed partial class TradeDetailDialog : UiDialog<TradeDetailPayload>, IInputActionHandler
{
    #region 常量与字段
    private const int MaxMoneyOffer = 99999;

    private static GameRunController CurrentGameRun
        => GameStateUtils.GetCurrentGameRun();

    private bool IsPlayerA(TradeSyncPatch.TradeSessionState state)
        => state != null && string.Equals(state.PlayerAId, _selfId, StringComparison.Ordinal);

    private static List<TradeSyncPatch.CardRef> GetLocalOffer(TradeSyncPatch.TradeSessionState state, bool localIsA)
        => localIsA ? state.OfferA : state.OfferB;

    private static int GetLocalMoney(TradeSyncPatch.TradeSessionState state, bool localIsA)
        => localIsA ? state.MoneyA : state.MoneyB;

    private static List<TradeSyncPatch.ExhibitRef> GetLocalExhibits(TradeSyncPatch.TradeSessionState state, bool localIsA)
        => localIsA ? state.ExhibitsA : state.ExhibitsB;

    private static List<TradeSyncPatch.CardRef> GetRemoteOffer(TradeSyncPatch.TradeSessionState state, bool localIsA)
        => localIsA ? state.OfferB : state.OfferA;

    private static int GetRemoteMoney(TradeSyncPatch.TradeSessionState state, bool localIsA)
        => localIsA ? state.MoneyB : state.MoneyA;

    private static List<TradeSyncPatch.ExhibitRef> GetRemoteExhibits(TradeSyncPatch.TradeSessionState state, bool localIsA)
        => localIsA ? state.ExhibitsB : state.ExhibitsA;

    private CanvasGroup _canvasGroup;

    private RectTransform _panelRoot;
    private CommonButtonWidget _buttonTemplate;
    private TextMeshProUGUI _textTemplate;
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

    #endregion

    #region 核心生命周期

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

        _localCards.Clear();
        _localMoney = 0;
        _localExhibitIds.Clear();
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
            if (UiManager.IsInitialized)
            {
                UiManager.PopActionHandler(this);
            }
            _actionHandlerPushed = false;
        }

        // 如果 dialog 不是在明确取消/完成的情况下关闭，补发一次取消请求，避免服务端会话悬空。
        if (!_completionApplied
            && !_cancelRequested
            && !string.IsNullOrWhiteSpace(_tradeId)
            && !string.IsNullOrWhiteSpace(_selfId)
            && _lastStatus != TradeSyncPatch.TradeStatus.Canceled
            && _lastStatus != TradeSyncPatch.TradeStatus.Completed)
        {
            _cancelRequested = true;
            TradeSyncPatch.RequestCancel(_tradeId, _selfId);
        }

        if (_subscribed)
        {
            TradeSyncPatch.OnTradeStateUpdated -= OnTradeStateUpdated;
            _subscribed = false;
        }

        // 如果选择器还开着，一并关掉。
        CloseCardPickerOverlay(applyChanges: false, closeOnly: true);
        _exhibitPickerRoot?.SetActive(false);

        // 延后一帧执行，避免和 dialog 的 OnHiding 产生输入栈/布局冲突。
        long scheduleNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (!(_lastReturnToTradePanelTimestamp > 0 && scheduleNow - _lastReturnToTradePanelTimestamp < 450))
        {
            _lastReturnToTradePanelTimestamp = scheduleNow;

            // 仅在仍处于联机状态时回到 TradePanel，避免不断弹“交易不可用”。
            var client = ModService.ServiceProvider.GetService<INetworkClient>();
            if (client != null && client.IsConnected)
            {
                UniTask.Void(async () =>
                {
                    try
                    {
                        await UniTask.NextFrame();

                        if (!UiManager.IsInitialized)
                        {
                            return;
                        }

                        Transform contextParent = null;
                        var shop = UiManager.GetPanel<ShopPanel>();
                        if (shop != null && shop.IsVisible && shop.transform != null)
                        {
                            contextParent = shop.transform.parent != null ? shop.transform.parent : shop.transform;
                        }
                        else
                        {
                            var gap = UiManager.GetPanel<GapOptionsPanel>();
                            if (gap != null && gap.IsVisible && gap.transform != null)
                            {
                                contextParent = gap.transform.parent != null ? gap.transform.parent : gap.transform;
                            }
                        }

                        if (contextParent == null)
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
        }
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

        _cancelRequested = true;
        TradeSyncPatch.RequestCancel(_tradeId, _selfId);

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
                    bool localIsA = IsPlayerA(state);

                    _localMoney = Mathf.Max(0, GetLocalMoney(state, localIsA));

                    _localExhibitIds.Clear();
                    foreach (var ex in GetLocalExhibits(state, localIsA))
                    {
                        if (ex != null && !string.IsNullOrWhiteSpace(ex.ExhibitId))
                        {
                            _localExhibitIds.Add(ex.ExhibitId);
                        }
                    }

                    _localCards.Clear();
                    var localOffer = GetLocalOffer(state, localIsA);
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

        bool localIsA = IsPlayerA(state);

        List<TradeSyncPatch.CardRef> mine = GetLocalOffer(state, localIsA);
        List<TradeSyncPatch.CardRef> theirs = GetRemoteOffer(state, localIsA);
        int myMoney = GetLocalMoney(state, localIsA);
        int theirMoney = GetRemoteMoney(state, localIsA);
        List<TradeSyncPatch.ExhibitRef> myExhibits = GetLocalExhibits(state, localIsA);
        List<TradeSyncPatch.ExhibitRef> theirExhibits = GetRemoteExhibits(state, localIsA);

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

        bool localIsA2 = IsPlayerA(state);
        bool hasLocal = (GetLocalOffer(state, localIsA2)?.Count ?? 0) > 0
            || GetLocalMoney(state, localIsA2) > 0
            || (GetLocalExhibits(state, localIsA2)?.Count ?? 0) > 0;
        bool localIsA3 = IsPlayerA(state);
        bool hasRemote = (GetRemoteOffer(state, localIsA3)?.Count ?? 0) > 0
            || GetRemoteMoney(state, localIsA3) > 0
            || (GetRemoteExhibits(state, localIsA3)?.Count ?? 0) > 0;
        _confirmButton.button.interactable = state.Status == TradeSyncPatch.TradeStatus.Open
            && hasLocal && hasRemote;
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

        bool localIsA = IsPlayerA(state);

        List<TradeSyncPatch.CardRef> mine = GetLocalOffer(state, localIsA);
        int myMoney = GetLocalMoney(state, localIsA);
        List<TradeSyncPatch.ExhibitRef> myExhibits = GetLocalExhibits(state, localIsA);

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

    #endregion
    #region UI 渲染
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
            RebuildCardList(_remoteCardsList, (List<TradeSyncPatch.CardRef>)null);
            RebuildExhibitList(_remoteExhibitsList, (List<string>)null, isLocal: false);
            return;
        }

        bool localIsA = IsPlayerA(state);

        int theirMoney = GetRemoteMoney(state, localIsA);
        List<TradeSyncPatch.ExhibitRef> theirEx = GetRemoteExhibits(state, localIsA);

        _remoteMoneyText.text = $"金币: {Mathf.Max(0, theirMoney)}";
        _remoteExhibitsText.text = $"遗物: {theirEx?.Count ?? 0}";

        var theirCards = GetRemoteOffer(state, localIsA);
        RebuildCardList(_remoteCardsList, theirCards);
        RebuildExhibitList(_remoteExhibitsList, theirEx);
    }

    private void RebuildExhibitList(Transform listRoot, List<string> exhibitIds, bool isLocal)
    {
        if (!BeginRebuildList(listRoot, exhibitIds, out _)) return;

        foreach (var exId in exhibitIds)
        {
            if (string.IsNullOrEmpty(exId)) continue;

            var row = CloneListItem(listRoot, $"Ex_{exId}", exId, false);
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

    private void RebuildExhibitList(Transform listRoot, List<TradeSyncPatch.ExhibitRef> exhibits)
    {
        if (!BeginRebuildList(listRoot, exhibits, out _)) return;

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
        _cancelButton.button.onClick.AddListener(new UnityAction(() => { AudioManager.Button(0); OnCancel(); }));

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
        if (_cardPickerRoot == null)
        {
            _cardPickerRoot = CreateCardSelectionOverlay();
            Plugin.Logger?.LogInfo($"[TradeDetailDialog] EnsureCardPicker created: tradeId={_tradeId ?? "<null>"}, pickerExists={(_cardPickerRoot != null)}");
        }
        if (_cardPickerRoot == null)
        {
            TryShowTopMessage("卡牌选择界面不可用。");
            return;
        }
        _cardPickerOriginalCards.Clear();
        _cardPickerOriginalCards.AddRange(_localCards.Where(c => c != null));
        _cardPickerEditing = true;
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
        if (_exhibitPickerRoot == null)
        {
            _exhibitPickerRoot = CreateFullOverlay("ExhibitPicker", "选择要交易的遗物");
        }
        RebuildExhibitPicker();
        _exhibitPickerRoot.SetActive(true);
        _canvasGroup.interactable = false;
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

    #endregion
    #region UI 辅助方法
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

    #endregion
    #region 卡牌/遗物选择器
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
        bool localIsA = IsPlayerA(state);
        List<TradeSyncPatch.CardRef> remoteCards = state != null ? GetRemoteOffer(state, localIsA) : null;

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

    #endregion

}
