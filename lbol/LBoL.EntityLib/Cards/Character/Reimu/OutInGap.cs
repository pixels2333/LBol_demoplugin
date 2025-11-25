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
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class OutInGap : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Card[] array = base.Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot, false), base.Value1, (CardConfig config) => config.Id != base.Id);
			MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(array, false, false, false)
			{
				Source = this
			};
			yield return new InteractionAction(interaction, false);
			Card selectedCard = interaction.SelectedCard;
			selectedCard.SetTurnCost(base.Mana);
			selectedCard.IsEthereal = true;
			selectedCard.IsExile = true;
			yield return new AddCardsToHandAction(new Card[] { selectedCard });
			yield break;
		}
	}
}
