using System;
using LBoL.Core;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;

namespace LBoL.Presentation.UI.ExtraWidgets
{
	// Token: 0x020000CB RID: 203
	[RequireComponent(typeof(ExhibitWidget))]
	public class ExhibitTooltipSource : TooltipSource
	{
		// Token: 0x170001D9 RID: 473
		// (get) Token: 0x06000C2D RID: 3117 RVA: 0x0003EAC6 File Offset: 0x0003CCC6
		public Exhibit Exhibit
		{
			get
			{
				return this._widget.Exhibit;
			}
		}

		// Token: 0x170001DA RID: 474
		// (get) Token: 0x06000C2E RID: 3118 RVA: 0x0003EAD3 File Offset: 0x0003CCD3
		public override string Title
		{
			get
			{
				return this._widget.Exhibit.Name;
			}
		}

		// Token: 0x170001DB RID: 475
		// (get) Token: 0x06000C2F RID: 3119 RVA: 0x0003EAE5 File Offset: 0x0003CCE5
		public override string Description
		{
			get
			{
				return this._widget.Exhibit.Description;
			}
		}

		// Token: 0x170001DC RID: 476
		// (get) Token: 0x06000C30 RID: 3120 RVA: 0x0003EAF7 File Offset: 0x0003CCF7
		public override RectTransform TargetRectTransform
		{
			get
			{
				return this._widget.GetComponent<RectTransform>();
			}
		}

		// Token: 0x170001DD RID: 477
		// (get) Token: 0x06000C31 RID: 3121 RVA: 0x0003EB04 File Offset: 0x0003CD04
		public override TooltipPosition[] TooltipPositions
		{
			get
			{
				return ExhibitTooltipSource.DefaultExhibitTooltipPositions;
			}
		}

		// Token: 0x06000C32 RID: 3122 RVA: 0x0003EB0B File Offset: 0x0003CD0B
		private void Awake()
		{
			this._widget = base.GetComponent<ExhibitWidget>();
		}

		// Token: 0x06000C33 RID: 3123 RVA: 0x0003EB19 File Offset: 0x0003CD19
		protected override void Show()
		{
			this._id = TooltipsLayer.ShowExhibit(this);
		}

		// Token: 0x0400096E RID: 2414
		private static readonly TooltipPosition[] DefaultExhibitTooltipPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max)
		};

		// Token: 0x0400096F RID: 2415
		private ExhibitWidget _widget;
	}
}
