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

namespace LBoL.EntityLib.Cards.DebugCards
{
	// Token: 0x02000379 RID: 889
	[UsedImplicitly]
	public sealed class DebugPlaySelectCard : Card
	{
		// Token: 0x06000CB7 RID: 3255 RVA: 0x00018864 File Offset: 0x00016A64
		public override Interaction Precondition()
		{
			IEnumerable<Card> enumerable = Enumerable.Where<Card>(base.Battle.EnumerateAllCardsButPlayingAreas(), (Card c) => c != this);
			return new SelectCardInteraction(0, base.Value1, enumerable, SelectedCardHandling.DoNothing);
		}

		// Token: 0x06000CB8 RID: 3256 RVA: 0x0001889C File Offset: 0x00016A9C
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				IReadOnlyList<Card> selectedCards = ((SelectCardInteraction)precondition).SelectedCards;
				if (Enumerable.Any<Card>(selectedCards))
				{
					foreach (Card card in selectedCards)
					{
						if (base.Battle.BattleShouldEnd)
						{
							yield break;
						}
						yield return new PlayCardAction(card);
					}
					IEnumerator<Card> enumerator = null;
				}
			}
			yield break;
			yield break;
		}
	}
}
