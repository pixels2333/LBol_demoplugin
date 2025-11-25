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
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	[UsedImplicitly]
	public sealed class YoumuHybrid : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int num = base.SynergyAmount(consumingMana, ManaColor.White, 1);
			int green = base.SynergyAmount(consumingMana, ManaColor.Green, 1);
			if (num > 0)
			{
				if (this.IsUpgraded)
				{
					List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(Enumerable.Concat<Card>(Enumerable.Concat<Card>(base.Battle.HandZone, base.Battle.DrawZoneToShow), base.Battle.DiscardZone), (Card card) => card != this));
					SelectCardInteraction interaction = new SelectCardInteraction(0, num, list, SelectedCardHandling.DoNothing)
					{
						Source = this
					};
					yield return new InteractionAction(interaction, false);
					IReadOnlyList<Card> selectedCards = interaction.SelectedCards;
					if (selectedCards.Count > 0)
					{
						yield return new ExileManyCardAction(selectedCards);
					}
					interaction = null;
				}
				else
				{
					List<Card> list2 = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card != this));
					SelectHandInteraction interaction2 = new SelectHandInteraction(0, num, list2)
					{
						Source = this
					};
					yield return new InteractionAction(interaction2, false);
					IReadOnlyList<Card> selectedCards2 = interaction2.SelectedCards;
					if (selectedCards2.Count > 0)
					{
						yield return new ExileManyCardAction(selectedCards2);
					}
					interaction2 = null;
				}
			}
			if (green > 0)
			{
				yield return base.BuffAction<Graze>(green, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
