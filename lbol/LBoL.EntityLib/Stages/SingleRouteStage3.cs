using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.EntityLib.Adventures.FirstPlace;

namespace LBoL.EntityLib.Stages
{
	// Token: 0x020000FF RID: 255
	[UsedImplicitly]
	public sealed class SingleRouteStage3 : Stage
	{
		// Token: 0x06000395 RID: 917 RVA: 0x00009628 File Offset: 0x00007828
		public SingleRouteStage3()
		{
			base.Level = 3;
			base.EnemyPoolAct1 = new UniqueRandomPool<string>(true) { { "31", 1f } };
			base.EnemyPoolAct2 = new UniqueRandomPool<string>(true) { { "34", 1f } };
			base.EnemyPoolAct3 = new UniqueRandomPool<string>(true) { { "37", 1f } };
			base.EliteEnemyPool = new UniqueRandomPool<string>(true) { { "Clownpiece", 1f } };
			base.BossPool = new RepeatableRandomPool<string> { { "Sanae", 1f } };
			base.FirstAdventurePool = new UniqueRandomPool<Type>(false)
			{
				{
					typeof(MiyoiBartender),
					1f
				},
				{
					typeof(WatatsukiPurify),
					1f
				},
				{
					typeof(DoremyPortal),
					1f
				}
			};
		}

		// Token: 0x06000396 RID: 918 RVA: 0x00009718 File Offset: 0x00007918
		public override void InitBoss(RandomGen rng)
		{
			base.Boss = Library.GetEnemyGroupEntry(base.BossPool.SampleOrDefault(rng));
		}

		// Token: 0x06000397 RID: 919 RVA: 0x00009731 File Offset: 0x00007931
		public override GameMap CreateMap()
		{
			return GameMap.CreateSingleRoute(base.Boss.Id, new StationType[] { StationType.Boss });
		}
	}
}
