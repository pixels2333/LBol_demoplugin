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
	public class SelectBaseManaPanel : UiPanel<SelectBaseManaPayload>
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}
		public int Min { get; private set; }
		public int Max { get; private set; }
		public ManaGroup SelectedMana { get; private set; }
		public bool IsCanceled { get; private set; }
		public void Awake()
		{
			this.confirmButton.onClick.AddListener(new UnityAction(this.Confirm));
			this.cancelButton.onClick.AddListener(new UnityAction(this.Cancel));
			this.manaTemplate.gameObject.SetActive(false);
		}
		protected override void OnEnterGameRun()
		{
			base.GameRun.InteractionViewer.Register<SelectBaseManaInteraction>(new InteractionViewer<SelectBaseManaInteraction>(this.ViewSelectBaseMana));
		}
		protected override void OnLeaveGameRun()
		{
			base.GameRun.InteractionViewer.Unregister<SelectBaseManaInteraction>(new InteractionViewer<SelectBaseManaInteraction>(this.ViewSelectBaseMana));
		}
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
		protected override void OnHided()
		{
			foreach (SelectBaseManaWidget selectBaseManaWidget in this._baseManaWidgets)
			{
				Object.Destroy(selectBaseManaWidget.gameObject);
			}
			this._baseManaWidgets.Clear();
		}
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
		private void Cancel()
		{
			this.IsCanceled = true;
			base.Hide();
		}
		public IEnumerator ShowAsync(SelectBaseManaPayload payload)
		{
			base.Show(payload);
			yield return new WaitWhile(() => base.IsVisible);
			yield break;
		}
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
		[SerializeField]
		private Transform manaParent;
		[SerializeField]
		private SelectBaseManaWidget manaTemplate;
		[SerializeField]
		private Button confirmButton;
		[SerializeField]
		private Button cancelButton;
		[FormerlySerializedAs("displaytext")]
		[SerializeField]
		private TextMeshProUGUI displayText;
		[SerializeField]
		private AssociationList<ManaColor, Sprite> colorSpriteTable;
		[SerializeField]
		private AssociationList<ManaColor, ParticleSystem.MinMaxGradient> colorParticleTable;
		[SerializeField]
		private MinimizedButtonWidget minimizedButton;
		private readonly List<SelectBaseManaWidget> _baseManaWidgets = new List<SelectBaseManaWidget>();
		private readonly List<int> _selectIndexOrder = new List<int>();
	}
}
