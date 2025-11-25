using System;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public class SpawnEnemyAction : SimpleEventBattleAction<UnitEventArgs>
	{
		public float WaitTime { get; }
		public float FadeInDelay { get; }
		public SpawnEnemyAction(EnemyUnit spawner, Type enemyType, int rootIndex, float waitTime = 0f, float fadeInDelay = 0.3f, bool isServant = true)
		{
			base.Args = new UnitEventArgs();
			this._spawner = spawner;
			this._enemyType = enemyType;
			this._rootIndex = rootIndex;
			this.WaitTime = waitTime;
			this.FadeInDelay = fadeInDelay;
			this._isServant = isServant;
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.EnemySpawning);
		}
		protected override void MainPhase()
		{
			base.Args.Unit = base.Battle.Spawn(this._spawner, this._enemyType, this._rootIndex, this._isServant);
			base.Args.IsModified = true;
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.EnemySpawned);
		}
		private readonly EnemyUnit _spawner;
		private readonly Type _enemyType;
		private readonly int _rootIndex;
		private readonly bool _isServant;
	}
}
