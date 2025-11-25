using System;
using System.Collections.Generic;
using System.Linq;
namespace LBoL.Core.SaveData
{
	public sealed class StageRecord
	{
		public StageRecord Clone()
		{
			StageRecord stageRecord = new StageRecord();
			stageRecord.Id = this.Id;
			stageRecord.Stations = Enumerable.ToList<StationRecord>(Enumerable.Select<StationRecord, StationRecord>(this.Stations, (StationRecord r) => r.Clone()));
			return stageRecord;
		}
		public string Id;
		public List<StationRecord> Stations = new List<StationRecord>();
	}
}
