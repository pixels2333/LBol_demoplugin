using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class FengyinYaomo : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			EnemyUnit enemy = selector.SelectedEnemy;
			if (enemy.IsAlive)
			{
				yield return base.DebuffAction<Weak>(enemy, 0, base.Value1, 0, 0, true, 0.2f);
				yield return base.DebuffAction<Vulnerable>(enemy, 0, base.Value1, 0, 0, true, 0.2f);
			}
			yield break;
		}
	}
}
