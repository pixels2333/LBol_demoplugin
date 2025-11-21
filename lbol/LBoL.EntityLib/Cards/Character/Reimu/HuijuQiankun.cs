using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003D8 RID: 984
	[UsedImplicitly]
	public sealed class HuijuQiankun : Card
	{
		// Token: 0x06000DCE RID: 3534 RVA: 0x00019BCD File Offset: 0x00017DCD
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> cards = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card != this));
			int count = cards.Count;
			YinyangCard yinyang = Library.CreateCard<YinyangCard>();
			yield return new AddCardsToHandAction(new Card[] { yinyang });
			if (count > 0)
			{
				yield return new ExileManyCardAction(cards);
				yinyang.DeltaDamage += base.Value1 * count;
				yinyang.DeltaShield += base.Value2 * count;
			}
			yield break;
		}
	}
}
