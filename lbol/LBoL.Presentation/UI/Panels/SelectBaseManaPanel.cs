using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle.Interactions;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000AF RID: 175
	public class SelectBaseManaPanel : UiPanel<SelectBaseManaPayload>
	{
		// Token: 0x17000184 RID: 388
		// (get) Token: 0x060009A4 RID: 2468 RVA: 0x00031346 File Offset: 0x0002F546
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}

		// Token: 0x17000185 RID: 389
		// (get) Token: 0x060009A5 RID: 2469 RVA: 0x00031349 File Offset: 0x0002F549
		// (set) Token: 0x060009A6 RID: 2470 RVA: 0x00031351 File Offset: 0x0002F551
		public int Min { get; private set; }

		// Token: 0x17000186 RID: 390
		// (get) Token: 0x060009A7 RID: 2471 RVA: 0x0003135A File Offset: 0x0002F55A
		// (set) Token: 0x060009A8 RID: 2472 RVA: 0x00031362 File Offset: 0x0002F562
		public int Max { get; private set; }

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x060009A9 RID: 2473 RVA: 0x0003136B File Offset: 0x0002F56B
		// (set) Token: 0x060009AA RID: 2474 RVA: 0x00031373 File Offset: 0x0002F573
		public ManaGroup SelectedMana { get; private set; }

		// Token: 0x17000188 RID: 392
		// (get) Token: 0x060009AB RID: 2475 RVA: 0x0003137C File Offset: 0x0002F57C
		// (set) Token: 0x060009AC RID: 2476 RVA: 0x00031384 File Offset: 0x0002F584
		public bool IsCanceled { get; private set; }

		// Token: 0x060009AD RID: 2477 RVA: 0x00031390 File Offset: 0x0002F590
		public void Awake()
		{
			this.confirmButton.onClick.AddListener(new UnityAction(this.Confirm));
			this.cancelButton.onClick.AddListener(new UnityAction(this.Cancel));
			this.manaTemplate.gameObject.SetActive(false);
		}

		// Token: 0x060009AE RID: 2478 RVA: 0x000313E6 File Offset: 0x0002F5E6
		protected override void OnEnterGameRun()
		{
			base.GameRun.InteractionViewer.Register<SelectBaseManaInteraction>(new InteractionViewer<SelectBaseManaInteraction>(this.ViewSelectBaseMana));
		}

		// Token: 0x060009AF RID: 2479 RVA: 0x00031404 File Offset: 0x0002F604
		protected override void OnLeaveGameRun()
		{
			base.GameRun.InteractionViewer.Unregister<SelectBaseManaInteraction>(new InteractionViewer<SelectBaseManaInteraction>(this.ViewSelectBaseMana));
		}

		// Token: 0x060009B0 RID: 2480 RVA: 0x00031424 File Offset: 0x0002F624
		public bool SwitchMinimized()
		{
			if (!base.IsVisible)
			{
				return false;
			}
			CanvasGroup component = base.GetComponent<CanvasGroup>();
			if (!component || !component.interactable)
			{
				return false;
			}
			this.minimizedButton.SwitchMinimized();
			return true;
		}

		// Token: 0x060009B1 RID: 2481 RVA: 0x00031460 File Offset: 0x0002F660
		protected override void OnShowing(SelectBaseManaPayload payload)
		{
			this.minimizedButton.Init();
			this.cancelButton.gameObject.SetActive(payload.CanCancel);
			this.Min = payload.Min;
			this.Max = payload.Max;
			this.confirmButton.interactable = 0 >= this.Min;
			this.SelectedMana = ManaGroup.Empty;
			this.IsCanceled = false;
			foreach (ManaColor manaColor in payload.BaseMana.EnumerateColors())
			{
				int num = Math.Min(payload.BaseMana.GetValue(manaColor), this.Max);
				for (int i = 0; i < num; i++)
				{
					SelectBaseManaWidget selectBaseManaWidget = Object.Instantiate<SelectBaseManaWidget>(this.manaTemplate, this.manaParent);
					selectBaseManaWidget.gameObject.SetActive(true);
					selectBaseManaWidget.Color = manaColor;
					Sprite sprite;
					ParticleSystem.MinMaxGradient minMaxGradient;
					selectBaseManaWidget.SetSprite(this.colorSpriteTable.TryGetValue(manaColor, out sprite) ? sprite : null, this.colorParticleTable.TryGetValue(manaColor, out minMaxGradient) ? minMaxGradient : null);
					selectBaseManaWidget.SelectedChanged += delegate(object sender, EventArgs args)
					{
						SelectBaseManaWidget widget = sender as SelectBaseManaWidget;
						if (widget != null)
						{
							if (widget.IsSelected && !this._selectIndexOrder.Exists((int t) => t == this._baseManaWidgets.IndexOf(widget)))
							{
								this._selectIndexOrder.Add(this._baseManaWidgets.IndexOf(widget));
							}
							if (!widget.IsSelected)
							{
								this._selectIndexOrder.Remove(this._baseManaWidgets.IndexOf(widget));
							}
						}
						int num2 = Enumerable.Count<SelectBaseManaWidget>(this._baseManaWidgets, (SelectBaseManaWidget w) => w.IsSelected);
						if (num2 > this.Max)
						{
							this._baseManaWidgets[this._selectIndexOrder[0]].SetSelected(false);
						}
						num2 = Enumerable.Count<SelectBaseManaWidget>(this._baseManaWidgets, (SelectBaseManaWidget w) => w.IsSelected);
						this.confirmButton.interactable = num2 >= this.Min && num2 <= this.Max;
					};
					this._baseManaWidgets.Add(selectBaseManaWidget);
				}
			}
		}

		// Token: 0x060009B2 RID: 2482 RVA: 0x000315C8 File Offset: 0x0002F7C8
		protected override void OnHided()
		{
			foreach (SelectBaseManaWidget selectBaseManaWidget in this._baseManaWidgets)
			{
				Object.Destroy(selectBaseManaWidget.gameObject);
			}
			this._baseManaWidgets.Clear();
		}

		// Token: 0x060009B3 RID: 2483 RVA: 0x00031628 File Offset: 0x0002F828
		private void Confirm()
		{
			foreach (SelectBaseManaWidget selectBaseManaWidget in this._baseManaWidgets)
			{
				if (selectBaseManaWidget.IsSelected)
				{
					this.SelectedMana += ManaGroup.Single(selectBaseManaWidget.Color);
				}
			}
			this.IsCanceled = false;
			base.Hide();
		}

		// Token: 0x060009B4 RID: 2484 RVA: 0x000316A8 File Offset: 0x0002F8A8
		private void Cancel()
		{
			this.IsCanceled = true;
			base.Hide();
		}

		// Token: 0x060009B5 RID: 2485 RVA: 0x000316B7 File Offset: 0x0002F8B7
		public IEnumerator ShowAsync(SelectBaseManaPayload payload)
		{
			base.Show(payload);
			yield return new WaitWhile(() => base.IsVisible);
			yield break;
		}

		// Token: 0x060009B6 RID: 2486 RVA: 0x000316CD File Offset: 0x0002F8CD
		private IEnumerator ViewSelectBaseMana(SelectBaseManaInteraction interaction)
		{
			if (interaction.Description != null)
			{
				this.displayText.text = interaction.Description;
			}
			else
			{
				GameEntity source = interaction.Source;
				if (source != null)
				{
					this.displayText.text = source.Name + ": " + source.Description;
				}
			}
			yield return this.ShowAsync(new SelectBaseManaPayload
			{
				BaseMana = interaction.PendingMana,
				Min = interaction.Min,
				Max = interaction.Max,
				CanCancel = interaction.CanCancel
			});
			if (this.IsCanceled)
			{
				interaction.Cancel();
			}
			else
			{
				interaction.SelectedMana = this.SelectedMana;
			}
			yield break;
		}

		// Token: 0x04000724 RID: 1828
		[SerializeField]
		private Transform manaParent;

		// Token: 0x04000725 RID: 1829
		[SerializeField]
		private SelectBaseManaWidget manaTemplate;

		// Token: 0x04000726 RID: 1830
		[SerializeField]
		private Button confirmButton;

		// Token: 0x04000727 RID: 1831
		[SerializeField]
		private Button cancelButton;

		// Token: 0x04000728 RID: 1832
		[FormerlySerializedAs("displaytext")]
		[SerializeField]
		private TextMeshProUGUI displayText;

		// Token: 0x04000729 RID: 1833
		[SerializeField]
		private AssociationList<ManaColor, Sprite> colorSpriteTable;

		// Token: 0x0400072A RID: 1834
		[SerializeField]
		private AssociationList<ManaColor, ParticleSystem.MinMaxGradient> colorParticleTable;

		// Token: 0x0400072B RID: 1835
		[SerializeField]
		private MinimizedButtonWidget minimizedButton;

		// Token: 0x04000730 RID: 1840
		private readonly List<SelectBaseManaWidget> _baseManaWidgets = new List<SelectBaseManaWidget>();

		// Token: 0x04000731 RID: 1841
		private readonly List<int> _selectIndexOrder = new List<int>();
	}
}
