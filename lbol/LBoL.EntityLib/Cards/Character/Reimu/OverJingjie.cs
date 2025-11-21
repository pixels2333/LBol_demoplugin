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
	// Token: 0x020003E5 RID: 997
	[UsedImplicitly]
	public sealed class OverJingjie : Card
	{
		// Token: 0x06000DF4 RID: 3572 RVA: 0x00019F25 File Offset: 0x00018125
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Card[] array = base.Battle.RollCards(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), base.Value1, (CardConfig config) => config.Type == CardType.Attack);
			if (array.Length != 0)
			{
				MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(array, false, false, false)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				Card selectedCard = interaction.SelectedCard;
				selectedCard.SetTurnCost(base.Mana);
				yield return new AddCardsToHandAction(new Card[] { selectedCard });
				interaction = null;
			}
			yield break;
		}
	}
}
