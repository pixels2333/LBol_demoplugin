using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.EntityLib.Adventures.Stage2;
namespace LBoL.EntityLib.Stages
{
	[UsedImplicitly]
	public sealed class QuickFinalPreStage : Stage
	{
		public QuickFinalPreStage()
		{
			base.Level = 3;
			base.EnemyPoolAct1 = new UniqueRandomPool<string>(true) { { "11", 1f } };
			base.EnemyPoolAct2 = new UniqueRandomPool<string>(true) { { "14", 1f } };
			base.EnemyPoolAct3 = new UniqueRandomPool<string>(true) { { "17", 1f } };
			base.EliteEnemyPool = new UniqueRandomPool<string>(true) { { "Doremy", 1f } };
			base.BossPool = new RepeatableRandomPool<string> { { "Sanae", 1f } };
			base.AdventurePool = new UniqueRandomPool<Type>(true) { 
			{
				typeof(BuduSuanming),
				1f
			} };
		}
		public override void InitBoss(RandomGen rng)
		{
			base.Boss = Library.GetEnemyGroupEntry(base.BossPool.SampleOrDefault(rng));
		}
		public override GameMap CreateMap()
		{
			return GameMap.CreateSingleRoute(base.Boss.Id, new StationType[] { StationType.Boss });
		}
	}
}
