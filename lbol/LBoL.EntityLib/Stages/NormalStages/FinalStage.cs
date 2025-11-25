using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.EntityLib.Exhibits;
namespace LBoL.EntityLib.Stages.NormalStages
{
	[UsedImplicitly]
	public sealed class FinalStage : Stage
	{
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
		public override void InitBoss(RandomGen rng)
		{
			base.Boss = Library.GetEnemyGroupEntry(base.BossPool.SampleOrDefault(rng));
		}
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
