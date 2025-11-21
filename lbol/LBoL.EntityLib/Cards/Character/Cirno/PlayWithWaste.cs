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

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004CE RID: 1230
	[UsedImplicitly]
	public sealed class PlayWithWaste : Card
	{
		// Token: 0x06001050 RID: 4176 RVA: 0x0001CDE6 File Offset: 0x0001AFE6
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Card[] array = base.Battle.RollCards(new CardWeightTable(RarityWeightTable.NoneRare, OwnerWeightTable.Valid, CardTypeWeightTable.OnlyTool, false), base.Value1, null);
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
