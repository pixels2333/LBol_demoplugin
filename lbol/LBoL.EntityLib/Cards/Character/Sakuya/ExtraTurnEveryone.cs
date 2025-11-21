using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x0200038C RID: 908
	[UsedImplicitly]
	public sealed class ExtraTurnEveryone : Card
	{
		// Token: 0x06000CF3 RID: 3315 RVA: 0x00018CF0 File Offset: 0x00016EF0
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return PerformAction.Effect(base.Battle.Player, "ExtraTime", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			yield return PerformAction.Sfx("ExtraTurnLaunch", 0f);
			yield return PerformAction.Animation(base.Battle.Player, "spell", 1.6f, null, 0f, -1);
			yield return base.BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
			foreach (BattleAction battleAction in base.DebuffAction<ExtraTurn>(base.Battle.EnemyGroup.Alives, 1, 0, 0, 0, true, 0.2f))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield return new RequestEndPlayerTurnAction();
			yield break;
			yield break;
		}
	}
}
