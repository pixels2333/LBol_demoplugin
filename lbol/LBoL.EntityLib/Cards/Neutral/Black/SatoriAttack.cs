using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Intentions;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Cards.Neutral.Black
{
	[UsedImplicitly]
	public sealed class SatoriAttack : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int attackCount = 0;
			if (base.TriggeredAnyhow)
			{
				EnemyUnit[] enemies = selector.GetEnemies(base.Battle);
				attackCount = Enumerable.Count<EnemyUnit>(enemies, (EnemyUnit enemy) => Enumerable.Any<Intention>(enemy.Intentions, delegate(Intention i)
				{
					if (!(i is AttackIntention))
					{
						SpellCardIntention spellCardIntention = i as SpellCardIntention;
						if (spellCardIntention == null || spellCardIntention.Damage == null)
						{
							return false;
						}
					}
					return true;
				}));
			}
			yield return base.AttackAction(selector, null);
			if (attackCount > 0)
			{
				yield return base.DefenseAction(false);
			}
			yield break;
		}
	}
}
