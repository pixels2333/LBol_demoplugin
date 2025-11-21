using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x02000330 RID: 816
	[UsedImplicitly]
	public sealed class GrimoireStudy : Card
	{
		// Token: 0x06000BF1 RID: 3057 RVA: 0x00017954 File Offset: 0x00015B54
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.SacrificeAction(base.Value2);
			List<Card> cards = Enumerable.ToList<Card>(base.Battle.RollCards(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), base.Value1, (CardConfig config) => config.Id != base.Id));
			yield return new AddCardsToHandAction(cards, AddCardsType.OneByOne);
			using (List<Card>.Enumerator enumerator = cards.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Card card = enumerator.Current;
					card.SetTurnCost(base.Mana);
					card.IsEthereal = true;
					card.IsExile = true;
				}
				yield break;
			}
			yield break;
		}
	}
}
