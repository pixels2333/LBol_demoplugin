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
	// Token: 0x02000043 RID: 67
	public sealed class GameRunController
	{
		// Token: 0x060001F7 RID: 503 RVA: 0x00005D40 File Offset: 0x00003F40
		public static GameRunController Create(GameRunStartupParameters parameters)
		{
			return new GameRunController(parameters);
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x00005D48 File Offset: 0x00003F48
		private GameRunController()
		{
			this.DialogLibrary = new DialogFunctions(this).ToLibrary();
		}

		// Token: 0x060001F9 RID: 505 RVA: 0x00005EC0 File Offset: 0x000040C0
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

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x060001FA RID: 506 RVA: 0x000066A4 File Offset: 0x000048A4
		public InteractionViewer InteractionViewer { get; } = new InteractionViewer();

		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x060001FB RID: 507 RVA: 0x000066AC File Offset: 0x000048AC
		// (set) Token: 0x060001FC RID: 508 RVA: 0x000066B4 File Offset: 0x000048B4
		public IGameRunAchievementHandler AchievementHandler { get; set; }

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x060001FD RID: 509 RVA: 0x000066C0 File Offset: 0x000048C0
		// (remove) Token: 0x060001FE RID: 510 RVA: 0x000066F8 File Offset: 0x000048F8
		public event Action<Card> CardRevealed;

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x060001FF RID: 511 RVA: 0x00006730 File Offset: 0x00004930
		// (remove) Token: 0x06000200 RID: 512 RVA: 0x00006768 File Offset: 0x00004968
		public event Action<Exhibit> ExhibitRevealed;

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x06000201 RID: 513 RVA: 0x000067A0 File Offset: 0x000049A0
		// (remove) Token: 0x06000202 RID: 514 RVA: 0x000067D8 File Offset: 0x000049D8
		public event Action<EnemyGroup> EnemyGroupRevealed;

		// Token: 0x06000203 RID: 515 RVA: 0x0000680D File Offset: 0x00004A0D
		internal void RevealCard(Card card)
		{
			Action<Card> cardRevealed = this.CardRevealed;
			if (cardRevealed == null)
			{
				return;
			}
			cardRevealed.Invoke(card);
		}

		// Token: 0x06000204 RID: 516 RVA: 0x00006820 File Offset: 0x00004A20
		public void RevealExhibit(Exhibit exhibit)
		{
			Action<Exhibit> exhibitRevealed = this.ExhibitRevealed;
			if (exhibitRevealed == null)
			{
				return;
			}
			exhibitRevealed.Invoke(exhibit);
		}

		// Token: 0x06000205 RID: 517 RVA: 0x00006833 File Offset: 0x00004A33
		internal void RevealEnemyGroup(EnemyGroup enemyGroup)
		{
			Action<EnemyGroup> enemyGroupRevealed = this.EnemyGroupRevealed;
			if (enemyGroupRevealed == null)
			{
				return;
			}
			enemyGroupRevealed.Invoke(enemyGroup);
		}

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x06000206 RID: 518 RVA: 0x00006846 File Offset: 0x00004A46
		// (set) Token: 0x06000207 RID: 519 RVA: 0x0000684E File Offset: 0x00004A4E
		public IGameRunVisualTrigger VisualTrigger { get; set; }

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x06000208 RID: 520 RVA: 0x00006857 File Offset: 0x00004A57
		// (set) Token: 0x06000209 RID: 521 RVA: 0x0000685F File Offset: 0x00004A5F
		public GameMode Mode { get; private set; }

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x0600020A RID: 522 RVA: 0x00006868 File Offset: 0x00004A68
		// (set) Token: 0x0600020B RID: 523 RVA: 0x00006870 File Offset: 0x00004A70
		public GameDifficulty Difficulty { get; private set; }

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x0600020C RID: 524 RVA: 0x00006879 File Offset: 0x00004A79
		public bool KaguyaInDebut
		{
			get
			{
				return this.Difficulty == GameDifficulty.Easy || this.HasJadeBox<StartWithMythic>();
			}
		}

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x0600020D RID: 525 RVA: 0x0000688B File Offset: 0x00004A8B
		// (set) Token: 0x0600020E RID: 526 RVA: 0x00006893 File Offset: 0x00004A93
		public PuzzleFlag Puzzles { get; private set; }

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x0600020F RID: 527 RVA: 0x0000689C File Offset: 0x00004A9C
		// (set) Token: 0x06000210 RID: 528 RVA: 0x000068A4 File Offset: 0x00004AA4
		public GameRunStatus Status { get; internal set; }

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x06000211 RID: 529 RVA: 0x000068AD File Offset: 0x00004AAD
		// (set) Token: 0x06000212 RID: 530 RVA: 0x000068B5 File Offset: 0x00004AB5
		public bool IsNormalEndFinished { get; private set; }

		// Token: 0x170000AD RID: 173
		// (get) Token: 0x06000213 RID: 531 RVA: 0x000068BE File Offset: 0x00004ABE
		// (set) Token: 0x06000214 RID: 532 RVA: 0x000068C6 File Offset: 0x00004AC6
		public ulong RootSeed { get; private set; }

		// Token: 0x170000AE RID: 174
		// (get) Token: 0x06000215 RID: 533 RVA: 0x000068CF File Offset: 0x00004ACF
		// (set) Token: 0x06000216 RID: 534 RVA: 0x000068D7 File Offset: 0x00004AD7
		public bool IsAutoSeed { get; private set; }

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x06000217 RID: 535 RVA: 0x000068E0 File Offset: 0x00004AE0
		// (set) Token: 0x06000218 RID: 536 RVA: 0x000068E8 File Offset: 0x00004AE8
		public RandomGen RootRng { get; private set; }

		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x06000219 RID: 537 RVA: 0x000068F1 File Offset: 0x00004AF1
		// (set) Token: 0x0600021A RID: 538 RVA: 0x000068F9 File Offset: 0x00004AF9
		public RandomGen StationRng { get; private set; }

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x0600021B RID: 539 RVA: 0x00006902 File Offset: 0x00004B02
		// (set) Token: 0x0600021C RID: 540 RVA: 0x0000690A File Offset: 0x00004B0A
		public ulong InitBossSeed { get; private set; }

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x0600021D RID: 541 RVA: 0x00006913 File Offset: 0x00004B13
		// (set) Token: 0x0600021E RID: 542 RVA: 0x0000691B File Offset: 0x00004B1B
		public RandomGen ShopRng { get; private set; }

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x0600021F RID: 543 RVA: 0x00006924 File Offset: 0x00004B24
		// (set) Token: 0x06000220 RID: 544 RVA: 0x0000692C File Offset: 0x00004B2C
		public RandomGen AdventureRng { get; private set; }

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x06000221 RID: 545 RVA: 0x00006935 File Offset: 0x00004B35
		// (set) Token: 0x06000222 RID: 546 RVA: 0x0000693D File Offset: 0x00004B3D
		public RandomGen ExhibitRng { get; private set; }

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x06000223 RID: 547 RVA: 0x00006946 File Offset: 0x00004B46
		// (set) Token: 0x06000224 RID: 548 RVA: 0x0000694E File Offset: 0x00004B4E
		public RandomGen ShinningExhibitRng { get; private set; }

		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x06000225 RID: 549 RVA: 0x00006957 File Offset: 0x00004B57
		// (set) Token: 0x06000226 RID: 550 RVA: 0x0000695F File Offset: 0x00004B5F
		public RandomGen CardRng { get; private set; }

		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x06000227 RID: 551 RVA: 0x00006968 File Offset: 0x00004B68
		// (set) Token: 0x06000228 RID: 552 RVA: 0x00006970 File Offset: 0x00004B70
		public RandomGen GameRunEventRng { get; private set; }

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x06000229 RID: 553 RVA: 0x00006979 File Offset: 0x00004B79
		// (set) Token: 0x0600022A RID: 554 RVA: 0x00006981 File Offset: 0x00004B81
		public RandomGen BattleRng { get; private set; }

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x0600022B RID: 555 RVA: 0x0000698A File Offset: 0x00004B8A
		// (set) Token: 0x0600022C RID: 556 RVA: 0x00006992 File Offset: 0x00004B92
		public RandomGen BattleCardRng { get; private set; }

		// Token: 0x170000BA RID: 186
		// (get) Token: 0x0600022D RID: 557 RVA: 0x0000699B File Offset: 0x00004B9B
		// (set) Token: 0x0600022E RID: 558 RVA: 0x000069A3 File Offset: 0x00004BA3
		public RandomGen ShuffleRng { get; private set; }

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x0600022F RID: 559 RVA: 0x000069AC File Offset: 0x00004BAC
		// (set) Token: 0x06000230 RID: 560 RVA: 0x000069B4 File Offset: 0x00004BB4
		public RandomGen EnemyMoveRng { get; private set; }

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x06000231 RID: 561 RVA: 0x000069BD File Offset: 0x00004BBD
		// (set) Token: 0x06000232 RID: 562 RVA: 0x000069C5 File Offset: 0x00004BC5
		public RandomGen EnemyBattleRng { get; private set; }

		// Token: 0x170000BD RID: 189
		// (get) Token: 0x06000233 RID: 563 RVA: 0x000069CE File Offset: 0x00004BCE
		// (set) Token: 0x06000234 RID: 564 RVA: 0x000069D6 File Offset: 0x00004BD6
		public RandomGen DebutRng { get; private set; }

		// Token: 0x170000BE RID: 190
		// (get) Token: 0x06000235 RID: 565 RVA: 0x000069DF File Offset: 0x00004BDF
		// (set) Token: 0x06000236 RID: 566 RVA: 0x000069E7 File Offset: 0x00004BE7
		public ulong FinalBossSeed { get; private set; }

		// Token: 0x170000BF RID: 191
		// (get) Token: 0x06000237 RID: 567 RVA: 0x000069F0 File Offset: 0x00004BF0
		// (set) Token: 0x06000238 RID: 568 RVA: 0x000069F8 File Offset: 0x00004BF8
		public ulong UISeed { get; private set; }

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x06000239 RID: 569 RVA: 0x00006A01 File Offset: 0x00004C01
		// (set) Token: 0x0600023A RID: 570 RVA: 0x00006A09 File Offset: 0x00004C09
		public int CardValidDebugLevel { get; private set; }

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x0600023B RID: 571 RVA: 0x00006A12 File Offset: 0x00004C12
		public Library DialogLibrary { get; }

		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x0600023C RID: 572 RVA: 0x00006A1A File Offset: 0x00004C1A
		// (set) Token: 0x0600023D RID: 573 RVA: 0x00006A22 File Offset: 0x00004C22
		public bool HasClearBonus { get; private set; }

		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x0600023E RID: 574 RVA: 0x00006A2B File Offset: 0x00004C2B
		// (set) Token: 0x0600023F RID: 575 RVA: 0x00006A33 File Offset: 0x00004C33
		public int UnlockLevel { get; private set; }

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x06000240 RID: 576 RVA: 0x00006A3C File Offset: 0x00004C3C
		// (set) Token: 0x06000241 RID: 577 RVA: 0x00006A44 File Offset: 0x00004C44
		public PlayerUnit Player { get; private set; }

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x06000242 RID: 578 RVA: 0x00006A4D File Offset: 0x00004C4D
		// (set) Token: 0x06000243 RID: 579 RVA: 0x00006A55 File Offset: 0x00004C55
		public PlayerType PlayerType { get; private set; }

		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x06000244 RID: 580 RVA: 0x00006A5E File Offset: 0x00004C5E
		// (set) Token: 0x06000245 RID: 581 RVA: 0x00006A66 File Offset: 0x00004C66
		public ManaGroup BaseMana { get; private set; }

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x06000246 RID: 582 RVA: 0x00006A6F File Offset: 0x00004C6F
		public IReadOnlyList<Card> BaseDeck
		{
			get
			{
				return this._baseDeck.AsReadOnly();
			}
		}

		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x06000247 RID: 583 RVA: 0x00006A7C File Offset: 0x00004C7C
		public IEnumerable<Card> BaseDeckWithoutUnremovable
		{
			get
			{
				return Enumerable.Where<Card>(this._baseDeck, (Card card) => !card.Unremovable);
			}
		}

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x06000248 RID: 584 RVA: 0x00006AA8 File Offset: 0x00004CA8
		public IEnumerable<Card> BaseDeckInBossRemoveReward
		{
			get
			{
				return Enumerable.Where<Card>(this._baseDeck, (Card card) => !card.IsBasic && !card.Negative);
			}
		}

		// Token: 0x170000CA RID: 202
		// (get) Token: 0x06000249 RID: 585 RVA: 0x00006AD4 File Offset: 0x00004CD4
		// (set) Token: 0x0600024A RID: 586 RVA: 0x00006ADC File Offset: 0x00004CDC
		public int Money { get; private set; }

		// Token: 0x170000CB RID: 203
		// (get) Token: 0x0600024B RID: 587 RVA: 0x00006AE5 File Offset: 0x00004CE5
		// (set) Token: 0x0600024C RID: 588 RVA: 0x00006AED File Offset: 0x00004CED
		public int TotalMoney { get; private set; }

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x0600024D RID: 589 RVA: 0x00006AF6 File Offset: 0x00004CF6
		// (set) Token: 0x0600024E RID: 590 RVA: 0x00006AFE File Offset: 0x00004CFE
		public int UltimateUseCount { get; set; }

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x0600024F RID: 591 RVA: 0x00006B07 File Offset: 0x00004D07
		// (set) Token: 0x06000250 RID: 592 RVA: 0x00006B0F File Offset: 0x00004D0F
		public int ReloadTimes { get; private set; }

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x06000251 RID: 593 RVA: 0x00006B18 File Offset: 0x00004D18
		// (set) Token: 0x06000252 RID: 594 RVA: 0x00006B20 File Offset: 0x00004D20
		public bool ShowRandomResult { get; private set; }

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x06000253 RID: 595 RVA: 0x00006B29 File Offset: 0x00004D29
		// (set) Token: 0x06000254 RID: 596 RVA: 0x00006B31 File Offset: 0x00004D31
		public BattleController Battle { get; private set; }

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x06000255 RID: 597 RVA: 0x00006B3A File Offset: 0x00004D3A
		public IReadOnlyList<Stage> Stages
		{
			get
			{
				return this._stages.AsReadOnly();
			}
		}

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x06000256 RID: 598 RVA: 0x00006B47 File Offset: 0x00004D47
		public IReadOnlyList<JadeBox> JadeBoxes
		{
			get
			{
				return this._jadeBoxes.AsReadOnly();
			}
		}

		// Token: 0x06000257 RID: 599 RVA: 0x00006B54 File Offset: 0x00004D54
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

		// Token: 0x06000258 RID: 600 RVA: 0x00006B90 File Offset: 0x00004D90
		public bool HasJadeBox(Type type)
		{
			return Enumerable.Any<JadeBox>(this._jadeBoxes, (JadeBox jadeBox) => jadeBox.GetType() == type);
		}

		// Token: 0x06000259 RID: 601 RVA: 0x00006BC1 File Offset: 0x00004DC1
		public bool HasJadeBox<T>() where T : JadeBox
		{
			return this.HasJadeBox(typeof(T));
		}

		// Token: 0x0600025A RID: 602 RVA: 0x00006BD3 File Offset: 0x00004DD3
		public bool HasJadeBox(JadeBox jadeBox)
		{
			return this._jadeBoxes.Contains(jadeBox);
		}

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x0600025B RID: 603 RVA: 0x00006BE1 File Offset: 0x00004DE1
		// (set) Token: 0x0600025C RID: 604 RVA: 0x00006BE9 File Offset: 0x00004DE9
		public int PlayedSeconds { get; private set; }

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x0600025D RID: 605 RVA: 0x00006BF2 File Offset: 0x00004DF2
		// (set) Token: 0x0600025E RID: 606 RVA: 0x00006BFA File Offset: 0x00004DFA
		public GameRunStats Stats { get; private set; }

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x0600025F RID: 607 RVA: 0x00006C03 File Offset: 0x00004E03
		// (set) Token: 0x06000260 RID: 608 RVA: 0x00006C0B File Offset: 0x00004E0B
		public HashSet<string> ExtraFlags { get; private set; } = new HashSet<string>();

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x06000261 RID: 609 RVA: 0x00006C14 File Offset: 0x00004E14
		// (set) Token: 0x06000262 RID: 610 RVA: 0x00006C1C File Offset: 0x00004E1C
		public List<StageRecord> StageRecords { get; private set; } = new List<StageRecord>();

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x06000263 RID: 611 RVA: 0x00006C25 File Offset: 0x00004E25
		// (set) Token: 0x06000264 RID: 612 RVA: 0x00006C2D File Offset: 0x00004E2D
		public Stage CurrentStage { get; private set; }

		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x06000265 RID: 613 RVA: 0x00006C36 File Offset: 0x00004E36
		// (set) Token: 0x06000266 RID: 614 RVA: 0x00006C3E File Offset: 0x00004E3E
		public GameMap CurrentMap { get; private set; }

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x06000267 RID: 615 RVA: 0x00006C47 File Offset: 0x00004E47
		public GameRunMapMode MapMode
		{
			get
			{
				return this._mapMode;
			}
		}

		// Token: 0x06000268 RID: 616 RVA: 0x00006C50 File Offset: 0x00004E50
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

		// Token: 0x06000269 RID: 617 RVA: 0x00006D14 File Offset: 0x00004F14
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

		// Token: 0x0600026A RID: 618 RVA: 0x00006DA1 File Offset: 0x00004FA1
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

		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x0600026B RID: 619 RVA: 0x00006DD1 File Offset: 0x00004FD1
		// (set) Token: 0x0600026C RID: 620 RVA: 0x00006DD9 File Offset: 0x00004FD9
		public Station CurrentStation { get; private set; }

		// Token: 0x170000DA RID: 218
		// (get) Token: 0x0600026D RID: 621 RVA: 0x00006DE2 File Offset: 0x00004FE2
		public int DrinkTeaHealRate
		{
			get
			{
				return 30;
			}
		}

		// Token: 0x170000DB RID: 219
		// (get) Token: 0x0600026E RID: 622 RVA: 0x00006DE6 File Offset: 0x00004FE6
		// (set) Token: 0x0600026F RID: 623 RVA: 0x00006DEE File Offset: 0x00004FEE
		public int DrinkTeaAdditionalHeal { get; set; }

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x06000270 RID: 624 RVA: 0x00006DF7 File Offset: 0x00004FF7
		// (set) Token: 0x06000271 RID: 625 RVA: 0x00006DFF File Offset: 0x00004FFF
		public int DrinkTeaAdditionalEnergy { get; set; }

		// Token: 0x170000DD RID: 221
		// (get) Token: 0x06000272 RID: 626 RVA: 0x00006E08 File Offset: 0x00005008
		// (set) Token: 0x06000273 RID: 627 RVA: 0x00006E10 File Offset: 0x00005010
		public int DrinkTeaCardRewardFlag { get; set; }

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x06000274 RID: 628 RVA: 0x00006E19 File Offset: 0x00005019
		public int DrinkTeaHealValue
		{
			get
			{
				return ((float)this.Player.MaxHp * ((float)this.DrinkTeaHealRate / 100f)).RoundToInt();
			}
		}

		// Token: 0x170000DF RID: 223
		// (get) Token: 0x06000275 RID: 629 RVA: 0x00006E3A File Offset: 0x0000503A
		public float PowerGainRate
		{
			get
			{
				return 1f;
			}
		}

		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x06000276 RID: 630 RVA: 0x00006E41 File Offset: 0x00005041
		// (set) Token: 0x06000277 RID: 631 RVA: 0x00006E49 File Offset: 0x00005049
		public int RewardAndShopCardColorLimitFlag { get; set; }

		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x06000278 RID: 632 RVA: 0x00006E52 File Offset: 0x00005052
		// (set) Token: 0x06000279 RID: 633 RVA: 0x00006E5A File Offset: 0x0000505A
		public int AdditionalRewardCardCount { get; set; }

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x0600027A RID: 634 RVA: 0x00006E63 File Offset: 0x00005063
		// (set) Token: 0x0600027B RID: 635 RVA: 0x00006E6B File Offset: 0x0000506B
		public int WanbaochuiFlag { get; set; }

		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x0600027C RID: 636 RVA: 0x00006E74 File Offset: 0x00005074
		// (set) Token: 0x0600027D RID: 637 RVA: 0x00006E7C File Offset: 0x0000507C
		public int BasicCardIncrease { get; set; }

		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x0600027E RID: 638 RVA: 0x00006E85 File Offset: 0x00005085
		// (set) Token: 0x0600027F RID: 639 RVA: 0x00006E8D File Offset: 0x0000508D
		public int YichuiPiaoFlag { get; set; }

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x06000280 RID: 640 RVA: 0x00006E96 File Offset: 0x00005096
		// (set) Token: 0x06000281 RID: 641 RVA: 0x00006E9E File Offset: 0x0000509E
		public int AllCharacterCardsFlag { get; set; }

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x06000282 RID: 642 RVA: 0x00006EA7 File Offset: 0x000050A7
		// (set) Token: 0x06000283 RID: 643 RVA: 0x00006EAF File Offset: 0x000050AF
		public int CanViewDrawZoneActualOrder { get; set; }

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x06000284 RID: 644 RVA: 0x00006EB8 File Offset: 0x000050B8
		// (set) Token: 0x06000285 RID: 645 RVA: 0x00006EC0 File Offset: 0x000050C0
		public int RemoveBadCardForbidden { get; set; }

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x06000286 RID: 646 RVA: 0x00006EC9 File Offset: 0x000050C9
		// (set) Token: 0x06000287 RID: 647 RVA: 0x00006ED1 File Offset: 0x000050D1
		public int LootCardCommonDupeCount { get; set; }

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x06000288 RID: 648 RVA: 0x00006EDA File Offset: 0x000050DA
		// (set) Token: 0x06000289 RID: 649 RVA: 0x00006EE2 File Offset: 0x000050E2
		public int LootCardUncommonDupeCount { get; set; }

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x0600028A RID: 650 RVA: 0x00006EEB File Offset: 0x000050EB
		public int RewardCardCount
		{
			get
			{
				return (3 + this.AdditionalRewardCardCount).Clamp(0, 5);
			}
		}

		// Token: 0x170000EB RID: 235
		// (get) Token: 0x0600028B RID: 651 RVA: 0x00006EFC File Offset: 0x000050FC
		// (set) Token: 0x0600028C RID: 652 RVA: 0x00006F04 File Offset: 0x00005104
		public int RewardCardAbandonMoney { get; set; } = 5;

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x0600028D RID: 653 RVA: 0x00006F0D File Offset: 0x0000510D
		// (set) Token: 0x0600028E RID: 654 RVA: 0x00006F15 File Offset: 0x00005115
		public float RewardMoneyMultiplier { get; set; } = 1f;

		// Token: 0x170000ED RID: 237
		// (get) Token: 0x0600028F RID: 655 RVA: 0x00006F1E File Offset: 0x0000511E
		// (set) Token: 0x06000290 RID: 656 RVA: 0x00006F26 File Offset: 0x00005126
		public int ExtraPowerLowerbound { get; set; }

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x06000291 RID: 657 RVA: 0x00006F2F File Offset: 0x0000512F
		// (set) Token: 0x06000292 RID: 658 RVA: 0x00006F37 File Offset: 0x00005137
		public int ExtraPowerUpperbound { get; set; }

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x06000293 RID: 659 RVA: 0x00006F40 File Offset: 0x00005140
		// (set) Token: 0x06000294 RID: 660 RVA: 0x00006F48 File Offset: 0x00005148
		public int UpgradeNewDeckAttackCardFlag { get; set; }

		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x06000295 RID: 661 RVA: 0x00006F51 File Offset: 0x00005151
		// (set) Token: 0x06000296 RID: 662 RVA: 0x00006F59 File Offset: 0x00005159
		public int UpgradeNewDeckDefenseCardFlag { get; set; }

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x06000297 RID: 663 RVA: 0x00006F62 File Offset: 0x00005162
		// (set) Token: 0x06000298 RID: 664 RVA: 0x00006F6A File Offset: 0x0000516A
		public int UpgradeNewDeckSkillCardFlag { get; set; }

		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x06000299 RID: 665 RVA: 0x00006F73 File Offset: 0x00005173
		// (set) Token: 0x0600029A RID: 666 RVA: 0x00006F7B File Offset: 0x0000517B
		public int UpgradeNewDeckAbilityCardFlag { get; set; }

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x0600029B RID: 667 RVA: 0x00006F84 File Offset: 0x00005184
		// (set) Token: 0x0600029C RID: 668 RVA: 0x00006F8C File Offset: 0x0000518C
		public int FragilExtraPercentage { get; set; }

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x0600029D RID: 669 RVA: 0x00006F95 File Offset: 0x00005195
		// (set) Token: 0x0600029E RID: 670 RVA: 0x00006F9D File Offset: 0x0000519D
		public int WeakExtraPercentage { get; set; }

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x0600029F RID: 671 RVA: 0x00006FA6 File Offset: 0x000051A6
		// (set) Token: 0x060002A0 RID: 672 RVA: 0x00006FAE File Offset: 0x000051AE
		public int EnemyVulnerableExtraPercentage { get; set; }

		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x060002A1 RID: 673 RVA: 0x00006FB7 File Offset: 0x000051B7
		// (set) Token: 0x060002A2 RID: 674 RVA: 0x00006FBF File Offset: 0x000051BF
		public int PlayerVulnerableExtraPercentage { get; set; }

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x060002A3 RID: 675 RVA: 0x00006FC8 File Offset: 0x000051C8
		// (set) Token: 0x060002A4 RID: 676 RVA: 0x00006FD0 File Offset: 0x000051D0
		public int BasicAttackCardExtraDamage1 { get; set; }

		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x060002A5 RID: 677 RVA: 0x00006FD9 File Offset: 0x000051D9
		// (set) Token: 0x060002A6 RID: 678 RVA: 0x00006FE1 File Offset: 0x000051E1
		public int BasicAttackCardExtraDamage2 { get; set; }

		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x060002A7 RID: 679 RVA: 0x00006FEA File Offset: 0x000051EA
		// (set) Token: 0x060002A8 RID: 680 RVA: 0x00006FF2 File Offset: 0x000051F2
		public int FinalBossInitialDamage { get; set; }

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x060002A9 RID: 681 RVA: 0x00006FFB File Offset: 0x000051FB
		// (set) Token: 0x060002AA RID: 682 RVA: 0x00007003 File Offset: 0x00005203
		public Exhibit ExtraExhibitReward { get; set; }

		// Token: 0x170000FB RID: 251
		// (get) Token: 0x060002AB RID: 683 RVA: 0x0000700C File Offset: 0x0000520C
		// (set) Token: 0x060002AC RID: 684 RVA: 0x00007014 File Offset: 0x00005214
		public int ExtraLoyalty { get; set; }

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x060002AD RID: 685 RVA: 0x0000701D File Offset: 0x0000521D
		public HashSet<GameEntity> TrueEndingBlockers { get; } = new HashSet<GameEntity>();

		// Token: 0x170000FD RID: 253
		// (get) Token: 0x060002AE RID: 686 RVA: 0x00007025 File Offset: 0x00005225
		public HashSet<GameEntity> TrueEndingProviders { get; } = new HashSet<GameEntity>();

		// Token: 0x060002AF RID: 687 RVA: 0x0000702D File Offset: 0x0000522D
		public bool CanEnterTrueEnding()
		{
			return this.TrueEndingBlockers.Empty<GameEntity>() && !this.TrueEndingProviders.Empty<GameEntity>();
		}

		// Token: 0x060002B0 RID: 688 RVA: 0x0000704C File Offset: 0x0000524C
		public bool IsTrueEndingBlocked()
		{
			return !this.TrueEndingProviders.Empty<GameEntity>() && !this.TrueEndingBlockers.Empty<GameEntity>();
		}

		// Token: 0x060002B1 RID: 689 RVA: 0x0000706C File Offset: 0x0000526C
		public void UpgradeNewDeckCardOnFlags(IEnumerable<Card> cards)
		{
			foreach (Card card in cards)
			{
				this.UpgradeNewDeckCardOnFlags(card);
			}
		}

		// Token: 0x060002B2 RID: 690 RVA: 0x000070B4 File Offset: 0x000052B4
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

		// Token: 0x060002B3 RID: 691 RVA: 0x0000714B File Offset: 0x0000534B
		public int ModifyMoneyReward(int money)
		{
			return ((float)money * this.RewardMoneyMultiplier).RoundToInt();
		}

		// Token: 0x060002B4 RID: 692 RVA: 0x0000715B File Offset: 0x0000535B
		public int ModifyMoneyReward(float money)
		{
			return (money * this.RewardMoneyMultiplier).RoundToInt();
		}

		// Token: 0x170000FE RID: 254
		// (get) Token: 0x060002B5 RID: 693 RVA: 0x0000716A File Offset: 0x0000536A
		// (set) Token: 0x060002B6 RID: 694 RVA: 0x00007172 File Offset: 0x00005372
		public float ShopPriceMultiplier { get; set; } = 1f;

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x060002B7 RID: 695 RVA: 0x0000717B File Offset: 0x0000537B
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

		// Token: 0x17000100 RID: 256
		// (get) Token: 0x060002B8 RID: 696 RVA: 0x000071A9 File Offset: 0x000053A9
		// (set) Token: 0x060002B9 RID: 697 RVA: 0x000071B1 File Offset: 0x000053B1
		public int ShopResupplyFlag { get; set; }

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x060002BA RID: 698 RVA: 0x000071BA File Offset: 0x000053BA
		// (set) Token: 0x060002BB RID: 699 RVA: 0x000071C2 File Offset: 0x000053C2
		public int ShopRemoveCardCounter { get; set; }

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x060002BC RID: 700 RVA: 0x000071CB File Offset: 0x000053CB
		// (set) Token: 0x060002BD RID: 701 RVA: 0x000071D3 File Offset: 0x000053D3
		public int? ShopRemoveCardPriceOverride { get; set; }

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x060002BE RID: 702 RVA: 0x000071DC File Offset: 0x000053DC
		public int UpgradeDeckCardPrice
		{
			get
			{
				return 50;
			}
		}

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x060002BF RID: 703 RVA: 0x000071E0 File Offset: 0x000053E0
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

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x060002C0 RID: 704 RVA: 0x00007211 File Offset: 0x00005411
		// (set) Token: 0x060002C1 RID: 705 RVA: 0x00007219 File Offset: 0x00005419
		public int DrawCardCount { get; set; } = 5;

		// Token: 0x17000106 RID: 262
		// (get) Token: 0x060002C2 RID: 706 RVA: 0x00007222 File Offset: 0x00005422
		// (set) Token: 0x060002C3 RID: 707 RVA: 0x0000722A File Offset: 0x0000542A
		public int SynergyAdditionalCount { get; set; }

		// Token: 0x17000107 RID: 263
		// (get) Token: 0x060002C4 RID: 708 RVA: 0x00007233 File Offset: 0x00005433
		// (set) Token: 0x060002C5 RID: 709 RVA: 0x0000723B File Offset: 0x0000543B
		public List<Type> ShiningExhibitPool { get; private set; }

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x060002C6 RID: 710 RVA: 0x00007244 File Offset: 0x00005444
		// (set) Token: 0x060002C7 RID: 711 RVA: 0x0000724C File Offset: 0x0000544C
		public List<Type> ExhibitPool { get; private set; }

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x060002C8 RID: 712 RVA: 0x00007255 File Offset: 0x00005455
		public List<string> ExhibitRecord { get; } = new List<string>();

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x060002C9 RID: 713 RVA: 0x0000725D File Offset: 0x0000545D
		public List<Type> AdventureHistory { get; } = new List<Type>();

		// Token: 0x060002CA RID: 714 RVA: 0x00007268 File Offset: 0x00005468
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

		// Token: 0x060002CB RID: 715 RVA: 0x00007300 File Offset: 0x00005500
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

		// Token: 0x060002CC RID: 716 RVA: 0x00007380 File Offset: 0x00005580
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

		// Token: 0x060002CD RID: 717 RVA: 0x00007400 File Offset: 0x00005600
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

		// Token: 0x060002CE RID: 718 RVA: 0x00007448 File Offset: 0x00005648
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

		// Token: 0x060002CF RID: 719 RVA: 0x00007610 File Offset: 0x00005810
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

		// Token: 0x060002D0 RID: 720 RVA: 0x000076B8 File Offset: 0x000058B8
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

		// Token: 0x060002D1 RID: 721 RVA: 0x00007950 File Offset: 0x00005B50
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

		// Token: 0x060002D2 RID: 722 RVA: 0x000079B8 File Offset: 0x00005BB8
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

		// Token: 0x060002D3 RID: 723 RVA: 0x00007A74 File Offset: 0x00005C74
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

		// Token: 0x060002D4 RID: 724 RVA: 0x00007B60 File Offset: 0x00005D60
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

		// Token: 0x060002D5 RID: 725 RVA: 0x00007C18 File Offset: 0x00005E18
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

		// Token: 0x060002D6 RID: 726 RVA: 0x00007EDC File Offset: 0x000060DC
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

		// Token: 0x060002D7 RID: 727 RVA: 0x00008124 File Offset: 0x00006324
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

		// Token: 0x060002D8 RID: 728 RVA: 0x0000835C File Offset: 0x0000655C
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

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x060002D9 RID: 729 RVA: 0x0000865C File Offset: 0x0000685C
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

		// Token: 0x060002DA RID: 730 RVA: 0x00008694 File Offset: 0x00006894
		internal Card[] GetShopCards(int count, CardWeightTable weightTable, List<string> exclude = null)
		{
			Card[] array = this.RollCards(this.ShopRng, weightTable, count, new ManaGroup?(this.BaseMana), this.RewardAndShopCardColorLimitFlag == 0, true, false, false, (CardConfig config) => exclude == null || !exclude.Contains(config.Id));
			foreach (Card card in array)
			{
				this.UpgradeNewDeckCardOnFlags(card);
			}
			return array;
		}

		// Token: 0x060002DB RID: 731 RVA: 0x00008700 File Offset: 0x00006900
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

		// Token: 0x060002DC RID: 732 RVA: 0x00008758 File Offset: 0x00006958
		public Card[] RollCards(RandomGen rng, CardWeightTable weightTable, int count, bool applyFactors = false, bool battleRolling = false, [MaybeNull] Predicate<CardConfig> filter = null)
		{
			return this.RollCards(rng, weightTable, count, new ManaGroup?(this.BaseMana), this.RewardAndShopCardColorLimitFlag == 0, applyFactors, battleRolling, false, filter);
		}

		// Token: 0x060002DD RID: 733 RVA: 0x00008789 File Offset: 0x00006989
		public Card RollCard(RandomGen rng, CardWeightTable weightTable, bool applyFactors = false, bool battleRolling = false, [MaybeNull] Predicate<CardConfig> filter = null)
		{
			return Enumerable.FirstOrDefault<Card>(this.RollCards(rng, weightTable, 1, applyFactors, battleRolling, filter));
		}

		// Token: 0x060002DE RID: 734 RVA: 0x000087A0 File Offset: 0x000069A0
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

		// Token: 0x060002DF RID: 735 RVA: 0x00008850 File Offset: 0x00006A50
		public Card[] RollCardsWithoutManaLimit(RandomGen rng, CardWeightTable weightTable, int count, bool applyFactors = false, bool battleRolling = false, [MaybeNull] Predicate<CardConfig> filter = null)
		{
			return this.RollCards(rng, weightTable, count, default(ManaGroup?), false, applyFactors, battleRolling, false, filter);
		}

		// Token: 0x060002E0 RID: 736 RVA: 0x00008878 File Offset: 0x00006A78
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

		// Token: 0x060002E1 RID: 737 RVA: 0x0000891C File Offset: 0x00006B1C
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

		// Token: 0x060002E2 RID: 738 RVA: 0x00008990 File Offset: 0x00006B90
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

		// Token: 0x060002E3 RID: 739 RVA: 0x000089F8 File Offset: 0x00006BF8
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

		// Token: 0x060002E4 RID: 740 RVA: 0x00008A68 File Offset: 0x00006C68
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

		// Token: 0x060002E5 RID: 741 RVA: 0x00008AC2 File Offset: 0x00006CC2
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

		// Token: 0x060002E6 RID: 742 RVA: 0x00008AE8 File Offset: 0x00006CE8
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

		// Token: 0x060002E7 RID: 743 RVA: 0x00008C05 File Offset: 0x00006E05
		public void HealToMaxHp(bool triggerVisual = true, string audioName = null)
		{
			this.Heal(this.Player.MaxHp - this.Player.Hp, triggerVisual, audioName);
		}

		// Token: 0x060002E8 RID: 744 RVA: 0x00008C28 File Offset: 0x00006E28
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

		// Token: 0x060002E9 RID: 745 RVA: 0x00008CCB File Offset: 0x00006ECB
		internal int InternalGainPower(int power)
		{
			return this.Player.GainPower((this.PowerGainRate * (float)power).RoundToInt());
		}

		// Token: 0x060002EA RID: 746 RVA: 0x00008CE6 File Offset: 0x00006EE6
		internal void InternalConsumePower(int power)
		{
			this.Player.ConsumePower((this.PowerGainRate * (float)power).RoundToInt());
		}

		// Token: 0x060002EB RID: 747 RVA: 0x00008D01 File Offset: 0x00006F01
		internal int InternalLosePower(int power)
		{
			return this.Player.LosePower((this.PowerGainRate * (float)power).RoundToInt());
		}

		// Token: 0x060002EC RID: 748 RVA: 0x00008D1C File Offset: 0x00006F1C
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

		// Token: 0x060002ED RID: 749 RVA: 0x00008D78 File Offset: 0x00006F78
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

		// Token: 0x060002EE RID: 750 RVA: 0x00008DD4 File Offset: 0x00006FD4
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

		// Token: 0x060002EF RID: 751 RVA: 0x00008E34 File Offset: 0x00007034
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

		// Token: 0x060002F0 RID: 752 RVA: 0x00008E5F File Offset: 0x0000705F
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

		// Token: 0x060002F1 RID: 753 RVA: 0x00008E98 File Offset: 0x00007098
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

		// Token: 0x060002F2 RID: 754 RVA: 0x00008EE8 File Offset: 0x000070E8
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

		// Token: 0x060002F3 RID: 755 RVA: 0x00008F55 File Offset: 0x00007155
		internal void InternalGainMoney(int money)
		{
			money = Math.Min(money, 99999 - this.Money);
			this.Money += money;
			this.TotalMoney += money;
		}

		// Token: 0x060002F4 RID: 756 RVA: 0x00008F87 File Offset: 0x00007187
		public void GainMoney(int money, bool triggerVisual = false, VisualSourceData sourceData = null)
		{
			this.InternalGainMoney(money);
			this.VisualTrigger.OnGainMoney(money, triggerVisual, sourceData);
			this.MoneyGained.Execute(new GameEventArgs
			{
				CanCancel = false
			});
		}

		// Token: 0x060002F5 RID: 757 RVA: 0x00008FB8 File Offset: 0x000071B8
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

		// Token: 0x060002F6 RID: 758 RVA: 0x00009025 File Offset: 0x00007225
		public void LoseMoney(int money)
		{
			this.Money = Math.Max(this.Money - money, 0);
			this.VisualTrigger.OnLoseMoney(money);
			this.MoneyLost.Execute(new GameEventArgs
			{
				CanCancel = false
			});
		}

		// Token: 0x060002F7 RID: 759 RVA: 0x00009060 File Offset: 0x00007260
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

		// Token: 0x060002F8 RID: 760 RVA: 0x000090EC File Offset: 0x000072EC
		public void AddDeckCard(Card card, bool triggerVisual = false, VisualSourceData sourceData = null)
		{
			this.AddDeckCards(new Card[] { card }, triggerVisual, sourceData);
		}

		// Token: 0x060002F9 RID: 761 RVA: 0x00009100 File Offset: 0x00007300
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

		// Token: 0x060002FA RID: 762 RVA: 0x00009170 File Offset: 0x00007370
		public Card GetDeckCardByInstanceId(int instanceId)
		{
			return Enumerable.FirstOrDefault<Card>(this._baseDeck, (Card c) => c.InstanceId == instanceId);
		}

		// Token: 0x060002FB RID: 763 RVA: 0x000091A4 File Offset: 0x000073A4
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

		// Token: 0x060002FC RID: 764 RVA: 0x00009208 File Offset: 0x00007408
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

		// Token: 0x060002FD RID: 765 RVA: 0x00009269 File Offset: 0x00007469
		public void RemoveDeckCard(Card card, bool triggerVisual = true)
		{
			this.RemoveDeckCards(new Card[] { card }, triggerVisual);
		}

		// Token: 0x060002FE RID: 766 RVA: 0x0000927C File Offset: 0x0000747C
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

		// Token: 0x060002FF RID: 767 RVA: 0x00009343 File Offset: 0x00007543
		public void RemoveGamerunInitialCards()
		{
			this.RemoveDeckCards(Enumerable.Where<Card>(this._baseDeck, (Card card) => card.IsGamerunInitial), false);
		}

		// Token: 0x06000300 RID: 768 RVA: 0x00009376 File Offset: 0x00007576
		public void UpgradeDeckCard(Card card, bool triggerVisual = false)
		{
			this.UpgradeDeckCards(new Card[] { card }, triggerVisual);
		}

		// Token: 0x06000301 RID: 769 RVA: 0x0000938C File Offset: 0x0000758C
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

		// Token: 0x06000302 RID: 770 RVA: 0x00009438 File Offset: 0x00007638
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

		// Token: 0x06000303 RID: 771 RVA: 0x000094E0 File Offset: 0x000076E0
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

		// Token: 0x06000304 RID: 772 RVA: 0x00009504 File Offset: 0x00007704
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

		// Token: 0x06000305 RID: 773 RVA: 0x000095AC File Offset: 0x000077AC
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

		// Token: 0x06000306 RID: 774 RVA: 0x00009624 File Offset: 0x00007824
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

		// Token: 0x06000307 RID: 775 RVA: 0x000096A4 File Offset: 0x000078A4
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

		// Token: 0x06000308 RID: 776 RVA: 0x00009730 File Offset: 0x00007930
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

		// Token: 0x06000309 RID: 777 RVA: 0x000098D4 File Offset: 0x00007AD4
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

		// Token: 0x0600030A RID: 778 RVA: 0x0000998C File Offset: 0x00007B8C
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

		// Token: 0x0600030B RID: 779 RVA: 0x00009A81 File Offset: 0x00007C81
		internal IEnumerator BuyExhibitRunner(ShopItem<Exhibit> exhibitItem, VisualSourceData sourceData)
		{
			exhibitItem.IsSoldOut = true;
			this.Stats.ShopConsumed += exhibitItem.Price;
			this.ConsumeMoney(exhibitItem.Price);
			yield return this.GainExhibitRunner(exhibitItem.Content, true, sourceData);
			yield break;
		}

		// Token: 0x0600030C RID: 780 RVA: 0x00009A9E File Offset: 0x00007C9E
		public void AbandonGameRun()
		{
			this.Status = GameRunStatus.Failure;
			this.GenerateRecords(true, null, null);
		}

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x0600030D RID: 781 RVA: 0x00009AB0 File Offset: 0x00007CB0
		// (set) Token: 0x0600030E RID: 782 RVA: 0x00009AB8 File Offset: 0x00007CB8
		public GameRunRecordSaveData GameRunRecord { get; private set; }

		// Token: 0x0600030F RID: 783 RVA: 0x00009AC4 File Offset: 0x00007CC4
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

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x06000310 RID: 784 RVA: 0x00009D2F File Offset: 0x00007F2F
		public GameEvent<GameEventArgs> StageEntered { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x1700010E RID: 270
		// (get) Token: 0x06000311 RID: 785 RVA: 0x00009D37 File Offset: 0x00007F37
		public GameEvent<StationEventArgs> StationEntering { get; } = new GameEvent<StationEventArgs>();

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x06000312 RID: 786 RVA: 0x00009D3F File Offset: 0x00007F3F
		public GameEvent<StationEventArgs> StationEntered { get; } = new GameEvent<StationEventArgs>();

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x06000313 RID: 787 RVA: 0x00009D47 File Offset: 0x00007F47
		public GameEvent<StationEventArgs> StationLeaving { get; } = new GameEvent<StationEventArgs>();

		// Token: 0x17000111 RID: 273
		// (get) Token: 0x06000314 RID: 788 RVA: 0x00009D4F File Offset: 0x00007F4F
		public GameEvent<StationEventArgs> StationLeft { get; } = new GameEvent<StationEventArgs>();

		// Token: 0x17000112 RID: 274
		// (get) Token: 0x06000315 RID: 789 RVA: 0x00009D57 File Offset: 0x00007F57
		public GameEvent<StationEventArgs> StationFinished { get; } = new GameEvent<StationEventArgs>();

		// Token: 0x17000113 RID: 275
		// (get) Token: 0x06000316 RID: 790 RVA: 0x00009D5F File Offset: 0x00007F5F
		public GameEvent<StationEventArgs> StationRewardGenerating { get; } = new GameEvent<StationEventArgs>();

		// Token: 0x17000114 RID: 276
		// (get) Token: 0x06000317 RID: 791 RVA: 0x00009D67 File Offset: 0x00007F67
		public GameEvent<StationEventArgs> GapOptionsGenerating { get; } = new GameEvent<StationEventArgs>();

		// Token: 0x17000115 RID: 277
		// (get) Token: 0x06000318 RID: 792 RVA: 0x00009D6F File Offset: 0x00007F6F
		public GameEvent<GameEventArgs> RewardAbandoned { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x06000319 RID: 793 RVA: 0x00009D77 File Offset: 0x00007F77
		public GameEvent<GameEventArgs> BaseManaChanged { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x17000117 RID: 279
		// (get) Token: 0x0600031A RID: 794 RVA: 0x00009D7F File Offset: 0x00007F7F
		public GameEvent<GameEventArgs> MoneyGained { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x0600031B RID: 795 RVA: 0x00009D87 File Offset: 0x00007F87
		public GameEvent<GameEventArgs> MoneyConsumed { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x17000119 RID: 281
		// (get) Token: 0x0600031C RID: 796 RVA: 0x00009D8F File Offset: 0x00007F8F
		public GameEvent<GameEventArgs> MoneyLost { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x0600031D RID: 797 RVA: 0x00009D97 File Offset: 0x00007F97
		public GameEvent<CardsEventArgs> DeckCardsAdding { get; } = new GameEvent<CardsEventArgs>();

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x0600031E RID: 798 RVA: 0x00009D9F File Offset: 0x00007F9F
		public GameEvent<CardsEventArgs> DeckCardsAdded { get; } = new GameEvent<CardsEventArgs>();

		// Token: 0x1700011C RID: 284
		// (get) Token: 0x0600031F RID: 799 RVA: 0x00009DA7 File Offset: 0x00007FA7
		public GameEvent<CardsEventArgs> DeckCardsRemoving { get; } = new GameEvent<CardsEventArgs>();

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x06000320 RID: 800 RVA: 0x00009DAF File Offset: 0x00007FAF
		public GameEvent<CardsEventArgs> DeckCardsRemoved { get; } = new GameEvent<CardsEventArgs>();

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x06000321 RID: 801 RVA: 0x00009DB7 File Offset: 0x00007FB7
		public GameEvent<CardsEventArgs> DeckCardsUpgraded { get; } = new GameEvent<CardsEventArgs>();

		// Token: 0x06000322 RID: 802 RVA: 0x00009DC0 File Offset: 0x00007FC0
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

		// Token: 0x06000323 RID: 803 RVA: 0x0000A600 File Offset: 0x00008800
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

		// Token: 0x06000324 RID: 804 RVA: 0x0000B1B0 File Offset: 0x000093B0
		[CompilerGenerated]
		internal static CardWeightTable <GetRewardCards>g__ModifyRare|436_0(CardWeightTable table, float rareMultiplier)
		{
			RarityWeightTable rarityTable = table.RarityTable;
			float rare = rarityTable.Rare;
			return table.WithRarity(rarityTable.WithRare(rare * rareMultiplier));
		}

		// Token: 0x06000325 RID: 805 RVA: 0x0000B1DC File Offset: 0x000093DC
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

		// Token: 0x06000326 RID: 806 RVA: 0x0000B232 File Offset: 0x00009432
		[CompilerGenerated]
		internal static string <Restore>g__EnemyGroupConverter|550_1(string s)
		{
			if (Library.TryGetEnemyGroupEntry(s) == null)
			{
				Debug.LogWarning("Cannot find EnemyGroup '" + s + "'");
			}
			return s;
		}

		// Token: 0x0400013A RID: 314
		private List<Card> _baseDeck;

		// Token: 0x0400013B RID: 315
		private int _deckCardInstanceId;

		// Token: 0x04000142 RID: 322
		private List<Stage> _stages;

		// Token: 0x04000143 RID: 323
		private int _stageIndex = -1;

		// Token: 0x04000144 RID: 324
		private readonly List<JadeBox> _jadeBoxes = new List<JadeBox>();

		// Token: 0x0400014B RID: 331
		private GameRunMapMode _mapMode;

		// Token: 0x0400014C RID: 332
		private readonly List<IMapModeOverrider> _mapModeOverriders = new List<IMapModeOverrider>();

		// Token: 0x0400014D RID: 333
		private IMapModeOverrider _activeMapModeOverrider;

		// Token: 0x04000179 RID: 377
		private float _cardRareWeightFactor;

		// Token: 0x0400017A RID: 378
		private Dictionary<string, float> _cardRewardWeightFactors;

		// Token: 0x0400017B RID: 379
		private bool _cardRewardDecreaseRepeatRare;

		// Token: 0x0400017C RID: 380
		private const float CardRewardDecreaseRepeatRareMultiplier = 0.01f;

		// Token: 0x0400017D RID: 381
		private const float CardRareFactorDefaultValue = 0.85f;

		// Token: 0x0400017E RID: 382
		private const float CardRareFactorIncrement = 0.01f;

		// Token: 0x0400017F RID: 383
		private const float CardNormalAbandonMultiplier = 0.9f;

		// Token: 0x04000180 RID: 384
		private const float CardAllAbandonMultipler = 0.85f;
	}
}
