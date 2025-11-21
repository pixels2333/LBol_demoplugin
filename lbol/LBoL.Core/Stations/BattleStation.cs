using System;
using LBoL.Core.SaveData;
using LBoL.Core.Units;

namespace LBoL.Core.Stations
{
	// Token: 0x020000BC RID: 188
	public abstract class BattleStation : Station
	{
		// Token: 0x06000842 RID: 2114 RVA: 0x000186F9 File Offset: 0x000168F9
		protected BattleStation()
		{
			base.Status = StationStatus.Battle;
		}

		// Token: 0x170002A5 RID: 677
		// (get) Token: 0x06000843 RID: 2115 RVA: 0x00018708 File Offset: 0x00016908
		// (set) Token: 0x06000844 RID: 2116 RVA: 0x00018710 File Offset: 0x00016910
		public EnemyGroupEntry EnemyGroupEntry { get; internal set; }

		// Token: 0x170002A6 RID: 678
		// (get) Token: 0x06000845 RID: 2117 RVA: 0x00018719 File Offset: 0x00016919
		// (set) Token: 0x06000846 RID: 2118 RVA: 0x00018721 File Offset: 0x00016921
		public EnemyGroup EnemyGroup { get; private set; }

		// Token: 0x06000847 RID: 2119
		protected abstract EnemyGroupEntry GetEnemyGroupEntry();

		// Token: 0x06000848 RID: 2120 RVA: 0x0001872A File Offset: 0x0001692A
		public virtual void GenerateRewards()
		{
		}

		// Token: 0x06000849 RID: 2121 RVA: 0x0001872C File Offset: 0x0001692C
		protected internal override void OnEnter()
		{
			if (this.EnemyGroupEntry == null)
			{
				this.EnemyGroupEntry = this.GetEnemyGroupEntry();
			}
			this.EnemyGroup = this.EnemyGroupEntry.Generate(base.GameRun);
		}

		// Token: 0x0600084A RID: 2122 RVA: 0x00018766 File Offset: 0x00016966
		internal override StationRecord GenerateRecord()
		{
			return new StationRecord
			{
				Type = this.Type,
				EnemyGroup = this.EnemyGroupEntry.Id
			};
		}
	}
}
