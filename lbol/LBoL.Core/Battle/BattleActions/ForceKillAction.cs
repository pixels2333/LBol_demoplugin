using System;
using System.Collections.Generic;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000182 RID: 386
	public class ForceKillAction : SimpleEventBattleAction<ForceKillEventArgs>
	{
		// Token: 0x06000E95 RID: 3733 RVA: 0x00027ADD File Offset: 0x00025CDD
		public ForceKillAction(Unit source, Unit target)
		{
			base.Args = new ForceKillEventArgs
			{
				Source = source,
				Target = target
			};
		}

		// Token: 0x06000E96 RID: 3734 RVA: 0x00027AFE File Offset: 0x00025CFE
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
