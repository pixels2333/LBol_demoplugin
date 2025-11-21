using System;
using LBoL.Core.Units;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;

namespace LBoL.Presentation.UI.ExtraWidgets
{
	// Token: 0x020000CA RID: 202
	[RequireComponent(typeof(DollInfoWidget))]
	public class DollTooltipSource : TooltipSource
	{
		// Token: 0x170001D4 RID: 468
		// (get) Token: 0x06000C24 RID: 3108 RVA: 0x0003EA23 File Offset: 0x0003CC23
		public Doll Doll
		{
			get
			{
				return this._widget.Doll;
			}
		}

		// Token: 0x170001D5 RID: 469
		// (get) Token: 0x06000C25 RID: 3109 RVA: 0x0003EA30 File Offset: 0x0003CC30
		public override RectTransform TargetRectTransform
		{
			get
			{
				return (RectTransform)this._widget.transform;
			}
		}

		// Token: 0x170001D6 RID: 470
		// (get) Token: 0x06000C26 RID: 3110 RVA: 0x0003EA42 File Offset: 0x0003CC42
		public override TooltipPosition[] TooltipPositions
		{
			get
			{
				return DollTooltipSource.DefaultTooltipPositions;
			}
		}

		// Token: 0x170001D7 RID: 471
		// (get) Token: 0x06000C27 RID: 3111 RVA: 0x0003EA49 File Offset: 0x0003CC49
		public override string Title
		{
			get
			{
				return this._widget.Doll.Name;
			}
		}

		// Token: 0x170001D8 RID: 472
		// (get) Token: 0x06000C28 RID: 3112 RVA: 0x0003EA5B File Offset: 0x0003CC5B
		public override string Description
		{
			get
			{
				return this._widget.Doll.Description;
			}
		}

		// Token: 0x06000C29 RID: 3113 RVA: 0x0003EA6D File Offset: 0x0003CC6D
		private void Awake()
		{
			this._widget = base.GetComponent<DollInfoWidget>();
		}

		// Token: 0x06000C2A RID: 3114 RVA: 0x0003EA7B File Offset: 0x0003CC7B
		protected override void Show()
		{
			this._id = TooltipsLayer.ShowDoll(this);
		}

		// Token: 0x0400096C RID: 2412
		private DollInfoWidget _widget;

		// Token: 0x0400096D RID: 2413
		private static readonly TooltipPosition[] DefaultTooltipPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max)
		};
	}
}
