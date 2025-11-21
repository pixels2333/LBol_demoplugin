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
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003A7 RID: 935
	[UsedImplicitly]
	public sealed class SakuyaDio : Card
	{
		// Token: 0x06000D4C RID: 3404 RVA: 0x000192E5 File Offset: 0x000174E5
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return PerformAction.Effect(base.Battle.Player, "ExtraTime", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			yield return PerformAction.Sfx("ExtraTurnLaunch", 0f);
			yield return PerformAction.Animation(base.Battle.Player, "spell", 1.6f, null, 0f, -1);
			yield return base.BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
			yield return base.BuffAction<KnifeWithLockedOn>(base.Value1, 0, 0, 0, 0.2f);
			yield return base.BuffAction<DrawPhaseAddKnife>(0, 0, base.Value2, 0, 0.2f);
			yield return new RequestEndPlayerTurnAction();
			yield break;
		}
	}
}
