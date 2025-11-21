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

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x0200049D RID: 1181
	[UsedImplicitly]
	public sealed class ChooseFriend : Card
	{
		// Token: 0x06000FC6 RID: 4038 RVA: 0x0001C16E File Offset: 0x0001A36E
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Card[] array = base.Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.OnlyFriend, false), base.Value1, (CardConfig config) => config.Cost.Amount < 5);
			foreach (Card card in array)
			{
				card.SetBaseCost(ManaGroup.Anys(card.ConfigCost.Amount));
			}
			SelectCardInteraction interaction = new SelectCardInteraction(0, base.Value2, array, SelectedCardHandling.DoNothing)
			{
				Source = this
			};
			yield return new InteractionAction(interaction, false);
			IReadOnlyList<Card> selectedCards = interaction.SelectedCards;
			if (selectedCards.Count > 0)
			{
				yield return new AddCardsToHandAction(selectedCards, AddCardsType.Normal);
			}
			yield break;
		}
	}
}
