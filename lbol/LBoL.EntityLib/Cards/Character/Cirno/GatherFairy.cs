using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;
namespace LBoL.EntityLib.Cards.Character.Cirno
{
	[UsedImplicitly]
	public sealed class GatherFairy : Card
	{
		[UsedImplicitly]
		public int MaxHand
		{
			get
			{
				return 12;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<MaxHandSe>(0, 0, 0, 0, 0.2f);
			yield return new DrawManyCardAction(base.Value1);
			if (base.Value2 > 0)
			{
				using (IEnumerator<Card> enumerator = Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card.CardType == CardType.Friend).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Card card2 = enumerator.Current;
						card2.NotifyActivating();
						card2.Loyalty += base.Value2;
					}
					yield break;
				}
			}
			yield break;
		}
	}
}
