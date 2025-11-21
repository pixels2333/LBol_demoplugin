using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x0200042E RID: 1070
	[UsedImplicitly]
	public sealed class MarisaHoldCard : Card
	{
		// Token: 0x06000EA0 RID: 3744 RVA: 0x0001AB1C File Offset: 0x00018D1C
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this && !hand.IsTempRetain && !hand.IsRetain && !hand.Summoned));
			if (!list.Empty<Card>())
			{
				return new SelectHandInteraction(0, base.Value1, list);
			}
			return null;
		}

		// Token: 0x06000EA1 RID: 3745 RVA: 0x0001AB62 File Offset: 0x00018D62
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			if (precondition != null)
			{
				using (IEnumerator<Card> enumerator = ((SelectHandInteraction)precondition).SelectedCards.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Card card = enumerator.Current;
						if (!card.IsRetain && !card.Summoned)
						{
							card.IsTempRetain = true;
						}
					}
					yield break;
				}
			}
			yield break;
		}
	}
}
