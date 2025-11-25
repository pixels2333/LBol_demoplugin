using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Adventure
{
	[UsedImplicitly]
	public sealed class NewsEntertainment : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int cardsToFull = base.Battle.CardsToFull;
			if (cardsToFull > 0)
			{
				yield return new DrawManyCardAction(cardsToFull);
			}
			yield break;
		}
	}
}
