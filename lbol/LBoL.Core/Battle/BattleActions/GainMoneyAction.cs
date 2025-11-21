using System;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000185 RID: 389
	public sealed class GainMoneyAction : SimpleAction
	{
		// Token: 0x1700050F RID: 1295
		// (get) Token: 0x06000E9D RID: 3741 RVA: 0x00027BE5 File Offset: 0x00025DE5
		public int Money { get; }

		// Token: 0x17000510 RID: 1296
		// (get) Token: 0x06000E9E RID: 3742 RVA: 0x00027BED File Offset: 0x00025DED
		public SpecialSourceType SpecialSource { get; }

		// Token: 0x06000E9F RID: 3743 RVA: 0x00027BF5 File Offset: 0x00025DF5
		public GainMoneyAction(int money, SpecialSourceType type = SpecialSourceType.None)
		{
			this.Money = money;
			this.SpecialSource = type;
		}

		// Token: 0x06000EA0 RID: 3744 RVA: 0x00027C0B File Offset: 0x00025E0B
		protected override void ResolvePhase()
		{
			base.Battle.GameRun.GainMoney(this.Money, false, null);
		}
	}
}
