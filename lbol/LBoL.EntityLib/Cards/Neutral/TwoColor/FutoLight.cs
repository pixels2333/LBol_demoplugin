using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x0200028E RID: 654
	[UsedImplicitly]
	public sealed class FutoLight : Card
	{
		// Token: 0x06000A43 RID: 2627 RVA: 0x000157CB File Offset: 0x000139CB
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			using (IEnumerator<Card> enumerator = base.Battle.HandZone.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Card card = enumerator.Current;
					if (card.Cost.Amount > 1)
					{
						card.NotifyActivating();
						card.SetTurnCost(base.Mana);
					}
				}
				yield break;
			}
			yield break;
		}
	}
}
