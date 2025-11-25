using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;
namespace LBoL.EntityLib.Cards.Character.Cirno
{
	[UsedImplicitly]
	public sealed class FrostGarden : Card
	{
		public override bool Triggered
		{
			get
			{
				return this.IsForceCost;
			}
		}
		public override bool IsForceCost
		{
			get
			{
				if (base.Battle != null)
				{
					return Enumerable.Any<Card>(base.Battle.HandZone, (Card card) => card.CardType == CardType.Friend && card.Summoned);
				}
				return false;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			foreach (BattleAction battleAction in base.DebuffAction<Cold>(base.Battle.AllAliveEnemies, 0, 0, 0, 0, true, 0.1f))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			if (this.IsUpgraded)
			{
				yield return base.BuffAction<NextTurnColdAll>(1, 0, 0, 0, 0.2f);
			}
			yield break;
			yield break;
		}
	}
}
