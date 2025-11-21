using System;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000190 RID: 400
	public sealed class LoseMoneyAction : SimpleAction
	{
		// Token: 0x17000518 RID: 1304
		// (get) Token: 0x06000ECA RID: 3786 RVA: 0x000280E2 File Offset: 0x000262E2
		public int Money { get; }

		// Token: 0x06000ECB RID: 3787 RVA: 0x000280EA File Offset: 0x000262EA
		public LoseMoneyAction(int money)
		{
			this.Money = money;
		}

		// Token: 0x06000ECC RID: 3788 RVA: 0x000280F9 File Offset: 0x000262F9
		protected override void ResolvePhase()
		{
			base.Battle.GameRun.LoseMoney(this.Money);
		}
	}
}
