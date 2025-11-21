using System;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200017B RID: 379
	public class EnemyMoveAction : SimpleAction
	{
		// Token: 0x06000E66 RID: 3686 RVA: 0x00027422 File Offset: 0x00025622
		public EnemyMoveAction(EnemyUnit enemy, string moveName, bool closeMoveName = true)
		{
			this.Enemy = enemy;
			this.MoveName = moveName;
			this.CloseMoveName = closeMoveName;
		}

		// Token: 0x170004FE RID: 1278
		// (get) Token: 0x06000E67 RID: 3687 RVA: 0x0002743F File Offset: 0x0002563F
		public EnemyUnit Enemy { get; }

		// Token: 0x170004FF RID: 1279
		// (get) Token: 0x06000E68 RID: 3688 RVA: 0x00027447 File Offset: 0x00025647
		public string MoveName { get; }

		// Token: 0x17000500 RID: 1280
		// (get) Token: 0x06000E69 RID: 3689 RVA: 0x0002744F File Offset: 0x0002564F
		public bool CloseMoveName { get; }
	}
}
