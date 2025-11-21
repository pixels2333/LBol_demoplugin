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
	// Token: 0x020002E6 RID: 742
	[UsedImplicitly]
	public sealed class VampireShoot1 : Card
	{
		// Token: 0x06000B24 RID: 2852 RVA: 0x00016900 File Offset: 0x00014B00
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			EnemyUnit target = selector.GetEnemy(base.Battle);
			yield return base.AttackAction(target);
			yield return new HealAction(target, base.Battle.Player, base.Value1, HealType.Vampire, 0.2f);
			yield break;
		}
	}
}
