using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.EntityLib.Adventures.FirstPlace;

namespace LBoL.EntityLib.Stages
{
	// Token: 0x020000FE RID: 254
	[UsedImplicitly]
	public sealed class SingleRouteStage2 : Stage
	{
		// Token: 0x06000392 RID: 914 RVA: 0x00009500 File Offset: 0x00007700
		public SingleRouteStage2()
		{
			base.Level = 2;
			base.EnemyPoolAct1 = new UniqueRandomPool<string>(true) { { "21", 1f } };
			base.EnemyPoolAct2 = new UniqueRandomPool<string>(true) { { "24", 1f } };
			base.EnemyPoolAct3 = new UniqueRandomPool<string>(true) { { "27", 1f } };
			base.EliteEnemyPool = new UniqueRandomPool<string>(true) { { "Youmu", 1f } };
			base.BossPool = new RepeatableRandomPool<string> { { "Tianzi", 1f } };
			base.FirstAdventurePool = new UniqueRandomPool<Type>(false)
			{
				{
					typeof(PatchouliPhilosophy),
					1f
				},
				{
					typeof(JunkoColorless),
					1f
				},
				{
					typeof(ShinmyoumaruForge),
					1f
				}
			};
		}

		// Token: 0x06000393 RID: 915 RVA: 0x000095F0 File Offset: 0x000077F0
		public override void InitBoss(RandomGen rng)
		{
			base.Boss = Library.GetEnemyGroupEntry(base.BossPool.SampleOrDefault(rng));
		}

		// Token: 0x06000394 RID: 916 RVA: 0x00009609 File Offset: 0x00007809
		public override GameMap CreateMap()
		{
			return GameMap.CreateSingleRoute(base.Boss.Id, new StationType[] { StationType.Boss });
		}
	}
}
