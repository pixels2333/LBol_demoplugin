using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.EntityLib.Adventures.Stage2;

namespace LBoL.EntityLib.Stages
{
	// Token: 0x020000FD RID: 253
	[UsedImplicitly]
	public sealed class SingleRoute : Stage
	{
		// Token: 0x0600038F RID: 911 RVA: 0x00009404 File Offset: 0x00007604
		public SingleRoute()
		{
			base.Level = 1;
			base.EnemyPoolAct1 = new UniqueRandomPool<string>(true) { { "11", 1f } };
			base.EnemyPoolAct2 = new UniqueRandomPool<string>(true) { { "14", 1f } };
			base.EnemyPoolAct3 = new UniqueRandomPool<string>(true) { { "17", 1f } };
			base.EliteEnemyPool = new UniqueRandomPool<string>(true) { { "Doremy", 1f } };
			base.BossPool = new RepeatableRandomPool<string> { { "Alice", 1f } };
			base.AdventurePool = new UniqueRandomPool<Type>(true) { 
			{
				typeof(BuduSuanming),
				1f
			} };
		}

		// Token: 0x06000390 RID: 912 RVA: 0x000094CA File Offset: 0x000076CA
		public override void InitBoss(RandomGen rng)
		{
			base.Boss = Library.GetEnemyGroupEntry(base.BossPool.SampleOrDefault(rng));
		}

		// Token: 0x06000391 RID: 913 RVA: 0x000094E3 File Offset: 0x000076E3
		public override GameMap CreateMap()
		{
			return GameMap.CreateSingleRoute(base.Boss.Id, new StationType[] { StationType.Boss });
		}
	}
}
