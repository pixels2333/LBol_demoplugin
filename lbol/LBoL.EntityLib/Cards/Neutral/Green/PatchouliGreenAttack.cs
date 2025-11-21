using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Green
{
	// Token: 0x020002FC RID: 764
	[UsedImplicitly]
	public sealed class PatchouliGreenAttack : Card
	{
		// Token: 0x17000144 RID: 324
		// (get) Token: 0x06000B5D RID: 2909 RVA: 0x00016E1E File Offset: 0x0001501E
		public override bool Triggered
		{
			get
			{
				return this.CanUse;
			}
		}

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x06000B5E RID: 2910 RVA: 0x00016E26 File Offset: 0x00015026
		public override bool CanUse
		{
			get
			{
				return this.BattleAmount > this.BaseAmount;
			}
		}

		// Token: 0x17000146 RID: 326
		// (get) Token: 0x06000B5F RID: 2911 RVA: 0x00016E38 File Offset: 0x00015038
		[UsedImplicitly]
		public int BattleAmount
		{
			get
			{
				if (base.Battle == null)
				{
					return 0;
				}
				return base.Battle.BattleMana.Amount;
			}
		}

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x06000B60 RID: 2912 RVA: 0x00016E64 File Offset: 0x00015064
		[UsedImplicitly]
		public int BaseAmount
		{
			get
			{
				return base.GameRun.BaseMana.Amount;
			}
		}

		// Token: 0x06000B61 RID: 2913 RVA: 0x00016E84 File Offset: 0x00015084
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<ManaEventArgs>(battle.ManaGained, delegate(ManaEventArgs _)
			{
				this.NotifyChanged();
			}, (GameEventPriority)0);
			base.HandleBattleEvent<ManaEventArgs>(battle.ManaLost, delegate(ManaEventArgs _)
			{
				this.NotifyChanged();
			}, (GameEventPriority)0);
		}
	}
}
