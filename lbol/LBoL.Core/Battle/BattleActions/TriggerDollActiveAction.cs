using System;
using System.Collections.Generic;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class TriggerDollActiveAction : EventBattleAction<DollTriggeredEventArgs>
	{
		public TriggerDollActiveAction(Doll doll, bool remove = true)
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
				this.Battle.React(new Reactor(doll.GetActiveActions(damageActions)), doll, ActionCause.Doll);
			}, false);
			yield return base.CreatePhase("AfterTrigger", delegate
			{
				if (this.Args.Remove)
				{
					this.Battle.React(new RemoveDollAction(this.Args.Doll), this.Args.Doll, ActionCause.Doll);
				}
			}, true);
			if (damageActions.Count > 0)
			{
				yield return base.CreatePhase("Statistics", delegate
				{
					this.Battle.React(new StatisticalTotalDamageAction(damageActions), this.Args.Doll, ActionCause.Doll);
				}, false);
			}
			yield return base.CreateEventPhase<DollTriggeredEventArgs>("DollTriggeredActive", base.Args, base.Battle.DollTriggeredActive);
			yield break;
		}
	}
}
