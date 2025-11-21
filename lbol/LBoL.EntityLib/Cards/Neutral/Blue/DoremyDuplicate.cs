using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	// Token: 0x02000313 RID: 787
	[UsedImplicitly]
	public sealed class DoremyDuplicate : Card
	{
		// Token: 0x06000BA3 RID: 2979 RVA: 0x00017420 File Offset: 0x00015620
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this && hand.CanBeDuplicated));
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectHandInteraction(1, 1, list);
		}

		// Token: 0x06000BA4 RID: 2980 RVA: 0x00017462 File Offset: 0x00015662
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				Card card = ((SelectHandInteraction)precondition).SelectedCards[0];
				List<Card> list = new List<Card>();
				for (int i = 0; i < base.Value1; i++)
				{
					list.Add(card.CloneBattleCard());
				}
				yield return new AddCardsToHandAction(list, AddCardsType.Normal);
			}
			yield break;
		}
	}
}
