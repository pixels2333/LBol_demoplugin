using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x0200044E RID: 1102
	[UsedImplicitly]
	public sealed class SuperNova : Card
	{
		// Token: 0x06000EFA RID: 3834 RVA: 0x0001B24A File Offset: 0x0001944A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			if (base.Battle.DrawZone.Count <= 0 && base.Battle.HandIsFull)
			{
				yield break;
			}
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.DrawZone, (Card card) => card.CardType == CardType.Attack).SampleManyOrAll(base.Value1, base.BattleRng));
			if (list.Count > 0)
			{
				foreach (Card card2 in list)
				{
					if (base.Battle.HandIsFull)
					{
						break;
					}
					yield return new MoveCardAction(card2, CardZone.Hand);
				}
				List<Card>.Enumerator enumerator = default(List<Card>.Enumerator);
			}
			yield break;
			yield break;
		}
	}
}
