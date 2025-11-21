using System;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;

namespace LBoL.Presentation.UI.ExtraWidgets
{
	// Token: 0x020000D7 RID: 215
	[RequireComponent(typeof(RectTransform))]
	public class UltimateSkillTooltipSource : TooltipSource
	{
		// Token: 0x17000226 RID: 550
		// (get) Token: 0x06000D12 RID: 3346 RVA: 0x000408C9 File Offset: 0x0003EAC9
		// (set) Token: 0x06000D13 RID: 3347 RVA: 0x000408D1 File Offset: 0x0003EAD1
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

		// Token: 0x17000227 RID: 551
		// (get) Token: 0x06000D14 RID: 3348 RVA: 0x000408DA File Offset: 0x0003EADA
		public override RectTransform TargetRectTransform
		{
			get
			{
				return (RectTransform)base.transform;
			}
		}

		// Token: 0x17000228 RID: 552
		// (get) Token: 0x06000D15 RID: 3349 RVA: 0x000408E7 File Offset: 0x0003EAE7
		public override TooltipPosition[] TooltipPositions
		{
			get
			{
				return UltimateSkillTooltipSource.DefaultTooltipPositions;
			}
		}

		// Token: 0x06000D16 RID: 3350 RVA: 0x000408EE File Offset: 0x0003EAEE
		protected override void Show()
		{
			this._id = TooltipsLayer.ShowUltimateSkill(this);
		}

		// Token: 0x06000D17 RID: 3351 RVA: 0x000408FC File Offset: 0x0003EAFC
		protected override void Hide()
		{
			if (this._id != 0)
			{
				TooltipsLayer.Hide(this._id);
				this._id = 0;
			}
		}

		// Token: 0x17000229 RID: 553
		// (get) Token: 0x06000D18 RID: 3352 RVA: 0x00040918 File Offset: 0x0003EB18
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

		// Token: 0x1700022A RID: 554
		// (get) Token: 0x06000D19 RID: 3353 RVA: 0x00040946 File Offset: 0x0003EB46
		public override string Description
		{
			get
			{
				return this.skill.Description;
			}
		}

		// Token: 0x06000D1A RID: 3354 RVA: 0x00040953 File Offset: 0x0003EB53
		private void OnDisable()
		{
			this.Hide();
		}

		// Token: 0x040009E6 RID: 2534
		private static readonly TooltipPosition[] DefaultTooltipPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max)
		};

		// Token: 0x040009E7 RID: 2535
		private UltimateSkill skill;
	}
}
