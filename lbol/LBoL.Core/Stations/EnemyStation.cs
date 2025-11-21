using System;
using LBoL.Core.Units;

namespace LBoL.Core.Stations
{
	// Token: 0x020000BF RID: 191
	public sealed class EnemyStation : BattleStation
	{
		// Token: 0x170002AB RID: 683
		// (get) Token: 0x06000859 RID: 2137 RVA: 0x0001890D File Offset: 0x00016B0D
		public override StationType Type
		{
			get
			{
				return StationType.Enemy;
			}
		}

		// Token: 0x0600085A RID: 2138 RVA: 0x00018910 File Offset: 0x00016B10
		public override void GenerateRewards()
		{
			base.GenerateEnemyRewards();
		}

		// Token: 0x0600085B RID: 2139 RVA: 0x00018918 File Offset: 0x00016B18
		protected override EnemyGroupEntry GetEnemyGroupEntry()
		{
			return base.Stage.GetEnemies(this);
		}
	}
}
