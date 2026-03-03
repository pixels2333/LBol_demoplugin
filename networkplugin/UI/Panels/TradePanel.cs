using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation;
using LBoL.Presentation.InputSystemExtend;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Core;
using NetworkPlugin.Network;
using NetworkPlugin.UI.Widgets;
using NetworkPlugin.UI.Dialogs;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;
using NetworkPlugin.Patch.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Panels;

/// <summary>
/// 交易面板类，处理玩家之间的物品（卡牌）交易界面与逻辑。
/// </summary>
public class TradePanel : UiPanel<TradePayload>, IInputActionHandler
{
    #region 常量

    /// <summary>
    /// 默认最大交易卡牌槽位数量。
    /// </summary>
    private const int DefaultMaxTradeSlots = 5;

    /// <summary>
    /// 交易完成后等待多少秒再关闭界面。
    /// </summary>
    private const float TradeCompleteWaitTime = 2f;

    private const int MaxMoneyOffer = 99999;

    #endregion

    #region 序列化字段（UI 组件）

    /// <summary>
    /// 玩家 1 的交易区域根节点。
    /// </summary>
    [SerializeField]
    private RectTransform player1TradeArea;

    /// <summary>
    /// 玩家 2 的交易区域根节点。
    /// </summary>
    [SerializeField]
    private RectTransform player2TradeArea;

    /// <summary>
    /// 玩家 1 的交易卡槽数组。
    /// </summary>
    [SerializeField]
    private TradeSlotWidget[] player1Slots;

    /// <summary>
    /// 玩家 2 的交易卡槽数组。
    /// </summary>
    [SerializeField]
    private TradeSlotWidget[] player2Slots;

    /// <summary>
    /// 确认交易按钮。
    /// </summary>
    [SerializeField]
    private CommonButtonWidget confirmButton;

    /// <summary>
    /// 取消交易按钮。
    /// </summary>
    [SerializeField]
    private CommonButtonWidget cancelButton;

    /// <summary>
    /// 状态提示文本（等待放入卡牌、可确认、交易完成等）。
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI statusText;

    /// <summary>
    /// 玩家 1 名称文本。
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI player1NameText;

    /// <summary>
    /// 玩家 2 名称文本。
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI player2NameText;

    #endregion

    #region 内部状态字段

    /// <summary>
    /// 玩家 1 已放入交易的卡牌列表。
    /// </summary>
    private readonly List<Card> _player1OfferedCards = new List<Card>();

    /// <summary>
    /// 玩家 2 已放入交易的卡牌列表。
    /// </summary>
    private readonly List<Card> _player2OfferedCards = new List<Card>();

    /// <summary>
    /// 当前交易会话的 ID。
    /// </summary>
    private string _tradeId;
    private string _selfPlayerId;
    private string _playerAId;
    private string _playerBId;
    private bool _isApplyingState;
    private bool _subscribedToTrade;

    // When true, this TradePanel has handed off to TradeDetailDialog and should not issue
    // any network requests or initialize in-panel editors.
    private bool _handoffToDetailDialog;

    /// <summary>
    /// 当前交易的参数载荷。
    /// </summary>
    private TradePayload _payload;

    /// <summary>
    /// 本次交易允许的最大卡牌数量。
    /// </summary>
    private int _maxTradeSlots = DefaultMaxTradeSlots;

    /// <summary>
    /// 用于整体控制交互和可见性的 CanvasGroup。
    /// </summary>
    private CanvasGroup _canvasGroup;

    /// <summary>
    /// 当前交易是否允许取消。
    /// </summary>
    private bool _canCancel = true;

    // v2 local offers (money + exhibits). Cards are stored in _player1OfferedCards.
    private int _localMoneyOffer;
    private readonly HashSet<string> _localExhibitOfferIds = new HashSet<string>(StringComparer.Ordinal);

    // Last known state for status transitions (Preparing validation).
    private TradeSyncPatch.TradeStatus? _lastTradeStatus;
    private long _lastPreparingHandledTimestamp;

    // Runtime partner picker overlay (kept minimal to avoid prefab dependencies).
    private GameObject _partnerPickerRoot;
    private bool _partnerPickerActive;
    private string _partnerPickerBuildError;
    private Button _partnerPickerCancelButton;
    private Button _partnerPickerRefreshButton;

    // Auto-refresh partner list once after opening (helps when location metadata arrives slightly later).
    private Coroutine _partnerPickerAutoRefreshCo;

    // Tracks whether this panel has pushed itself onto UiManager's action handler stack.
    private bool _actionHandlerPushed;

    // When true, trade details are suppressed because we are showing a modal message dialog.
    private bool _blockingCenterMessageActive;

    // Offline/local debug mode: allow opening and interacting with the TradePanel without a server.
    // This is used to validate UI flows (partner picker, card/exhibit selection, offer editor) when
    // DebugVirtualPlayerAiDefault/DebugFakePlayersForTrade is enabled.
    private bool _localDebugTradeMode;

    // Runtime offer editor overlay (money + exhibits).
    private GameObject _offerEditorRoot;
    private GameObject _offerActionsRoot;
    private Button _offerActionsPickCardsBtn;
    private Button _offerActionsPickExhibitsBtn;
    private GameObject _moneyTripletRoot;
    private HorizontalLayoutGroup _moneyTripletLayout;
    private float _moneyValueBaseFontSize;
    private TextMeshProUGUI _ownedMoneyText;
    private TextMeshProUGUI _moneyValueText;
    private TextMeshProUGUI _exhibitValueText;

    // Runtime card picker overlay (deck cards).
    private GameObject _cardPickerRoot;
    private TextMeshProUGUI _cardCountText;

    internal void BindRuntimeUi(
        RectTransform runtimePlayer1TradeArea,
        RectTransform runtimePlayer2TradeArea,
        TradeSlotWidget[] runtimePlayer1Slots,
        TradeSlotWidget[] runtimePlayer2Slots,
        CommonButtonWidget runtimeConfirmButton,
        CommonButtonWidget runtimeCancelButton,
        TextMeshProUGUI runtimeStatusText,
        TextMeshProUGUI runtimePlayer1NameText,
        TextMeshProUGUI runtimePlayer2NameText)
    {
        // These fields are normally wired via prefab. Runtime creation must bind them explicitly.
        player1TradeArea = runtimePlayer1TradeArea;
        player2TradeArea = runtimePlayer2TradeArea;
        player1Slots = runtimePlayer1Slots;
        player2Slots = runtimePlayer2Slots;
        confirmButton = runtimeConfirmButton;
        cancelButton = runtimeCancelButton;
        statusText = runtimeStatusText;
        player1NameText = runtimePlayer1NameText;
        player2NameText = runtimePlayer2NameText;

        // Runtime-created panels are bound after Awake(), so we must hook button listeners here.
        try
        {
            if (confirmButton?.button != null)
            {
                confirmButton.button.onClick.RemoveAllListeners();
                confirmButton.button.onClick.AddListener(OnConfirmTrade);

                // User choice: confirm button uses Open behavior + Normal weight.
                try
                {
                    var traverse = HarmonyLib.Traverse.Create(confirmButton);
                    traverse.Field("buttonBehavior").SetValue(0);
                    traverse.Field("buttonWeight").SetValue(0);
                }
                catch
                {
                    // ignored
                }
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            if (cancelButton?.button != null)
            {
                cancelButton.button.onClick.RemoveAllListeners();
                cancelButton.button.onClick.AddListener(OnCancelTrade);
            }
        }
        catch
        {
            // ignored
        }
    }

    #endregion

    #region UiPanel 属性

    /// <summary>
    /// 面板层级，交易面板使用顶层。
    /// </summary>
    public override PanelLayer Layer => PanelLayer.Top;

    #endregion

    #region Unity 生命周期

    /// <summary>
    /// Unity Awake 生命周期回调，用于初始化组件引用及事件绑定。
    /// </summary>
    public void Awake()
    {
        // 获取或添加 CanvasGroup，用于控制面板交互
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 注册按钮点击事件
        confirmButton?.button?.onClick.AddListener(OnConfirmTrade);
        cancelButton?.button?.onClick.AddListener(OnCancelTrade);
    }

    #endregion

    #region 多语言

    /// <summary>
    /// 语言切换时回调，刷新界面文本。
    /// </summary>
    public override void OnLocaleChanged()
    {
        // 语言切换时刷新界面文本（如果当前有有效的 payload）
        if (_payload != null)
        {
            UpdateUIStrings();
        }
    }

    #endregion

    #region 面板生命周期

    /// <summary>
    /// 面板开始显示时（动画前）调用。
    /// </summary>
    /// <param name="payload">交易参数载荷。</param>
    protected override void OnShowing(TradePayload payload)
    {
        // For local UI testing, allow opening without a server when debug toggles are enabled.
        _localDebugTradeMode = !TryEnsureNetworkConnected() && IsLocalDebugTradeAllowed();
        if (!_localDebugTradeMode && !TryEnsureNetworkConnected())
        {
            Hide();
            return;
        }

        // Register input handler early so any MessageDialog shown during setup stacks on top correctly.
        // (Otherwise, MessageDialog/TradePanel Push/Pop order can be reversed and UiManager will log errors.)
        UiManager.PushActionHandler(this);
        _actionHandlerPushed = true;

        // Important: make sure the panel is interactable before we potentially return early
        // due to partner picker / modal dialogs. Otherwise, the picker can be visible but not clickable
        // on subsequent opens after the panel was previously hidden.
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        // 缓存本次交易的参数
        _payload = payload;
        // 根据 payload 设置允许的最大交易卡位
        _maxTradeSlots = payload?.MaxTradeSlots ?? DefaultMaxTradeSlots;
        // 是否允许玩家取消本次交易
        _canCancel = payload?.CanCancel ?? true;

        // 重置交易数据和显示。
        // 注意：如果 payload 未指定交易对象，我们会立即弹出 partner picker。
        ResetTradeData();

        // 初始化交易参与者（联机：会触发 partner picker；本地调试：也需要 partner picker）。
        SetupTradeSession(payload);

        // If we handed off to the dialog, do not create any in-panel editors or issue further UI work.
        if (_handoffToDetailDialog)
        {
            return;
        }

        // 若正在选择交易对象，或已经进入“阻塞提示”状态，则不需要提前初始化报价编辑/卡牌选择等 overlay。
        if (_partnerPickerActive || _blockingCenterMessageActive)
        {
            return;
        }

        // Ensure runtime overlays exist (factory-created panels won't have prefab-wired UI).
        EnsureOfferEditorOverlay();
        EnsureCardPickerOverlay();

        // 设置玩家名称显示（不使用 Player 1/2 之类的占位文本）
        if (player1NameText != null)
        {
            player1NameText.text = ResolveLocalPlayerDisplayName(payload);
        }
        if (player2NameText != null)
        {
            player2NameText.text = ResolvePartnerDisplayName(payload);
        }

        // 根据配置显示/隐藏取消按钮
        cancelButton?.gameObject.SetActive(_canCancel);

        // 允许面板交互
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        // 刷新本地化文案
        UpdateUIStrings();
    }

    /// <summary>
    /// 面板展示完成后调用（动画后）。
    /// </summary>
    protected override void OnShown()
    {
        // 面板显示完成后的处理（当前未使用，预留扩展点）
    }

    /// <summary>
    /// 面板开始隐藏时调用。
    /// </summary>
    protected override void OnHiding()
    {
        // 隐藏动画开始时禁用交互
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = false;
        }

        // 取消注册输入处理器
        if (_actionHandlerPushed)
        {
            UiManager.PopActionHandler(this);
            _actionHandlerPushed = false;
        }

        TryUnsubscribeTradeEvents();
    }

    /// <summary>
    /// 面板完全隐藏后调用。
    /// </summary>
    protected override void OnHided()
    {
        // 完全隐藏后重置数据并清空 payload
        ResetTradeData();
        _payload = null;
    }

    #endregion

    #region UI 文本与状态

    /// <summary>
    /// 根据当前语言和状态刷新 UI 文本。
    /// </summary>
    private void UpdateUIStrings()
    {
        // 设置初始状态提示为“等待放入卡牌”
        UpdateUIStatus(TryLocalize("Trade.WaitingForItems", "等待放入物品..."));

        // Ensure action button labels are stable across prefab/runtime panels.
        try { SetButtonText(confirmButton, "确认交易"); } catch { }
        try { SetButtonText(cancelButton, "取消"); } catch { }
    }

    private static string TryLocalize(string key, string fallback)
    {
        try
        {
            // Some builds/mod packs don't ship these keys, which produces Unity Log noise.
            // Use a readable Chinese fallback so users can still see state changes.
            var s = key.Localize();
            if (string.IsNullOrWhiteSpace(s) || string.Equals(s, key, StringComparison.Ordinal))
            {
                return fallback;
            }

            return s;
        }
        catch
        {
            return fallback;
        }
    }

    /// <summary>
    /// 将交易数据重置并清空所有槽位显示。
    /// </summary>
    private void ResetTradeData()
    {
        // 清空两侧玩家已放入的卡牌列表
        _player1OfferedCards.Clear();
        _player2OfferedCards.Clear();

        _tradeId = null;
        _playerAId = null;
        _playerBId = null;
        _selfPlayerId = null;
        _isApplyingState = false;

        _localDebugTradeMode = false;
        _handoffToDetailDialog = false;

        _localMoneyOffer = 0;
        _localExhibitOfferIds.Clear();
        _lastTradeStatus = null;
        _lastPreparingHandledTimestamp = 0;

        // Clear visible labels early to avoid showing stale/placeholder names.
        if (player1NameText != null)
        {
            player1NameText.text = string.Empty;
        }
        if (player2NameText != null)
        {
            player2NameText.text = string.Empty;
        }

        // Hide any active overlays.
        try
        {
            _partnerPickerActive = false;
            _blockingCenterMessageActive = false;
            if (_partnerPickerRoot != null)
            {
                _partnerPickerRoot.SetActive(false);
            }

            if (_partnerPickerAutoRefreshCo != null)
            {
                StopCoroutine(_partnerPickerAutoRefreshCo);
                _partnerPickerAutoRefreshCo = null;
            }
            if (_cardPickerRoot != null)
            {
                _cardPickerRoot.SetActive(false);
            }
            if (_exhibitPickerRoot != null)
            {
                _exhibitPickerRoot.SetActive(false);
            }
        }
        catch
        {
            // ignored
        }

        // 清空玩家1所有交易槽的显示
        if (player1Slots != null)
        {
            foreach (TradeSlotWidget slot in player1Slots)
            {
                slot?.ClearSlot();
            }
        }

        // 清空玩家2所有交易槽的显示
        if (player2Slots != null)
        {
            foreach (TradeSlotWidget slot in player2Slots)
            {
                slot?.ClearSlot();
            }
        }

        // 默认禁止点击确认按钮，直到双方都放入了卡牌
        if (confirmButton?.button != null)
        {
            confirmButton.button.interactable = false;
        }

        RefreshOfferEditorTexts();
    }

    /// <summary>
    /// 更新状态提示文本。
    /// </summary>
    /// <param name="message">要显示的消息内容。</param>
    private void UpdateUIStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    #endregion

    #region 交易卡牌操作

    /// <summary>
    /// 将一张卡牌加入交易。
    /// </summary>
    /// <param name="card">要加入交易的卡牌实例。</param>
    /// <param name="isPlayer1">true 表示玩家 1，false 表示玩家 2。</param>
    public void AddCardToTrade(Card card, bool isPlayer1)
    {
        // 防守式判空
        if (card == null)
        {
            return;
        }

        // 联机模式下：只允许玩家操作“本地侧”(player1)。
        // 在应用网络状态时会临时放开限制。
        if (!_isApplyingState && TryIsNetworkTrade(out _) && !isPlayer1)
        {
            return;
        }

        // 根据玩家选择对应的卡牌列表
        List<Card> offeredCards = isPlayer1 ? _player1OfferedCards : _player2OfferedCards;

        // 仅在未超过最大交易卡位时添加
        if (offeredCards.Count < _maxTradeSlots)
        {
            // 记录到列表
            offeredCards.Add(card);
            // 更新对应槽位的 UI 显示
            UpdateTradeSlot(card, isPlayer1 ? player1Slots : player2Slots, offeredCards.Count - 1);
            // 尝试检测是否可确认交易
            CheckTradeReady();

            // 同步到 Host
            if (!_isApplyingState)
            {
                TrySendOfferUpdate();
            }
        }
    }

    /// <summary>
    /// 从交易中移除一张已加入的卡牌。
    /// </summary>
    /// <param name="card">要移除的卡牌实例。</param>
    /// <param name="isPlayer1">true 表示玩家 1，false 表示玩家 2。</param>
    public void RemoveCardFromTrade(Card card, bool isPlayer1)
    {
        // 防守式判空
        if (card == null)
        {
            return;
        }

        // 联机模式下：只允许玩家操作“本地侧”(player1)。
        if (!_isApplyingState && TryIsNetworkTrade(out _) && !isPlayer1)
        {
            return;
        }

        // 根据玩家选择对应的卡牌列表和槽位数组
        List<Card> offeredCards = isPlayer1 ? _player1OfferedCards : _player2OfferedCards;
        TradeSlotWidget[] slots = isPlayer1 ? player1Slots : player2Slots;

        // 找到该卡牌在列表中的下标
        int index = offeredCards.IndexOf(card);
        if (index >= 0)
        {
            // 从列表中移除该卡牌
            offeredCards.RemoveAt(index);

            // 从被移除的位置起，将后面的卡牌依次往前移动并刷新 UI
            for (int i = index; i < offeredCards.Count; i++)
            {
                UpdateTradeSlot(offeredCards[i], slots, i);
            }

            // 清空末尾的 UI 槽位（避免旧卡牌残留显示）
            if (slots != null && offeredCards.Count < slots.Length)
            {
                slots[offeredCards.Count]?.ClearSlot();
            }

            // 移除后重新检查是否仍满足双方都有卡牌
            CheckTradeReady();

            // 同步到 Host
            if (!_isApplyingState)
            {
                TrySendOfferUpdate();
            }
        }
    }

    /// <summary>
    /// 刷新指定索引处交易槽的显示。
    /// </summary>
    /// <param name="card">要显示的卡牌。</param>
    /// <param name="slots">所属玩家的槽位数组。</param>
    /// <param name="index">槽位索引。</param>
    private void UpdateTradeSlot(Card card, TradeSlotWidget[] slots, int index)
    {
        // 检查下标和槽位合法性
        if (slots != null && index >= 0 && index < slots.Length && slots[index] != null)
        {
            // 通过引用比较判断属于哪一侧玩家
            bool isPlayer1 = slots == player1Slots;

            // 设置槽位显示的卡牌，并注册点击回调用于移除
            slots[index].SetCard(card, (c) => RemoveCardFromTrade(c, isPlayer1));
        }
    }

    /// <summary>
    /// 检查双方是否已放入至少一张卡牌，从而决定是否允许确认交易。
    /// </summary>
    private void CheckTradeReady()
    {
        // v2: allow trading by any asset (cards/tools/money/exhibits).
        bool localHasOffer = HasLocalOffer();
        bool remoteHasOffer = HasRemoteOffer();
        bool bothPlayersReady = localHasOffer && remoteHasOffer;

        // User choice: never grey out the confirm button in the panel.
        // When not ready, clicking will show a status message but won't send confirm.
        if (confirmButton?.button != null)
        {
            confirmButton.button.interactable = true;
        }

        // 更新提示文本
        if (bothPlayersReady)
        {
            UpdateUIStatus("Trade.ReadyToConfirm".Localize());
        }
        else
        {
            UpdateUIStatus(TryLocalize("Trade.WaitingForItems", "等待放入物品..."));
        }
    }

    private bool HasLocalOffer()
        => (_player1OfferedCards?.Count ?? 0) > 0 || _localMoneyOffer > 0 || _localExhibitOfferIds.Count > 0;

    private bool HasRemoteOffer()
        => (_player2OfferedCards?.Count ?? 0) > 0;

    #endregion

    #region 按钮与输入处理

    /// <summary>
    /// 点击“确认交易”按钮时回调。
    /// </summary>
    private void OnConfirmTrade()
    {
        if (TryIsNetworkTrade(out _))
        {
            // v2: allow confirming when both sides offered ANY assets (cards/tools/money/exhibits).
            // The remote side's offer is sourced from the host state, not necessarily from _player2OfferedCards.
            TradeSyncPatch.TradeSessionState state = TradeSyncPatch.GetLastKnown(_tradeId);
            if (state == null)
            {
                return;
            }

            bool localIsA = string.Equals(state.PlayerAId, _selfPlayerId, StringComparison.Ordinal);
            bool localHasOffer = (localIsA ? (state.OfferA?.Count ?? 0) : (state.OfferB?.Count ?? 0)) > 0
                               || (localIsA ? state.MoneyA : state.MoneyB) > 0
                               || (localIsA ? (state.ExhibitsA?.Count ?? 0) : (state.ExhibitsB?.Count ?? 0)) > 0;
            bool remoteHasOffer = (localIsA ? (state.OfferB?.Count ?? 0) : (state.OfferA?.Count ?? 0)) > 0
                                || (localIsA ? state.MoneyB : state.MoneyA) > 0
                                || (localIsA ? (state.ExhibitsB?.Count ?? 0) : (state.ExhibitsA?.Count ?? 0)) > 0;

            if (!localHasOffer || !remoteHasOffer)
            {
                UpdateUIStatus(TryLocalize("Trade.WaitingForItems", "等待放入物品..."));
                return;
            }

            UpdateUIStatus("Trade.Confirmed".Localize());
            TrySendConfirm();
            return;
        }

        // 单机：未满足条件时只提示，不执行交易。
        if (_player1OfferedCards.Count <= 0 || _player2OfferedCards.Count <= 0)
        {
            UpdateUIStatus(TryLocalize("Trade.WaitingForItems", "等待放入物品..."));
            return;
        }

        // 单机：沿用本地交易
        UpdateUIStatus("Trade.Confirmed".Localize());
        StartCoroutine(ExecuteTrade());
    }

    /// <summary>
    /// 点击“取消交易”按钮时回调。
    /// </summary>
    private void OnCancelTrade()
    {
        if (TryIsNetworkTrade(out _))
        {
            TrySendCancel();
        }

        Hide();
    }

    /// <summary>
    /// 输入系统触发的取消事件回调（例如按下退出/返回键）。
    /// </summary>
    public void OnCancel()
    {
        // 输入事件层面的取消处理，需判断当前是否允许取消
        if (_canCancel)
        {
            OnCancelTrade();
        }
    }

    #endregion

    #region 协程与网络同步

    /// <summary>
    /// 执行实际交易逻辑的协程。
    /// </summary>
    private IEnumerator ExecuteTrade()
    {
        // 联机：该协程仅用于“交易完成后本地落地”。
        if (TryIsNetworkTrade(out _))
        {
            bool isA = string.Equals(_selfPlayerId, _playerAId, StringComparison.Ordinal);
            yield return ApplyNetworkTradeAndClose(isA);
            yield break;
        }

        // 禁用按钮以防止重复点击触发多次交易
        if (confirmButton?.button != null)
        {
            confirmButton.button.interactable = false;
        }
        if (cancelButton?.button != null)
        {
            cancelButton.button.interactable = false;
        }

        // 将玩家1提供的卡牌从其卡组移除并加入到玩家2（当前实现视为本地玩家）
        foreach (Card card in _player1OfferedCards)
        {
            // 从玩家1移除卡牌（false 可根据实际游戏逻辑表示来源）
            GameRun.RemoveDeckCard(card, false);
            // 将卡牌加入到另一方（当前为本地卡组，网络同步需另行处理）
            GameRun.AddDeckCard(card, true, new VisualSourceData
            {
                SourceType = VisualSourceType.CardSelect
            });
        }

        // 将玩家2提供的卡牌加入到玩家1侧（目前仅做本地添加）
        foreach (Card card in _player2OfferedCards)
        {
            // 从玩家2移除卡牌（网络模式下需要真正从对方卡组移除）
            // NOTE: 单机分支无法实现“从对方卡组移除”的多玩家卡组操作。
            // 联机交易已通过 TradeSyncPatch 的 Host 权威裁决 + 客户端本地落地（ApplyNetworkTradeAndClose）实现，且不会走到该分支。
            GameRun.AddDeckCard(card, true, new VisualSourceData
            {
                SourceType = VisualSourceType.CardSelect
            });
        }

        // 更新状态为“交易完成”
        UpdateUIStatus("Trade.Completed".Localize());

        // 发送网络事件通知其他玩家本次交易已经完成
        SendTradeEvent();

        // 等待一小段时间，让玩家看清结果
        yield return new WaitForSeconds(TradeCompleteWaitTime);

        // 关闭交易面板
        Hide();
    }

    /// <summary>
    /// 发送交易完成事件到同步管理器（网络广播）。
    /// </summary>
    private void SendTradeEvent()
    {
        try
        {
            // v1：交易完成事件由 TradeSyncPatch 统一发送/广播。
            // 这里保留方法作为本地模式的扩展点。
        }
        catch (Exception ex)
        {
            // 打印网络事件发送失败的错误日志
            Debug.LogError($"[TradePanel] Failed to send trade event: {ex.Message}");
        }
    }

    private void SetupTradeSession(TradePayload payload)
    {
        try
        {
            INetworkClient client = ModService.ServiceProvider.GetService<INetworkClient>();
            bool connected = client != null && client.IsConnected;
            if (connected)
            {
                NetworkIdentityTracker.EnsureSubscribed(client);
                TradeSyncPatch.EnsureSubscribed(client);

                _selfPlayerId = NetworkIdentityTracker.GetSelfPlayerId();
                if (string.IsNullOrWhiteSpace(_selfPlayerId))
                {
                    return;
                }
            }
            else
            {
                // Offline/local debug path: fabricate a stable local identity so filters and labels work.
                // This avoids waiting for any network state updates.
                _selfPlayerId = "local";
            }

            _tradeId = payload?.TradeId;
            if (string.IsNullOrWhiteSpace(_tradeId))
            {
                _tradeId = Guid.NewGuid().ToString("N");
            }

            _playerAId = payload?.Player1Id;
            if (string.IsNullOrWhiteSpace(_playerAId))
            {
                _playerAId = _selfPlayerId;
            }

            _playerBId = payload?.Player2Id;

            // Per requirements: must have UI to pick partner if not explicitly provided or invalid.
            if (string.IsNullOrWhiteSpace(_playerBId) || string.Equals(_playerBId, _selfPlayerId, StringComparison.Ordinal))
            {
                ShowPartnerPickerOverlay();
                return;
            }

            // Connected: request the host-driven session. Offline/local debug: skip network.
            if (connected)
            {
                // Prefer the dialog-based editor. If dialog creation fails, fall back to in-panel flow.
                if (TryShowTradeDetailDialog(_playerBId, payload?.Player2Name))
                {
                    _handoffToDetailDialog = true;
                    Hide(false);
                    return;
                }

                TrySubscribeTradeEvents();

                // 若本端是参与者之一，发起会话（Host 会裁决并广播状态）。
                TradeSyncPatch.RequestStartTrade(_tradeId, _playerAId, _playerBId, _maxTradeSlots);
                TradeSyncPatch.RequestSnapshot(_tradeId, _selfPlayerId);
            }
            else
            {
                // Local debug session: show trade details immediately.
                EnsureOfferEditorOverlay();
                EnsureCardPickerOverlay();
                EnsureExhibitPickerOverlay();
                SetTradeDetailsVisible(true);
                cancelButton?.gameObject.SetActive(_canCancel);
                UpdateUIStatus("本地调试交易：未连接服务器");
            }
        }
        catch
        {
            // ignored
        }
    }

    private bool TryShowTradeDetailDialog(string partnerPlayerId, string partnerPreferredName)
    {
        try
        {
            // Only hand off when we are actually connected; local debug stays in-panel.
            if (!TryIsNetworkTrade(out _))
            {
                return false;
            }

            var dialog = TradeDetailDialogRuntimeFactory.GetOrCreate();
            if (dialog == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(_tradeId))
            {
                _tradeId = Guid.NewGuid().ToString("N");
            }

            dialog.Show(new TradeDetailPayload
            {
                TradeId = _tradeId,
                SelfPlayerId = _selfPlayerId,
                PartnerPlayerId = partnerPlayerId,
                PartnerPlayerName = TryResolveDisplayName(partnerPlayerId, partnerPreferredName, isLocal: false),
                MaxTradeSlots = _maxTradeSlots
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsLocalDebugTradeAllowed()
    {
        try
        {
            var cfg = ModService.ServiceProvider.GetService<NetworkPlugin.Configuration.ConfigManager>();
            return cfg?.DebugVirtualPlayerAiDefault?.Value == true || cfg?.DebugFakePlayersForTrade?.Value == true;
        }
        catch
        {
            return false;
        }
    }

    private bool TryEnsureNetworkConnected()
    {
        try
        {
            INetworkClient client = ModService.ServiceProvider.GetService<INetworkClient>();
            if (client == null || !client.IsConnected)
            {
                TryShowTopMessage("交易仅在联机模式下可用。");
                return false;
            }

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

    private void ShowPartnerPickerOverlay()
    {
        try
        {
            if (_partnerPickerActive)
            {
                return;
            }

            // Ensure the overlay can receive clicks even if the panel was previously hidden.
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            // Build overlay lazily.
            EnsurePartnerPickerOverlay();
            if (_partnerPickerRoot == null)
            {
                ShowTradeTargetUnavailableDialog(_partnerPickerBuildError);
                return;
            }

            _partnerPickerActive = true;
            _partnerPickerRoot.SetActive(true);

            // We instantiate MessageDialog as a prefab without calling UiDialog.Show(), so its CanvasGroup
            // may still be non-interactable by default. Force-enable raycasts so partner rows are clickable.
            ForceEnableRaycasts(_partnerPickerRoot);

            // Hide the underlying trade details until a partner is selected.
            SetTradeDetailsVisible(false);

            // The picker provides its own cancel button; hide the underlying one to avoid visual duplication.
            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(false);
            }

            // Disable underlying trade UI until a partner is selected.
            // NOTE: Do NOT disable the root CanvasGroup here.
            // The partner picker overlay is a child of TradePanel, so disabling the root CanvasGroup
            // would also make the overlay buttons (including Cancel and partner rows) unclickable.

            // Avoid duplicated texts (overlay already has a title).
            UpdateUIStatus(string.Empty);
            RebuildPartnerPickerList();

            // If self location is not yet available, show a lightweight waiting message and refresh once.
            if (!OtherPlayersOverlayPatch.TryGetSelfLocation(out _, out _, out _, out _))
            {
                TryShowPartnerPickerWaiting();
                if (_partnerPickerAutoRefreshCo != null)
                {
                    StopCoroutine(_partnerPickerAutoRefreshCo);
                    _partnerPickerAutoRefreshCo = null;
                }
                _partnerPickerAutoRefreshCo = StartCoroutine(CoPartnerPickerAutoRefreshOnce());
            }
        }
        catch
        {
            // ignored
        }
    }

    private void TryShowPartnerPickerWaiting()
    {
        try
        {
            if (_partnerPickerRoot == null)
            {
                return;
            }

            PartnerPickerTag tag = _partnerPickerRoot.GetComponentInChildren<PartnerPickerTag>(true);
            if (tag == null)
            {
                return;
            }

            if (tag.ScrollRect != null)
            {
                tag.ScrollRect.gameObject.SetActive(false);
            }

            if (tag.EmptyText != null)
            {
                tag.EmptyText.gameObject.SetActive(true);
                tag.EmptyText.text = "正在同步位置信息...";
                tag.EmptyText.alignment = TextAlignmentOptions.Center;
                var c = tag.EmptyText.color;
                c.a = 1f;
                tag.EmptyText.color = c;
            }
        }
        catch
        {
            // ignored
        }
    }

    private IEnumerator CoPartnerPickerAutoRefreshOnce()
    {
        // Wait up to 1.0s and refresh the list once.
        float t = 0f;
        while (t < 1.0f)
        {
            if (!_partnerPickerActive || _partnerPickerRoot == null)
            {
                _partnerPickerAutoRefreshCo = null;
                yield break;
            }

            // If we already have location, we can refresh immediately.
            if (OtherPlayersOverlayPatch.TryGetSelfLocation(out _, out _, out _, out _))
            {
                break;
            }

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        try
        {
            if (_partnerPickerActive && _partnerPickerRoot != null)
            {
                RebuildPartnerPickerList();
            }
        }
        catch
        {
            // ignored
        }

        _partnerPickerAutoRefreshCo = null;
    }

    private static void ForceEnableRaycasts(GameObject root)
    {
        try
        {
            if (root == null)
            {
                return;
            }

            foreach (var cg in root.GetComponentsInChildren<CanvasGroup>(true))
            {
                if (cg == null)
                {
                    continue;
                }

                cg.interactable = true;
                cg.blocksRaycasts = true;

                // Ensure the dialog is still clickable even if a parent CanvasGroup is temporarily disabled.
                cg.ignoreParentGroups = true;
            }
        }
        catch
        {
            // ignored
        }
    }

    private void ShowTradeTargetUnavailableDialog(string detail)
    {
        // Match vanilla: UiManager-managed MessageDialog plays its transition animations.
        try
        {
            if (!UiManager.IsInitialized)
            {
                return;
            }

            // Block the underlying trade UI while the dialog is up.
            _blockingCenterMessageActive = true;
            SetTradeDetailsVisible(false);

            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(false);
            }
            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(false);
            }
            UpdateUIStatus(string.Empty);
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
            }

            UiManager.GetDialog<MessageDialog>().Show(new MessageContent
            {
                Text = "交易对象不可用",
                SubText = string.IsNullOrWhiteSpace(detail) ? "交易对象选择界面不可用。" : ("交易对象选择界面不可用。\n" + detail),
                Icon = MessageIcon.Error,
                Buttons = DialogButtons.Confirm,
                OnConfirm = () =>
                {
                    try
                    {
                        _blockingCenterMessageActive = false;
                        Hide();
                    }
                    catch
                    {
                        // ignored
                    }
                },
                OnCancel = () =>
                {
                    try
                    {
                        _blockingCenterMessageActive = false;
                        Hide();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            });
        }
        catch
        {
            // ignored
        }
    }

    private void HidePartnerPickerOverlay()
    {
        if (_partnerPickerAutoRefreshCo != null)
        {
            try
            {
                StopCoroutine(_partnerPickerAutoRefreshCo);
            }
            catch
            {
                // ignored
            }
            _partnerPickerAutoRefreshCo = null;
        }

        _partnerPickerActive = false;
        if (_partnerPickerRoot != null)
        {
            _partnerPickerRoot.SetActive(false);
        }

        // Re-enable the trade UI.
        SetTradeDetailsVisible(true);

        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        // Restore the underlying cancel button state.
        if (cancelButton != null)
        {
            cancelButton.gameObject.SetActive(_canCancel);
        }
    }

    private void SetTradeDetailsVisible(bool visible)
    {
        try
        {
            // 背景框（RuntimeFactory 创建时挂在根节点下的 TradeFrame）随主界面一起显隐。
            var tradeFrame = transform.Find("TradeFrame");
            if (tradeFrame != null)
            {
                tradeFrame.gameObject.SetActive(visible);
            }

            if (statusText != null)
            {
                statusText.gameObject.SetActive(visible);
            }

            if (player1TradeArea != null)
            {
                player1TradeArea.gameObject.SetActive(visible);
            }
            if (player2TradeArea != null)
            {
                player2TradeArea.gameObject.SetActive(visible);
            }
            if (player1NameText != null)
            {
                player1NameText.gameObject.SetActive(visible);
            }
            if (player2NameText != null)
            {
                player2NameText.gameObject.SetActive(visible);
            }

            // Confirm/cancel should remain visible while partner picker is open only if you want.
            // We hide confirm to reduce confusion before a session starts.
            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(visible);
            }

            // Keep cancel visible if cancel is allowed.
            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(visible || _canCancel);
            }

            if (_offerEditorRoot != null)
            {
                _offerEditorRoot.SetActive(visible);
            }

            if (_offerActionsRoot != null)
            {
                _offerActionsRoot.SetActive(visible);
            }
        }
        catch
        {
            // ignored
        }
    }

    private void EnsurePartnerPickerOverlay()
    {
        if (_partnerPickerRoot != null)
        {
            return;
        }

        _partnerPickerBuildError = null;
        try
        {
            Transform parent = transform;

            // Strict requirement: use an in-game dialog prefab for the overlay/mask/window frame,
            // instead of runtime-only Images/Outline.
            GameObject prefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (prefab == null)
            {
                _partnerPickerBuildError = "无法加载 UI/Dialogs/MessageDialog";
                _partnerPickerRoot = null;
                return;
            }

            _partnerPickerRoot = Instantiate(prefab, parent, false);
            _partnerPickerRoot.name = "TradePartnerPicker";
            _partnerPickerRoot.SetActive(false);

            RectTransform rootRect = _partnerPickerRoot.GetComponent<RectTransform>();
            if (rootRect != null)
            {
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.offsetMin = Vector2.zero;
                rootRect.offsetMax = Vector2.zero;
            }

            MessageDialog dialog = _partnerPickerRoot.GetComponentInChildren<MessageDialog>(true);
            if (dialog == null)
            {
                _partnerPickerBuildError = "MessageDialog 组件缺失";
                Destroy(_partnerPickerRoot);
                _partnerPickerRoot = null;
                return;
            }

            // Extract serialized fields via reflection so we can reuse the prefab's text/buttons.
            TextMeshProUGUI mainText = GetDialogField<TextMeshProUGUI>(dialog, "mainText");
            TextMeshProUGUI subText = GetDialogField<TextMeshProUGUI>(dialog, "subText");
            Button singleConfirm = GetDialogField<Button>(dialog, "singleConfirmButton");
            Button confirm = GetDialogField<Button>(dialog, "confirmButton");
            Button cancel = GetDialogField<Button>(dialog, "cancelButton");

            RectTransform subTextRect = subText != null ? subText.rectTransform : null;

            if (mainText != null)
            {
                mainText.text = "请选择交易对象";
                mainText.alignment = TextAlignmentOptions.Center;
                mainText.raycastTarget = false;
            }

            if (subText != null)
            {
                // Reuse its rect as the list placeholder; make it a clickable text (tap to refresh).
                subText.text = string.Empty;
                subText.raycastTarget = true;
                subText.alignment = TextAlignmentOptions.Center;
                var c = subText.color;
                c.a = 0f;
                subText.color = c;
                subText.gameObject.SetActive(true);

                // Add a Button so the empty-state label is tappable (triggers a refresh).
                var subTextBtn = subText.gameObject.GetComponent<Button>() ?? subText.gameObject.AddComponent<Button>();
                subTextBtn.targetGraphic = subText;
                subTextBtn.onClick.RemoveAllListeners();
                subTextBtn.onClick.AddListener(() =>
                {
                    try { OnPartnerPickerRefreshClicked(); }
                    catch { }
                });
            }

            // Ensure dialog buttons don't call UiDialog.Hide() (which would touch UiManager current dialog state).
            if (singleConfirm != null)
            {
                singleConfirm.onClick.RemoveAllListeners();
                Destroy(singleConfirm.gameObject);
            }
            if (confirm != null)
            {
                confirm.onClick.RemoveAllListeners();
                confirm.gameObject.SetActive(true);

                _partnerPickerRefreshButton = confirm;

                var refreshLabel = confirm.GetComponentInChildren<TextMeshProUGUI>(true);
                if (refreshLabel != null)
                {
                    refreshLabel.text = "刷新";
                    refreshLabel.alignment = TextAlignmentOptions.Center;
                }

                confirm.onClick.AddListener(() =>
                {
                    try
                    {
                        OnPartnerPickerRefreshClicked();
                    }
                    catch
                    {
                        // ignored
                    }
                });
            }

            if (cancel != null)
            {
                cancel.onClick.RemoveAllListeners();
                cancel.gameObject.SetActive(true);

                _partnerPickerCancelButton = cancel;

                var cancelLabel = cancel.GetComponentInChildren<TextMeshProUGUI>(true);
                if (cancelLabel != null)
                {
                    cancelLabel.text = "取消";
                    cancelLabel.alignment = TextAlignmentOptions.Center;
                }

                cancel.onClick.AddListener(() =>
                {
                    try
                    {
                        HidePartnerPickerOverlay();
                        Hide();
                    }
                    catch
                    {
                        // ignored
                    }
                });
            }

            // Safety net: if row-level pointer events are blocked by prefab raycast targets/layering,
            // catch clicks at the overlay root and resolve the clicked row by rectangle hit testing.
            var catcher = _partnerPickerRoot.GetComponent<PartnerPickerClickCatcher>();
            if (catcher == null)
            {
                catcher = _partnerPickerRoot.AddComponent<PartnerPickerClickCatcher>();
            }
            catcher.Panel = this;

            RectTransform panelRect = TryFindCommonAncestorRect(mainText != null ? mainText.rectTransform : null,
                cancel != null ? cancel.GetComponent<RectTransform>() : null);
            if (panelRect == null)
            {
                panelRect = mainText != null ? mainText.rectTransform.parent as RectTransform : null;
            }
            if (panelRect == null)
            {
                panelRect = rootRect;
            }
            if (panelRect == null)
            {
                Destroy(_partnerPickerRoot);
                _partnerPickerRoot = null;
                _partnerPickerBuildError = "找不到可挂载列表的面板 RectTransform";
                return;
            }

            // Build a simple ScrollRect container for TMP clickable-text partner entries.
            TextMeshProUGUI pickerTextTemplate = mainText ?? subText;
            if (pickerTextTemplate == null)
            {
                Destroy(_partnerPickerRoot);
                _partnerPickerRoot = null;
                _partnerPickerBuildError = "无法获取文字模板";
                return;
            }

            {
                var scrollGo = new GameObject("PartnerScroll");
                scrollGo.transform.SetParent(panelRect, false);
                scrollGo.transform.SetAsLastSibling();

                var scrollRt = scrollGo.AddComponent<RectTransform>();
                if (subTextRect != null)
                {
                    CopyRectTransform(scrollRt, subTextRect);
                }
                else
                {
                    scrollRt.anchorMin = new Vector2(0.06f, 0.20f);
                    scrollRt.anchorMax = new Vector2(0.94f, 0.78f);
                    scrollRt.offsetMin = Vector2.zero;
                    scrollRt.offsetMax = Vector2.zero;
                }

                var scrollImg = scrollGo.AddComponent<Image>();
                scrollImg.color = new Color(0f, 0f, 0f, 0f);
                scrollImg.raycastTarget = true;

                var scrollRect = scrollGo.AddComponent<ScrollRect>();
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                scrollRect.movementType = ScrollRect.MovementType.Clamped;

                var viewport = new GameObject("Viewport");
                viewport.transform.SetParent(scrollGo.transform, false);
                var viewportRt = viewport.AddComponent<RectTransform>();
                viewportRt.anchorMin = Vector2.zero;
                viewportRt.anchorMax = Vector2.one;
                viewportRt.offsetMin = Vector2.zero;
                viewportRt.offsetMax = Vector2.zero;
                viewport.AddComponent<RectMask2D>();

                var contentGo = new GameObject("Content");
                contentGo.transform.SetParent(viewport.transform, false);
                var contentRt = contentGo.AddComponent<RectTransform>();
                contentRt.anchorMin = new Vector2(0f, 1f);
                contentRt.anchorMax = new Vector2(1f, 1f);
                contentRt.pivot = new Vector2(0.5f, 1f);
                contentRt.sizeDelta = new Vector2(0f, 0f);

                var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
                vlg.childAlignment = TextAnchor.UpperCenter;
                vlg.spacing = 8f;
                vlg.padding = new RectOffset(10, 10, 10, 10);
                vlg.childControlWidth = true;
                vlg.childControlHeight = false;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;

                var csf = contentGo.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                scrollRect.viewport = viewportRt;
                scrollRect.content = contentRt;

                var tag = contentGo.AddComponent<PartnerPickerTag>();
                tag.TextTemplate = pickerTextTemplate;
                tag.ListContainer = contentRt;
                tag.ScrollRect = scrollRect;
                tag.EmptyText = subText;
            }

            // Keep cancel above the list.
            try
            {
                if (cancel != null)
                {
                    cancel.transform.SetAsLastSibling();
                }
            }
            catch
            {
                // ignored
            }

            // Validate the tag was created successfully.
            PartnerPickerTag tagCheck = _partnerPickerRoot.GetComponentInChildren<PartnerPickerTag>(true);
            if (tagCheck == null || tagCheck.TextTemplate == null)
            {
                Destroy(_partnerPickerRoot);
                _partnerPickerRoot = null;
                _partnerPickerBuildError = "列表模板创建失败";
                return;
            }

            // Disable the dialog component to avoid unexpected input handling; we only need its visuals.
            dialog.enabled = false;
        }
        catch
        {
            if (string.IsNullOrWhiteSpace(_partnerPickerBuildError))
            {
                _partnerPickerBuildError = "构建交易对象选择界面时发生异常";
            }
            try
            {
                if (_partnerPickerRoot != null)
                {
                    Destroy(_partnerPickerRoot);
                }
            }
            catch
            {
                // ignored
            }
            _partnerPickerRoot = null;
        }
    }

    private void OnPartnerPickerRefreshClicked()
    {
        if (!_partnerPickerActive || _partnerPickerRoot == null)
        {
            return;
        }

        // Always attempt an immediate rebuild.
        RebuildPartnerPickerList();

        // If self location is still unavailable, show the waiting state and schedule a one-shot refresh.
        if (!OtherPlayersOverlayPatch.TryGetSelfLocation(out _, out _, out _, out _))
        {
            TryShowPartnerPickerWaiting();

            if (_partnerPickerAutoRefreshCo != null)
            {
                try
                {
                    StopCoroutine(_partnerPickerAutoRefreshCo);
                }
                catch
                {
                    // ignored
                }
                _partnerPickerAutoRefreshCo = null;
            }

            _partnerPickerAutoRefreshCo = StartCoroutine(CoPartnerPickerAutoRefreshOnce());
        }
    }

    private static void CopyRectTransform(RectTransform dst, RectTransform src)
    {
        if (dst == null || src == null)
        {
            return;
        }

        dst.anchorMin = src.anchorMin;
        dst.anchorMax = src.anchorMax;
        dst.pivot = src.pivot;
        dst.anchoredPosition = src.anchoredPosition;
        dst.sizeDelta = src.sizeDelta;
        dst.offsetMin = src.offsetMin;
        dst.offsetMax = src.offsetMax;
        dst.localScale = src.localScale;
    }

    private static T GetDialogField<T>(MessageDialog dialog, string fieldName) where T : class
    {
        try
        {
            if (dialog == null || string.IsNullOrWhiteSpace(fieldName))
            {
                return null;
            }

            FieldInfo fi = typeof(MessageDialog).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi == null)
            {
                return null;
            }

            return fi.GetValue(dialog) as T;
        }
        catch
        {
            return null;
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

            Type t = target.GetType();
            while (t != null)
            {
                FieldInfo fi = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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

    private static RectTransform TryFindCommonAncestorRect(RectTransform a, RectTransform b)
    {
        try
        {
            if (a == null || b == null)
            {
                return null;
            }

            var ancestors = new HashSet<Transform>();
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
        catch
        {
            return null;
        }
    }

    private void RebuildPartnerPickerList()
    {
        try
        {
            if (_partnerPickerRoot == null)
            {
                return;
            }

            PartnerPickerTag tag = _partnerPickerRoot.GetComponentInChildren<PartnerPickerTag>(true);
            if (tag == null || tag.TextTemplate == null)
            {
                return;
            }

            Transform container = tag.ListContainer != null ? (Transform)tag.ListContainer : tag.transform;
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            string selfId = _selfPlayerId ?? NetworkIdentityTracker.GetSelfPlayerId();

            // Prefer detailed snapshot so we can filter to "players currently in shop" and render avatar/where.
            var players = OtherPlayersOverlayPatch.SnapshotPlayersDetailed();

            bool hasSelfLoc = OtherPlayersOverlayPatch.TryGetSelfLocation(out int selfStage, out int selfX, out int selfY, out string selfLocName);

            // Some environments may not inject the virtual debug player into the snapshot early enough.
            // If debug toggles are enabled, synthesize an "AI Default" entry aligned to self location so it
            // obeys the strict same-node rule and can be used for local UI testing.
            if (hasSelfLoc && IsLocalDebugTradeAllowed())
            {
                try
                {
                    if (players.All(p => !string.Equals(p.PlayerId, "aidefault", StringComparison.Ordinal)))
                    {
                        string loc = IsShopLikeLocation(selfLocName) ? selfLocName : "Trade";
                        players.Add(("aidefault", "AI Default", true, false, selfStage, selfX, selfY, loc, null));
                    }
                }
                catch
                {
                    // ignored
                }
            }

            var connectedOthers = players
                .Where(p => !string.IsNullOrWhiteSpace(p.PlayerId))
                .Where(p => !string.Equals(p.PlayerId, selfId, StringComparison.Ordinal))
                .Where(p => p.IsConnected)
                .ToList();

            var candidates = connectedOthers
                .Where(p => IsShopLikeLocation(p.LocationName))
                .ToList();

            // Chosen rule: must be on the same node to be selectable.
            // If self location is unknown, we cannot safely enforce the rule.
            if (!hasSelfLoc)
            {
                candidates.Clear();
            }
            else
            {
                candidates = candidates
                    .Where(p => p.Stage >= 0 && selfStage >= 0 && p.Stage == selfStage
                                && p.LocationX >= 0 && selfX >= 0 && p.LocationX == selfX
                                && p.LocationY >= 0 && selfY >= 0 && p.LocationY == selfY)
                    .ToList();
            }

            if (candidates.Count == 0)
            {
                // Strict mode: empty state must also use in-game UI elements.
                if (tag.ScrollRect != null)
                {
                    tag.ScrollRect.gameObject.SetActive(false);
                }

                if (tag.EmptyText != null)
                {
                    tag.EmptyText.gameObject.SetActive(true);
                    tag.EmptyText.text = hasSelfLoc
                        ? "暂无同节点玩家"
                        : "无法获取自身位置，暂不显示可交易玩家";
                    tag.EmptyText.alignment = TextAlignmentOptions.Center;
                    var c = tag.EmptyText.color;
                    c.a = 1f;
                    tag.EmptyText.color = c;
                }
                return;
            }

            if (tag.EmptyText != null)
            {
                tag.EmptyText.text = string.Empty;
                var c = tag.EmptyText.color;
                c.a = 0f;
                tag.EmptyText.color = c;
            }
            if (tag.ScrollRect != null)
            {
                tag.ScrollRect.gameObject.SetActive(true);
            }

            foreach (var p in candidates)
            {
                string displayName = string.IsNullOrWhiteSpace(p.PlayerName) ? p.PlayerId : p.PlayerName;

                bool sameNode = hasSelfLoc
                    && (p.Stage < 0 || selfStage < 0 || p.Stage == selfStage)
                    && (p.LocationX < 0 || selfX < 0 || p.LocationX == selfX)
                    && (p.LocationY < 0 || selfY < 0 || p.LocationY == selfY);

                string where = BuildWhereText(p.LocationName, p.Stage, p.LocationX, p.LocationY, sameNode);

                string label = string.IsNullOrWhiteSpace(where) ? displayName : $"{displayName}  {where}";
                if (p.IsHost)
                {
                    label += " [Host]";
                }

                Button btn = CreateTextButton(tag.TextTemplate, container, $"Player_{p.PlayerId}", label, tag.TextTemplate.fontSize * 0.5f);

                var le = btn.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 40f;
                le.flexibleWidth = 1f;

                var cand = btn.gameObject.AddComponent<PartnerCandidateTag>();
                cand.PlayerId = p.PlayerId;
                cand.PlayerName = displayName;

                string pid = p.PlayerId;
                string pname = displayName;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    try { OnPartnerSelected(pid, pname); }
                    catch { }
                });
            }
        }
        catch
        {
            // ignored
        }
    }

    private void OnPartnerSelected(string partnerPlayerId, string partnerPlayerName)
    {
        try
        {
            _playerAId = _selfPlayerId;
            _playerBId = partnerPlayerId;

            if (string.IsNullOrWhiteSpace(_tradeId))
            {
                _tradeId = Guid.NewGuid().ToString("N");
            }

            if (player1NameText != null)
            {
                player1NameText.text = ResolveLocalPlayerDisplayName(_payload);
            }
            if (player2NameText != null)
            {
                player2NameText.text = TryResolveDisplayName(partnerPlayerId, partnerPlayerName, isLocal: false);
            }

            HidePartnerPickerOverlay();

            // Connected: proceed with the real host-driven session.
            // Offline/local debug: keep UI local (no network requests).
            if (IsLocalDebugTradeAllowed() && string.Equals(partnerPlayerId, "aidefault", StringComparison.Ordinal))
            {
                // Even when connected, allow selecting aidefault to start a purely local UI test session.
                _localDebugTradeMode = true;
                PopulateLocalDebugRemoteOffer();
                EnsureOfferEditorOverlay();
                EnsureCardPickerOverlay();
                EnsureExhibitPickerOverlay();
                SetTradeDetailsVisible(true);
                cancelButton?.gameObject.SetActive(_canCancel);
                UpdateUIStatus("本地调试交易：AI Default（不走服务器）");
                return;
            }

            // Prefer the dialog-based editor for network trades.
            if (TryShowTradeDetailDialog(partnerPlayerId, partnerPlayerName))
            {
                _handoffToDetailDialog = true;
                Hide(false);
                return;
            }

            // Dialog failed: fall back to the in-panel editor so trading remains usable.
            UpdateUIStatus("交易详情界面不可用，已回退到面板模式");
            EnsureOfferEditorOverlay();
            EnsureCardPickerOverlay();
            EnsureExhibitPickerOverlay();
            SetTradeDetailsVisible(true);
            cancelButton?.gameObject.SetActive(_canCancel);

            if (TryIsNetworkTrade(out _))
            {
                TrySubscribeTradeEvents();
                TradeSyncPatch.RequestStartTrade(_tradeId, _playerAId, _playerBId, _maxTradeSlots);
                TradeSyncPatch.RequestSnapshot(_tradeId, _selfPlayerId);
            }
            else
            {
                UpdateUIStatus("本地调试交易：未连接服务器");
            }
        }
        catch
        {
            // ignored
        }
    }

    private void PopulateLocalDebugRemoteOffer()
    {
        // Populate the remote side (player2) with a small deterministic offer so UI can be tested offline.
        // This must not touch the real deck/inventory because it is just for display.
        try
        {
            using (new ApplyingStateScope(this))
            {
                // Pick a few local deck cards and create display clones for the remote side.
                var deck = GameRun?.BaseDeck?.Where(c => c != null).ToList() ?? new List<Card>();
                int take = Math.Min(2, deck.Count);
                for (int i = 0; i < take; i++)
                {
                    var src = deck[i];
                    if (src == null)
                    {
                        continue;
                    }

                    Card temp = null;
                    try
                    {
                        temp = Library.TryCreateCard(src.Id, src.IsUpgraded, src.UpgradeCounter ?? 0);
                    }
                    catch
                    {
                        temp = null;
                    }

                    if (temp != null)
                    {
                        AddCardToTrade(temp, false);
                    }
                }

                // Add a small money offer on the local side so the offer editor shows non-zero state.
                _localMoneyOffer = Math.Min(10, GameRun?.Money ?? 10);
                RefreshOfferEditorTexts();
                CheckTradeReady();
            }
        }
        catch
        {
            // ignored
        }
    }

    private static string BuildWhereText(string locationName, int stage, int x, int y, bool sameNode)
    {
        string loc = string.IsNullOrWhiteSpace(locationName) ? "?" : locationName;
        string coord = (stage >= 0 || x >= 0 || y >= 0) ? $"Act {stage}, ({x},{y})" : "位置未知";
        return sameNode ? $"{loc} - {coord} - 同节点" : $"{loc} - {coord}";
    }

    private string ResolveLocalPlayerDisplayName(TradePayload payload)
    {
        string id = _selfPlayerId;
        if (string.IsNullOrWhiteSpace(id))
        {
            id = payload?.Player1Id;
        }

        return TryResolveDisplayName(id, payload?.Player1Name, isLocal: true);
    }

    private string ResolvePartnerDisplayName(TradePayload payload)
    {
        // If partner picker is used, _playerBId will be set after selection.
        string id = _playerBId;
        if (string.IsNullOrWhiteSpace(id))
        {
            id = payload?.Player2Id;
        }

        return TryResolveDisplayName(id, payload?.Player2Name, isLocal: false);
    }

    private static string TryResolveDisplayName(string playerId, string preferredName, bool isLocal)
    {
        if (!string.IsNullOrWhiteSpace(preferredName))
        {
            return preferredName;
        }

        // Prefer network-provided player names (same source as other-player overlay).
        try
        {
            if (!string.IsNullOrWhiteSpace(playerId))
            {
                var snap = OtherPlayersOverlayPatch.SnapshotPlayersDetailed();
                foreach (var p in snap)
                {
                    if (string.Equals(p.PlayerId, playerId, StringComparison.Ordinal)
                        && !string.IsNullOrWhiteSpace(p.PlayerName)
                        && !string.Equals(p.PlayerName, p.PlayerId, StringComparison.Ordinal))
                    {
                        return p.PlayerName;
                    }
                }
            }
        }
        catch
        {
            // ignored
        }

        // For local player, fall back to the in-run unit name (character name) rather than a generic placeholder.
        if (isLocal)
        {
            try
            {
                string n = GameStateUtils.GetCurrentPlayer()?.Name;
                if (!string.IsNullOrWhiteSpace(n))
                {
                    return n;
                }
            }
            catch
            {
                // ignored
            }
        }

        // Last resort: show the ID (still better than "Player 1/2").
        return string.IsNullOrWhiteSpace(playerId) ? string.Empty : playerId;
    }

    private static bool IsShopLikeLocation(string locationName)
    {
        if (string.IsNullOrWhiteSpace(locationName))
        {
            return false;
        }

        // LocationName is set by network sync to visitingNode.StationType.ToString().
        // We match loosely to be resilient to renames/variants.
        // Eligible contexts for trade: Shop/Trade (merchant) and Gap (campfire-style).
        return locationName.IndexOf("shop", StringComparison.OrdinalIgnoreCase) >= 0
            || locationName.IndexOf("trade", StringComparison.OrdinalIgnoreCase) >= 0
            || locationName.IndexOf("gap", StringComparison.OrdinalIgnoreCase) >= 0
            || locationName.IndexOf("商店", StringComparison.OrdinalIgnoreCase) >= 0
            || locationName.IndexOf("交易", StringComparison.OrdinalIgnoreCase) >= 0
            || locationName.IndexOf("间隙", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static Sprite TryLoadAvatarSprite(string characterId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            return ResourcesHelper.LoadCharacterAvatarSprite(characterId);
        }
        catch
        {
            return null;
        }
    }

    private void EnsureCardPickerOverlay()
    {
        if (_cardPickerRoot != null)
        {
            return;
        }

        try
        {
            // Strict: use in-game UI prefabs (MessageDialog + HistoryPanel list + RecordRow). No runtime-built UI.
            GameObject dialogPrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (dialogPrefab == null)
            {
                _cardPickerRoot = null;
                return;
            }

            _cardPickerRoot = Instantiate(dialogPrefab, transform, false);
            _cardPickerRoot.name = "TradeCardPicker";
            _cardPickerRoot.SetActive(false);

            var rootRect = _cardPickerRoot.GetComponent<RectTransform>();
            if (rootRect != null)
            {
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.offsetMin = Vector2.zero;
                rootRect.offsetMax = Vector2.zero;
            }

            MessageDialog dialog = _cardPickerRoot.GetComponentInChildren<MessageDialog>(true);
            if (dialog == null)
            {
                Destroy(_cardPickerRoot);
                _cardPickerRoot = null;
                return;
            }

            TextMeshProUGUI mainText = GetDialogField<TextMeshProUGUI>(dialog, "mainText");
            TextMeshProUGUI subText = GetDialogField<TextMeshProUGUI>(dialog, "subText");
            Button singleConfirm = GetDialogField<Button>(dialog, "singleConfirmButton");
            Button confirm = GetDialogField<Button>(dialog, "confirmButton");
            Button cancel = GetDialogField<Button>(dialog, "cancelButton");

            RectTransform subTextRect = subText != null ? subText.rectTransform : null;

            if (mainText != null)
            {
                mainText.text = "选择要交易的卡牌";
                mainText.alignment = TextAlignmentOptions.Center;
                mainText.raycastTarget = false;
            }

            if (subText != null)
            {
                // Used for empty/disabled/full messages.
                subText.text = string.Empty;
                subText.raycastTarget = false;
                subText.alignment = TextAlignmentOptions.Center;
                var c = subText.color;
                c.a = 0f;
                subText.color = c;
                subText.gameObject.SetActive(true);
            }

            if (singleConfirm != null)
            {
                singleConfirm.onClick.RemoveAllListeners();
                singleConfirm.gameObject.SetActive(false);
            }
            if (confirm != null)
            {
                confirm.onClick.RemoveAllListeners();
                confirm.gameObject.SetActive(false);
            }
            if (cancel != null)
            {
                cancel.onClick.RemoveAllListeners();
                cancel.gameObject.SetActive(true);

                var cancelLabel = cancel.GetComponentInChildren<TextMeshProUGUI>(true);
                if (cancelLabel != null)
                {
                    cancelLabel.text = "关闭";
                    cancelLabel.alignment = TextAlignmentOptions.Center;
                }

                cancel.onClick.AddListener(() =>
                {
                    try
                    {
                        HideCardPickerOverlay();
                    }
                    catch
                    {
                        // ignored
                    }
                });
            }

            RectTransform panelRect = TryFindCommonAncestorRect(mainText != null ? mainText.rectTransform : null,
                cancel != null ? cancel.GetComponent<RectTransform>() : null);
            panelRect ??= mainText != null ? mainText.rectTransform.parent as RectTransform : null;
            panelRect ??= rootRect;
            if (panelRect == null)
            {
                Destroy(_cardPickerRoot);
                _cardPickerRoot = null;
                return;
            }

            if (!TryAttachHistoryListWithRecordRow(panelRect, subTextRect, out ScrollRect listScrollRect, out RectTransform listContent, out RecordRow rowTemplate))
            {
                Destroy(_cardPickerRoot);
                _cardPickerRoot = null;
                return;
            }

            CardPickerTag tag = listContent.gameObject.AddComponent<CardPickerTag>();
            tag.RecordRowTemplate = rowTemplate;
            tag.ScrollRect = listScrollRect;
            tag.EmptyText = subText;

            if (rowTemplate != null)
            {
                rowTemplate.gameObject.SetActive(false);
            }

            dialog.enabled = false;
        }
        catch
        {
            _cardPickerRoot = null;
        }
    }

    private sealed class CardPickerTag : MonoBehaviour
    {
        public RecordRow RecordRowTemplate;
        public ScrollRect ScrollRect;
        public TextMeshProUGUI EmptyText;
    }

    private void ShowCardPickerOverlay()
    {
        EnsureCardPickerOverlay();
        if (_cardPickerRoot == null)
        {
            TryShowTopMessage("卡牌选择界面不可用。");
            return;
        }

        RebuildCardPickerList();
        _cardPickerRoot.SetActive(true);
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = false;
        }
    }

    private void HideCardPickerOverlay()
    {
        if (_cardPickerRoot != null)
        {
            _cardPickerRoot.SetActive(false);
        }
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = true;
        }
    }

    private void RebuildCardPickerList()
    {
        if (_cardPickerRoot == null)
        {
            return;
        }

        CardPickerTag tag = _cardPickerRoot.GetComponentInChildren<CardPickerTag>(true);
        if (tag == null || tag.RecordRowTemplate == null)
        {
            return;
        }

        Transform container = tag.transform;
        foreach (Transform child in container)
        {
            if (tag.RecordRowTemplate != null && child == tag.RecordRowTemplate.transform)
            {
                continue;
            }
            Destroy(child.gameObject);
        }

        void ShowEmpty(string msg)
        {
            if (tag.ScrollRect != null)
            {
                tag.ScrollRect.gameObject.SetActive(false);
            }

            if (tag.EmptyText != null)
            {
                tag.EmptyText.gameObject.SetActive(true);
                tag.EmptyText.text = msg ?? string.Empty;
                var c = tag.EmptyText.color;
                c.a = 1f;
                tag.EmptyText.color = c;
            }
        }

        void HideEmptyAndShowList()
        {
            if (tag.EmptyText != null)
            {
                tag.EmptyText.text = string.Empty;
                var c = tag.EmptyText.color;
                c.a = 0f;
                tag.EmptyText.color = c;
            }

            if (tag.ScrollRect != null)
            {
                tag.ScrollRect.gameObject.SetActive(true);
            }
        }

        // Only allow editing local offer in network trades.
        if (!CanEditOffer())
        {
            ShowEmpty("当前状态无法编辑报价。");
            return;
        }

        List<Card> deck = new List<Card>();
        try
        {
            if (GameRun?.BaseDeck != null)
            {
                deck = GameRun.BaseDeck
                    .Where(c => c != null)
                    .OrderBy(c => c.Name)
                    .ToList();
            }
        }
        catch
        {
            deck = new List<Card>();
        }

        // Remove cards already offered.
        var offered = new HashSet<int>();
        try
        {
            foreach (var c in _player1OfferedCards)
            {
                if (c != null)
                {
                    offered.Add(c.InstanceId);
                }
            }
        }
        catch
        {
            // ignored
        }

        var candidates = deck.Where(c => c != null && !offered.Contains(c.InstanceId)).ToList();
        if (candidates.Count == 0)
        {
            ShowEmpty("没有可交易的卡牌。");
            return;
        }

        int remaining = Math.Max(0, _maxTradeSlots - (_player1OfferedCards?.Count ?? 0));
        if (remaining <= 0)
        {
            ShowEmpty("卡槽已满。");
            return;
        }

        HideEmptyAndShowList();

        foreach (var card in candidates)
        {
            CreateCardRecordRow(container, tag.RecordRowTemplate, card);
        }
    }

    private void CreateCardRecordRow(Transform parent, RecordRow template, Card card)
    {
        try
        {
            if (template == null || card == null)
            {
                return;
            }

            RecordRow row = Instantiate(template, parent, false);
            row.name = $"Card_{card.InstanceId}";
            row.gameObject.SetActive(true);

            Image avatarImage = GetPrivateFieldValue<Image>(row, "avatarImage");
            TextMeshProUGUI gameResultText = GetPrivateFieldValue<TextMeshProUGUI>(row, "gameResultText");
            TextMeshProUGUI difficultyText = GetPrivateFieldValue<TextMeshProUGUI>(row, "difficultyText");
            TextMeshProUGUI timestampText = GetPrivateFieldValue<TextMeshProUGUI>(row, "timestampText");
            GameObject selectedIndicator = GetPrivateFieldValue<GameObject>(row, "selectedIndicator");
            Image exhibitIcon = GetPrivateFieldValue<Image>(row, "exhibitIcon");

            if (avatarImage != null)
            {
                avatarImage.gameObject.SetActive(false);
            }
            if (exhibitIcon != null)
            {
                exhibitIcon.gameObject.SetActive(false);
            }
            if (selectedIndicator != null)
            {
                selectedIndicator.SetActive(false);
            }

            if (gameResultText != null)
            {
                gameResultText.text = card.Name;
            }
            if (difficultyText != null)
            {
                difficultyText.text = card.Id;
            }
            if (timestampText != null)
            {
                timestampText.text = string.Empty;
            }

            row.SetSelected(false, false);
            row.Click += () =>
            {
                try
                {
                    AddCardToTrade(card, true);
                    RefreshOfferEditorTexts();
                    RebuildCardPickerList();
                }
                catch
                {
                    // ignored
                }
            };
        }
        catch
        {
            // ignored
        }
    }

    // Marker component to locate the list container under the runtime overlay.
    private sealed class PartnerPickerTag : MonoBehaviour
    {
        public RecordRow RecordRowTemplate;
        public TextMeshProUGUI EmptyText;
        public ScrollRect ScrollRect;
        public Button ButtonTemplate;
        public RectTransform ButtonContainer;
        public TextMeshProUGUI TextTemplate;
        public RectTransform ListContainer;
    }

    private sealed class PartnerCandidateTag : MonoBehaviour
    {
        public string PlayerId;
        public string PlayerName;
    }

    private sealed class PartnerPickerClickCatcher : MonoBehaviour, IPointerClickHandler
    {
        public TradePanel Panel;

        private static bool Contains(RectTransform rt, Vector2 screenPoint)
        {
            if (rt == null)
            {
                return false;
            }

            // LBoL UI is typically camera-based, but some prefabs can behave like overlay.
            // Try both camera and null to be resilient.
            try
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPoint, CameraController.UiCamera))
                {
                    return true;
                }
            }
            catch
            {
                // ignored
            }

            return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPoint, null);
        }

        private static bool TryGetLeftClickThisFrame(out Vector2 screenPos)
        {
            screenPos = default;

            // Prefer the new Input System (some builds disable legacy UnityEngine.Input APIs).
            try
            {
                var mouse = Mouse.current;
                if (mouse != null && mouse.leftButton != null && mouse.leftButton.wasPressedThisFrame)
                {
                    screenPos = mouse.position.ReadValue();
                    return true;
                }
            }
            catch
            {
                // ignored
            }

            // Fallback for legacy input.
            try
            {
                if (Input.GetMouseButtonDown(0))
                {
                    screenPos = Input.mousePosition;
                    return true;
                }
            }
            catch
            {
                // ignored
            }

            return false;
        }

        private void Update()
        {
            try
            {
                if (Panel == null || !Panel._partnerPickerActive || Panel._partnerPickerRoot == null)
                {
                    return;
                }

                if (!TryGetLeftClickThisFrame(out Vector2 pos))
                {
                    return;
                }

                // Ignore clicks on cancel.
                if (Panel._partnerPickerCancelButton != null)
                {
                    var cancelRt = Panel._partnerPickerCancelButton.transform as RectTransform;
                    if (Contains(cancelRt, pos))
                    {
                        return;
                    }
                }

                PartnerPickerTag pickerTag = Panel._partnerPickerRoot.GetComponentInChildren<PartnerPickerTag>(true);
                if (pickerTag == null)
                {
                    return;
                }

                RectTransform listRegion = null;
                if (pickerTag.ListContainer != null)
                {
                    listRegion = pickerTag.ListContainer;
                }
                else if (pickerTag.ScrollRect != null)
                {
                    listRegion = pickerTag.ScrollRect.GetComponent<RectTransform>();
                }
                else
                {
                    listRegion = pickerTag.transform as RectTransform;
                }

                // Only handle clicks inside the list region.
                if (listRegion != null && !Contains(listRegion, pos))
                {
                    return;
                }

                Transform container = pickerTag.ListContainer != null ? (Transform)pickerTag.ListContainer : pickerTag.transform;

                if (container == null)
                {
                    return;
                }

                // Hit-test candidates. Iterate in reverse so later siblings win (top-most).
                var candidates = container.GetComponentsInChildren<PartnerCandidateTag>(true);
                for (int i = candidates.Length - 1; i >= 0; i--)
                {
                    var cand = candidates[i];
                    if (cand == null || string.IsNullOrWhiteSpace(cand.PlayerId))
                    {
                        continue;
                    }

                    // Prefer the candidate root rect.
                    var rt = cand.transform as RectTransform;
                    if (Contains(rt, pos))
                    {
                        Panel.OnPartnerSelected(cand.PlayerId, cand.PlayerName);
                        return;
                    }

                    // Fallback: some prefabs have zero-sized roots; hit-test all graphics (TMP/Image/etc.).
                    foreach (var g in cand.GetComponentsInChildren<Graphic>(true))
                    {
                        if (g == null)
                        {
                            continue;
                        }

                        if (Contains(g.rectTransform, pos))
                        {
                            Panel.OnPartnerSelected(cand.PlayerId, cand.PlayerName);
                            return;
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            try
            {
                if (eventData == null || eventData.button != PointerEventData.InputButton.Left)
                {
                    return;
                }

                if (Panel == null || !Panel._partnerPickerActive || Panel._partnerPickerRoot == null)
                {
                    return;
                }

                // Do not interfere with the cancel button.
                try
                {
                    if (Panel._partnerPickerCancelButton != null)
                    {
                        var press = eventData.pointerPress;
                        if (press != null && press.transform != null && press.transform.IsChildOf(Panel._partnerPickerCancelButton.transform))
                        {
                            return;
                        }

                        var hit = eventData.pointerCurrentRaycast.gameObject;
                        if (hit != null && hit.transform != null && hit.transform.IsChildOf(Panel._partnerPickerCancelButton.transform))
                        {
                            return;
                        }
                    }
                }
                catch
                {
                    // ignored
                }

                PartnerPickerTag pickerTag = Panel._partnerPickerRoot.GetComponentInChildren<PartnerPickerTag>(true);
                if (pickerTag == null)
                {
                    return;
                }

                Transform container = pickerTag.ListContainer != null ? (Transform)pickerTag.ListContainer : pickerTag.transform;

                if (container == null)
                {
                    return;
                }

                // Resolve the clicked candidate purely by rectangle hit testing.
                var candidates = container.GetComponentsInChildren<PartnerCandidateTag>(true);
                foreach (var cand in candidates)
                {
                    if (cand == null || string.IsNullOrWhiteSpace(cand.PlayerId))
                    {
                        continue;
                    }

                    var rt = cand.transform as RectTransform;
                    if (rt == null || !rt.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    Camera cam;
                    try
                    {
                        cam = eventData.pressEventCamera ?? CameraController.UiCamera;
                    }
                    catch
                    {
                        cam = null;
                    }

                    if (RectTransformUtility.RectangleContainsScreenPoint(rt, eventData.position, cam)
                        || RectTransformUtility.RectangleContainsScreenPoint(rt, eventData.position, null))
                    {
                        Panel.OnPartnerSelected(cand.PlayerId, cand.PlayerName);
                        return;
                    }

                    // Fallback: test candidate graphics too.
                    foreach (var g in cand.GetComponentsInChildren<Graphic>(true))
                    {
                        if (g == null)
                        {
                            continue;
                        }

                        var grt = g.rectTransform;
                        if (grt == null)
                        {
                            continue;
                        }

                        if (RectTransformUtility.RectangleContainsScreenPoint(grt, eventData.position, cam)
                            || RectTransformUtility.RectangleContainsScreenPoint(grt, eventData.position, null))
                        {
                            Panel.OnPartnerSelected(cand.PlayerId, cand.PlayerName);
                            return;
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }
        }
    }

    private bool TryIsNetworkTrade(out bool localIsA)
    {
        localIsA = false;
        try
        {
            // In local debug mode, suppress network trade semantics so UI can be interacted with freely.
            if (_localDebugTradeMode)
            {
                return false;
            }

            INetworkClient client = ModService.ServiceProvider.GetService<INetworkClient>();
            if (client == null || !client.IsConnected)
            {
                return false;
            }

            localIsA = string.Equals(_selfPlayerId, _playerAId, StringComparison.Ordinal);
            return !string.IsNullOrWhiteSpace(_tradeId) && !string.IsNullOrWhiteSpace(_selfPlayerId);
        }
        catch
        {
            return false;
        }
    }

    private void TrySubscribeTradeEvents()
    {
        if (_subscribedToTrade)
        {
            return;
        }

        TradeSyncPatch.OnTradeStateUpdated += OnTradeStateUpdated;
        _subscribedToTrade = true;
    }

    private void TryUnsubscribeTradeEvents()
    {
        if (!_subscribedToTrade)
        {
            return;
        }

        TradeSyncPatch.OnTradeStateUpdated -= OnTradeStateUpdated;
        _subscribedToTrade = false;
    }

    private void OnTradeStateUpdated(TradeSyncPatch.TradeSessionState state)
    {
        try
        {
            if (state == null || !string.Equals(state.TradeId, _tradeId, StringComparison.Ordinal))
            {
                return;
            }

            // 只关心参与者。
            if (!state.IsParticipant(_selfPlayerId))
            {
                return;
            }

            ApplyStateToUi(state);

            // Preparing: run strict local precheck and report once.
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
                // If host reverted to Open after prepare failure, show reason and allow retry.
                if (_lastTradeStatus == TradeSyncPatch.TradeStatus.Preparing && !string.IsNullOrWhiteSpace(state.Reason))
                {
                    UpdateUIStatus($"Prepare failed: {state.Reason}");
                }
            }

            _lastTradeStatus = state.Status;
        }
        catch
        {
            // ignored
        }
    }

    private void ApplyStateToUi(TradeSyncPatch.TradeSessionState state)
    {
        // 以 Host 广播状态为准刷新 UI。
        using (new ApplyingStateScope(this))
        {
            bool localIsA = string.Equals(state.PlayerAId, _selfPlayerId, StringComparison.Ordinal);

            _playerAId = state.PlayerAId;
            _playerBId = state.PlayerBId;

            // 清空现有 UI
            ResetTradeData();
            _tradeId = state.TradeId;
            _selfPlayerId = NetworkIdentityTracker.GetSelfPlayerId();
            _playerAId = state.PlayerAId;
            _playerBId = state.PlayerBId;

            // Pull local money/exhibit offers from host state so UI stays consistent.
            _localMoneyOffer = localIsA ? state.MoneyA : state.MoneyB;
            _localExhibitOfferIds.Clear();
            foreach (var ex in localIsA ? state.ExhibitsA : state.ExhibitsB)
            {
                if (ex != null && !string.IsNullOrWhiteSpace(ex.ExhibitId))
                {
                    _localExhibitOfferIds.Add(ex.ExhibitId);
                }
            }
            RefreshOfferEditorTexts();

            // 本地报价：显示在 player1
            foreach (var c in localIsA ? state.OfferA : state.OfferB)
            {
                Card real = TryFindDeckCard(c);
                if (real != null)
                {
                    AddCardToTrade(real, true);
                }
            }

            // 远端报价：显示在 player2（临时卡用于展示）
            foreach (var c in localIsA ? state.OfferB : state.OfferA)
            {
                Card temp = TryCreateDisplayCard(c);
                if (temp != null)
                {
                    AddCardToTrade(temp, false);
                }
            }

            // 锁住远端槽位，避免误删
            LockSlots(player2Slots);
        }

        // 只有 Host 状态为 Open 时允许继续交互
        if (confirmButton?.button != null)
        {
            bool localIsA = string.Equals(state.PlayerAId, _selfPlayerId, StringComparison.Ordinal);
            bool localHasOffer = (localIsA ? (state.OfferA?.Count ?? 0) : (state.OfferB?.Count ?? 0)) > 0
                               || (localIsA ? state.MoneyA : state.MoneyB) > 0
                               || (localIsA ? (state.ExhibitsA?.Count ?? 0) : (state.ExhibitsB?.Count ?? 0)) > 0;
            bool remoteHasOffer = (localIsA ? (state.OfferB?.Count ?? 0) : (state.OfferA?.Count ?? 0)) > 0
                                || (localIsA ? state.MoneyB : state.MoneyA) > 0
                                || (localIsA ? (state.ExhibitsB?.Count ?? 0) : (state.ExhibitsA?.Count ?? 0)) > 0;

            // User choice: never grey out the confirm button. Guard logic happens on click.
            confirmButton.button.interactable = true;
        }

        if (state.Status == TradeSyncPatch.TradeStatus.Open)
        {
            UpdateUIStatus((state.OfferA?.Count ?? 0) > 0 && (state.OfferB?.Count ?? 0) > 0
                ? "Trade.ReadyToConfirm".Localize()
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

    private void LockSlots(TradeSlotWidget[] slots)
    {
        if (slots == null)
        {
            return;
        }

        foreach (var s in slots)
        {
            s?.SetLocked(true);
        }
    }

    private Card TryFindDeckCard(TradeSyncPatch.CardRef cardRef)
    {
        try
        {
            if (cardRef == null || cardRef.InstanceId < 0)
            {
                return null;
            }

            return GameRun?.GetDeckCardByInstanceId(cardRef.InstanceId);
        }
        catch
        {
            return null;
        }
    }

    private Card TryCreateDisplayCard(TradeSyncPatch.CardRef cardRef)
    {
        try
        {
            if (cardRef == null || string.IsNullOrWhiteSpace(cardRef.CardId))
            {
                return null;
            }

            // 展示用临时卡：用 TryCreateCard 保证名称正确。
            Card created = Library.TryCreateCard(cardRef.CardId, cardRef.IsUpgraded, cardRef.UpgradeCounter);
            return created;
        }
        catch
        {
            return null;
        }
    }

    private void TrySendOfferUpdate()
    {
        if (!TryIsNetworkTrade(out bool localIsA))
        {
            return;
        }

        // 只发送本地侧(player1)报价。
        List<Card> offered = _player1OfferedCards;
        var refs = offered
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

        TradeSyncPatch.RequestOfferUpdate(_tradeId, _selfPlayerId, refs, _localMoneyOffer, _localExhibitOfferIds.ToList());
    }

    private void TrySendConfirm()
    {
        if (!TryIsNetworkTrade(out _))
        {
            return;
        }

        TradeSyncPatch.RequestConfirm(_tradeId, _selfPlayerId);
    }

    private void TrySendCancel()
    {
        if (!TryIsNetworkTrade(out _))
        {
            return;
        }

        TradeSyncPatch.RequestCancel(_tradeId, _selfPlayerId);
    }

    private IEnumerator ApplyNetworkTradeAndClose(bool localIsA)
    {
        TradeSyncPatch.TradeSessionState state = TradeSyncPatch.GetLastKnown(_tradeId);
        if (state == null || state.Status != TradeSyncPatch.TradeStatus.Completed)
        {
            yield break;
        }

        // 禁用交互
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = false;
        }

        using (TradeSyncPatch.EnterApplyingTradeScope())
        {
            // 自己移除自己报价，添加对方报价。
            List<TradeSyncPatch.CardRef> mine = localIsA ? state.OfferA : state.OfferB;
            List<TradeSyncPatch.CardRef> theirs = localIsA ? state.OfferB : state.OfferA;

            int myMoney = localIsA ? state.MoneyA : state.MoneyB;
            int theirMoney = localIsA ? state.MoneyB : state.MoneyA;

            List<TradeSyncPatch.ExhibitRef> myExhibits = localIsA ? state.ExhibitsA : state.ExhibitsB;
            List<TradeSyncPatch.ExhibitRef> theirExhibits = localIsA ? state.ExhibitsB : state.ExhibitsA;

            if (mine != null)
            {
                foreach (var c in mine)
                {
                    if (c == null)
                    {
                        continue;
                    }

                    Card deck = TryFindDeckCard(c);
                    if (deck == null)
                    {
                        UpdateUIStatus("Trade failed: missing offered card.");
                        yield break;
                    }

                    GameRun.RemoveDeckCard(deck, false);
                }
            }

            // Money: strict (insufficient => fail).
            try
            {
                if (myMoney > 0)
                {
                    GameRun.ConsumeMoney(myMoney);
                }
            }
            catch
            {
                UpdateUIStatus("Trade failed: insufficient money.");
                yield break;
            }

            if (theirMoney > 0)
            {
                GameRun.GainMoney(theirMoney, true, new VisualSourceData { SourceType = VisualSourceType.CardSelect });
            }

            // Exhibits: strict (not found/not losable/blacklisted/dup => fail).
            if (myExhibits != null)
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
                        owned = GameRun?.Player?.Exhibits?.FirstOrDefault(e => e != null && string.Equals(e.Id, id, StringComparison.Ordinal));
                    }
                    catch
                    {
                        owned = null;
                    }

                    if (owned == null)
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
                        GameRun.LoseExhibit(owned, true, true);
                    }
                    catch
                    {
                        UpdateUIStatus("Trade failed: cannot lose exhibit.");
                        yield break;
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

                    if (created == null)
                    {
                        UpdateUIStatus("Trade failed: cannot create received exhibit.");
                        yield break;
                    }

                    try
                    {
                        GameRun.GainExhibitInstantly(created, true, new VisualSourceData { SourceType = VisualSourceType.CardSelect });
                    }
                    catch
                    {
                        UpdateUIStatus("Trade failed: cannot gain received exhibit.");
                        yield break;
                    }
                }
            }

            if (theirs != null)
            {
                foreach (var c in theirs)
                {
                    if (c == null || string.IsNullOrWhiteSpace(c.CardId))
                    {
                        continue;
                    }

                    Card created = Library.TryCreateCard(c.CardId, c.IsUpgraded, c.UpgradeCounter);
                    if (created != null)
                    {
                        GameRun.AddDeckCard(created, true, new VisualSourceData
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
        if (state == null)
        {
            return;
        }

        // Avoid sending multiple results for the same preparing phase.
        if (state.Timestamp > 0 && _lastPreparingHandledTimestamp == state.Timestamp)
        {
            return;
        }

        _lastPreparingHandledTimestamp = state.Timestamp;

        bool localIsA = string.Equals(state.PlayerAId, _selfPlayerId, StringComparison.Ordinal);

        // Validate only our own offer strictly.
        List<TradeSyncPatch.CardRef> mine = localIsA ? state.OfferA : state.OfferB;
        int myMoney = localIsA ? state.MoneyA : state.MoneyB;
        List<TradeSyncPatch.ExhibitRef> myExhibits = localIsA ? state.ExhibitsA : state.ExhibitsB;

        if ((mine?.Count ?? 0) == 0 && myMoney <= 0 && (myExhibits?.Count ?? 0) == 0)
        {
            TradeSyncPatch.RequestPrepareResult(_tradeId, _selfPlayerId, false, "EmptyOffer");
            return;
        }

        // Cards must exist by instance-id.
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
                    TradeSyncPatch.RequestPrepareResult(_tradeId, _selfPlayerId, false, "InvalidInstanceId");
                    return;
                }

                if (TryFindDeckCard(c) == null)
                {
                    TradeSyncPatch.RequestPrepareResult(_tradeId, _selfPlayerId, false, "MissingCard");
                    return;
                }
            }
        }

        // Money must be affordable.
        try
        {
            int current = GameRun?.Money ?? 0;
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

        // Exhibits must exist and be tradable.
        if (myExhibits != null)
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
                    owned = GameRun?.Player?.Exhibits?.FirstOrDefault(e => e != null && string.Equals(e.Id, id, StringComparison.Ordinal));
                }
                catch
                {
                    owned = null;
                }

                if (owned == null)
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

    private void EnsureOfferEditorOverlay()
    {
        if (_offerEditorRoot != null)
        {
            return;
        }

        try
        {
            _offerEditorRoot = new GameObject("TradeOfferEditor");
            _offerEditorRoot.transform.SetParent(transform, false);

            RectTransform rootRect = _offerEditorRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.02f, 0.02f);
            rootRect.anchorMax = new Vector2(0.48f, 0.18f);
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // Background removed per user request.

            if (!TryPickOfferEditorTemplates(out TextMeshProUGUI textTemplate, out CommonButtonWidget buttonTemplate))
            {
                try { Destroy(_offerEditorRoot); } catch { }
                _offerEditorRoot = null;
                return;
            }

            // Cards row
            var cardsLabel = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            cardsLabel.name = "CardsLabel";
            cardsLabel.text = "卡牌:";
            cardsLabel.alignment = TextAlignmentOptions.Center;
            SetRect(cardsLabel.rectTransform, 0.02f, 0.70f, 0.25f, 0.95f);

            _cardCountText = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            _cardCountText.name = "CardsValue";
            _cardCountText.text = "0";
            _cardCountText.alignment = TextAlignmentOptions.Left;
            SetRect(_cardCountText.rectTransform, 0.25f, 0.70f, 0.50f, 0.95f);

            // Money row
            var moneyLabel = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            moneyLabel.name = "MoneyLabel";
            moneyLabel.text = "金币:";
            moneyLabel.alignment = TextAlignmentOptions.Center;
            SetRect(moneyLabel.rectTransform, 0.02f, 0.38f, 0.25f, 0.63f);

            // Owned money value (left-bottom list): should reflect the player's current money (top bar).
            _ownedMoneyText = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            _ownedMoneyText.name = "OwnedMoneyValue";
            _ownedMoneyText.text = "0";
            _ownedMoneyText.alignment = TextAlignmentOptions.Left;
            _ownedMoneyText.raycastTarget = false;
            SetRect(_ownedMoneyText.rectTransform, 0.25f, 0.38f, 0.50f, 0.63f);

            // Money triplet: keep '-' and '+' equally spaced around the number.
            // Requirement: when number becomes 1/2/3 digits, spacing stays equal and the triplet stays centered.
            _moneyTripletRoot = new GameObject("MoneyTriplet");
            _moneyTripletRoot.transform.SetParent(_offerEditorRoot.transform, false);
            var moneyTripletRect = _moneyTripletRoot.AddComponent<RectTransform>();
            SetRect(moneyTripletRect, 0.52f, 0.40f, 0.82f, 0.61f);

            _moneyTripletLayout = _moneyTripletRoot.AddComponent<HorizontalLayoutGroup>();
            _moneyTripletLayout.childAlignment = TextAnchor.MiddleCenter;
            _moneyTripletLayout.spacing = 12f;
            _moneyTripletLayout.childControlWidth = true;
            _moneyTripletLayout.childControlHeight = true;
            _moneyTripletLayout.childForceExpandWidth = false;
            _moneyTripletLayout.childForceExpandHeight = false;

            // Money +/-: clickable text (no background).
            var minusBtn = CreateTextButton(textTemplate, _moneyTripletRoot.transform, "MoneyMinus", "-");
            minusBtn.onClick.RemoveAllListeners();
            minusBtn.onClick.AddListener(() =>
            {
                if (!CanEditOffer()) return;
                _localMoneyOffer = Math.Max(0, _localMoneyOffer - 1);
                RefreshOfferEditorTexts();
                TrySendOfferUpdate();
            });

            _moneyValueText = Instantiate(textTemplate, _moneyTripletRoot.transform, false);
            _moneyValueText.name = "MoneyValue";
            _moneyValueText.text = "0";
            _moneyValueText.alignment = TextAlignmentOptions.Center;
            _moneyValueText.raycastTarget = false; // number is not clickable
            _moneyValueText.enableWordWrapping = false;
            _moneyValueText.overflowMode = TextOverflowModes.Overflow;
            try { _moneyValueBaseFontSize = _moneyValueText.fontSize; } catch { _moneyValueBaseFontSize = 0f; }

            var plusBtn = CreateTextButton(textTemplate, _moneyTripletRoot.transform, "MoneyPlus", "+");
            plusBtn.onClick.RemoveAllListeners();
            plusBtn.onClick.AddListener(() =>
            {
                if (!CanEditOffer()) return;

                // Clamp to owned money when available; if we cannot resolve owned money reliably,
                // allow increasing so the UI remains usable (final confirm still validates affordability).
                int owned;
                bool hasOwned = TryGetOwnedMoney(out owned);
                int next = _localMoneyOffer + 1;
                if (hasOwned)
                {
                    next = Math.Min(next, owned);
                }
                next = Math.Min(next, MaxMoneyOffer);
                _localMoneyOffer = next;
                RefreshOfferEditorTexts();
                TrySendOfferUpdate();
            });

            // Exhibit row
            var exLabel = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            exLabel.name = "ExLabel";
            exLabel.text = "展品:";
            exLabel.alignment = TextAlignmentOptions.Center;
            SetRect(exLabel.rectTransform, 0.02f, 0.05f, 0.25f, 0.30f);

            _exhibitValueText = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            _exhibitValueText.name = "ExValue";
            _exhibitValueText.text = "0";
            _exhibitValueText.alignment = TextAlignmentOptions.Left;
            SetRect(_exhibitValueText.rectTransform, 0.25f, 0.05f, 0.50f, 0.30f);

            // Final cleanup: aggressively remove any accidental "cancel"/extra button objects brought
            // in by the button template hierarchy. Keep only our known widgets.
            PruneOfferEditorExtraButtons(_offerEditorRoot);

            EnsureOfferActionsOverlay(textTemplate);

            // Keep offer editor above the frame visuals so its text doesn't get covered by dialog masks.
            // It doesn't overlap the bottom confirm/cancel buttons in the runtime layout.
            try { _offerEditorRoot.transform.SetAsLastSibling(); } catch { }

            RefreshOfferEditorTexts();
        }
        catch
        {
            try
            {
                if (_offerEditorRoot != null)
                {
                    Destroy(_offerEditorRoot);
                }
            }
            catch
            {
                // ignored
            }
            _offerEditorRoot = null;
        }
    }

    private static void PruneOfferEditorExtraButtons(GameObject offerRoot)
    {
        if (offerRoot == null)
        {
            return;
        }

        try
        {
            // Keep the offer editor stable by preserving the full subtree of the controls we created.
            // Deleting individual Button objects can accidentally remove parts of the widgets (especially
            // when templates include multiple nested buttons/images).
            var keepRoots = new HashSet<string>(StringComparer.Ordinal)
            {
                "CardsLabel",
                "CardsValue",
                "MoneyLabel",
                "OwnedMoneyValue",
                "MoneyTriplet",
                "ExLabel",
                "ExValue"
            };

            // Remove any direct child under offerRoot that isn't ours.
            // (We only ever create direct children; template internals remain under the kept roots.)
            var children = new List<Transform>();
            foreach (Transform child in offerRoot.transform)
            {
                if (child != null)
                {
                    children.Add(child);
                }
            }

            foreach (var child in children)
            {
                if (child == null)
                {
                    continue;
                }

                var n = child.name ?? string.Empty;
                if (!keepRoots.Contains(n))
                {
                    try { Destroy(child.gameObject); } catch { }
                }
            }

            // Safety: within kept subtrees, remove any obvious cancel/close named objects.
            // This avoids red rings/icons embedded in certain templates.
            foreach (var t in offerRoot.GetComponentsInChildren<Transform>(true))
            {
                if (t == null || ReferenceEquals(t.gameObject, offerRoot))
                {
                    continue;
                }

                var n = t.name ?? string.Empty;
                if (n.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    n.IndexOf("close", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Only remove if this isn't one of our root nodes.
                    if (!keepRoots.Contains(n))
                    {
                        try { Destroy(t.gameObject); } catch { }
                    }
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private void ApplyMoneyValueSizingForDigits()
    {
        if (_moneyValueText == null)
        {
            return;
        }

        try
        {
            string s = _moneyValueText.text ?? string.Empty;
            int digits = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                if (ch >= '0' && ch <= '9')
                {
                    digits++;
                }
            }

            float baseSize = _moneyValueBaseFontSize > 0f ? _moneyValueBaseFontSize : _moneyValueText.fontSize;
            if (digits >= 4)
            {
                // User choice: shrink only the number when digits >= 4, keep min at 60%.
                float scale = Mathf.Clamp(3f / digits, 0.6f, 1f);
                _moneyValueText.fontSize = baseSize * scale;
            }
            else
            {
                _moneyValueText.fontSize = baseSize;
            }

            // Rebuild layout so '-' and '+' stay equally spaced around the number.
            if (_moneyTripletRoot != null)
            {
                try { LayoutRebuilder.ForceRebuildLayoutImmediate(_moneyTripletRoot.transform as RectTransform); } catch { }
            }
        }
        catch
        {
            // ignored
        }
    }

    private bool TryGetOwnedMoney(out int ownedMoney)
    {
        ownedMoney = 0;

        try
        {
            int best = 0;

            // Primary: GameRun.Money (used elsewhere in this panel).
            try { best = Math.Max(best, GameRun?.Money ?? 0); } catch { }

            // Fallback 1: Player might expose a money-like property.
            try
            {
                var p = GameRun?.Player;
                if (p != null)
                {
                    var t = p.GetType();
                    var prop = t.GetProperty("Money", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (prop != null && prop.PropertyType == typeof(int))
                    {
                        best = Math.Max(best, (int)prop.GetValue(p));
                    }
                    var field = t.GetField("Money", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field != null && field.FieldType == typeof(int))
                    {
                        best = Math.Max(best, (int)field.GetValue(p));
                    }
                }
            }
            catch
            {
                // ignored
            }

            // Fallback 2: GameMaster.CurrentGameRun (some UI contexts use this).
            try
            {
                var gm = GameMaster.Instance;
                if (gm != null)
                {
                    var t = gm.GetType();
                    var prop = t.GetProperty("CurrentGameRun", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var run = prop != null ? prop.GetValue(gm) : null;
                    if (run != null)
                    {
                        var rt = run.GetType();
                        var moneyProp = rt.GetProperty("Money", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (moneyProp != null && moneyProp.PropertyType == typeof(int))
                        {
                            best = Math.Max(best, (int)moneyProp.GetValue(run));
                        }
                        var moneyField = rt.GetField("Money", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (moneyField != null && moneyField.FieldType == typeof(int))
                        {
                            best = Math.Max(best, (int)moneyField.GetValue(run));
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            if (best < 0)
            {
                best = 0;
            }

            ownedMoney = best;
            // Treat 0 as a valid value only if we at least had a known run context.
            return (GameRun != null) || ownedMoney > 0;
        }
        catch
        {
            ownedMoney = 0;
            return false;
        }
    }

    private void EnsureOfferActionsOverlay(TextMeshProUGUI textTemplate)
    {
        if (_offerActionsRoot != null)
        {
            return;
        }

        try
        {
            _offerActionsRoot = new GameObject("TradeOfferActions");
            _offerActionsRoot.transform.SetParent(transform, false);

            var rt = _offerActionsRoot.AddComponent<RectTransform>();

            // Bottom-right (green box area). User wants it aligned with the details column and
            // consistent with the existing offer editor spacing.
            rt.anchorMin = new Vector2(0.80f, 0.08f);
            rt.anchorMax = new Vector2(0.98f, 0.20f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _offerActionsPickCardsBtn = CreateTextButton(textTemplate, _offerActionsRoot.transform, "PickCards", "选择卡牌");
            var cardsRt = _offerActionsPickCardsBtn.GetComponent<RectTransform>();
            SetRect(cardsRt, 0f, 0.45f, 1f, 1f); // 55%/45%
            try
            {
                var tmp = _offerActionsPickCardsBtn.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.alignment = TextAlignmentOptions.Left;
                }
            }
            catch { }

            _offerActionsPickCardsBtn.onClick.RemoveAllListeners();
            _offerActionsPickCardsBtn.onClick.AddListener(() =>
            {
                if (!CanEditOffer())
                {
                    UpdateUIStatus(TryLocalize("Trade.WaitingForItems", "请先开始交易/等待交易开启"));
                    return;
                }

                ShowCardPickerOverlay();
            });

            _offerActionsPickExhibitsBtn = CreateTextButton(textTemplate, _offerActionsRoot.transform, "PickExhibits", "选择展品");
            var exRt = _offerActionsPickExhibitsBtn.GetComponent<RectTransform>();
            SetRect(exRt, 0f, 0f, 1f, 0.45f);
            try
            {
                var tmp = _offerActionsPickExhibitsBtn.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.alignment = TextAlignmentOptions.Left;
                }
            }
            catch { }

            _offerActionsPickExhibitsBtn.onClick.RemoveAllListeners();
            _offerActionsPickExhibitsBtn.onClick.AddListener(() =>
            {
                if (!CanEditOffer())
                {
                    UpdateUIStatus(TryLocalize("Trade.WaitingForItems", "请先开始交易/等待交易开启"));
                    return;
                }

                ShowExhibitPickerOverlay();
            });

            // Follow the offer editor visibility when created.
            try { _offerActionsRoot.SetActive(_offerEditorRoot != null && _offerEditorRoot.activeSelf); } catch { }
        }
        catch
        {
            try
            {
                if (_offerActionsRoot != null)
                {
                    Destroy(_offerActionsRoot);
                }
            }
            catch
            {
                // ignored
            }

            _offerActionsRoot = null;
            _offerActionsPickCardsBtn = null;
            _offerActionsPickExhibitsBtn = null;
        }
    }

    private sealed class TextButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public float HoverScale = 1.08f;
        public float PressedScale = 1.02f;

        private RectTransform _rt;
        private bool _hovering;
        private bool _pressed;

        private void Awake()
        {
            _rt = transform as RectTransform;
            if (_rt != null)
            {
                _rt.localScale = Vector3.one;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;
            ApplyScale();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            _pressed = false;
            ApplyScale();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
            ApplyScale();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pressed = false;
            ApplyScale();
        }

        private void ApplyScale()
        {
            if (_rt == null)
            {
                return;
            }

            float s = 1f;
            if (_pressed)
            {
                s = PressedScale;
            }
            else if (_hovering)
            {
                s = HoverScale;
            }

            _rt.localScale = new Vector3(s, s, 1f);
        }
    }

    private static Button CreateTextButton(TextMeshProUGUI template, Transform parent, string name, string text, float fontSize = -1f)
    {
        // Clone TMP from an in-game template so font/material matches vanilla.
        var tmp = Instantiate(template, parent, false);
        tmp.name = name;
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = true;
        if (fontSize > 0f)
        {
            tmp.enableAutoSizing = false;
            tmp.fontSize = fontSize;
            tmp.fontSizeMin = 1f;
            tmp.fontSizeMax = fontSize;
        }
        try
        {
            var c = tmp.color;
            c.a = 1f;
            tmp.color = c;
        }
        catch
        {
            // ignored
        }

        var btn = tmp.gameObject.AddComponent<Button>();
        btn.targetGraphic = tmp;
        btn.transition = Selectable.Transition.ColorTint;

        // Use a bright hover like the in-game "clickable text" affordance.
        try
        {
            Color baseColor = tmp.color;
            Color hover = new Color(
                Mathf.Clamp01(baseColor.r * 1.15f),
                Mathf.Clamp01(baseColor.g * 1.15f),
                Mathf.Clamp01(baseColor.b * 1.15f),
                baseColor.a);
            Color pressed = new Color(
                Mathf.Clamp01(baseColor.r * 0.95f),
                Mathf.Clamp01(baseColor.g * 0.95f),
                Mathf.Clamp01(baseColor.b * 0.95f),
                baseColor.a);
            Color disabled = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * 0.35f);

            var colors = btn.colors;
            colors.normalColor = baseColor;
            colors.highlightedColor = hover;
            colors.selectedColor = hover;
            colors.pressedColor = pressed;
            colors.disabledColor = disabled;
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
        }
        catch
        {
            // ignored
        }

        try
        {
            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;
        }
        catch
        {
            // ignored
        }

        _ = tmp.gameObject.AddComponent<TextButtonHover>();
        return btn;
    }

    private bool TryPickOfferEditorTemplates(out TextMeshProUGUI textTemplate, out CommonButtonWidget buttonTemplate)
    {
        textTemplate = null;
        buttonTemplate = null;

        try
        {
            // Strong preference: reuse vanilla dialog prefab templates.
            try
            {
                GameObject dialogPrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
                if (dialogPrefab != null)
                {
                    // Prefer the dialog's own text fields so TMP font/material matches vanilla.
                    var dialog = dialogPrefab.GetComponentInChildren<MessageDialog>(true);
                    if (dialog != null)
                    {
                        textTemplate = GetDialogField<TextMeshProUGUI>(dialog, "mainText")
                                       ?? GetDialogField<TextMeshProUGUI>(dialog, "subText")
                                       ?? dialogPrefab.GetComponentInChildren<TextMeshProUGUI>(true);

                        // Prioritize singleConfirmButton (usually gold/wood confirm) or confirmButton.
                        // Only use cancelButton as an absolute last resort.
                        Button targetButton = GetDialogField<Button>(dialog, "singleConfirmButton")
                                           ?? GetDialogField<Button>(dialog, "confirmButton")
                                           ?? GetDialogField<Button>(dialog, "cancelButton");

                        buttonTemplate = TryResolveCommonButtonWidget(targetButton);
                    }
                    else
                    {
                        textTemplate = dialogPrefab.GetComponentInChildren<TextMeshProUGUI>(true);
                    }

                    // Fallback to heuristic if specific fields are not found.
                    if (buttonTemplate == null)
                    {
                        CommonButtonWidget best = null;
                        int bestScore = int.MaxValue;
                        var widgets = dialogPrefab.GetComponentsInChildren<CommonButtonWidget>(true);
                        foreach (var w in widgets)
                        {
                            if (w == null || w.button == null)
                            {
                                continue;
                            }

                            int buttons = 0;
                            int nodes = 0;
                            try { buttons = w.GetComponentsInChildren<Button>(true)?.Length ?? 0; } catch { buttons = 0; }
                            try { nodes = w.GetComponentsInChildren<Transform>(true)?.Length ?? 0; } catch { nodes = 0; }

                            if (buttons <= 0)
                            {
                                continue;
                            }

                            int score = (buttons * 1000) + nodes;
                            
                            // Highly prefer confirm/singleConfirm buttons visually.
                            if (w.name.IndexOf("confirm", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                score -= 500;
                            }
                            // Strictly avoid anything named "cancel" for these choices.
                            if (w.name.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                score += 2000;
                            }

                            if (score < bestScore)
                            {
                                bestScore = score;
                                best = w;
                            }
                        }
                        buttonTemplate = best;
                    }

                    // Final guard: for the offer editor, we want a *single* button widget.
                    // If the picked template contains multiple Buttons, try to find a better child widget.
                    buttonTemplate = PreferSingleButtonWidget(buttonTemplate);
                }
            }
            catch
            {
                // ignored
            }

            // Fallback: use existing in-panel references (still game UI, but less strict).
            // Prefer confirmButton over cancelButton for offer editor actions.
            textTemplate ??= statusText != null ? statusText : player1NameText;
            buttonTemplate ??= confirmButton != null ? confirmButton : cancelButton;

            buttonTemplate = PreferSingleButtonWidget(buttonTemplate);

            return textTemplate != null && buttonTemplate != null;
        }
        catch
        {
            textTemplate = null;
            buttonTemplate = null;
            return false;
        }
    }

    private static void NormalizeButtonWidget(CommonButtonWidget widget)
    {
        try
        {
            if (widget == null)
            {
                return;
            }

            // Ensure only the primary button remains interactive.
            Button keep = widget.button;
            var buttons = widget.GetComponentsInChildren<Button>(true);
            if (buttons != null && buttons.Length > 1)
            {
                foreach (var b in buttons)
                {
                    if (b == null || b == keep)
                    {
                        continue;
                    }

                    // Prefer hard removal: extra buttons from templates should not exist on our
                    // runtime widgets (they can render as red cancel circles and confuse input).
                    try
                    {
                        if (b.gameObject != null)
                        {
                            Destroy(b.gameObject);
                        }
                        else
                        {
                            Destroy(b);
                        }
                        continue;
                    }
                    catch
                    {
                        // Fallback: if we can't destroy (rare), at least disable & hide.
                        try { b.enabled = false; } catch { }
                        try { b.interactable = false; } catch { }
                        try { b.gameObject.SetActive(false); } catch { }
                    }
                }
            }

            // Avoid popup tooltips / extra pointer handlers from templates.
            DisableTooltipBehaviours(widget.gameObject);

            // Avoid gamepad cursor / extra visual affordances that some templates include.
            DisableCursorBehaviours(widget.gameObject);

            // Force behavior to "Open" (0, usually non-red) for these functional buttons.
            try
            {
                var traverse = HarmonyLib.Traverse.Create(widget);
                traverse.Field("buttonBehavior").SetValue(0);
            }
            catch
            {
                // ignored
            }
        }
        catch
        {
            // ignored
        }
    }

    private static CommonButtonWidget TryResolveCommonButtonWidget(Button target)
    {
        try
        {
            if (target == null)
            {
                return null;
            }

            // Prefer the closest widget that explicitly references this Button.
            var widgets = target.GetComponentsInParent<CommonButtonWidget>(true);
            if (widgets != null)
            {
                foreach (var w in widgets)
                {
                    if (w == null)
                    {
                        continue;
                    }

                    if (ReferenceEquals(w.button, target))
                    {
                        return w;
                    }
                }
            }

            return target.GetComponentInParent<CommonButtonWidget>();
        }
        catch
        {
            return null;
        }
    }

    private static CommonButtonWidget PreferSingleButtonWidget(CommonButtonWidget template)
    {
        try
        {
            if (template == null)
            {
                return null;
            }

            int buttons = 0;
            try { buttons = template.GetComponentsInChildren<Button>(true)?.Length ?? 0; } catch { buttons = 0; }
            if (buttons <= 1)
            {
                return template;
            }

            CommonButtonWidget best = null;
            int bestNodes = int.MaxValue;
            foreach (var w in template.GetComponentsInChildren<CommonButtonWidget>(true))
            {
                if (w == null || w.button == null)
                {
                    continue;
                }

                // Avoid any cancel-labeled objects.
                if (!string.IsNullOrWhiteSpace(w.name) && w.name.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                int wButtons = 0;
                try { wButtons = w.GetComponentsInChildren<Button>(true)?.Length ?? 0; } catch { wButtons = 0; }
                if (wButtons != 1)
                {
                    continue;
                }

                int nodes = 0;
                try { nodes = w.GetComponentsInChildren<Transform>(true)?.Length ?? 0; } catch { nodes = 0; }
                if (nodes < bestNodes)
                {
                    bestNodes = nodes;
                    best = w;
                }
            }

            return best ?? template;
        }
        catch
        {
            return template;
        }
    }

    private static void DisableCursorBehaviours(GameObject root)
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

                // We intentionally use name matching to avoid hard references to optional assemblies.
                var n = behaviour.GetType().Name;
                if (string.IsNullOrWhiteSpace(n))
                {
                    continue;
                }

                if (n.IndexOf("Gamepad", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    n.IndexOf("Cursor", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Do not disable the main Button itself.
                    if (behaviour is Button)
                    {
                        continue;
                    }

                    try { behaviour.enabled = false; } catch { }
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void DisableTooltipBehaviours(GameObject root)
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
                if (!string.IsNullOrWhiteSpace(n) && n.IndexOf("Tooltip", StringComparison.OrdinalIgnoreCase) >= 0)
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

    private bool CanEditOffer()
    {
        try
        {
            if (_isApplyingState || _partnerPickerActive)
            {
                return false;
            }

            // Local debug session should always be editable.
            if (_localDebugTradeMode)
            {
                return true;
            }

            TradeSyncPatch.TradeSessionState state = TradeSyncPatch.GetLastKnown(_tradeId);
            return state == null || state.Status == TradeSyncPatch.TradeStatus.Open;
        }
        catch
        {
            return false;
        }
    }

    private void RefreshOfferEditorTexts()
    {
        if (_cardCountText != null)
        {
            _cardCountText.text = (_player1OfferedCards?.Count ?? 0).ToString();
        }

        if (_ownedMoneyText != null)
        {
            try
            {
                int owned;
                if (!TryGetOwnedMoney(out owned))
                {
                    owned = GameRun?.Money ?? 0;
                }
                if (owned < 0) owned = 0;
                _ownedMoneyText.text = owned.ToString();
            }
            catch
            {
                _ownedMoneyText.text = "0";
            }
        }

        if (_moneyValueText != null)
        {
            _moneyValueText.text = _localMoneyOffer.ToString();
            ApplyMoneyValueSizingForDigits();
        }

        if (_exhibitValueText != null)
        {
            _exhibitValueText.text = _localExhibitOfferIds.Count.ToString();
        }
    }

    private void SetRect(RectTransform rt, float minX, float minY, float maxX, float maxY)
    {
        rt.anchorMin = new Vector2(minX, minY);
        rt.anchorMax = new Vector2(maxX, maxY);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private GameObject _exhibitPickerRoot;

    private void ShowExhibitPickerOverlay()
    {
        EnsureExhibitPickerOverlay();
        if (_exhibitPickerRoot == null)
        {
            TryShowTopMessage("展品选择界面不可用。");
            return;
        }

        RebuildExhibitPickerList();
        _exhibitPickerRoot.SetActive(true);
        ForceEnableRaycasts(_exhibitPickerRoot);
        SetTradeDetailsVisible(false);
    }

    private void HideExhibitPickerOverlay(bool apply)
    {
        if (_exhibitPickerRoot != null)
        {
            _exhibitPickerRoot.SetActive(false);
        }
        SetTradeDetailsVisible(true);

        if (apply)
        {
            RefreshOfferEditorTexts();
            TrySendOfferUpdate();
            CheckTradeReady();
        }
    }

    private void EnsureExhibitPickerOverlay()
    {
        if (_exhibitPickerRoot != null)
        {
            return;
        }

        try
        {
            GameObject dialogPrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (dialogPrefab == null)
            {
                _exhibitPickerRoot = null;
                return;
            }

            _exhibitPickerRoot = Instantiate(dialogPrefab, transform, false);
            _exhibitPickerRoot.name = "TradeExhibitPicker";
            _exhibitPickerRoot.SetActive(false);

            var rootRect = _exhibitPickerRoot.GetComponent<RectTransform>();
            if (rootRect != null)
            {
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.offsetMin = Vector2.zero;
                rootRect.offsetMax = Vector2.zero;
            }

            MessageDialog dialog = _exhibitPickerRoot.GetComponentInChildren<MessageDialog>(true);
            if (dialog == null)
            {
                Destroy(_exhibitPickerRoot);
                _exhibitPickerRoot = null;
                return;
            }

            TextMeshProUGUI mainText = GetDialogField<TextMeshProUGUI>(dialog, "mainText");
            TextMeshProUGUI subText = GetDialogField<TextMeshProUGUI>(dialog, "subText");
            Button singleConfirm = GetDialogField<Button>(dialog, "singleConfirmButton");
            Button confirm = GetDialogField<Button>(dialog, "confirmButton");
            Button cancel = GetDialogField<Button>(dialog, "cancelButton");

            RectTransform subTextRect = subText != null ? subText.rectTransform : null;

            if (mainText != null)
            {
                mainText.text = "选择要交易的展品";
                mainText.alignment = TextAlignmentOptions.Center;
                mainText.raycastTarget = false;
            }

            if (subText != null)
            {
                // Used for empty/disabled/full messages.
                subText.text = string.Empty;
                subText.raycastTarget = false;
                subText.alignment = TextAlignmentOptions.Center;
                var c = subText.color;
                c.a = 0f;
                subText.color = c;
                subText.gameObject.SetActive(true);
            }

            if (singleConfirm != null)
            {
                singleConfirm.onClick.RemoveAllListeners();
                singleConfirm.gameObject.SetActive(false);
            }

            if (confirm != null)
            {
                confirm.onClick.RemoveAllListeners();
                confirm.gameObject.SetActive(true);

                var confirmLabel = confirm.GetComponentInChildren<TextMeshProUGUI>(true);
                if (confirmLabel != null)
                {
                    confirmLabel.text = "确定";
                    confirmLabel.alignment = TextAlignmentOptions.Center;
                }

                confirm.onClick.AddListener(() =>
                {
                    try
                    {
                        HideExhibitPickerOverlay(true);
                    }
                    catch
                    {
                        // ignored
                    }
                });
            }

            if (cancel != null)
            {
                cancel.onClick.RemoveAllListeners();
                cancel.gameObject.SetActive(true);

                var cancelLabel = cancel.GetComponentInChildren<TextMeshProUGUI>(true);
                if (cancelLabel != null)
                {
                    cancelLabel.text = "取消";
                    cancelLabel.alignment = TextAlignmentOptions.Center;
                }

                cancel.onClick.AddListener(() =>
                {
                    try
                    {
                        HideExhibitPickerOverlay(false);
                    }
                    catch
                    {
                        // ignored
                    }
                });
            }

            RectTransform panelRect = TryFindCommonAncestorRect(mainText != null ? mainText.rectTransform : null,
                cancel != null ? cancel.GetComponent<RectTransform>() : null);
            panelRect ??= mainText != null ? mainText.rectTransform.parent as RectTransform : null;
            panelRect ??= rootRect;
            if (panelRect == null)
            {
                Destroy(_exhibitPickerRoot);
                _exhibitPickerRoot = null;
                return;
            }

            if (!TryAttachHistoryListWithRecordRow(panelRect, subTextRect, out ScrollRect listScrollRect, out RectTransform listContent, out RecordRow rowTemplate))
            {
                Destroy(_exhibitPickerRoot);
                _exhibitPickerRoot = null;
                return;
            }

            ExhibitPickerTag tag = listContent.gameObject.AddComponent<ExhibitPickerTag>();
            tag.RecordRowTemplate = rowTemplate;
            tag.ScrollRect = listScrollRect;
            tag.EmptyText = subText;

            if (rowTemplate != null)
            {
                rowTemplate.gameObject.SetActive(false);
            }

            dialog.enabled = false;
        }
        catch
        {
            _exhibitPickerRoot = null;
        }
    }

    private sealed class ExhibitPickerTag : MonoBehaviour
    {
        public RecordRow RecordRowTemplate;
        public ScrollRect ScrollRect;
        public TextMeshProUGUI EmptyText;
    }

    private void RebuildExhibitPickerList()
    {
        if (_exhibitPickerRoot == null)
        {
            return;
        }

        ExhibitPickerTag tag = _exhibitPickerRoot.GetComponentInChildren<ExhibitPickerTag>(true);
        if (tag == null || tag.RecordRowTemplate == null)
        {
            return;
        }

        Transform container = tag.transform;
        foreach (Transform child in container)
        {
            if (tag.RecordRowTemplate != null && child == tag.RecordRowTemplate.transform)
            {
                continue;
            }
            Destroy(child.gameObject);
        }

        void ShowEmpty(string msg)
        {
            if (tag.ScrollRect != null)
            {
                tag.ScrollRect.gameObject.SetActive(false);
            }

            if (tag.EmptyText != null)
            {
                tag.EmptyText.gameObject.SetActive(true);
                tag.EmptyText.text = msg ?? string.Empty;
                var c = tag.EmptyText.color;
                c.a = 1f;
                tag.EmptyText.color = c;
            }
        }

        void HideEmptyAndShowList()
        {
            if (tag.EmptyText != null)
            {
                tag.EmptyText.text = string.Empty;
                var c = tag.EmptyText.color;
                c.a = 0f;
                tag.EmptyText.color = c;
            }

            if (tag.ScrollRect != null)
            {
                tag.ScrollRect.gameObject.SetActive(true);
            }
        }

        List<Exhibit> tradable = new List<Exhibit>();
        try
        {
            if (GameRun?.Player?.Exhibits != null)
            {
                tradable = GameRun.Player.Exhibits
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
            ShowEmpty("没有可交易的展品。");
            return;
        }

        HideEmptyAndShowList();

        foreach (var ex in tradable)
        {
            CreateExhibitRecordRow(container, tag.RecordRowTemplate, ex);
        }
    }

    private void CreateExhibitRecordRow(Transform parent, RecordRow template, Exhibit exhibit)
    {
        try
        {
            if (template == null || exhibit == null)
            {
                return;
            }

            RecordRow row = Instantiate(template, parent, false);
            row.name = $"Ex_{exhibit.Id}";
            row.gameObject.SetActive(true);

            Image avatarImage = GetPrivateFieldValue<Image>(row, "avatarImage");
            TextMeshProUGUI gameResultText = GetPrivateFieldValue<TextMeshProUGUI>(row, "gameResultText");
            TextMeshProUGUI difficultyText = GetPrivateFieldValue<TextMeshProUGUI>(row, "difficultyText");
            TextMeshProUGUI timestampText = GetPrivateFieldValue<TextMeshProUGUI>(row, "timestampText");
            Image exhibitIcon = GetPrivateFieldValue<Image>(row, "exhibitIcon");

            if (avatarImage != null)
            {
                try
                {
                    avatarImage.sprite = ResourcesHelper.TryGetSprite<Exhibit>(exhibit.Id);
                    avatarImage.gameObject.SetActive(avatarImage.sprite != null);
                }
                catch
                {
                    avatarImage.gameObject.SetActive(false);
                }
            }
            if (exhibitIcon != null)
            {
                exhibitIcon.gameObject.SetActive(false);
            }
            if (gameResultText != null)
            {
                gameResultText.text = exhibit.Name;
            }
            if (difficultyText != null)
            {
                difficultyText.text = exhibit.Id;
            }
            if (timestampText != null)
            {
                timestampText.text = string.Empty;
            }

            bool selected = _localExhibitOfferIds.Contains(exhibit.Id);
            row.SetSelected(selected, false);

            row.Click += () =>
            {
                try
                {
                    bool nowSelected = !_localExhibitOfferIds.Contains(exhibit.Id);
                    if (nowSelected)
                    {
                        _localExhibitOfferIds.Add(exhibit.Id);
                    }
                    else
                    {
                        _localExhibitOfferIds.Remove(exhibit.Id);
                    }

                    row.SetSelected(nowSelected, false);
                    RefreshOfferEditorTexts();
                }
                catch
                {
                    // ignored
                }
            };
        }
        catch
        {
            // ignored
        }
    }

    private bool TryAttachHistoryListWithRecordRow(
        RectTransform dialogPanelRect,
        RectTransform placeholderRect,
        out ScrollRect listScrollRect,
        out RectTransform listContent,
        out RecordRow recordRowTemplate)
    {
        listScrollRect = null;
        listContent = null;
        recordRowTemplate = null;

        GameObject historyInstance = null;
        try
        {
            GameObject historyPrefab = Resources.Load<GameObject>("UI/Panels/HistoryPanel");
            if (historyPrefab == null)
            {
                return false;
            }

            historyInstance = Instantiate(historyPrefab);
            historyInstance.SetActive(false);

            var historyPanel = historyInstance.GetComponentInChildren<HistoryPanel>(true);
            if (historyPanel == null)
            {
                return false;
            }

            listScrollRect = GetPrivateFieldValue<ScrollRect>(historyPanel, "listScrollRect");
            listContent = GetPrivateFieldValue<RectTransform>(historyPanel, "listContent");
            recordRowTemplate = GetPrivateFieldValue<RecordRow>(historyPanel, "recordRowTemplate");

            if (listScrollRect == null || listContent == null || recordRowTemplate == null)
            {
                return false;
            }

            listScrollRect.transform.SetParent(dialogPanelRect, false);

            RectTransform scrollRt = listScrollRect.GetComponent<RectTransform>();
            if (scrollRt != null)
            {
                if (placeholderRect != null)
                {
                    CopyRectTransform(scrollRt, placeholderRect);
                }
                else
                {
                    scrollRt.anchorMin = new Vector2(0.06f, 0.20f);
                    scrollRt.anchorMax = new Vector2(0.94f, 0.78f);
                    scrollRt.offsetMin = Vector2.zero;
                    scrollRt.offsetMax = Vector2.zero;
                }
            }

            Destroy(historyInstance);
            historyInstance = null;
            return true;
        }
        catch
        {
            return false;
        }
        finally
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
        }
    }

    private static void SetButtonText(CommonButtonWidget button, string label)
    {
        try
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
        catch
        {
            // ignored
        }
    }

    private readonly struct ApplyingStateScope : IDisposable
    {
        private readonly TradePanel _panel;
        private readonly bool _prev;

        public ApplyingStateScope(TradePanel panel)
        {
            _panel = panel;
            _prev = panel._isApplyingState;
            panel._isApplyingState = true;
        }

        public void Dispose()
        {
            if (_panel != null)
            {
                _panel._isApplyingState = _prev;
            }
        }
    }

    /// <summary>
    /// 显示交易 UI 的协程方法，调用方可等待该协程直到面板被关闭。
    /// </summary>
    /// <param name="payload">交易配置参数。</param>
    /// <returns>用于等待面板关闭的协程。</returns>
    public IEnumerator ShowTradeAsync(TradePayload payload)
    {
        // 显示交易面板
        Show(payload);
        // 在面板可见期间一直等待
        yield return new WaitWhile(() => IsVisible);
    }

    #endregion
}
