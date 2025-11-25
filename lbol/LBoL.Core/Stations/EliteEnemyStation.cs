using System;
using LBoL.Core.Units;
namespace LBoL.Core.Stations
{
	public sealed class EliteEnemyStation : BattleStation
	{
		public override StationType Type
		{
			get
			{
				return StationType.EliteEnemy;
			}
		}
		public override void GenerateRewards()
		{
			base.GenerateEliteEnemyRewards();
		}
		protected override EnemyGroupEntry GetEnemyGroupEntry()
		{
			return base.Stage.GetEliteEnemies(this);
		}
	}
}
