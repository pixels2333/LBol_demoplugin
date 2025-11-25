using System;
using LBoL.Core.Units;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
namespace LBoL.Presentation.UI.ExtraWidgets
{
	[RequireComponent(typeof(IntentionWidget))]
	public class IntentionTooltipSource : TooltipSource
	{
		private Intention Intention
		{
			get
			{
				return this.widget.Intention;
			}
		}
		public override RectTransform TargetRectTransform
		{
			get
			{
				return (RectTransform)this.widget.transform;
			}
		}
		public override TooltipPosition[] TooltipPositions
		{
			get
			{
				return IntentionTooltipSource.DefaultPositions;
			}
		}
		public override string Title
		{
			get
			{
				return this.Intention.Name;
			}
		}
		public override string Description
		{
			get
			{
				return this.Intention.Description;
			}
		}
		private static readonly TooltipPosition[] DefaultPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max)
		};
		public IntentionWidget widget;
	}
}
