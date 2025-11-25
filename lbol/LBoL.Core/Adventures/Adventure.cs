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
	public abstract class Adventure : IInitializable
	{
		public string Id { get; private set; }
		public string Title
		{
			get
			{
				return TypeFactory<Adventure>.LocalizeProperty(this.Id, "Title", false, true);
			}
		}
		public string HostName
		{
			get
			{
				return TypeFactory<Adventure>.LocalizeProperty(this.Id, "HostName", false, true);
			}
		}
		public AdventureConfig Config { get; private set; }
		public void Initialize()
		{
			this.Id = base.GetType().Name;
			this.Config = AdventureConfig.FromId(this.Id);
			if (this.Config == null)
			{
				throw new InvalidDataException("Cannot find adventure config for <" + this.Id + ">");
			}
		}
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
		internal void SetStation(Station station)
		{
			this.Station = station;
			Stage stage = station.Stage;
			this.Stage = stage;
			this.GameRun = stage.GameRun;
		}
		public virtual string DialogName
		{
			get
			{
				return base.GetType().Name;
			}
		}
		public DialogStorage Storage { get; private set; }
		public void SetStorage(DialogStorage storage)
		{
			this.Storage = storage;
			this.InitVariables(storage);
		}
		public void RestoreStorage(DialogStorage storage)
		{
			this.Storage = storage;
		}
		protected virtual void InitVariables(IVariableStorage storage)
		{
		}
		[RuntimeCommand("damage", "")]
		[UsedImplicitly]
		public void Damage(int damage, bool isSelf = true)
		{
			this.GameRun.Damage(damage, DamageType.HpLose, isSelf, true, this);
		}
		[RuntimeCommand("heal", "")]
		[UsedImplicitly]
		public void Heal(int heal, string audioName = null)
		{
			this.GameRun.Heal(heal, true, audioName);
		}
		[RuntimeCommand("healPercentage", "")]
		[UsedImplicitly]
		public void HealPercentage(int percentage)
		{
			this.GameRun.Heal(Mathf.RoundToInt((float)(this.GameRun.Player.MaxHp * percentage) / 100f), true, null);
		}
		[RuntimeCommand("healToMax", "")]
		[UsedImplicitly]
		public void HealToMax(string audioName = null)
		{
			this.GameRun.HealToMaxHp(true, audioName);
		}
		[RuntimeCommand("gainMaxHp", "")]
		[UsedImplicitly]
		public void GainMaxHp(int maxHp, bool stats = true)
		{
			this.GameRun.GainMaxHp(maxHp, true, stats);
		}
		[RuntimeCommand("loseMaxHp", "")]
		[UsedImplicitly]
		public void LoseMaxHp(int maxHp)
		{
			this.GameRun.LoseMaxHp(maxHp, true);
		}
		[RuntimeCommand("gainMoney", "")]
		[UsedImplicitly]
		public void GainMoney(int money)
		{
			this.GameRun.GainMoney(money, true, new VisualSourceData
			{
				SourceType = VisualSourceType.Vn
			});
		}
		[RuntimeCommand("consumeMoney", "")]
		[UsedImplicitly]
		public void ConsumeMoney(int money)
		{
			this.GameRun.ConsumeMoney(money);
		}
		[RuntimeCommand("loseMoney", "")]
		[UsedImplicitly]
		public void LoseMoney(int money)
		{
			this.GameRun.LoseMoney(money);
		}
		[RuntimeCommand("gainPower", "")]
		[UsedImplicitly]
		public void GainPower(int power)
		{
			this.GameRun.GainPower(power, false);
		}
		[RuntimeCommand("losePower", "")]
		[UsedImplicitly]
		public void LosePower(int power)
		{
			this.GameRun.LosePower(power, false);
		}
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
		[RuntimeCommand("generateEnemyRewards", "")]
		[UsedImplicitly]
		public void GenerateEnemyRewards()
		{
			this.Station.GenerateEnemyRewards();
		}
		[RuntimeCommand("generateEliteEnemyRewards", "")]
		[UsedImplicitly]
		public void GenerateEliteEnemyRewards()
		{
			this.Station.GenerateEliteEnemyRewards();
		}
		[RuntimeCommand("clearRewards", "")]
		[UsedImplicitly]
		public void ClearRewards()
		{
			this.Station.ClearRewards();
		}
		[RuntimeCommand("setGameRunFlag", "")]
		[UsedImplicitly]
		public void SetGameRunFlag(string flag)
		{
			this.GameRun.ExtraFlags.Add(flag);
		}
		[RuntimeCommand("clearGameRunFlag", "")]
		[UsedImplicitly]
		public void ClearGameRunFlag(string flag)
		{
			this.GameRun.ExtraFlags.Remove(flag);
		}
		private readonly WeakReference<Station> _station = new WeakReference<Station>(null);
		private readonly WeakReference<Stage> _stage = new WeakReference<Stage>(null);
		private readonly WeakReference<GameRunController> _gameRun = new WeakReference<GameRunController>(null);
	}
}
