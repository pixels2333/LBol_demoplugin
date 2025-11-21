using System;
using LBoL.Core.Units;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;

namespace LBoL.Presentation.UI.ExtraWidgets
{
	// Token: 0x020000CD RID: 205
	[RequireComponent(typeof(IntentionWidget))]
	public class IntentionTooltipSource : TooltipSource
	{
		// Token: 0x170001FB RID: 507
		// (get) Token: 0x06000C7F RID: 3199 RVA: 0x0003F436 File Offset: 0x0003D636
		private Intention Intention
		{
			get
			{
				return this.widget.Intention;
			}
		}

		// Token: 0x170001FC RID: 508
		// (get) Token: 0x06000C80 RID: 3200 RVA: 0x0003F443 File Offset: 0x0003D643
		public override RectTransform TargetRectTransform
		{
			get
			{
				return (RectTransform)this.widget.transform;
			}
		}

		// Token: 0x170001FD RID: 509
		// (get) Token: 0x06000C81 RID: 3201 RVA: 0x0003F455 File Offset: 0x0003D655
		public override TooltipPosition[] TooltipPositions
		{
			get
			{
				return IntentionTooltipSource.DefaultPositions;
			}
		}

		// Token: 0x170001FE RID: 510
		// (get) Token: 0x06000C82 RID: 3202 RVA: 0x0003F45C File Offset: 0x0003D65C
		public override string Title
		{
			get
			{
				return this.Intention.Name;
			}
		}

		// Token: 0x170001FF RID: 511
		// (get) Token: 0x06000C83 RID: 3203 RVA: 0x0003F469 File Offset: 0x0003D669
		public override string Description
		{
			get
			{
				return this.Intention.Description;
			}
		}

		// Token: 0x04000999 RID: 2457
		private static readonly TooltipPosition[] DefaultPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max)
		};

		// Token: 0x0400099A RID: 2458
		public IntentionWidget widget;
	}
}
