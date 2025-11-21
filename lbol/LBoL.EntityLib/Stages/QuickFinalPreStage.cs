using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.EntityLib.Adventures.Stage2;

namespace LBoL.EntityLib.Stages
{
	// Token: 0x020000FC RID: 252
	[UsedImplicitly]
	public sealed class QuickFinalPreStage : Stage
	{
		// Token: 0x0600038C RID: 908 RVA: 0x00009308 File Offset: 0x00007508
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

		// Token: 0x0600038D RID: 909 RVA: 0x000093CE File Offset: 0x000075CE
		public override void InitBoss(RandomGen rng)
		{
			base.Boss = Library.GetEnemyGroupEntry(base.BossPool.SampleOrDefault(rng));
		}

		// Token: 0x0600038E RID: 910 RVA: 0x000093E7 File Offset: 0x000075E7
		public override GameMap CreateMap()
		{
			return GameMap.CreateSingleRoute(base.Boss.Id, new StationType[] { StationType.Boss });
		}
	}
}
