using System;
using System.Collections.Generic;
using LBoL.Base.Extensions;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200019B RID: 411
	public sealed class RemoveDollAction : EventBattleAction<DollEventArgs>
	{
		// Token: 0x06000F0D RID: 3853 RVA: 0x00028A5C File Offset: 0x00026C5C
		public RemoveDollAction(Doll doll)
		{
			base.Args = new DollEventArgs
			{
				Doll = doll
			};
		}

		// Token: 0x06000F0E RID: 3854 RVA: 0x00028A76 File Offset: 0x00026C76
		internal override IEnumerable<Phase> GetPhases()
		{
			RemoveDollAction.<>c__DisplayClass1_0 CS$<>8__locals1 = new RemoveDollAction.<>c__DisplayClass1_0();
			CS$<>8__locals1.<>4__this = this;
			yield return base.CreateEventPhase<DollEventArgs>("DollRemoving", base.Args, base.Battle.DollRemoving);
			CS$<>8__locals1.doll = base.Args.Doll;
			CS$<>8__locals1.reactor = CS$<>8__locals1.doll.OnRemove();
			if (CS$<>8__locals1.reactor != null)
			{
				RemoveDollAction.<>c__DisplayClass1_1 CS$<>8__locals2 = new RemoveDollAction.<>c__DisplayClass1_1();
				CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
				CS$<>8__locals2.damageActions = new List<DamageAction>();
				yield return base.CreatePhase("SpecialRemove", delegate
				{
					CS$<>8__locals2.CS$<>8__locals1.<>4__this.React(new Reactor(StatisticalTotalDamageAction.WrapReactorWithStats(CS$<>8__locals2.CS$<>8__locals1.reactor, CS$<>8__locals2.damageActions)), CS$<>8__locals2.CS$<>8__locals1.doll, new ActionCause?(ActionCause.Doll));
				}, false);
				if (CS$<>8__locals2.damageActions.NotEmpty<DamageAction>())
				{
					yield return base.CreatePhase("Statistics", delegate
					{
						CS$<>8__locals2.CS$<>8__locals1.<>4__this.Battle.React(new StatisticalTotalDamageAction(CS$<>8__locals2.damageActions), CS$<>8__locals2.CS$<>8__locals1.doll, ActionCause.Doll);
					}, false);
				}
				CS$<>8__locals2 = null;
			}
			yield return base.CreatePhase("Main", delegate
			{
				PlayerUnit player = CS$<>8__locals1.<>4__this.Battle.Player;
				if (player.HasDoll(CS$<>8__locals1.doll))
				{
					player.RemoveDoll(CS$<>8__locals1.doll);
					return;
				}
				Debug.LogError("[Battle] Removing doll " + CS$<>8__locals1.<>4__this.Args.Doll.DebugName + " failed");
			}, true);
			yield return base.CreateEventPhase<DollEventArgs>("DollRemoved", base.Args, base.Battle.DollRemoved);
			yield break;
		}
	}
}
