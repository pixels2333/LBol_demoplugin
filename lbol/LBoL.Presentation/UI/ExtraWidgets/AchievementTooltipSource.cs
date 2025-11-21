using System;
using LBoL.Core;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;

namespace LBoL.Presentation.UI.ExtraWidgets
{
	// Token: 0x020000C5 RID: 197
	[RequireComponent(typeof(AchievementWidget))]
	public class AchievementTooltipSource : TooltipSource
	{
		// Token: 0x170001CF RID: 463
		// (get) Token: 0x06000C07 RID: 3079 RVA: 0x0003E333 File Offset: 0x0003C533
		public string Id
		{
			get
			{
				return this._widget.Id;
			}
		}

		// Token: 0x170001D0 RID: 464
		// (get) Token: 0x06000C08 RID: 3080 RVA: 0x0003E340 File Offset: 0x0003C540
		public override RectTransform TargetRectTransform
		{
			get
			{
				return (RectTransform)this._widget.transform;
			}
		}

		// Token: 0x170001D1 RID: 465
		// (get) Token: 0x06000C09 RID: 3081 RVA: 0x0003E352 File Offset: 0x0003C552
		public override TooltipPosition[] TooltipPositions
		{
			get
			{
				return AchievementTooltipSource.DefaultTooltipPositions;
			}
		}

		// Token: 0x170001D2 RID: 466
		// (get) Token: 0x06000C0A RID: 3082 RVA: 0x0003E359 File Offset: 0x0003C559
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

		// Token: 0x170001D3 RID: 467
		// (get) Token: 0x06000C0B RID: 3083 RVA: 0x0003E384 File Offset: 0x0003C584
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

		// Token: 0x06000C0C RID: 3084 RVA: 0x0003E3AF File Offset: 0x0003C5AF
		private void Awake()
		{
			this._widget = base.GetComponent<AchievementWidget>();
		}

		// Token: 0x06000C0D RID: 3085 RVA: 0x0003E3BD File Offset: 0x0003C5BD
		protected override void Show()
		{
			this._id = TooltipsLayer.ShowAchievement(this);
		}

		// Token: 0x04000959 RID: 2393
		private AchievementWidget _widget;

		// Token: 0x0400095A RID: 2394
		private static readonly TooltipPosition[] DefaultTooltipPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center)
		};
	}
}
