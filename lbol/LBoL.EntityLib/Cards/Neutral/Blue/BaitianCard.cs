using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	[UsedImplicitly]
	public sealed class BaitianCard : Card
	{
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int count = base.Battle.HandZone.Count;
			yield return new DiscardManyAction(base.Battle.HandZone);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new DrawManyCardAction(count);
			yield break;
		}
	}
}
