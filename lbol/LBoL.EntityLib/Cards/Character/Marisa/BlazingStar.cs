using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.ExtraTurn;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x0200040F RID: 1039
	[UsedImplicitly]
	public sealed class BlazingStar : LimitedStopTimeCard
	{
		// Token: 0x06000E56 RID: 3670 RVA: 0x0001A63C File Offset: 0x0001883C
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
			yield return base.BuffAction<TurnStartDontLoseBlock>(1, 0, 0, 0, 0.2f);
			if (base.Limited)
			{
				yield return base.DebuffAction<TimeIsLimited>(base.Battle.Player, 1, 0, 0, 0, true, 0.2f);
			}
			yield return new RequestEndPlayerTurnAction();
			yield break;
		}
	}
}
