using System;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000167 RID: 359
	public sealed class ConsumeMoneyAction : SimpleAction
	{
		// Token: 0x170004D8 RID: 1240
		// (get) Token: 0x06000DEB RID: 3563 RVA: 0x000264E9 File Offset: 0x000246E9
		public int Money { get; }

		// Token: 0x06000DEC RID: 3564 RVA: 0x000264F1 File Offset: 0x000246F1
		public ConsumeMoneyAction(int money)
		{
			this.Money = money;
		}

		// Token: 0x06000DED RID: 3565 RVA: 0x00026500 File Offset: 0x00024700
		protected override void ResolvePhase()
		{
			base.Battle.GameRun.ConsumeMoney(this.Money);
		}

		// Token: 0x06000DEE RID: 3566 RVA: 0x00026518 File Offset: 0x00024718
		public override string ExportDebugDetails()
		{
			return string.Format("Money = {0}", this.Money);
		}
	}
}
