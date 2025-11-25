using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	[UsedImplicitly]
	public sealed class FutoLight : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			using (IEnumerator<Card> enumerator = base.Battle.HandZone.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Card card = enumerator.Current;
					if (card.Cost.Amount > 1)
					{
						card.NotifyActivating();
						card.SetTurnCost(base.Mana);
					}
				}
				yield break;
			}
			yield break;
		}
	}
}
