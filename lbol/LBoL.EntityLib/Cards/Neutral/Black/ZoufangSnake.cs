using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x02000345 RID: 837
	[UsedImplicitly]
	public sealed class ZoufangSnake : Card
	{
		// Token: 0x06000C30 RID: 3120 RVA: 0x00017EAA File Offset: 0x000160AA
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.SacrificeAction(base.Value1);
			Card[] array = base.Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.OnlyRare, OwnerWeightTable.OnlyNeutral, CardTypeWeightTable.OnlyAbility, false), base.Value2, null);
			MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(array, false, false, false)
			{
				Source = this
			};
			yield return new InteractionAction(interaction, false);
			Card selectedCard = interaction.SelectedCard;
			selectedCard.SetTurnCost(base.Mana);
			yield return new AddCardsToHandAction(new Card[] { selectedCard });
			yield break;
		}
	}
}
