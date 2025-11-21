using System;
using LBoL.Core.SaveData;
using LBoL.Core.Units;

namespace LBoL.Core.Stations
{
	// Token: 0x020000C3 RID: 195
	public sealed class SelectStation : Station
	{
		// Token: 0x170002B1 RID: 689
		// (get) Token: 0x0600086A RID: 2154 RVA: 0x00018B2F File Offset: 0x00016D2F
		public override StationType Type
		{
			get
			{
				return StationType.Select;
			}
		}

		// Token: 0x0600086B RID: 2155 RVA: 0x00018B32 File Offset: 0x00016D32
		protected internal override void OnEnter()
		{
			this.Opponents = base.GameRun.GetOpponentCandidates();
		}

		// Token: 0x170002B2 RID: 690
		// (get) Token: 0x0600086C RID: 2156 RVA: 0x00018B45 File Offset: 0x00016D45
		// (set) Token: 0x0600086D RID: 2157 RVA: 0x00018B4D File Offset: 0x00016D4D
		public EnemyUnit[] Opponents { get; private set; }

		// Token: 0x0600086E RID: 2158 RVA: 0x00018B56 File Offset: 0x00016D56
		internal override StationRecord GenerateRecord()
		{
			return new StationRecord
			{
				Type = this.Type
			};
		}
	}
}
