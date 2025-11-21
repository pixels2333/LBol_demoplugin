using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003AC RID: 940
	[UsedImplicitly]
	public sealed class SakuyaInvestigate : Card
	{
		// Token: 0x06000D5B RID: 3419 RVA: 0x000193F2 File Offset: 0x000175F2
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<SakuyaInvestigateSe>(base.Value1, 0, 0, 0, 0.2f);
			foreach (BattleAction battleAction in base.DebuffAction<LockedOn>(base.Battle.AllAliveEnemies, base.Value1, 0, 0, 0, true, 0.03f))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}
	}
}
