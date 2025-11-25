using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.DebugCards
{
	[UsedImplicitly]
	public sealed class DebugExileDrawDiscard : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			IEnumerable<Card> enumerable = Enumerable.Concat<Card>(base.Battle.DrawZone, base.Battle.DiscardZone);
			yield return new ExileManyCardAction(enumerable);
			yield break;
		}
	}
}
