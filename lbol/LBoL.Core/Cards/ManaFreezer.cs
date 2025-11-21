using System;
using System.Collections.Generic;
using LBoL.Core.Battle;

namespace LBoL.Core.Cards
{
	// Token: 0x02000133 RID: 307
	public abstract class ManaFreezer : Card
	{
		// Token: 0x170003FE RID: 1022
		// (get) Token: 0x06000BE6 RID: 3046 RVA: 0x000214A1 File Offset: 0x0001F6A1
		public virtual int FreezeLevel
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x170003FF RID: 1023
		// (get) Token: 0x06000BE7 RID: 3047 RVA: 0x000214A4 File Offset: 0x0001F6A4
		public virtual int InitialTimes
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x17000400 RID: 1024
		// (get) Token: 0x06000BE8 RID: 3048 RVA: 0x000214A7 File Offset: 0x0001F6A7
		// (set) Token: 0x06000BE9 RID: 3049 RVA: 0x000214AF File Offset: 0x0001F6AF
		public int FreezeTimes
		{
			get
			{
				return this._freezeTimes;
			}
			set
			{
				this._freezeTimes = value;
				this.NotifyChanged();
			}
		}

		// Token: 0x06000BEA RID: 3050 RVA: 0x000214BE File Offset: 0x0001F6BE
		public override void Initialize()
		{
			base.Initialize();
			this.FreezeTimes = this.InitialTimes;
		}

		// Token: 0x17000401 RID: 1025
		// (get) Token: 0x06000BEB RID: 3051 RVA: 0x000214D2 File Offset: 0x0001F6D2
		public override bool OnDrawVisual
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000402 RID: 1026
		// (get) Token: 0x06000BEC RID: 3052 RVA: 0x000214D5 File Offset: 0x0001F6D5
		public override bool OnDiscardVisual
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000403 RID: 1027
		// (get) Token: 0x06000BED RID: 3053 RVA: 0x000214D8 File Offset: 0x0001F6D8
		public override bool OnExileVisual
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000404 RID: 1028
		// (get) Token: 0x06000BEE RID: 3054 RVA: 0x000214DB File Offset: 0x0001F6DB
		public override bool OnMoveVisual
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000BEF RID: 3055 RVA: 0x000214DE File Offset: 0x0001F6DE
		public override IEnumerable<BattleAction> OnDraw()
		{
			this.EnterHand();
			return null;
		}

		// Token: 0x06000BF0 RID: 3056 RVA: 0x000214E7 File Offset: 0x0001F6E7
		public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			if (srcZone == CardZone.Hand)
			{
				this.LeaveHand();
			}
			if (dstZone == CardZone.Hand)
			{
				this.EnterHand();
			}
			return null;
		}

		// Token: 0x06000BF1 RID: 3057 RVA: 0x000214FE File Offset: 0x0001F6FE
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.Zone == CardZone.Hand)
			{
				this.EnterHand();
			}
		}

		// Token: 0x06000BF2 RID: 3058 RVA: 0x0002150F File Offset: 0x0001F70F
		protected override void OnLeaveBattle()
		{
			base.Battle.CheckManaFreeze();
		}

		// Token: 0x06000BF3 RID: 3059 RVA: 0x0002151C File Offset: 0x0001F71C
		public override IEnumerable<BattleAction> OnDiscard(CardZone srcZone)
		{
			if (srcZone == CardZone.Hand)
			{
				this.LeaveHand();
			}
			return null;
		}

		// Token: 0x06000BF4 RID: 3060 RVA: 0x00021529 File Offset: 0x0001F729
		public override IEnumerable<BattleAction> OnExile(CardZone srcZone)
		{
			if (srcZone == CardZone.Hand)
			{
				this.LeaveHand();
			}
			this.FreezeTimes = this.InitialTimes;
			return null;
		}

		// Token: 0x06000BF5 RID: 3061 RVA: 0x00021542 File Offset: 0x0001F742
		private void EnterHand()
		{
			base.Battle.CheckManaFreeze();
		}

		// Token: 0x06000BF6 RID: 3062 RVA: 0x0002154F File Offset: 0x0001F74F
		private void LeaveHand()
		{
			base.Battle.CheckManaFreeze();
		}

		// Token: 0x04000566 RID: 1382
		private int _freezeTimes;
	}
}
