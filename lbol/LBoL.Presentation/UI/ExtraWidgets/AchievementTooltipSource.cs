using System;
using LBoL.Core;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
namespace LBoL.Presentation.UI.ExtraWidgets
{
	[RequireComponent(typeof(AchievementWidget))]
	public class AchievementTooltipSource : TooltipSource
	{
		public string Id
		{
			get
			{
				return this._widget.Id;
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
				return AchievementTooltipSource.DefaultTooltipPositions;
			}
		}
		public override string Title
		{
			get
			{
				if (!this._widget.Hidden)
				{
					return this._widget.DisplayWord.Name;
				}
				return "Museum.Hidden".Localize(true);
			}
		}
		public override string Description
		{
			get
			{
				if (!this._widget.Hidden)
				{
					return this._widget.DisplayWord.Description;
				}
				return "Museum.HiddenDescription".Localize(true);
			}
		}
		private void Awake()
		{
			this._widget = base.GetComponent<AchievementWidget>();
		}
		protected override void Show()
		{
			this._id = TooltipsLayer.ShowAchievement(this);
		}
		private AchievementWidget _widget;
		private static readonly TooltipPosition[] DefaultTooltipPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center)
		};
	}
}
