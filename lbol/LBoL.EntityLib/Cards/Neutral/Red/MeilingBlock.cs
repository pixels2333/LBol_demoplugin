using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002CC RID: 716
	[UsedImplicitly]
	public sealed class MeilingBlock : Card
	{
		// Token: 0x1700013A RID: 314
		// (get) Token: 0x06000AE6 RID: 2790 RVA: 0x00016433 File Offset: 0x00014633
		protected override int AdditionalBlock
		{
			get
			{
				if (base.Battle != null)
				{
					return this.PlayerFirepowerPositive;
				}
				return 0;
			}
		}

		// Token: 0x1700013B RID: 315
		// (get) Token: 0x06000AE7 RID: 2791 RVA: 0x00016445 File Offset: 0x00014645
		protected override int AdditionalShield
		{
			get
			{
				if (base.Battle != null)
				{
					return this.PlayerFirepowerPositive;
				}
				return 0;
			}
		}

		// Token: 0x1700013C RID: 316
		// (get) Token: 0x06000AE8 RID: 2792 RVA: 0x00016457 File Offset: 0x00014657
		private int PlayerFirepowerPositive
		{
			get
			{
				return Math.Max(0, base.Battle.Player.TotalFirepower);
			}
		}
	}
}
