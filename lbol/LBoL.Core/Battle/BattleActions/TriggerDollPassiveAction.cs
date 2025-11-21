using System;
using System.Collections.Generic;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001AD RID: 429
	public sealed class TriggerDollPassiveAction : EventBattleAction<DollTriggeredEventArgs>
	{
		// Token: 0x06000F57 RID: 3927 RVA: 0x00029481 File Offset: 0x00027681
		public TriggerDollPassiveAction(Doll doll, bool remove = false)
		{
			base.Args = new DollTriggeredEventArgs
			{
				Doll = doll,
				Remove = remove
			};
		}

		// Token: 0x06000F58 RID: 3928 RVA: 0x000294A2 File Offset: 0x000276A2
		internal override IEnumerable<Phase> GetPhases()
		{
			List<DamageAction> damageActions = new List<DamageAction>();
			yield return base.CreatePhase("DollAction", delegate
			{
				Doll doll = this.Args.Doll;
				this.Battle.React(new Reactor(doll.GetPassiveActions(damageActions)), doll, ActionCause.Doll);
			}, false);
			yield return base.CreatePhase("AfterTrigger", delegate
			{
				if (this.Args.Remove)
				{
					Doll doll2 = this.Args.Doll;
					PlayerUnit player = this.Battle.Player;
					if (player.HasDoll(doll2))
					{
						player.RemoveDoll(doll2);
						return;
					}
					Debug.LogError("[Battle] Removing doll " + this.Args.Doll.DebugName + " failed");
				}
			}, true);
			if (damageActions.Count > 0)
			{
				yield return base.CreatePhase("Statistics", delegate
				{
					this.Battle.React(new StatisticalTotalDamageAction(damageActions), this.Args.Doll, ActionCause.Doll);
				}, false);
			}
			yield return base.CreateEventPhase<DollTriggeredEventArgs>("DollTriggeredPassive", base.Args, base.Battle.DollTriggeredPassive);
			yield break;
		}
	}
}
