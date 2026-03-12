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
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.UI.Dialogs;
using NetworkPlugin.UI.Factories;
using NetworkPlugin.UI.Payloads;
using NetworkPlugin.UI.Rules;
using NetworkPlugin.UI.Widgets;
using NetworkPlugin.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

    // 当 true 时，TradePanel 已将控制权交给 TradeDetailDialog，不应再发起网络请求或初始化面板内编辑器。
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

    // v2 本地报价（金币 + 展品），卡牌存储于 _player1OfferedCards。
    private int _localMoneyOffer;
    private readonly HashSet<string> _localExhibitOfferIds = new HashSet<string>(StringComparer.Ordinal);

    // 状态转换时用于记录上一次已知状态（Preparing 阶段验证）。
    private TradeSyncPatch.TradeStatus? _lastTradeStatus;
    private long _lastPreparingHandledTimestamp;

    // 运行时 partner picker overlay（精简构建，避免 prefab 依赖）。
    private GameObject _partnerPickerRoot;
    private bool _partnerPickerActive;
    private string _partnerPickerBuildError;
    private Button _partnerPickerCancelButton;
    private Button _partnerPickerRefreshButton;

    // 打开面板后自动刷新一次 partner 列表（应对位置元数据稍晚到达的情况）。
    private Coroutine _partnerPickerAutoRefreshCo;

    // 标记本面板是否已将自身压入 UiManager 的 action handler 栈。
    private bool _actionHandlerPushed;

    // 当 true 时，中央正在显示模态提示对话框，交易详情被屏蔽。
    private bool _blockingCenterMessageActive;

    // 离线/本地调试模式：允许在无服务器时打开并操作 TradePanel。
    // 用于在 DebugVirtualPlayerAiDefault/DebugFakePlayersForTrade 启用时验证 UI 流程
    // （partner picker、卡牌/展品选)。
    private bool _localDebugTradeMode;

    // 运行时报价编辑器 overlay（金币 + 展品）。
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

    // 运行时卡牌选择器 overlay（卡组卡牌）。
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
        // 这些字段通常由 prefab 连接，运行时创建时需手动绑定。
        player1TradeArea = runtimePlayer1TradeArea;
        player2TradeArea = runtimePlayer2TradeArea;
        player1Slots = runtimePlayer1Slots;
        player2Slots = runtimePlayer2Slots;
        confirmButton = runtimeConfirmButton;
        cancelButton = runtimeCancelButton;
        statusText = runtimeStatusText;
        player1NameText = runtimePlayer1NameText;
        player2NameText = runtimePlayer2NameText;

        // 运行时创建的面板在 Awake() 之后绑定，因此需在此处注册按钮事件。
        if (confirmButton?.button != null)
        {
            confirmButton.button.onClick.RemoveAllListeners();
            confirmButton.button.onClick.AddListener(OnConfirmTrade);

            // 用户需求：确认按钮使用 Open behavior + Normal weight。
            try
            {
                Traverse traverse = HarmonyLib.Traverse.Create(confirmButton);
                traverse.Field("buttonBehavior").SetValue(0);
                traverse.Field("buttonWeight").SetValue(0);
            }
            catch
            {
                // 忽略
            }
        }

        cancelButton?.button?.onClick.RemoveAllListeners();
        cancelButton?.button?.onClick.AddListener(OnCancelTrade);
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
        // 本地 UI 测试：调试开关启用时允许在无服务器情况下打开面板。
        _localDebugTradeMode = !TryEnsureNetworkConnected() && IsLocalDebugTradeAllowed();
        if (!_localDebugTradeMode && !TryEnsureNetworkConnected())
        {
            Hide();
            return;
        }

        // 提前注册输入处理器，确保在设置期间弹出的 MessageDialog 能正确压栈。
        // （否则 MessageDialog/TradePanel Push/Pop 顺序可能错误，UiManager 会记录错误日志。）
        UiManager.PushActionHandler(this);
        _actionHandlerPushed = true;

        // 重要：在可能提前返回之前（即显示 partner picker 或 modal dialog 前）确保面板可交互。
        // 否则如果面板在上次隐藏后再次打开，picker 可能显示但无法点击。
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

        // 已移交给 dialog，不创建面板内编辑器或执行更多 UI 操作。
        if (_handoffToDetailDialog)
        {
            return;
        }

        // 若正在选择交易对象，或已经进入“阻塞提示”状态，则不需要提前初始化报价编辑/卡牌选择等 overlay。
        if (_partnerPickerActive || _blockingCenterMessageActive)
        {
            return;
        }

        // 确保运行时 overlay 存在（工厂创建的面板无法通过 prefab 和进 UI）。
        EnsureOfferEditorOverlay();
        EnsureCardPickerOverlay();

        // 设置玩家名称显示（不使用 Player 1/2 之类的占位文本）
        player1NameText?.text = ResolveLocalPlayerDisplayName(payload);
        player2NameText?.text = ResolvePartnerDisplayName(payload);

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
        _canvasGroup?.interactable = false;

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

        // 确保 action 按钮文字在 prefab 和运行时面板间保持一致。
        SetButtonText(confirmButton, "确认交易");
        SetButtonText(cancelButton, "取消");
    }

    private static string TryLocalize(string key, string fallback)
    {
        try
        {
            // 部分模组包可能未包含这些本地化 key，会产生 Unity 日志噪声。
            // 使用可读的中文备选项，确保玩家仍可看到状态变化。
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

        // 提前清空可见标签，避免显示过时/占位符名称。
        player1NameText?.text = string.Empty;
        player2NameText?.text = string.Empty;

        // 隐藏所有活跃的 overlay。
        _partnerPickerActive = false;
        _blockingCenterMessageActive = false;
        _partnerPickerRoot?.SetActive(false);

        if (_partnerPickerAutoRefreshCo != null)
        {
            StopCoroutine(_partnerPickerAutoRefreshCo);
            _partnerPickerAutoRefreshCo = null;
        }
        _cardPickerRoot?.SetActive(false);
        _exhibitPickerRoot?.SetActive(false);

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
        statusText?.text = message;
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

        List<Card> offeredCards = isPlayer1 ? _player1OfferedCards : _player2OfferedCards;

        // 仅在未超过最大交易卡位时添加
        if (offeredCards.Count < _maxTradeSlots)
        {
            offeredCards.Add(card);
            UpdateTradeSlot(card, isPlayer1 ? player1Slots : player2Slots, offeredCards.Count - 1);
            CheckTradeReady();

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
        if (card == null)
        {
            return;
        }

        // 联机模式下：只允许玩家操作“本地侧”(player1)。
        if (!_isApplyingState && TryIsNetworkTrade(out _) && !isPlayer1)
        {
            return;
        }

        List<Card> offeredCards = isPlayer1 ? _player1OfferedCards : _player2OfferedCards;
        TradeSlotWidget[] slots = isPlayer1 ? player1Slots : player2Slots;

        int index = offeredCards.IndexOf(card);
        if (index >= 0)
        {
            offeredCards.RemoveAt(index);

            for (int i = index; i < offeredCards.Count; i++)
            {
                UpdateTradeSlot(offeredCards[i], slots, i);
            }

            // 清空末尾的 UI 槽位（避免旧卡牌残留显示）
            if (slots != null && offeredCards.Count < slots.Length)
            {
                slots[offeredCards.Count]?.ClearSlot();
            }

            CheckTradeReady();

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
        if (slots != null && index >= 0 && index < slots.Length && slots[index] != null)
        {
            bool isPlayer1 = slots == player1Slots;
            slots[index].SetCard(card, (c) => RemoveCardFromTrade(c, isPlayer1));
        }
    }

    /// <summary>
    /// 检查双方是否已放入至少一张卡牌，从而决定是否允许确认交易。
    /// </summary>
    private void CheckTradeReady()
    {
        // v2：允许交易任意资产（卡牌/道具/金币/展品）。
        bool localHasOffer = HasLocalOffer();
        bool remoteHasOffer = HasRemoteOffer();
        bool bothPlayersReady = localHasOffer && remoteHasOffer;

        // 用户需求：确认按钮永不置灰，点击服务端未就绪时会显示状态提示但不发送确认。
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
            // v2：双方都提供了任意资产（卡牌/道具/金币/展品）即可确认。
            // 远端报价来自 host 状态，不一定来自 _player2OfferedCards。
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
        // v1：交易完成事件由 TradeSyncPatch 统一发送/广播。
        // 这里保留方法作为本地模式的扩展点。
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
                // 离线/本地调试分支：伪造稳定本地身份，使展示和过滤能正常工作。
                // 避免等待网络状态更新。
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

            // 需求：无法明确确定 partner 时必须显示 partner picker UI。
            if (string.IsNullOrWhiteSpace(_playerBId) || string.Equals(_playerBId, _selfPlayerId, StringComparison.Ordinal))
            {
                ShowPartnerPickerOverlay();
                return;
            }

            // 已连接：请求 host 驱动的交易会话。离线/本地调试：跳过网络。
            if (connected)
            {
                // 首选 dialog 屏山类编辑器。创建失败时回退到面板内流程。
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
                // 本地调试会话：立即显示交易详情。
                EnsureOfferEditorOverlay();
                EnsureCardPickerOverlay();
                EnsureExhibitPickerOverlay();
                SetTradeDetailsVisible(true);
                cancelButton?.gameObject.SetActive(_canCancel);
                UpdateUIStatus("本地调试交易：未连接服务器");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TradePanel] SetupTradeSession 失败: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private bool TryShowTradeDetailDialog(string partnerPlayerId, string partnerPreferredName)
    {
        try
        {
            // 仅在已连接时移交控制权；本地调试模式保持在面板内。
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
                PartnerPlayerName = OtherPlayersOverlayPatch.ResolveDisplayName(partnerPlayerId, partnerPreferredName, isLocal: false),
                MaxTradeSlots = _maxTradeSlots
            });

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[TradePanel] TryShowTradeDetailDialog 失败: {ex.Message}");
            return false;
        }
    }

    private static bool IsLocalDebugTradeAllowed()
    {
        var cfg = ModService.ServiceProvider?.GetService<Configuration.ConfigManager>();
        return cfg?.DebugVirtualPlayerAiDefault?.Value == true || cfg?.DebugFakePlayersForTrade?.Value == true;
    }

    private bool TryEnsureNetworkConnected()
    {
        INetworkClient client = ModService.ServiceProvider?.GetService<INetworkClient>();
        if (client == null || !client.IsConnected)
        {
            TryShowTopMessage("交易仅在联机模式下可用。");
            return false;
        }

        return true;
    }

    private void TryShowTopMessage(string message)
    {
        if (UiManager.IsInitialized)
            UiManager.GetPanel<TopMessagePanel>().ShowMessage(message);
    }

    private void ShowPartnerPickerOverlay()
    {
        if (_partnerPickerActive)
        {
            return;
        }

        // 确保 overlay 获得点击响应（即使面板之前被隐藏过）。
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        // 懒创建 overlay。
        EnsurePartnerPickerOverlay();
        if (_partnerPickerRoot == null)
        {
            ShowTradeTargetUnavailableDialog(_partnerPickerBuildError);
            return;
        }

        _partnerPickerActive = true;
        _partnerPickerRoot.SetActive(true);

        // 通过 prefab 实例化 MessageDialog 后其 CanvasGroup 默认可能不可交互，强制开启 raycasts。
        ForceEnableRaycasts(_partnerPickerRoot);

        // 隐藏底层交易详情，直到选择了交易对象。
        SetTradeDetailsVisible(false);

        // picker 自带取消按钮，隐藏底层的以避免重复。
        cancelButton?.gameObject.SetActive(false);

        // 注意：不要在此处禁用根 CanvasGroup。
        // partner picker overlay 是 TradePanel 的子对象，禁用根 CanvasGroup
        // 也会使 overlay 按钮（包括取消和交易对象行）无法点击。

        // 避免重复标题文本（overlay 已有自己的标题）。
        UpdateUIStatus(string.Empty);
        RebuildPartnerPickerList();

        // 如果自身位置尚未获取，显示等待提示并安排一次自动刷新。
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

    private void TryShowPartnerPickerWaiting()
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

        tag.ScrollRect?.gameObject.SetActive(false);

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

    private IEnumerator CoPartnerPickerAutoRefreshOnce()
    {
        // 最多等待 1.0 秒并刷新一次列表。
        float t = 0f;
        while (t < 1.0f)
        {
            if (!_partnerPickerActive || _partnerPickerRoot == null)
            {
                _partnerPickerAutoRefreshCo = null;
                yield break;
            }

            // 若已取得位置信息，可立即刷新。
            if (OtherPlayersOverlayPatch.TryGetSelfLocation(out _, out _, out _, out _))
            {
                break;
            }

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (_partnerPickerActive && _partnerPickerRoot != null)
        {
            RebuildPartnerPickerList();
        }

        _partnerPickerAutoRefreshCo = null;
    }

    private static void ForceEnableRaycasts(GameObject root)
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

            // 确保即使父 CanvasGroup 被临时禁用，dialog 也可以点击。
            cg.ignoreParentGroups = true;
        }
    }

    private void ShowTradeTargetUnavailableDialog(string detail)
    {
        // 与 vanilla 保持一致：使用 UiManager 管理的 MessageDialog 播放过渡动画。
        if (!UiManager.IsInitialized)
        {
            return;
        }

        // 显示 dialog 期间阻断底层交易 UI。
        _blockingCenterMessageActive = true;
        SetTradeDetailsVisible(false);

        confirmButton?.gameObject.SetActive(false);
        cancelButton?.gameObject.SetActive(false);
        UpdateUIStatus(string.Empty);
        _canvasGroup?.interactable = false;

        UiManager.GetDialog<MessageDialog>().Show(new MessageContent
        {
            Text = "交易对象不可用",
            SubText = string.IsNullOrWhiteSpace(detail) ? "交易对象选择界面不可用。" : ("交易对象选择界面不可用。\n" + detail),
            Icon = MessageIcon.Error,
            Buttons = DialogButtons.Confirm,
            OnConfirm = () =>
            {
                _blockingCenterMessageActive = false;
                Hide();
            },
            OnCancel = () =>
            {
                _blockingCenterMessageActive = false;
                Hide();
            }
        });
    }

    private void HidePartnerPickerOverlay()
    {
        if (_partnerPickerAutoRefreshCo != null)
        {
            StopCoroutine(_partnerPickerAutoRefreshCo);
            _partnerPickerAutoRefreshCo = null;
        }

        _partnerPickerActive = false;
        _partnerPickerRoot?.SetActive(false);

        // 重新开启交易 UI。
        SetTradeDetailsVisible(true);

        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        // 恢复底层取消按钮状态。
        cancelButton?.gameObject.SetActive(_canCancel);
    }

    private void SetTradeDetailsVisible(bool visible)
    {
        // 背景框（RuntimeFactory 创建时挂在根节点下的 TradeFrame）随主界面一起显隐。
        var tradeFrame = transform.Find("TradeFrame");
        tradeFrame?.gameObject.SetActive(visible);

        statusText?.gameObject.SetActive(visible);

        player1TradeArea?.gameObject.SetActive(visible);
        player2TradeArea?.gameObject.SetActive(visible);
        player1NameText?.gameObject.SetActive(visible);
        player2NameText?.gameObject.SetActive(visible);

        // 隐藏确认按钮以减少 session 开始前的视觉混乱，取消按钮在允许取消时保持可见。
        confirmButton?.gameObject.SetActive(visible);
        cancelButton?.gameObject.SetActive(visible || _canCancel);

        _offerEditorRoot?.SetActive(visible);
        _offerActionsRoot?.SetActive(visible);
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

            // 严格要求：使用游戏内 dialog prefab 作为 overlay 窗口框架，而非运行时构建的 Image/Outline。
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

            // 通过反射提取序列化字段，以复用 prefab 的文字/按钮。
            TextMeshProUGUI mainText = GetDialogField<TextMeshProUGUI>(dialog, "mainText");
            TextMeshProUGUI subText = GetDialogField<TextMeshProUGUI>(dialog, "subText");
            Button singleConfirm = GetDialogField<Button>(dialog, "singleConfirmButton");
            Button confirm = GetDialogField<Button>(dialog, "confirmButton");
            Button cancel = GetDialogField<Button>(dialog, "cancelButton");

            RectTransform subTextRect = subText?.rectTransform;

            if (mainText != null)
            {
                mainText.text = "请选择交易对象";
                mainText.alignment = TextAlignmentOptions.Center;
                mainText.raycastTarget = false;
            }

            if (subText != null)
            {
                // 复用其 rect 作为列表占位区域；并添加点击刺激（点击可触发刷新）。
                subText.text = string.Empty;
                subText.raycastTarget = true;
                subText.alignment = TextAlignmentOptions.Center;
                var c = subText.color;
                c.a = 0f;
                subText.color = c;
                subText.gameObject.SetActive(true);

                // 添加 Button 使空状态标签可点击（触发刷新）。
                var subTextBtn = subText.gameObject.GetComponent<Button>() ?? subText.gameObject.AddComponent<Button>();
                subTextBtn.targetGraphic = subText;
                subTextBtn.onClick.RemoveAllListeners();
                subTextBtn.onClick.AddListener(() => OnPartnerPickerRefreshClicked());
            }

            // 确保 dialog 按钮不会调用 UiDialog.Hide()（调用会修改 UiManager 当前 dialog 状态）。
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

                confirm.onClick.AddListener(OnPartnerPickerRefreshClicked);
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
                    HidePartnerPickerOverlay();
                    Hide();
                });
            }

            // 安全网：如果行级 pointer 事件受 prefab raycast 层次/逆序阻塞，
            // 则在 overlay 根捕获点击并通过矩形命中测试解析被点击的行。
            var catcher = _partnerPickerRoot.GetComponent<PartnerPickerClickCatcher>();
            if (catcher == null)
            {
                catcher = _partnerPickerRoot.AddComponent<PartnerPickerClickCatcher>();
            }
            catcher.Panel = this;

            RectTransform panelRect = TryFindCommonAncestorRect(mainText?.rectTransform,
                cancel?.GetComponent<RectTransform>());
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

            // 构建用于显示 TMP 可点击文字 partner 条目的简单 ScrollRect 容器。
            TextMeshProUGUI pickerTextTemplate = mainText ?? subText;
            if (pickerTextTemplate == null)
            {
                Destroy(_partnerPickerRoot);
                _partnerPickerRoot = null;
                _partnerPickerBuildError = "无法获取文字模板";
                return;
            }

            {
                GameObject scrollGo = new GameObject("PartnerScroll");
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

                GameObject viewport = new GameObject("Viewport");
                viewport.transform.SetParent(scrollGo.transform, false);
                var viewportRt = viewport.AddComponent<RectTransform>();
                viewportRt.anchorMin = Vector2.zero;
                viewportRt.anchorMax = Vector2.one;
                viewportRt.offsetMin = Vector2.zero;
                viewportRt.offsetMax = Vector2.zero;
                viewport.AddComponent<RectMask2D>();

                GameObject contentGo = new GameObject("Content");
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

            // 确保取消按钮在列表上方。
            if (cancel != null)
            {
                cancel.transform.SetAsLastSibling();
            }

            // 验证标签创建成功。
            PartnerPickerTag tagCheck = _partnerPickerRoot.GetComponentInChildren<PartnerPickerTag>(true);
            if (tagCheck == null || tagCheck.TextTemplate == null)
            {
                Destroy(_partnerPickerRoot);
                _partnerPickerRoot = null;
                _partnerPickerBuildError = "列表模板创建失败";
                return;
            }

            // 禁用 dialog 组件以避免意外的输入处理；我们只需要其视觉呈现。
            dialog.enabled = false;
        }
        catch
        {
            if (string.IsNullOrWhiteSpace(_partnerPickerBuildError))
            {
                _partnerPickerBuildError = "构建交易对象选择界面时发生异常";
            }
            if (_partnerPickerRoot != null)
            {
                Destroy(_partnerPickerRoot);
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

        // 立即尝试重建列表。
        RebuildPartnerPickerList();

        // 若自身位置仍不可用，显示等待状态并安排一次性刷新。
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

    private static T GetPrivateFieldValue<T>(object target, string fieldName) where T : class
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

    private void RebuildPartnerPickerList()
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

        Transform container = tag.ListContainer != null ? tag.ListContainer : tag.transform;
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        string selfId = _selfPlayerId ?? NetworkIdentityTracker.GetSelfPlayerId();

        // 优先使用详细快照，以便过滤“当前在商店中的玩家”并展示头像/位置。
        var players = OtherPlayersOverlayPatch.SnapshotPlayersDetailed();

        bool hasSelfLoc = OtherPlayersOverlayPatch.TryGetSelfLocation(out int selfStage, out int selfX, out int selfY, out string selfLocName);

        // 部分环境可能过早没有将虚拟调试玩家注入快照。
        // 如果调试开关已开启，合成一个对齐到自身位置的 "AI Default" 条目，小节点同节点规则并可用于本地 UI 测试。
        if (hasSelfLoc && IsLocalDebugTradeAllowed())
        {
            if (players.All(p => !string.Equals(p.PlayerId, "aidefault", StringComparison.Ordinal)))
            {
                string loc = IsShopLikeLocation(selfLocName) ? selfLocName : "Trade";
                players.Add(("aidefault", "AI Default", true, false, selfStage, selfX, selfY, loc, null));
            }

            if (players.All(p => !string.Equals(p.PlayerId, "aidefault2", StringComparison.Ordinal)))
            {
                string loc = IsShopLikeLocation(selfLocName) ? selfLocName : "Trade";
                players.Add(("aidefault2", "AI Default 2", true, false, selfStage, selfX, selfY, loc, null));
            }
        }

            List<(string PlayerId, string PlayerName, bool IsConnected, bool IsHost, int Stage, int LocationX, int LocationY, string LocationName, string CharacterId)> connectedOthers = players
                .Where(p => !string.IsNullOrWhiteSpace(p.PlayerId))
                .Where(p => !string.Equals(p.PlayerId, selfId, StringComparison.Ordinal))
                .Where(p => p.IsConnected)
                .ToList();

            List<(string PlayerId, string PlayerName, bool IsConnected, bool IsHost, int Stage, int LocationX, int LocationY, string LocationName, string CharacterId)> candidates = connectedOthers
                .Where(p => IsShopLikeLocation(p.LocationName))
                .ToList();

            // 选择规则：必须在相同节点才可选择。
            // 如果尚不知道自身位置，无法安全强制执行该规则。
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
                // 严格模式：空状态也必须使用游戏内 UI 元素。
                tag.ScrollRect?.gameObject.SetActive(false);

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
            tag.ScrollRect?.gameObject.SetActive(true);

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
                btn.onClick.AddListener(() => OnPartnerSelected(pid, pname));
            }
    }

    private void OnPartnerSelected(string partnerPlayerId, string partnerPlayerName)
    {
        _playerAId = _selfPlayerId;
        _playerBId = partnerPlayerId;

        if (string.IsNullOrWhiteSpace(_tradeId))
        {
            _tradeId = Guid.NewGuid().ToString("N");
        }

        player1NameText?.text = ResolveLocalPlayerDisplayName(_payload);
        player2NameText?.text = OtherPlayersOverlayPatch.ResolveDisplayName(partnerPlayerId, partnerPlayerName, isLocal: false);

        HidePartnerPickerOverlay();

        // 已连接：进行实际的 host 驱动会话。离线/本地调试：保持本地 UI（不发送网络请求）。
        if (IsLocalDebugTradeAllowed() && (string.Equals(partnerPlayerId, "aidefault", StringComparison.Ordinal) || string.Equals(partnerPlayerId, "aidefault2", StringComparison.Ordinal)))
        {
            // 即使已连接，选择本地调试虚拟玩家也允许启动纯本地 UI 测试会话。
            _localDebugTradeMode = true;
            PopulateLocalDebugRemoteOffer();
            EnsureOfferEditorOverlay();
            EnsureCardPickerOverlay();
            EnsureExhibitPickerOverlay();
            SetTradeDetailsVisible(true);
            cancelButton?.gameObject.SetActive(_canCancel);
            UpdateUIStatus($"本地调试交易：{partnerPlayerName}（不走服务器）");
            return;
        }

        // 网络交易优先使用基于 dialog 的编辑器。
        if (TryShowTradeDetailDialog(partnerPlayerId, partnerPlayerName))
        {
            _handoffToDetailDialog = true;
            Hide(false);
            return;
        }

        // Dialog 失败：回退到面板内编辑器，保证交易仍可用。
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

    private void PopulateLocalDebugRemoteOffer()
    {
        // 初始化远端侧（player2）的展示报价，供离线 UI 测试。不修改真实牌组/背包。
        using (new ApplyingStateScope(this))
        {
            // 取少量本地牌组卡牌作为展示克隆。
            var deck = GameRun?.BaseDeck?.Where(c => c != null).ToList() ?? new List<Card>();
            int take = Math.Min(2, deck.Count);
            for (int i = 0; i < take; i++)
            {
                var src = deck[i];
                if (src == null)
                {
                    continue;
                }

                Card temp = Library.TryCreateCard(src.Id, src.IsUpgraded, src.UpgradeCounter ?? 0);
                if (temp != null)
                {
                    AddCardToTrade(temp, false);
                }
            }

            // 本地侧加小额金币报价，使报价编辑器显示非零状态。
            _localMoneyOffer = Math.Min(10, GameRun?.Money ?? 10);
            RefreshOfferEditorTexts();
            CheckTradeReady();
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

        return OtherPlayersOverlayPatch.ResolveDisplayName(id, payload?.Player1Name, isLocal: true);
    }

    private string ResolvePartnerDisplayName(TradePayload payload)
    {
        // 如使用了 partner picker，_playerBId 将在选择后被赋值。
        string id = _playerBId;
        if (string.IsNullOrWhiteSpace(id))
        {
            id = payload?.Player2Id;
        }

        return OtherPlayersOverlayPatch.ResolveDisplayName(id, payload?.Player2Name, isLocal: false);
    }

    private static bool IsShopLikeLocation(string locationName)
    {
        if (string.IsNullOrWhiteSpace(locationName))
        {
            return false;
        }

        // LocationName 由网络同步设置为 visitingNode.StationType.ToString()。
        // 模糊匹配以应对重命名/变体。允许交易的地点：Shop/Trade（商人）和 Gap（GapOptions 节点）。
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
            // 严格要求：使用游戏内 UI prefab（MessageDialog + HistoryPanel + RecordRow），不构建运行时 UI。
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

            RectTransform subTextRect = subText?.rectTransform;

            if (mainText != null)
            {
                mainText.text = "选择要交易的卡牌";
                mainText.alignment = TextAlignmentOptions.Center;
                mainText.raycastTarget = false;
            }

            if (subText != null)
            {
                // 用于显示空列表/禁用状态/夷位已满等提示。
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
                        // 忽略
                    }
                });
            }

            RectTransform panelRect = TryFindCommonAncestorRect(mainText?.rectTransform,
                cancel?.GetComponent<RectTransform>());
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

            rowTemplate?.gameObject.SetActive(false);

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
        _canvasGroup?.interactable = false;
    }

    private void HideCardPickerOverlay()
    {
        _cardPickerRoot?.SetActive(false);
        _canvasGroup?.interactable = true;
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
            tag.ScrollRect?.gameObject.SetActive(false);

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

            tag.ScrollRect?.gameObject.SetActive(true);
        }

        // 网络交易下仅允许编辑本地报价。
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

        // 删除已报价的卡牌。
        HashSet<int> offered = new HashSet<int>();
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
            // 忽略
        }

        List<Card> candidates = deck.Where(c => c != null && !offered.Contains(c.InstanceId)).ToList();
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

            avatarImage?.gameObject.SetActive(false);
            exhibitIcon?.gameObject.SetActive(false);
            selectedIndicator?.SetActive(false);

            gameResultText?.text = card.Name;
            difficultyText?.text = card.Id;
            timestampText?.text = string.Empty;

            row.SetSelected(false, false);
            row.Click += () =>
            {
                AddCardToTrade(card, true);
                RefreshOfferEditorTexts();
                RebuildCardPickerList();
            };
        }
        catch
        {
            // 忽略
        }
    }

    // 标记组件：用于在运行时 overlay 下定位列表容器。
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

            // LBoL UI 通常基于 camera，但部分 prefab 可能如 overlay 行为。
            // 根据 camera 和 null 分别尝试以增强鲁棒性。
            try
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPoint, CameraController.UiCamera))
                {
                    return true;
                }
            }
            catch
            {
                // 忽略
            }

            return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPoint, null);
        }

        private static bool TryGetLeftClickThisFrame(out Vector2 screenPos)
        {
            screenPos = default;

            // 优先新 Input System（部分构建禁用了旧版 UnityEngine.Input API）。
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
                // 忽略
            }

            // 旧版输入备用方式。
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
                // 忽略
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

                // 忽略取消按钮点击。
                if (Panel._partnerPickerCancelButton != null)
                {
                    RectTransform cancelRt = Panel._partnerPickerCancelButton.transform as RectTransform;
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

                // 仅处理列表区域内的点击。
                if (listRegion != null && !Contains(listRegion, pos))
                {
                    return;
                }

                Transform container = pickerTag.ListContainer != null ? pickerTag.ListContainer : pickerTag.transform;

                if (container == null)
                {
                    return;
                }

                // 命中测试候选项，倒序遍历以从当前最高层开始。
                var candidates = container.GetComponentsInChildren<PartnerCandidateTag>(true);
                for (int i = candidates.Length - 1; i >= 0; i--)
                {
                    var cand = candidates[i];
                    if (cand == null || string.IsNullOrWhiteSpace(cand.PlayerId))
                    {
                        continue;
                    }

                    // 优先测试候选项根节点 rect。
                    RectTransform rt = cand.transform as RectTransform;
                    if (Contains(rt, pos))
                    {
                        Panel.OnPartnerSelected(cand.PlayerId, cand.PlayerName);
                        return;
                    }

                    // 备用：部分 prefab 根 rect 尺寸为零，需对其全部图形进行命中测试（TMP/Image/等）。
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
                // 忽略
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

                // 忽略对取消按钮的干扰。
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
                    // 忽略
                }

                PartnerPickerTag pickerTag = Panel._partnerPickerRoot.GetComponentInChildren<PartnerPickerTag>(true);
                if (pickerTag == null)
                {
                    return;
                }

                Transform container = pickerTag.ListContainer != null ? pickerTag.ListContainer : pickerTag.transform;

                if (container == null)
                {
                    return;
                }

                // 仅依矩形命中测试解析被点击的候选。
                var candidates = container.GetComponentsInChildren<PartnerCandidateTag>(true);
                foreach (var cand in candidates)
                {
                    if (cand == null || string.IsNullOrWhiteSpace(cand.PlayerId))
                    {
                        continue;
                    }

                    RectTransform rt = cand.transform as RectTransform;
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

                    // 备用：测试候选项的所有图形方块。
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
                // 忽略
            }
        }
    }

    private bool TryIsNetworkTrade(out bool localIsA)
    {
        localIsA = false;

        // 本地调试模式下抑制网络交易语义，以便自由操作 UI。
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
            bool localIsA = string.Equals(state.PlayerAId, _selfPlayerId, StringComparison.Ordinal);

            _playerAId = state.PlayerAId;
            _playerBId = state.PlayerBId;

            // 清空现有 UI
            ResetTradeData();
            _tradeId = state.TradeId;
            _selfPlayerId = NetworkIdentityTracker.GetSelfPlayerId();
            _playerAId = state.PlayerAId;
            _playerBId = state.PlayerBId;

            // 从 host 状态拉取本地金币/展品报价，保持 UI 一致。
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

        // 用户需求：永不禁用确认按钮，点击时再做逻辑守卫。
        if (confirmButton?.button != null)
        {
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
        List<TradeSyncPatch.CardRef> refs = offered
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
        _canvasGroup?.interactable = false;

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

            // 金币：严格核查（不足则失败）。
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

            // 展品：严格核查（未找到/不可交易/黑名单/重复 则失败）。
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

        // 避免对同一 preparing 阶段发送多次结果。
        if (state.Timestamp > 0 && _lastPreparingHandledTimestamp == state.Timestamp)
        {
            return;
        }

        _lastPreparingHandledTimestamp = state.Timestamp;

        bool localIsA = string.Equals(state.PlayerAId, _selfPlayerId, StringComparison.Ordinal);

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

        // 金币必须足够支付。
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

        // 展品必须存在且可交易。
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

            // 用户需求：已移除背景。

            if (!TryPickOfferEditorTemplates(out TextMeshProUGUI textTemplate, out CommonButtonWidget buttonTemplate))
            {
                Destroy(_offerEditorRoot);
                _offerEditorRoot = null;
                return;
            }

            // 卡牌行
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

            // 金币行
            var moneyLabel = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            moneyLabel.name = "MoneyLabel";
            moneyLabel.text = "金币:";
            moneyLabel.alignment = TextAlignmentOptions.Center;
            SetRect(moneyLabel.rectTransform, 0.02f, 0.38f, 0.25f, 0.63f);

            // 已持金币显示值（左下列表）：应展示玩家当前金币量（与顶栏一致）。
            _ownedMoneyText = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            _ownedMoneyText.name = "OwnedMoneyValue";
            _ownedMoneyText.text = "0";
            _ownedMoneyText.alignment = TextAlignmentOptions.Left;
            _ownedMoneyText.raycastTarget = false;
            SetRect(_ownedMoneyText.rectTransform, 0.25f, 0.38f, 0.50f, 0.63f);

            // 金币三元组：保持 '-' 和 '+' 在数字两侧等距。
            // 需求：数字位数变化时间距保持不变，三元组整体居中。
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

            // 金币 +/-：可点击文字（无背景）。
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
            _moneyValueText.raycastTarget = false; // 数字本身不可点击
            _moneyValueText.enableWordWrapping = false;
            _moneyValueText.overflowMode = TextOverflowModes.Overflow;
            _moneyValueBaseFontSize = _moneyValueText.fontSize;

            var plusBtn = CreateTextButton(textTemplate, _moneyTripletRoot.transform, "MoneyPlus", "+");
            plusBtn.onClick.RemoveAllListeners();
            plusBtn.onClick.AddListener(() =>
            {
                if (!CanEditOffer()) return;

                // 将金币报价封妖在持有金币量内；若无法可靠地获取持有金币，
                // 则允许增加以保持 UI 可用（确认时仍会验证负担能力）。
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

            // 展品行
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

            // 最终清理：将 button 模板层次带入的意外“取消”/多余按钮对象全部删除，仅保留已知 widget。
            PruneOfferEditorExtraButtons(_offerEditorRoot);

            EnsureOfferActionsOverlay(textTemplate);

            // 确保报价编辑器在框架视觉层上方，避免文字被 dialog mask 遮挡。
            // 在运行时布局中不与底部确认/取消按钮重叠。
            _offerEditorRoot?.transform.SetAsLastSibling();

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
                // 忽略
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
            // 保留报价编辑器稳定性：完整保留已创建控件的全部子树。
            // 删除单个 Button 对象可能意外破坏 widget 结构（尤其是模板包含多层嵌套 button/image 时）。
            HashSet<string> keepRoots = new HashSet<string>(StringComparer.Ordinal)
            {
                "CardsLabel",
                "CardsValue",
                "MoneyLabel",
                "OwnedMoneyValue",
                "MoneyTriplet",
                "ExLabel",
                "ExValue"
            };

            // 删除 offerRoot 下不属于我们的直接子节点。
            // （我们只创建直接子节点；模板内部元素保留在已索引的根节点下。）
            List<Transform> children = new List<Transform>();
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
                    Destroy(child.gameObject);
            }

            // 安全清理：在已保留的子树内，删除明显命名为 cancel/close 的对象。
            // 避免模板嵌入的红色圈/图标干扰。
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
                    // 只删除非根节点。
                    if (!keepRoots.Contains(n))
                        Destroy(t.gameObject);
                }
            }
        }
        catch
        {
            // 忽略
        }
    }

    private void ApplyMoneyValueSizingForDigits()
    {
        if (_moneyValueText == null)
        {
            return;
        }

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
            // 用户需求：数字 >= 4 位时仅缩小数字本身，最小保持 60%。
            float scale = Mathf.Clamp(3f / digits, 0.6f, 1f);
            _moneyValueText.fontSize = baseSize * scale;
        }
        else
        {
            _moneyValueText.fontSize = baseSize;
        }

        // 重建布局确保 '-' 和 '+' 在数字两侧等距。
        if (_moneyTripletRoot != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_moneyTripletRoot.transform as RectTransform);
    }

    private bool TryGetOwnedMoney(out int ownedMoney)
    {
        try
        {
            int best = 0;

            // 主来源：GameRun.Money（在此面板其他地方也使用）。
            best = Math.Max(best, GameRun?.Money ?? 0);

            // 备用方1：Player 可能暴露金币相关属性。
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
                // 忽略
            }

            // 备用方2：GameMaster.CurrentGameRun（部分 UI 上下文使用这个）。
            try
            {
                var gm = GameMaster.Instance;
                if (gm != null)
                {
                    var t = gm.GetType();
                    var prop = t.GetProperty("CurrentGameRun", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var run = prop?.GetValue(gm);
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
                // 忽略
            }

            if (best < 0)
            {
                best = 0;
            }

            ownedMoney = best;
            // 只有在至少有已知运行上下文时，才将 0 视为有效。
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

            // 右下区域（绿色框区域），与详情列对齐，保持与报价编辑器的间距一致。
            rt.anchorMin = new Vector2(0.80f, 0.08f);
            rt.anchorMax = new Vector2(0.98f, 0.20f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _offerActionsPickCardsBtn = CreateTextButton(textTemplate, _offerActionsRoot.transform, "PickCards", "选择卡牌");
            var cardsRt = _offerActionsPickCardsBtn.GetComponent<RectTransform>();
            SetRect(cardsRt, 0f, 0.45f, 1f, 1f); // 55%/45%
            {
                var tmp = _offerActionsPickCardsBtn.GetComponent<TextMeshProUGUI>();
                tmp?.alignment = TextAlignmentOptions.Left;
            }

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
            {
                var tmp = _offerActionsPickExhibitsBtn.GetComponent<TextMeshProUGUI>();
                tmp?.alignment = TextAlignmentOptions.Left;
            }

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

            // 初始化时同步报价编辑器的显示状态。
            _offerActionsRoot?.SetActive(_offerEditorRoot != null && _offerEditorRoot.activeSelf);
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
                // 忽略
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
            _rt?.localScale = Vector3.one;
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
        // 从游戏内模板克隆 TMP，保证字体/材质与原生 UI 一致。
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
        var c = tmp.color;
        c.a = 1f;
        tmp.color = c;

        var btn = tmp.gameObject.AddComponent<Button>();
        btn.targetGraphic = tmp;
        btn.transition = Selectable.Transition.ColorTint;

        // 使用与游戏内「可点击文字”一致的亮色 hover 效果。
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

        var nav = btn.navigation;
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;

        _ = tmp.gameObject.AddComponent<TextButtonHover>();
        return btn;
    }

    private bool TryPickOfferEditorTemplates(out TextMeshProUGUI textTemplate, out CommonButtonWidget buttonTemplate)
    {
        textTemplate = null;
        buttonTemplate = null;

        // 首选复用 vanilla dialog prefab 模板，以保持 TMP 字体/材质与原生 UI 一致。
        try
        {
            GameObject dialogPrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (dialogPrefab != null)
            {
                var dialog = dialogPrefab.GetComponentInChildren<MessageDialog>(true);
                if (dialog != null)
                {
                    textTemplate = GetDialogField<TextMeshProUGUI>(dialog, "mainText")
                                   ?? GetDialogField<TextMeshProUGUI>(dialog, "subText")
                                   ?? dialogPrefab.GetComponentInChildren<TextMeshProUGUI>(true);

                    // 优先 singleConfirmButton（通常为金/木色确认按钮）或 confirmButton，最后才用 cancelButton。
                    Button targetButton = GetDialogField<Button>(dialog, "singleConfirmButton")
                                       ?? GetDialogField<Button>(dialog, "confirmButton")
                                       ?? GetDialogField<Button>(dialog, "cancelButton");

                    buttonTemplate = TryResolveCommonButtonWidget(targetButton);
                }
                else
                {
                    textTemplate = dialogPrefab.GetComponentInChildren<TextMeshProUGUI>(true);
                }

                // 备用启发式地：若具体字段未找到，按得分选择最佳自定义按钮。
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

                        int buttons = w.GetComponentsInChildren<Button>(true).Length;
                        int nodes = w.GetComponentsInChildren<Transform>(true).Length;

                        if (buttons <= 0)
                        {
                            continue;
                        }

                        int score = (buttons * 1000) + nodes;

                        // 报价编辑器高度优先選 confirm/singleConfirm 按钮外观。
                        if (w.name.IndexOf("confirm", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            score -= 500;
                        }
                        // 严格排除名为 "cancel" 的任何对象。
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

                // 最终守卫：报价编辑器需要单一 button widget。
                buttonTemplate = PreferSingleButtonWidget(buttonTemplate);
            }
        }
        catch
        {
            // 忽略
        }

        // 备用：使用面板内已有引用（仍是游戏 UI，但兼容性稍差）。优先 confirmButton 而非 cancelButton。
        textTemplate ??= statusText != null ? statusText : player1NameText;
        buttonTemplate ??= confirmButton != null ? confirmButton : cancelButton;

        buttonTemplate = PreferSingleButtonWidget(buttonTemplate);

        return textTemplate != null && buttonTemplate != null;
    }

    private static void NormalizeButtonWidget(CommonButtonWidget widget)
    {
        if (widget == null)
        {
            return;
        }

        // 确保只有主按钮保持可交互。
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

                // 优先硬删除：模板带入的额外按钮不应存在于运行时 widget。
                Destroy(b.gameObject);
            }
        }

        // 禁用 tooltip / 额外 pointer handler。
        DisableTooltipBehaviours(widget.gameObject);

        // 禁用 gamepad cursor / 额外视觉效果。
        DisableCursorBehaviours(widget.gameObject);

        // 强制设置 behavior 为 Open（0，通常为非红色）。
        try
        {
            Traverse traverse = HarmonyLib.Traverse.Create(widget);
            traverse.Field("buttonBehavior").SetValue(0);
        }
        catch
        {
            // 忽略
        }
    }

    private static CommonButtonWidget TryResolveCommonButtonWidget(Button target)
    {
        // 优先选择明确引用该 Button 的最近 widget。
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

    private static CommonButtonWidget PreferSingleButtonWidget(CommonButtonWidget template)
    {
        if (template == null)
        {
            return null;
        }

        int buttons = template.GetComponentsInChildren<Button>(true).Length;
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

            // 跳过名为 "cancel" 的对象。
            if (!string.IsNullOrWhiteSpace(w.name) && w.name.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            int wButtons = w.GetComponentsInChildren<Button>(true).Length;
            if (wButtons != 1)
            {
                continue;
            }

            int nodes = w.GetComponentsInChildren<Transform>(true).Length;
            if (nodes < bestNodes)
            {
                bestNodes = nodes;
                best = w;
            }
        }

        return best ?? template;
    }

    private static void DisableCursorBehaviours(GameObject root)
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

            // 使用名称匹配来避免对可选包的硬引用。
            var n = behaviour.GetType().Name;
            if (string.IsNullOrWhiteSpace(n))
            {
                continue;
            }

            if (n.IndexOf("Gamepad", StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Cursor", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // 不禁用 Button 本身。
                if (behaviour is Button)
                    continue;

                behaviour.enabled = false;
            }
        }
    }

    private static void DisableTooltipBehaviours(GameObject root)
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

    private bool CanEditOffer()
    {
        if (_isApplyingState || _partnerPickerActive)
        {
            return false;
        }

        // 本地调试会话始终允许编辑。
        if (_localDebugTradeMode)
        {
            return true;
        }

        TradeSyncPatch.TradeSessionState state = TradeSyncPatch.GetLastKnown(_tradeId);
        return state == null || state.Status == TradeSyncPatch.TradeStatus.Open;
    }

    private void RefreshOfferEditorTexts()
    {
        _cardCountText?.text = (_player1OfferedCards?.Count ?? 0).ToString();

        if (_ownedMoneyText != null)
        {
            if (!TryGetOwnedMoney(out int owned))
            {
                owned = GameRun?.Money ?? 0;
            }
            if (owned < 0) owned = 0;
            _ownedMoneyText.text = owned.ToString();
        }

        if (_moneyValueText != null)
        {
            _moneyValueText.text = _localMoneyOffer.ToString();
            ApplyMoneyValueSizingForDigits();
        }

        _exhibitValueText?.text = _localExhibitOfferIds.Count.ToString();
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
        _exhibitPickerRoot?.SetActive(false);
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

            RectTransform subTextRect = subText?.rectTransform;

            if (mainText != null)
            {
                mainText.text = "选择要交易的展品";
                mainText.alignment = TextAlignmentOptions.Center;
                mainText.raycastTarget = false;
            }

            if (subText != null)
            {
                // 用于显示空列表/禁用状态/夷位已满等提示。
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
                        // 忽略
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
                        // 忽略
                    }
                });
            }

            RectTransform panelRect = TryFindCommonAncestorRect(mainText?.rectTransform,
                cancel?.GetComponent<RectTransform>());
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

            rowTemplate?.gameObject.SetActive(false);

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
            tag.ScrollRect?.gameObject.SetActive(false);

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

            tag.ScrollRect?.gameObject.SetActive(true);
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
            exhibitIcon?.gameObject.SetActive(false);
            gameResultText?.text = exhibit.Name;
            difficultyText?.text = exhibit.Id;
            timestampText?.text = string.Empty;

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
                    // 忽略
                }
            };
        }
        catch
        {
            // 忽略
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
                // 忽略
            }
        }
    }

    private static void SetButtonText(CommonButtonWidget button, string label)
    {
        if (button == null)
            return;

        var tmp = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp != null)
        {
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
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
            _panel?._isApplyingState = _prev;
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
