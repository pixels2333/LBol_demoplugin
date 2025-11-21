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
using LBoL.EntityLib.Cards.Neutral.NoColor;

namespace LBoL.EntityLib.Cards.Neutral.White
{
	// Token: 0x02000272 RID: 626
	[UsedImplicitly]
	public sealed class BailianNingshen : Card
	{
		// Token: 0x060009F3 RID: 2547 RVA: 0x0001511F File Offset: 0x0001331F
		protected override string GetBaseDescription()
		{
			if (base.DebutActive || !this.IsUpgraded)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}

		// Token: 0x060009F4 RID: 2548 RVA: 0x0001513E File Offset: 0x0001333E
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Card[] array = base.Battle.RollCards(new CardWeightTable(RarityWeightTable.OnlyCommon, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), base.Value1, null);
			if (array.Length != 0)
			{
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
				if (base.TriggeredAnyhow)
				{
					yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<WManaCard>() });
				}
				interaction = null;
			}
			yield break;
		}
	}
}
