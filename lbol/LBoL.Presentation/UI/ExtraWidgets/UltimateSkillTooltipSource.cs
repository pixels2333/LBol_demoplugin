using System;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
namespace LBoL.Presentation.UI.ExtraWidgets
{
	[RequireComponent(typeof(RectTransform))]
	public class UltimateSkillTooltipSource : TooltipSource
	{
		public UltimateSkill Skill
		{
			get
			{
				return this.skill;
			}
			set
			{
				this.skill = value;
			}
		}
		public override RectTransform TargetRectTransform
		{
			get
			{
				return (RectTransform)base.transform;
			}
		}
		public override TooltipPosition[] TooltipPositions
		{
			get
			{
				return UltimateSkillTooltipSource.DefaultTooltipPositions;
			}
		}
		protected override void Show()
		{
			this._id = TooltipsLayer.ShowUltimateSkill(this);
		}
		protected override void Hide()
		{
			if (this._id != 0)
			{
				TooltipsLayer.Hide(this._id);
				this._id = 0;
			}
		}
		public override string Title
		{
			get
			{
				return "Tooltip.UsTitle".LocalizeFormat(new object[]
				{
					this.skill.Title,
					this.skill.Content
				});
			}
		}
		public override string Description
		{
			get
			{
				return this.skill.Description;
			}
		}
		private void OnDisable()
		{
			this.Hide();
		}
		private static readonly TooltipPosition[] DefaultTooltipPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max)
		};
		private UltimateSkill skill;
	}
}
