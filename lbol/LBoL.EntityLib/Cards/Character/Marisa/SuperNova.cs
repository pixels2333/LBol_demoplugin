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
	[UsedImplicitly]
	public sealed class SuperNova : Card
	{
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
