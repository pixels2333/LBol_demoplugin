using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;
namespace LBoL.EntityLib.Cards.Enemy
{
	[UsedImplicitly]
	public sealed class QiannianShenqiCard : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			EnemyUnit enemyUnit = Enumerable.FirstOrDefault<EnemyUnit>(base.Battle.EnemyGroup, (EnemyUnit u) => u is Seija && u.IsAlive);
			if (enemyUnit != null)
			{
				StatusEffect statusEffect = Enumerable.FirstOrDefault<StatusEffect>(enemyUnit.StatusEffects, (StatusEffect se) => se is QiannianShenqiSe);
				if (statusEffect != null)
				{
					if (statusEffect.Level != base.Value1)
					{
						statusEffect.Level = base.Value1;
					}
					((QiannianShenqiSe)statusEffect).LoseLifeVersion = true;
				}
			}
			yield break;
		}
	}
}
