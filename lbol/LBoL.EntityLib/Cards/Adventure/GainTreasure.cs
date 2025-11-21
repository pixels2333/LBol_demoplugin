using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Adventure
{
	// Token: 0x020004F6 RID: 1270
	[UsedImplicitly]
	public sealed class GainTreasure : Card
	{
		// Token: 0x170001D8 RID: 472
		// (get) Token: 0x060010B5 RID: 4277 RVA: 0x0001D2DD File Offset: 0x0001B4DD
		[UsedImplicitly]
		public int TotalMoney
		{
			get
			{
				if (base.GameRun == null)
				{
					return 0;
				}
				return base.GameRun.Stats.TotalGainTreasure;
			}
		}

		// Token: 0x060010B6 RID: 4278 RVA: 0x0001D2F9 File Offset: 0x0001B4F9
		protected override string GetBaseDescription()
		{
			if (!((base.Battle != null) | (this.TotalMoney != 0)))
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}

		// Token: 0x060010B7 RID: 4279 RVA: 0x0001D31D File Offset: 0x0001B51D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainMoneyAction(base.Value1, SpecialSourceType.None);
			yield break;
		}
	}
}
