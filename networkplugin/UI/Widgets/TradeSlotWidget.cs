using System;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation;
using LBoL.Presentation.InputSystemExtend;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Widgets;

/// <summary>
/// 交易槽位控件类
/// 基于游戏 UI 模式显示单个可交易的卡牌
/// 继承 CommonButtonWidget 以获得游戏标准的按钮行为
/// </summary>
public class TradeSlotWidget : CommonButtonWidget
	, ICardTooltipSource
	, IPointerEnterHandler
	, IPointerExitHandler
	, IPointerClickHandler
{
	[SerializeField]
	private Image cardIcon;

	[SerializeField]
	private RawImage cardImage;

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
	private Action<Card> _onRemoveCard;
	private Image _bgImage;
	private bool _capturedButtonStyle;
	private Selectable.Transition _originalTransition;
	private ColorBlock _originalColors;
	private int _tooltipId;

	private static readonly TooltipPosition[] DefaultTooltipPositions = new TooltipPosition[]
	{
		new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max),
		new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Min),
		new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
		new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center),
		new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center)
	};

	public Card Card => _currentCard;
	public RectTransform RectTransform => transform as RectTransform;
	public TooltipPosition[] TooltipPositions => DefaultTooltipPositions;

	internal void BindRuntime(TextMeshProUGUI runtimeCardNameText, RawImage runtimeCardImage = null)
	{
		// Runtime-created slots won't have prefab-wired references.
		cardNameText = runtimeCardNameText;
		cardImage = runtimeCardImage;
	}

	private Image ResolveBackgroundImage()
	{
		if (_bgImage != null)
		{
			return _bgImage;
		}

		try
		{
			if (button != null && button.targetGraphic is Image img)
			{
				_bgImage = img;
				return _bgImage;
			}
		}
		catch
		{
			// ignored
		}

		_bgImage = GetComponent<Image>();
		return _bgImage;
	}

	private void CaptureButtonStyleOnce()
	{
		if (_capturedButtonStyle)
		{
			return;
		}

		try
		{
			if (button != null)
			{
				_originalTransition = button.transition;
				_originalColors = button.colors;
				_capturedButtonStyle = true;
			}
		}
		catch
		{
			// ignored
		}
	}

	private void ApplyEmptyVisual()
	{
		try
		{
			var bg = ResolveBackgroundImage();
			if (bg != null)
			{
				// Disable rendering & raycast surface completely when empty.
				bg.enabled = false;
				bg.raycastTarget = false;
				var c = bg.color;
				c.a = 0f;
				bg.color = c;
			}
		}
		catch
		{
			// ignored
		}

		try
		{
			if (button != null)
			{
				CaptureButtonStyleOnce();
				button.transition = Selectable.Transition.None;
			}
		}
		catch
		{
			// ignored
		}
	}

	private void ApplyFilledVisual()
	{
		try
		{
			var bg = ResolveBackgroundImage();
			if (bg != null)
			{
				bg.enabled = true;
				bg.raycastTarget = true;
				// Alpha/tint is handled by SetSelected().
			}
		}
		catch
		{
			// ignored
		}

		try
		{
			if (button != null)
			{
				CaptureButtonStyleOnce();
				button.transition = _originalTransition;
				button.colors = _originalColors;
			}
		}
		catch
		{
			// ignored
		}
	}

	public void SetCard(Card card, Action<Card> removeCallback = null)
	{
		_currentCard = card;
		_onRemoveCard = removeCallback;

		// Avoid accumulating listeners if SetCard is called multiple times.
		button?.onClick.RemoveListener(OnRemoveClicked);

                if (card != null)
                {
						cardNameText?.text = card.Name;

			// Use the game's own card textures if available.
			TrySetCardImage(card);

			// Hide icon placeholder when using textures.
			cardIcon?.gameObject.SetActive(false);

			ApplyFilledVisual();

			// 启用按钮交互
			button?.interactable = true;

			// 注册移除事件
			button?.onClick.AddListener(OnRemoveClicked);
		}
		else
		{
			ClearSlot();
		}
	}

	private void TrySetCardImage(Card card)
	{
		try
		{
			if (cardImage == null)
			{
				return;
			}

			if (card == null)
			{
				cardImage.texture = null;
				cardImage.gameObject.SetActive(false);
				return;
			}

			string preferredCardIllustrator = GameMaster.GetPreferredCardIllustrator(card);
			string imageId;
			if (!string.IsNullOrWhiteSpace(card.Config?.UpgradeImageId) && card.IsUpgraded)
			{
				imageId = card.Config.UpgradeImageId;
			}
			else
			{
				imageId = string.IsNullOrWhiteSpace(card.Config?.ImageId) ? card.Id : card.Config.ImageId;
			}

			// Mirror vanilla RecordCardCell: TryGetCardImage(imageId + preferredIllustrator)
			Texture tex = ResourcesHelper.TryGetCardImage(imageId + preferredCardIllustrator);
			if (tex == null)
			{
				tex = ResourcesHelper.TryGetCardImage(imageId);
			}

			cardImage.texture = tex;
			cardImage.color = Color.white;
			cardImage.raycastTarget = false;
			cardImage.gameObject.SetActive(tex != null);
		}
		catch
		{
			try
			{
				if (cardImage != null)
				{
					cardImage.texture = null;
					cardImage.gameObject.SetActive(false);
				}
			}
			catch
			{
				// ignored
			}
		}
	}

	public void ClearSlot()
	{
                _currentCard = null;
                _onRemoveCard = null;

                cardNameText?.text = "";
                cardIcon?.gameObject.SetActive(false);
				try
				{
					if (cardImage != null)
					{
						cardImage.texture = null;
						cardImage.gameObject.SetActive(false);
					}
				}
				catch
				{
					// ignored
				}

		if (button != null)
		{
			button.interactable = false;
			// 移除所有监听器
			button.onClick.RemoveAllListeners();
		}

		SetSelected(false);
		ApplyEmptyVisual();

		lockedOverlay?.SetActive(false);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		try
		{
			if (_currentCard == null || !UiManager.IsInitialized)
			{
				return;
			}

			_tooltipId = TooltipsLayer.ShowCard(this, true);
			UiManager.HoveringRightClickInteractionElements = true;
		}
		catch
		{
			// ignored
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		try
		{
			TooltipsLayer.Hide(_tooltipId);
			_tooltipId = 0;
			UiManager.HoveringRightClickInteractionElements = false;
		}
		catch
		{
			// ignored
		}
	}

	private void OnDisable()
	{
		try
		{
			TooltipsLayer.Hide(_tooltipId);
			_tooltipId = 0;
		}
		catch
		{
			// ignored
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		base.OnPointerClick(eventData);
		try
		{
			if (eventData == null || eventData.button != PointerEventData.InputButton.Right)
			{
				return;
			}

			if (_currentCard == null || !UiManager.IsInitialized)
			{
				return;
			}

			CardDetailPanel panel = UiManager.GetPanel<CardDetailPanel>();
			if (panel == null)
			{
				return;
			}

			string topPanel = Singleton<GamepadNavigationManager>.Instance.GetTopPanel();
			GameObject currentSelected = EventSystem.current?.currentSelectedGameObject;
			panel.Show(new CardDetailPayload(RectTransform, _currentCard, false));
			GamepadNavigationManager.SetOverrideOrigin(currentSelected, topPanel);
		}
		catch
		{
			// ignored
		}
	}

	public void SetLocked(bool locked)
	{
		lockedOverlay?.SetActive(locked);

		button?.interactable = !locked;
	}

	public void SetSelected(bool selected)
	{
		var bg = ResolveBackgroundImage();
		if (bg != null)
		{
			if (_currentCard == null)
			{
				// Empty slot should not show any button body.
				var empty = bg.color;
				empty.a = 0f;
				bg.color = empty;
				return;
			}

			// Preserve vanilla button visuals (sprite/material) by not tinting it into a flat rectangle.
			// Only adjust alpha, and only fall back to tint when there's no sprite (runtime placeholder).
			if (bg.sprite != null)
			{
				var c = bg.color;
				c.a = selected ? 0.95f : 0.75f;
				bg.color = c;
			}
			else
			{
				bg.color = selected ? selectedColor : normalColor;
			}
		}

		selectedBorder?.gameObject.SetActive(selected);
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
