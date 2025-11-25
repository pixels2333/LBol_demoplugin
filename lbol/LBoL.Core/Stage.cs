using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core.Adventures;
using LBoL.Core.Attributes;
using LBoL.Core.Cards;
using LBoL.Core.GapOptions;
using LBoL.Core.Randoms;
using LBoL.Core.SaveData;
using LBoL.Core.Stations;
using LBoL.Core.Units;
using UnityEngine;
namespace LBoL.Core
{
	[Localizable]
	public abstract class Stage : GameEntity
	{
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return (GameEventPriority)0;
			}
		}
		public StageConfig Config { get; private set; }
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<Stage>.LocalizeProperty(base.Id, key, decorated, required);
		}
		protected virtual IReadOnlyList<string> LocalizeListProperty(string key, bool required = true)
		{
			return TypeFactory<Stage>.LocalizeListProperty(base.Id, key, required);
		}
		public int Index { get; internal set; }
		public ulong MapSeed { get; internal set; }
		public int Level { get; protected internal set; }
		public override string Name
		{
			get
			{
				return this.LocalizeProperty("Name", false, true);
			}
		}
		public bool IsSelectingBoss { get; protected set; }
		public string SelectedBoss { get; private set; }
		public Type SentinelExhibitType
		{
			[return: MaybeNull]
			get;
			protected set; }
		public Type DebutAdventureType { get; internal set; }
		public Type SupplyAdventureType { get; protected set; }
		public Type TradeAdventureType { get; protected set; }
		public bool IsNormalFinalStage { get; internal set; }
		public bool IsTrueEndFinalStage { get; internal set; }
		public bool EternalStageMusic { get; protected set; }
		public bool DontAutoOpenMapInEntry { get; protected set; }
		public bool EnterWithSpecialPresentation { get; protected set; }
		public EnemyGroupEntry Boss { get; protected set; }
		public RepeatableRandomPool<string> BossPool { get; set; } = new RepeatableRandomPool<string>();
		public Type FirstAdventure { get; protected set; }
		public UniqueRandomPool<Type> FirstAdventurePool { get; set; } = new UniqueRandomPool<Type>(false);
		public UniqueRandomPool<Type> AdventurePool { get; set; } = new UniqueRandomPool<Type>(false);
		public UniqueRandomPool<string> EnemyPoolAct1 { get; set; } = new UniqueRandomPool<string>(true);
		public UniqueRandomPool<string> EnemyPoolAct2 { get; set; } = new UniqueRandomPool<string>(true);
		public UniqueRandomPool<string> EnemyPoolAct3 { get; set; } = new UniqueRandomPool<string>(true);
		public UniqueRandomPool<string> EliteEnemyPool { get; set; } = new UniqueRandomPool<string>(true);
		public List<Type> AdventureHistory { get; } = new List<Type>();
		public HashSet<string> ExtraFlags { get; internal set; } = new HashSet<string>();
		protected ExhibitWeightTable ShopExhibitWeightTable { get; set; } = new ExhibitWeightTable(new RarityWeightTable(0.5f, 0.33f, 0.17f, 0f), new AppearanceWeightTable(1f, 2f, 0f, 0f));
		protected ExhibitWeightTable ShopExhibitWeightTableShopOnly { get; set; } = new ExhibitWeightTable(new RarityWeightTable(0.5f, 0.33f, 0.17f, 0f), AppearanceWeightTable.OnlyShopOnly);
		protected ExhibitWeightTable EliteEnemyExhibitWeightTable { get; set; } = new ExhibitWeightTable(new RarityWeightTable(0.5f, 0.33f, 0.17f, 0f), new AppearanceWeightTable(1f, 0f, 1f, 0f));
		protected ExhibitWeightTable SupplyExhibitWeightTable { get; set; } = new ExhibitWeightTable(new RarityWeightTable(0.3f, 0.5f, 0.2f, 0f), new AppearanceWeightTable(1f, 0f, 1f, 0f));
		protected CardWeightTable DrinkTeaAdditionalCardWeight { get; set; } = new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.Hierarchy, CardTypeWeightTable.CanBeLoot, false);
		public float CardUpgradedChance { get; protected set; }
		protected CardWeightTable EnemyCardWeight { get; set; } = CardWeightTable.WithoutTool;
		protected CardWeightTable EnemyCardOnlyPlayerWeight { get; set; } = CardWeightTable.WithoutTool;
		protected CardWeightTable EnemyCardWithFriendWeight { get; set; } = CardWeightTable.WithoutTool;
		protected CardWeightTable EnemyCardNeutralWeight { get; set; } = CardWeightTable.WithoutTool;
		protected CardWeightTable EliteEnemyCardWeight { get; set; } = CardWeightTable.WithoutTool;
		protected CardWeightTable EliteEnemyCardCharaWeight { get; set; } = CardWeightTable.WithoutTool;
		protected CardWeightTable EliteEnemyCardFriendWeight { get; set; } = CardWeightTable.WithoutTool;
		protected CardWeightTable EliteEnemyCardNeutralWeight { get; set; } = CardWeightTable.WithoutTool;
		protected CardWeightTable BossCardWeight { get; set; } = CardWeightTable.WithoutTool;
		protected CardWeightTable BossCardCharaWeight { get; set; } = CardWeightTable.WithoutTool;
		protected CardWeightTable BossCardFriendWeight { get; set; } = CardWeightTable.WithoutTool;
		protected CardWeightTable BossCardNeutralWeight { get; set; } = CardWeightTable.WithoutTool;
		protected CardWeightTable ShopNormalAtkWeight { get; set; } = CardWeightTable.ShopAtk;
		protected CardWeightTable ShopNormalDefWeight { get; set; } = CardWeightTable.ShopDef;
		protected CardWeightTable ShopNormalSklWeight { get; set; } = CardWeightTable.ShopSkl;
		protected CardWeightTable ShopNormalAblWeight { get; set; } = CardWeightTable.ShopAbl;
		protected CardWeightTable ShopSkillAndFriendWeight { get; set; } = CardWeightTable.ShopSkillAndFriend;
		protected CardWeightTable ShopToolCardWeight { get; set; } = CardWeightTable.OnlyTool;
		public override void Initialize()
		{
			base.Initialize();
			this.Config = StageConfig.FromId(base.Id);
			if (this.Config == null)
			{
				throw new InvalidDataException("Cannot find stage config for <" + base.Id + ">");
			}
		}
		public Stage AsNormalFinal()
		{
			this.IsNormalFinalStage = true;
			return this;
		}
		public Stage AsTrueEndFinal()
		{
			this.IsTrueEndFinalStage = true;
			return this;
		}
		internal void Enter()
		{
			this.OnEnter();
		}
		protected void OnEnter()
		{
		}
		public virtual void InitExtraFlags(ProfileSaveData userProfile)
		{
		}
		public virtual void InitBoss(RandomGen rng)
		{
			string text = this.BossPool.SampleOrDefault(rng);
			if (text != null)
			{
				this.Boss = Library.GetEnemyGroupEntry(text);
				return;
			}
			if (!this.IsSelectingBoss)
			{
				throw new InvalidOperationException("Cannot generate non-boss-selecting stage '" + base.GetType().Name + "' map with empty boss pool");
			}
		}
		public void InitFirstAdventure(RandomGen initBossRng)
		{
			this.FirstAdventure = (this.FirstAdventurePool.IsEmpty ? null : this.FirstAdventurePool.Sample(initBossRng));
		}
		public virtual GameMap CreateMap()
		{
			RandomGen randomGen = new RandomGen(this.MapSeed);
			EnemyGroupEntry boss = this.Boss;
			return GameMap.CreateNormalMap(randomGen, (boss != null) ? boss.Id : null, this.IsSelectingBoss);
		}
		public virtual void SetBoss(string enemyGroupName)
		{
			if (this.Boss != null)
			{
				throw new InvalidOperationException(string.Concat(new string[]
				{
					this.DebugName,
					" already has boss '",
					this.Boss.Id,
					"', tried setting to '",
					enemyGroupName,
					"'"
				}));
			}
			this.Boss = Library.GetEnemyGroupEntry(enemyGroupName);
			this.SelectedBoss = enemyGroupName;
		}
		public Station CreateStation(MapNode node)
		{
			return this.CreateStationFromType(node, node.StationType);
		}
		internal Station CreateStationFromType(MapNode node, StationType type)
		{
			Station station = Station.Create(type);
			BossStation bossStation = station as BossStation;
			if (bossStation != null)
			{
				if (this.Boss == null)
				{
					Debug.LogError("Stage has no boss.");
				}
				else
				{
					bossStation.BossId = this.Boss.Id;
				}
			}
			station.GameRun = base.GameRun;
			station.Stage = this;
			station.Level = node.X;
			station.Act = node.Act;
			if (node.FollowerList.Empty<int>())
			{
				station.IsStageEnd = true;
				if (this.IsTrueEndFinalStage)
				{
					station.IsTrueEnd = true;
				}
				else if (this.IsNormalFinalStage)
				{
					station.IsNormalEnd = true;
				}
			}
			return station;
		}
		public virtual Type GetAdventure()
		{
			UniqueRandomPool<Type> uniqueRandomPool = new UniqueRandomPool<Type>(false);
			foreach (RandomPoolEntry<Type> randomPoolEntry in Enumerable.ToList<RandomPoolEntry<Type>>(this.AdventurePool))
			{
				Type type;
				float num;
				randomPoolEntry.Deconstruct(out type, out num);
				Type type2 = type;
				float num2 = num;
				if (base.GameRun.AdventureHistory.Contains(type2))
				{
					Debug.Log("[Stage: " + this.DebugName + "] Removing duplicated adventure " + type2.Name);
					this.AdventurePool.Remove(type2, true);
				}
				else
				{
					uniqueRandomPool.Add(type2, Library.WeightForAdventure(type2, base.GameRun) * num2);
				}
			}
			if (uniqueRandomPool.IsEmpty)
			{
				return typeof(FakeAdventure);
			}
			Type type3 = uniqueRandomPool.Sample(base.GameRun.StationRng);
			this.AdventurePool.Remove(type3, true);
			return type3;
		}
		public virtual Card[] GetShopNormalCards()
		{
			List<Card> list = new List<Card>();
			list.AddRange(base.GameRun.GetShopCards(2, this.ShopNormalAtkWeight, null));
			list.AddRange(base.GameRun.GetShopCards(2, this.ShopNormalDefWeight, null));
			Card card = Enumerable.First<Card>(base.GameRun.GetShopCards(1, this.ShopNormalSklWeight, null));
			list.Add(card);
			GameRunController gameRun = base.GameRun;
			int num = 1;
			CardWeightTable shopSkillAndFriendWeight = this.ShopSkillAndFriendWeight;
			List<string> list2 = new List<string>();
			list2.Add(card.Id);
			list.AddRange(gameRun.GetShopCards(num, shopSkillAndFriendWeight, list2));
			list.AddRange(base.GameRun.GetShopCards(2, this.ShopNormalAblWeight, null));
			return list.ToArray();
		}
		public virtual Card[] GetShopToolCards(int count)
		{
			return base.GameRun.GetShopCards(count, this.ShopToolCardWeight, null);
		}
		public virtual Card SupplyShopCard(Card justBought, List<string> nowCardIds)
		{
			switch (justBought.CardType)
			{
			case CardType.Attack:
				return base.GameRun.GetShopCards(1, this.ShopNormalAtkWeight, nowCardIds)[0];
			case CardType.Defense:
				return base.GameRun.GetShopCards(1, this.ShopNormalDefWeight, nowCardIds)[0];
			case CardType.Skill:
				return base.GameRun.GetShopCards(1, this.ShopSkillAndFriendWeight, nowCardIds)[0];
			case CardType.Ability:
				return base.GameRun.GetShopCards(1, this.ShopNormalAblWeight, nowCardIds)[0];
			case CardType.Friend:
				return base.GameRun.GetShopCards(1, this.ShopSkillAndFriendWeight, nowCardIds)[0];
			case CardType.Tool:
				return base.GameRun.GetShopCards(1, this.ShopToolCardWeight, nowCardIds)[0];
			default:
				throw new InvalidOperationException(string.Format("Bought a card:{0} with wrong card type:{1}.", justBought.DebugName, justBought.CardType));
			}
		}
		public virtual Exhibit GetShopExhibit(bool shopOnly)
		{
			return base.GameRun.RollNormalExhibit(base.GameRun.ShopRng, shopOnly ? this.ShopExhibitWeightTableShopOnly : this.ShopExhibitWeightTable, new Func<Exhibit>(this.GetSentinelExhibit), null);
		}
		public virtual Exhibit GetEliteEnemyExhibit()
		{
			return base.GameRun.RollNormalExhibit(base.GameRun.ExhibitRng, this.EliteEnemyExhibitWeightTable, new Func<Exhibit>(this.GetSentinelExhibit), null);
		}
		public virtual Exhibit GetSupplyExhibit()
		{
			return base.GameRun.RollNormalExhibit(base.GameRun.ExhibitRng, this.SupplyExhibitWeightTable, new Func<Exhibit>(this.GetSentinelExhibit), null);
		}
		public Exhibit GetSpecialAdventureExhibit()
		{
			return base.GameRun.RollNormalExhibit(base.GameRun.ExhibitRng, new ExhibitWeightTable(new RarityWeightTable(0.5f, 0.33f, 0.17f, 0f), new AppearanceWeightTable(1f, 1f, 1f, 0f)), new Func<Exhibit>(this.GetSentinelExhibit), null);
		}
		public virtual Exhibit GetNeutralShiningExhibit()
		{
			return base.GameRun.RollShiningExhibit(base.GameRun.ShinningExhibitRng, new Func<Exhibit>(this.GetSentinelExhibit), (ExhibitConfig config) => string.IsNullOrWhiteSpace(config.Owner));
		}
		public virtual Exhibit RollExhibitInAdventure(ExhibitWeightTable weightTable, [MaybeNull] Predicate<ExhibitConfig> filter = null)
		{
			return base.GameRun.RollNormalExhibit(base.GameRun.ExhibitRng, weightTable, new Func<Exhibit>(this.GetSentinelExhibit), filter);
		}
		[return: MaybeNull]
		internal Exhibit GetSentinelExhibit()
		{
			if (this.SentinelExhibitType == null)
			{
				throw new InvalidOperationException("[Stage: " + this.DebugName + "] Has no sentinal exhibit");
			}
			Exhibit exhibit = Library.CreateExhibit(this.SentinelExhibitType);
			if (!exhibit.Config.IsSentinel)
			{
				Debug.LogError(string.Concat(new string[] { "[Stage: ", this.DebugName, "] Sentinal exhibit ", exhibit.DebugName, " is not sentinal in config" }));
			}
			return exhibit;
		}
		public virtual Exhibit[] GetBossExhibits()
		{
			if (this.Boss == null)
			{
				Debug.LogError("Stage has no boss, thus cannot get boss exhibits");
				return Array.Empty<Exhibit>();
			}
			return base.GameRun.RollBossExhibits(base.GameRun.ShinningExhibitRng, this.Boss.Id, this.Boss.RollBossExhibit, new Func<Exhibit>(this.<GetBossExhibits>g__FallbackShining|229_0));
		}
		public virtual StationReward GetDrinkTeaCardReward(DrinkTea drinkTea)
		{
			return StationReward.CreateCards(base.GameRun.GetRewardCards(this.EnemyCardOnlyPlayerWeight, this.EnemyCardWithFriendWeight, this.EnemyCardNeutralWeight, this.DrinkTeaAdditionalCardWeight, drinkTea.CardCount, false));
		}
		public virtual StationReward GetEnemyCardReward()
		{
			return StationReward.CreateCards(base.GameRun.GetRewardCards(this.EnemyCardOnlyPlayerWeight, this.EnemyCardWithFriendWeight, this.EnemyCardNeutralWeight, this.EnemyCardWeight, base.GameRun.RewardCardCount, false));
		}
		public virtual StationReward GetEliteEnemyCardReward()
		{
			return StationReward.CreateCards(base.GameRun.GetRewardCards(this.EliteEnemyCardCharaWeight, this.EliteEnemyCardFriendWeight, this.EliteEnemyCardNeutralWeight, this.EliteEnemyCardWeight, base.GameRun.RewardCardCount, false));
		}
		public virtual StationReward GetBossCardReward()
		{
			return StationReward.CreateCards(base.GameRun.GetRewardCards(this.BossCardCharaWeight, this.BossCardFriendWeight, this.BossCardNeutralWeight, this.BossCardWeight, base.GameRun.RewardCardCount, true));
		}
		public virtual EnemyGroupEntry GetEnemies(Station station)
		{
			string text;
			switch (station.Act)
			{
			case 1:
				text = this.EnemyPoolAct1.Sample(base.GameRun.StationRng);
				break;
			case 2:
				text = this.EnemyPoolAct2.Sample(base.GameRun.StationRng);
				break;
			case 3:
				text = this.EnemyPoolAct3.Sample(base.GameRun.StationRng);
				break;
			default:
				throw new ArgumentOutOfRangeException("Act");
			}
			return Library.GetEnemyGroupEntry(text);
		}
		public virtual EnemyGroupEntry GetEliteEnemies(Station station)
		{
			return Library.GetEnemyGroupEntry(this.EliteEnemyPool.Sample(base.GameRun.StationRng));
		}
		public virtual EnemyGroupEntry GetBoss()
		{
			return this.Boss;
		}
		[CompilerGenerated]
		private Exhibit <GetBossExhibits>g__FallbackShining|229_0()
		{
			return base.GameRun.RollShiningExhibit(base.GameRun.ExhibitRng, new Func<Exhibit>(this.<GetBossExhibits>g__FallbackNormal|229_1), (ExhibitConfig config) => string.IsNullOrWhiteSpace(config.Owner) && config.BaseManaColor == null);
		}
		[CompilerGenerated]
		private Exhibit <GetBossExhibits>g__FallbackNormal|229_1()
		{
			return base.GameRun.RollNormalExhibit(base.GameRun.ExhibitRng, ExhibitWeightTable.AllOnes, new Func<Exhibit>(this.GetSentinelExhibit), (ExhibitConfig config) => string.IsNullOrWhiteSpace(config.Owner));
		}
		public bool isNormalStage;
	}
}
