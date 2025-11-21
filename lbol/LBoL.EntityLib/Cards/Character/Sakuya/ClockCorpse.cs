using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.ExtraTurn.Partners;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x02000384 RID: 900
	[UsedImplicitly]
	public sealed class ClockCorpse : Card
	{
		// Token: 0x06000CD6 RID: 3286 RVA: 0x00018AC8 File Offset: 0x00016CC8
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return PerformAction.Effect(base.Battle.Player, "ExtraTime", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			yield return PerformAction.Sfx("ExtraTurnLaunch", 0f);
			yield return PerformAction.Animation(base.Battle.Player, "spell", 1.6f, null, 0f, -1);
			yield return base.BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
			yield return base.BuffAction<ClockCorpseSe>(0, 0, 0, 0, 0.2f);
			yield return new RequestEndPlayerTurnAction();
			yield break;
		}
	}
}
