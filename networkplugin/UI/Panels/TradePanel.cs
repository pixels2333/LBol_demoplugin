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
using UnityEngine.UI;

namespace NetworkPlugin.UI.Panels;

/// <summary>
/// 交易面板类，处理玩家之间的物品（卡牌）交易界面与逻辑。
/// </summary>
public partial class TradePanel : UiPanel<TradePayload>, IInputActionHandler
{
    #region 常量

    /// <summary>
    /// 默认最大交易卡牌槽位数量。
    /// </summary>
    private const int DefaultMaxTradeSlots = 3;

    /// <summary>
    /// 交易完成后等待多少秒再关闭界面。
    /// </summary>
    private const float TradeCompleteWaitTime = 2f;

    private const int MaxMoneyOffer = 99999;

    // 通过反射读取本地化总表，避免直接调用 key.Localize() 在 key 缺失时产生日志噪声。
    private static readonly FieldInfo LocalizationTableField =
        typeof(Localization).GetField("LocalizationTable", BindingFlags.NonPublic | BindingFlags.Static);

    private GameRunController ActiveGameRun
        => GameRun ?? GameStateUtils.GetCurrentGameRun();

    private bool IsPlayerA(TradeSyncPatch.TradeSessionState state)
        => state != null && string.Equals(state.PlayerAId, _selfPlayerId, StringComparison.Ordinal);

    #endregion

    #region 序列化字段（UI 组件）

    [SerializeField]
    private RectTransform player1TradeArea;

    [SerializeField]
    private RectTransform player2TradeArea;

    [SerializeField]
    private TradeSlotWidget[] player1Slots;

    [SerializeField]
    private TradeSlotWidget[] player2Slots;

    [SerializeField]
    private CommonButtonWidget confirmButton;

    [SerializeField]
    private CommonButtonWidget cancelButton;

    [SerializeField]
    private TextMeshProUGUI statusText;

    [SerializeField]
    private TextMeshProUGUI player1NameText;

    [SerializeField]
    private TextMeshProUGUI player2NameText;

    #endregion

    #region 内部状态字段

    private readonly List<Card> _player1OfferedCards = new List<Card>();
    private readonly List<Card> _player2OfferedCards = new List<Card>();

    private string _tradeId;
    private string _selfPlayerId;
    private string _playerAId;
    private string _playerBId;
    private bool _isApplyingState;
    private bool _subscribedToTrade;

    // 当 true 时，TradePanel 已将控制权交给 TradeDetailDialog，不应再发起网络请求或初始化面板内编辑器。
    private bool _handoffToDetailDialog;

    private TradePayload _payload;
    private int _maxTradeSlots = DefaultMaxTradeSlots;
    private CanvasGroup _canvasGroup;
    private bool _canCancel = true;

    // v2 本地报价（金币 + 展品），卡牌存储于 _player1OfferedCards。
    private int _localMoneyOffer;
    private readonly HashSet<string> _localExhibitOfferIds = new HashSet<string>(StringComparer.Ordinal);

    // 运行时展品预览容器（位于 player1TradeArea / player2TradeArea 内）。
    private GameObject _localExhibitContainer;
    private GameObject _remoteExhibitContainer;
    private ExhibitWidget _exhibitIconTemplate;

    // 状态转换时用于记录上一次已知状态（Preparing 阶段验证）。
    private TradeSyncPatch.TradeStatus? _lastTradeStatus;
    private long _lastPreparingHandledTimestamp;

    // 运行时 partner picker overlay（精简构建，避免 prefab 依赖）。
    private GameObject _partnerPickerRoot;
    private bool _partnerPickerActive;
    private float _partnerPickerInputReadyTime;
    private string _partnerPickerBuildError;
    private Button _partnerPickerCancelButton;

    // 打开面板后自动刷新一次 partner 列表（应对位置元数据稍晚到达的情况）。
    private Coroutine _partnerPickerAutoRefreshCo;

    // 标记本面板是否已将自身压入 UiManager 的 action handler 栈。
    private bool _actionHandlerPushed;

    // 当 true 时，中央正在显示模态提示对话框，交易详情被屏蔽。
    private bool _blockingCenterMessageActive;

    // 离线/本地调试模式：允许在无服务器时打开并操作 TradePanel。
    private bool _localDebugTradeMode;

    // 运行时报价编辑器 overlay（金币 + 展品）。
    private GameObject _offerEditorRoot;
    private GameObject _offerActionsRoot;
    private GameObject _moneyTripletRoot;
    private float _moneyValueBaseFontSize;
    private TextMeshProUGUI _ownedMoneyText;
    private TextMeshProUGUI _moneyValueText;
    private TextMeshProUGUI _exhibitValueText;

    // 运行时卡牌选择器 overlay（卡组卡牌）。
    private GameObject _cardPickerRoot;
    private bool _cardPickerApplyingSelection;
    private TextMeshProUGUI _cardCountText;

    // 主面板结果展示区：位于状态文案下方，展示双方已选卡牌。
    private GameObject _offerPreviewRoot;
    private OfferPreviewPanelTag _localOfferPreviewPanel;
    private OfferPreviewPanelTag _remoteOfferPreviewPanel;
    private CardWidget _offerPreviewCardTemplate;
    private RectTransform _runtimeContentRoot;

    internal void BindRuntimeUi(
        RectTransform runtimeContentRoot,
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
        _runtimeContentRoot = runtimeContentRoot;
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
        if (confirmButton?.button is not null)
        {
            confirmButton.button.onClick.RemoveAllListeners();
            confirmButton.button.onClick.AddListener(OnConfirmTrade);

            // 用户需求：确认按钮使用 Open behavior + Normal weight.
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

    private Transform GetTradePanelContentParent()
        => _runtimeContentRoot is not null ? _runtimeContentRoot.transform : transform;

    #endregion

    #region UiPanel 属性

    public override PanelLayer Layer => PanelLayer.Top;

    #endregion

    #region Unity 生命周期

    public void Awake()
    {
        // 获取或添加 CanvasGroup，用于控制面板交互
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup ??= gameObject.AddComponent<CanvasGroup>();

        // 注册按钮点击事件
        confirmButton?.button?.onClick.AddListener(OnConfirmTrade);
        cancelButton?.button?.onClick.AddListener(OnCancelTrade);
    }

    #endregion

    #region 多语言

    public override void OnLocaleChanged()
    {
        // 语言切换时刷新界面文本（如果当前有有效的 payload）
        if (_payload is not null)
        {
            UpdateUIStrings();
        }
    }

    #endregion

    #region 面板生命周期

    protected override void OnShowing(TradePayload payload)
    {
        Plugin.Logger?.LogInfo($"[TradePanel] OnShowing enter: payloadNull={(payload is null)}, activeGameRun={(ActiveGameRun is not null)}");
        EnsurePopupTopmost();

        // 本地 UI 测试：调试开关启用时允许在无服务器情况下打开面板。
        bool networkConnected = TryEnsureNetworkConnected();
        _localDebugTradeMode = !networkConnected && IsLocalDebugTradeAllowed();
        if (!networkConnected && !_localDebugTradeMode)
        {
            Plugin.Logger?.LogWarning("[TradePanel] OnShowing aborted: network unavailable.");
            Hide();
            return;
        }

        // 提前注册输入处理器，确保在设置期间弹出的 MessageDialog 能正确压栈。
        // （否则 MessageDialog/TradePanel Push/Pop 顺序可能错误，UiManager 会记录错误日志。）
        UiManager.PushActionHandler(this);
        _actionHandlerPushed = true;

        // 重要：在可能提前返回之前（即显示 partner picker 或 modal dialog 前）确保面板可交互。
        // 否则如果面板在上次隐藏后再次打开，picker 可能显示但无法点击。
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

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
        EnsureOfferPreviewOverlay();

        // 设置玩家名称显示（不使用 Player 1/2 之类的占位文本）
        if (player1NameText is not null) player1NameText.text = ResolveLocalPlayerDisplayName(payload);
        if (player2NameText is not null) player2NameText.text = ResolvePartnerDisplayName(payload);

        // 根据配置显示/隐藏取消按钮
        cancelButton?.gameObject.SetActive(_canCancel);

        // 刷新本地化文案
        UpdateUIStrings();
    }

    protected override void OnShown()
    {
        // 面板显示完成后再次置顶，避免与 GapOptionsPanel/OptionWidget 的 sibling 顺序竞争。
        EnsurePopupTopmost();
    }

    protected override void OnHiding()
    {
        // 隐藏动画开始时禁用交互
        _canvasGroup.interactable = false;

        // 取消注册输入处理器
        if (_actionHandlerPushed)
        {
            UiManager.PopActionHandler(this);
            _actionHandlerPushed = false;
        }

        TryUnsubscribeTradeEvents();
    }

    protected override void OnHided()
    {
        // 完全隐藏后重置数据并清空 payload
        ResetTradeData();
        _payload = null;
    }

    #endregion

    #region UI 文本与状态

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
        if (string.IsNullOrWhiteSpace(key))
        {
            return fallback;
        }

        // 部分模组包可能未包含这些本地化 key。
        // 若 key 缺失，直接回退，不触发 Localization.Localize 的 not found 报错。
        if (TryGetLocalizedStringQuiet(key, out string localized))
        {
            return localized;
        }

        return fallback;
    }

    private static bool TryGetLocalizedStringQuiet(string key, out string localized)
    {
        localized = null;

        try
        {
            if (LocalizationTableField?.GetValue(null) is not IDictionary table)
            {
                return false;
            }

            if (!table.Contains(key))
            {
                return false;
            }

            if (table[key] is not string raw || string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            try
            {
                localized = StringDecorator.Decorate(raw);
            }
            catch
            {
                localized = raw;
            }

            return !string.IsNullOrWhiteSpace(localized);
        }
        catch
        {
            return false;
        }
    }

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
        if (player1NameText is not null) player1NameText.text = string.Empty;
        if (player2NameText is not null) player2NameText.text = string.Empty;

        // 隐藏所有活跃的 overlay。
        _partnerPickerActive = false;
        _blockingCenterMessageActive = false;
        _partnerPickerRoot?.SetActive(false);

        if (_partnerPickerAutoRefreshCo is not null)
        {
            StopCoroutine(_partnerPickerAutoRefreshCo);
            _partnerPickerAutoRefreshCo = null;
        }
        _cardPickerRoot?.SetActive(false);
        _exhibitPickerRoot?.SetActive(false);

        // 清空玩家1所有交易槽的显示
        player1Slots?.ToList().ForEach(s => s?.ClearSlot());

        // 清空玩家2所有交易槽的显示
        player2Slots?.ToList().ForEach(s => s?.ClearSlot());

        ClearOfferPreviewPanel(_localOfferPreviewPanel);
        ClearOfferPreviewPanel(_remoteOfferPreviewPanel);

        // 默认禁止点击确认按钮，直到双方都放入了卡牌
        if (confirmButton?.button is not null)
        {
            confirmButton.button.interactable = false;
        }

        RefreshOfferEditorTexts();
    }

    private void UpdateUIStatus(string message)
    {
        if (statusText is not null) statusText.text = message;
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
        if (card is null)
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
            RefreshOfferPreview();
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
        if (card is null)
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
            if (slots is not null && offeredCards.Count < slots.Length)
            {
                slots[offeredCards.Count]?.ClearSlot();
            }

            RefreshOfferPreview();
            CheckTradeReady();

            if (!_isApplyingState)
            {
                TrySendOfferUpdate();
            }
        }
    }

    private void UpdateTradeSlot(Card card, TradeSlotWidget[] slots, int index)
    {
        if (slots is not null && index >= 0 && index < slots.Length && slots[index] is not null)
        {
            bool isPlayer1 = slots == player1Slots;
            slots[index].SetCard(card, (c) => RemoveCardFromTrade(c, isPlayer1));
        }
    }

    private void CheckTradeReady()
    {
        // v2：允许交易任意资产（卡牌/道具/金币/展品）。
        bool localHasOffer = (_player1OfferedCards?.Count ?? 0) > 0 || _localMoneyOffer > 0 || _localExhibitOfferIds.Count > 0;
        bool remoteHasOffer = (_player2OfferedCards?.Count ?? 0) > 0;
        bool bothPlayersReady = localHasOffer && remoteHasOffer;

        // 用户需求：确认按钮永不置灰，点击服务端未就绪时会显示状态提示但不发送确认。
        if (confirmButton?.button is not null)
        {
            confirmButton.button.interactable = true;
        }

        // 更新提示文本
        if (bothPlayersReady)
        {
            UpdateUIStatus(TryLocalize("Trade.ReadyToConfirm", "可以确认交易"));
        }
        else
        {
            UpdateUIStatus(TryLocalize("Trade.WaitingForItems", "等待放入物品..."));
        }
    }

    private static bool HasOffer(TradeSyncPatch.TradeSessionState state, bool isOfferA)
        => (isOfferA ? state.OfferA : state.OfferB)?.Count > 0
        || (isOfferA ? state.MoneyA : state.MoneyB) > 0
        || (isOfferA ? state.ExhibitsA : state.ExhibitsB)?.Count > 0;

    #endregion

    #region 按钮与输入处理

    private void OnConfirmTrade()
    {
        if (TryIsNetworkTrade(out _))
        {
            // v2：双方都提供了任意资产（卡牌/道具/金币/展品）即可确认。
            // 远端报价来自 host 状态，不一定来自 _player2OfferedCards。
            TradeSyncPatch.TradeSessionState state = TradeSyncPatch.GetLastKnown(_tradeId);
            if (state is null)
            {
                return;
            }

            bool localIsA = IsPlayerA(state);
            bool localHasOffer = HasOffer(state, localIsA);
            bool remoteHasOffer = HasOffer(state, !localIsA);

            if (!localHasOffer || !remoteHasOffer)
            {
                UpdateUIStatus(TryLocalize("Trade.WaitingForItems", "等待放入物品..."));
                return;
            }

            UpdateUIStatus("Trade.Confirmed".Localize());
            if (TryIsNetworkTrade(out _))
            {
                TradeSyncPatch.RequestConfirm(_tradeId, _selfPlayerId);
            }
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

    private void OnCancelTrade()
    {
        if (TryIsNetworkTrade(out _))
        {
            TradeSyncPatch.RequestCancel(_tradeId, _selfPlayerId);
        }

        Hide();
    }

    public void OnCancel()
    {
        if (_cardPickerRoot is not null && _cardPickerRoot.activeSelf)
        {
            HideCardPickerOverlay();
            return;
        }

        // 输入事件层面的取消处理，需判断当前是否允许取消
        if (_canCancel)
        {
            OnCancelTrade();
        }
    }

    #endregion

    #region 协程与网络同步

    private IEnumerator ExecuteTrade()
    {
        GameRunController run = ActiveGameRun;

        // 联机：该协程仅用于“交易完成后本地落地”。
        if (TryIsNetworkTrade(out _))
        {
            bool isA = string.Equals(_selfPlayerId, _playerAId, StringComparison.Ordinal);
            yield return ApplyNetworkTradeAndClose(isA);
            yield break;
        }

        if (run is null)
        {
            UpdateUIStatus("Trade.Failed".Localize());
            Plugin.Logger?.LogWarning("[TradePanel] ExecuteTrade aborted: ActiveGameRun is null.");
            yield break;
        }

        // 禁用按钮以防止重复点击触发多次交易
        if (confirmButton?.button is not null)
        {
            confirmButton.button.interactable = false;
        }
        if (cancelButton?.button is not null)
        {
            cancelButton.button.interactable = false;
        }

        // 将玩家1提供的卡牌从其卡组移除并加入到玩家2（当前实现视为本地玩家）
        _player1OfferedCards.ForEach(card =>
        {
            run.RemoveDeckCard(card, false);
            run.AddDeckCard(card, true, new VisualSourceData
            {
                SourceType = VisualSourceType.CardSelect
            });
        });

        // 将玩家2提供的卡牌加入到玩家1侧（目前仅做本地添加）
        _player2OfferedCards.ForEach(card =>
        {
            run.AddDeckCard(card, true, new VisualSourceData
            {
                SourceType = VisualSourceType.CardSelect
            });
        });

        // 更新状态为"交易完成"
        UpdateUIStatus("Trade.Completed".Localize());

        // 发送网络事件通知其他玩家本次交易已经完成
        // 等待一小段时间，让玩家看清结果
        yield return new WaitForSeconds(TradeCompleteWaitTime);
        Hide();
    }

    private void SetupTradeSession(TradePayload payload)
    {
        try
        {
            INetworkClient client = ModService.ServiceProvider.GetService<INetworkClient>();
            bool connected = client is not null && client.IsConnected;
            Plugin.Logger?.LogInfo($"[TradePanel] SetupTradeSession enter: connected={connected}, tradeId={payload?.TradeId ?? "<null>"}");
            if (connected)
            {
                NetworkIdentityTracker.EnsureSubscribed(client);
                TradeSyncPatch.EnsureSubscribed(client);

                _selfPlayerId = NetworkIdentityTracker.GetSelfPlayerId();
                if (string.IsNullOrWhiteSpace(_selfPlayerId))
                {
                    Plugin.Logger?.LogWarning("[TradePanel] SetupTradeSession aborted: self player id empty.");
                    return;
                }
            }
            else
            {
                _selfPlayerId = "local";
            }

            _tradeId = payload?.TradeId;
            _tradeId ??= Guid.NewGuid().ToString("N");
            _playerAId = payload?.Player1Id ?? _selfPlayerId;
            _playerBId = payload?.Player2Id;

            // 需求：无法明确确定 partner 时必须显示 partner picker UI。
            if (string.IsNullOrWhiteSpace(_playerBId) || string.Equals(_playerBId, _selfPlayerId, StringComparison.Ordinal))
            {
                Plugin.Logger?.LogInfo($"[TradePanel] SetupTradeSession: partner unresolved, showing picker. self={_selfPlayerId ?? "<null>"}, playerB={_playerBId ?? "<null>"}");
                ShowPartnerPickerOverlay();
                return;
            }

            // 已连接：请求 host 驱动的交易会话。离线/本地调试：跳过网络。
            if (connected)
            {
                if (TryShowTradeDetailDialog(_playerBId, payload?.Player2Name))
                {
                    Plugin.Logger?.LogInfo($"[TradePanel] SetupTradeSession: handed off to TradeDetailDialog, tradeId={_tradeId}");
                    _handoffToDetailDialog = true;
                    Hide(false);
                    return;
                }

                Plugin.Logger?.LogInfo($"[TradePanel] SetupTradeSession: dialog unavailable, fallback to panel. tradeId={_tradeId}");
                TrySubscribeTradeEvents();
                TradeSyncPatch.RequestStartTrade(_tradeId, _playerAId, _playerBId, _maxTradeSlots);
                TradeSyncPatch.RequestSnapshot(_tradeId, _selfPlayerId);
            }
            else
            {
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
            if (dialog is null)
            {
                return false;
            }

            _tradeId ??= Guid.NewGuid().ToString("N");

            GameRunController activeRun = ActiveGameRun;
            Plugin.Logger?.LogInfo($"[TradePanel] TryShowTradeDetailDialog: tradeId={_tradeId}, self={_selfPlayerId}, partner={partnerPlayerId}");

            dialog.Show(new TradeDetailPayload
            {
                TradeId = _tradeId,
                SelfPlayerId = _selfPlayerId,
                PartnerPlayerId = partnerPlayerId,
                PartnerPlayerName = OtherPlayersOverlayPatch.ResolveDisplayName(partnerPlayerId, partnerPreferredName, isLocal: false),
                MaxTradeSlots = _maxTradeSlots,
                InitialDeckCards = activeRun?.BaseDeck?.Where(c => c is not null).ToList() ?? new List<Card>()
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
        if (client is null || !client.IsConnected)
        {
            Plugin.Logger?.LogWarning("[TradePanel] TryEnsureNetworkConnected failed.");
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

    #endregion
}
