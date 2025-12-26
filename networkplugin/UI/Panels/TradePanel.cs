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
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Panels;

/// <summary>
/// 交易面板类
/// 处理玩家之间的物品交易功能
/// </summary>
public class TradePanel : UiPanel<TradePayload>, IInputActionHandler
{
	// 常量
	private const int DefaultMaxTradeSlots = 5;
	private const float TradeCompleteWaitTime = 2f;

	// UI组件
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

	// 交易数据
	private readonly List<Card> _player1OfferedCards = new List<Card>();
	private readonly List<Card> _player2OfferedCards = new List<Card>();
	private bool _isPlayer1Turn = true;
	private bool _tradeConfirmed;
	private TradePayload _payload;
	private int _maxTradeSlots = DefaultMaxTradeSlots;
	private CanvasGroup _canvasGroup;
	private bool _canCancel = true;

	// PanelLayer 属性 - 使用 Top 而不是不存在的 Popup
	public override PanelLayer Layer => PanelLayer.Top;

	public void Awake()
	{
		_canvasGroup = GetComponent<CanvasGroup>();
		if (_canvasGroup == null)
		{
			_canvasGroup = gameObject.AddComponent<CanvasGroup>();
		}

		// 注册按钮事件
		confirmButton?.button?.onClick.AddListener(OnConfirmTrade);
		cancelButton?.button?.onClick.AddListener(OnCancelTrade);
	}

	public override void OnLocaleChanged()
	{
		if (_payload != null)
        {
            UpdateUIStrings();
        }
    }

	protected override void OnShowing(TradePayload payload)
	{
		_payload = payload;
		_maxTradeSlots = payload?.MaxTradeSlots ?? DefaultMaxTradeSlots;
		_canCancel = payload?.CanCancel ?? true;

		// 重置交易数据
		ResetTradeData();

		// 设置UI状态
		player1NameText?.text = payload?.Player1Name ?? "Player 1";

        player2NameText?.text = payload?.Player2Name ?? "Player 2";

        // 设置取消按钮可见性
        cancelButton?.gameObject.SetActive(_canCancel);

        _canvasGroup?.interactable = true;

        UpdateUIStrings();
		UiManager.PushActionHandler(this);
	}

	protected override void OnShown()
	{
		// 面板显示完成后的处理
	}

	protected override void OnHiding()
	{
		_canvasGroup?.interactable = false;

        UiManager.PopActionHandler(this);
	}

	protected override void OnHided()
	{
		ResetTradeData();
		_payload = null;
	}

	private void UpdateUIStrings()
	{
		UpdateUIStatus("Trade.WaitingForItems".Localize());
	}

	private void ResetTradeData()
	{
		_player1OfferedCards.Clear();
		_player2OfferedCards.Clear();
		_tradeConfirmed = false;
		_isPlayer1Turn = true;

		// 清空交易槽位
		if (player1Slots != null)
		{
			foreach (TradeSlotWidget slot in player1Slots)
			{
				slot?.ClearSlot();
			}
		}

		if (player2Slots != null)
		{
			foreach (TradeSlotWidget slot in player2Slots)
			{
				slot?.ClearSlot();
			}
		}

		// 禁用确认按钮
		confirmButton?.button?.interactable = false;
	}

	private void UpdateUIStatus(string message)
	{
		statusText?.text = message;
    }

	/// <summary>
	/// 添加卡牌到交易
	/// </summary>
	public void AddCardToTrade(Card card, bool isPlayer1)
	{
		if (card == null)
        {
            return;
        }

        List<Card> offeredCards = isPlayer1 ? _player1OfferedCards : _player2OfferedCards;

		if (offeredCards.Count < _maxTradeSlots)
		{
			offeredCards.Add(card);
			UpdateTradeSlot(card, isPlayer1 ? player1Slots : player2Slots, offeredCards.Count - 1);
			CheckTradeReady();
		}
	}

	/// <summary>
	/// 从交易中移除卡牌
	/// </summary>
	public void RemoveCardFromTrade(Card card, bool isPlayer1)
	{
		if (card == null)
        {
            return;
        }

        List<Card> offeredCards = isPlayer1 ? _player1OfferedCards : _player2OfferedCards;
        TradeSlotWidget[] slots = isPlayer1 ? player1Slots : player2Slots;

		int index = offeredCards.IndexOf(card);
		if (index >= 0)
		{
			offeredCards.RemoveAt(index);

			// 重新排列剩余卡牌
			for (int i = index; i < offeredCards.Count; i++)
			{
				UpdateTradeSlot(offeredCards[i], slots, i);
			}

			// 清空最后一个槽位
			if (slots != null && offeredCards.Count < slots.Length)
			{
				slots[offeredCards.Count]?.ClearSlot();
			}

			CheckTradeReady();
		}
	}

	private void UpdateTradeSlot(Card card, TradeSlotWidget[] slots, int index)
	{
		if (slots != null && index >= 0 && index < slots.Length && slots[index] != null)
		{
			bool isPlayer1 = slots == player1Slots;
			slots[index].SetCard(card, (c) => RemoveCardFromTrade(c, isPlayer1));
		}
	}

	private void CheckTradeReady()
	{
		bool bothPlayersReady = _player1OfferedCards.Count > 0 && _player2OfferedCards.Count > 0;

		confirmButton?.button?.interactable = bothPlayersReady;

		if (bothPlayersReady)
		{
			UpdateUIStatus("Trade.ReadyToConfirm".Localize());
		}
		else
		{
			UpdateUIStatus("Trade.WaitingForItems".Localize());
		}
	}

	private void OnConfirmTrade()
	{
		if (_player1OfferedCards.Count > 0 && _player2OfferedCards.Count > 0)
		{
			_tradeConfirmed = true;
			UpdateUIStatus("Trade.Confirmed".Localize());
			StartCoroutine(ExecuteTrade());
		}
	}

	private void OnCancelTrade()
	{
		_tradeConfirmed = false;
		Hide();
	}

	public void OnCancel()
	{
		if (_canCancel)
		{
			OnCancelTrade();
		}
	}

	private IEnumerator ExecuteTrade()
	{
		// 禁用按钮以防止重复交易
		confirmButton?.button?.interactable = false;
		cancelButton?.button?.interactable = false;

        // 交换卡牌
        foreach (Card card in _player1OfferedCards)
		{
			// 从玩家1移除卡牌
			GameRun.RemoveDeckCard(card, false);
			// 添加到玩家2
			GameRun.AddDeckCard(card, true, new VisualSourceData
			{
				SourceType = VisualSourceType.CardSelect
			});
		}

		foreach (Card card in _player2OfferedCards)
		{
			// 从玩家2移除卡牌（需要在实际实现中处理）
			// TODO: 需要根据网络架构处理多玩家卡组操作
			GameRun.AddDeckCard(card, true, new VisualSourceData
			{
				SourceType = VisualSourceType.CardSelect
			});
		}

		UpdateUIStatus("Trade.Completed".Localize());

		// 发送网络事件通知其他玩家
		SendTradeEvent();

		yield return new WaitForSeconds(TradeCompleteWaitTime);

		Hide();
	}

	private void SendTradeEvent()
	{
		try
		{
            // 发送交易完成事件到网络
            ISynchronizationManager syncManager = ModService.ServiceProvider.GetService<ISynchronizationManager>();
			if (syncManager != null)
			{
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
			Debug.LogError($"[TradePanel] Failed to send trade event: {ex.Message}");
		}
	}

	/// <summary>
	/// 显示交易UI的协程方法
	/// </summary>
	public IEnumerator ShowTradeAsync(TradePayload payload)
	{
		Show(payload);
		yield return new WaitWhile(() => IsVisible);
	}
}
