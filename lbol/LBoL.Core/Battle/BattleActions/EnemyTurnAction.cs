using System;
using System.Collections.Generic;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public class EnemyTurnAction : SimpleAction
	{
		public EnemyUnit Enemy { get; }
		public EnemyTurnAction(EnemyUnit enemy)
		{
			this.Enemy = enemy;
		}
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
