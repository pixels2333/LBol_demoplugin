using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.ExtraTurn;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x02000287 RID: 647
	[UsedImplicitly]
	public sealed class AyaExtraTurn : LimitedStopTimeCard
	{
		// Token: 0x06000A32 RID: 2610 RVA: 0x000156C5 File Offset: 0x000138C5
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Graze>(base.Value1, 0, 0, 0, 0.2f);
			yield return base.BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
			if (base.Limited)
			{
				yield return base.DebuffAction<TimeIsLimited>(base.Battle.Player, 1, 0, 0, 0, true, 0.2f);
			}
			yield return new RequestEndPlayerTurnAction();
			yield break;
		}
	}
}
