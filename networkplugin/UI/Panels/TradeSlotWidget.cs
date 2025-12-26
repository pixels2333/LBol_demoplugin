using LBoL.Core.Cards;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Panels;

/// <summary>
/// 交易槽位控件类
/// 基于游戏 UI 模式显示单个可交易的卡牌
/// 继承 CommonButtonWidget 以获得游戏标准的按钮行为
/// </summary>
public class TradeSlotWidget : CommonButtonWidget
{
	[SerializeField]
	private Image cardIcon;

	[SerializeField]
	private TextMeshProUGUI cardNameText;

	[SerializeField]
	private Image selectedBorder;

	[SerializeField]
	private GameObject lockedOverlay;

	[SerializeField]
	private Color selectedColor = new Color(0.4f, 0.6f, 0.8f, 0.9f);

	[SerializeField]
	private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

	private Card _currentCard;
	private System.Action<Card> _onRemoveCard;
	private Image _bgImage;

	public void SetCard(Card card, System.Action<Card> removeCallback = null)
	{
		_currentCard = card;
		_onRemoveCard = removeCallback;

		if (card != null)
		{
			cardNameText?.text = card.Name;

			// 隐藏卡牌图标（因为 Image 组件不直接支持卡牌的 Sprite）
			cardIcon?.gameObject.SetActive(false);

			// 启用按钮交互
			if (button != null)
			{
				button.interactable = true;
			}

			// 注册移除事件
			if (button != null)
			{
				button.onClick.AddListener(OnRemoveClicked);
			}
		}
		else
		{
			ClearSlot();
		}
	}

	public void ClearSlot()
	{
		_currentCard = null;
		_onRemoveCard = null;

		cardNameText?.text = "";
		cardIcon?.gameObject.SetActive(false);

		if (button != null)
		{
			button.interactable = false;
			// 移除所有监听器
			button.onClick.RemoveAllListeners();
		}

		SetSelected(false);

		if (lockedOverlay != null)
		{
			lockedOverlay.SetActive(false);
		}
	}

	public void SetLocked(bool locked)
	{
		if (lockedOverlay != null)
		{
			lockedOverlay.SetActive(locked);
		}

		if (button != null)
		{
			button.interactable = !locked;
		}
	}

	public void SetSelected(bool selected)
	{
		if (_bgImage == null)
		{
			_bgImage = GetComponent<Image>();
		}

		if (_bgImage != null)
		{
			_bgImage.color = selected ? selectedColor : normalColor;
		}

		if (selectedBorder != null)
		{
			selectedBorder.gameObject.SetActive(selected);
		}
	}

	private void OnRemoveClicked()
	{
		if (_currentCard != null && _onRemoveCard != null)
		{
			_onRemoveCard(_currentCard);
			ClearSlot();
		}
	}
}
