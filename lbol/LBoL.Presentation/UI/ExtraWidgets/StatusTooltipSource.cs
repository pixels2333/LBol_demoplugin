using System;
using LBoL.Core.StatusEffects;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;

namespace LBoL.Presentation.UI.ExtraWidgets
{
	// Token: 0x020000D6 RID: 214
	[RequireComponent(typeof(StatusEffectWidget))]
	public class StatusTooltipSource : TooltipSource
	{
		// Token: 0x17000221 RID: 545
		// (get) Token: 0x06000D09 RID: 3337 RVA: 0x00040826 File Offset: 0x0003EA26
		public StatusEffect StatusEffect
		{
			get
			{
				return this._widget.StatusEffect;
			}
		}

		// Token: 0x17000222 RID: 546
		// (get) Token: 0x06000D0A RID: 3338 RVA: 0x00040833 File Offset: 0x0003EA33
		public override RectTransform TargetRectTransform
		{
			get
			{
				return (RectTransform)this._widget.transform;
			}
		}

		// Token: 0x17000223 RID: 547
		// (get) Token: 0x06000D0B RID: 3339 RVA: 0x00040845 File Offset: 0x0003EA45
		public override TooltipPosition[] TooltipPositions
		{
			get
			{
				return StatusTooltipSource.DefaultTooltipPositions;
			}
		}

		// Token: 0x17000224 RID: 548
		// (get) Token: 0x06000D0C RID: 3340 RVA: 0x0004084C File Offset: 0x0003EA4C
		public override string Title
		{
			get
			{
				return this._widget.StatusEffect.Name;
			}
		}

		// Token: 0x17000225 RID: 549
		// (get) Token: 0x06000D0D RID: 3341 RVA: 0x0004085E File Offset: 0x0003EA5E
		public override string Description
		{
			get
			{
				return this._widget.StatusEffect.Description;
			}
		}

		// Token: 0x06000D0E RID: 3342 RVA: 0x00040870 File Offset: 0x0003EA70
		private void Awake()
		{
			this._widget = base.GetComponent<StatusEffectWidget>();
		}

		// Token: 0x06000D0F RID: 3343 RVA: 0x0004087E File Offset: 0x0003EA7E
		protected override void Show()
		{
			this._id = TooltipsLayer.ShowStatus(this);
		}

		// Token: 0x040009E4 RID: 2532
		private StatusEffectWidget _widget;

		// Token: 0x040009E5 RID: 2533
		private static readonly TooltipPosition[] DefaultTooltipPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max)
		};
	}
}
