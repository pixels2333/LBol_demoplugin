using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;
namespace LBoL.EntityLib.Cards.Neutral.White
{
	[UsedImplicitly]
	public sealed class ZhenmiaowanAttack : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return base.BuffAction<ZhenmiaowanAttackSe>(1, 0, this.IsUpgraded ? 1 : 0, 0, 0.2f);
			yield break;
		}
	}
}
