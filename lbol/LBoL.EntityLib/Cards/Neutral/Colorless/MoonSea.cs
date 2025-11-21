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
using LBoL.EntityLib.Cards.Neutral.NoColor;

namespace LBoL.EntityLib.Cards.Neutral.Colorless
{
	// Token: 0x0200030A RID: 778
	[UsedImplicitly]
	public sealed class MoonSea : Card
	{
		// Token: 0x06000B8F RID: 2959 RVA: 0x00017298 File Offset: 0x00015498
		public override Interaction Precondition()
		{
			if (!this.IsUpgraded)
			{
				return null;
			}
			List<Card> list = new List<Card>();
			list.Add(Library.CreateCard<CManaCard>());
			list.Add(Library.CreateCard<UManaCard>());
			List<Card> list2 = list;
			return new SelectCardInteraction(1, 1, list2, SelectedCardHandling.DoNothing);
		}

		// Token: 0x06000B90 RID: 2960 RVA: 0x000172D4 File Offset: 0x000154D4
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			if (precondition == null)
			{
				yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<CManaCard>() });
				yield break;
			}
			SelectCardInteraction selectCardInteraction = (SelectCardInteraction)precondition;
			Card card = ((selectCardInteraction != null) ? Enumerable.FirstOrDefault<Card>(selectCardInteraction.SelectedCards) : null);
			if (card != null)
			{
				yield return new AddCardsToHandAction(new Card[] { card });
			}
			else
			{
				yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<CManaCard>() });
			}
			yield break;
		}
	}
}
