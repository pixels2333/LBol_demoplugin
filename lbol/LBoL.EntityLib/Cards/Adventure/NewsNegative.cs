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
	// Token: 0x020004FA RID: 1274
	[UsedImplicitly]
	public sealed class NewsNegative : Card
	{
		// Token: 0x170001D9 RID: 473
		// (get) Token: 0x060010BF RID: 4287 RVA: 0x0001D384 File Offset: 0x0001B584
		public override bool Negative
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060010C0 RID: 4288 RVA: 0x0001D387 File Offset: 0x0001B587
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new LosePowerAction(base.Value1);
			yield break;
		}
	}
}
