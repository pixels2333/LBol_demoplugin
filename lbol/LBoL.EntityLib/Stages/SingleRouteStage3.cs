using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.EntityLib.Adventures.FirstPlace;
namespace LBoL.EntityLib.Stages
{
	[UsedImplicitly]
	public sealed class SingleRouteStage3 : Stage
	{
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
