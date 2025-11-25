using System;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.SaveData;
using LBoL.Presentation.InputSystemExtend;
using LBoL.Presentation.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	[DisallowMultipleComponent]
	public sealed class RecordCardCell : MonoBehaviour, ICardTooltipSource, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
	{
		public RectTransform RectTransform
		{
			get
			{
				return this.rectTransform;
			}
		}
		public TooltipPosition[] TooltipPositions
		{
			get
			{
				return RecordCardCell.DefaultTooltipPositions;
			}
		}
		public Card Card
		{
			get
			{
				return this._card;
			}
			set
			{
				this._card = value;
				this.nameText.color = (this._card.IsUpgraded ? GlobalConfig.UpgradedGreen : Color.white);
				this.numText.color = (this._card.IsUpgraded ? GlobalConfig.UpgradedGreen : Color.white);
				this.nameText.text = (this._card.IsUpgraded ? ((this._card.UpgradeCounter > 0) ? (this._card.Name + "+" + this._card.UpgradeCounter.Value.ToString()) : (this._card.Name + "+")) : this._card.Name);
				this.SetImage();
			}
		}
		private void SetImage()
		{
			string preferredCardIllustrator = GameMaster.GetPreferredCardIllustrator(this._card);
			string text;
			if (!this._card.Config.UpgradeImageId.IsNullOrEmpty() && this._card.IsUpgraded)
			{
				text = this._card.Config.UpgradeImageId;
			}
			else
			{
				text = (this._card.Config.ImageId.IsNullOrEmpty() ? this._card.Id : this._card.Config.ImageId);
			}
			Texture texture = ResourcesHelper.TryGetCardImage(text + preferredCardIllustrator);
			this.image.texture = texture;
		}
		public void SetNum(int value)
		{
			this.numText.text = "×" + value.ToString();
			this.numText.gameObject.SetActive(value > 1);
			this.numMask.gameObject.SetActive(value > 1);
		}
		public void SetBorderColor(Color color)
		{
			this.borderImage.color = color;
		}
		private void OnEnable()
		{
			GameMaster.SettingsChanged += new Action<GameSettingsSaveData>(this.OnSettingsChanged);
		}
		private void OnDisable()
		{
			GameMaster.SettingsChanged -= new Action<GameSettingsSaveData>(this.OnSettingsChanged);
			TooltipsLayer.Hide(this._tooltipId);
		}
		public void OnPointerEnter(PointerEventData eventData)
		{
			this._tooltipId = TooltipsLayer.ShowCard(this, true);
			UiManager.HoveringRightClickInteractionElements = true;
		}
		public void OnPointerExit(PointerEventData eventData)
		{
			TooltipsLayer.Hide(this._tooltipId);
			UiManager.HoveringRightClickInteractionElements = false;
		}
		public void OnPointerClick(PointerEventData eventData)
		{
			AudioManager.Card(3);
			CardDetailPanel panel = UiManager.GetPanel<CardDetailPanel>();
			if (panel.IsVisible)
			{
				panel.ShowRelativeCard(this);
				return;
			}
			string topPanel = Singleton<GamepadNavigationManager>.Instance.GetTopPanel();
			GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			panel.Show(new CardDetailPayload(base.GetComponent<RectTransform>(), this._card, false));
			GamepadNavigationManager.SetOverrideOrigin(currentSelectedGameObject, topPanel);
		}
		private void OnSettingsChanged(GameSettingsSaveData settings)
		{
			this.SetImage();
		}
		public void OnGamepadSelectedChanged(bool value)
		{
			if (value)
			{
				this._tooltipId = TooltipsLayer.ShowCard(this, true);
				return;
			}
			TooltipsLayer.Hide(this._tooltipId);
		}
		public void OnGamepadClick()
		{
			this.OnPointerClick(null);
		}
		private static readonly TooltipPosition[] DefaultTooltipPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Min),
			new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center)
		};
		[SerializeField]
		private RectTransform rectTransform;
		[SerializeField]
		private RawImage image;
		[SerializeField]
		private TextMeshProUGUI nameText;
		[SerializeField]
		private TextMeshProUGUI numText;
		[SerializeField]
		private Transform numMask;
		[SerializeField]
		private Image borderImage;
		private Card _card;
		private int _tooltipId;
	}
}
