using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.EntityLib.Exhibits;
namespace LBoL.EntityLib.Stages
{
	[UsedImplicitly]
	public sealed class BattleAdvTest : Stage
	{
		public BattleAdvTest()
		{
			base.Level = 3;
			base.BossPool = new RepeatableRandomPool<string> { { "Remilia", 1f } };
			base.SentinelExhibitType = typeof(KongZhanpinhe);
		}
		public override void InitBoss(RandomGen rng)
		{
			base.Boss = Library.GetEnemyGroupEntry(base.BossPool.SampleOrDefault(rng));
		}
		public override GameMap CreateMap()
		{
			return GameMap.CreateThreeRoute(base.Boss.Id, new StationType[]
			{
				StationType.BattleAdvTest,
				StationType.Shop,
				StationType.Gap,
				StationType.BattleAdvTest,
				StationType.Shop,
				StationType.Gap,
				StationType.BattleAdvTest,
				StationType.Shop,
				StationType.Gap,
				StationType.BattleAdvTest,
				StationType.Shop,
				StationType.Gap,
				StationType.BattleAdvTest,
				StationType.Shop,
				StationType.Gap
			});
		}
	}
}
