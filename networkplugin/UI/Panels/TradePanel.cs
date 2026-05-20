using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation;
using LBoL.Presentation.I10N;
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

            // 需求：无法明确确定 partner 时必须显示 partner picker UI.
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

    private void ShowPartnerPickerOverlay()
    {
        if (_partnerPickerActive)
        {
            Plugin.Logger?.LogInfo("[TradePanel] ShowPartnerPickerOverlay skipped: picker already active.");
            return;
        }

        Plugin.Logger?.LogInfo($"[TradePanel] ShowPartnerPickerOverlay enter: tradeId={_tradeId ?? "<null>"}, self={_selfPlayerId ?? "<null>"}, currentPartner={_playerBId ?? "<null>"}");

        // 确保 overlay 获得点击响应（即使面板之前被隐藏过）。
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        // 懒创建 overlay。
        EnsurePartnerPickerOverlay();
        if (_partnerPickerRoot is null)
        {
            Plugin.Logger?.LogWarning($"[TradePanel] ShowPartnerPickerOverlay failed: picker root missing, buildError={_partnerPickerBuildError ?? "<null>"}");
            ShowTradeTargetUnavailableDialog(_partnerPickerBuildError);
            return;
        }

        _partnerPickerActive = true;
    _partnerPickerInputReadyTime = Time.unscaledTime + 0.15f;
        _partnerPickerRoot.SetActive(true);
        EnsurePopupTopmost();
        // 隐藏外层 Frame，避免与 picker 重叠。
        transform.Find("NetworkPlugin_TradePanel_Frame")?.gameObject.SetActive(false);
    Plugin.Logger?.LogInfo($"[TradePanel] ShowPartnerPickerOverlay armed click guard until={_partnerPickerInputReadyTime:F3}");

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
            Plugin.Logger?.LogInfo("[TradePanel] ShowPartnerPickerOverlay: self location unavailable, waiting for auto refresh.");
            TryShowPartnerPickerWaiting();
            if (_partnerPickerAutoRefreshCo is not null)
            {
                StopCoroutine(_partnerPickerAutoRefreshCo);
                _partnerPickerAutoRefreshCo = null;
            }
            _partnerPickerAutoRefreshCo = StartCoroutine(CoPartnerPickerAutoRefreshOnce());
        }
    }

    private void TryShowPartnerPickerWaiting()
    {
        if (_partnerPickerRoot is null)
        {
            return;
        }

        PartnerPickerTag tag = _partnerPickerRoot.GetComponentInChildren<PartnerPickerTag>(true);
        if (tag is null)
        {
            return;
        }

        tag.ScrollRect?.gameObject.SetActive(false);

        if (tag.EmptyText is not null)
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
            if (!_partnerPickerActive || _partnerPickerRoot is null)
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

        if (_partnerPickerActive && _partnerPickerRoot is not null)
        {
            RebuildPartnerPickerList();
        }

        _partnerPickerAutoRefreshCo = null;
    }

    private static void ForceEnableRaycasts(GameObject root)
    {
        if (root is null)
        {
            return;
        }

        root.GetComponentsInChildren<CanvasGroup>(true).ToList().ForEach(cg =>
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.ignoreParentGroups = true;
        });
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
        SetCanvasInteractable(false);

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
        if (_partnerPickerAutoRefreshCo is not null)
        {
            StopCoroutine(_partnerPickerAutoRefreshCo);
            _partnerPickerAutoRefreshCo = null;
        }

        _partnerPickerActive = false;
    _partnerPickerInputReadyTime = 0f;
        _partnerPickerRoot?.SetActive(false);

        // 恢复外层 Frame。
        transform.Find("NetworkPlugin_TradePanel_Frame")?.gameObject.SetActive(true);

        // 重新开启交易 UI。
        SetTradeDetailsVisible(true);

        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        // 恢复底层取消按钮状态。
        cancelButton?.gameObject.SetActive(_canCancel);
    }

    private void EnsurePopupTopmost()
    {
        transform.SetAsLastSibling();
        _partnerPickerRoot?.transform.SetAsLastSibling();
        _cardPickerRoot?.transform.SetAsLastSibling();
        _exhibitPickerRoot?.transform.SetAsLastSibling();
        _offerPreviewRoot?.transform.SetAsLastSibling();
        _offerEditorRoot?.transform.SetAsLastSibling();
        _offerActionsRoot?.transform.SetAsLastSibling();
    }

    private void SetTradeDetailsVisible(bool visible)
    {
        statusText?.gameObject.SetActive(visible);

        player1TradeArea?.gameObject.SetActive(visible);
        player2TradeArea?.gameObject.SetActive(visible);
        player1NameText?.gameObject.SetActive(visible);
        player2NameText?.gameObject.SetActive(visible);

        // 隐藏确认按钮以减少 session 开始前的视觉混乱，取消按钮在允许取消时保持可见。
        confirmButton?.gameObject.SetActive(visible);
        cancelButton?.gameObject.SetActive(visible || _canCancel);

        _offerPreviewRoot?.SetActive(visible);
        _offerEditorRoot?.SetActive(visible);
        _offerActionsRoot?.SetActive(visible);
    }

    private void SetCanvasInteractable(bool interactable)
    {
        if (_canvasGroup is null) return;
        _canvasGroup.interactable = interactable;
        _canvasGroup.blocksRaycasts = interactable;
    }

    private void EnsurePartnerPickerOverlay()
    {
        if (_partnerPickerRoot is not null)
        {
            return;
        }

        _partnerPickerBuildError = null;
        try
        {
            Transform parent = transform;

            // 严格要求：使用游戏内 dialog prefab 作为 overlay 窗口框架，而非运行时构建的 Image/Outline。
            GameObject prefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (prefab is null)
            {
                _partnerPickerBuildError = "无法加载 UI/Dialogs/MessageDialog";
                _partnerPickerRoot = null;
                return;
            }

            _partnerPickerRoot = Instantiate(prefab, parent, false);
            _partnerPickerRoot.name = "TradePartnerPicker";
            _partnerPickerRoot.SetActive(false);

            RectTransform rootRect = _partnerPickerRoot.GetComponent<RectTransform>();
            if (rootRect is not null)
            {
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.offsetMin = Vector2.zero;
                rootRect.offsetMax = Vector2.zero;
            }

            MessageDialog dialog = _partnerPickerRoot.GetComponentInChildren<MessageDialog>(true);
            if (dialog is null)
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

            if (mainText is not null)
            {
                mainText.text = "请选择交易对象";
                mainText.alignment = TextAlignmentOptions.Center;
                mainText.raycastTarget = false;
            }

            if (subText is not null)
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
            if (singleConfirm is not null)
            {
                singleConfirm.onClick.RemoveAllListeners();
                Destroy(singleConfirm.gameObject);
            }
            if (confirm is not null)
            {
                confirm.onClick.RemoveAllListeners();
                confirm.gameObject.SetActive(true);

                var refreshLabel = confirm.GetComponentInChildren<TextMeshProUGUI>(true);
                if (refreshLabel is not null)
                {
                    refreshLabel.text = "刷新";
                    refreshLabel.alignment = TextAlignmentOptions.Center;
                }

                confirm.onClick.AddListener(OnPartnerPickerRefreshClicked);
            }

            if (cancel is not null)
            {
                cancel.onClick.RemoveAllListeners();
                cancel.gameObject.SetActive(true);

                _partnerPickerCancelButton = cancel;

                var cancelLabel = cancel.GetComponentInChildren<TextMeshProUGUI>(true);
                if (cancelLabel is not null)
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
            if (catcher is null)
            {
                catcher = _partnerPickerRoot.AddComponent<PartnerPickerClickCatcher>();
            }
            catcher.Panel = this;

            RectTransform panelRect = TryFindCommonAncestorRect(mainText?.rectTransform,
                cancel?.GetComponent<RectTransform>());
            if (panelRect is null)
            {
                panelRect = mainText is not null ? mainText.rectTransform.parent as RectTransform : null;
            }
            if (panelRect is null)
            {
                panelRect = rootRect;
            }
            if (panelRect is null)
            {
                Destroy(_partnerPickerRoot);
                _partnerPickerRoot = null;
                _partnerPickerBuildError = "找不到可挂载列表的面板 RectTransform";
                return;
            }

            // 构建用于显示 TMP 可点击文字 partner 条目的简单 ScrollRect 容器。
            TextMeshProUGUI pickerTextTemplate = mainText ?? subText;
            if (pickerTextTemplate is null)
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
                scrollRt.anchorMin = new Vector2(0.12f, 0.28f);
                scrollRt.anchorMax = new Vector2(0.88f, 0.64f);
                scrollRt.offsetMin = Vector2.zero;
                scrollRt.offsetMax = Vector2.zero;

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
                vlg.spacing = 140f;
                vlg.padding = new RectOffset(10, 10, 4, 4);
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
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
            if (cancel is not null)
            {
                cancel.transform.SetAsLastSibling();
            }

            // 验证标签创建成功。
            PartnerPickerTag tagCheck = _partnerPickerRoot.GetComponentInChildren<PartnerPickerTag>(true);
            if (tagCheck is null || tagCheck.TextTemplate is null)
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
            if (_partnerPickerRoot is not null)
            {
                Destroy(_partnerPickerRoot);
            }
            _partnerPickerRoot = null;
        }
    }

    private void OnPartnerPickerRefreshClicked()
    {
        if (!_partnerPickerActive || _partnerPickerRoot is null)
        {
            return;
        }

        // 立即尝试重建列表。
        RebuildPartnerPickerList();

        // 若自身位置仍不可用，显示等待状态并安排一次性刷新。
        if (!OtherPlayersOverlayPatch.TryGetSelfLocation(out _, out _, out _, out _))
        {
            TryShowPartnerPickerWaiting();

            if (_partnerPickerAutoRefreshCo is not null)
            {
                StopCoroutine(_partnerPickerAutoRefreshCo);
                _partnerPickerAutoRefreshCo = null;
            }

            _partnerPickerAutoRefreshCo = StartCoroutine(CoPartnerPickerAutoRefreshOnce());
        }
    }

    private static void CopyRectTransform(RectTransform dst, RectTransform src)
    {
        if (dst is null || src is null)
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
        if (dialog is null || string.IsNullOrWhiteSpace(fieldName))
        {
            return null;
        }

        FieldInfo fi = typeof(MessageDialog).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (fi is null)
        {
            return null;
        }

        return fi.GetValue(dialog) as T;
    }

    private static T GetPrivateFieldValue<T>(object target, string fieldName) where T : class
    {
        if (target is null || string.IsNullOrWhiteSpace(fieldName))
        {
            return null;
        }

        Type t = target.GetType();
        while (t is not null)
        {
            FieldInfo fi = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fi is not null)
            {
                return fi.GetValue(target) as T;
            }

            t = t.BaseType;
        }

        return null;
    }

    private static RectTransform TryFindCommonAncestorRect(RectTransform a, RectTransform b)
    {
        if (a is null || b is null)
        {
            return null;
        }

        HashSet<Transform> ancestors = new HashSet<Transform>();
        Transform t = a;
        while (t is not null)
        {
            ancestors.Add(t);
            t = t.parent;
        }

        Transform u = b;
        while (u is not null)
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
        if (_partnerPickerRoot is null)
        {
            return;
        }

        PartnerPickerTag tag = _partnerPickerRoot.GetComponentInChildren<PartnerPickerTag>(true);
        if (tag is null || tag.TextTemplate is null)
        {
            return;
        }

        Transform container = tag.ListContainer is not null ? tag.ListContainer : tag.transform;
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

                if (tag.EmptyText is not null)
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

            if (tag.EmptyText is not null)
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

                string loc2 = string.IsNullOrWhiteSpace(p.LocationName) ? "?" : p.LocationName;
                string coord = (p.Stage >= 0 || p.LocationX >= 0 || p.LocationY >= 0) ? $"Act {p.Stage}, ({p.LocationX},{p.LocationY})" : "位置未知";
                string where = sameNode ? $"{loc2} - {coord} - 同节点" : $"{loc2} - {coord}";

                string label = string.IsNullOrWhiteSpace(where) ? displayName : $"{displayName}  {where}";
                if (p.IsHost)
                {
                    label += " [Host]";
                }

                Button btn = CreateTextButton(tag.TextTemplate, container, $"Player_{p.PlayerId}", label, tag.TextTemplate.fontSize * 0.5f);

                var le = btn.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 28f;
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
        Plugin.Logger?.LogInfo($"[TradePanel] OnPartnerSelected: tradeId={_tradeId ?? "<null>"}, self={_selfPlayerId ?? "<null>"}, partner={partnerPlayerId ?? "<null>"}, partnerName={partnerPlayerName ?? "<null>"}");
        _playerAId = _selfPlayerId;
        _playerBId = partnerPlayerId;

        if (string.IsNullOrWhiteSpace(_tradeId))
        {
            _tradeId = Guid.NewGuid().ToString("N");
        }

        if (player1NameText is not null) player1NameText.text = ResolveLocalPlayerDisplayName(_payload);
        if (player2NameText is not null) player2NameText.text = OtherPlayersOverlayPatch.ResolveDisplayName(partnerPlayerId, partnerPlayerName, isLocal: false);

        HidePartnerPickerOverlay();

        // 已连接：进行实际的 host 驱动会话。离线/本地调试：保持本地 UI（不发送网络请求）。
        if (IsLocalDebugTradeAllowed() && (string.Equals(partnerPlayerId, "aidefault", StringComparison.Ordinal) || string.Equals(partnerPlayerId, "aidefault2", StringComparison.Ordinal)))
        {
            // 即使已连接，选择本地调试虚拟玩家也允许启动纯本地 UI 测试会话。
            _localDebugTradeMode = true;
            Plugin.Logger?.LogInfo($"[TradePanel] OnPartnerSelected: local debug shortcut for {partnerPlayerId}");
            PopulateLocalDebugRemoteOffer();
            EnsureOfferEditorOverlay();
            EnsureCardPickerOverlay();
            EnsureOfferPreviewOverlay();
            EnsureExhibitPickerOverlay();
            SetTradeDetailsVisible(true);
            cancelButton?.gameObject.SetActive(_canCancel);
            UpdateUIStatus($"本地调试交易：{partnerPlayerName}（不走服务器）");
            return;
        }

        // 网络交易优先使用基于 dialog 的编辑器。
        if (TryShowTradeDetailDialog(partnerPlayerId, partnerPlayerName))
        {
            Plugin.Logger?.LogInfo($"[TradePanel] OnPartnerSelected: handed off to TradeDetailDialog, tradeId={_tradeId}");
            _handoffToDetailDialog = true;
            Hide(false);
            return;
        }

        // Dialog 失败：回退到面板内编辑器，保证交易仍可用。
        UpdateUIStatus("交易详情界面不可用，已回退到面板模式");
        EnsureOfferEditorOverlay();
        EnsureCardPickerOverlay();
        EnsureOfferPreviewOverlay();
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
        GameRunController run = ActiveGameRun;

        // 初始化远端侧（player2）的展示报价，供离线 UI 测试。不修改真实牌组/背包。
        using (new ApplyingStateScope(this))
        {
            // 取少量本地牌组卡牌作为展示克隆。
            var deck = run?.BaseDeck?.Where(c => c is not null).ToList() ?? new List<Card>();
            deck.Take(2)
                .Where(src => src is not null)
                .Select(src => Library.TryCreateCard(src.Id, src.IsUpgraded, src.UpgradeCounter ?? 0))
                .Where(temp => temp is not null)
                .ToList()
                .ForEach(temp => AddCardToTrade(temp, false));

            // 本地侧加小额金币报价，使报价编辑器显示非零状态。
            _localMoneyOffer = Math.Min(10, run?.Money ?? 10);
            RefreshOfferEditorTexts();
            CheckTradeReady();
        }
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

    private void EnsureCardPickerOverlay()
    {
        if (_cardPickerRoot is not null)
        {
            Plugin.Logger?.LogInfo($"[TradePanel] EnsureCardPickerOverlay skipped: existing root name={_cardPickerRoot.name}, activeSelf={_cardPickerRoot.activeSelf}");
            return;
        }

        try
        {
            ShowCardsPanel sourcePanel = UiManager.GetPanel<ShowCardsPanel>();
            if (sourcePanel is null)
            {
                Plugin.Logger?.LogWarning("[TradePanel] EnsureCardPickerOverlay failed: ShowCardsPanel template unavailable.");
                _cardPickerRoot = null;
                return;
            }

            SelectCardPanel selectCardPanel = UiManager.GetPanel<SelectCardPanel>();
            Plugin.Logger?.LogInfo($"[TradePanel] EnsureCardPickerOverlay creating: sourcePanel={sourcePanel.name}, selectCardPanel={(selectCardPanel is not null ? selectCardPanel.name : "<null>")}");
            _cardPickerRoot = Instantiate(sourcePanel.gameObject, transform, false);
            _cardPickerRoot.name = "TradeCardPicker";
            _cardPickerRoot.SetActive(false);

            ShowCardsPanel clonedPanel = _cardPickerRoot.GetComponent<ShowCardsPanel>();
            if (clonedPanel is null)
            {
                Plugin.Logger?.LogWarning("[TradePanel] EnsureCardPickerOverlay failed: cloned ShowCardsPanel missing.");
                Destroy(_cardPickerRoot);
                _cardPickerRoot = null;
                return;
            }

            clonedPanel.enabled = false;

            CardPickerTag tag = _cardPickerRoot.AddComponent<CardPickerTag>();
            tag.DeckHolder = GetPrivateFieldValue<DeckHolder>(clonedPanel, "deckHolder");
            tag.ReturnButton = GetPrivateFieldValue<Button>(clonedPanel, "returnButton");
            tag.TopHideButton = GetPrivateFieldValue<Button>(clonedPanel, "topHideButton");
            tag.Portrait = GetPrivateFieldValue<Image>(clonedPanel, "portrait");
            tag.ActualOrderToggle = GetPrivateFieldValue<CommonToggleWidget>(clonedPanel, "actualOrderToggle");
            tag.IndexOrderToggle = GetPrivateFieldValue<CommonToggleWidget>(clonedPanel, "indexOrderToggle");
            tag.TypeOrderToggle = GetPrivateFieldValue<CommonToggleWidget>(clonedPanel, "typeOrderToggle");
            tag.RarityOrderToggle = GetPrivateFieldValue<CommonToggleWidget>(clonedPanel, "rarityOrderToggle");
            tag.FollowCardOnlyToggle = GetPrivateFieldValue<CommonToggleWidget>(clonedPanel, "followCardOnlyToggle");
            tag.DreamCardOnlyToggle = GetPrivateFieldValue<CommonToggleWidget>(clonedPanel, "dreamCardOnlyToggle");
            tag.MinimizedButton = GetPrivateFieldValue<GameObject>(clonedPanel, "minimizedButton");
            tag.SelectParticleTemplate = selectCardPanel is not null
                ? GetPrivateFieldValue<GameObject>(selectCardPanel, "selectParticle")
                : null;

            if (tag.DeckHolder is null)
            {
                Plugin.Logger?.LogWarning("[TradePanel] EnsureCardPickerOverlay failed: DeckHolder missing on cloned panel.");
                Destroy(_cardPickerRoot);
                _cardPickerRoot = null;
                return;
            }

            Plugin.Logger?.LogInfo($"[TradePanel] EnsureCardPickerOverlay ready: returnButton={(tag.ReturnButton is not null)}, topHideButton={(tag.TopHideButton is not null)}, portrait={(tag.Portrait is not null)}, deckHolder={(tag.DeckHolder is not null)}");

            if (tag.MinimizedButton is not null)
            {
                tag.MinimizedButton.SetActive(false);
            }

            // ReturnButton → 确认选择（覆盖当前报价）
            if (tag.ReturnButton is not null)
            {
                tag.ReturnButton.onClick.RemoveAllListeners();
                tag.ReturnButton.onClick.AddListener(() => ApplyCardPickerSelection(tag));
            }

            // TopHideButton → 取消（关闭选择器，不应用变更）
            if (tag.TopHideButton != null)
            {
                tag.TopHideButton.onClick.RemoveAllListeners();
                tag.TopHideButton.onClick.AddListener(HideCardPickerOverlay);
            }

            BindCardPickerToggle(tag.ActualOrderToggle, on =>
            {
                if (!on)
                {
                    return;
                }

                tag.CurrentOrder = CardPickerOrderStatus.Actual;
                PopulateCardPickerWidgets(tag);
            });

            BindCardPickerToggle(tag.IndexOrderToggle, on =>
            {
                if (!on)
                {
                    return;
                }

                tag.CurrentOrder = CardPickerOrderStatus.Index;
                PopulateCardPickerWidgets(tag);
            });

            BindCardPickerToggle(tag.TypeOrderToggle, on =>
            {
                if (!on)
                {
                    return;
                }

                tag.CurrentOrder = CardPickerOrderStatus.Type;
                PopulateCardPickerWidgets(tag);
            });

            BindCardPickerToggle(tag.RarityOrderToggle, on =>
            {
                if (!on)
                {
                    return;
                }

                tag.CurrentOrder = CardPickerOrderStatus.Rarity;
                PopulateCardPickerWidgets(tag);
            });

            BindCardPickerToggle(tag.DreamCardOnlyToggle, on =>
            {
                if (on)
                {
                    tag.CurrentFilter = CardPickerFilterStatus.DreamCardsOnly;
                    tag.FollowCardOnlyToggle?.toggle?.SetIsOnWithoutNotify(false);
                }
                else
                {
                    tag.CurrentFilter = tag.FollowCardOnlyToggle?.toggle?.isOn == true
                        ? CardPickerFilterStatus.FollowCardsOnly
                        : CardPickerFilterStatus.AllCards;
                }

                PopulateCardPickerWidgets(tag);
            });

            BindCardPickerToggle(tag.FollowCardOnlyToggle, on =>
            {
                if (on)
                {
                    tag.CurrentFilter = CardPickerFilterStatus.FollowCardsOnly;
                    tag.DreamCardOnlyToggle?.toggle?.SetIsOnWithoutNotify(false);
                }
                else
                {
                    tag.CurrentFilter = tag.DreamCardOnlyToggle?.toggle?.isOn == true
                        ? CardPickerFilterStatus.DreamCardsOnly
                        : CardPickerFilterStatus.AllCards;
                }

                PopulateCardPickerWidgets(tag);
            });
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TradePanel] EnsureCardPickerOverlay exception: {ex.Message}");
            _cardPickerRoot = null;
        }
    }

    private enum CardPickerOrderStatus
    {
        Actual,
        Index,
        Type,
        Rarity,
    }

    private enum CardPickerFilterStatus
    {
        AllCards,
        DreamCardsOnly,
        FollowCardsOnly,
    }

    private sealed class CardPickerTag : MonoBehaviour
    {
        public DeckHolder DeckHolder;
        public Button ReturnButton;
        public Button TopHideButton;
        public Image Portrait;
        public CommonToggleWidget ActualOrderToggle;
        public CommonToggleWidget IndexOrderToggle;
        public CommonToggleWidget TypeOrderToggle;
        public CommonToggleWidget RarityOrderToggle;
        public CommonToggleWidget FollowCardOnlyToggle;
        public CommonToggleWidget DreamCardOnlyToggle;
        public GameObject MinimizedButton;
        public GameObject SelectParticleTemplate;
        public readonly List<Card> SourceCards = new();
        public readonly List<SelectCardWidget> SelectWidgets = new();
        public readonly List<int> SelectIndexOrder = new();
        public int TargetSelectCount;
        public CardPickerOrderStatus CurrentOrder = CardPickerOrderStatus.Actual;
        public CardPickerFilterStatus CurrentFilter = CardPickerFilterStatus.AllCards;
    }

    private void ShowCardPickerOverlay()
    {
        GameRunController run = ActiveGameRun;
        Plugin.Logger?.LogInfo($"[TradePanel] ShowCardPickerOverlay enter: tradeId={_tradeId ?? "<null>"}, cardPickerExists={(_cardPickerRoot is not null)}, activeGameRun={(run is not null)}, deckCount={(run?.BaseDeck?.Count ?? 0)}, offeredLocal={_player1OfferedCards.Count}, maxSlots={_maxTradeSlots}, canEdit={CanEditOffer()}");
        EnsureCardPickerOverlay();
        if (_cardPickerRoot is null)
        {
            Plugin.Logger?.LogWarning($"[TradePanel] ShowCardPickerOverlay aborted: card picker root missing, tradeId={_tradeId ?? "<null>"}");
            TryShowTopMessage("卡牌选择界面不可用。");
            return;
        }

        CardPickerTag tag = _cardPickerRoot.GetComponent<CardPickerTag>();
        if (tag is not null)
        {
            tag.CurrentOrder = CardPickerOrderStatus.Actual;
            tag.CurrentFilter = CardPickerFilterStatus.AllCards;

            // 每次打开选择器时，把 returnButton 的文字强制设为"确认"
            if (tag.ReturnButton is not null)
            {
                var allTmps = tag.ReturnButton.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var tmp in allTmps)
                {
                    if (tmp is not null)
                    {
                        tmp.text = "确认";
                    }
                }
                // 尝试通过 SetAllDirty 强制刷新
                if (allTmps.Length > 0 && allTmps[0] is not null)
                {
                    allTmps[0].SetAllDirty();
                }
            }
        }

        _cardPickerRoot.SetActive(true);
        ForceEnableRaycasts(_cardPickerRoot);
        SetTradeDetailsVisible(false);
        EnsurePopupTopmost();
        RebuildCardPickerList();

        // 每次打开选择器，强制设置 returnButton 的所有文字子对象为"确认"
        if (tag is not null && tag.ReturnButton is not null)
        {
            // 移除 LocalizedText 组件，防止 OnEnable 时自动覆盖文字
            var localizedTexts = tag.ReturnButton.GetComponentsInChildren<LocalizedText>(true);
            foreach (var lt in localizedTexts)
            {
                if (lt is not null)
                {
                    UnityEngine.Object.Destroy(lt);
                }
            }
            var allTmps = tag.ReturnButton.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTmps)
            {
                if (tmp is not null)
                {
                    tmp.text = "确认";
                }
            }
        }
        Plugin.Logger?.LogInfo($"[TradePanel] ShowCardPickerOverlay shown: tradeId={_tradeId ?? "<null>"}, pickerActive={_cardPickerRoot.activeSelf}");
    }

    private void HideCardPickerOverlay()
    {
        CardPickerTag tag = _cardPickerRoot is not null ? _cardPickerRoot.GetComponent<CardPickerTag>() : null;
        if (tag?.DeckHolder is not null)
        {
            tag.SelectWidgets.Clear();
            tag.SelectIndexOrder.Clear();
            tag.DeckHolder.Clear();
        }

        _cardPickerRoot?.SetActive(false);
        SetTradeDetailsVisible(true);
    }

    private void EnsureOfferPreviewOverlay()
    {
        if (_offerPreviewRoot is not null)
        {
            return;
        }

        try
        {
            ShowCardsPanel sourcePanel = UiManager.GetPanel<ShowCardsPanel>();
            DeckHolder template = sourcePanel is not null ? GetPrivateFieldValue<DeckHolder>(sourcePanel, "deckHolder") : null;
            CardWidget cardTemplate = template is not null ? GetPrivateFieldValue<CardWidget>(template, "cardTemplate") : null;
            if (template is null || cardTemplate is null)
            {
                return;
            }

            _offerPreviewCardTemplate = cardTemplate;

            _offerPreviewRoot = new GameObject("TradeOfferPreview");
            _offerPreviewRoot.transform.SetParent(GetTradePanelContentParent(), false);

            RectTransform rootRect = _offerPreviewRoot.AddComponent<RectTransform>();
            SetRect(rootRect, 0.02f, 0.40f, 0.98f, 0.74f);

            _localOfferPreviewPanel = CreateOfferPreviewPanel(_offerPreviewRoot.transform, "LocalOfferPreview", 0.00f, 0.00f, 0.48f, 1.00f);
            _remoteOfferPreviewPanel = CreateOfferPreviewPanel(_offerPreviewRoot.transform, "RemoteOfferPreview", 0.52f, 0.00f, 1.00f, 1.00f);

            RefreshOfferPreview();
        }
        catch
        {
            _offerPreviewRoot = null;
            _localOfferPreviewPanel = null;
            _remoteOfferPreviewPanel = null;
            _offerPreviewCardTemplate = null;
        }
    }

    private static OfferPreviewPanelTag CreateOfferPreviewPanel(Transform parent, string name, float minX, float minY, float maxX, float maxY)
    {
        if (parent is null)
        {
            return null;
        }

        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);

        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(minX, minY);
        rootRect.anchorMax = new Vector2(maxX, maxY);
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        GameObject layout = new GameObject("CardLayout");
        layout.transform.SetParent(root.transform, false);
        RectTransform layoutRect = layout.AddComponent<RectTransform>();
        layoutRect.anchorMin = Vector2.zero;
        layoutRect.anchorMax = Vector2.one;
        layoutRect.offsetMin = new Vector2(8f, 8f);
        layoutRect.offsetMax = new Vector2(-8f, -8f);

        OfferPreviewPanelTag tag = root.AddComponent<OfferPreviewPanelTag>();
        tag.CardLayout = layoutRect;
        return tag;
    }

    private void RefreshOfferPreview()
    {
        EnsureOfferPreviewOverlay();

        RebuildOfferPreviewPanel(_localOfferPreviewPanel, _player1OfferedCards);
        RebuildOfferPreviewPanel(_remoteOfferPreviewPanel, _player2OfferedCards);
    }



    private void RebuildOfferPreviewPanel(OfferPreviewPanelTag panel, IEnumerable<Card> cards)
    {
        if (panel?.CardLayout is null || _offerPreviewCardTemplate is null)
        {
            return;
        }

        ClearOfferPreviewPanel(panel);

        List<Card> list = (cards ?? Enumerable.Empty<Card>())
            .Where(card => card is not null)
            .ToList();

        if (list.Count == 0)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        RectTransform layout = panel.CardLayout;
        LayoutRebuilder.ForceRebuildLayoutImmediate(layout);

        float availableWidth = layout.rect.width > 1f ? layout.rect.width : 480f;
        float availableHeight = layout.rect.height > 1f ? layout.rect.height : 240f;
        RectTransform templateRect = _offerPreviewCardTemplate.transform as RectTransform;
        float cardWidth = templateRect is not null && templateRect.rect.width > 1f ? templateRect.rect.width : 360f;
        float cardHeight = templateRect is not null && templateRect.rect.height > 1f ? templateRect.rect.height : 540f;

        int count = list.Count;
        float spacing = Mathf.Clamp(availableWidth * 0.02f, 8f, 18f);
        float scaleByWidth = (availableWidth - Mathf.Max(0, count - 1) * spacing) / (count * cardWidth);
        float scaleByHeight = availableHeight / cardHeight;
        float scale = Mathf.Clamp(Mathf.Min(scaleByWidth, scaleByHeight, 0.42f), 0.22f, 0.42f);
        float displayedWidth = cardWidth * scale;
        float totalWidth = displayedWidth * count + Mathf.Max(0, count - 1) * spacing;
        float startX = -totalWidth * 0.5f + displayedWidth * 0.5f;

        for (int i = 0; i < list.Count; i++)
        {
            Card card = list[i];
            if (card is null)
            {
                continue;
            }

            CardWidget cardWidget = Instantiate(_offerPreviewCardTemplate, layout, false);
            cardWidget.name = $"PreviewCard_{card.InstanceId}_{i}";
            cardWidget.Card = card;

            ShowingCard showing = cardWidget.gameObject.GetComponent<ShowingCard>() ?? cardWidget.gameObject.AddComponent<ShowingCard>();
            showing.SetScale(scale, scale);

            RectTransform rect = cardWidget.transform as RectTransform;
            if (rect is not null)
            {
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(startX + i * (displayedWidth + spacing), 0f);
            }
        }
    }

    private static void ClearOfferPreviewPanel(OfferPreviewPanelTag panel)
    {
        if (panel?.CardLayout is null)
        {
            return;
        }

        panel.CardLayout.Cast<Transform>()
            .Where(c => c is not null)
            .ToList()
            .ForEach(c => Destroy(c.gameObject));
    }

    private sealed class OfferPreviewPanelTag : MonoBehaviour
    {
        public RectTransform CardLayout;
    }

    private void RebuildCardPickerList()
    {
        if (_cardPickerRoot is null)
        {
            Plugin.Logger?.LogWarning($"[TradePanel] RebuildCardPickerList aborted: card picker root missing, tradeId={_tradeId ?? "<null>"}");
            return;
        }

        CardPickerTag tag = _cardPickerRoot.GetComponent<CardPickerTag>();
        if (tag is null || tag.DeckHolder is null)
        {
            Plugin.Logger?.LogWarning($"[TradePanel] RebuildCardPickerList aborted: picker tag/holder missing, tradeId={_tradeId ?? "<null>"}, tag={(tag is not null)}, holder={(tag?.DeckHolder is not null)}");
            return;
        }

        GameRunController run = ActiveGameRun;
        Plugin.Logger?.LogInfo($"[TradePanel] RebuildCardPickerList enter: tradeId={_tradeId ?? "<null>"}, activeGameRun={(run is not null)}, deckCount={(run?.BaseDeck?.Count ?? 0)}, offeredLocal={_player1OfferedCards.Count}, maxSlots={_maxTradeSlots}, pickerActive={_cardPickerRoot.activeSelf}, canEdit={CanEditOffer()}");

        // 网络交易下仅允许编辑本地报价。
        if (!CanEditOffer())
        {
            Plugin.Logger?.LogInfo($"[TradePanel] RebuildCardPickerList blocked: cannot edit offer, tradeId={_tradeId ?? "<null>"}");
            tag.SourceCards.Clear();
            tag.TargetSelectCount = 0;
            tag.DeckHolder.Clear();
            tag.DeckHolder.SetTitle("Game.Deck".Localize(true), "当前状态无法编辑报价。");
            return;
        }

        List<Card> deck = new List<Card>();
        try
        {
            if (run?.BaseDeck is not null)
            {
                deck = run.BaseDeck
                    .Where(c => c is not null)
                    .ToList();
            }
        }
        catch
        {
            deck = new List<Card>();
        }

        // 覆盖模式：从完整牌组中选择，已有报价在打开选择器时已被清空
        List<Card> candidates = deck.Where(c => c is not null).ToList();
        if (candidates.Count == 0)
        {
            Plugin.Logger?.LogInfo($"[TradePanel] Card picker empty: panelGameRun={(GameRun is not null)}, activeGameRun={(run is not null)}, deckCount={deck.Count}, tradeId={_tradeId ?? "<null>"}");
            tag.SourceCards.Clear();
            tag.TargetSelectCount = 0;
            tag.DeckHolder.Clear();
            tag.DeckHolder.SetTitle("Game.Deck".Localize(true), "没有可交易的卡牌。");
            return;
        }

        tag.SourceCards.Clear();
        tag.SourceCards.AddRange(candidates);
        tag.TargetSelectCount = Math.Min(_maxTradeSlots, candidates.Count);
        Plugin.Logger?.LogInfo($"[TradePanel] RebuildCardPickerList prepared: tradeId={_tradeId ?? "<null>"}, candidateCount={candidates.Count}, maxSlots={_maxTradeSlots}, targetSelectCount={tag.TargetSelectCount}");

        if (tag.Portrait is not null)
        {
            ShowCardsPanel sourcePanel = UiManager.GetPanel<ShowCardsPanel>();
            Image sourcePortrait = sourcePanel is not null ? GetPrivateFieldValue<Image>(sourcePanel, "portrait") : null;
            if (sourcePortrait is not null && sourcePortrait.sprite is not null)
            {
                tag.Portrait.sprite = sourcePortrait.sprite;
            }
        }

        ApplyCardPickerToggleStates(tag);
        PopulateCardPickerWidgets(tag);
    }

    private static void BindCardPickerToggle(CommonToggleWidget widget, UnityEngine.Events.UnityAction<bool> handler)
    {
        if (widget?.toggle is null || handler is null)
        {
            return;
        }

        widget.toggle.onValueChanged.AddListener(handler);
    }

    private static void ApplyCardPickerToggleStates(CardPickerTag tag)
    {
        if (tag is null)
        {
            return;
        }

        if (tag.ActualOrderToggle?.toggle is not null)
        {
            tag.ActualOrderToggle.toggle.SetIsOnWithoutNotify(tag.CurrentOrder == CardPickerOrderStatus.Actual);
            tag.ActualOrderToggle.toggle.interactable = true;
            tag.ActualOrderToggle.SetLock(true);
        }

        if (tag.IndexOrderToggle?.toggle is not null)
        {
            tag.IndexOrderToggle.toggle.SetIsOnWithoutNotify(tag.CurrentOrder == CardPickerOrderStatus.Index);
        }

        if (tag.TypeOrderToggle?.toggle is not null)
        {
            tag.TypeOrderToggle.toggle.SetIsOnWithoutNotify(tag.CurrentOrder == CardPickerOrderStatus.Type);
        }

        if (tag.RarityOrderToggle?.toggle is not null)
        {
            tag.RarityOrderToggle.toggle.SetIsOnWithoutNotify(tag.CurrentOrder == CardPickerOrderStatus.Rarity);
        }

        if (tag.DreamCardOnlyToggle?.toggle is not null)
        {
            tag.DreamCardOnlyToggle.toggle.SetIsOnWithoutNotify(tag.CurrentFilter == CardPickerFilterStatus.DreamCardsOnly);
        }

        if (tag.FollowCardOnlyToggle?.toggle is not null)
        {
            tag.FollowCardOnlyToggle.toggle.SetIsOnWithoutNotify(tag.CurrentFilter == CardPickerFilterStatus.FollowCardsOnly);
        }
    }

    private void PopulateCardPickerWidgets(CardPickerTag tag)
    {
        if (tag?.DeckHolder is null)
        {
            return;
        }

        tag.DeckHolder.Clear();
        tag.SelectWidgets.Clear();
        tag.SelectIndexOrder.Clear();

        int dreamCount = tag.SourceCards.Count(c => c is not null && c.IsDreamCard);
        int followCount = tag.SourceCards.Count(c => c is not null && c.IsFollowCard);
        if (tag.DreamCardOnlyToggle is not null)
        {
            tag.DreamCardOnlyToggle.gameObject.SetActive(dreamCount > 0);
        }

        if (tag.FollowCardOnlyToggle is not null)
        {
            tag.FollowCardOnlyToggle.gameObject.SetActive(followCount > 0);
        }

        List<Card> displayCards = GetCardPickerDisplayCards(tag);
        string description = $"请选择 0~{_maxTradeSlots} 张卡牌交易（选满自动截断）";

        if (displayCards.Count == 0)
        {
            string emptyReason = tag.SourceCards.Count == 0
                ? "没有可交易的卡牌。"
                : "当前过滤条件下没有可交易的卡牌。";
            tag.DeckHolder.SetTitle("Game.Deck".Localize(true), emptyReason);
            return;
        }

        tag.DeckHolder.SetTitle("Game.Deck".Localize(true), description);

        foreach (Card card in displayCards)
        {
            CreateCardPickerCardWidget(tag, card);
        }
    }

    private static List<Card> GetCardPickerDisplayCards(CardPickerTag tag)
    {
        IEnumerable<Card> ordered = tag.SourceCards.Where(card => card is not null);
        switch (tag.CurrentOrder)
        {
            case CardPickerOrderStatus.Index:
                ordered = ordered.OrderBy(card => card.Config.Index);
                break;
            case CardPickerOrderStatus.Type:
                ordered = ordered.OrderBy(card => card.CardType).ThenBy(card => card.Config.Index);
                break;
            case CardPickerOrderStatus.Rarity:
                ordered = ordered.OrderBy(card => card.Config.Rarity).ThenBy(card => card.Config.Index);
                break;
            case CardPickerOrderStatus.Actual:
            default:
                break;
        }

        switch (tag.CurrentFilter)
        {
            case CardPickerFilterStatus.DreamCardsOnly:
                ordered = ordered.Where(card => card.IsDreamCard);
                break;
            case CardPickerFilterStatus.FollowCardsOnly:
                ordered = ordered.Where(card => card.IsFollowCard);
                break;
            case CardPickerFilterStatus.AllCards:
            default:
                break;
        }

        return ordered.ToList();
    }

    private void CreateCardPickerCardWidget(CardPickerTag tag, Card card)
    {
        try
        {
            if (tag?.DeckHolder is null || card is null)
            {
                return;
            }

            CardWidget cardWidget = tag.DeckHolder.AddCardWidget(card, true);
            if (cardWidget is null)
            {
                return;
            }

            SelectCardWidget selectWidget = cardWidget.gameObject.AddComponent<SelectCardWidget>();
            selectWidget.SelectParticle = CreateCardPickerSelectionMarker(selectWidget.transform, tag.SelectParticleTemplate);
            selectWidget.SelectParticle.SetActive(false);
            selectWidget.SelectedChanged += (_, _) => OnCardPickerSelectionChanged(tag, selectWidget);
            tag.SelectWidgets.Add(selectWidget);
        }
        catch
        {
            // 忽略
        }
    }

    private static GameObject CreateCardPickerSelectionMarker(Transform parent, GameObject template)
    {
        if (template is not null)
        {
            GameObject marker = Instantiate(template, parent, false);
            marker.name = "TradeSelectParticle";
            return marker;
        }

        GameObject fallback = new GameObject("TradeSelectParticle", typeof(RectTransform), typeof(Image));
        fallback.transform.SetParent(parent, false);
        RectTransform rect = fallback.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = fallback.GetComponent<Image>();
        image.color = new Color(0.35f, 0.85f, 1f, 0.22f);
        image.raycastTarget = false;
        return fallback;
    }

    private void OnCardPickerSelectionChanged(CardPickerTag tag, SelectCardWidget widget)
    {
        if (tag is null || widget is null || tag.TargetSelectCount <= 0)
        {
            return;
        }

        int widgetIndex = tag.SelectWidgets.IndexOf(widget);
        if (widgetIndex < 0)
        {
            return;
        }

        if (widget.IsSelected)
        {
            if (!tag.SelectIndexOrder.Contains(widgetIndex))
            {
                tag.SelectIndexOrder.Add(widgetIndex);
            }
        }
        else
        {
            tag.SelectIndexOrder.Remove(widgetIndex);
        }

        int selectedCount = tag.SelectWidgets.Count(w => w is not null && w.IsSelected);
        if (selectedCount > tag.TargetSelectCount && tag.SelectIndexOrder.Count > 0)
        {
            int firstIndex = tag.SelectIndexOrder[0];
            tag.SelectIndexOrder.RemoveAt(0);
            if (firstIndex >= 0 && firstIndex < tag.SelectWidgets.Count)
            {
                SelectCardWidget firstWidget = tag.SelectWidgets[firstIndex];
                if (firstWidget is not null)
                {
                    firstWidget.SetSelected(false, false);
                }
            }
        }
    }

    private void ApplyCardPickerSelection(CardPickerTag tag)
    {
        if (tag is null || _cardPickerApplyingSelection)
        {
            return;
        }

        _cardPickerApplyingSelection = true;
        try
        {
            // 清除已选卡牌，以本次选择为准
            foreach (var card in _player1OfferedCards.ToList())
            {
                RemoveCardFromTrade(card, true);
            }

            foreach (int index in tag.SelectIndexOrder.ToList())
            {
                if (index < 0 || index >= tag.SelectWidgets.Count)
                {
                    continue;
                }

                SelectCardWidget widget = tag.SelectWidgets[index];
                if (widget is null || !widget.IsSelected || widget.Card is null)
                {
                    continue;
                }

                AddCardToTrade(widget.Card, true);
            }

            RefreshOfferEditorTexts();
            CheckTradeReady();
            HideCardPickerOverlay();
        }
        finally
        {
            _cardPickerApplyingSelection = false;
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
            if (rt is null)
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
                if (mouse is not null && mouse.leftButton is not null && mouse.leftButton.wasPressedThisFrame)
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
                if (Panel is null || !Panel._partnerPickerActive || Panel._partnerPickerRoot is null)
                {
                    return;
                }

                if (Time.unscaledTime < Panel._partnerPickerInputReadyTime)
                {
                    return;
                }

                if (!TryGetLeftClickThisFrame(out Vector2 pos))
                {
                    return;
                }

                // 忽略取消按钮点击。
                if (Panel._partnerPickerCancelButton is not null)
                {
                    RectTransform cancelRt = Panel._partnerPickerCancelButton.transform as RectTransform;
                    if (Contains(cancelRt, pos))
                    {
                        return;
                    }
                }

                PartnerPickerTag pickerTag = Panel._partnerPickerRoot.GetComponentInChildren<PartnerPickerTag>(true);
                if (pickerTag is null)
                {
                    return;
                }

                RectTransform listRegion = null;
                if (pickerTag.ListContainer is not null)
                {
                    listRegion = pickerTag.ListContainer;
                }
                else if (pickerTag.ScrollRect is not null)
                {
                    listRegion = pickerTag.ScrollRect.GetComponent<RectTransform>();
                }
                else
                {
                    listRegion = pickerTag.transform as RectTransform;
                }

                // 仅处理列表区域内的点击。
                if (listRegion is not null && !Contains(listRegion, pos))
                {
                    return;
                }

                Transform container = pickerTag.ListContainer is not null ? pickerTag.ListContainer : pickerTag.transform;

                if (container is null)
                {
                    return;
                }

                // 命中测试候选项，倒序遍历以从当前最高层开始。
                var candidates = container.GetComponentsInChildren<PartnerCandidateTag>(true);
                for (int i = candidates.Length - 1; i >= 0; i--)
                {
                    var cand = candidates[i];
                    if (cand is null || string.IsNullOrWhiteSpace(cand.PlayerId))
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
                        if (g is null)
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
                if (eventData is null || eventData.button != PointerEventData.InputButton.Left)
                {
                    return;
                }

                if (Panel is null || !Panel._partnerPickerActive || Panel._partnerPickerRoot is null)
                {
                    return;
                }

                if (Time.unscaledTime < Panel._partnerPickerInputReadyTime)
                {
                    return;
                }

                // 忽略对取消按钮的干扰。
                try
                {
                    if (Panel._partnerPickerCancelButton is not null)
                    {
                        var press = eventData.pointerPress;
                        if (press is not null && press.transform is not null && press.transform.IsChildOf(Panel._partnerPickerCancelButton.transform))
                        {
                            return;
                        }

                        var hit = eventData.pointerCurrentRaycast.gameObject;
                        if (hit is not null && hit.transform is not null && hit.transform.IsChildOf(Panel._partnerPickerCancelButton.transform))
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
                if (pickerTag is null)
                {
                    return;
                }

                Transform container = pickerTag.ListContainer is not null ? pickerTag.ListContainer : pickerTag.transform;

                if (container is null)
                {
                    return;
                }

                // 仅依矩形命中测试解析被点击的候选。
                var candidates = container.GetComponentsInChildren<PartnerCandidateTag>(true);
                foreach (var cand in candidates)
                {
                    if (cand is null || string.IsNullOrWhiteSpace(cand.PlayerId))
                    {
                        continue;
                    }

                    RectTransform rt = cand.transform as RectTransform;
                    if (rt is null || !rt.gameObject.activeInHierarchy)
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
                        if (g is null)
                        {
                            continue;
                        }

                        var grt = g.rectTransform;
                        if (grt is null)
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
        if (client is null || !client.IsConnected)
        {
            return false;
        }

        localIsA = string.Equals(_selfPlayerId, _playerAId, StringComparison.Ordinal);
        return !string.IsNullOrWhiteSpace(_tradeId) && !string.IsNullOrWhiteSpace(_selfPlayerId);
    }

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

    private void EnsureOfferEditorOverlay()
    {
        if (_offerEditorRoot is not null)
        {
            Plugin.Logger?.LogInfo($"[TradePanel] EnsureOfferEditorOverlay skipped: existing root name={_offerEditorRoot.name}, activeSelf={_offerEditorRoot.activeSelf}");
            return;
        }

        try
        {
            Plugin.Logger?.LogInfo($"[TradePanel] EnsureOfferEditorOverlay creating: tradeId={_tradeId ?? "<null>"}");
            _offerEditorRoot = new GameObject("TradeOfferEditor");
            _offerEditorRoot.transform.SetParent(GetTradePanelContentParent(), false);

            RectTransform rootRect = _offerEditorRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.00f, -0.35f);
            rootRect.anchorMax = new Vector2(0.26f, -0.06f);
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // 用户需求：已移除背景。

            if (!TryPickOfferEditorTemplates(out TextMeshProUGUI textTemplate, out CommonButtonWidget buttonTemplate))
            {
                Plugin.Logger?.LogWarning($"[TradePanel] EnsureOfferEditorOverlay failed: template lookup failed, tradeId={_tradeId ?? "<null>"}");
                Destroy(_offerEditorRoot);
                _offerEditorRoot = null;
                return;
            }

            // 卡牌行
            var cardsLabel = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            cardsLabel.name = "CardsLabel";
            cardsLabel.text = "卡牌:";
            cardsLabel.alignment = TextAlignmentOptions.Right;
            ConfigureSingleLineText(cardsLabel);
            SetRect(cardsLabel.rectTransform, 0.02f, 0.70f, 0.25f, 0.95f);

            _cardCountText = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            _cardCountText.name = "CardsValue";
            _cardCountText.text = "0";
            _cardCountText.alignment = TextAlignmentOptions.Left;
            ConfigureSingleLineText(_cardCountText);
            SetRect(_cardCountText.rectTransform, 0.28f, 0.70f, 0.52f, 0.95f);

            // 金币行
            var moneyLabel = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            moneyLabel.name = "MoneyLabel";
            moneyLabel.text = "金币:";
            moneyLabel.alignment = TextAlignmentOptions.Right;
            ConfigureSingleLineText(moneyLabel);
            SetRect(moneyLabel.rectTransform, 0.02f, 0.38f, 0.25f, 0.63f);

            // 已持金币显示值：用户不需要看到，隐藏之。
            _ownedMoneyText = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            _ownedMoneyText.name = "OwnedMoneyValue";
            _ownedMoneyText.text = "0";
            _ownedMoneyText.alignment = TextAlignmentOptions.Left;
            _ownedMoneyText.raycastTarget = false;
            ConfigureSingleLineText(_ownedMoneyText);
            SetRect(_ownedMoneyText.rectTransform, 0.28f, 0.38f, 0.52f, 0.63f);
            _ownedMoneyText.gameObject.SetActive(false);

            // 金币三元组：保持 '-' 和 '+' 在数字两侧等距。
            // 需求：数字位数变化时间距保持不变，三元组整体居中。
            _moneyTripletRoot = new GameObject("MoneyTriplet");
            _moneyTripletRoot.transform.SetParent(_offerEditorRoot.transform, false);
            var moneyTripletRect = _moneyTripletRoot.AddComponent<RectTransform>();
            // 金币 triplet 从标签右侧开始撑到右边，完整显示加减号和数字。
            SetRect(moneyTripletRect, 0.28f, 0.38f, 1.00f, 0.65f);

            // 金币 +/-：明确定位，避免 HorizontalLayoutGroup 导致字体尺寸坍缩为零。
            var minusBtn = CreateTextButton(textTemplate, _moneyTripletRoot.transform, "MoneyMinus", "-");
            SetRect(minusBtn.GetComponent<RectTransform>(), 0.00f, 0.00f, 0.28f, 1.00f);
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
            ConfigureSingleLineText(_moneyValueText);
            SetRect(_moneyValueText.rectTransform, 0.30f, 0.00f, 0.70f, 1.00f);
            _moneyValueBaseFontSize = _moneyValueText.fontSize;

            var plusBtn = CreateTextButton(textTemplate, _moneyTripletRoot.transform, "MoneyPlus", "+");
            SetRect(plusBtn.GetComponent<RectTransform>(), 0.72f, 0.00f, 1.00f, 1.00f);
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
            exLabel.alignment = TextAlignmentOptions.Right;
            ConfigureSingleLineText(exLabel);
            SetRect(exLabel.rectTransform, 0.02f, 0.05f, 0.25f, 0.30f);

            _exhibitValueText = Instantiate(textTemplate, _offerEditorRoot.transform, false);
            _exhibitValueText.name = "ExValue";
            _exhibitValueText.text = "0";
            _exhibitValueText.alignment = TextAlignmentOptions.Left;
            ConfigureSingleLineText(_exhibitValueText);
            SetRect(_exhibitValueText.rectTransform, 0.28f, 0.05f, 0.52f, 0.30f);

            // 最终清理：将 button 模板层次带入的意外“取消”/多余按钮对象全部删除，仅保留已知 widget。
            PruneOfferEditorExtraButtons(_offerEditorRoot);

            EnsureOfferActionsOverlay(textTemplate);

            // 确保报价编辑器在框架视觉层上方，避免文字被 dialog mask 遮挡。
            // 在运行时布局中不与底部确认/取消按钮重叠。
            _offerEditorRoot?.transform.SetAsLastSibling();

            RefreshOfferEditorTexts();
            Plugin.Logger?.LogInfo($"[TradePanel] EnsureOfferEditorOverlay ready: tradeId={_tradeId ?? "<null>"}, cardCountText={(_cardCountText is not null)}, ownedMoneyText={(_ownedMoneyText is not null)}, exhibitValueText={(_exhibitValueText is not null)}, offerActionsExists={(_offerActionsRoot is not null)}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TradePanel] EnsureOfferEditorOverlay exception: {ex.Message}");
            try
            {
                if (_offerEditorRoot is not null)
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
        if (offerRoot is null)
            return;

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

        offerRoot.transform.Cast<Transform>()
            .Where(child => child is not null && !keepRoots.Contains(child.name ?? string.Empty))
            .ToList()
            .ForEach(child => Destroy(child.gameObject));

        offerRoot.GetComponentsInChildren<Transform>(true)
            .Where(t => t is not null && !ReferenceEquals(t.gameObject, offerRoot))
            .Where(t =>
            {
                var n = t.name ?? string.Empty;
                return (n.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        n.IndexOf("close", StringComparison.OrdinalIgnoreCase) >= 0) &&
                       !keepRoots.Contains(n);
            })
            .ToList()
            .ForEach(t => Destroy(t.gameObject));
    }

    private bool TryGetOwnedMoney(out int ownedMoney)
    {
        try
        {
            int best = 0;
            GameRunController run = ActiveGameRun;

            // 主来源：GameRun.Money（在此面板其他地方也使用）。
            best = Math.Max(best, run?.Money ?? 0);

            // 备用方1：Player 可能暴露金币相关属性。
            try
            {
                var p = run?.Player;
                if (p is not null)
                {
                    var t = p.GetType();
                    var prop = t.GetProperty("Money", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (prop is not null && prop.PropertyType == typeof(int))
                    {
                        best = Math.Max(best, (int)prop.GetValue(p));
                    }
                    var field = t.GetField("Money", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field is not null && field.FieldType == typeof(int))
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
                if (gm is not null)
                {
                    var t = gm.GetType();
                    var prop = t.GetProperty("CurrentGameRun", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var reflectedRun = prop?.GetValue(gm);
                    if (reflectedRun is not null)
                    {
                        var rt = reflectedRun.GetType();
                        var moneyProp = rt.GetProperty("Money", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (moneyProp is not null && moneyProp.PropertyType == typeof(int))
                        {
                            best = Math.Max(best, (int)moneyProp.GetValue(reflectedRun));
                        }
                        var moneyField = rt.GetField("Money", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (moneyField is not null && moneyField.FieldType == typeof(int))
                        {
                            best = Math.Max(best, (int)moneyField.GetValue(reflectedRun));
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
            return (run is not null) || ownedMoney > 0;
        }
        catch
        {
            ownedMoney = 0;
            return false;
        }
    }

    private void EnsureOfferActionsOverlay(TextMeshProUGUI textTemplate)
    {
        if (_offerActionsRoot is not null)
        {
            return;
        }

        try
        {
            _offerActionsRoot = new GameObject("TradeOfferActions");
            _offerActionsRoot.transform.SetParent(GetTradePanelContentParent(), false);

            var rt = _offerActionsRoot.AddComponent<RectTransform>();

            // 右下区域（绿色框区域），与详情列对齐，保持与报价编辑器的间距一致。
            rt.anchorMin = new Vector2(0.74f, -0.35f);
            rt.anchorMax = new Vector2(1.00f, -0.06f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var pickCardsBtn = CreateTextButton(textTemplate, _offerActionsRoot.transform, "PickCards", "选择卡牌", textTemplate.fontSize * 0.50f);
            SetRect(pickCardsBtn.GetComponent<RectTransform>(), 0.04f, 0.56f, 0.96f, 1f);
            var cardsTmp = pickCardsBtn.GetComponent<TextMeshProUGUI>();
            if (cardsTmp is not null) cardsTmp.alignment = TextAlignmentOptions.Center;

            pickCardsBtn.onClick.RemoveAllListeners();
            pickCardsBtn.onClick.AddListener(() =>
            {
                if (!CanEditOffer())
                {
                    UpdateUIStatus(TryLocalize("Trade.WaitingForItems", "请先开始交易/等待交易开启"));
                    return;
                }

                ShowCardPickerOverlay();
            });

            var pickExhibitsBtn = CreateTextButton(textTemplate, _offerActionsRoot.transform, "PickExhibits", "选择展品", textTemplate.fontSize * 0.50f);
            SetRect(pickExhibitsBtn.GetComponent<RectTransform>(), 0.04f, 0.00f, 0.96f, 0.40f);
            var exhibitsTmp = pickExhibitsBtn.GetComponent<TextMeshProUGUI>();
            if (exhibitsTmp is not null) exhibitsTmp.alignment = TextAlignmentOptions.Center;

            pickExhibitsBtn.onClick.RemoveAllListeners();
            pickExhibitsBtn.onClick.AddListener(() =>
            {
                if (!CanEditOffer())
                {
                    UpdateUIStatus(TryLocalize("Trade.WaitingForItems", "请先开始交易/等待交易开启"));
                    return;
                }

                ShowExhibitPickerOverlay();
            });

            // 初始化时同步报价编辑器的显示状态。
            _offerActionsRoot?.SetActive(_offerEditorRoot is not null && _offerEditorRoot.activeSelf);
        }
        catch
        {
            try
            {
                if (_offerActionsRoot is not null)
                {
                    Destroy(_offerActionsRoot);
                }
            }
            catch
            {
                // 忽略
            }

            _offerActionsRoot = null;
        }
    }

    private sealed class TextButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public float HoverScale = 1.08f;
        public float PressedScale = 1.12f;
        public float AnimationSpeed = 8f;

        private RectTransform _rt;
        private bool _hovering;
        private bool _pressed;
        private float _targetScale = 1f;

        private void Awake()
        {
            _rt = transform as RectTransform;
            _targetScale = 1f;
            if (_rt is not null) _rt.localScale = Vector3.one;
        }

        private void Update()
        {
            if (_rt is null) return;
            float cur = _rt.localScale.x;
            if (Mathf.Approximately(cur, _targetScale))
                return;
            float next = Mathf.Lerp(cur, _targetScale, Time.unscaledDeltaTime * AnimationSpeed);
            _rt.localScale = new Vector3(next, next, 1f);
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
            if (_rt is null) return;
            _targetScale = _pressed ? PressedScale : _hovering ? HoverScale : 1f;
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
        ConfigureSingleLineText(tmp);
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

    private static void ConfigureSingleLineText(TextMeshProUGUI text)
    {
        if (text is null)
        {
            return;
        }

        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
    }

    private bool TryPickOfferEditorTemplates(out TextMeshProUGUI textTemplate, out CommonButtonWidget buttonTemplate)
    {
        textTemplate = null;
        buttonTemplate = null;

        // 首选复用 vanilla dialog prefab 模板，以保持 TMP 字体/材质与原生 UI 一致。
        try
        {
            GameObject dialogPrefab = Resources.Load<GameObject>("UI/Dialogs/MessageDialog");
            if (dialogPrefab is not null)
            {
                var dialog = dialogPrefab.GetComponentInChildren<MessageDialog>(true);
                if (dialog is not null)
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
                if (buttonTemplate is null)
                {
                    CommonButtonWidget best = null;
                    int bestScore = int.MaxValue;
                    var widgets = dialogPrefab.GetComponentsInChildren<CommonButtonWidget>(true);
                    foreach (var w in widgets)
                    {
                        if (w is null || w.button is null)
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
        textTemplate ??= statusText ?? player1NameText;
        buttonTemplate ??= confirmButton ?? cancelButton;

        buttonTemplate = PreferSingleButtonWidget(buttonTemplate);

        return textTemplate != null && buttonTemplate != null;
    }

    private static CommonButtonWidget TryResolveCommonButtonWidget(Button target)
    {
        // 优先选择明确引用该 Button 的最近 widget。
        var widgets = target.GetComponentsInParent<CommonButtonWidget>(true);
        if (widgets is not null)
        {
            foreach (var w in widgets)
            {
                if (w is null)
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
        if (template is null)
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
            if (w is null || w.button is null)
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
        return state is null || state.Status == TradeSyncPatch.TradeStatus.Open;
    }

    private void RefreshOfferEditorTexts()
    {
        if (_cardCountText is not null) _cardCountText.text = (_player1OfferedCards?.Count ?? 0).ToString();

        if (_ownedMoneyText is not null)
        {
            if (!TryGetOwnedMoney(out int owned))
                owned = ActiveGameRun?.Money ?? 0;
            _ownedMoneyText.text = Math.Max(0, owned).ToString();
        }

        if (_moneyValueText is not null)
        {
            _moneyValueText.text = _localMoneyOffer.ToString();
            // 根据数字位数自动缩放字体
            string s2 = _moneyValueText.text ?? string.Empty;
            int digits = 0;
            for (int i = 0; i < s2.Length; i++)
            {
                char ch = s2[i];
                if (ch >= '0' && ch <= '9')
                {
                    digits++;
                }
            }
            float baseSize = _moneyValueBaseFontSize > 0f ? _moneyValueBaseFontSize : _moneyValueText.fontSize;
            if (digits >= 4)
            {
                float scale = Mathf.Clamp(3f / digits, 0.6f, 1f);
                _moneyValueText.fontSize = baseSize * scale;
            }
            else
            {
                _moneyValueText.fontSize = baseSize;
            }
            if (_moneyTripletRoot != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_moneyTripletRoot.transform as RectTransform);
        }

        if (_exhibitValueText is not null) _exhibitValueText.text = _localExhibitOfferIds.Count.ToString();
        RebuildExhibitPreviews();
    }

    #region 展品预览栏

    private void EnsureExhibitPreviewContainers()
    {
        if (_localExhibitContainer != null) return;

        if (_exhibitIconTemplate == null)
        {
            try
            {
                var systemBoard = UiManager.GetPanel<SystemBoard>();
                if (systemBoard != null)
                {
                    var templateField = typeof(SystemBoard).GetField("exhibitTemplate",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    if (templateField != null)
                        _exhibitIconTemplate = templateField.GetValue(systemBoard) as ExhibitWidget;
                }
            }
            catch (Exception ex) { Plugin.Logger?.LogWarning($"[TradePanel] 获取 ExhibitTemplate 失败: {ex.Message}"); }
        }

        _localExhibitContainer = new GameObject("LocalExhibitPreviews");
        _localExhibitContainer.transform.SetParent(player1TradeArea, false);
        var lRt = _localExhibitContainer.AddComponent<RectTransform>();
        lRt.anchorMin = new Vector2(0f, 0f);
        lRt.anchorMax = new Vector2(1f, 0f);
        lRt.pivot = new Vector2(0.5f, 0f);
        lRt.sizeDelta = new Vector2(0f, 30f);
        lRt.anchoredPosition = new Vector2(0f, 6f);

        _remoteExhibitContainer = new GameObject("RemoteExhibitPreviews");
        _remoteExhibitContainer.transform.SetParent(player2TradeArea, false);
        var rRt = _remoteExhibitContainer.AddComponent<RectTransform>();
        rRt.anchorMin = new Vector2(0f, 0f);
        rRt.anchorMax = new Vector2(1f, 0f);
        rRt.pivot = new Vector2(0.5f, 0f);
        rRt.sizeDelta = new Vector2(0f, 30f);
        rRt.anchoredPosition = new Vector2(0f, 6f);
    }

    private void RebuildExhibitPreviews()
    {
        EnsureExhibitPreviewContainers();

        RebuildSideExhibitPreviews(_localExhibitContainer, true);
        RebuildSideExhibitPreviews(_remoteExhibitContainer, false);
    }

    private void RebuildSideExhibitPreviews(GameObject container, bool isLocal)
    {
        if (container == null) return;

        foreach (Transform child in container.transform)
            Destroy(child.gameObject);

        GameRunController run = ActiveGameRun;
        List<Exhibit> tradable = new List<Exhibit>();
        try
        {
            if (run?.Player?.Exhibits is not null)
            {
                tradable = run.Player.Exhibits
                    .Where(TradeExhibitRules.IsTradable)
                    .OrderBy(e => e.Name)
                    .ToList();
            }
        }
        catch
        {
            tradable = new List<Exhibit>();
        }

        if (tradable.Count == 0) return;

        float iconSize = 26f;
        float spacing = 4f;
        float totalWidth = tradable.Count * iconSize + (tradable.Count - 1) * spacing;
        float startX = -totalWidth * 0.5f + iconSize * 0.5f;

        for (int i = 0; i < tradable.Count; i++)
        {
            Exhibit exhibit = tradable[i];
            ExhibitWidget widget = null;

            if (_exhibitIconTemplate != null)
            {
                widget = Instantiate(_exhibitIconTemplate, container.transform, false);
                widget.Exhibit = exhibit;
                widget.ShowBattleStatus = false;
                widget.ShowCounter = false;
            }
            else
            {
                var iconGo = new GameObject($"ExIcon_{exhibit.Id}");
                iconGo.transform.SetParent(container.transform, false);
                var img = iconGo.AddComponent<Image>();
                img.preserveAspect = true;
                img.raycastTarget = true;
                Sprite sprite = null;
                try { sprite = ResourcesHelper.TryGetSprite<Exhibit>(exhibit.Id); }
                catch (Exception ex) { Plugin.Logger?.LogWarning($"[TradePanel] 加载展品图标失败: ExhibitId={exhibit.Id}, {ex.Message}"); }
                if (sprite != null) img.sprite = sprite;

                var commonBtn = iconGo.AddComponent<CommonButtonWidget>();
                var btnField = typeof(CommonButtonWidget).GetField("button", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (btnField != null)
                {
                    var btn = iconGo.AddComponent<Button>();
                    btn.targetGraphic = img;
                    btnField.SetValue(commonBtn, btn);
                }

                widget = iconGo.AddComponent<ExhibitWidget>();
                var widgetExhibitField = typeof(ExhibitWidget).GetField("image", BindingFlags.Instance | BindingFlags.NonPublic);
                if (widgetExhibitField != null)
                    widgetExhibitField.SetValue(widget, img);
                widget.Exhibit = exhibit;
            }

            var rt = widget.transform as RectTransform;
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(iconSize, iconSize);
                rt.anchoredPosition = new Vector2(startX + i * (iconSize + spacing), 0f);
                rt.localScale = Vector3.one;
            }
        }
    }

    #endregion

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
        if (_exhibitPickerRoot is null)
        {
            TryShowTopMessage("展品选择界面不可用。");
            return;
        }

        RebuildExhibitPickerList();
        _exhibitPickerRoot.SetActive(true);
        ForceEnableRaycasts(_exhibitPickerRoot);
        SetTradeDetailsVisible(false);
        EnsurePopupTopmost();
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
        if (_exhibitPickerRoot is not null)
        {
            return;
        }

        try
        {
            RuntimeSelectionPanelFactory.SelectionPanelScaffold scaffold = RuntimeSelectionPanelFactory.CreateModal(
                transform,
                "TradeExhibitPicker",
                "选择要交易的展品",
                "确定",
                "取消",
                showConfirmButton: true);
            if (scaffold is null)
            {
                _exhibitPickerRoot = null;
                return;
            }

            _exhibitPickerRoot = scaffold.Root;

            if (scaffold.ConfirmButton is not null)
            {
                scaffold.ConfirmButton.onClick.RemoveAllListeners();
                scaffold.ConfirmButton.onClick.AddListener(() => HideExhibitPickerOverlay(true));
            }

            if (scaffold.CancelButton is not null)
            {
                scaffold.CancelButton.onClick.RemoveAllListeners();
                scaffold.CancelButton.onClick.AddListener(() => HideExhibitPickerOverlay(false));
            }

            ExhibitPickerTag tag = _exhibitPickerRoot.AddComponent<ExhibitPickerTag>();
            tag.Content = scaffold.Content;
            tag.ScrollRect = scaffold.ScrollRect;
            tag.EmptyText = scaffold.EmptyText;
            tag.RowButtonTemplate = scaffold.RowButtonTemplate;
            tag.TextTemplate = scaffold.TextTemplate;
        }
        catch
        {
            _exhibitPickerRoot = null;
        }
    }

    private sealed class ExhibitPickerTag : MonoBehaviour
    {
        public RectTransform Content;
        public ScrollRect ScrollRect;
        public TextMeshProUGUI EmptyText;
        public CommonButtonWidget RowButtonTemplate;
        public TextMeshProUGUI TextTemplate;
    }

    private void RebuildExhibitPickerList()
    {
        if (_exhibitPickerRoot is null)
        {
            return;
        }

        ExhibitPickerTag tag = _exhibitPickerRoot.GetComponent<ExhibitPickerTag>();
        if (tag is null || tag.Content is null)
        {
            return;
        }

        foreach (Transform child in tag.Content)
        {
            Destroy(child.gameObject);
        }

        GameRunController run = ActiveGameRun;
        List<Exhibit> tradable = new List<Exhibit>();
        try
        {
            if (run?.Player?.Exhibits is not null)
            {
                tradable = run.Player.Exhibits
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
            Plugin.Logger?.LogInfo($"[TradePanel] Exhibit picker empty: panelGameRun={(GameRun != null)}, activeGameRun={(run != null)}, exhibitCount={(run?.Player?.Exhibits?.Count ?? 0)}, selectedCount={_localExhibitOfferIds.Count}, tradeId={_tradeId ?? "<null>"}");
            tag.ScrollRect?.gameObject.SetActive(false);
            if (tag.EmptyText != null)
            {
                tag.EmptyText.gameObject.SetActive(true);
                tag.EmptyText.text = "没有可交易的展品。";
                Color c2 = tag.EmptyText.color;
                c2.a = 1f;
                tag.EmptyText.color = c2;
            }
            return;
        }

        if (tag.EmptyText != null)
        {
            tag.EmptyText.gameObject.SetActive(true);
            tag.EmptyText.text = string.Empty;
            Color c3 = tag.EmptyText.color;
            c3.a = 0f;
            tag.EmptyText.color = c3;
        }
        tag.ScrollRect?.gameObject.SetActive(true);

        foreach (var ex in tradable)
        {
            CreateExhibitRecordRow(tag, ex);
        }
    }

    private void CreateExhibitRecordRow(ExhibitPickerTag tag, Exhibit exhibit)
    {
        try
        {
            if (tag?.Content is null || exhibit is null)
            {
                return;
            }

            bool selected = _localExhibitOfferIds.Contains(exhibit.Id);
            Sprite sprite = null;
            try
            {
                sprite = ResourcesHelper.TryGetSprite<Exhibit>(exhibit.Id);
            }
            catch
            {
                sprite = null;
            }

            RuntimeSelectionPanelFactory.RuntimeSelectionRow row = RuntimeSelectionPanelFactory.CreateRow(
                tag.Content,
                tag.RowButtonTemplate,
                tag.TextTemplate,
                $"Ex_{exhibit.Id}",
                exhibit.Name,
                BuildExhibitSecondaryText(exhibit.Id, selected),
                sprite,
                selected,
                interactable: true);
            if (row is null)
            {
                return;
            }

            row.Button.onClick.RemoveAllListeners();
            row.Button.onClick.AddListener(() =>
            {
                bool nowSelected = !_localExhibitOfferIds.Contains(exhibit.Id);
                if (nowSelected)
                {
                    // 限制最多选择 1 个展品，取消之前选中的展品
                    _localExhibitOfferIds.Clear();
                    _localExhibitOfferIds.Add(exhibit.Id);
                }
                else
                {
                    _localExhibitOfferIds.Remove(exhibit.Id);
                }

                // 重新构建整个展品列表以刷新所有行的选中状态
                RebuildExhibitPickerList();
                RefreshOfferEditorTexts();
                TrySendOfferUpdate();
            });
        }
        catch
        {
            // 忽略
        }
    }

    private static string BuildExhibitSecondaryText(string exhibitId, bool selected)
    {
        return selected ? $"{exhibitId} · 已选择" : exhibitId;
    }

    private static void SetButtonText(CommonButtonWidget button, string label)
    {
        if (button is null)
            return;

        var tmp = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp is not null)
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
            if (_panel is not null) _panel._isApplyingState = _prev;
        }
    }

    /// <summary>
    /// 显示交易 UI 的协程方法，调用方可等待该协程直到面板被关闭。
    /// </summary>
    /// <param name="payload">交易配置参数。</param>
    /// <returns>用于等待面板关闭的协程。</returns>
    public IEnumerator ShowTradeAsync(TradePayload payload)
    {
        Plugin.Logger?.LogInfo($"[TradePanel] ShowTradeAsync enter: payloadNull={(payload is null)}, isVisibleBefore={IsVisible}, gameObjectActiveSelf={gameObject.activeSelf}, activeInHierarchy={gameObject.activeInHierarchy}");
        // 显示交易面板
        Show(payload);
        Plugin.Logger?.LogInfo($"[TradePanel] ShowTradeAsync after Show: isVisibleAfter={IsVisible}, gameObjectActiveSelf={gameObject.activeSelf}, activeInHierarchy={gameObject.activeInHierarchy}");
        // 在面板可见期间一直等待
        yield return new WaitWhile(() => IsVisible);
        Plugin.Logger?.LogInfo("[TradePanel] ShowTradeAsync exit: panel hidden.");
    }

    #endregion
}
