using System;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001A2 RID: 418
	public class SpawnEnemyAction : SimpleEventBattleAction<UnitEventArgs>
	{
		// Token: 0x17000525 RID: 1317
		// (get) Token: 0x06000F22 RID: 3874 RVA: 0x00028D27 File Offset: 0x00026F27
		public float WaitTime { get; }

		// Token: 0x17000526 RID: 1318
		// (get) Token: 0x06000F23 RID: 3875 RVA: 0x00028D2F File Offset: 0x00026F2F
		public float FadeInDelay { get; }

		// Token: 0x06000F24 RID: 3876 RVA: 0x00028D37 File Offset: 0x00026F37
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

		// Token: 0x06000F25 RID: 3877 RVA: 0x00028D77 File Offset: 0x00026F77
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.EnemySpawning);
		}

		// Token: 0x06000F26 RID: 3878 RVA: 0x00028D8A File Offset: 0x00026F8A
		protected override void MainPhase()
		{
			base.Args.Unit = base.Battle.Spawn(this._spawner, this._enemyType, this._rootIndex, this._isServant);
			base.Args.IsModified = true;
		}

		// Token: 0x06000F27 RID: 3879 RVA: 0x00028DC6 File Offset: 0x00026FC6
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.EnemySpawned);
		}

		// Token: 0x04000699 RID: 1689
		private readonly EnemyUnit _spawner;

		// Token: 0x0400069A RID: 1690
		private readonly Type _enemyType;

		// Token: 0x0400069B RID: 1691
		private readonly int _rootIndex;

		// Token: 0x0400069C RID: 1692
		private readonly bool _isServant;
	}
}
