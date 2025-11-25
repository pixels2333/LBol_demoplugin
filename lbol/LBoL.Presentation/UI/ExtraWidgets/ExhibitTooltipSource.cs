using System;
using LBoL.Core;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
namespace LBoL.Presentation.UI.ExtraWidgets
{
	[RequireComponent(typeof(ExhibitWidget))]
	public class ExhibitTooltipSource : TooltipSource
	{
		public Exhibit Exhibit
		{
			get
			{
				return this._widget.Exhibit;
			}
		}
		public override string Title
		{
			get
			{
				return this._widget.Exhibit.Name;
			}
		}
		public override string Description
		{
			get
			{
				return this._widget.Exhibit.Description;
			}
		}
		public override RectTransform TargetRectTransform
		{
			get
			{
				return this._widget.GetComponent<RectTransform>();
			}
		}
		public override TooltipPosition[] TooltipPositions
		{
			get
			{
				return ExhibitTooltipSource.DefaultExhibitTooltipPositions;
			}
		}
		private void Awake()
		{
			this._widget = base.GetComponent<ExhibitWidget>();
		}
		protected override void Show()
		{
			this._id = TooltipsLayer.ShowExhibit(this);
		}
		private static readonly TooltipPosition[] DefaultExhibitTooltipPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max)
		};
		private ExhibitWidget _widget;
	}
}
