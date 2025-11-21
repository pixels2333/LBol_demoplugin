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
using LBoL.Core.Randoms;

namespace LBoL.EntityLib.Cards.Neutral.MultiColor
{
	// Token: 0x020002EF RID: 751
	[UsedImplicitly]
	public sealed class PatchouliLibrary : Card
	{
		// Token: 0x06000B35 RID: 2869 RVA: 0x00016A3E File Offset: 0x00014C3E
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			List<Card> list = Enumerable.ToList<Card>(base.Battle.RollCards(new CardWeightTable(RarityWeightTable.OnlyRare, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), base.Value1, null));
			if (base.Value2 > 0)
			{
				list.AddRange(base.Battle.RollCards(new CardWeightTable(RarityWeightTable.OnlyUncommon, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), base.Value2, null));
			}
			if (list.Count > 0)
			{
				SelectCardInteraction interaction = new SelectCardInteraction(1, 1, list, SelectedCardHandling.DoNothing)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				Card card = Enumerable.FirstOrDefault<Card>(interaction.SelectedCards);
				if (card != null)
				{
					card.IsEthereal = true;
					card.IsExile = true;
					yield return new AddCardsToHandAction(new Card[] { card });
				}
				interaction = null;
			}
			yield break;
		}
	}
}
