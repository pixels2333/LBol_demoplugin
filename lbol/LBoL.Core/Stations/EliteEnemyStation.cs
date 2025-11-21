using System;
using LBoL.Core.Units;

namespace LBoL.Core.Stations
{
	// Token: 0x020000BE RID: 190
	public sealed class EliteEnemyStation : BattleStation
	{
		// Token: 0x170002AA RID: 682
		// (get) Token: 0x06000855 RID: 2133 RVA: 0x000188EC File Offset: 0x00016AEC
		public override StationType Type
		{
			get
			{
				return StationType.EliteEnemy;
			}
		}

		// Token: 0x06000856 RID: 2134 RVA: 0x000188EF File Offset: 0x00016AEF
		public override void GenerateRewards()
		{
			base.GenerateEliteEnemyRewards();
		}

		// Token: 0x06000857 RID: 2135 RVA: 0x000188F7 File Offset: 0x00016AF7
		protected override EnemyGroupEntry GetEnemyGroupEntry()
		{
			return base.Stage.GetEliteEnemies(this);
		}
	}
}
