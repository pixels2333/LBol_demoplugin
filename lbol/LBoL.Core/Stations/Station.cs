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
	[Localizable]
	public abstract class Station
	{
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
		public int Level { get; set; }
		public int Act { get; set; }
		public bool IsStageEnd { get; set; }
		public bool IsNormalEnd { get; set; }
		public bool IsTrueEnd { get; set; }
		public abstract StationType Type { get; }
		public List<StationReward> Rewards { get; } = new List<StationReward>();
		public List<StationDialogSource> PreDialogs { get; } = new List<StationDialogSource>();
		public List<StationDialogSource> PostDialogs { get; } = new List<StationDialogSource>();
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
		protected void AddRewards(IEnumerable<StationReward> rewards)
		{
			foreach (StationReward stationReward in rewards)
			{
				this.AddReward(stationReward);
			}
		}
		protected void AddRewards(params StationReward[] rewards)
		{
			this.AddRewards(rewards);
		}
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
		public void ClearRewards()
		{
			this.Rewards.Clear();
		}
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
		public StationStatus Status { get; protected set; }
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
		protected internal virtual void OnEnter()
		{
		}
		protected internal virtual void OnLeave()
		{
		}
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
		internal void ForceFinish()
		{
			this.Status = StationStatus.Finished;
		}
		internal abstract StationRecord GenerateRecord();
		private readonly WeakReference<GameRunController> _gameRun = new WeakReference<GameRunController>(null);
		private readonly WeakReference<Stage> _stage = new WeakReference<Stage>(null);
	}
}
