using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x02000297 RID: 663
	[UsedImplicitly]
	public sealed class LifeGame : Card
	{
		// Token: 0x06000A5D RID: 2653 RVA: 0x000159FF File Offset: 0x00013BFF
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> list = new List<Card>();
			Card card = base.Battle.HandZone.TryGetValue(base.HandIndexWhenPlaying - 1);
			Card card2 = base.Battle.HandZone.TryGetValue(base.HandIndexWhenPlaying);
			if (card != null)
			{
				list.Add(card);
			}
			if (card2 != null)
			{
				list.Add(card2);
			}
			List<Card> list2 = new List<Card>();
			List<Card> exile = new List<Card>();
			if (list.Count > 0)
			{
				foreach (Card card3 in list)
				{
					CardType cardType = card3.CardType;
					if (cardType == CardType.Misfortune || cardType == CardType.Status)
					{
						exile.Add(card3);
					}
					else
					{
						card3.NotifyActivating();
						card3.SetTurnCost(base.Mana);
						if (this.IsUpgraded && card3.CanUpgrade)
						{
							list2.Add(card3);
						}
					}
				}
				if (list2.Count > 0)
				{
					yield return new UpgradeCardsAction(list2);
				}
				if (exile.Count > 0)
				{
					yield return new ExileManyCardAction(exile);
				}
			}
			yield break;
		}
	}
}
