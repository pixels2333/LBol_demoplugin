using System;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public class EnemyMoveAction : SimpleAction
	{
		public EnemyMoveAction(EnemyUnit enemy, string moveName, bool closeMoveName = true)
		{
			this.Enemy = enemy;
			this.MoveName = moveName;
			this.CloseMoveName = closeMoveName;
		}
		public EnemyUnit Enemy { get; }
		public string MoveName { get; }
		public bool CloseMoveName { get; }
	}
}
