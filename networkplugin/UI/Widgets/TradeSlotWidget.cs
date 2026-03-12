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
		// 运行时创建的槽位不会自带 prefab 绑定引用。
		cardNameText = runtimeCardNameText;
		cardImage = runtimeCardImage;
	}

	private Image ResolveBackgroundImage()
	{
		if (_bgImage != null)
		{
			return _bgImage;
		}

		if (button != null && button.targetGraphic is Image img)
		{
			_bgImage = img;
			return _bgImage;
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

		if (button == null)
		{
			return;
		}

		_originalTransition = button.transition;
		_originalColors = button.colors;
		_capturedButtonStyle = true;
	}

	private void ApplyEmptyVisual()
	{
		var bg = ResolveBackgroundImage();
		if (bg != null)
		{
			// 空槽位时彻底关闭渲染和射线命中区域。
			bg.enabled = false;
			bg.raycastTarget = false;
			var c = bg.color;
			c.a = 0f;
			bg.color = c;
		}

		if (button != null)
		{
			CaptureButtonStyleOnce();
			button.transition = Selectable.Transition.None;
		}
	}

	private void ApplyFilledVisual()
	{
		var bg = ResolveBackgroundImage();
		if (bg != null)
		{
			bg.enabled = true;
			bg.raycastTarget = true;
			// 透明度和染色交给 SetSelected() 处理。
		}

		if (button != null)
		{
			CaptureButtonStyleOnce();
			button.transition = _originalTransition;
			button.colors = _originalColors;
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

			// 优先使用游戏原生卡牌纹理。
			TrySetCardImage(card);

			// 使用纹理时隐藏占位图标。
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
		if (cardImage == null)
		{
			return;
		}

		try
		{
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

			// 对齐原版 RecordCardCell：优先尝试 imageId + preferredIllustrator。
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
			cardImage.texture = null;
			cardImage.gameObject.SetActive(false);
		}
	}

	public void ClearSlot()
	{
		_currentCard = null;
		_onRemoveCard = null;

		cardNameText?.text = string.Empty;
		cardIcon?.gameObject.SetActive(false);
		if (cardImage != null)
		{
			cardImage.texture = null;
			cardImage.gameObject.SetActive(false);
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
		if (_currentCard == null || !UiManager.IsInitialized)
		{
			return;
		}

		_tooltipId = TooltipsLayer.ShowCard(this, true);
		UiManager.HoveringRightClickInteractionElements = true;
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		TooltipsLayer.Hide(_tooltipId);
		_tooltipId = 0;
		UiManager.HoveringRightClickInteractionElements = false;
	}

	private void OnDisable()
	{
		TooltipsLayer.Hide(_tooltipId);
		_tooltipId = 0;
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		base.OnPointerClick(eventData);
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
				// 空槽位不应显示按钮主体。
				var empty = bg.color;
				empty.a = 0f;
				bg.color = empty;
				return;
			}

			// 保留原版按钮的 sprite/material 外观，不把它染成纯色矩形。
			// 仅调整透明度；只有没有 sprite 的运行时占位按钮才回退到纯色染色。
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
		if (_currentCard == null || _onRemoveCard == null)
		{
			return;
		}

		_onRemoveCard(_currentCard);
		ClearSlot();
	}
}
