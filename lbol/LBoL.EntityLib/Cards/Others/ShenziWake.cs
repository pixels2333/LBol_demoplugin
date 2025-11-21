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

namespace LBoL.EntityLib.Cards.Others
{
	// Token: 0x02000270 RID: 624
	[UsedImplicitly]
	public sealed class ShenziWake : Card
	{
		// Token: 0x060009EE RID: 2542 RVA: 0x000150E5 File Offset: 0x000132E5
		public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			return pooledMana;
		}

		// Token: 0x060009EF RID: 2543 RVA: 0x000150E8 File Offset: 0x000132E8
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int num = consumingMana.Amount;
			List<Card> list = new List<Card>();
			while (list.Count < base.Value1 && num >= 0)
			{
				int a = num;
				num--;
				list.AddRange(base.Battle.RollCards(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), base.Value1 - list.Count, (CardConfig config) => config.Cost.Amount == a));
			}
			if (list.Count > 0)
			{
				MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(list, false, false, false)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				Card selectedCard = interaction.SelectedCard;
				selectedCard.SetTurnCost(base.Mana);
				selectedCard.IsEthereal = true;
				selectedCard.IsExile = true;
				yield return new AddCardsToHandAction(new Card[] { selectedCard });
				interaction = null;
			}
			else
			{
				yield return PerformAction.Chat(base.Battle.Player, "ErrorChat.NoCardByMama".Localize(true), 3f, 0f, 0f, true);
			}
			yield break;
		}
	}
}
