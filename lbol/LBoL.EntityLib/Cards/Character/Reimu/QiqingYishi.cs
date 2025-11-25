using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Neutral.NoColor;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class QiqingYishi : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> list = new List<Card>();
			list.Add(Library.CreateCard<PManaCard>());
			List<Card> list2 = list;
			if (this.IsUpgraded)
			{
				list2.Add(Library.CreateCard<WManaCard>());
			}
			yield return new AddCardsToHandAction(list2, AddCardsType.Normal);
			yield break;
		}
	}
}
