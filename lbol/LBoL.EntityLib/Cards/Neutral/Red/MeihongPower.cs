using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.Red;

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002CB RID: 715
	[UsedImplicitly]
	public sealed class MeihongPower : Card
	{
		// Token: 0x06000AE4 RID: 2788 RVA: 0x0001641B File Offset: 0x0001461B
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<MeihongPowerSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
