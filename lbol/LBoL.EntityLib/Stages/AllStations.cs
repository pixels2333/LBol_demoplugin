using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.EntityLib.Adventures;
using LBoL.EntityLib.Adventures.FirstPlace;
using LBoL.EntityLib.Cards.Neutral.Black;
using LBoL.EntityLib.Exhibits;
using LBoL.EntityLib.Exhibits.Common;
namespace LBoL.EntityLib.Stages
{
	[UsedImplicitly]
	public sealed class AllStations : Stage
	{
		public AllStations()
		{
			base.Level = 2;
			base.CardUpgradedChance = 0.5f;
			base.EnemyPoolAct1 = new UniqueRandomPool<string>(true) { { "11", 1f } };
			base.EnemyPoolAct2 = new UniqueRandomPool<string>(true) { { "14", 1f } };
			base.EnemyPoolAct3 = new UniqueRandomPool<string>(true) { { "17", 1f } };
			base.EliteEnemyPool = new UniqueRandomPool<string>(true)
			{
				{ "Nitori", 1f },
				{ "Youmu", 1f },
				{ "Kokoro", 1f }
			};
			base.BossPool = new RepeatableRandomPool<string> { { "Sakuya", 1f } };
			base.AdventurePool = new UniqueRandomPool<Type>(true) { 
			{
				typeof(DoremyPortal),
				1f
			} };
			base.SupplyAdventureType = typeof(Supply);
			base.TradeAdventureType = typeof(SumirekoGathering);
			base.SentinelExhibitType = typeof(KongZhanpinhe);
		}
		public override Card[] GetShopNormalCards()
		{
			Card[] shopNormalCards = base.GetShopNormalCards();
			shopNormalCards[0] = Library.CreateCard<HinaAttack>();
			return shopNormalCards;
		}
		private bool EnsureExhibit { get; set; } = true;
		public override Exhibit GetShopExhibit(bool shopOnly)
		{
			if (!base.GameRun.Player.HasExhibit<SunhuaiHufu>() && this.EnsureExhibit)
			{
				this.EnsureExhibit = false;
				return Library.CreateExhibit<SunhuaiHufu>();
			}
			return base.GetShopExhibit(shopOnly);
		}
		public override Exhibit GetEliteEnemyExhibit()
		{
			if (!base.GameRun.Player.HasExhibit<SunhuaiHufu>() && this.EnsureExhibit)
			{
				this.EnsureExhibit = false;
				return Library.CreateExhibit<SunhuaiHufu>();
			}
			return base.GetEliteEnemyExhibit();
		}
		public override void InitBoss(RandomGen rng)
		{
			base.Boss = Library.GetEnemyGroupEntry(base.BossPool.SampleOrDefault(rng));
		}
		public override GameMap CreateMap()
		{
			return GameMap.CreateFourRoute(base.Boss.Id, new StationType[]
			{
				StationType.Shop,
				StationType.EliteEnemy,
				StationType.Gap,
				StationType.Adventure,
				StationType.Shop,
				StationType.Supply,
				StationType.Gap,
				StationType.Enemy,
				StationType.Shop,
				StationType.EliteEnemy,
				StationType.Adventure,
				StationType.Supply,
				StationType.Shop,
				StationType.Enemy,
				StationType.Enemy,
				StationType.Enemy
			});
		}
	}
}
