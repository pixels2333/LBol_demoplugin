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
using NetworkPlugin.Configuration;
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

    private static bool IsLocalDebugTradeAllowed()
    {
        var cfg = ModService.ServiceProvider?.GetService<ConfigManager>();
        return cfg?.DebugVirtualPlayerAiDefault?.Value == true || cfg?.DebugFakePlayersForTrade?.Value == true;
    }

    private bool TryEnsureNetworkConnected()
    {
        var client = ModService.ServiceProvider.GetService<INetworkClient>();
        if (client == null || !client.IsConnected)
        {
            if (IsLocalDebugTradeAllowed())
            {
                return true;
            }
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


}
