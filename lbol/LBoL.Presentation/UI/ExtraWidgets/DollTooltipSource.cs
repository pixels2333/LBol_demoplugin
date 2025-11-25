using System;
using LBoL.Core.Units;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
namespace LBoL.Presentation.UI.ExtraWidgets
{
	[RequireComponent(typeof(DollInfoWidget))]
	public class DollTooltipSource : TooltipSource
	{
		public Doll Doll
		{
			get
			{
				return this._widget.Doll;
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
				return DollTooltipSource.DefaultTooltipPositions;
			}
		}
		public override string Title
		{
			get
			{
				return this._widget.Doll.Name;
			}
		}
		public override string Description
		{
			get
			{
				return this._widget.Doll.Description;
			}
		}
		private void Awake()
		{
			this._widget = base.GetComponent<DollInfoWidget>();
		}
		protected override void Show()
		{
			this._id = TooltipsLayer.ShowDoll(this);
		}
		private DollInfoWidget _widget;
		private static readonly TooltipPosition[] DefaultTooltipPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max)
		};
	}
}
