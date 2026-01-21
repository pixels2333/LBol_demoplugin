using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Core;
using NetworkPlugin.Network;
using NetworkPlugin.UI.Widgets;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;
using NetworkPlugin.Patch.UI;
using TMPro;
using UnityEngine;
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
    /// 当前是否轮到玩家 1（如需回合制控制时可扩展）。
    /// </summary>
    private bool _isPlayer1Turn = true;

    /// <summary>
    /// 是否已经点击了确认交易。
    /// </summary>
    private bool _tradeConfirmed;

    private string _tradeId;
    private string _selfPlayerId;
    private string _playerAId;
    private string _playerBId;
    private bool _isApplyingState;
    private bool _subscribedToTrade;

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

    // Runtime offer editor overlay (money + exhibits).
    private GameObject _offerEditorRoot;
    private TextMeshProUGUI _moneyValueText;
    private TextMeshProUGUI _exhibitValueText;

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
        // Network-only: if not connected, show a message and bail out.
        if (!TryEnsureNetworkConnected())
        {
            Hide();
            return;
        }

        // 缓存本次交易的参数
        _payload = payload;
        // 根据 payload 设置允许的最大交易卡位
        _maxTradeSlots = payload?.MaxTradeSlots ?? DefaultMaxTradeSlots;
        // 是否允许玩家取消本次交易
        _canCancel = payload?.CanCancel ?? true;

        // 重置交易数据和显示
        ResetTradeData();

        // 设置玩家名称显示（使用默认值兜底）
        if (player1NameText != null)
        {
            player1NameText.text = payload?.Player1Name ?? "Player 1";
        }
        if (player2NameText != null)
        {
            player2NameText.text = payload?.Player2Name ?? "Player 2";
        }

        // 根据配置显示/隐藏取消按钮
        cancelButton?.gameObject.SetActive(_canCancel);

        // 允许面板交互
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = true;
        }

        // 刷新本地化文案
        UpdateUIStrings();

        // 联机：初始化交易参与者与订阅同步。
        SetupTradeSession(payload);

        // 将本面板注册为输入处理器（接收取消等操作）
        UiManager.PushActionHandler(this);
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
        UiManager.PopActionHandler(this);

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
        UpdateUIStatus("Trade.WaitingForItems".Localize());
    }

    /// <summary>
    /// 将交易数据重置并清空所有槽位显示。
    /// </summary>
    private void ResetTradeData()
    {
        // 清空两侧玩家已放入的卡牌列表
        _player1OfferedCards.Clear();
        _player2OfferedCards.Clear();
        _tradeConfirmed = false;
        _isPlayer1Turn = true;

        _tradeId = null;
        _playerAId = null;
        _playerBId = null;
        _selfPlayerId = null;
        _isApplyingState = false;

        _localMoneyOffer = 0;
        _localExhibitOfferIds.Clear();
        _lastTradeStatus = null;
        _lastPreparingHandledTimestamp = 0;

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

        // 根据当前是否满足条件更新确认按钮状态
        if (confirmButton?.button != null)
        {
            confirmButton.button.interactable = bothPlayersReady;
        }

        // 更新提示文本
        if (bothPlayersReady)
        {
            UpdateUIStatus("Trade.ReadyToConfirm".Localize());
        }
        else
        {
            UpdateUIStatus("Trade.WaitingForItems".Localize());
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
                return;
            }

            _tradeConfirmed = true;
            UpdateUIStatus("Trade.Confirmed".Localize());
            TrySendConfirm();
            return;
        }

        // 单机：再次检查双方是否都放入了卡牌
        if (_player1OfferedCards.Count <= 0 || _player2OfferedCards.Count <= 0)
        {
            return;
        }

        // 单机：沿用本地交易
        _tradeConfirmed = true;
        UpdateUIStatus("Trade.Confirmed".Localize());
        StartCoroutine(ExecuteTrade());
    }

    /// <summary>
    /// 点击“取消交易”按钮时回调。
    /// </summary>
    private void OnCancelTrade()
    {
        // 标记为未确认，并直接关闭面板
        _tradeConfirmed = false;

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
            if (client == null || !client.IsConnected)
            {
                return;
            }

            NetworkIdentityTracker.EnsureSubscribed(client);
            TradeSyncPatch.EnsureSubscribed(client);

            _selfPlayerId = NetworkIdentityTracker.GetSelfPlayerId();
            if (string.IsNullOrWhiteSpace(_selfPlayerId))
            {
                return;
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

            TrySubscribeTradeEvents();

            // 若本端是参与者之一，发起会话（Host 会裁决并广播状态）。
            if (!string.IsNullOrWhiteSpace(_playerBId))
            {
                TradeSyncPatch.RequestStartTrade(_tradeId, _playerAId, _playerBId, _maxTradeSlots);
                TradeSyncPatch.RequestSnapshot(_tradeId, _selfPlayerId);
            }
        }
        catch
        {
            // ignored
        }
    }

    private bool TryEnsureNetworkConnected()
    {
        try
        {
            INetworkClient client = ModService.ServiceProvider.GetService<INetworkClient>();
            if (client == null || !client.IsConnected)
            {
                TryShowTopMessage("Trading is only available in network mode.");
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

            // Build overlay lazily.
            EnsurePartnerPickerOverlay();
            if (_partnerPickerRoot == null)
            {
                TryShowTopMessage("Trade partner picker UI is not available.");
                Hide();
                return;
            }

            _partnerPickerActive = true;
            _partnerPickerRoot.SetActive(true);

            // Disable underlying trade UI until a partner is selected.
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
            }

            UpdateUIStatus("Select trade partner");
            RebuildPartnerPickerList();
        }
        catch
        {
            // ignored
        }
    }

    private void HidePartnerPickerOverlay()
    {
        _partnerPickerActive = false;
        if (_partnerPickerRoot != null)
        {
            _partnerPickerRoot.SetActive(false);
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = true;
        }
    }

    private void EnsurePartnerPickerOverlay()
    {
        if (_partnerPickerRoot != null)
        {
            return;
        }

        try
        {
            Transform parent = transform;

            _partnerPickerRoot = new GameObject("TradePartnerPicker");
            _partnerPickerRoot.transform.SetParent(parent, false);
            _partnerPickerRoot.SetActive(false);

            RectTransform rootRect = _partnerPickerRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            Image bg = _partnerPickerRoot.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.6f);
            bg.raycastTarget = true;

            // Title
            GameObject titleGo = new GameObject("Title");
            titleGo.transform.SetParent(_partnerPickerRoot.transform, false);
            var title = titleGo.AddComponent<TextMeshProUGUI>();
            title.text = "Select trade partner";
            title.alignment = TextAlignmentOptions.Center;
            title.fontSize = 28;
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.85f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // Container
            GameObject containerGo = new GameObject("List");
            containerGo.transform.SetParent(_partnerPickerRoot.transform, false);
            RectTransform containerRect = containerGo.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.1f, 0.2f);
            containerRect.anchorMax = new Vector2(0.9f, 0.82f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = containerGo.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 10f;
            vlg.padding = new RectOffset(10, 10, 10, 10);

            ContentSizeFitter fitter = containerGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Store container as a component for rebuilding.
            containerGo.AddComponent<PartnerPickerTag>();

            // Cancel button
            GameObject cancelGo = new GameObject("Cancel");
            cancelGo.transform.SetParent(_partnerPickerRoot.transform, false);
            RectTransform cancelRect = cancelGo.AddComponent<RectTransform>();
            cancelRect.anchorMin = new Vector2(0.35f, 0.08f);
            cancelRect.anchorMax = new Vector2(0.65f, 0.16f);
            cancelRect.offsetMin = Vector2.zero;
            cancelRect.offsetMax = Vector2.zero;

            Image cancelBg = cancelGo.AddComponent<Image>();
            cancelBg.color = new Color(1f, 1f, 1f, 0.15f);
            Button cancelBtn = cancelGo.AddComponent<Button>();
            cancelBtn.targetGraphic = cancelBg;
            cancelBtn.onClick.AddListener(() =>
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

            GameObject cancelLabelGo = new GameObject("Label");
            cancelLabelGo.transform.SetParent(cancelGo.transform, false);
            var cancelLabel = cancelLabelGo.AddComponent<TextMeshProUGUI>();
            cancelLabel.text = "Cancel";
            cancelLabel.alignment = TextAlignmentOptions.Center;
            cancelLabel.fontSize = 24;
            RectTransform cancelLabelRect = cancelLabel.GetComponent<RectTransform>();
            cancelLabelRect.anchorMin = Vector2.zero;
            cancelLabelRect.anchorMax = Vector2.one;
            cancelLabelRect.offsetMin = Vector2.zero;
            cancelLabelRect.offsetMax = Vector2.zero;
        }
        catch
        {
            _partnerPickerRoot = null;
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
            if (tag == null)
            {
                return;
            }

            Transform container = tag.transform;
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            string selfId = _selfPlayerId ?? NetworkIdentityTracker.GetSelfPlayerId();
            var players = OtherPlayersOverlayPatch.SnapshotPlayers();
            var candidates = players
                .Where(p => !string.IsNullOrWhiteSpace(p.PlayerId))
                .Where(p => !string.Equals(p.PlayerId, selfId, StringComparison.Ordinal))
                .Where(p => p.IsConnected)
                .ToList();

            if (candidates.Count == 0)
            {
                var txtGo = new GameObject("Empty");
                txtGo.transform.SetParent(container, false);
                var txt = txtGo.AddComponent<TextMeshProUGUI>();
                txt.text = "No other connected players.";
                txt.alignment = TextAlignmentOptions.Center;
                txt.fontSize = 22;
                return;
            }

            foreach (var p in candidates)
            {
                CreatePartnerButton(container, p.PlayerId, string.IsNullOrWhiteSpace(p.PlayerName) ? p.PlayerId : p.PlayerName, p.IsHost);
            }
        }
        catch
        {
            // ignored
        }
    }

    private void CreatePartnerButton(Transform parent, string playerId, string playerName, bool isHost)
    {
        GameObject go = new GameObject($"Player_{playerId}");
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 56f);

        Image bg = go.AddComponent<Image>();
        bg.color = isHost ? new Color(1f, 0.95f, 0.4f, 0.18f) : new Color(1f, 1f, 1f, 0.12f);
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;

        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(go.transform, false);
        var label = labelGo.AddComponent<TextMeshProUGUI>();
        label.text = isHost ? $"{playerName} [Host]" : playerName;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 22;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        btn.onClick.AddListener(() =>
        {
            try
            {
                _playerAId = _selfPlayerId;
                _playerBId = playerId;

                if (player1NameText != null)
                {
                    player1NameText.text = _payload?.Player1Name ?? "Player 1";
                }
                if (player2NameText != null)
                {
                    player2NameText.text = playerName;
                }

                HidePartnerPickerOverlay();

                TradeSyncPatch.RequestStartTrade(_tradeId, _playerAId, _playerBId, _maxTradeSlots);
                TradeSyncPatch.RequestSnapshot(_tradeId, _selfPlayerId);
            }
            catch
            {
                // ignored
            }
        });
    }

    // Marker component to locate the list container under the runtime overlay.
    private sealed class PartnerPickerTag : MonoBehaviour
    {
    }

    private bool TryIsNetworkTrade(out bool localIsA)
    {
        localIsA = false;
        try
        {
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

            confirmButton.button.interactable = state.Status == TradeSyncPatch.TradeStatus.Open && localHasOffer && remoteHasOffer;
        }

        if (state.Status == TradeSyncPatch.TradeStatus.Open)
        {
            UpdateUIStatus((state.OfferA?.Count ?? 0) > 0 && (state.OfferB?.Count ?? 0) > 0
                ? "Trade.ReadyToConfirm".Localize()
                : "Trade.WaitingForItems".Localize());
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

            Image bg = _offerEditorRoot.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.35f);
            bg.raycastTarget = false;

            // Money row
            var moneyLabel = CreateSmallText(_offerEditorRoot.transform, "MoneyLabel", "Money:");
            SetRect(moneyLabel.rectTransform, 0.02f, 0.52f, 0.32f, 0.92f);

            _moneyValueText = CreateSmallText(_offerEditorRoot.transform, "MoneyValue", "0");
            _moneyValueText.alignment = TextAlignmentOptions.Left;
            SetRect(_moneyValueText.rectTransform, 0.32f, 0.52f, 0.62f, 0.92f);

            Button minus = CreateTinyButton(_offerEditorRoot.transform, "MoneyMinus", "-");
            SetRect(minus.GetComponent<RectTransform>(), 0.62f, 0.56f, 0.72f, 0.92f);
            minus.onClick.AddListener(() =>
            {
                if (!CanEditOffer()) return;
                _localMoneyOffer = Math.Max(0, _localMoneyOffer - 1);
                RefreshOfferEditorTexts();
                TrySendOfferUpdate();
            });

            Button plus = CreateTinyButton(_offerEditorRoot.transform, "MoneyPlus", "+");
            SetRect(plus.GetComponent<RectTransform>(), 0.72f, 0.56f, 0.82f, 0.92f);
            plus.onClick.AddListener(() =>
            {
                if (!CanEditOffer()) return;
                int current = GameRun?.Money ?? 0;
                _localMoneyOffer = Math.Min(MaxMoneyOffer, Math.Min(current, _localMoneyOffer + 1));
                RefreshOfferEditorTexts();
                TrySendOfferUpdate();
            });

            // Exhibit row
            var exLabel = CreateSmallText(_offerEditorRoot.transform, "ExLabel", "Exhibits:");
            SetRect(exLabel.rectTransform, 0.02f, 0.08f, 0.32f, 0.48f);

            _exhibitValueText = CreateSmallText(_offerEditorRoot.transform, "ExValue", "0");
            _exhibitValueText.alignment = TextAlignmentOptions.Left;
            SetRect(_exhibitValueText.rectTransform, 0.32f, 0.08f, 0.62f, 0.48f);

            Button editEx = CreateTinyButton(_offerEditorRoot.transform, "ExEdit", "Edit");
            SetRect(editEx.GetComponent<RectTransform>(), 0.62f, 0.12f, 0.82f, 0.48f);
            editEx.onClick.AddListener(() =>
            {
                if (!CanEditOffer()) return;
                ShowExhibitPickerOverlay();
            });

            RefreshOfferEditorTexts();
        }
        catch
        {
            _offerEditorRoot = null;
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
        if (_moneyValueText != null)
        {
            _moneyValueText.text = _localMoneyOffer.ToString();
        }

        if (_exhibitValueText != null)
        {
            _exhibitValueText.text = _localExhibitOfferIds.Count.ToString();
        }
    }

    private TextMeshProUGUI CreateSmallText(Transform parent, string name, string text)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 18;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }

    private Button CreateTinyButton(Transform parent, string name, string label)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.15f);
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(go.transform, false);
        var tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        RectTransform r = tmp.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;

        return btn;
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
            return;
        }

        RebuildExhibitPickerList();
        _exhibitPickerRoot.SetActive(true);
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = false;
        }
    }

    private void HideExhibitPickerOverlay(bool apply)
    {
        if (_exhibitPickerRoot != null)
        {
            _exhibitPickerRoot.SetActive(false);
        }
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = true;
        }

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
            _exhibitPickerRoot = new GameObject("TradeExhibitPicker");
            _exhibitPickerRoot.transform.SetParent(transform, false);
            _exhibitPickerRoot.SetActive(false);

            RectTransform rootRect = _exhibitPickerRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            Image bg = _exhibitPickerRoot.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.65f);
            bg.raycastTarget = true;

            var title = CreateSmallText(_exhibitPickerRoot.transform, "Title", "Select exhibits to offer");
            title.fontSize = 26;
            SetRect(title.rectTransform, 0.1f, 0.88f, 0.9f, 0.97f);

            GameObject listGo = new GameObject("List");
            listGo.transform.SetParent(_exhibitPickerRoot.transform, false);
            RectTransform listRect = listGo.AddComponent<RectTransform>();
            SetRect(listRect, 0.1f, 0.18f, 0.9f, 0.86f);
            VerticalLayoutGroup vlg = listGo.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 8f;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            listGo.AddComponent<ExhibitPickerTag>();

            Button cancel = CreateTinyButton(_exhibitPickerRoot.transform, "Cancel", "Cancel");
            SetRect(cancel.GetComponent<RectTransform>(), 0.25f, 0.06f, 0.45f, 0.14f);
            cancel.onClick.AddListener(() => HideExhibitPickerOverlay(false));

            Button ok = CreateTinyButton(_exhibitPickerRoot.transform, "OK", "OK");
            SetRect(ok.GetComponent<RectTransform>(), 0.55f, 0.06f, 0.75f, 0.14f);
            ok.onClick.AddListener(() => HideExhibitPickerOverlay(true));
        }
        catch
        {
            _exhibitPickerRoot = null;
        }
    }

    private sealed class ExhibitPickerTag : MonoBehaviour
    {
    }

    private void RebuildExhibitPickerList()
    {
        if (_exhibitPickerRoot == null)
        {
            return;
        }

        ExhibitPickerTag tag = _exhibitPickerRoot.GetComponentInChildren<ExhibitPickerTag>(true);
        if (tag == null)
        {
            return;
        }

        Transform container = tag.transform;
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
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
            var empty = CreateSmallText(container, "Empty", "No tradable exhibits.");
            empty.alignment = TextAlignmentOptions.Center;
            return;
        }

        foreach (var ex in tradable)
        {
            CreateExhibitToggle(container, ex);
        }
    }

    private void CreateExhibitToggle(Transform parent, Exhibit exhibit)
    {
        GameObject row = new GameObject($"Ex_{exhibit.Id}");
        row.transform.SetParent(parent, false);
        RectTransform rect = row.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 44f);

        Image bg = row.AddComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0.10f);

        Toggle toggle = row.AddComponent<Toggle>();
        toggle.targetGraphic = bg;
        toggle.isOn = _localExhibitOfferIds.Contains(exhibit.Id);

        // Simple checkmark by tinting bg.
        toggle.onValueChanged.AddListener(on =>
        {
            if (on)
            {
                _localExhibitOfferIds.Add(exhibit.Id);
                bg.color = new Color(0.4f, 0.8f, 0.4f, 0.18f);
            }
            else
            {
                _localExhibitOfferIds.Remove(exhibit.Id);
                bg.color = new Color(1f, 1f, 1f, 0.10f);
            }
        });

        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(row.transform, false);
        var label = labelGo.AddComponent<TextMeshProUGUI>();
        label.text = $"{exhibit.Name} ({exhibit.Id})";
        label.fontSize = 18;
        label.alignment = TextAlignmentOptions.Left;
        label.color = Color.white;
        label.raycastTarget = false;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(12f, 0f);
        labelRect.offsetMax = new Vector2(-12f, 0f);

        // initialize bg tint
        bg.color = toggle.isOn ? new Color(0.4f, 0.8f, 0.4f, 0.18f) : new Color(1f, 1f, 1f, 0.10f);
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
