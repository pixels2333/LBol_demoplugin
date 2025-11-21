using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000422 RID: 1058
	[UsedImplicitly]
	public sealed class HuoliQuankai : Card
	{
		// Token: 0x06000E88 RID: 3720 RVA: 0x0001A9E3 File Offset: 0x00018BE3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<BurstUpgrade>(0, 0, 0, 0, 0.2f);
			if (this.IsUpgraded)
			{
				yield return base.BuffAction<Burst>(1, 0, 0, 0, 0.2f);
			}
			else
			{
				yield return base.BuffAction<Charging>(base.Value1, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
