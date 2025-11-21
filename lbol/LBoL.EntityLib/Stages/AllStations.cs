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
	// Token: 0x020000F9 RID: 249
	[UsedImplicitly]
	public sealed class AllStations : Stage
	{
		// Token: 0x0600037D RID: 893 RVA: 0x00009008 File Offset: 0x00007208
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

		// Token: 0x0600037E RID: 894 RVA: 0x00009130 File Offset: 0x00007330
		public override Card[] GetShopNormalCards()
		{
			Card[] shopNormalCards = base.GetShopNormalCards();
			shopNormalCards[0] = Library.CreateCard<HinaAttack>();
			return shopNormalCards;
		}

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x0600037F RID: 895 RVA: 0x00009140 File Offset: 0x00007340
		// (set) Token: 0x06000380 RID: 896 RVA: 0x00009148 File Offset: 0x00007348
		private bool EnsureExhibit { get; set; } = true;

		// Token: 0x06000381 RID: 897 RVA: 0x00009151 File Offset: 0x00007351
		public override Exhibit GetShopExhibit(bool shopOnly)
		{
			if (!base.GameRun.Player.HasExhibit<SunhuaiHufu>() && this.EnsureExhibit)
			{
				this.EnsureExhibit = false;
				return Library.CreateExhibit<SunhuaiHufu>();
			}
			return base.GetShopExhibit(shopOnly);
		}

		// Token: 0x06000382 RID: 898 RVA: 0x00009181 File Offset: 0x00007381
		public override Exhibit GetEliteEnemyExhibit()
		{
			if (!base.GameRun.Player.HasExhibit<SunhuaiHufu>() && this.EnsureExhibit)
			{
				this.EnsureExhibit = false;
				return Library.CreateExhibit<SunhuaiHufu>();
			}
			return base.GetEliteEnemyExhibit();
		}

		// Token: 0x06000383 RID: 899 RVA: 0x000091B0 File Offset: 0x000073B0
		public override void InitBoss(RandomGen rng)
		{
			base.Boss = Library.GetEnemyGroupEntry(base.BossPool.SampleOrDefault(rng));
		}

		// Token: 0x06000384 RID: 900 RVA: 0x000091C9 File Offset: 0x000073C9
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
