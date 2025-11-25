using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class Jinfuzhen : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			EnemyUnit target = selector.SelectedEnemy;
			yield return base.AttackAction(target);
			if (base.Battle.Player.Shield > 0)
			{
				Unit unit = target;
				int? num = new int?(base.Value1);
				yield return new ApplyStatusEffectAction<Vulnerable>(unit, default(int?), num, default(int?), default(int?), 0f, true);
			}
			yield break;
		}
	}
}
