using System;
using LBoL.Core.SaveData;
using LBoL.Core.Units;
namespace LBoL.Core.Stations
{
	public sealed class SelectStation : Station
	{
		public override StationType Type
		{
			get
			{
				return StationType.Select;
			}
		}
		protected internal override void OnEnter()
		{
			this.Opponents = base.GameRun.GetOpponentCandidates();
		}
		public EnemyUnit[] Opponents { get; private set; }
		internal override StationRecord GenerateRecord()
		{
			return new StationRecord
			{
				Type = this.Type
			};
		}
	}
}
