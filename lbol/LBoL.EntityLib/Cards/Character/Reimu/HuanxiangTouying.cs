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
	// Token: 0x020003D7 RID: 983
	[UsedImplicitly]
	public sealed class HuanxiangTouying : Card
	{
		// Token: 0x06000DCC RID: 3532 RVA: 0x00019BB5 File Offset: 0x00017DB5
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			List<Card> list = new List<Card>();
			for (int i = 1; i <= base.Value1; i++)
			{
				int amount = i;
				Card card = base.Battle.RollCard(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), (CardConfig config) => config.Cost.Amount == amount);
				if (card != null)
				{
					list.Add(card);
				}
			}
			if (list.Count > 0)
			{
				MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(list, false, false, false)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				Card selectedCard = interaction.SelectedCard;
				selectedCard.IsEthereal = true;
				selectedCard.IsExile = true;
				yield return new AddCardsToHandAction(new Card[] { selectedCard });
				interaction = null;
			}
			yield break;
		}
	}
}
