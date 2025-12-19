using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using UnityEngine;

namespace NetworkPlugin.UI.Panels;

/// <summary>
/// 交易面板类
/// 处理玩家之间的物品交易功能
/// </summary>
public class TradePanel : UiPanel
{
    public override PanelLayer Layer => PanelLayer.Popup;

    // UI组件
    [SerializeField] private Transform player1TradeArea;
    [SerializeField] private Transform player2TradeArea;
    [SerializeField] private TradeSlot[] player1Slots;
    [SerializeField] private TradeSlot[] player2Slots;
    [SerializeField] private UnityEngine.UI.Button confirmButton;
    [SerializeField] private UnityEngine.UI.Button cancelButton;
    [SerializeField] private UnityEngine.UI.Text statusText;

    // 交易数据
    private List<Card> player1OfferedCards = new List<Card>();
    private List<Card> player2OfferedCards = new List<Card>();
    private bool isPlayer1Turn = true;
    private bool tradeConfirmed = false;

    public void Awake()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmTrade);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelTrade);
    }

    public IEnumerator ShowTradeUI()
    {
        // 重置交易数据
        ResetTradeData();

        // 设置UI状态
        UpdateUIStatus("等待玩家1选择交易物品...");

        // 显示面板
        Show();

        // 等待交易完成
        yield return new WaitUntil(() => tradeConfirmed || this.IsHidden);

        // 处理交易结果
        if (tradeConfirmed)
        {
            yield return ExecuteTrade();
        }

        Hide();
    }

    private void ResetTradeData()
    {
        player1OfferedCards.Clear();
        player2OfferedCards.Clear();
        tradeConfirmed = false;
        isPlayer1Turn = true;

        // 清空交易槽位
        if (player1Slots != null)
        {
            foreach (var slot in player1Slots)
            {
                slot?.ClearSlot();
            }
        }

        if (player2Slots != null)
        {
            foreach (var slot in player2Slots)
            {
                slot?.ClearSlot();
            }
        }
    }

    private void UpdateUIStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    public void AddCardToTrade(Card card, bool isPlayer1)
    {
        if (card == null) return;

        if (isPlayer1)
        {
            if (player1OfferedCards.Count < GetMaxTradeSlots())
            {
                player1OfferedCards.Add(card);
                UpdateTradeSlot(card, player1Slots, player1OfferedCards.Count - 1);
            }
        }
        else
        {
            if (player2OfferedCards.Count < GetMaxTradeSlots())
            {
                player2OfferedCards.Add(card);
                UpdateTradeSlot(card, player2Slots, player2OfferedCards.Count - 1);
            }
        }

        CheckTradeReady();
    }

    private void UpdateTradeSlot(Card card, TradeSlot[] slots, int index)
    {
        if (slots != null && index >= 0 && index < slots.Length && slots[index] != null)
        {
            slots[index].SetCard(card);
        }
    }

    private int GetMaxTradeSlots()
    {
        return 5; // 每个玩家最多交易5张卡牌
    }

    private void CheckTradeReady()
    {
        bool bothPlayersReady = player1OfferedCards.Count > 0 && player2OfferedCards.Count > 0;

        if (confirmButton != null)
            confirmButton.interactable = bothPlayersReady;

        if (bothPlayersReady)
            UpdateUIStatus("双方都已选择物品，点击确认完成交易");
    }

    private void OnConfirmTrade()
    {
        if (player1OfferedCards.Count > 0 && player2OfferedCards.Count > 0)
        {
            tradeConfirmed = true;
            UpdateUIStatus("交易已确认，正在处理...");
        }
    }

    private void OnCancelTrade()
    {
        tradeConfirmed = false;
        Hide();
    }

    private IEnumerator ExecuteTrade()
    {
        // 交换卡牌
        foreach (var card in player1OfferedCards)
        {
            // 从玩家1移除卡牌
            base.GameRun.RemoveDeckCard(card, false);
            // 添加到玩家2
            base.GameRun.AddDeckCard(card, true, new VisualSourceData
            {
                SourceType = VisualSourceType.CardSelect
            });
        }

        foreach (var card in player2OfferedCards)
        {
            // 从玩家2移除卡牌
            // base.GameRun2.RemoveDeckCard(card, false);
            // 添加到玩家1
            base.GameRun.AddDeckCard(card, true, new VisualSourceData
            {
                SourceType = VisualSourceType.CardSelect
            });
        }

        UpdateUIStatus("交易完成！");

        // 发送网络事件通知其他玩家
        SendTradeEvent();

        yield return new WaitForSeconds(2f);
    }

    private void SendTradeEvent()
    {
        try
        {
            // 发送交易完成事件到网络
            var syncManager = NetworkPlugin.Core.ModService.ServiceProvider.GetService<NetworkPlugin.Core.ISynchronizationManager>();
            if (syncManager != null)
            {
                var tradeData = new Dictionary<string, object>
                {
                    ["EventType"] = "TradeCompleted",
                    ["Player1Cards"] = player1OfferedCards.Count,
                    ["Player2Cards"] = player2OfferedCards.Count,
                    ["Timestamp"] = DateTime.Now.Ticks
                };

                // 这里需要根据实际的网络管理器实现发送逻辑
                // syncManager.SendGameEvent(tradeData);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TradePanel] 发送交易事件失败: {ex.Message}");
        }
    }
}

/// <summary>
/// 交易槽位类
/// 显示单个可交易的物品
/// </summary>
public class TradeSlot : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image cardIcon;
    [SerializeField] private UnityEngine.UI.Text cardName;
    [SerializeField] private UnityEngine.UI.Button removeButton;

    private Card currentCard;
    private Action<Card> onRemoveCard;

    public void Awake()
    {
        if (removeButton != null)
            removeButton.onClick.AddListener(OnRemoveClicked);
    }

    public void SetCard(Card card, Action<Card> removeCallback = null)
    {
        currentCard = card;
        onRemoveCard = removeCallback;

        if (card != null)
        {
            if (cardName != null)
                cardName.text = card.Name;

            // 设置卡牌图标
            if (cardIcon != null && card.Sprite != null)
                cardIcon.sprite = card.Sprite;
        }
    }

    public void ClearSlot()
    {
        currentCard = null;
        onRemoveCard = null;

        if (cardName != null)
            cardName.text = "";

        if (cardIcon != null)
            cardIcon.sprite = null;
    }

    private void OnRemoveClicked()
    {
        if (currentCard != null && onRemoveCard != null)
        {
            onRemoveCard(currentCard);
            ClearSlot();
        }
    }
}