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

	public void SetCard(Card card, Action<Card> removeCallback = null)
	{
		_currentCard = card;
		_onRemoveCard = removeCallback;

		if (button != null)
			button.onClick.RemoveListener(OnRemoveClicked);

		if (card != null)
		{
			if (cardNameText != null) cardNameText.text = card.Name;

			TrySetCardImage(card);

			cardIcon?.gameObject.SetActive(false);

			var bg = ResolveBackgroundImage();
			if (bg != null)
			{
				bg.enabled = true;
				bg.raycastTarget = true;

			}

			if (button != null)
			{
				CaptureButtonStyleOnce();
				button.transition = _originalTransition;
				button.colors = _originalColors;
			}

			if (button != null) button.interactable = true;

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

		if (cardNameText != null) cardNameText.text = string.Empty;
		cardIcon?.gameObject.SetActive(false);
		if (cardImage != null)
		{
			cardImage.texture = null;
			cardImage.gameObject.SetActive(false);
		}

		if (button != null)
		{
			button.interactable = false;

			button.onClick.RemoveAllListeners();
		}

		SetSelected(false);

		var bg = ResolveBackgroundImage();
		if (bg != null)
		{

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

		if (button != null) button.interactable = !locked;
	}

        public void SetSelected(bool selected)
	{
		var bg = ResolveBackgroundImage();
		if (bg != null)
		{
			if (_currentCard == null)
			{

				var empty = bg.color;
				empty.a = 0f;
				bg.color = empty;
				return;
			}

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
