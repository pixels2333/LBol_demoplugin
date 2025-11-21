using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Green
{
	// Token: 0x020002F9 RID: 761
	[UsedImplicitly]
	public sealed class LigeluCard : Card
	{
		// Token: 0x06000B55 RID: 2901 RVA: 0x00016D16 File Offset: 0x00014F16
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return new GainPowerAction(base.Value1);
			yield break;
		}
	}
}
