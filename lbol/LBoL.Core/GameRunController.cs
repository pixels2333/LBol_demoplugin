using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core.Adventures;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Dialogs;
using LBoL.Core.JadeBoxes;
using LBoL.Core.Randoms;
using LBoL.Core.SaveData;
using LBoL.Core.Stations;
using LBoL.Core.Stats;
using LBoL.Core.Units;
using UnityEngine;
using Yarn;
namespace LBoL.Core
{
	public sealed class GameRunController
	{
		public static GameRunController Create(GameRunStartupParameters parameters)
		{
			return new GameRunController(parameters);
		}
		private GameRunController()
		{
			this.DialogLibrary = new DialogFunctions(this).ToLibrary();
		}
		private GameRunController(GameRunStartupParameters startupParameters)
			: this()
		{
			this.Stats = new GameRunStats();
			this.HasClearBonus = startupParameters.UserProfile.HasClearBonus;
			this.UnlockLevel = startupParameters.UnlockLevel;
			PlayerUnit player = startupParameters.Player;
			Exhibit initExhibit = startupParameters.InitExhibit;
			this.Mode = startupParameters.Mode;
			this.ShowRandomResult = startupParameters.ShowRandomResult;
			this.Difficulty = startupParameters.Difficulty;
			this.Puzzles = startupParameters.Puzzles;
			this._stages = new List<Stage>(startupParameters.Stages);
			if (this._stages.Empty<Stage>())
			{
				throw new ArgumentException("Cannot create game-run with empty stages");
			}
			foreach (ValueTuple<int, Stage> valueTuple in this._stages.WithIndices<Stage>())
			{
				int item = valueTuple.Item1;
				Stage item2 = valueTuple.Item2;
				item2.GameRun = this;
				item2.Index = item;
				item2.InitExtraFlags(startupParameters.UserProfile);
			}
			ulong? seed = startupParameters.Seed;
			if (seed != null)
			{
				ulong valueOrDefault = seed.GetValueOrDefault();
				this.RootSeed = valueOrDefault;
				this.IsAutoSeed = false;
				this.HasClearBonus = true;
			}
			else
			{
				this.RootSeed = RandomGen.GetRandomSeed();
				this.IsAutoSeed = true;
			}
			this.RootRng = new RandomGen(this.RootSeed);
			foreach (Stage stage in this._stages)
			{
				stage.MapSeed = this.RootRng.NextULong();
			}
			this._stages[0].DebutAdventureType = startupParameters.DebutAdventureType;
			this.StationRng = new RandomGen(this.RootRng.NextULong());
			this.InitBossSeed = this.RootRng.NextULong();
			this.ShopRng = new RandomGen(this.RootRng.NextULong());
			this.AdventureRng = new RandomGen(this.RootRng.NextULong());
			this.ExhibitRng = new RandomGen(this.RootRng.NextULong());
			this.ShinningExhibitRng = new RandomGen(this.RootRng.NextULong());
			this.CardRng = new RandomGen(this.RootRng.NextULong());
			this.GameRunEventRng = new RandomGen(this.RootRng.NextULong());
			this.BattleRng = new RandomGen(this.RootRng.NextULong());
			this.BattleCardRng = new RandomGen(this.RootRng.NextULong());
			this.ShuffleRng = new RandomGen(this.RootRng.NextULong());
			this.EnemyMoveRng = new RandomGen(this.RootRng.NextULong());
			this.EnemyBattleRng = new RandomGen(this.RootRng.NextULong());
			this.DebutRng = new RandomGen(this.RootRng.NextULong());
			this.FinalBossSeed = this.RootRng.NextULong();
			this.UISeed = this.RootRng.NextULong();
			RandomGen randomGen = new RandomGen(this.InitBossSeed);
			foreach (Stage stage2 in this._stages)
			{
				stage2.InitBoss(randomGen);
			}
			foreach (Stage stage3 in this._stages)
			{
				stage3.InitFirstAdventure(randomGen);
			}
			this.Player = player;
			this.PlayerType = startupParameters.PlayerType;
			player.Us.GameRun = this;
			this._baseDeck = new List<Card>(startupParameters.Deck);
			foreach (Card card in this._baseDeck)
			{
				card.IsGamerunInitial = true;
			}
			if (this.Puzzles.HasFlag(PuzzleFlag.StartMisfortune))
			{
				this._baseDeck.Add(Library.CreateCard<Zhukeling>());
			}
			foreach (Card card2 in this._baseDeck)
			{
				int num = this._deckCardInstanceId + 1;
				this._deckCardInstanceId = num;
				card2.InstanceId = num;
			}
			this.BaseMana = player.Config.InitialMana;
			this.Money = startupParameters.InitMoneyOverride ?? player.Config.InitialMoney;
			this.TotalMoney = this.Money;
			this.ExhibitPool = new List<Type>();
			this.ShiningExhibitPool = new List<Type>();
			foreach (ValueTuple<Type, ExhibitConfig> valueTuple2 in Library.EnumerateRollableExhibitTypes(startupParameters.UnlockLevel))
			{
				Type item3 = valueTuple2.Item1;
				ExhibitConfig item4 = valueTuple2.Item2;
				if (item4.Rarity == Rarity.Shining && item4.Id != initExhibit.Id)
				{
					this.ShiningExhibitPool.Add(item3);
				}
				else if (item4.IsPooled)
				{
					this.ExhibitPool.Add(item3);
				}
			}
			this._cardRewardWeightFactors = new Dictionary<string, float>();
			this._cardRareWeightFactor = 0.85f;
			foreach (Card card3 in this._baseDeck)
			{
				card3.GameRun = this;
			}
			player.EnterGameRun(this);
			if (this.Puzzles.HasFlag(PuzzleFlag.LowMaxHp))
			{
				int num2 = Math.Max(1, player.MaxHp - 10);
				player.SetMaxHp(num2, num2);
			}
			this.GainExhibitInstantly(initExhibit, false, null);
			HashSet<string> hashSet = new HashSet<string>();
			foreach (JadeBox jadeBox in startupParameters.JadeBoxes)
			{
				foreach (string text in jadeBox.Config.Group)
				{
					if (hashSet.Contains(text))
					{
						throw new InvalidOperationException("Cannot gain jade-box " + jadeBox.DebugName + ": already has group " + text);
					}
					hashSet.Add(text);
				}
				this.GainJadeBox(jadeBox);
			}
			if (this.HasJadeBox<TwoColorStart>())
			{
				Exhibit exhibit = Library.CreateExhibit((this.PlayerType == PlayerType.TypeA) ? player.Config.ExhibitB : player.Config.ExhibitA);
				this.GainExhibitInstantly(exhibit, false, null);
				this.ShiningExhibitPool.Remove(exhibit.GetType());
				this.GainBaseMana(ManaGroup.Single(ManaColor.Philosophy), false);
				this.LoseBaseMana(initExhibit.BaseMana, false);
				this.LoseBaseMana(exhibit.BaseMana, false);
			}
			if (this.Difficulty == GameDifficulty.Easy || this._jadeBoxes.NotEmpty<JadeBox>())
			{
				CharacterStatsSaveData characterStatsSaveData = Enumerable.FirstOrDefault<CharacterStatsSaveData>(startupParameters.UserProfile.CharacterStats, (CharacterStatsSaveData s) => s.CharacterId == player.Id);
				if (characterStatsSaveData != null && characterStatsSaveData.HighestPerfectSuccessDifficulty != null)
				{
					this.ExtraFlags.Add("DebutYuzhi");
				}
			}
		}
		public InteractionViewer InteractionViewer { get; } = new InteractionViewer();
		public IGameRunAchievementHandler AchievementHandler { get; set; }
		public event Action<Card> CardRevealed;
		public event Action<Exhibit> ExhibitRevealed;
		public event Action<EnemyGroup> EnemyGroupRevealed;
		internal void RevealCard(Card card)
		{
			Action<Card> cardRevealed = this.CardRevealed;
			if (cardRevealed == null)
			{
				return;
			}
			cardRevealed.Invoke(card);
		}
		public void RevealExhibit(Exhibit exhibit)
		{
			Action<Exhibit> exhibitRevealed = this.ExhibitRevealed;
			if (exhibitRevealed == null)
			{
				return;
			}
			exhibitRevealed.Invoke(exhibit);
		}
		internal void RevealEnemyGroup(EnemyGroup enemyGroup)
		{
			Action<EnemyGroup> enemyGroupRevealed = this.EnemyGroupRevealed;
			if (enemyGroupRevealed == null)
			{
				return;
			}
			enemyGroupRevealed.Invoke(enemyGroup);
		}
		public IGameRunVisualTrigger VisualTrigger { get; set; }
		public GameMode Mode { get; private set; }
		public GameDifficulty Difficulty { get; private set; }
		public bool KaguyaInDebut
		{
			get
			{
				return this.Difficulty == GameDifficulty.Easy || this.HasJadeBox<StartWithMythic>();
			}
		}
		public PuzzleFlag Puzzles { get; private set; }
		public GameRunStatus Status { get; internal set; }
		public bool IsNormalEndFinished { get; private set; }
		public ulong RootSeed { get; private set; }
		public bool IsAutoSeed { get; private set; }
		public RandomGen RootRng { get; private set; }
		public RandomGen StationRng { get; private set; }
		public ulong InitBossSeed { get; private set; }
		public RandomGen ShopRng { get; private set; }
		public RandomGen AdventureRng { get; private set; }
		public RandomGen ExhibitRng { get; private set; }
		public RandomGen ShinningExhibitRng { get; private set; }
		public RandomGen CardRng { get; private set; }
		public RandomGen GameRunEventRng { get; private set; }
		public RandomGen BattleRng { get; private set; }
		public RandomGen BattleCardRng { get; private set; }
		public RandomGen ShuffleRng { get; private set; }
		public RandomGen EnemyMoveRng { get; private set; }
		public RandomGen EnemyBattleRng { get; private set; }
		public RandomGen DebutRng { get; private set; }
		public ulong FinalBossSeed { get; private set; }
		public ulong UISeed { get; private set; }
		public int CardValidDebugLevel { get; private set; }
		public Library DialogLibrary { get; }
		public bool HasClearBonus { get; private set; }
		public int UnlockLevel { get; private set; }
		public PlayerUnit Player { get; private set; }
		public PlayerType PlayerType { get; private set; }
		public ManaGroup BaseMana { get; private set; }
		public IReadOnlyList<Card> BaseDeck
		{
			get
			{
				return this._baseDeck.AsReadOnly();
			}
		}
		public IEnumerable<Card> BaseDeckWithoutUnremovable
		{
			get
			{
				return Enumerable.Where<Card>(this._baseDeck, (Card card) => !card.Unremovable);
			}
		}
		public IEnumerable<Card> BaseDeckInBossRemoveReward
		{
			get
			{
				return Enumerable.Where<Card>(this._baseDeck, (Card card) => !card.IsBasic && !card.Negative);
			}
		}
		public int Money { get; private set; }
		public int TotalMoney { get; private set; }
		public int UltimateUseCount { get; set; }
		public int ReloadTimes { get; private set; }
		public bool ShowRandomResult { get; private set; }
		public BattleController Battle { get; private set; }
		public IReadOnlyList<Stage> Stages
		{
			get
			{
				return this._stages.AsReadOnly();
			}
		}
		public IReadOnlyList<JadeBox> JadeBoxes
		{
			get
			{
				return this._jadeBoxes.AsReadOnly();
			}
		}
		public bool HasJadeBox(string id)
		{
			Type type = TypeFactory<Exhibit>.GetType(id);
			if (type != null)
			{
				return this.HasJadeBox(type);
			}
			Debug.LogError("Cannot find jadeBox type '" + id + "'");
			return false;
		}
		public bool HasJadeBox(Type type)
		{
			return Enumerable.Any<JadeBox>(this._jadeBoxes, (JadeBox jadeBox) => jadeBox.GetType() == type);
		}
		public bool HasJadeBox<T>() where T : JadeBox
		{
			return this.HasJadeBox(typeof(T));
		}
		public bool HasJadeBox(JadeBox jadeBox)
		{
			return this._jadeBoxes.Contains(jadeBox);
		}
		public int PlayedSeconds { get; private set; }
		public GameRunStats Stats { get; private set; }
		public HashSet<string> ExtraFlags { get; private set; } = new HashSet<string>();
		public List<StageRecord> StageRecords { get; private set; } = new List<StageRecord>();
		public Stage CurrentStage { get; private set; }
		public GameMap CurrentMap { get; private set; }
		public GameRunMapMode MapMode
		{
			get
			{
				return this._mapMode;
			}
		}
		private void CheckMapMode()
		{
			this._activeMapModeOverrider = null;
			this._mapMode = GameRunMapMode.Normal;
			foreach (IMapModeOverrider mapModeOverrider in this._mapModeOverriders)
			{
				GameRunMapMode? mapMode = mapModeOverrider.MapMode;
				if (mapMode != null)
				{
					GameRunMapMode valueOrDefault = mapMode.GetValueOrDefault();
					if (valueOrDefault.CompareTo(this._mapMode) > 0)
					{
						this._activeMapModeOverrider = mapModeOverrider;
						this._mapMode = valueOrDefault;
					}
				}
			}
			Station currentStation = this.CurrentStation;
			if (currentStation != null && currentStation.Status == StationStatus.Finished)
			{
				this.CurrentMap.SetAdjacentNodesStatus(this._mapMode);
			}
		}
		public void AddMapModeOverrider(IMapModeOverrider overrider)
		{
			GameRunMapMode? mapMode = overrider.MapMode;
			GameRunMapMode gameRunMapMode = GameRunMapMode.TeleportBoss;
			if ((mapMode.GetValueOrDefault() == gameRunMapMode) & (mapMode != null))
			{
				Stage currentStage = this.CurrentStage;
				if (currentStage.IsSelectingBoss && currentStage.SelectedBoss == null)
				{
					throw new InvalidOperationException("Current stage " + currentStage.Id + " boss is not selected, cannot teleport");
				}
			}
			if (this._mapModeOverriders.Contains(overrider))
			{
				throw new InvalidOperationException(string.Format("MapModeOverrider {0} already in overrider list", overrider));
			}
			this._mapModeOverriders.Add(overrider);
			this.CheckMapMode();
		}
		public void RemoveMapModeOverrider(IMapModeOverrider overrider)
		{
			if (!this._mapModeOverriders.Remove(overrider))
			{
				throw new InvalidOperationException(string.Format("Removing MapModeOverrider {0} not in overrider list", overrider));
			}
			if (overrider == this._activeMapModeOverrider)
			{
				this.CheckMapMode();
			}
		}
		public Station CurrentStation { get; private set; }
		public int DrinkTeaHealRate
		{
			get
			{
				return 30;
			}
		}
		public int DrinkTeaAdditionalHeal { get; set; }
		public int DrinkTeaAdditionalEnergy { get; set; }
		public int DrinkTeaCardRewardFlag { get; set; }
		public int DrinkTeaHealValue
		{
			get
			{
				return ((float)this.Player.MaxHp * ((float)this.DrinkTeaHealRate / 100f)).RoundToInt();
			}
		}
		public float PowerGainRate
		{
			get
			{
				return 1f;
			}
		}
		public int RewardAndShopCardColorLimitFlag { get; set; }
		public int AdditionalRewardCardCount { get; set; }
		public int WanbaochuiFlag { get; set; }
		public int BasicCardIncrease { get; set; }
		public int YichuiPiaoFlag { get; set; }
		public int AllCharacterCardsFlag { get; set; }
		public int CanViewDrawZoneActualOrder { get; set; }
		public int RemoveBadCardForbidden { get; set; }
		public int LootCardCommonDupeCount { get; set; }
		public int LootCardUncommonDupeCount { get; set; }
		public int RewardCardCount
		{
			get
			{
				return (3 + this.AdditionalRewardCardCount).Clamp(0, 5);
			}
		}
		public int RewardCardAbandonMoney { get; set; } = 5;
		public float RewardMoneyMultiplier { get; set; } = 1f;
		public int ExtraPowerLowerbound { get; set; }
		public int ExtraPowerUpperbound { get; set; }
		public int UpgradeNewDeckAttackCardFlag { get; set; }
		public int UpgradeNewDeckDefenseCardFlag { get; set; }
		public int UpgradeNewDeckSkillCardFlag { get; set; }
		public int UpgradeNewDeckAbilityCardFlag { get; set; }
		public int FragilExtraPercentage { get; set; }
		public int WeakExtraPercentage { get; set; }
		public int EnemyVulnerableExtraPercentage { get; set; }
		public int PlayerVulnerableExtraPercentage { get; set; }
		public int BasicAttackCardExtraDamage1 { get; set; }
		public int BasicAttackCardExtraDamage2 { get; set; }
		public int FinalBossInitialDamage { get; set; }
		public Exhibit ExtraExhibitReward { get; set; }
		public int ExtraLoyalty { get; set; }
		public HashSet<GameEntity> TrueEndingBlockers { get; } = new HashSet<GameEntity>();
		public HashSet<GameEntity> TrueEndingProviders { get; } = new HashSet<GameEntity>();
		public bool CanEnterTrueEnding()
		{
			return this.TrueEndingBlockers.Empty<GameEntity>() && !this.TrueEndingProviders.Empty<GameEntity>();
		}
		public bool IsTrueEndingBlocked()
		{
			return !this.TrueEndingProviders.Empty<GameEntity>() && !this.TrueEndingBlockers.Empty<GameEntity>();
		}
		public void UpgradeNewDeckCardOnFlags(IEnumerable<Card> cards)
		{
			foreach (Card card in cards)
			{
				this.UpgradeNewDeckCardOnFlags(card);
			}
		}
		public void UpgradeNewDeckCardOnFlags(Card card)
		{
			if (card.IsUpgraded || !card.CanUpgrade)
			{
				return;
			}
			switch (card.CardType)
			{
			case CardType.Unknown:
			case CardType.Friend:
			case CardType.Tool:
			case CardType.Status:
			case CardType.Misfortune:
				break;
			case CardType.Attack:
				if (this.UpgradeNewDeckAttackCardFlag > 0)
				{
					card.Upgrade();
					return;
				}
				break;
			case CardType.Defense:
				if (this.UpgradeNewDeckDefenseCardFlag > 0)
				{
					card.Upgrade();
					return;
				}
				break;
			case CardType.Skill:
				if (this.UpgradeNewDeckSkillCardFlag > 0)
				{
					card.Upgrade();
					return;
				}
				break;
			case CardType.Ability:
				if (this.UpgradeNewDeckAbilityCardFlag > 0)
				{
					card.Upgrade();
					return;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		public int ModifyMoneyReward(int money)
		{
			return ((float)money * this.RewardMoneyMultiplier).RoundToInt();
		}
		public int ModifyMoneyReward(float money)
		{
			return (money * this.RewardMoneyMultiplier).RoundToInt();
		}
		public float ShopPriceMultiplier { get; set; } = 1f;
		public float FinalShopPriceMultiplier
		{
			get
			{
				if (!this.Puzzles.HasFlag(PuzzleFlag.LowUpgradeRate))
				{
					return this.ShopPriceMultiplier;
				}
				return this.ShopPriceMultiplier * 1.1f;
			}
		}
		public int ShopResupplyFlag { get; set; }
		public int ShopRemoveCardCounter { get; set; }
		public int? ShopRemoveCardPriceOverride { get; set; }
		public int UpgradeDeckCardPrice
		{
			get
			{
				return 50;
			}
		}
		public int RemoveDeckCardPrice
		{
			get
			{
				int? shopRemoveCardPriceOverride = this.ShopRemoveCardPriceOverride;
				if (shopRemoveCardPriceOverride == null)
				{
					return 50 + this.ShopRemoveCardCounter * 25;
				}
				return shopRemoveCardPriceOverride.GetValueOrDefault();
			}
		}
		public int DrawCardCount { get; set; } = 5;
		public int SynergyAdditionalCount { get; set; }
		public List<Type> ShiningExhibitPool { get; private set; }
		public List<Type> ExhibitPool { get; private set; }
		public List<string> ExhibitRecord { get; } = new List<string>();
		public List<Type> AdventureHistory { get; } = new List<Type>();
		private void EnterStage(int index)
		{
			if (index < 0 || index >= this._stages.Count)
			{
				throw new IndexOutOfRangeException(string.Format("Enter stage index {0} out of range", index));
			}
			this._stageIndex = index;
			Stage stage = this._stages[index];
			this.CurrentStage = stage;
			this.CurrentMap = stage.CreateMap();
			stage.Enter();
			this.StageRecords.Add(new StageRecord
			{
				Id = stage.Id
			});
			this.StageEntered.Execute(new GameEventArgs
			{
				CanCancel = false
			});
		}
		public void EnterNextStage()
		{
			if (this.Status != GameRunStatus.Running)
			{
				throw new InvalidOperationException("Cannot enter next stage while not running");
			}
			this.LeaveStation();
			if (this._stageIndex >= 0)
			{
				if (this.Puzzles.HasFlag(PuzzleFlag.LowStageRegen))
				{
					this.Heal(((float)this.Player.MaxHp * 0.5f).RoundToInt(), true, null);
				}
				else
				{
					this.HealToMaxHp(true, null);
				}
			}
			this.EnterStage(this._stageIndex + 1);
		}
		private void LeaveStation()
		{
			Station currentStation = this.CurrentStation;
			if (currentStation == null)
			{
				return;
			}
			StationEventArgs stationEventArgs = new StationEventArgs
			{
				CanCancel = false,
				Station = currentStation
			};
			this.StationLeaving.Execute(stationEventArgs);
			currentStation.OnLeave();
			StageRecord stageRecord = Enumerable.LastOrDefault<StageRecord>(this.StageRecords);
			if (stageRecord == null)
			{
				Debug.LogError("Leave station while stage records is empty");
			}
			else
			{
				stageRecord.Stations.Add(currentStation.GenerateRecord());
			}
			this.CurrentStation = null;
			this.StationLeft.Execute(stationEventArgs);
		}
		private void EnterStation(Station station)
		{
			StationEventArgs stationEventArgs = new StationEventArgs
			{
				CanCancel = false,
				Station = station
			};
			this.StationEntering.Execute(stationEventArgs);
			this.CurrentStation = station;
			station.OnEnter();
			this.StationEntered.Execute(stationEventArgs);
		}
		public GameRunSaveData EnterMapNode(MapNode node, bool forced = false)
		{
			if (this.Status != GameRunStatus.Running)
			{
				throw new InvalidOperationException("Cannot enter map node while not running");
			}
			if (this.CurrentStation != null && this.CurrentStation.Status != StationStatus.Finished)
			{
				throw new InvalidOperationException("Cannot enter other map-node when current station is not finished");
			}
			if (!forced)
			{
				switch (this.MapMode)
				{
				case GameRunMapMode.Normal:
					if (node.Status != MapNodeStatus.Active)
					{
						throw new InvalidOperationException(string.Format("Entering MapNode status {0} != {1}", node.Status, "Active"));
					}
					break;
				case GameRunMapMode.Crossing:
					if (node.Status != MapNodeStatus.Active && node.Status != MapNodeStatus.CrossActive)
					{
						throw new InvalidOperationException(string.Format("Entering MapNode status {0} != {1} or {2}", node.Status, "Active", "CrossActive"));
					}
					break;
				case GameRunMapMode.TeleportBoss:
					if (node.StationType != StationType.Boss)
					{
						throw new InvalidOperationException("Teleporting to non-boss map-node");
					}
					break;
				}
			}
			this.LeaveStation();
			GameRunSaveData gameRunSaveData = this.Save();
			gameRunSaveData.Timing = SaveTiming.EnterMapNode;
			gameRunSaveData.EnteringNode = new MapNodeSaveData
			{
				X = node.X,
				Y = node.Y
			};
			if (this.MapMode == GameRunMapMode.Normal)
			{
				this.CurrentMap.EnterNode(node, false, forced);
			}
			else if (this.MapMode == GameRunMapMode.Crossing)
			{
				if (node.Status == MapNodeStatus.CrossActive)
				{
					this._activeMapModeOverrider.OnEnteredWithMode();
					this.CurrentMap.EnterNode(node, true, forced);
				}
				else
				{
					this.CurrentMap.EnterNode(node, false, forced);
				}
			}
			else if (this.MapMode == GameRunMapMode.TeleportBoss)
			{
				this._activeMapModeOverrider.OnEnteredWithMode();
				this.CurrentMap.EnterNode(node, true, forced);
			}
			else if (this.MapMode == GameRunMapMode.Free)
			{
				this._activeMapModeOverrider.OnEnteredWithMode();
				this.CurrentMap.EnterNode(node, false, forced);
			}
			this.CheckMapMode();
			Station station = this.CurrentStage.CreateStation(node);
			this.EnterStation(station);
			return gameRunSaveData;
		}
		public void EnterBattle(EnemyGroup enemyGroup)
		{
			this.RevealEnemyGroup(enemyGroup);
			this.Battle = new BattleController(this, enemyGroup, this._baseDeck);
			foreach (Exhibit exhibit in this.Player.Exhibits)
			{
				exhibit.EnterBattle();
			}
			foreach (JadeBox jadeBox in this._jadeBoxes)
			{
				jadeBox.EnterBattle();
			}
		}
		public BattleStats LeaveBattle(EnemyGroup enemyGroup)
		{
			foreach (Exhibit exhibit in this.Player.Exhibits)
			{
				exhibit.LeaveBattle();
			}
			foreach (JadeBox jadeBox in this._jadeBoxes)
			{
				jadeBox.LeaveBattle();
			}
			this.Battle.Leave();
			BattleStats stats = this.Battle.Stats;
			this.Stats.ContinuousTurnCount = Math.Max(this.Stats.ContinuousTurnCount, stats.ContinuousTurnCount);
			this.Stats.MaxSingleAttackDamage = Math.Max(this.Stats.MaxSingleAttackDamage, stats.MaxSingleAttackDamage);
			if (this.Player.IsDead)
			{
				this.Status = GameRunStatus.Failure;
				this.Stats.PlayerSuicide = stats.PlayerSuicide;
				this.GenerateRecords(false, enemyGroup, null);
			}
			else
			{
				while (this.Stats.Stages.Count <= this._stageIndex)
				{
					this.Stats.Stages.Add(new StageStats());
				}
				StageStats stageStats = this.Stats.Stages[this._stageIndex];
				switch (enemyGroup.EnemyType)
				{
				case EnemyType.Normal:
					stageStats.NormalEnemyDefeated++;
					stageStats.NormalEnemyBluePoint += stats.BluePoint;
					break;
				case EnemyType.Elite:
					stageStats.EliteEnemyDefeated++;
					stageStats.EliteEnemyBluePoint += stats.BluePoint;
					if (!stats.PlayerDamaged)
					{
						this.Stats.PerfectElite++;
					}
					break;
				case EnemyType.Boss:
					stageStats.BossDefeated++;
					stageStats.BossBluePoint += stats.BluePoint;
					if (!this.CurrentStage.IsTrueEndFinalStage && !stats.PlayerDamaged)
					{
						this.Stats.PerfectBoss++;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			if (enemyGroup.EnemyType == EnemyType.Boss)
			{
				this.Stats.Bosses.Add(enemyGroup.Id);
			}
			this.Battle = null;
			foreach (EnemyUnit enemyUnit in enemyGroup)
			{
				enemyUnit.LeaveGameRun();
			}
			return stats;
		}
		internal void FinishStation(Station station)
		{
			if (this.Status == GameRunStatus.Running)
			{
				if (station.IsNormalEnd)
				{
					this.IsNormalEndFinished = true;
					if (!this.CanEnterTrueEnding())
					{
						this.Status = GameRunStatus.NormalEnd;
						this.GenerateRecords(false, null, null);
						return;
					}
				}
				else
				{
					if (station.IsTrueEnd)
					{
						this.Status = GameRunStatus.TrueEnd;
						this.GenerateRecords(false, null, null);
						return;
					}
					this.CurrentMap.SetAdjacentNodesStatus(this.MapMode);
				}
			}
		}
		internal EnemyUnit[] GetOpponentCandidates()
		{
			string playerId = this.Player.Id;
			UniqueRandomPool<string> uniqueRandomPool = new UniqueRandomPool<string>(false);
			IEnumerable<string> enumerable = Library.EnumerateOpponentIds();
			Func<string, bool> <>9__0;
			Func<string, bool> func;
			if ((func = <>9__0) == null)
			{
				func = (<>9__0 = (string op) => op != playerId);
			}
			foreach (string text in Enumerable.Where<string>(enumerable, func))
			{
				uniqueRandomPool.Add(text, 1f);
			}
			return Enumerable.ToArray<EnemyUnit>(Enumerable.Select<string, EnemyUnit>(uniqueRandomPool.SampleMany(this.StationRng, 3, true), new Func<string, EnemyUnit>(Library.CreateEnemyUnit)));
		}
		internal Exhibit RollNormalExhibit(RandomGen rng, ExhibitWeightTable weightTable, Func<Exhibit> fallback, [MaybeNull] Predicate<ExhibitConfig> filter = null)
		{
			RepeatableRandomPool<Type> repeatableRandomPool = new RepeatableRandomPool<Type>();
			foreach (Type type in this.ExhibitPool)
			{
				ExhibitConfig exhibitConfig = ExhibitConfig.FromId(type.Name);
				ManaColor? baseManaRequirement = exhibitConfig.BaseManaRequirement;
				if (baseManaRequirement != null)
				{
					ManaColor valueOrDefault = baseManaRequirement.GetValueOrDefault();
					if (this.BaseMana[valueOrDefault] == 0)
					{
						continue;
					}
				}
				if (filter == null || filter.Invoke(exhibitConfig))
				{
					float num = weightTable.WeightFor(exhibitConfig);
					if (num > 0f)
					{
						repeatableRandomPool.Add(type, Library.WeightForExhibit(type, this) * num);
					}
				}
			}
			Type type2 = repeatableRandomPool.SampleOrDefault(rng);
			if (type2 != null)
			{
				this.ExhibitPool.Remove(type2);
				return Library.CreateExhibit(type2);
			}
			return fallback.Invoke();
		}
		internal Exhibit RollShiningExhibit(RandomGen rng, [MaybeNull] Func<Exhibit> fallback = null, [MaybeNull] Predicate<ExhibitConfig> filter = null)
		{
			RepeatableRandomPool<Type> repeatableRandomPool = new RepeatableRandomPool<Type>();
			foreach (Type type in this.ShiningExhibitPool)
			{
				ExhibitConfig exhibitConfig = ExhibitConfig.FromId(type.Name);
				if ((filter == null || filter.Invoke(exhibitConfig)) && !this.Player.HasExhibit(type))
				{
					repeatableRandomPool.Add(type, Library.WeightForExhibit(type, this));
				}
			}
			if (!repeatableRandomPool.IsEmpty)
			{
				Type type2 = repeatableRandomPool.Sample(rng);
				this.ShiningExhibitPool.Remove(type2);
				return Library.CreateExhibit(type2);
			}
			if (fallback == null)
			{
				return null;
			}
			return fallback.Invoke();
		}
		internal Exhibit[] RollBossExhibits(RandomGen rng, string bossId, bool rollBossExhibit, Func<Exhibit> fallback)
		{
			GameRunController.<>c__DisplayClass425_0 CS$<>8__locals1 = new GameRunController.<>c__DisplayClass425_0();
			CS$<>8__locals1.bossId = bossId;
			EnemyUnitConfig enemyUnitConfig = EnemyUnitConfig.FromId(CS$<>8__locals1.bossId);
			if (enemyUnitConfig == null)
			{
				throw new ArgumentException("Cannot find boss config for '" + CS$<>8__locals1.bossId + "'", "bossId");
			}
			Exhibit exhibit = null;
			if (rollBossExhibit)
			{
				exhibit = this.RollShiningExhibit(rng, null, (ExhibitConfig config) => CS$<>8__locals1.bossId.Equals(config.Owner, 4));
			}
			if (exhibit == null)
			{
				IReadOnlyList<ManaColor> bossColors = enemyUnitConfig.BaseManaColor;
				if (bossColors.Empty<ManaColor>())
				{
					Debug.LogError("Cannot roll fallback exhibit for " + CS$<>8__locals1.bossId + ": no BaseManaColor defined, using fallback.");
					exhibit = fallback.Invoke();
				}
				else
				{
					exhibit = this.RollShiningExhibit(rng, null, delegate(ExhibitConfig config)
					{
						if (!string.IsNullOrWhiteSpace(config.Owner))
						{
							return false;
						}
						ManaColor? baseManaColor = config.BaseManaColor;
						if (baseManaColor != null)
						{
							ManaColor valueOrDefault = baseManaColor.GetValueOrDefault();
							return Enumerable.Contains<ManaColor>(bossColors, valueOrDefault);
						}
						return false;
					});
					if (exhibit == null)
					{
						string text = string.Join<char>(", ", Enumerable.Select<ManaColor, char>(bossColors, (ManaColor c) => c.ToShortName()));
						Debug.LogWarning("Cannot roll boss fallback shining exhibit with color [" + text + "], using fallback.");
						exhibit = fallback.Invoke();
					}
				}
			}
			CS$<>8__locals1.bossExhibitColor = ((exhibit != null) ? exhibit.Config.BaseManaColor : default(ManaColor?));
			CS$<>8__locals1.playerColorSet = Enumerable.ToHashSet<ManaColor>(Enumerable.Where<ManaColor>(this.BaseMana.EnumerateColors(), delegate(ManaColor c)
			{
				ManaColor? bossExhibitColor = CS$<>8__locals1.bossExhibitColor;
				return !((c == bossExhibitColor.GetValueOrDefault()) & (bossExhibitColor != null));
			}));
			Exhibit exhibit2 = this.RollShiningExhibit(rng, null, delegate(ExhibitConfig config)
			{
				if (!string.IsNullOrWhiteSpace(config.Owner))
				{
					return false;
				}
				ManaColor? baseManaColor2 = config.BaseManaColor;
				if (baseManaColor2 != null)
				{
					ManaColor valueOrDefault2 = baseManaColor2.GetValueOrDefault();
					return CS$<>8__locals1.playerColorSet.Contains(valueOrDefault2);
				}
				return false;
			});
			if (exhibit2 == null)
			{
				string text2 = string.Join<ManaColor>(", ", CS$<>8__locals1.playerColorSet);
				Debug.LogWarning(string.Concat(new string[]
				{
					"Cannot roll exhibit for ",
					this.Player.DebugName,
					" with color [",
					text2,
					"], using fallback."
				}));
				exhibit2 = fallback.Invoke();
			}
			CS$<>8__locals1.playerExhibitColor = ((exhibit2 != null) ? exhibit2.Config.BaseManaColor : default(ManaColor?));
			Exhibit exhibit3 = this.RollShiningExhibit(rng, null, delegate(ExhibitConfig config)
			{
				if (!string.IsNullOrWhiteSpace(config.Owner))
				{
					return false;
				}
				ManaColor? manaColor = config.BaseManaColor;
				ManaColor? manaColor2 = CS$<>8__locals1.playerExhibitColor;
				if (!((manaColor.GetValueOrDefault() == manaColor2.GetValueOrDefault()) & (manaColor != null == (manaColor2 != null))))
				{
					manaColor2 = config.BaseManaColor;
					manaColor = CS$<>8__locals1.bossExhibitColor;
					return !((manaColor2.GetValueOrDefault() == manaColor.GetValueOrDefault()) & (manaColor2 != null == (manaColor != null)));
				}
				return false;
			});
			if (exhibit3 == null)
			{
				GameRunController.<>c__DisplayClass425_0 CS$<>8__locals3 = CS$<>8__locals1;
				string text3 = ((CS$<>8__locals3.playerExhibitColor != null) ? new char?(CS$<>8__locals3.playerExhibitColor.GetValueOrDefault().ToShortName()) : default(char?)).ToString();
				string text4 = ", ";
				GameRunController.<>c__DisplayClass425_0 CS$<>8__locals4 = CS$<>8__locals1;
				string text5 = text3 + text4 + ((CS$<>8__locals4.bossExhibitColor != null) ? new char?(CS$<>8__locals4.bossExhibitColor.GetValueOrDefault().ToShortName()) : default(char?)).ToString();
				Debug.Log("Cannot roll exhibit without color [" + text5 + "], using fallback");
				exhibit3 = fallback.Invoke();
			}
			return new Exhibit[] { exhibit2, exhibit, exhibit3 };
		}
		private float BaseCardWeight(CardConfig config, bool applyFactors)
		{
			ManaGroup cost = config.Cost;
			float num;
			switch (Math.Max(config.Colors.Count, config.Cost.TrivialColorCount))
			{
			case 0:
				num = 0.9f;
				break;
			case 1:
			{
				float num2;
				switch (cost.GetValue(cost.MaxTrivialColor))
				{
				case 0:
					num2 = 0.9f;
					break;
				case 1:
					num2 = 0.9f;
					break;
				case 2:
					num2 = 1f;
					break;
				case 3:
					num2 = 1.1f;
					break;
				case 4:
					num2 = 1.2f;
					break;
				default:
					throw new InvalidDataException(string.Format("Invalid cost pattern {0} of card '{1}'", cost, config.Id));
				}
				num = num2;
				break;
			}
			case 2:
			{
				float num2;
				switch (cost.GetValue(cost.MaxTrivialColor))
				{
				case 0:
					num2 = 0.9f;
					break;
				case 1:
					num2 = 1f;
					break;
				case 2:
					num2 = 1.1f;
					break;
				default:
					throw new InvalidDataException(string.Format("Invalid cost pattern {0} of card '{1}'", cost, config.Id));
				}
				num = num2;
				break;
			}
			case 3:
				num = 1.3f;
				break;
			default:
				throw new InvalidDataException(string.Format("Invalid cost pattern {0} of card '{1}'", cost, config.Id));
			}
			float num3 = num;
			float num4 = 1f;
			int count = config.Colors.Count;
			if (count <= 0)
			{
				if (count == 0)
				{
					num4 = 0.8f;
				}
			}
			else
			{
				foreach (ManaColor manaColor in config.Colors)
				{
					float num5 = (float)this.BaseMana.GetValue(manaColor) / (float)this.BaseMana.Amount;
					num5 -= 0.5f;
					num5 *= 0.8f;
					num4 += num5;
				}
				num4 = Math.Max(num4, 0.8f);
			}
			num3 *= num4;
			if (applyFactors)
			{
				if (config.Rarity == Rarity.Rare)
				{
					num3 *= this._cardRareWeightFactor;
				}
				float num6;
				if (this._cardRewardWeightFactors.TryGetValue(config.Id, ref num6))
				{
					num3 *= num6;
				}
			}
			return num3;
		}
		public UniqueRandomPool<Type> CreateValidCardsPool(CardWeightTable weightTable, ManaGroup? manaLimit, bool colorLimit, bool applyFactors, bool battleRolling, [MaybeNull] Predicate<CardConfig> filter = null)
		{
			HashSet<string> hashSet = new HashSet<string>();
			hashSet.Add(this.Player.Id);
			HashSet<string> hashSet2 = hashSet;
			if (this.AllCharacterCardsFlag > 0)
			{
				using (IEnumerator<PlayerUnitConfig> enumerator = PlayerUnitConfig.AllConfig().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						PlayerUnitConfig playerUnitConfig = enumerator.Current;
						hashSet2.Add(playerUnitConfig.Id);
					}
					goto IL_00A1;
				}
			}
			foreach (Exhibit exhibit in this.Player.Exhibits)
			{
				if (exhibit.OwnerId != null)
				{
					hashSet2.Add(exhibit.OwnerId);
				}
			}
			IL_00A1:
			UniqueRandomPool<Type> uniqueRandomPool = new UniqueRandomPool<Type>(false);
			foreach (ValueTuple<Type, CardConfig> valueTuple in Library.EnumerateRollableCardTypes(this.UnlockLevel))
			{
				Type item = valueTuple.Item1;
				CardConfig item2 = valueTuple.Item2;
				if (item2.IsPooled && item2.DebugLevel <= this.CardValidDebugLevel && (!battleRolling || item2.FindInBattle) && (filter == null || filter.Invoke(item2)))
				{
					if (manaLimit != null)
					{
						ManaGroup mana = manaLimit.GetValueOrDefault();
						ManaGroup cost = item2.Cost;
						if (cost.Amount > 5)
						{
							if (!mana.CanAfford(cost.WithAny(0)))
							{
								continue;
							}
						}
						else if (!mana.CanAfford(cost))
						{
							continue;
						}
						if (colorLimit && Enumerable.Any<ManaColor>(item2.Colors, (ManaColor c) => mana.GetValue(c) == 0))
						{
							continue;
						}
					}
					float num = weightTable.WeightFor(item2, this.Player.Id, hashSet2);
					if (num > 0f)
					{
						float num2 = this.BaseCardWeight(item2, applyFactors);
						uniqueRandomPool.Add(item, num * num2);
					}
				}
			}
			return uniqueRandomPool;
		}
		internal Card[] GetRewardCards(CardWeightTable playerWeightTable, CardWeightTable friendWeightTable, CardWeightTable neutralWeightTable, CardWeightTable randomWeightTable, int count, bool isBossReward = false)
		{
			if (this._cardRewardDecreaseRepeatRare)
			{
				playerWeightTable = GameRunController.<GetRewardCards>g__ModifyRare|436_0(playerWeightTable, 0.01f);
				friendWeightTable = GameRunController.<GetRewardCards>g__ModifyRare|436_0(friendWeightTable, 0.01f);
				neutralWeightTable = GameRunController.<GetRewardCards>g__ModifyRare|436_0(neutralWeightTable, 0.01f);
				randomWeightTable = GameRunController.<GetRewardCards>g__ModifyRare|436_0(randomWeightTable, 0.01f);
				this._cardRewardDecreaseRepeatRare = false;
			}
			List<Card> cards = new List<Card>();
			if (count > 0)
			{
				Card[] array = this.RollCards(this.CardRng, playerWeightTable, 1, new ManaGroup?(this.BaseMana), this.RewardAndShopCardColorLimitFlag == 0, true, false, false, null);
				cards.AddRange(array);
			}
			if (count > 1)
			{
				if (Enumerable.Any<Card>(cards, (Card c) => c.Config.Rarity == Rarity.Rare && !isBossReward))
				{
					friendWeightTable = GameRunController.<GetRewardCards>g__ModifyRare|436_0(friendWeightTable, 0f);
				}
				Card[] array2 = this.RollCards(this.CardRng, friendWeightTable, 1, new ManaGroup?(this.BaseMana), this.RewardAndShopCardColorLimitFlag == 0, true, false, false, (CardConfig config) => config.Id != cards[0].Id);
				cards.AddRange(array2);
			}
			if (count > 2)
			{
				if (Enumerable.Any<Card>(cards, (Card c) => c.Config.Rarity == Rarity.Rare && !isBossReward))
				{
					neutralWeightTable = GameRunController.<GetRewardCards>g__ModifyRare|436_0(neutralWeightTable, 0f);
				}
				Card[] array3 = this.RollCards(this.CardRng, neutralWeightTable, 1, new ManaGroup?(this.BaseMana), this.RewardAndShopCardColorLimitFlag == 0, true, false, false, null);
				cards.AddRange(array3);
			}
			if (count > 3)
			{
				if (Enumerable.Any<Card>(cards, (Card c) => c.Config.Rarity == Rarity.Rare && !isBossReward))
				{
					randomWeightTable = GameRunController.<GetRewardCards>g__ModifyRare|436_0(randomWeightTable, 0f);
				}
				Card[] array4 = this.RollCards(this.CardRng, randomWeightTable, count - 3, new ManaGroup?(this.BaseMana), this.RewardAndShopCardColorLimitFlag == 0, true, false, false, (CardConfig config) => Enumerable.All<Card>(cards, (Card c) => c.Id != config.Id));
				cards.AddRange(array4);
			}
			if (Enumerable.Any<Card>(cards, (Card c) => c.Config.Rarity == Rarity.Rare))
			{
				this._cardRewardDecreaseRepeatRare = true;
				this._cardRareWeightFactor = 0.85f;
			}
			else
			{
				this._cardRareWeightFactor += 0.01f;
			}
			foreach (Card card in cards)
			{
				Rarity rarity = card.Config.Rarity;
				if ((rarity == Rarity.Common || rarity == Rarity.Uncommon) && card.CanUpgrade && this.CardRng.NextFloat(0f, 1f) < this.CardUpgradedChance)
				{
					card.Upgrade();
				}
			}
			foreach (Card card2 in cards)
			{
				this.UpgradeNewDeckCardOnFlags(card2);
			}
			return cards.ToArray();
		}
		private float CardUpgradedChance
		{
			get
			{
				if (!this.Puzzles.HasFlag(PuzzleFlag.LowUpgradeRate))
				{
					return this.CurrentStage.CardUpgradedChance;
				}
				return this.CurrentStage.CardUpgradedChance * 0.5f;
			}
		}
		internal Card[] GetShopCards(int count, CardWeightTable weightTable, List<string> exclude = null)
		{
			Card[] array = this.RollCards(this.ShopRng, weightTable, count, new ManaGroup?(this.BaseMana), this.RewardAndShopCardColorLimitFlag == 0, true, false, false, (CardConfig config) => exclude == null || !exclude.Contains(config.Id));
			foreach (Card card in array)
			{
				this.UpgradeNewDeckCardOnFlags(card);
			}
			return array;
		}
		public Card[] RollCards(RandomGen rng, CardWeightTable weightTable, int count, ManaGroup? manaLimit, bool colorLimit, bool applyFactors = false, bool battleRolling = false, bool ensureCount = false, [MaybeNull] Predicate<CardConfig> filter = null)
		{
			Card[] array = Enumerable.ToArray<Card>(Enumerable.Select<Type, Card>(this.CreateValidCardsPool(weightTable, manaLimit, colorLimit, applyFactors, battleRolling, filter).SampleMany(rng, count, ensureCount), new Func<Type, Card>(Library.CreateCard)));
			Card[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].GameRun = this;
			}
			return array;
		}
		public Card[] RollCards(RandomGen rng, CardWeightTable weightTable, int count, bool applyFactors = false, bool battleRolling = false, [MaybeNull] Predicate<CardConfig> filter = null)
		{
			return this.RollCards(rng, weightTable, count, new ManaGroup?(this.BaseMana), this.RewardAndShopCardColorLimitFlag == 0, applyFactors, battleRolling, false, filter);
		}
		public Card RollCard(RandomGen rng, CardWeightTable weightTable, bool applyFactors = false, bool battleRolling = false, [MaybeNull] Predicate<CardConfig> filter = null)
		{
			return Enumerable.FirstOrDefault<Card>(this.RollCards(rng, weightTable, 1, applyFactors, battleRolling, filter));
		}
		public Card RollTransformCard(RandomGen rng, CardWeightTable weightTable, bool applyFactors = false, bool battleRolling = false, [MaybeNull] Predicate<CardConfig> filter = null)
		{
			List<string> deck = new List<string>();
			foreach (Card card in this.BaseDeck)
			{
				if (!deck.Contains(card.Id))
				{
					deck.Add(card.Id);
				}
			}
			return this.RollCard(rng, weightTable, applyFactors, battleRolling, (CardConfig config) => (filter == null || filter.Invoke(config)) && !deck.Contains(config.Id)) ?? this.RollCard(rng, weightTable, applyFactors, battleRolling, filter);
		}
		public Card[] RollCardsWithoutManaLimit(RandomGen rng, CardWeightTable weightTable, int count, bool applyFactors = false, bool battleRolling = false, [MaybeNull] Predicate<CardConfig> filter = null)
		{
			return this.RollCards(rng, weightTable, count, default(ManaGroup?), false, applyFactors, battleRolling, false, filter);
		}
		public Card GetRandomCurseCard(RandomGen rng, bool containUnremovable = false)
		{
			List<Type> list = new List<Type>();
			foreach (ValueTuple<Type, CardConfig> valueTuple in Library.EnumerateCardTypes())
			{
				Type item = valueTuple.Item1;
				CardConfig item2 = valueTuple.Item2;
				if (item2.Type == CardType.Misfortune && (containUnremovable || !item2.Keywords.HasFlag(Keyword.Unremovable)))
				{
					list.Add(item);
				}
			}
			if (list.Count == 0)
			{
				Debug.Log("No curse card in library found");
				return null;
			}
			return TypeFactory<Card>.CreateInstance(list.Sample(rng));
		}
		public void GainMaxHp(int amount, bool triggerVisual = true, bool stats = true)
		{
			if (amount < 0)
			{
				throw new ArgumentException("Can not gain negative MaxHp.");
			}
			this.Player.SetMaxHp(this.Player.Hp, this.Player.MaxHp + amount);
			this.Heal(amount, triggerVisual, null);
			IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
			if (visualTrigger != null)
			{
				visualTrigger.OnGainMaxHp(amount, triggerVisual);
			}
			if (stats)
			{
				this.Stats.MaxHpGained += amount;
			}
		}
		public void GainMaxHpOnly(int amount, bool triggerVisual = false)
		{
			if (amount < 0)
			{
				throw new ArgumentException("Can not gain negative MaxHp.");
			}
			this.Player.SetMaxHp(this.Player.Hp, this.Player.MaxHp + amount);
			IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
			if (visualTrigger != null)
			{
				visualTrigger.OnGainMaxHp(amount, triggerVisual);
			}
			this.Stats.MaxHpGained += amount;
		}
		public void LoseMaxHp(int amount, bool triggerVisual = false)
		{
			if (amount < 0)
			{
				throw new ArgumentException("Can not lose negative MaxHp.");
			}
			int num = Math.Min(this.Player.MaxHp - 1, amount);
			int num2 = this.Player.MaxHp - num;
			int num3 = Math.Min(this.Player.Hp, num2);
			this.Player.SetMaxHp(num3, num2);
			IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
			if (visualTrigger == null)
			{
				return;
			}
			visualTrigger.OnLoseMaxHp(num, triggerVisual);
		}
		public void SetHpAndMaxHp(int hp, int maxHp, bool triggerVisual = false)
		{
			if (maxHp <= 0)
			{
				throw new ArgumentException("MaxHp must be greater than 0.");
			}
			if (hp <= 0)
			{
				throw new ArgumentException("Hp must be greater than 0.");
			}
			if (hp > maxHp)
			{
				throw new ArgumentException("Hp must be lesser than or equal to MaxHp");
			}
			this.Player.SetMaxHp(hp, maxHp);
			IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
			if (visualTrigger == null)
			{
				return;
			}
			visualTrigger.OnSetHpAndMaxHp(hp, maxHp, triggerVisual);
		}
		public void SetEnemyHpAndMaxHp(int hp, int maxHp, EnemyUnit unit, bool triggerVisual = false)
		{
			unit.SetMaxHp(hp, maxHp);
			IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
			if (visualTrigger == null)
			{
				return;
			}
			visualTrigger.OnEnemySetHpAndMaxHp(unit.RootIndex, hp, maxHp, triggerVisual);
		}
		public void Damage(int damage, DamageType damageType, bool isSelf, bool triggerVisual = false, Adventure fromAdventure = null)
		{
			DamageEventArgs damageEventArgs = new DamageEventArgs
			{
				Target = this.Player,
				DamageInfo = new DamageInfo((float)damage, damageType, false, false, false)
			};
			this.Player.DamageReceiving.Execute(damageEventArgs);
			this.Player.DamageTaking.Execute(damageEventArgs);
			if (!damageEventArgs.IsCanceled)
			{
				this.Player.TakeDamage(damageEventArgs.DamageInfo);
				if (this.Player.Hp == 0)
				{
					this.Player.Status = UnitStatus.Dying;
					DieEventArgs dieEventArgs = new DieEventArgs
					{
						Unit = this.Player,
						DieCause = DieCause.GameRun
					};
					this.Player.Dying.Execute(dieEventArgs);
					if (!dieEventArgs.IsCanceled)
					{
						this.Player.Status = UnitStatus.Dead;
						if (isSelf)
						{
							this.Stats.PlayerSuicide = true;
						}
						this.Status = GameRunStatus.Failure;
						this.GenerateRecords(false, null, fromAdventure);
					}
					else
					{
						this.Player.Status = UnitStatus.Alive;
					}
				}
				this.Player.DamageReceived.Execute(damageEventArgs);
				IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
				if (visualTrigger == null)
				{
					return;
				}
				visualTrigger.OnDamage(damageEventArgs.DamageInfo, triggerVisual);
			}
		}
		public void HealToMaxHp(bool triggerVisual = true, string audioName = null)
		{
			this.Heal(this.Player.MaxHp - this.Player.Hp, triggerVisual, audioName);
		}
		public void Heal(int amount, bool triggerVisual = true, string audioName = null)
		{
			HealEventArgs healEventArgs = new HealEventArgs
			{
				Amount = (float)amount,
				Target = this.Player,
				Cause = ActionCause.Gap,
				CanCancel = true
			};
			this.Player.HealingReceiving.Execute(healEventArgs);
			if (!healEventArgs.IsCanceled)
			{
				healEventArgs.Amount = healEventArgs.Amount.Round();
				int num = this.Player.Heal(healEventArgs.Amount.ToInt());
				healEventArgs.Amount = (float)num;
				this.Player.HealingReceived.Execute(healEventArgs);
				IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
				if (visualTrigger == null)
				{
					return;
				}
				visualTrigger.OnHeal(num, triggerVisual, audioName);
			}
		}
		internal int InternalGainPower(int power)
		{
			return this.Player.GainPower((this.PowerGainRate * (float)power).RoundToInt());
		}
		internal void InternalConsumePower(int power)
		{
			this.Player.ConsumePower((this.PowerGainRate * (float)power).RoundToInt());
		}
		internal int InternalLosePower(int power)
		{
			return this.Player.LosePower((this.PowerGainRate * (float)power).RoundToInt());
		}
		public void GainPower(int power, bool triggerVisual = false)
		{
			PowerEventArgs powerEventArgs = new PowerEventArgs
			{
				Power = power,
				CanCancel = false
			};
			powerEventArgs.Power = this.InternalGainPower(powerEventArgs.Power);
			this.Player.PowerGained.Execute(powerEventArgs);
			IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
			if (visualTrigger == null)
			{
				return;
			}
			visualTrigger.OnGainPower(powerEventArgs.Power, triggerVisual);
		}
		public void ConsumePower(int power, bool triggerVisual = false)
		{
			PowerEventArgs powerEventArgs = new PowerEventArgs
			{
				Power = power,
				CanCancel = false
			};
			this.Player.ConsumePower(powerEventArgs.Power);
			this.Player.PowerConsumed.Execute(powerEventArgs);
			IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
			if (visualTrigger == null)
			{
				return;
			}
			visualTrigger.OnConsumePower(powerEventArgs.Power, triggerVisual);
		}
		public void LosePower(int power, bool triggerVisual = false)
		{
			PowerEventArgs powerEventArgs = new PowerEventArgs
			{
				Power = power,
				CanCancel = false
			};
			powerEventArgs.Power = this.Player.LosePower(powerEventArgs.Power);
			this.Player.PowerLost.Execute(powerEventArgs);
			IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
			if (visualTrigger == null)
			{
				return;
			}
			visualTrigger.OnLosePower(powerEventArgs.Power, triggerVisual);
		}
		public void SetBaseMana(ManaGroup mana, bool triggerVisual = false)
		{
			this.BaseMana = mana;
			this.BaseManaChanged.Execute(new ManaEventArgs());
			IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
			if (visualTrigger == null)
			{
				return;
			}
			visualTrigger.OnSetBaseMana(mana, triggerVisual);
		}
		public void GainBaseMana(ManaGroup mana, bool triggerVisual = false)
		{
			this.BaseMana += mana;
			this.BaseManaChanged.Execute(new ManaEventArgs());
			IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
			if (visualTrigger == null)
			{
				return;
			}
			visualTrigger.OnGainBaseMana(mana, triggerVisual);
		}
		public bool TryLoseBaseMana(ManaGroup mana, bool triggerVisual = false)
		{
			ManaGroup manaGroup = this.BaseMana - mana;
			if (manaGroup.IsInvalid)
			{
				return false;
			}
			this.BaseMana = manaGroup;
			this.BaseManaChanged.Execute(new ManaEventArgs());
			IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
			if (visualTrigger != null)
			{
				visualTrigger.OnLoseBaseMana(mana, triggerVisual);
			}
			return true;
		}
		public void LoseBaseMana(ManaGroup mana, bool triggerVisual = false)
		{
			ManaGroup manaGroup = this.BaseMana - mana;
			if (manaGroup.IsInvalid)
			{
				throw new ArgumentException(string.Format("Cannot lose {0} from base-mana: {1}", mana, this.BaseMana));
			}
			this.BaseMana = manaGroup;
			this.BaseManaChanged.Execute(new ManaEventArgs());
			IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
			if (visualTrigger == null)
			{
				return;
			}
			visualTrigger.OnLoseBaseMana(mana, triggerVisual);
		}
		internal void InternalGainMoney(int money)
		{
			money = Math.Min(money, 99999 - this.Money);
			this.Money += money;
			this.TotalMoney += money;
		}
		public void GainMoney(int money, bool triggerVisual = false, VisualSourceData sourceData = null)
		{
			this.InternalGainMoney(money);
			this.VisualTrigger.OnGainMoney(money, triggerVisual, sourceData);
			this.MoneyGained.Execute(new GameEventArgs
			{
				CanCancel = false
			});
		}
		public void ConsumeMoney(int cost)
		{
			if (this.Money < cost)
			{
				throw new InvalidOperationException(string.Format("Cannot pay {0} with {1} = {2}", cost, "Money", this.Money));
			}
			this.Money -= cost;
			this.VisualTrigger.OnConsumeMoney(cost);
			this.MoneyConsumed.Execute(new GameEventArgs
			{
				CanCancel = false
			});
		}
		public void LoseMoney(int money)
		{
			this.Money = Math.Max(this.Money - money, 0);
			this.VisualTrigger.OnLoseMoney(money);
			this.MoneyLost.Execute(new GameEventArgs
			{
				CanCancel = false
			});
		}
		internal Card[] InternalAddDeckCards(Card[] cards)
		{
			List<Card> list = new List<Card>();
			foreach (Card card in cards)
			{
				this.UpgradeNewDeckCardOnFlags(card);
				card.GameRun = this;
				Card card2 = card;
				int num = this._deckCardInstanceId + 1;
				this._deckCardInstanceId = num;
				card2.InstanceId = num;
				this._baseDeck.Add(card);
				Action<Card> cardRevealed = this.CardRevealed;
				if (cardRevealed != null)
				{
					cardRevealed.Invoke(card);
				}
				list.Add(card);
				if (this._baseDeck.Count == 9999)
				{
					break;
				}
			}
			return list.ToArray();
		}
		public void AddDeckCard(Card card, bool triggerVisual = false, VisualSourceData sourceData = null)
		{
			this.AddDeckCards(new Card[] { card }, triggerVisual, sourceData);
		}
		public void AddDeckCards(IEnumerable<Card> cards, bool triggerVisual = false, VisualSourceData sourceData = null)
		{
			CardsEventArgs cardsEventArgs = new CardsEventArgs
			{
				Cards = Enumerable.ToArray<Card>(cards)
			};
			this.DeckCardsAdding.Execute(cardsEventArgs);
			if (!cardsEventArgs.IsCanceled)
			{
				cardsEventArgs.CanCancel = false;
				cardsEventArgs.Cards = this.InternalAddDeckCards(cardsEventArgs.Cards);
				this.DeckCardsAdded.Execute(cardsEventArgs);
				IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
				if (visualTrigger == null)
				{
					return;
				}
				visualTrigger.OnAddDeckCards(cardsEventArgs.Cards, triggerVisual, sourceData);
			}
		}
		public Card GetDeckCardByInstanceId(int instanceId)
		{
			return Enumerable.FirstOrDefault<Card>(this._baseDeck, (Card c) => c.InstanceId == instanceId);
		}
		public void UpgradeDeckCardByInstanceId(int instanceId)
		{
			Card card = Enumerable.FirstOrDefault<Card>(this._baseDeck, (Card c) => c.InstanceId == instanceId);
			if (card == null)
			{
				Debug.LogWarning(string.Format("Try upgrading card (instance-id: {0}) from deck: not found", instanceId));
				return;
			}
			this.UpgradeDeckCards(new Card[] { card }, false);
		}
		public void RemoveDeckCardByInstanceId(int instanceId)
		{
			Card card = Enumerable.FirstOrDefault<Card>(this._baseDeck, (Card c) => c.InstanceId == instanceId);
			if (card == null)
			{
				Debug.LogWarning(string.Format("Try removing card (instance-id: {0}) from deck: not found", instanceId));
				return;
			}
			this.RemoveDeckCards(new Card[] { card }, true);
		}
		public void RemoveDeckCard(Card card, bool triggerVisual = true)
		{
			this.RemoveDeckCards(new Card[] { card }, triggerVisual);
		}
		public void RemoveDeckCards(IEnumerable<Card> cards, bool triggerVisual = true)
		{
			Card[] array = Enumerable.ToArray<Card>(cards);
			foreach (Card card in array)
			{
				if (!this._baseDeck.Contains(card))
				{
					throw new InvalidOperationException("Cannot remove " + card.Name + " which is not in deck");
				}
			}
			CardsEventArgs cardsEventArgs = new CardsEventArgs
			{
				Cards = array
			};
			this.DeckCardsRemoving.Execute(cardsEventArgs);
			if (!cardsEventArgs.IsCanceled)
			{
				cardsEventArgs.CanCancel = false;
				foreach (Card card2 in cardsEventArgs.Cards)
				{
					this._baseDeck.Remove(card2);
				}
				this.DeckCardsRemoved.Execute(cardsEventArgs);
				IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
				if (visualTrigger == null)
				{
					return;
				}
				visualTrigger.OnRemoveDeckCards(cardsEventArgs.Cards, triggerVisual);
			}
		}
		public void RemoveGamerunInitialCards()
		{
			this.RemoveDeckCards(Enumerable.Where<Card>(this._baseDeck, (Card card) => card.IsGamerunInitial), false);
		}
		public void UpgradeDeckCard(Card card, bool triggerVisual = false)
		{
			this.UpgradeDeckCards(new Card[] { card }, triggerVisual);
		}
		public void UpgradeDeckCards(IEnumerable<Card> cards, bool triggerVisual = false)
		{
			Card[] array = Enumerable.ToArray<Card>(cards);
			foreach (Card card in array)
			{
				if (!this._baseDeck.Contains(card))
				{
					throw new InvalidOperationException("Upgrading card " + card.Name + " not in deck");
				}
			}
			Card[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Upgrade();
			}
			CardsEventArgs cardsEventArgs = new CardsEventArgs
			{
				Cards = array,
				Cause = ActionCause.Gap,
				CanCancel = false
			};
			this.DeckCardsUpgraded.Execute(cardsEventArgs);
			IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
			if (visualTrigger == null)
			{
				return;
			}
			visualTrigger.OnUpgradeDeckCards(cardsEventArgs.Cards, triggerVisual);
		}
		public void UpgradeRandomCards(int amount = 1, CardType? type = null)
		{
			if (amount <= 0)
			{
				throw new InvalidOperationException("随机升级牌数量为0或负数。");
			}
			List<Card> list = new List<Card>();
			foreach (Card card in this._baseDeck)
			{
				if (card.CanUpgradeAndPositive)
				{
					if (type != null)
					{
						if (card.CardType == type.Value)
						{
							list.Add(card);
						}
					}
					else
					{
						list.Add(card);
					}
				}
			}
			Card[] array = list.SampleManyOrAll(amount, this.GameRunEventRng);
			this.UpgradeDeckCards(array, false);
		}
		public IEnumerator GainExhibitRunner(Exhibit exhibit, bool triggerVisual = false, [MaybeNull] VisualSourceData exhibitSource = null)
		{
			if (exhibit.IsSentinel)
			{
				Debug.LogError("Cannot gain sentinel exhibit " + exhibit.DebugName);
				yield break;
			}
			if (this.Player.HasExhibit(exhibit.GetType()))
			{
				throw new ArgumentException("Cannot add duplicated Exhibit.", "exhibit");
			}
			exhibit.GameRun = this;
			this.Player.AddExhibit(exhibit);
			this.ExhibitRecord.Add(exhibit.Id);
			IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
			yield return (visualTrigger != null) ? visualTrigger.OnGainExhibit(exhibit, triggerVisual, exhibitSource) : null;
			yield return exhibit.TriggerGain(this.Player);
			Action<Exhibit> exhibitRevealed = this.ExhibitRevealed;
			if (exhibitRevealed != null)
			{
				exhibitRevealed.Invoke(exhibit);
			}
			Rarity rarity = exhibit.Config.Rarity;
			if (rarity != Rarity.Mythic && rarity != Rarity.Shining)
			{
				this.Stats.NoExhibitFlag = false;
			}
			yield break;
		}
		public void GainExhibitInstantly(Exhibit exhibit, bool triggerVisual = false, [MaybeNull] VisualSourceData exhibitSource = null)
		{
			if (this.Player.HasExhibit(exhibit.GetType()))
			{
				throw new ArgumentException("Cannot add duplicated Exhibit.", "exhibit");
			}
			exhibit.GameRun = this;
			this.Player.AddExhibit(exhibit);
			this.ExhibitRecord.Add(exhibit.Id);
			IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
			if (visualTrigger != null)
			{
				visualTrigger.OnGainExhibit(exhibit, triggerVisual, exhibitSource);
			}
			exhibit.TriggerGainInstantly(this.Player);
			Action<Exhibit> exhibitRevealed = this.ExhibitRevealed;
			if (exhibitRevealed != null)
			{
				exhibitRevealed.Invoke(exhibit);
			}
			Rarity rarity = exhibit.Config.Rarity;
			if (rarity != Rarity.Mythic && rarity != Rarity.Shining)
			{
				this.Stats.NoExhibitFlag = false;
			}
		}
		private void GainJadeBox(JadeBox jadeBox)
		{
			if (Enumerable.Any<JadeBox>(this._jadeBoxes, (JadeBox j) => j.Id == jadeBox.Id))
			{
				throw new ArgumentException("Cannot add duplicated JadeBox.", "jadeBox");
			}
			jadeBox.GameRun = this;
			jadeBox.TriggerGain(this);
			this._jadeBoxes.Add(jadeBox);
			jadeBox.TriggerAdded();
		}
		public void LoseExhibit(Exhibit exhibit, bool triggerVisual, bool removeFromRecord)
		{
			if (!this.Player.HasExhibit(exhibit))
			{
				throw new InvalidOperationException("Player does not has this exhibit \"" + exhibit.Name + "\"");
			}
			this.Player.RemoveExhibit(exhibit);
			exhibit.TriggerLose(this.Player);
			IGameRunVisualTrigger visualTrigger = this.VisualTrigger;
			if (visualTrigger != null)
			{
				visualTrigger.OnLoseExhibit(exhibit, triggerVisual);
			}
			exhibit.GameRun = null;
			if (removeFromRecord)
			{
				this.ExhibitRecord.Remove(exhibit.Id);
			}
		}
		internal Exhibit[] InternalLoseAllExhibits(bool removeFromRecord)
		{
			Exhibit[] array = Enumerable.ToArray<Exhibit>(Enumerable.Where<Exhibit>(this.Player.Exhibits, (Exhibit e) => e.LosableType == ExhibitLosableType.Losable));
			foreach (Exhibit exhibit in array)
			{
				this.Player.RemoveExhibit(exhibit);
				exhibit.TriggerLose(this.Player);
				exhibit.GameRun = null;
				if (removeFromRecord)
				{
					this.ExhibitRecord.Remove(exhibit.Id);
				}
			}
			return array;
		}
		internal void AcquireCardReward(StationReward reward, Card card, int index)
		{
			if (this.LootCardCommonDupeCount > 0 && card.Config.Rarity == Rarity.Common)
			{
				List<Card> list = new List<Card>();
				list.Add(card);
				List<Card> list2 = list;
				list2.AddRange(card.Clone(this.LootCardCommonDupeCount, false));
				this.AddDeckCards(list2, true, new VisualSourceData
				{
					SourceType = VisualSourceType.Reward,
					Index = index
				});
			}
			else if (this.LootCardUncommonDupeCount > 0 && card.Config.Rarity == Rarity.Uncommon)
			{
				List<Card> list3 = new List<Card>();
				list3.Add(card);
				List<Card> list4 = list3;
				list4.AddRange(card.Clone(this.LootCardUncommonDupeCount, false));
				this.AddDeckCards(list4, true, new VisualSourceData
				{
					SourceType = VisualSourceType.Reward,
					Index = index
				});
			}
			else
			{
				this.AddDeckCards(new Card[] { card }, true, new VisualSourceData
				{
					SourceType = VisualSourceType.Reward,
					Index = index
				});
			}
			IEnumerable<Card> cards = reward.Cards;
			Func<Card, bool> <>9__0;
			Func<Card, bool> func;
			if ((func = <>9__0) == null)
			{
				func = (<>9__0 = (Card c) => c != card);
			}
			foreach (Card card2 in Enumerable.Where<Card>(cards, func))
			{
				float num;
				if (!this._cardRewardWeightFactors.TryGetValue(card2.Id, ref num))
				{
					num = 1f;
				}
				this._cardRewardWeightFactors[card2.Id] = num * 0.9f;
			}
		}
		public void AbandonReward(StationReward reward)
		{
			this.GainMoney(this.RewardCardAbandonMoney, true, new VisualSourceData
			{
				SourceType = VisualSourceType.AbandonReward
			});
			if (reward.Type == StationRewardType.Card)
			{
				foreach (Card card in reward.Cards)
				{
					float num;
					if (!this._cardRewardWeightFactors.TryGetValue(card.Id, ref num))
					{
						num = 1f;
					}
					this._cardRewardWeightFactors[card.Id] = num * 0.85f;
				}
			}
			this.RewardAbandoned.Execute(new GameEventArgs
			{
				CanCancel = false
			});
		}
		internal void BuyCard(ShopItem<Card> cardItem)
		{
			cardItem.IsSoldOut = true;
			Card content = cardItem.Content;
			if (this.LootCardCommonDupeCount > 0 && content.Config.Rarity == Rarity.Common)
			{
				List<Card> list = new List<Card>();
				list.Add(content);
				List<Card> list2 = list;
				list2.AddRange(content.Clone(this.LootCardCommonDupeCount, false));
				this.AddDeckCards(list2, true, new VisualSourceData
				{
					SourceType = VisualSourceType.Shop
				});
			}
			else if (this.LootCardUncommonDupeCount > 0 && content.Config.Rarity == Rarity.Uncommon)
			{
				List<Card> list3 = new List<Card>();
				list3.Add(content);
				List<Card> list4 = list3;
				list4.AddRange(content.Clone(this.LootCardUncommonDupeCount, false));
				this.AddDeckCards(list4, true, new VisualSourceData
				{
					SourceType = VisualSourceType.Shop
				});
			}
			else
			{
				this.AddDeckCards(new Card[] { content }, true, new VisualSourceData
				{
					SourceType = VisualSourceType.Shop
				});
			}
			this.Stats.ShopConsumed += cardItem.Price;
			this.ConsumeMoney(cardItem.Price);
		}
		internal IEnumerator BuyExhibitRunner(ShopItem<Exhibit> exhibitItem, VisualSourceData sourceData)
		{
			exhibitItem.IsSoldOut = true;
			this.Stats.ShopConsumed += exhibitItem.Price;
			this.ConsumeMoney(exhibitItem.Price);
			yield return this.GainExhibitRunner(exhibitItem.Content, true, sourceData);
			yield break;
		}
		public void AbandonGameRun()
		{
			this.Status = GameRunStatus.Failure;
			this.GenerateRecords(true, null, null);
		}
		public GameRunRecordSaveData GameRunRecord { get; private set; }
		private void GenerateRecords(bool isAbandon, EnemyGroup failingEnemyGroup = null, Adventure failingAdventure = null)
		{
			if (this.GameRunRecord != null)
			{
				Debug.LogError("GenerateRecords multiple times");
				return;
			}
			if (!isAbandon)
			{
				if (this.CurrentStation == null)
				{
					Debug.LogError("GenerateRecords without current station entered.");
				}
				else
				{
					StageRecord stageRecord = Enumerable.LastOrDefault<StageRecord>(this.StageRecords);
					if (stageRecord == null)
					{
						Debug.LogError("GenerateRecords while stage records is empty.");
					}
					else
					{
						stageRecord.Stations.Add(this.CurrentStation.GenerateRecord());
					}
				}
			}
			GameResultType gameResultType;
			switch (this.Status)
			{
			case GameRunStatus.Running:
				throw new InvalidOperationException("Still in game-run");
			case GameRunStatus.NormalEnd:
				gameResultType = GameResultType.NormalEnd;
				break;
			case GameRunStatus.TrueEnd:
				gameResultType = GameResultType.TrueEnd;
				break;
			case GameRunStatus.Failure:
				gameResultType = (this.IsNormalEndFinished ? GameResultType.TrueEndFail : GameResultType.Failure);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			GameResultType gameResultType2 = gameResultType;
			GameRunRecordSaveData gameRunRecordSaveData = new GameRunRecordSaveData();
			gameRunRecordSaveData.Player = this.Player.Id;
			gameRunRecordSaveData.PlayerType = new PlayerType?(this.PlayerType);
			gameRunRecordSaveData.Us = this.Player.Us.Id;
			gameRunRecordSaveData.Mode = this.Mode;
			gameRunRecordSaveData.Difficulty = this.Difficulty;
			gameRunRecordSaveData.Puzzles = this.Puzzles;
			gameRunRecordSaveData.Seed = this.RootSeed;
			gameRunRecordSaveData.ResultType = gameResultType2;
			gameRunRecordSaveData.FailingEnemyGroup = ((failingEnemyGroup != null) ? failingEnemyGroup.Id : null);
			gameRunRecordSaveData.FailingAdventure = ((failingAdventure != null) ? failingAdventure.Id : null);
			gameRunRecordSaveData.Stages = Enumerable.ToArray<StageRecord>(Enumerable.Select<StageRecord, StageRecord>(this.StageRecords, (StageRecord r) => r.Clone()));
			gameRunRecordSaveData.MaxHp = this.Player.MaxHp;
			gameRunRecordSaveData.TotalMoney = this.TotalMoney;
			gameRunRecordSaveData.BaseMana = this.BaseMana.ToString();
			gameRunRecordSaveData.Cards = Enumerable.ToArray<CardRecordSaveData>(Enumerable.Select<Card, CardRecordSaveData>(this._baseDeck, (Card c) => new CardRecordSaveData
			{
				Id = c.Id,
				Upgraded = c.IsUpgraded,
				UpgradeCounter = c.UpgradeCounter
			}));
			gameRunRecordSaveData.Exhibits = this.ExhibitRecord.ToArray();
			gameRunRecordSaveData.JadeBoxes = Enumerable.ToArray<string>(Enumerable.Select<JadeBox, string>(this._jadeBoxes, (JadeBox j) => j.Id));
			gameRunRecordSaveData.IsAutoSeed = this.IsAutoSeed;
			gameRunRecordSaveData.ShowRandomResult = this.ShowRandomResult;
			gameRunRecordSaveData.ReloadTimes = this.ReloadTimes;
			gameRunRecordSaveData.GameVersion = VersionInfo.Current.Version;
			this.GameRunRecord = gameRunRecordSaveData;
		}
		public GameEvent<GameEventArgs> StageEntered { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<StationEventArgs> StationEntering { get; } = new GameEvent<StationEventArgs>();
		public GameEvent<StationEventArgs> StationEntered { get; } = new GameEvent<StationEventArgs>();
		public GameEvent<StationEventArgs> StationLeaving { get; } = new GameEvent<StationEventArgs>();
		public GameEvent<StationEventArgs> StationLeft { get; } = new GameEvent<StationEventArgs>();
		public GameEvent<StationEventArgs> StationFinished { get; } = new GameEvent<StationEventArgs>();
		public GameEvent<StationEventArgs> StationRewardGenerating { get; } = new GameEvent<StationEventArgs>();
		public GameEvent<StationEventArgs> GapOptionsGenerating { get; } = new GameEvent<StationEventArgs>();
		public GameEvent<GameEventArgs> RewardAbandoned { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<GameEventArgs> BaseManaChanged { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<GameEventArgs> MoneyGained { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<GameEventArgs> MoneyConsumed { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<GameEventArgs> MoneyLost { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<CardsEventArgs> DeckCardsAdding { get; } = new GameEvent<CardsEventArgs>();
		public GameEvent<CardsEventArgs> DeckCardsAdded { get; } = new GameEvent<CardsEventArgs>();
		public GameEvent<CardsEventArgs> DeckCardsRemoving { get; } = new GameEvent<CardsEventArgs>();
		public GameEvent<CardsEventArgs> DeckCardsRemoved { get; } = new GameEvent<CardsEventArgs>();
		public GameEvent<CardsEventArgs> DeckCardsUpgraded { get; } = new GameEvent<CardsEventArgs>();
		public GameRunSaveData Save()
		{
			GameRunSaveData gameRunSaveData = new GameRunSaveData();
			gameRunSaveData.Status = this.Status;
			gameRunSaveData.IsNormalEndFinished = this.IsNormalEndFinished;
			gameRunSaveData.Mode = this.Mode;
			gameRunSaveData.Difficulty = this.Difficulty;
			gameRunSaveData.Puzzles = this.Puzzles;
			gameRunSaveData.RootSeed = this.RootSeed;
			gameRunSaveData.IsAutoSeed = this.IsAutoSeed;
			gameRunSaveData.RootRng = this.RootRng.State;
			gameRunSaveData.StationRng = this.StationRng.State;
			gameRunSaveData.InitBossSeed = this.InitBossSeed;
			gameRunSaveData.ShopRng = this.ShopRng.State;
			gameRunSaveData.AdventureRng = this.AdventureRng.State;
			gameRunSaveData.ExhibitRng = this.ExhibitRng.State;
			gameRunSaveData.ShinningExhibitRng = this.ShinningExhibitRng.State;
			gameRunSaveData.CardRng = this.CardRng.State;
			gameRunSaveData.GamerunEventRng = this.GameRunEventRng.State;
			gameRunSaveData.BattleRng = this.BattleRng.State;
			gameRunSaveData.BattleCardRng = this.BattleCardRng.State;
			gameRunSaveData.ShuffleRng = this.ShuffleRng.State;
			gameRunSaveData.EnemyMoveRng = this.EnemyMoveRng.State;
			gameRunSaveData.EnemyBattleRng = this.EnemyBattleRng.State;
			gameRunSaveData.DebutRng = this.DebutRng.State;
			gameRunSaveData.FinalBossSeed = this.FinalBossSeed;
			gameRunSaveData.UISeed = this.UISeed;
			gameRunSaveData.HasClearBonus = this.HasClearBonus;
			gameRunSaveData.UnlockLevel = this.UnlockLevel;
			gameRunSaveData.FinalBossInitialDamage = this.FinalBossInitialDamage;
			Exhibit extraExhibitReward = this.ExtraExhibitReward;
			gameRunSaveData.ExtraExhibitReward = ((extraExhibitReward != null) ? extraExhibitReward.Id : null);
			gameRunSaveData.ReloadTimes = this.ReloadTimes;
			gameRunSaveData.ShowRandomResult = this.ShowRandomResult;
			GameRunSaveData gameRunSaveData2 = gameRunSaveData;
			foreach (Stage stage in this._stages)
			{
				List<StageSaveData> stages = gameRunSaveData2.Stages;
				StageSaveData stageSaveData = new StageSaveData();
				stageSaveData.Name = stage.GetType().Name;
				stageSaveData.Index = stage.Index;
				stageSaveData.MapSeed = stage.MapSeed;
				stageSaveData.Level = stage.Level;
				stageSaveData.SelectedBoss = stage.SelectedBoss;
				Type debutAdventureType = stage.DebutAdventureType;
				stageSaveData.DebutAdventure = ((debutAdventureType != null) ? debutAdventureType.Name : null);
				stageSaveData.IsNormalFinalStage = stage.IsNormalFinalStage;
				stageSaveData.IsTrueEndFinalStage = stage.IsTrueEndFinalStage;
				stageSaveData.AdventurePool = stage.AdventurePool.Save<string>((Type t) => t.Name);
				stageSaveData.EnemyPoolAct1 = stage.EnemyPoolAct1.Save<string>((string s) => s);
				stageSaveData.EnemyPoolAct2 = stage.EnemyPoolAct2.Save<string>((string s) => s);
				stageSaveData.EnemyPoolAct3 = stage.EnemyPoolAct3.Save<string>((string s) => s);
				stageSaveData.EliteEnemyPool = stage.EliteEnemyPool.Save<string>((string s) => s);
				stageSaveData.AdventureHistory = Enumerable.ToList<string>(Enumerable.Select<Type, string>(stage.AdventureHistory, (Type a) => a.Name));
				stageSaveData.ExtraFlags = Enumerable.ToList<string>(stage.ExtraFlags);
				stages.Add(stageSaveData);
			}
			GameRunSaveData gameRunSaveData3 = gameRunSaveData2;
			Stage currentStage = this.CurrentStage;
			gameRunSaveData3.StageIndex = ((currentStage != null) ? new int?(currentStage.Index) : default(int?));
			Station currentStation = this.CurrentStation;
			foreach (MapNode mapNode in this.CurrentMap.Path)
			{
				gameRunSaveData2.Path.Add(new MapNodeSaveData
				{
					X = mapNode.X,
					Y = mapNode.Y
				});
			}
			GameRunSaveData gameRunSaveData4 = gameRunSaveData2;
			PlayerSaveData playerSaveData = new PlayerSaveData();
			playerSaveData.Name = this.Player.Id;
			UltimateSkill us = this.Player.Us;
			playerSaveData.Us = ((us != null) ? us.Id : null);
			playerSaveData.Hp = this.Player.Hp;
			playerSaveData.MaxHp = this.Player.MaxHp;
			playerSaveData.Power = this.Player.Power;
			gameRunSaveData4.Player = playerSaveData;
			gameRunSaveData2.PlayerType = this.PlayerType;
			gameRunSaveData2.Mana = this.BaseMana.ToString();
			foreach (Card card in this._baseDeck)
			{
				gameRunSaveData2.Deck.Add(new CardSaveData
				{
					Name = card.Id,
					InstanceId = card.InstanceId,
					IsUpgraded = card.IsUpgraded,
					DeckCounter = card.DeckCounter,
					UpgradeCounter = card.UpgradeCounter
				});
			}
			gameRunSaveData2.DeckCardInstanceId = this._deckCardInstanceId;
			gameRunSaveData2.Money = this.Money;
			gameRunSaveData2.TotalMoney = this.TotalMoney;
			gameRunSaveData2.ShopRemoveCardCounter = this.ShopRemoveCardCounter;
			gameRunSaveData2.UltimateUseCount = this.UltimateUseCount;
			foreach (Exhibit exhibit in this.Player.Exhibits)
			{
				gameRunSaveData2.Exhibits.Add(new ExhibitSaveData
				{
					Name = exhibit.Id,
					Counter = (exhibit.HasCounter ? new int?(exhibit.Counter) : default(int?)),
					CardInstanceId = exhibit.CardInstanceId
				});
			}
			foreach (JadeBox jadeBox in this._jadeBoxes)
			{
				gameRunSaveData2.JadeBoxes.Add(new JadeBoxSaveData
				{
					Name = jadeBox.Id
				});
			}
			gameRunSaveData2.CardRareWeightFactor = this._cardRareWeightFactor;
			gameRunSaveData2.CardRewardWeightFactors = Enumerable.ToList<CardWeightFactorSaveData>(Enumerable.Select<KeyValuePair<string, float>, CardWeightFactorSaveData>(this._cardRewardWeightFactors, (KeyValuePair<string, float> kv) => new CardWeightFactorSaveData
			{
				Id = kv.Key,
				Value = kv.Value
			}));
			gameRunSaveData2.CardRewardDecreaseRepeatRare = this._cardRewardDecreaseRepeatRare;
			gameRunSaveData2.ShiningExhibitPool = Enumerable.ToList<string>(Enumerable.Select<Type, string>(this.ShiningExhibitPool, (Type e) => e.Name));
			gameRunSaveData2.ExhibitPool = Enumerable.ToList<string>(Enumerable.Select<Type, string>(this.ExhibitPool, (Type e) => e.Name));
			gameRunSaveData2.ExhibitRecord = Enumerable.ToList<string>(this.ExhibitRecord);
			gameRunSaveData2.AdventureHistory = Enumerable.ToList<string>(Enumerable.Select<Type, string>(this.AdventureHistory, (Type t) => t.Name));
			gameRunSaveData2.PlayedSeconds = this.PlayedSeconds;
			gameRunSaveData2.Stats = this.Stats.Clone();
			gameRunSaveData2.ExtraFlags = this.ExtraFlags;
			gameRunSaveData2.StageRecords = Enumerable.ToList<StageRecord>(Enumerable.Select<StageRecord, StageRecord>(this.StageRecords, (StageRecord r) => r.Clone()));
			return gameRunSaveData2;
		}
		public static GameRunController Restore(GameRunSaveData data, bool plusOne = true)
		{
			if (plusOne)
			{
				data.ReloadTimes++;
			}
			GameRunController gameRunController = new GameRunController
			{
				Status = data.Status,
				IsNormalEndFinished = data.IsNormalEndFinished,
				Mode = data.Mode,
				Difficulty = data.Difficulty,
				Puzzles = data.Puzzles,
				RootSeed = data.RootSeed,
				IsAutoSeed = data.IsAutoSeed,
				RootRng = RandomGen.FromState(data.RootRng),
				StationRng = RandomGen.FromState(data.StationRng),
				InitBossSeed = data.InitBossSeed,
				ShopRng = RandomGen.FromState(data.ShopRng),
				AdventureRng = RandomGen.FromState(data.AdventureRng),
				ExhibitRng = RandomGen.FromState(data.ExhibitRng),
				ShinningExhibitRng = RandomGen.FromState(data.ShinningExhibitRng),
				CardRng = RandomGen.FromState(data.CardRng),
				GameRunEventRng = RandomGen.FromState(data.GamerunEventRng),
				BattleRng = RandomGen.FromState(data.BattleRng),
				BattleCardRng = RandomGen.FromState(data.BattleCardRng),
				ShuffleRng = RandomGen.FromState(data.ShuffleRng),
				EnemyMoveRng = RandomGen.FromState(data.EnemyMoveRng),
				EnemyBattleRng = RandomGen.FromState(data.EnemyBattleRng),
				DebutRng = RandomGen.FromState(data.DebutRng),
				FinalBossSeed = data.FinalBossSeed,
				UISeed = data.UISeed,
				HasClearBonus = data.HasClearBonus,
				UnlockLevel = data.UnlockLevel,
				FinalBossInitialDamage = data.FinalBossInitialDamage,
				ExtraExhibitReward = ((data.ExtraExhibitReward != null) ? Library.TryCreateExhibit(data.ExtraExhibitReward) : null),
				ReloadTimes = data.ReloadTimes,
				ShowRandomResult = data.ShowRandomResult
			};
			gameRunController._stages = new List<Stage>();
			foreach (StageSaveData stageSaveData in data.Stages)
			{
				Stage stage = Library.CreateStage(stageSaveData.Name);
				stage.Index = stageSaveData.Index;
				stage.MapSeed = stageSaveData.MapSeed;
				stage.Level = stageSaveData.Level;
				if (stageSaveData.SelectedBoss != null)
				{
					stage.SetBoss(stageSaveData.SelectedBoss);
				}
				if (stageSaveData.DebutAdventure != null)
				{
					stage.DebutAdventureType = TypeFactory<Adventure>.GetType(stageSaveData.DebutAdventure);
				}
				stage.IsNormalFinalStage = stageSaveData.IsNormalFinalStage;
				stage.IsTrueEndFinalStage = stageSaveData.IsTrueEndFinalStage;
				stage.AdventurePool = UniqueRandomPool<Type>.Restore<string>(stageSaveData.AdventurePool, new Func<string, Type>(GameRunController.<Restore>g__GameEntityTypeConverter|550_0<Adventure>));
				stage.EnemyPoolAct1 = UniqueRandomPool<string>.Restore<string>(stageSaveData.EnemyPoolAct1, new Func<string, string>(GameRunController.<Restore>g__EnemyGroupConverter|550_1));
				stage.EnemyPoolAct2 = UniqueRandomPool<string>.Restore<string>(stageSaveData.EnemyPoolAct2, new Func<string, string>(GameRunController.<Restore>g__EnemyGroupConverter|550_1));
				stage.EnemyPoolAct3 = UniqueRandomPool<string>.Restore<string>(stageSaveData.EnemyPoolAct3, new Func<string, string>(GameRunController.<Restore>g__EnemyGroupConverter|550_1));
				stage.EliteEnemyPool = UniqueRandomPool<string>.Restore<string>(stageSaveData.EliteEnemyPool, new Func<string, string>(GameRunController.<Restore>g__EnemyGroupConverter|550_1));
				foreach (string text in stageSaveData.AdventureHistory)
				{
					Type type = TypeFactory<Adventure>.TryGetType(text);
					if (type != null)
					{
						stage.AdventureHistory.Add(type);
					}
				}
				stage.ExtraFlags = Enumerable.ToHashSet<string>(stageSaveData.ExtraFlags);
				stage.GameRun = gameRunController;
				gameRunController._stages.Add(stage);
			}
			RandomGen randomGen = new RandomGen(gameRunController.InitBossSeed);
			foreach (Stage stage2 in gameRunController._stages)
			{
				stage2.InitBoss(randomGen);
			}
			foreach (Stage stage3 in gameRunController._stages)
			{
				stage3.InitFirstAdventure(randomGen);
			}
			int? stageIndex = data.StageIndex;
			if (stageIndex != null)
			{
				int valueOrDefault = stageIndex.GetValueOrDefault();
				gameRunController._stageIndex = valueOrDefault;
				Stage stage4 = (gameRunController.CurrentStage = gameRunController._stages[valueOrDefault]);
				GameMap map = (gameRunController.CurrentMap = stage4.CreateMap());
				if (data.Path != null)
				{
					map.RestorePath(Enumerable.Select<MapNodeSaveData, MapNode>(data.Path, (MapNodeSaveData xy) => map.Nodes[xy.X, xy.Y]));
				}
			}
			PlayerUnit playerUnit = Library.CreatePlayerUnit(data.Player.Name);
			if (data.Player.Us != null)
			{
				playerUnit.SetUs(Library.CreateUs(data.Player.Us));
			}
			playerUnit.SetMaxHp(data.Player.Hp, data.Player.MaxHp);
			playerUnit.Power = data.Player.Power;
			gameRunController.Player = playerUnit;
			gameRunController.Player.Us.GameRun = gameRunController;
			gameRunController.PlayerType = data.PlayerType;
			gameRunController.BaseMana = ManaGroup.Parse(data.Mana);
			gameRunController._baseDeck = new List<Card>();
			foreach (CardSaveData cardSaveData in data.Deck)
			{
				Card card = Library.CreateCard(cardSaveData.Name);
				card.GameRun = gameRunController;
				card.InstanceId = cardSaveData.InstanceId;
				if (cardSaveData.IsUpgraded)
				{
					card.Upgrade();
				}
				card.DeckCounter = cardSaveData.DeckCounter;
				card.UpgradeCounter = cardSaveData.UpgradeCounter;
				gameRunController._baseDeck.Add(card);
			}
			gameRunController._deckCardInstanceId = data.DeckCardInstanceId;
			gameRunController.Money = data.Money;
			gameRunController.TotalMoney = data.TotalMoney;
			gameRunController.ShopRemoveCardCounter = data.ShopRemoveCardCounter;
			gameRunController.UltimateUseCount = data.UltimateUseCount;
			gameRunController._cardRareWeightFactor = data.CardRareWeightFactor;
			gameRunController._cardRewardWeightFactors = new Dictionary<string, float>();
			foreach (CardWeightFactorSaveData cardWeightFactorSaveData in data.CardRewardWeightFactors)
			{
				gameRunController._cardRewardWeightFactors.TryAdd(cardWeightFactorSaveData.Id, cardWeightFactorSaveData.Value);
			}
			gameRunController._cardRewardDecreaseRepeatRare = data.CardRewardDecreaseRepeatRare;
			gameRunController.ShiningExhibitPool = Enumerable.ToList<Type>(Enumerable.Select<string, Type>(data.ShiningExhibitPool, new Func<string, Type>(TypeFactory<Exhibit>.GetType)));
			gameRunController.ExhibitPool = Enumerable.ToList<Type>(Enumerable.Select<string, Type>(data.ExhibitPool, new Func<string, Type>(TypeFactory<Exhibit>.GetType)));
			gameRunController.ExhibitRecord.AddRange(data.ExhibitRecord);
			foreach (string text2 in data.AdventureHistory)
			{
				Type type2 = TypeFactory<Adventure>.TryGetType(text2);
				if (type2 != null)
				{
					gameRunController.AdventureHistory.Add(type2);
				}
			}
			gameRunController.Player.EnterGameRun(gameRunController);
			foreach (ExhibitSaveData exhibitSaveData in data.Exhibits)
			{
				Exhibit exhibit = Library.CreateExhibit(exhibitSaveData.Name);
				if (exhibit.HasCounter)
				{
					exhibit.Counter = exhibitSaveData.Counter.Value;
				}
				exhibit.CardInstanceId = exhibitSaveData.CardInstanceId;
				exhibit.GameRun = gameRunController;
				gameRunController.Player.AddExhibit(exhibit);
			}
			foreach (JadeBoxSaveData jadeBoxSaveData in data.JadeBoxes)
			{
				JadeBox jadeBox = Library.CreateJadeBox(jadeBoxSaveData.Name);
				jadeBox.GameRun = gameRunController;
				gameRunController._jadeBoxes.Add(jadeBox);
				jadeBox.TriggerAdded();
			}
			if (data.Timing == SaveTiming.EnterMapNode)
			{
				if (data.EnteringNode == null)
				{
					throw new InvalidDataException(string.Format("Entering node is null while SaveTiming = {0}", data.Timing));
				}
				gameRunController.CurrentMap.SetAdjacentNodesStatus(gameRunController.MapMode);
			}
			else
			{
				SaveTiming timing = data.Timing;
				if (timing == SaveTiming.BattleFinish || timing == SaveTiming.AfterBossReward)
				{
					StationType? stationType = data.EnteredStationType;
					if (stationType == null)
					{
						throw new InvalidDataException(string.Format("Entered station type is null while SaveTiming = {0}", data.Timing));
					}
					StationType valueOrDefault2 = stationType.GetValueOrDefault();
					if (data.Path == null || data.Path.Count == 0)
					{
						throw new InvalidDataException(string.Format("Entered station with no path while SaveTiming = {0}", data.Timing));
					}
					MapNodeSaveData mapNodeSaveData = Enumerable.Last<MapNodeSaveData>(data.Path);
					MapNode mapNode = gameRunController.CurrentMap.Nodes[mapNodeSaveData.X, mapNodeSaveData.Y];
					BattleStation battleStation = gameRunController.CurrentStage.CreateStationFromType(mapNode, valueOrDefault2) as BattleStation;
					if (battleStation == null)
					{
						throw new InvalidDataException(string.Format("Entered non-battle station while SaveTiming = {0}", data.Timing));
					}
					string battleStationEnemyGroup = data.BattleStationEnemyGroup;
					if (battleStationEnemyGroup == null)
					{
						throw new InvalidDataException(string.Format("Entered battle station with out enemy group while SaveTiming = {0}", data.Timing));
					}
					battleStation.EnemyGroupEntry = Library.TryGetEnemyGroupEntry(battleStationEnemyGroup);
					if (battleStation.EnemyGroupEntry == null)
					{
						throw new InvalidDataException(string.Format("Entered battle station enemy group {0} is not found while SaveTiming = {1}", battleStationEnemyGroup, data.Timing));
					}
					gameRunController.CurrentStation = battleStation;
					battleStation.ForceFinish();
					gameRunController.FinishStation(battleStation);
				}
				else if (data.Timing == SaveTiming.Adventure)
				{
					StationType? stationType = data.EnteredStationType;
					if (stationType == null)
					{
						throw new InvalidDataException(string.Format("Entered station type is null while SaveTiming = {0}", data.Timing));
					}
					StationType valueOrDefault3 = stationType.GetValueOrDefault();
					MapNodeSaveData mapNodeSaveData2 = Enumerable.Last<MapNodeSaveData>(data.Path);
					MapNode mapNode2 = gameRunController.CurrentMap.Nodes[mapNodeSaveData2.X, mapNodeSaveData2.Y];
					Station station = gameRunController.CurrentStage.CreateStationFromType(mapNode2, valueOrDefault3);
					Adventure adventure = Library.CreateAdventure(data.AdventureState.AdventureId);
					IAdventureStation adventureStation = station as IAdventureStation;
					if (adventureStation == null)
					{
						throw new InvalidDataException(string.Format("Entered non-adventure station while SaveTiming = {0}", data.Timing));
					}
					adventureStation.Restore(adventure);
					gameRunController.CurrentStation = station;
				}
			}
			gameRunController.PlayedSeconds = data.PlayedSeconds;
			gameRunController.Stats = data.Stats.Clone();
			gameRunController.ExtraFlags = Enumerable.ToHashSet<string>(data.ExtraFlags);
			gameRunController.StageRecords = Enumerable.ToList<StageRecord>(Enumerable.Select<StageRecord, StageRecord>(data.StageRecords, (StageRecord r) => r.Clone()));
			return gameRunController;
		}
		[CompilerGenerated]
		internal static CardWeightTable <GetRewardCards>g__ModifyRare|436_0(CardWeightTable table, float rareMultiplier)
		{
			RarityWeightTable rarityTable = table.RarityTable;
			float rare = rarityTable.Rare;
			return table.WithRarity(rarityTable.WithRare(rare * rareMultiplier));
		}
		[CompilerGenerated]
		internal static Type <Restore>g__GameEntityTypeConverter|550_0<T>(string s) where T : class
		{
			Type type = TypeFactory<T>.TryGetType(s);
			if (type == null)
			{
				Debug.LogWarning(string.Concat(new string[]
				{
					"Cannot find ",
					typeof(T).Name,
					" type '",
					s,
					"'"
				}));
			}
			return type;
		}
		[CompilerGenerated]
		internal static string <Restore>g__EnemyGroupConverter|550_1(string s)
		{
			if (Library.TryGetEnemyGroupEntry(s) == null)
			{
				Debug.LogWarning("Cannot find EnemyGroup '" + s + "'");
			}
			return s;
		}
		private List<Card> _baseDeck;
		private int _deckCardInstanceId;
		private List<Stage> _stages;
		private int _stageIndex = -1;
		private readonly List<JadeBox> _jadeBoxes = new List<JadeBox>();
		private GameRunMapMode _mapMode;
		private readonly List<IMapModeOverrider> _mapModeOverriders = new List<IMapModeOverrider>();
		private IMapModeOverrider _activeMapModeOverrider;
		private float _cardRareWeightFactor;
		private Dictionary<string, float> _cardRewardWeightFactors;
		private bool _cardRewardDecreaseRepeatRare;
		private const float CardRewardDecreaseRepeatRareMultiplier = 0.01f;
		private const float CardRareFactorDefaultValue = 0.85f;
		private const float CardRareFactorIncrement = 0.01f;
		private const float CardNormalAbandonMultiplier = 0.9f;
		private const float CardAllAbandonMultipler = 0.85f;
	}
}
