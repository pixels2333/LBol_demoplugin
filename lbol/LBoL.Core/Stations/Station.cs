using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Attributes;
using LBoL.Core.Cards;
using LBoL.Core.SaveData;
using UnityEngine;

namespace LBoL.Core.Stations
{
	// Token: 0x020000C6 RID: 198
	[Localizable]
	public abstract class Station
	{
		// Token: 0x170002BE RID: 702
		// (get) Token: 0x0600088C RID: 2188 RVA: 0x000191BC File Offset: 0x000173BC
		// (set) Token: 0x0600088D RID: 2189 RVA: 0x000191DB File Offset: 0x000173DB
		public GameRunController GameRun
		{
			get
			{
				GameRunController gameRunController;
				if (!this._gameRun.TryGetTarget(ref gameRunController))
				{
					return null;
				}
				return gameRunController;
			}
			internal set
			{
				this._gameRun.SetTarget(value);
			}
		}

		// Token: 0x170002BF RID: 703
		// (get) Token: 0x0600088E RID: 2190 RVA: 0x000191EC File Offset: 0x000173EC
		// (set) Token: 0x0600088F RID: 2191 RVA: 0x0001920B File Offset: 0x0001740B
		public Stage Stage
		{
			get
			{
				Stage stage;
				if (!this._stage.TryGetTarget(ref stage))
				{
					return null;
				}
				return stage;
			}
			set
			{
				this._stage.SetTarget(value);
			}
		}

		// Token: 0x170002C0 RID: 704
		// (get) Token: 0x06000890 RID: 2192 RVA: 0x00019219 File Offset: 0x00017419
		// (set) Token: 0x06000891 RID: 2193 RVA: 0x00019221 File Offset: 0x00017421
		public int Level { get; set; }

		// Token: 0x170002C1 RID: 705
		// (get) Token: 0x06000892 RID: 2194 RVA: 0x0001922A File Offset: 0x0001742A
		// (set) Token: 0x06000893 RID: 2195 RVA: 0x00019232 File Offset: 0x00017432
		public int Act { get; set; }

		// Token: 0x170002C2 RID: 706
		// (get) Token: 0x06000894 RID: 2196 RVA: 0x0001923B File Offset: 0x0001743B
		// (set) Token: 0x06000895 RID: 2197 RVA: 0x00019243 File Offset: 0x00017443
		public bool IsStageEnd { get; set; }

		// Token: 0x170002C3 RID: 707
		// (get) Token: 0x06000896 RID: 2198 RVA: 0x0001924C File Offset: 0x0001744C
		// (set) Token: 0x06000897 RID: 2199 RVA: 0x00019254 File Offset: 0x00017454
		public bool IsNormalEnd { get; set; }

		// Token: 0x170002C4 RID: 708
		// (get) Token: 0x06000898 RID: 2200 RVA: 0x0001925D File Offset: 0x0001745D
		// (set) Token: 0x06000899 RID: 2201 RVA: 0x00019265 File Offset: 0x00017465
		public bool IsTrueEnd { get; set; }

		// Token: 0x170002C5 RID: 709
		// (get) Token: 0x0600089A RID: 2202
		public abstract StationType Type { get; }

		// Token: 0x170002C6 RID: 710
		// (get) Token: 0x0600089B RID: 2203 RVA: 0x0001926E File Offset: 0x0001746E
		public List<StationReward> Rewards { get; } = new List<StationReward>();

		// Token: 0x170002C7 RID: 711
		// (get) Token: 0x0600089C RID: 2204 RVA: 0x00019276 File Offset: 0x00017476
		public List<StationDialogSource> PreDialogs { get; } = new List<StationDialogSource>();

		// Token: 0x170002C8 RID: 712
		// (get) Token: 0x0600089D RID: 2205 RVA: 0x0001927E File Offset: 0x0001747E
		public List<StationDialogSource> PostDialogs { get; } = new List<StationDialogSource>();

		// Token: 0x0600089E RID: 2206 RVA: 0x00019288 File Offset: 0x00017488
		protected void AddReward(StationReward reward)
		{
			switch (reward.Type)
			{
			case StationRewardType.Money:
			case StationRewardType.RemoveCard:
				goto IL_00B5;
			case StationRewardType.Card:
			{
				using (List<Card>.Enumerator enumerator = reward.Cards.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Card card = enumerator.Current;
						this.GameRun.RevealCard(card);
					}
					goto IL_00B5;
				}
				break;
			}
			case StationRewardType.Exhibit:
				break;
			case StationRewardType.Tool:
			{
				using (List<Card>.Enumerator enumerator = reward.Cards.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Card card2 = enumerator.Current;
						this.GameRun.RevealCard(card2);
					}
					goto IL_00B5;
				}
				goto IL_00AF;
			}
			default:
				goto IL_00AF;
			}
			this.GameRun.RevealExhibit(reward.Exhibit);
			goto IL_00B5;
			IL_00AF:
			throw new ArgumentOutOfRangeException();
			IL_00B5:
			this.Rewards.Add(reward);
		}

		// Token: 0x0600089F RID: 2207 RVA: 0x00019374 File Offset: 0x00017574
		protected void AddRewards(IEnumerable<StationReward> rewards)
		{
			foreach (StationReward stationReward in rewards)
			{
				this.AddReward(stationReward);
			}
		}

		// Token: 0x060008A0 RID: 2208 RVA: 0x000193BC File Offset: 0x000175BC
		protected void AddRewards(params StationReward[] rewards)
		{
			this.AddRewards(rewards);
		}

		// Token: 0x060008A1 RID: 2209 RVA: 0x000193C8 File Offset: 0x000175C8
		internal void GenerateEnemyRewards()
		{
			List<StationReward> rewards = this.Rewards;
			if (rewards != null && rewards.Count > 0)
			{
				Debug.LogError("GenerateEnemyRewards invoked while already has rewards");
			}
			int num = ((this.Act == 3) ? this.GameRun.GameRunEventRng.NextInt(GlobalConfig.EnemyStationMoneyHigh[0], GlobalConfig.EnemyStationMoneyHigh[1]) : this.GameRun.GameRunEventRng.NextInt(GlobalConfig.EnemyStationMoney[0], GlobalConfig.EnemyStationMoney[1]));
			int num2 = this.GameRun.ModifyMoneyReward(num);
			if (this.GameRun.ExtraExhibitReward != null)
			{
				this.AddRewards(new StationReward[]
				{
					StationReward.CreateMoney(num2),
					this.Stage.GetEnemyCardReward(),
					StationReward.CreateExhibit(this.GameRun.ExtraExhibitReward)
				});
				this.GameRun.ExtraExhibitReward = null;
			}
			else
			{
				this.AddRewards(new StationReward[]
				{
					StationReward.CreateMoney(num2),
					this.Stage.GetEnemyCardReward()
				});
			}
			this.GameRun.StationRewardGenerating.Execute(new StationEventArgs
			{
				Station = this,
				CanCancel = false
			});
		}

		// Token: 0x060008A2 RID: 2210 RVA: 0x000194E0 File Offset: 0x000176E0
		internal void GenerateEliteEnemyRewards()
		{
			List<StationReward> rewards = this.Rewards;
			if (rewards != null && rewards.Count > 0)
			{
				Debug.LogError("GenerateEliteEnemyRewards invoked while already has rewards");
			}
			int num = this.GameRun.GameRunEventRng.NextInt(GlobalConfig.EliteStationMoney[0], GlobalConfig.EliteStationMoney[1]);
			int num2 = this.GameRun.ModifyMoneyReward(num);
			this.AddRewards(new StationReward[]
			{
				StationReward.CreateMoney(num2),
				this.Stage.GetEliteEnemyCardReward(),
				StationReward.CreateExhibit(this.Stage.GetEliteEnemyExhibit())
			});
			this.GameRun.StationRewardGenerating.Execute(new StationEventArgs
			{
				Station = this,
				CanCancel = false
			});
		}

		// Token: 0x060008A3 RID: 2211 RVA: 0x0001958F File Offset: 0x0001778F
		public void ClearRewards()
		{
			this.Rewards.Clear();
		}

		// Token: 0x060008A4 RID: 2212 RVA: 0x0001959C File Offset: 0x0001779C
		public IEnumerator AcquireRewardRunner(StationReward reward)
		{
			int num = this.Rewards.IndexOf(reward);
			if (num < 0)
			{
				throw new InvalidOperationException(string.Format("Cannot gain reward {0} that is not in station", reward));
			}
			if (reward.Type == StationRewardType.Card)
			{
				throw new InvalidOperationException("Cannot gain card reward without selection");
			}
			if (reward.Type == StationRewardType.Money)
			{
				this.GameRun.GainMoney(reward.Money, true, new VisualSourceData
				{
					SourceType = VisualSourceType.Reward,
					Index = this.Rewards.IndexOf(reward)
				});
			}
			if (reward.Type == StationRewardType.Tool)
			{
				this.GameRun.AcquireCardReward(reward, Enumerable.FirstOrDefault<Card>(reward.Cards), num);
			}
			else if (reward.Type == StationRewardType.Exhibit)
			{
				yield return this.GameRun.GainExhibitRunner(reward.Exhibit, true, new VisualSourceData
				{
					SourceType = VisualSourceType.Reward,
					Index = this.Rewards.IndexOf(reward)
				});
				using (List<StationReward>.Enumerator enumerator = this.Rewards.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						StationReward stationReward = enumerator.Current;
						if (stationReward.Type == StationRewardType.Card)
						{
							foreach (Card card in stationReward.Cards)
							{
								this.GameRun.UpgradeNewDeckCardOnFlags(card);
							}
						}
					}
					yield break;
				}
			}
			yield break;
		}

		// Token: 0x060008A5 RID: 2213 RVA: 0x000195B4 File Offset: 0x000177B4
		public void AcquireCardReward(StationReward reward, Card card)
		{
			if (reward.Type != StationRewardType.Card)
			{
				throw new InvalidOperationException("Cannot acquire card from non-card reward");
			}
			int num = this.Rewards.IndexOf(reward);
			if (num < 0)
			{
				throw new InvalidOperationException(string.Format("Cannot gain reward {0} that is not in station", reward));
			}
			if (!reward.Cards.Contains(card))
			{
				throw new InvalidOperationException(string.Concat(new string[]
				{
					"Cannot gain card ",
					card.DebugName,
					" that is not in card reward [",
					string.Join<Card>(", ", reward.Cards),
					"]"
				}));
			}
			this.GameRun.AcquireCardReward(reward, card, num);
		}

		// Token: 0x170002C9 RID: 713
		// (get) Token: 0x060008A6 RID: 2214 RVA: 0x00019658 File Offset: 0x00017858
		// (set) Token: 0x060008A7 RID: 2215 RVA: 0x00019660 File Offset: 0x00017860
		public StationStatus Status { get; protected set; }

		// Token: 0x060008A8 RID: 2216 RVA: 0x0001966C File Offset: 0x0001786C
		public static Station Create(StationType type)
		{
			Station station;
			switch (type)
			{
			case StationType.None:
				throw new InvalidOperationException("Map node station-type not set");
			case StationType.Enemy:
				station = new EnemyStation();
				break;
			case StationType.EliteEnemy:
				station = new EliteEnemyStation();
				break;
			case StationType.Supply:
				station = new SupplyStation();
				break;
			case StationType.Gap:
				station = new GapStation();
				break;
			case StationType.Shop:
				station = new ShopStation();
				break;
			case StationType.Adventure:
				station = new AdventureStation();
				break;
			case StationType.Entry:
				station = new EntryStation();
				break;
			case StationType.Select:
				station = new SelectStation();
				break;
			case StationType.Trade:
				station = new TradeStation();
				break;
			case StationType.Boss:
				station = new BossStation();
				break;
			case StationType.BattleAdvTest:
				station = new BattleAdvTestStation();
				break;
			default:
				throw new ArgumentOutOfRangeException("type", type, null);
			}
			return station;
		}

		// Token: 0x060008A9 RID: 2217 RVA: 0x00019727 File Offset: 0x00017927
		protected internal virtual void OnEnter()
		{
		}

		// Token: 0x060008AA RID: 2218 RVA: 0x00019729 File Offset: 0x00017929
		protected internal virtual void OnLeave()
		{
		}

		// Token: 0x060008AB RID: 2219 RVA: 0x0001972B File Offset: 0x0001792B
		public virtual void Finish()
		{
			this.Status = StationStatus.Finished;
			this.GameRun.FinishStation(this);
			this.GameRun.StationFinished.Execute(new StationEventArgs
			{
				Station = this,
				CanCancel = false
			});
		}

		// Token: 0x060008AC RID: 2220 RVA: 0x00019763 File Offset: 0x00017963
		internal void ForceFinish()
		{
			this.Status = StationStatus.Finished;
		}

		// Token: 0x060008AD RID: 2221
		internal abstract StationRecord GenerateRecord();

		// Token: 0x040003A2 RID: 930
		private readonly WeakReference<GameRunController> _gameRun = new WeakReference<GameRunController>(null);

		// Token: 0x040003A3 RID: 931
		private readonly WeakReference<Stage> _stage = new WeakReference<Stage>(null);
	}
}
