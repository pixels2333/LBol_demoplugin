using System;
using System.Collections.Generic;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public class ForceKillAction : SimpleEventBattleAction<ForceKillEventArgs>
	{
		public ForceKillAction(Unit source, Unit target)
		{
			base.Args = new ForceKillEventArgs
			{
				Source = source,
				Target = target
			};
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Main", delegate
			{
				base.Battle.ForceKill(base.Args.Source, base.Args.Target);
			}, true);
			yield return base.CreatePhase("Dying", delegate
			{
				base.React(new DieAction(base.Args.Target, DieCause.ForceKill, base.Args.Source, base.Args.ActionSource), base.Source, default(ActionCause?));
			}, false);
			yield break;
		}
	}
}
