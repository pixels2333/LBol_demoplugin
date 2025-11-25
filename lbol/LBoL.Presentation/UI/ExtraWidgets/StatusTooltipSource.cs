using System;
using LBoL.Core.StatusEffects;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
namespace LBoL.Presentation.UI.ExtraWidgets
{
	[RequireComponent(typeof(StatusEffectWidget))]
	public class StatusTooltipSource : TooltipSource
	{
		public StatusEffect StatusEffect
		{
			get
			{
				return this._widget.StatusEffect;
			}
		}
		public override RectTransform TargetRectTransform
		{
			get
			{
				return (RectTransform)this._widget.transform;
			}
		}
		public override TooltipPosition[] TooltipPositions
		{
			get
			{
				return StatusTooltipSource.DefaultTooltipPositions;
			}
		}
		public override string Title
		{
			get
			{
				return this._widget.StatusEffect.Name;
			}
		}
		public override string Description
		{
			get
			{
				return this._widget.StatusEffect.Description;
			}
		}
		private void Awake()
		{
			this._widget = base.GetComponent<StatusEffectWidget>();
		}
		protected override void Show()
		{
			this._id = TooltipsLayer.ShowStatus(this);
		}
		private StatusEffectWidget _widget;
		private static readonly TooltipPosition[] DefaultTooltipPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max)
		};
	}
}
