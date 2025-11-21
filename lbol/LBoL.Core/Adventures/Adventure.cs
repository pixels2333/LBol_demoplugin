using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Dialogs;
using LBoL.Core.Stations;
using UnityEngine;
using Yarn;

namespace LBoL.Core.Adventures
{
	// Token: 0x020001BA RID: 442
	public abstract class Adventure : IInitializable
	{
		// Token: 0x17000551 RID: 1361
		// (get) Token: 0x06000FAD RID: 4013 RVA: 0x0002A16F File Offset: 0x0002836F
		// (set) Token: 0x06000FAE RID: 4014 RVA: 0x0002A177 File Offset: 0x00028377
		public string Id { get; private set; }

		// Token: 0x17000552 RID: 1362
		// (get) Token: 0x06000FAF RID: 4015 RVA: 0x0002A180 File Offset: 0x00028380
		public string Title
		{
			get
			{
				return TypeFactory<Adventure>.LocalizeProperty(this.Id, "Title", false, true);
			}
		}

		// Token: 0x17000553 RID: 1363
		// (get) Token: 0x06000FB0 RID: 4016 RVA: 0x0002A194 File Offset: 0x00028394
		public string HostName
		{
			get
			{
				return TypeFactory<Adventure>.LocalizeProperty(this.Id, "HostName", false, true);
			}
		}

		// Token: 0x17000554 RID: 1364
		// (get) Token: 0x06000FB1 RID: 4017 RVA: 0x0002A1A8 File Offset: 0x000283A8
		// (set) Token: 0x06000FB2 RID: 4018 RVA: 0x0002A1B0 File Offset: 0x000283B0
		public AdventureConfig Config { get; private set; }

		// Token: 0x06000FB3 RID: 4019 RVA: 0x0002A1BC File Offset: 0x000283BC
		public void Initialize()
		{
			this.Id = base.GetType().Name;
			this.Config = AdventureConfig.FromId(this.Id);
			if (this.Config == null)
			{
				throw new InvalidDataException("Cannot find adventure config for <" + this.Id + ">");
			}
		}

		// Token: 0x17000555 RID: 1365
		// (get) Token: 0x06000FB4 RID: 4020 RVA: 0x0002A210 File Offset: 0x00028410
		// (set) Token: 0x06000FB5 RID: 4021 RVA: 0x0002A22F File Offset: 0x0002842F
		public Station Station
		{
			get
			{
				Station station;
				if (!this._station.TryGetTarget(ref station))
				{
					return null;
				}
				return station;
			}
			private set
			{
				this._station.SetTarget(value);
			}
		}

		// Token: 0x17000556 RID: 1366
		// (get) Token: 0x06000FB6 RID: 4022 RVA: 0x0002A240 File Offset: 0x00028440
		// (set) Token: 0x06000FB7 RID: 4023 RVA: 0x0002A25F File Offset: 0x0002845F
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
			private set
			{
				this._stage.SetTarget(value);
			}
		}

		// Token: 0x17000557 RID: 1367
		// (get) Token: 0x06000FB8 RID: 4024 RVA: 0x0002A270 File Offset: 0x00028470
		// (set) Token: 0x06000FB9 RID: 4025 RVA: 0x0002A28F File Offset: 0x0002848F
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
			private set
			{
				this._gameRun.SetTarget(value);
			}
		}

		// Token: 0x06000FBA RID: 4026 RVA: 0x0002A2A0 File Offset: 0x000284A0
		internal void SetStation(Station station)
		{
			this.Station = station;
			Stage stage = station.Stage;
			this.Stage = stage;
			this.GameRun = stage.GameRun;
		}

		// Token: 0x17000558 RID: 1368
		// (get) Token: 0x06000FBB RID: 4027 RVA: 0x0002A2CE File Offset: 0x000284CE
		public virtual string DialogName
		{
			get
			{
				return base.GetType().Name;
			}
		}

		// Token: 0x17000559 RID: 1369
		// (get) Token: 0x06000FBC RID: 4028 RVA: 0x0002A2DB File Offset: 0x000284DB
		// (set) Token: 0x06000FBD RID: 4029 RVA: 0x0002A2E3 File Offset: 0x000284E3
		public DialogStorage Storage { get; private set; }

		// Token: 0x06000FBE RID: 4030 RVA: 0x0002A2EC File Offset: 0x000284EC
		public void SetStorage(DialogStorage storage)
		{
			this.Storage = storage;
			this.InitVariables(storage);
		}

		// Token: 0x06000FBF RID: 4031 RVA: 0x0002A2FC File Offset: 0x000284FC
		public void RestoreStorage(DialogStorage storage)
		{
			this.Storage = storage;
		}

		// Token: 0x06000FC0 RID: 4032 RVA: 0x0002A305 File Offset: 0x00028505
		protected virtual void InitVariables(IVariableStorage storage)
		{
		}

		// Token: 0x06000FC1 RID: 4033 RVA: 0x0002A307 File Offset: 0x00028507
		[RuntimeCommand("damage", "")]
		[UsedImplicitly]
		public void Damage(int damage, bool isSelf = true)
		{
			this.GameRun.Damage(damage, DamageType.HpLose, isSelf, true, this);
		}

		// Token: 0x06000FC2 RID: 4034 RVA: 0x0002A319 File Offset: 0x00028519
		[RuntimeCommand("heal", "")]
		[UsedImplicitly]
		public void Heal(int heal, string audioName = null)
		{
			this.GameRun.Heal(heal, true, audioName);
		}

		// Token: 0x06000FC3 RID: 4035 RVA: 0x0002A329 File Offset: 0x00028529
		[RuntimeCommand("healPercentage", "")]
		[UsedImplicitly]
		public void HealPercentage(int percentage)
		{
			this.GameRun.Heal(Mathf.RoundToInt((float)(this.GameRun.Player.MaxHp * percentage) / 100f), true, null);
		}

		// Token: 0x06000FC4 RID: 4036 RVA: 0x0002A356 File Offset: 0x00028556
		[RuntimeCommand("healToMax", "")]
		[UsedImplicitly]
		public void HealToMax(string audioName = null)
		{
			this.GameRun.HealToMaxHp(true, audioName);
		}

		// Token: 0x06000FC5 RID: 4037 RVA: 0x0002A365 File Offset: 0x00028565
		[RuntimeCommand("gainMaxHp", "")]
		[UsedImplicitly]
		public void GainMaxHp(int maxHp, bool stats = true)
		{
			this.GameRun.GainMaxHp(maxHp, true, stats);
		}

		// Token: 0x06000FC6 RID: 4038 RVA: 0x0002A375 File Offset: 0x00028575
		[RuntimeCommand("loseMaxHp", "")]
		[UsedImplicitly]
		public void LoseMaxHp(int maxHp)
		{
			this.GameRun.LoseMaxHp(maxHp, true);
		}

		// Token: 0x06000FC7 RID: 4039 RVA: 0x0002A384 File Offset: 0x00028584
		[RuntimeCommand("gainMoney", "")]
		[UsedImplicitly]
		public void GainMoney(int money)
		{
			this.GameRun.GainMoney(money, true, new VisualSourceData
			{
				SourceType = VisualSourceType.Vn
			});
		}

		// Token: 0x06000FC8 RID: 4040 RVA: 0x0002A39F File Offset: 0x0002859F
		[RuntimeCommand("consumeMoney", "")]
		[UsedImplicitly]
		public void ConsumeMoney(int money)
		{
			this.GameRun.ConsumeMoney(money);
		}

		// Token: 0x06000FC9 RID: 4041 RVA: 0x0002A3AD File Offset: 0x000285AD
		[RuntimeCommand("loseMoney", "")]
		[UsedImplicitly]
		public void LoseMoney(int money)
		{
			this.GameRun.LoseMoney(money);
		}

		// Token: 0x06000FCA RID: 4042 RVA: 0x0002A3BB File Offset: 0x000285BB
		[RuntimeCommand("gainPower", "")]
		[UsedImplicitly]
		public void GainPower(int power)
		{
			this.GameRun.GainPower(power, false);
		}

		// Token: 0x06000FCB RID: 4043 RVA: 0x0002A3CA File Offset: 0x000285CA
		[RuntimeCommand("losePower", "")]
		[UsedImplicitly]
		public void LosePower(int power)
		{
			this.GameRun.LosePower(power, false);
		}

		// Token: 0x06000FCC RID: 4044 RVA: 0x0002A3D9 File Offset: 0x000285D9
		[RuntimeCommand("gainCards", "")]
		[UsedImplicitly]
		public void GainCards(params string[] names)
		{
			this.GameRun.AddDeckCards(Enumerable.Select<string, Card>(names, new Func<string, Card>(Library.CreateCard)), true, new VisualSourceData
			{
				SourceType = VisualSourceType.Vn,
				Index = -1
			});
		}

		// Token: 0x06000FCD RID: 4045 RVA: 0x0002A40C File Offset: 0x0002860C
		[RuntimeCommand("gainExhibit", "")]
		[UsedImplicitly]
		public IEnumerator GainExhibitRunner(string name, string message = null, int optionIndex = -1)
		{
			Exhibit exhibit = Library.CreateExhibit(name);
			if (message != null)
			{
				MethodInfo method = exhibit.GetType().GetMethod(message);
				if (method != null)
				{
					method.Invoke(exhibit, null);
				}
			}
			return this.GameRun.GainExhibitRunner(exhibit, true, new VisualSourceData
			{
				SourceType = VisualSourceType.Vn,
				Index = optionIndex
			});
		}

		// Token: 0x06000FCE RID: 4046 RVA: 0x0002A464 File Offset: 0x00028664
		[RuntimeCommand("loseExhibit", "")]
		[UsedImplicitly]
		public void LoseExhibit(string name)
		{
			Exhibit exhibit = this.GameRun.Player.GetExhibit(TypeFactory<Exhibit>.GetType(name));
			if (exhibit != null)
			{
				this.GameRun.LoseExhibit(exhibit, true, true);
				return;
			}
			Debug.LogError("Cannot lose '" + name + "': no such exhibit");
		}

		// Token: 0x06000FCF RID: 4047 RVA: 0x0002A4AF File Offset: 0x000286AF
		[RuntimeCommand("upgradeDeckCards", "")]
		[UsedImplicitly]
		public IEnumerator UpgradeDeckCards(string description, bool canCancel = false)
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(this.GameRun.BaseDeck, (Card c) => c.CanUpgrade));
			if (list.Count > 0)
			{
				UpgradeCardInteraction interaction = new UpgradeCardInteraction(list)
				{
					CanCancel = canCancel,
					Description = description
				};
				yield return this.GameRun.InteractionViewer.View(interaction);
				if (!interaction.IsCanceled)
				{
					this.GameRun.UpgradeDeckCards(new Card[] { interaction.SelectedCard }, true);
				}
				interaction = null;
			}
			yield break;
		}

		// Token: 0x06000FD0 RID: 4048 RVA: 0x0002A4CC File Offset: 0x000286CC
		[RuntimeCommand("removeDeckCards", "")]
		[UsedImplicitly]
		public IEnumerator RemoveDeckCards(string description, int min, int max, bool canCancel = false)
		{
			List<Card> list = Enumerable.ToList<Card>(this.GameRun.BaseDeckWithoutUnremovable);
			if (list.Count > 0)
			{
				SelectCardInteraction interaction = new SelectCardInteraction(min, max, list, SelectedCardHandling.DoNothing)
				{
					CanCancel = canCancel,
					Description = description
				};
				yield return this.GameRun.InteractionViewer.View(interaction);
				if (!interaction.IsCanceled)
				{
					this.GameRun.RemoveDeckCards(interaction.SelectedCards, true);
				}
				interaction = null;
			}
			yield break;
		}

		// Token: 0x06000FD1 RID: 4049 RVA: 0x0002A4F8 File Offset: 0x000286F8
		[RuntimeCommand("removeDeckCard", "")]
		[UsedImplicitly]
		public IEnumerator RemoveDeckCards(string description, bool canCancel = false)
		{
			List<Card> list = Enumerable.ToList<Card>(this.GameRun.BaseDeckWithoutUnremovable);
			if (list.Count > 0)
			{
				RemoveCardInteraction interaction = new RemoveCardInteraction(list)
				{
					CanCancel = canCancel,
					Description = description
				};
				yield return this.GameRun.InteractionViewer.View(interaction);
				if (!interaction.IsCanceled)
				{
					this.GameRun.RemoveDeckCards(new Card[] { interaction.SelectedCard }, true);
				}
				interaction = null;
			}
			yield break;
		}

		// Token: 0x06000FD2 RID: 4050 RVA: 0x0002A515 File Offset: 0x00028715
		[RuntimeCommand("transformDeckCard", "")]
		[UsedImplicitly]
		public IEnumerator TransformDeckCard(string description, string transformCard, bool upgrade = false, bool canCancel = false)
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(this.GameRun.BaseDeckWithoutUnremovable, (Card c) => c.CardType != CardType.Misfortune));
			if (list.Count > 0)
			{
				Card tCard = Library.CreateCard(transformCard, upgrade, default(int?));
				this.GameRun.UpgradeNewDeckCardOnFlags(tCard);
				TransformCardInteraction interaction = new TransformCardInteraction(list)
				{
					TransformCard = tCard,
					CanCancel = canCancel,
					Description = description
				};
				yield return this.GameRun.InteractionViewer.View(interaction);
				if (!interaction.IsCanceled)
				{
					this.GameRun.RemoveDeckCards(new Card[] { interaction.SelectedCard }, false);
					this.GameRun.AddDeckCard(tCard, false, null);
				}
				tCard = null;
				interaction = null;
			}
			yield break;
		}

		// Token: 0x06000FD3 RID: 4051 RVA: 0x0002A541 File Offset: 0x00028741
		[RuntimeCommand("selectCards", "")]
		[UsedImplicitly]
		public IEnumerator SelectCards(params string[] cardList)
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Select<string, Card>(cardList, new Func<string, Card>(Library.CreateCard)));
			this.GameRun.UpgradeNewDeckCardOnFlags(list);
			MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(list, true, true, false)
			{
				CanCancel = false
			};
			yield return this.GameRun.InteractionViewer.View(interaction);
			this.GameRun.AddDeckCard(interaction.SelectedCard, true, new VisualSourceData
			{
				SourceType = VisualSourceType.CardSelect
			});
			yield break;
		}

		// Token: 0x06000FD4 RID: 4052 RVA: 0x0002A557 File Offset: 0x00028757
		[RuntimeCommand("generateEnemyRewards", "")]
		[UsedImplicitly]
		public void GenerateEnemyRewards()
		{
			this.Station.GenerateEnemyRewards();
		}

		// Token: 0x06000FD5 RID: 4053 RVA: 0x0002A564 File Offset: 0x00028764
		[RuntimeCommand("generateEliteEnemyRewards", "")]
		[UsedImplicitly]
		public void GenerateEliteEnemyRewards()
		{
			this.Station.GenerateEliteEnemyRewards();
		}

		// Token: 0x06000FD6 RID: 4054 RVA: 0x0002A571 File Offset: 0x00028771
		[RuntimeCommand("clearRewards", "")]
		[UsedImplicitly]
		public void ClearRewards()
		{
			this.Station.ClearRewards();
		}

		// Token: 0x06000FD7 RID: 4055 RVA: 0x0002A57E File Offset: 0x0002877E
		[RuntimeCommand("setGameRunFlag", "")]
		[UsedImplicitly]
		public void SetGameRunFlag(string flag)
		{
			this.GameRun.ExtraFlags.Add(flag);
		}

		// Token: 0x06000FD8 RID: 4056 RVA: 0x0002A592 File Offset: 0x00028792
		[RuntimeCommand("clearGameRunFlag", "")]
		[UsedImplicitly]
		public void ClearGameRunFlag(string flag)
		{
			this.GameRun.ExtraFlags.Remove(flag);
		}

		// Token: 0x040006C7 RID: 1735
		private readonly WeakReference<Station> _station = new WeakReference<Station>(null);

		// Token: 0x040006C8 RID: 1736
		private readonly WeakReference<Stage> _stage = new WeakReference<Stage>(null);

		// Token: 0x040006C9 RID: 1737
		private readonly WeakReference<GameRunController> _gameRun = new WeakReference<GameRunController>(null);
	}
}
