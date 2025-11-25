using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Cards.Neutral.NoColor
{
	[UsedImplicitly]
	public sealed class VampireShoot2 : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			EnemyUnit target = selector.GetEnemy(base.Battle);
			yield return base.AttackAction(target);
			yield return new HealAction(target, base.Battle.Player, base.Value1, HealType.Vampire, 0.2f);
			yield break;
		}
	}
}
