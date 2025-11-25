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
	[UsedImplicitly]
	public sealed class TwoBalls : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<YinyangCard> list = Enumerable.ToList<YinyangCard>(Library.CreateCards<YinyangCard>(base.Value1, false));
			if (this.IsUpgraded)
			{
				foreach (YinyangCard yinyangCard in list)
				{
					yinyangCard.DecreaseTurnCost(base.Mana);
				}
			}
			yield return new AddCardsToHandAction(list, AddCardsType.Normal);
			yield break;
		}
	}
}
