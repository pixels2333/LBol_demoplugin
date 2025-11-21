using System;
using System.Collections.Generic;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200017C RID: 380
	public class EnemyTurnAction : SimpleAction
	{
		// Token: 0x17000501 RID: 1281
		// (get) Token: 0x06000E6A RID: 3690 RVA: 0x00027457 File Offset: 0x00025657
		public EnemyUnit Enemy { get; }

		// Token: 0x06000E6B RID: 3691 RVA: 0x0002745F File Offset: 0x0002565F
		public EnemyTurnAction(EnemyUnit enemy)
		{
			this.Enemy = enemy;
		}

		// Token: 0x06000E6C RID: 3692 RVA: 0x0002746E File Offset: 0x0002566E
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Flow", delegate
			{
				base.React(new StartEnemyTurnAction(this.Enemy), null, default(ActionCause?));
				base.React(new Reactor(this.Enemy.GetActions()), this.Enemy, new ActionCause?(ActionCause.EnemyAction));
				base.React(new EndEnemyTurnAction(this.Enemy), null, default(ActionCause?));
			}, false);
			yield break;
		}
	}
}
