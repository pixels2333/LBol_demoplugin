using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Others;
namespace LBoL.EntityLib.Cards.Neutral.Black
{
	[UsedImplicitly]
	public sealed class ShannvAttack : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (this.IsUpgraded)
			{
				foreach (BattleAction battleAction in base.DebuffAction<Poison>(base.Battle.AllAliveEnemies, base.Value1, 0, 0, 0, true, 0.1f))
				{
					yield return battleAction;
				}
				IEnumerator<BattleAction> enumerator = null;
			}
			else
			{
				yield return base.DebuffAction<Poison>(selector.SelectedEnemy, base.Value1, 0, 0, 0, true, 0.2f);
			}
			yield break;
			yield break;
		}
	}
}
