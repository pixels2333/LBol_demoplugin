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

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000478 RID: 1144
	[UsedImplicitly]
	public sealed class KoishiPlayDiscard : Card
	{
		// Token: 0x06000F58 RID: 3928 RVA: 0x0001B838 File Offset: 0x00019A38
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(base.Battle.DiscardZone);
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectCardInteraction(0, base.Value1, list, SelectedCardHandling.DoNothing);
		}

		// Token: 0x06000F59 RID: 3929 RVA: 0x0001B86F File Offset: 0x00019A6F
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				IReadOnlyList<Card> selectedCards = ((SelectCardInteraction)precondition).SelectedCards;
				if (selectedCards.Count > 0)
				{
					foreach (Card card in selectedCards)
					{
						if (!card.IsFollowCard)
						{
							card.IsTempExile = true;
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
