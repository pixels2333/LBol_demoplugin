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

namespace LBoL.EntityLib.Cards.Tool
{
	// Token: 0x02000267 RID: 615
	[UsedImplicitly]
	public sealed class ToolRandomTool : Card
	{
		// Token: 0x060009DC RID: 2524 RVA: 0x00014FFB File Offset: 0x000131FB
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Card[] array = base.Battle.RollCards(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.OnlyTool, false), base.Value1, null);
			if (array.Length != 0)
			{
				foreach (Card card in array)
				{
					card.DeckCounter = new int?(1);
					card.IsCopy = true;
				}
				MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(array, false, false, false)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				yield return new AddCardsToHandAction(new Card[] { interaction.SelectedCard });
				interaction = null;
			}
			yield break;
		}
	}
}
