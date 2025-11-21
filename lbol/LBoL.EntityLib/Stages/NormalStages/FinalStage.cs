using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.EntityLib.Exhibits;

namespace LBoL.EntityLib.Stages.NormalStages
{
	// Token: 0x02000101 RID: 257
	[UsedImplicitly]
	public sealed class FinalStage : Stage
	{
		// Token: 0x0600039C RID: 924 RVA: 0x00009B98 File Offset: 0x00007D98
		public FinalStage()
		{
			this.isNormalStage = true;
			base.Level = 4;
			base.EternalStageMusic = true;
			base.DontAutoOpenMapInEntry = true;
			base.EnterWithSpecialPresentation = true;
			base.BossPool = new RepeatableRandomPool<string> { { "Seija", 1f } };
			base.SentinelExhibitType = typeof(KongZhanpinhe);
		}

		// Token: 0x0600039D RID: 925 RVA: 0x00009BF9 File Offset: 0x00007DF9
		public override void InitBoss(RandomGen rng)
		{
			base.Boss = Library.GetEnemyGroupEntry(base.BossPool.SampleOrDefault(rng));
		}

		// Token: 0x0600039E RID: 926 RVA: 0x00009C12 File Offset: 0x00007E12
		public override GameMap CreateMap()
		{
			return GameMap.CreateSingleRoute(base.Boss.Id, new StationType[]
			{
				StationType.Shop,
				StationType.Gap,
				StationType.Boss
			});
		}
	}
}
