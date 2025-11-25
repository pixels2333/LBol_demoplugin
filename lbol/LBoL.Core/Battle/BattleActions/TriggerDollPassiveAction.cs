using System;
using System.Collections.Generic;
using LBoL.Core.Units;
using UnityEngine;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class TriggerDollPassiveAction : EventBattleAction<DollTriggeredEventArgs>
	{
		public TriggerDollPassiveAction(Doll doll, bool remove = false)
		{
			base.Args = new DollTriggeredEventArgs
			{
				Doll = doll,
				Remove = remove
			};
		}
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
