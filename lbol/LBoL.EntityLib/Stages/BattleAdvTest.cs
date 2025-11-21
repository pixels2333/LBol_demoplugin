using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.EntityLib.Exhibits;

namespace LBoL.EntityLib.Stages
{
	// Token: 0x020000FA RID: 250
	[UsedImplicitly]
	public sealed class BattleAdvTest : Stage
	{
		// Token: 0x06000385 RID: 901 RVA: 0x000091ED File Offset: 0x000073ED
		public BattleAdvTest()
		{
			base.Level = 3;
			base.BossPool = new RepeatableRandomPool<string> { { "Remilia", 1f } };
			base.SentinelExhibitType = typeof(KongZhanpinhe);
		}

		// Token: 0x06000386 RID: 902 RVA: 0x00009227 File Offset: 0x00007427
		public override void InitBoss(RandomGen rng)
		{
			base.Boss = Library.GetEnemyGroupEntry(base.BossPool.SampleOrDefault(rng));
		}

		// Token: 0x06000387 RID: 903 RVA: 0x00009240 File Offset: 0x00007440
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
