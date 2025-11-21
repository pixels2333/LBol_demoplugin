using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	// Token: 0x02000311 RID: 785
	[UsedImplicitly]
	public sealed class BaitianCard : Card
	{
		// Token: 0x1700014F RID: 335
		// (get) Token: 0x06000B9E RID: 2974 RVA: 0x000173E5 File Offset: 0x000155E5
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000B9F RID: 2975 RVA: 0x000173E8 File Offset: 0x000155E8
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int count = base.Battle.HandZone.Count;
			yield return new DiscardManyAction(base.Battle.HandZone);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new DrawManyCardAction(count);
			yield break;
		}
	}
}
