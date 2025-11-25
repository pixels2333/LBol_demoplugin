using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.EntityLib.StatusEffects.Sakuya;
namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	[UsedImplicitly]
	public sealed class TimeMagic : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Card[] array = base.Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.NonCommon, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot, false), base.Value2, (CardConfig config) => config.Id != "TimeMagic");
			SelectCardInteraction interaction = new SelectCardInteraction(0, base.Value1, array, SelectedCardHandling.DoNothing)
			{
				Source = this
			};
			yield return new InteractionAction(interaction, false);
			IReadOnlyList<Card> selectedCards = interaction.SelectedCards;
			if (selectedCards.Count > 0)
			{
				foreach (Card card in selectedCards)
				{
					card.IsEthereal = true;
					card.IsExile = true;
					card.SetBaseCost(ManaGroup.Anys(card.ConfigCost.Amount));
					card.DecreaseTurnCost(base.Mana);
				}
				yield return new AddCardsToHandAction(selectedCards, AddCardsType.Normal);
			}
			yield return base.BuffAction<TimeMagicSe>(base.Value1, 0, base.Value2, 0, 0.2f);
			TimeMagicSe statusEffect = base.Battle.Player.GetStatusEffect<TimeMagicSe>();
			if (statusEffect != null)
			{
				statusEffect.Mana = base.Mana;
			}
			yield break;
		}
	}
}
