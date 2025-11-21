using System;
using LBoL.Presentation.Effect;
using Spine.Unity;
using UnityEngine;

namespace LBoL.Presentation.Units.SpecialUnits
{
	// Token: 0x0200001C RID: 28
	public class MuskData
	{
		// Token: 0x060002EB RID: 747 RVA: 0x0000D402 File Offset: 0x0000B602
		public MuskData(SkeletonAnimation skeleton, Transform point, int index)
		{
			this.Index = index;
			this.Skeleton = skeleton;
			this.EffectPoint = point;
			this.Front = index < 2;
		}

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x060002EC RID: 748 RVA: 0x0000D429 File Offset: 0x0000B629
		// (set) Token: 0x060002ED RID: 749 RVA: 0x0000D431 File Offset: 0x0000B631
		public int Index { get; set; }

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x060002EE RID: 750 RVA: 0x0000D43A File Offset: 0x0000B63A
		// (set) Token: 0x060002EF RID: 751 RVA: 0x0000D442 File Offset: 0x0000B642
		public SkeletonAnimation Skeleton { get; set; }

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x060002F0 RID: 752 RVA: 0x0000D44B File Offset: 0x0000B64B
		// (set) Token: 0x060002F1 RID: 753 RVA: 0x0000D453 File Offset: 0x0000B653
		public Transform EffectPoint { get; set; }

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x060002F2 RID: 754 RVA: 0x0000D45C File Offset: 0x0000B65C
		// (set) Token: 0x060002F3 RID: 755 RVA: 0x0000D464 File Offset: 0x0000B664
		public EffectWidget EffectWidget { get; set; }

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x060002F4 RID: 756 RVA: 0x0000D46D File Offset: 0x0000B66D
		// (set) Token: 0x060002F5 RID: 757 RVA: 0x0000D475 File Offset: 0x0000B675
		public bool Front { get; set; }
	}
}
