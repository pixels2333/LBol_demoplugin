using System;
using LBoL.Core.Units;
namespace LBoL.Core.Stations
{
	public sealed class EnemyStation : BattleStation
	{
		public override StationType Type
		{
			get
			{
				return StationType.Enemy;
			}
		}
		public override void GenerateRewards()
		{
			base.GenerateEnemyRewards();
		}
		protected override EnemyGroupEntry GetEnemyGroupEntry()
		{
			return base.Stage.GetEnemies(this);
		}
	}
}
