using System;
using System.Collections.Generic;
using System.Linq;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000E6 RID: 230
	public sealed class StageRecord
	{
		// Token: 0x060008FE RID: 2302 RVA: 0x0001A418 File Offset: 0x00018618
		public StageRecord Clone()
		{
			StageRecord stageRecord = new StageRecord();
			stageRecord.Id = this.Id;
			stageRecord.Stations = Enumerable.ToList<StationRecord>(Enumerable.Select<StationRecord, StationRecord>(this.Stations, (StationRecord r) => r.Clone()));
			return stageRecord;
		}

		// Token: 0x0400049B RID: 1179
		public string Id;

		// Token: 0x0400049C RID: 1180
		public List<StationRecord> Stations = new List<StationRecord>();
	}
}
