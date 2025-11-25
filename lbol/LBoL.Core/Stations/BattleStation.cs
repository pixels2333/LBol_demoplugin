using System;
using LBoL.Core.SaveData;
using LBoL.Core.Units;
namespace LBoL.Core.Stations
{
	public abstract class BattleStation : Station
	{
		protected BattleStation()
		{
			base.Status = StationStatus.Battle;
		}
		public EnemyGroupEntry EnemyGroupEntry { get; internal set; }
		public EnemyGroup EnemyGroup { get; private set; }
		protected abstract EnemyGroupEntry GetEnemyGroupEntry();
		public virtual void GenerateRewards()
		{
		}
		protected internal override void OnEnter()
		{
			if (this.EnemyGroupEntry == null)
			{
				this.EnemyGroupEntry = this.GetEnemyGroupEntry();
			}
			this.EnemyGroup = this.EnemyGroupEntry.Generate(base.GameRun);
		}
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
