using System;
using System.Collections;
using System.Collections.Generic;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Widgets;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Core;
using NetworkPlugin.Network;
using NetworkPlugin.UI.Widgets;
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
        // 双方都至少放入一张卡牌才允许确认交易
        bool bothPlayersReady = _player1OfferedCards.Count > 0 && _player2OfferedCards.Count > 0;

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

    #endregion

    #region 按钮与输入处理

    /// <summary>
    /// 点击“确认交易”按钮时回调。
    /// </summary>
    private void OnConfirmTrade()
    {
        // 再次检查双方是否都放入了卡牌
        if (_player1OfferedCards.Count > 0 && _player2OfferedCards.Count > 0)
        {
            _tradeConfirmed = true;
            // 更新提示为“交易已确认”
            UpdateUIStatus("Trade.Confirmed".Localize());
            // 开始执行交易逻辑的协程
            StartCoroutine(ExecuteTrade());
        }
    }

    /// <summary>
    /// 点击“取消交易”按钮时回调。
    /// </summary>
    private void OnCancelTrade()
    {
        // 标记为未确认，并直接关闭面板
        _tradeConfirmed = false;
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
            // TODO: 需要根据网络架构处理多玩家卡组操作
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
            // 从 IoC 容器中获取同步管理器（若存在）
            ISynchronizationManager syncManager = ModService.ServiceProvider.GetService<ISynchronizationManager>();
            if (syncManager != null)
            {
                // 构造简单的交易完成事件数据
                Dictionary<string, object> tradeData = new Dictionary<string, object>
                {
                    ["EventType"] = "TradeCompleted",
                    ["Player1Cards"] = _player1OfferedCards.Count,
                    ["Player2Cards"] = _player2OfferedCards.Count,
                    ["Timestamp"] = DateTime.Now.Ticks
                };

                // TODO: 根据实际的网络管理器实现发送逻辑
                // syncManager.SendGameEvent(tradeData);
            }
        }
        catch (Exception ex)
        {
            // 打印网络事件发送失败的错误日志
            Debug.LogError($"[TradePanel] Failed to send trade event: {ex.Message}");
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
