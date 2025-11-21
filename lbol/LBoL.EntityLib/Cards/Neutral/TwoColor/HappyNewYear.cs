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
using LBoL.Core.StatusEffects;
using UnityEngine;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x0200028F RID: 655
	[UsedImplicitly]
	public sealed class HappyNewYear : Card
	{
		// Token: 0x06000A45 RID: 2629 RVA: 0x000157E3 File Offset: 0x000139E3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Card attack = base.Battle.RollCard(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), (CardConfig config) => config.Type == CardType.Attack);
			Card defense = base.Battle.RollCard(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), (CardConfig config) => config.Type == CardType.Defense);
			Card card2 = base.Battle.RollCard(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), delegate(CardConfig config)
			{
				CardType type = config.Type;
				return (type == CardType.Attack || type == CardType.Defense) && config.Id != attack.Id && config.Id != defense.Id;
			});
			List<Card> list = new List<Card>();
			list.Add(attack);
			list.Add(card2);
			list.Add(defense);
			List<Card> list2 = list;
			MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(list2, false, false, false)
			{
				Source = this
			};
			yield return new InteractionAction(interaction, false);
			Card card = interaction.SelectedCard;
			yield return new AddCardsToHandAction(new Card[] { card });
			CardType cardType = card.CardType;
			if (cardType != CardType.Attack)
			{
				if (cardType != CardType.Defense)
				{
					Debug.LogWarning(string.Format("{0} should not roll card with type : {1}", this.Name, base.CardType));
					throw new ArgumentOutOfRangeException();
				}
				yield return base.BuffAction<Spirit>(base.Value1, 0, 0, 0, 0.2f);
			}
			else
			{
				yield return base.BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
