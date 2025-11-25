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
	public sealed class SingleRouteStage2 : Stage
	{
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
