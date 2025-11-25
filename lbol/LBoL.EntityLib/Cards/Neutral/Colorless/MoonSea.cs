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
	[UsedImplicitly]
	public sealed class MoonSea : Card
	{
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
