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
	// Token: 0x0200006B RID: 107
	[DisallowMultipleComponent]
	public sealed class RecordCardCell : MonoBehaviour, ICardTooltipSource, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
	{
		// Token: 0x17000100 RID: 256
		// (get) Token: 0x060005B7 RID: 1463 RVA: 0x00018AA3 File Offset: 0x00016CA3
		public RectTransform RectTransform
		{
			get
			{
				return this.rectTransform;
			}
		}

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x060005B8 RID: 1464 RVA: 0x00018AAB File Offset: 0x00016CAB
		public TooltipPosition[] TooltipPositions
		{
			get
			{
				return RecordCardCell.DefaultTooltipPositions;
			}
		}

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x060005B9 RID: 1465 RVA: 0x00018AB2 File Offset: 0x00016CB2
		// (set) Token: 0x060005BA RID: 1466 RVA: 0x00018ABC File Offset: 0x00016CBC
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

		// Token: 0x060005BB RID: 1467 RVA: 0x00018BAC File Offset: 0x00016DAC
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

		// Token: 0x060005BC RID: 1468 RVA: 0x00018C4C File Offset: 0x00016E4C
		public void SetNum(int value)
		{
			this.numText.text = "×" + value.ToString();
			this.numText.gameObject.SetActive(value > 1);
			this.numMask.gameObject.SetActive(value > 1);
		}

		// Token: 0x060005BD RID: 1469 RVA: 0x00018C9D File Offset: 0x00016E9D
		public void SetBorderColor(Color color)
		{
			this.borderImage.color = color;
		}

		// Token: 0x060005BE RID: 1470 RVA: 0x00018CAB File Offset: 0x00016EAB
		private void OnEnable()
		{
			GameMaster.SettingsChanged += new Action<GameSettingsSaveData>(this.OnSettingsChanged);
		}

		// Token: 0x060005BF RID: 1471 RVA: 0x00018CBE File Offset: 0x00016EBE
		private void OnDisable()
		{
			GameMaster.SettingsChanged -= new Action<GameSettingsSaveData>(this.OnSettingsChanged);
			TooltipsLayer.Hide(this._tooltipId);
		}

		// Token: 0x060005C0 RID: 1472 RVA: 0x00018CDC File Offset: 0x00016EDC
		public void OnPointerEnter(PointerEventData eventData)
		{
			this._tooltipId = TooltipsLayer.ShowCard(this, true);
			UiManager.HoveringRightClickInteractionElements = true;
		}

		// Token: 0x060005C1 RID: 1473 RVA: 0x00018CF1 File Offset: 0x00016EF1
		public void OnPointerExit(PointerEventData eventData)
		{
			TooltipsLayer.Hide(this._tooltipId);
			UiManager.HoveringRightClickInteractionElements = false;
		}

		// Token: 0x060005C2 RID: 1474 RVA: 0x00018D04 File Offset: 0x00016F04
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

		// Token: 0x060005C3 RID: 1475 RVA: 0x00018D60 File Offset: 0x00016F60
		private void OnSettingsChanged(GameSettingsSaveData settings)
		{
			this.SetImage();
		}

		// Token: 0x060005C4 RID: 1476 RVA: 0x00018D68 File Offset: 0x00016F68
		public void OnGamepadSelectedChanged(bool value)
		{
			if (value)
			{
				this._tooltipId = TooltipsLayer.ShowCard(this, true);
				return;
			}
			TooltipsLayer.Hide(this._tooltipId);
		}

		// Token: 0x060005C5 RID: 1477 RVA: 0x00018D86 File Offset: 0x00016F86
		public void OnGamepadClick()
		{
			this.OnPointerClick(null);
		}

		// Token: 0x04000370 RID: 880
		private static readonly TooltipPosition[] DefaultTooltipPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Min),
			new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center)
		};

		// Token: 0x04000371 RID: 881
		[SerializeField]
		private RectTransform rectTransform;

		// Token: 0x04000372 RID: 882
		[SerializeField]
		private RawImage image;

		// Token: 0x04000373 RID: 883
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x04000374 RID: 884
		[SerializeField]
		private TextMeshProUGUI numText;

		// Token: 0x04000375 RID: 885
		[SerializeField]
		private Transform numMask;

		// Token: 0x04000376 RID: 886
		[SerializeField]
		private Image borderImage;

		// Token: 0x04000377 RID: 887
		private Card _card;

		// Token: 0x04000378 RID: 888
		private int _tooltipId;
	}
}
