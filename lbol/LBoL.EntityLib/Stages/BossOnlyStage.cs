using System;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.EntityLib.Exhibits;
namespace LBoL.EntityLib.Stages
{
	public sealed class BossOnlyStage : Stage
	{
		public BossOnlyStage()
		{
			base.SentinelExhibitType = typeof(KongZhanpinhe);
		}
		public BossOnlyStage WithLevel(int level)
		{
			base.Level = level;
			return this;
		}
		public override void InitBoss(RandomGen rng)
		{
			string text;
			switch (base.Level)
			{
			case 1:
				text = "Koishi";
				break;
			case 2:
				text = "Tianzi";
				break;
			case 3:
				text = "Junko";
				break;
			case 4:
				text = "Sanae";
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			base.Boss = Library.GetEnemyGroupEntry(text);
		}
		public override GameMap CreateMap()
		{
			return GameMap.CreateSingleRoute(base.Boss.Id, new StationType[] { StationType.Boss });
		}
		private string[] BossIds;
	}
}
