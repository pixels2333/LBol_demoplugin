using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Cards.Neutral.Black
{
	[UsedImplicitly]
	public sealed class XiaodingKill : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			EnemyUnit selectedEnemy = selector.SelectedEnemy;
			if (selectedEnemy.Hp <= base.Value1)
			{
				yield return new ForceKillAction(base.Battle.Player, selectedEnemy);
			}
			yield break;
		}
	}
}
