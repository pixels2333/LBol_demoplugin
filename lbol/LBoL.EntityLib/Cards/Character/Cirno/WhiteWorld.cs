using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004D4 RID: 1236
	[UsedImplicitly]
	public sealed class WhiteWorld : Card
	{
		// Token: 0x0600105D RID: 4189 RVA: 0x0001CE8F File Offset: 0x0001B08F
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<WhiteWorldSe>(1, 0, 0, 0, 0.2f);
			foreach (BattleAction battleAction in base.DebuffAction<Cold>(base.Battle.AllAliveEnemies, 0, 0, 0, 0, true, 0.03f))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}
	}
}
