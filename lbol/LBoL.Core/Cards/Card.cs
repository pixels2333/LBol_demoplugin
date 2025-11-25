using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core.Attributes;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Helpers;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using UnityEngine;
namespace LBoL.Core.Cards
{
	[Localizable]
	public abstract class Card : GameEntity, IVerifiable, INotifyActivating, IXCostFilter
	{
		public virtual bool OnDrawVisual
		{
			get
			{
				return true;
			}
		}
		public virtual IEnumerable<BattleAction> OnDraw()
		{
			return null;
		}
		public virtual bool OnDiscardVisual
		{
			get
			{
				return true;
			}
		}
		public virtual IEnumerable<BattleAction> OnDiscard(CardZone srcZone)
		{
			return null;
		}
		public virtual IEnumerable<BattleAction> OnTurnStartedInHand()
		{
			return null;
		}
		public virtual IEnumerable<BattleAction> OnTurnEndingInHand()
		{
			return null;
		}
		public virtual bool OnExileVisual
		{
			get
			{
				return true;
			}
		}
		public virtual IEnumerable<BattleAction> OnExile(CardZone srcZone)
		{
			return null;
		}
		public virtual bool OnMoveVisual
		{
			get
			{
				return true;
			}
		}
		public virtual IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			return null;
		}
		public virtual bool OnRetainVisual
		{
			get
			{
				return true;
			}
		}
		public virtual IEnumerable<BattleAction> OnRetain()
		{
			return null;
		}
		public virtual void OnRemove()
		{
		}
		public virtual void OnLeaveHand()
		{
			if (!this.IsXCost)
			{
				this.TurnCostDelta = ManaGroup.Empty;
			}
			if (this.IsTempRetain)
			{
				this.IsTempRetain = false;
			}
		}
		public virtual bool RemoveFromBattleAfterPlay { get; set; }
		public virtual IEnumerable<BattleAction> AfterUseAction()
		{
			if (this.RemoveFromBattleAfterPlay)
			{
				yield return this.EchoCloneAction();
				yield return new RemoveCardAction(this);
				yield break;
			}
			int num3;
			switch (this.CardType)
			{
			case CardType.Ability:
				yield return this.EchoCloneAction();
				yield return new RemoveCardAction(this);
				yield break;
			case CardType.Friend:
				if (this.Summoning)
				{
					this.IsEthereal = false;
					this.Battle.PlayerSummonAFriendThisTurn = true;
					if (this.Zone == CardZone.PlayArea)
					{
						if (this.Battle.HandZone.Count < this.Battle.MaxHand)
						{
							yield return new MoveCardAction(this, CardZone.Hand);
						}
						else
						{
							yield return new MoveCardAction(this, CardZone.Discard);
						}
					}
				}
				else if (this.UltimateUsed || this.Loyalty == 0)
				{
					yield return this.EchoCloneAction();
					yield return new RemoveCardAction(this);
					yield break;
				}
				break;
			case CardType.Tool:
				if (this.DeckCounter == null)
				{
					Debug.LogError(this.DebugName + " is tool card but has no DeckCounter");
				}
				else
				{
					Card deckCardByInstanceId = this.Battle.GameRun.GetDeckCardByInstanceId(this.InstanceId);
					int? num = this.DeckCounter - 1;
					this.DeckCounter = num;
					int? num2 = num;
					num3 = 0;
					if ((num2.GetValueOrDefault() == num3) & (num2 != null))
					{
						if (deckCardByInstanceId != null)
						{
							this.Battle.GameRun.RemoveDeckCard(deckCardByInstanceId, false);
						}
						yield return new RemoveCardAction(this);
						yield break;
					}
					if (deckCardByInstanceId != null)
					{
						deckCardByInstanceId.DeckCounter = this.DeckCounter;
					}
				}
				break;
			}
			yield return this.EchoCloneAction();
			if (this.IsExile || this.IsTempExile)
			{
				this.IsTempExile = false;
				yield return new ExileCardAction(this);
			}
			if (this.Zone == CardZone.PlayArea)
			{
				yield return new MoveCardAction(this, CardZone.Discard);
			}
			num3 = this.PlayCount + 1;
			this.PlayCount = num3;
			if (this.IsDebut)
			{
				this.DebutCardPlayedOnce = true;
			}
			yield break;
		}
		public bool IsPlayTwiceToken { get; set; }
		public Card PlayTwiceSourceCard { get; set; }
		public virtual IEnumerable<BattleAction> AfterFollowPlayAction()
		{
			if (this.IsPlayTwiceToken)
			{
				yield return new RemoveCardAction(this);
				yield break;
			}
			if (this.RemoveFromBattleAfterPlay)
			{
				yield return this.EchoCloneAction();
				yield return new RemoveCardAction(this);
				yield break;
			}
			int num3;
			switch (this.CardType)
			{
			case CardType.Ability:
				yield return this.EchoCloneAction();
				yield return new RemoveCardAction(this);
				yield break;
			case CardType.Friend:
				if (this.Summoning)
				{
					this.IsEthereal = false;
					this.Battle.PlayerSummonAFriendThisTurn = true;
					CardZone zone = this.Zone;
					if (zone == CardZone.PlayArea || zone == CardZone.FollowArea)
					{
						if (this.Battle.HandZone.Count < this.Battle.MaxHand)
						{
							yield return new MoveCardAction(this, CardZone.Hand);
						}
						else
						{
							yield return new MoveCardAction(this, CardZone.Discard);
						}
					}
				}
				else if (this.UltimateUsed || this.Loyalty == 0)
				{
					yield return this.EchoCloneAction();
					yield return new RemoveCardAction(this);
					yield break;
				}
				break;
			case CardType.Tool:
				if (this.DeckCounter == null)
				{
					Debug.LogError(this.DebugName + " is tool card but has no DeckCounter");
				}
				else
				{
					Card deckCardByInstanceId = this.Battle.GameRun.GetDeckCardByInstanceId(this.InstanceId);
					int? num = this.DeckCounter - 1;
					this.DeckCounter = num;
					int? num2 = num;
					num3 = 0;
					if ((num2.GetValueOrDefault() == num3) & (num2 != null))
					{
						if (deckCardByInstanceId != null)
						{
							this.Battle.GameRun.RemoveDeckCard(deckCardByInstanceId, false);
						}
						yield return new RemoveCardAction(this);
						yield break;
					}
					if (deckCardByInstanceId != null)
					{
						deckCardByInstanceId.DeckCounter = this.DeckCounter;
					}
				}
				break;
			}
			yield return this.EchoCloneAction();
			if (this.IsExile || this.IsTempExile)
			{
				this.IsTempExile = false;
				yield return new ExileCardAction(this);
			}
			if (this.Zone == CardZone.FollowArea)
			{
				yield return new MoveCardAction(this, CardZone.Discard);
			}
			num3 = this.PlayCount + 1;
			this.PlayCount = num3;
			if (this.IsDebut)
			{
				this.DebutCardPlayedOnce = true;
			}
			yield break;
		}
		protected BattleAction EchoCloneAction()
		{
			if (this.IsCopy)
			{
				return null;
			}
			if (this.IsEternalEcho)
			{
				Card card = this.CloneBattleCard();
				card.IsEternalEcho = false;
				card.IsExile = true;
				return new AddCardsToHandAction(new Card[] { card });
			}
			if (this.IsEcho)
			{
				this.IsEcho = false;
				Card card2 = this.CloneBattleCard();
				card2.IsExile = true;
				return new AddCardsToHandAction(new Card[] { card2 });
			}
			return null;
		}
		public virtual void Upgrade()
		{
			if (!this.Config.IsUpgradable)
			{
				throw new InvalidOperationException(this.DebugName + " is not upgradable");
			}
			if (this.IsUpgraded)
			{
				throw new InvalidOperationException(this.DebugName + " already upgraded");
			}
			this.IsUpgraded = true;
			this.ProcessKeywordUpgrade();
			this.CostChangeInUpgrading();
			if (this.Config.Loyalty != null && this.Config.UpgradedLoyalty != null)
			{
				this.Loyalty += this.Config.UpgradedLoyalty.Value - this.Config.Loyalty.Value;
			}
			this.NotifyChanged();
		}
		protected void ProcessKeywordUpgrade()
		{
			Keyword keyword = this.Keywords & ~this.Config.Keywords;
			this.Keywords = this.Config.UpgradedKeywords | keyword;
		}
		protected void CostChangeInUpgrading()
		{
			if (this.IsXCost)
			{
				return;
			}
			if (this.Config.UpgradedCost != null)
			{
				ManaGroup? upgradedCost = this.Config.UpgradedCost;
				ManaGroup cost = this.Config.Cost;
				if (upgradedCost == null || (upgradedCost != null && !(upgradedCost.GetValueOrDefault() == cost)))
				{
					ManaGroup cost2 = this.Config.Cost;
					ManaGroup value = this.Config.UpgradedCost.Value;
					ManaGroup manaGroup = cost2 - value;
					this.DecreaseBaseCost(manaGroup);
					if (manaGroup.Hybrid < 0)
					{
						List<ManaColor> getHybridColors = manaGroup.GetHybridColors;
						foreach (ManaColor manaColor in getHybridColors)
						{
							if (this.BaseCost.GetValue(manaColor) < 0 && this.BaseCost.Hybrid > 0)
							{
								int num = Math.Min(-this.BaseCost.GetValue(manaColor), this.BaseCost.Hybrid);
								this.IncreaseBaseCost(ManaGroup.FromColor(manaColor, num));
								this.DecreaseBaseCost(ManaGroup.Hybrids(num, manaGroup.HybridColor));
							}
						}
						foreach (ManaColor manaColor2 in getHybridColors)
						{
							if (this.TurnCostDelta.GetValue(manaColor2) < 0 && (this.TurnCostDelta.Hybrid > 0 || this.TurnCost.Hybrid > 0))
							{
								int num2 = Math.Min(-this.TurnCostDelta.GetValue(manaColor2), this.BaseCost.Hybrid);
								this.IncreaseTurnCost(ManaGroup.FromColor(manaColor2, num2));
								this.DecreaseTurnCost(ManaGroup.Hybrids(num2, manaGroup.HybridColor));
							}
						}
					}
					if (manaGroup.Any < 0 || this.BaseCost.IsInvalid || this.TurnCost.IsInvalid)
					{
						foreach (ManaColor manaColor3 in ManaColors.SingleColorsWithHybrid)
						{
							if (this.BaseCost.GetValue(manaColor3) < 0 && this.BaseCost.Any > 0)
							{
								int num3 = Math.Min(-this.BaseCost.GetValue(manaColor3), this.BaseCost.Any);
								this.IncreaseBaseCost(ManaGroup.FromColor(manaColor3, num3));
								this.DecreaseBaseCost(ManaGroup.Anys(num3));
							}
						}
						foreach (ManaColor manaColor4 in ManaColors.SingleColorsWithHybrid)
						{
							if (this.TurnCostDelta.GetValue(manaColor4) < 0 && (this.TurnCostDelta.Any > 0 || this.TurnCost.Any > 0))
							{
								int num4 = Math.Min(-this.TurnCostDelta.GetValue(manaColor4), this.BaseCost.Any);
								this.IncreaseTurnCost(ManaGroup.FromColor(manaColor4, num4));
								this.DecreaseTurnCost(ManaGroup.Anys(num4));
							}
						}
					}
					return;
				}
			}
		}
		public Card Clone(bool setGameRun = false)
		{
			Card card = TypeFactory<Card>.CreateInstance(base.GetType());
			if (this.IsUpgraded)
			{
				card.Upgrade();
			}
			card.IsGamerunInitial = false;
			card.DeckCounter = this.DeckCounter;
			card.Loyalty = this.Loyalty;
			card.UpgradeCounter = this.UpgradeCounter;
			if (setGameRun)
			{
				card.GameRun = base.GameRun;
			}
			return card;
		}
		public IEnumerable<Card> Clone(int count, bool setGameRun = false)
		{
			List<Card> list = new List<Card>();
			for (int i = 0; i < count; i++)
			{
				list.Add(this.Clone(setGameRun));
			}
			return list;
		}
		public Card CloneBattleCard()
		{
			Card card = TypeFactory<Card>.CreateInstance(base.GetType());
			if (this.IsUpgraded)
			{
				card.Upgrade();
			}
			card.IsGamerunInitial = false;
			card.DeckCounter = this.DeckCounter;
			card.UpgradeCounter = this.UpgradeCounter;
			card.Keywords = this.Keywords;
			card.DeltaDamage = this.DeltaDamage;
			card.DeltaBlock = this.DeltaBlock;
			card.DeltaShield = this.DeltaShield;
			card.DeltaValue1 = this.DeltaValue1;
			card.DeltaValue2 = this.DeltaValue2;
			card.DeltaInt = this.DeltaInt;
			if (!this.IsXCost)
			{
				card.BaseCost = this.BaseCost;
				card.TurnCostDelta = this.TurnCostDelta;
			}
			card.DebutCardPlayedOnce = this.DebutCardPlayedOnce;
			card.PlayCount = this.PlayCount;
			card.IsCopy = true;
			card.FreeCost = this.FreeCost;
			return card;
		}
		public Card CloneTwiceToken()
		{
			Card card = TypeFactory<Card>.CreateInstance(base.GetType());
			if (this.IsUpgraded)
			{
				card.Upgrade();
			}
			card.IsGamerunInitial = false;
			card.DeckCounter = this.DeckCounter;
			card.UpgradeCounter = this.UpgradeCounter;
			card.Keywords = this.Keywords;
			card.DeltaDamage = this.DeltaDamage;
			card.DeltaBlock = this.DeltaBlock;
			card.DeltaShield = this.DeltaShield;
			card.DeltaValue1 = this.DeltaValue1;
			card.DeltaValue2 = this.DeltaValue2;
			card.DeltaInt = this.DeltaInt;
			if (!this.IsXCost)
			{
				card.BaseCost = this.BaseCost;
				card.TurnCostDelta = this.TurnCostDelta;
			}
			card.DebutCardPlayedOnce = this.DebutCardPlayedOnce;
			card.PlayCount = this.PlayCount;
			card.IsCopy = this.IsCopy;
			card.FreeCost = this.FreeCost;
			card.Summoned = this.Summoned;
			card.Loyalty = this.Loyalty;
			return card;
		}
		internal void EnterBattle(BattleController battle)
		{
			if (this.Battle != null)
			{
				throw new InvalidOperationException("Cannot enter battle while already in battle");
			}
			this.Battle = battle;
			if (this.CardType == CardType.Friend && base.GameRun.ExtraLoyalty > 0)
			{
				this.Loyalty += base.GameRun.ExtraLoyalty;
			}
			if (this.IsForbidden && base.GameRun.RemoveBadCardForbidden > 0)
			{
				CardType cardType = this.CardType;
				if (cardType == CardType.Misfortune || cardType == CardType.Status)
				{
					this.IsForbidden = false;
					this.IsExile = true;
				}
			}
			this.OnEnterBattle(battle);
		}
		internal void LeaveBattle()
		{
			if (this.Battle == null)
			{
				throw new InvalidOperationException("Cannot leave battle while not in battle");
			}
			this.AuraCost = ManaGroup.Empty;
			this.OnLeaveBattle();
			this._handlerHolder.ClearEventHandlers();
			this.Battle = null;
		}
		protected virtual void OnEnterBattle(BattleController battle)
		{
		}
		protected virtual void OnLeaveBattle()
		{
		}
		protected void HandleBattleEvent<T>(GameEvent<T> @event, GameEventHandler<T> handler, GameEventPriority priority = (GameEventPriority)0) where T : GameEventArgs
		{
			this._handlerHolder.HandleEvent<T>(@event, handler, priority);
		}
		protected void ReactBattleEvent<TEventArgs>(GameEvent<TEventArgs> @event, EventSequencedReactor<TEventArgs> reactor, GameEventPriority priority) where TEventArgs : GameEventArgs
		{
			this.HandleBattleEvent<TEventArgs>(@event, delegate(TEventArgs args)
			{
				this.React(reactor(args));
			}, priority);
		}
		protected void ReactBattleEvent<TEventArgs>(GameEvent<TEventArgs> @event, EventSequencedReactor<TEventArgs> reactor) where TEventArgs : GameEventArgs
		{
			this.HandleBattleEvent<TEventArgs>(@event, delegate(TEventArgs args)
			{
				this.React(reactor(args));
			}, this.DefaultEventPriority);
		}
		protected override void React(Reactor reactor)
		{
			if (this.Battle == null)
			{
				Debug.LogError("[Card: " + this.DebugName + "] Cannot react outside battle");
				return;
			}
			this.Battle.React(reactor, this, ActionCause.Card);
		}
		public virtual Interaction Precondition()
		{
			if (this.CardType == CardType.Friend && this.Summoned && this.Loyalty >= this.MinLoyaltyToChooseSkill)
			{
				List<FriendToken> list = new List<FriendToken>();
				if (this.Config.ActiveCost != null && this.Loyalty >= -this.ActiveCost)
				{
					list.Add(FriendToken.Active);
				}
				if (this.Config.ActiveCost2 != null && this.Loyalty >= -this.ActiveCost2)
				{
					list.Add(FriendToken.Active2);
				}
				if (this.Config.UltimateCost != null && this.Loyalty >= -this.UltimateCost)
				{
					list.Add(FriendToken.Ultimate);
				}
				List<Card> list2 = Enumerable.ToList<Card>(this.Clone(list.Count, false));
				foreach (ValueTuple<int, FriendToken> valueTuple in list.WithIndices<FriendToken>())
				{
					int item = valueTuple.Item1;
					FriendToken item2 = valueTuple.Item2;
					list2[item].FriendToken = item2;
				}
				return new MiniSelectCardInteraction(list2, false, false, false);
			}
			return null;
		}
		protected PerformAction SkillAnime
		{
			get
			{
				return PerformAction.Animation(this.Battle.Player, "skill", 0.2f, null, 0f, -1);
			}
		}
		protected PerformAction SpellAnime
		{
			get
			{
				return PerformAction.Animation(this.Battle.Player, "spell", 0.2f, null, 0f, -1);
			}
		}
		internal IEnumerable<BattleAction> GetActions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition, bool kicker, bool summoning, IList<DamageAction> damageActions)
		{
			bool overrideAnima = false;
			if (this.Config.Perform.Length != 0)
			{
				if (this.Config.AutoPerform)
				{
					int j;
					for (int i = 0; i < this.Config.Perform.Length; i = j + 1)
					{
						yield return this.CardPerformAction(i, new Card.PerformTargetType?(Card.PerformTargetType.Player), null);
						j = i;
					}
				}
				string[][] perform = this.Config.Perform;
				for (int j = 0; j < perform.Length; j++)
				{
					if (int.Parse(perform[j][0]) == 3)
					{
						overrideAnima = true;
					}
				}
			}
			if (!overrideAnima)
			{
				if (this.CardType == CardType.Skill)
				{
					yield return this.SkillAnime;
				}
				else if (this.CardType == CardType.Ability)
				{
					yield return this.SpellAnime;
				}
			}
			this.PendingManaUsage = new ManaGroup?(consumingMana);
			if (selector.Type == TargetType.SingleEnemy)
			{
				this.PendingTarget = selector.SelectedEnemy;
			}
			if (kicker)
			{
				this.KickerPlaying = true;
				foreach (BattleAction battleAction in this.KickerActions(selector, consumingMana, precondition))
				{
					DamageAction damageAction = battleAction as DamageAction;
					if (damageAction != null)
					{
						damageActions.Add(damageAction);
					}
					yield return battleAction;
				}
				IEnumerator<BattleAction> enumerator = null;
			}
			else
			{
				this.KickerPlaying = false;
				if (summoning)
				{
					yield return this.SkillAnime;
					foreach (BattleAction battleAction2 in this.SummonActions(selector, consumingMana, precondition))
					{
						DamageAction damageAction2 = battleAction2 as DamageAction;
						if (damageAction2 != null)
						{
							damageActions.Add(damageAction2);
						}
						yield return battleAction2;
					}
					IEnumerator<BattleAction> enumerator = null;
				}
				else
				{
					foreach (BattleAction battleAction3 in this.Actions(selector, consumingMana, precondition))
					{
						DamageAction damageAction3 = battleAction3 as DamageAction;
						if (damageAction3 != null)
						{
							damageActions.Add(damageAction3);
						}
						yield return battleAction3;
					}
					IEnumerator<BattleAction> enumerator = null;
				}
			}
			this.PendingManaUsage = default(ManaGroup?);
			this.PendingTarget = null;
			yield break;
			yield break;
		}
		protected virtual IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			CardType cardType = this.CardType;
			if (cardType != CardType.Attack)
			{
				if (cardType == CardType.Defense)
				{
					yield return this.DefenseAction(true);
				}
			}
			else
			{
				this.SetGuns();
				if (this.CardGuns == null)
				{
					yield return this.AttackAction(selector, null);
				}
				else
				{
					foreach (GunPair gunPair in this.CardGuns.GunPairs)
					{
						yield return this.AttackAction(selector, gunPair);
					}
					List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
				}
			}
			yield break;
			yield break;
		}
		protected virtual IEnumerable<BattleAction> KickerActions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			return this.Actions(selector, consumingMana, precondition);
		}
		protected virtual IEnumerable<BattleAction> SummonActions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			this.Summoned = true;
			yield return PerformAction.SummonFriend(this);
			yield break;
		}
		protected BattleAction AttackAction(Unit target, DamageInfo damage, string gunName)
		{
			if (this.Battle.BattleShouldEnd)
			{
				return new EndShootAction(this.Battle.Player);
			}
			if (target != null && target.IsAlive)
			{
				return new DamageAction(this.Battle.Player, target, damage, gunName, GunType.Single);
			}
			return new EndShootAction(this.Battle.Player);
		}
		protected BattleAction AttackAction(Unit target)
		{
			return this.AttackAction(target, this.Damage, this.GunName);
		}
		protected BattleAction AttackAction(IEnumerable<Unit> targets, string gunName, DamageInfo damage)
		{
			if (this.Battle.BattleShouldEnd)
			{
				return new EndShootAction(this.Battle.Player);
			}
			List<Unit> list = Enumerable.ToList<Unit>(Enumerable.Where<Unit>(targets, (Unit u) => u.IsAlive));
			if (list.Count > 0)
			{
				return new DamageAction(this.Battle.Player, list, damage, gunName, GunType.Single);
			}
			return new EndShootAction(this.Battle.Player);
		}
		protected BattleAction AttackAction(IEnumerable<Unit> targets, string gunName)
		{
			return this.AttackAction(targets, gunName, this.Damage);
		}
		protected BattleAction AttackAction(IEnumerable<Unit> targets)
		{
			return this.AttackAction(targets, this.GunName, this.Damage);
		}
		protected BattleAction AttackAction(UnitSelector selector, DamageInfo damage, GunPair gunPair = null)
		{
			if (this.Battle.BattleShouldEnd)
			{
				return new EndShootAction(this.Battle.Player);
			}
			string text;
			GunType gunType;
			if (gunPair != null)
			{
				text = gunPair.GunName;
				gunType = gunPair.GunType;
			}
			else
			{
				text = this.GunName;
				gunType = GunType.Single;
			}
			TargetType? targetType = this.Config.TargetType;
			if (targetType != null)
			{
				switch (targetType.GetValueOrDefault())
				{
				case TargetType.SingleEnemy:
				{
					EnemyUnit enemy = selector.GetEnemy(this.Battle);
					if (enemy != null && enemy.IsAlive)
					{
						return new DamageAction(this.Battle.Player, enemy, damage, text, gunType);
					}
					return new EndShootAction(this.Battle.Player);
				}
				case TargetType.AllEnemies:
				{
					EnemyUnit[] enemies = selector.GetEnemies(this.Battle);
					if (enemies.Length != 0)
					{
						return new DamageAction(this.Battle.Player, enemies, damage, text, gunType);
					}
					return new EndShootAction(this.Battle.Player);
				}
				case TargetType.RandomEnemy:
				{
					EnemyUnit enemy2 = selector.GetEnemy(this.Battle);
					if (enemy2 != null && enemy2.IsAlive)
					{
						return new DamageAction(this.Battle.Player, enemy2, damage, text, gunType);
					}
					return new EndShootAction(this.Battle.Player);
				}
				}
			}
			throw new ArgumentException(string.Format("Invalid TargetType for {0}: {1}", this.DebugName, this.Config.TargetType));
		}
		protected BattleAction AttackAction(UnitSelector selector, GunPair gunPair = null)
		{
			return this.AttackAction(selector, this.Damage, gunPair);
		}
		protected BattleAction AttackAction(UnitSelector selector, string gunName)
		{
			return this.AttackAction(selector, this.Damage, new GunPair(gunName, GunType.Single));
		}
		protected BattleAction AttackAllAliveEnemyAction(GunPair gunPair = null)
		{
			if (this.Battle.BattleShouldEnd)
			{
				return new EndShootAction(this.Battle.Player);
			}
			string text;
			GunType gunType;
			if (gunPair != null)
			{
				text = gunPair.GunName;
				gunType = gunPair.GunType;
			}
			else
			{
				text = this.GunName;
				gunType = GunType.Single;
			}
			IEnumerable<EnemyUnit> allAliveEnemies = this.Battle.AllAliveEnemies;
			return new DamageAction(this.Battle.Player, allAliveEnemies, this.Damage, text, gunType);
		}
		protected BattleAction AttackRandomAliveEnemyAction(GunPair gunPair = null)
		{
			if (this.Battle.BattleShouldEnd)
			{
				return new EndShootAction(this.Battle.Player);
			}
			string text;
			GunType gunType;
			if (gunPair != null)
			{
				text = gunPair.GunName;
				gunType = gunPair.GunType;
			}
			else
			{
				text = this.GunName;
				gunType = GunType.Single;
			}
			EnemyUnit randomAliveEnemy = this.Battle.RandomAliveEnemy;
			if (randomAliveEnemy != null && randomAliveEnemy.IsAlive)
			{
				return new DamageAction(this.Battle.Player, randomAliveEnemy, this.Damage, text, gunType);
			}
			return new EndShootAction(this.Battle.Player);
		}
		private bool UseBurstGun
		{
			get
			{
				return (double)(this.Damage.Amount + (float)this.Battle.Player.TotalFirepower) > (double)this.ConfigDamage * 1.5 || this.IsUpgraded || this.Battle.Player.HasStatusEffect<Burst>();
			}
		}
		public string GunName
		{
			get
			{
				if (!this.UseBurstGun)
				{
					return this.Config.GunName;
				}
				if (!this.Config.GunNameBurst.IsNullOrEmpty())
				{
					return this.Config.GunNameBurst;
				}
				return this.Config.GunName;
			}
		}
		protected Guns CardGuns { get; set; }
		protected virtual void SetGuns()
		{
		}
		private bool HasBlock
		{
			get
			{
				return this.Config.Block != null || this.Config.UpgradedBlock != null;
			}
		}
		private bool HasShield
		{
			get
			{
				return this.Config.Shield != null || this.Config.UpgradedShield != null;
			}
		}
		protected BattleAction DefenseAction(bool cast = true)
		{
			return new CastBlockShieldAction(this.Battle.Player, this.HasBlock ? this.Block.Block : 0, this.HasShield ? this.Shield.Shield : 0, BlockShieldType.Normal, cast);
		}
		protected BattleAction DefenseAction(int block, int shield, BlockShieldType type = BlockShieldType.Normal, bool cast = true)
		{
			return new CastBlockShieldAction(this.Battle.Player, block, shield, type, cast);
		}
		protected BattleAction BuffAction(Type type, int level = 0, int duration = 0, int limit = 0, int count = 0, float occupationTime = 0.2f)
		{
			Unit player = this.Battle.Player;
			int? num = new int?(level);
			int? num2 = new int?(duration);
			int? num3 = new int?(limit);
			return new ApplyStatusEffectAction(type, player, num, num2, new int?(count), num3, occupationTime, true)
			{
				Args = 
				{
					Effect = 
					{
						SourceCard = this
					}
				}
			};
		}
		protected BattleAction BuffAction<TEffect>(int level = 0, int duration = 0, int limit = 0, int count = 0, float occupationTime = 0.2f)
		{
			return this.BuffAction(typeof(TEffect), level, duration, limit, count, occupationTime);
		}
		protected BattleAction DebuffAction(Type type, Unit target, int level = 0, int duration = 0, int limit = 0, int count = 0, bool startAutoDecreasing = true, float occupationTime = 0.2f)
		{
			int? num = new int?(level);
			int? num2 = new int?(duration);
			int? num3 = new int?(limit);
			return new ApplyStatusEffectAction(type, target, num, num2, new int?(count), num3, occupationTime, startAutoDecreasing)
			{
				Args = 
				{
					Effect = 
					{
						SourceCard = this
					}
				}
			};
		}
		protected BattleAction DebuffAction<TEffect>(Unit target, int level = 0, int duration = 0, int limit = 0, int count = 0, bool startAutoDecreasing = true, float occupationTime = 0.2f)
		{
			return this.DebuffAction(typeof(TEffect), target, level, duration, limit, count, startAutoDecreasing, occupationTime);
		}
		protected IEnumerable<BattleAction> DebuffAction(Type type, IEnumerable<Unit> targets, int level = 0, int duration = 0, int limit = 0, int count = 0, bool startAutoDecreasing = true, float occupationTime = 0.2f)
		{
			List<Unit> list = Enumerable.ToList<Unit>(targets);
			foreach (Unit unit in list)
			{
				Unit unit2 = unit;
				int? num = new int?(level);
				int? num2 = new int?(duration);
				int? num3 = new int?(limit);
				yield return new ApplyStatusEffectAction(type, unit2, num, num2, new int?(count), num3, (unit == Enumerable.LastOrDefault<Unit>(list)) ? occupationTime : 0f, startAutoDecreasing)
				{
					Args = 
					{
						Effect = 
						{
							SourceCard = this
						}
					}
				};
			}
			List<Unit>.Enumerator enumerator = default(List<Unit>.Enumerator);
			yield break;
			yield break;
		}
		protected IEnumerable<BattleAction> DebuffAction<TEffect>(IEnumerable<Unit> targets, int level = 0, int duration = 0, int limit = 0, int count = 0, bool startAutoDecreasing = true, float occupationTime = 0.2f)
		{
			return this.DebuffAction(typeof(TEffect), targets, level, duration, limit, count, startAutoDecreasing, occupationTime);
		}
		protected BattleAction SacrificeAction(int life)
		{
			return new DamageAction(this.Battle.Player, this.Battle.Player, DamageInfo.HpLose((float)life, false), "Sacrifice", GunType.Single);
		}
		protected BattleAction LoseLifeAction(int life)
		{
			return new DamageAction(this.Battle.Player, this.Battle.Player, DamageInfo.HpLose((float)life, false), "Instant", GunType.Single);
		}
		protected BattleAction DamageSelfAction(int damage, string gun = "")
		{
			return new DamageAction(this.Battle.Player, this.Battle.Player, DamageInfo.Reaction((float)damage, false), gun, GunType.Single);
		}
		protected BattleAction HealAction(int heal)
		{
			return new HealAction(this.Battle.Player, this.Battle.Player, heal, HealType.Normal, 0.2f);
		}
		protected BattleAction UpgradeAllHandsAction()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(this.Battle.HandZone, (Card card) => card.CanUpgradeAndPositive));
			if (list.Count <= 0)
			{
				return null;
			}
			return new UpgradeCardsAction(list);
		}
		protected BattleAction UpgradeRandomHandAction(int amount = 1, CardType firstType = CardType.Unknown)
		{
			if (amount < 1)
			{
				return null;
			}
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(this.Battle.HandZone, (Card card) => card.CanUpgradeAndPositive));
			if (firstType != CardType.Unknown)
			{
				IEnumerable<Card> enumerable = Enumerable.Where<Card>(list, (Card card) => card.CardType == firstType);
				IEnumerable<Card> enumerable2 = Enumerable.Where<Card>(list, (Card card) => card.CardType != firstType);
				List<Card> list2 = Enumerable.ToList<Card>(enumerable.SampleManyOrAll(amount, base.GameRun.BattleRng));
				if (list2.Count < amount)
				{
					list2.AddRange(enumerable2.SampleManyOrAll(amount - list2.Count, base.GameRun.BattleRng));
				}
				return new UpgradeCardsAction(list2);
			}
			if (list.Count != 0)
			{
				return new UpgradeCardsAction(list.SampleManyOrAll(amount, base.GameRun.BattleRng));
			}
			return null;
		}
		protected BattleAction DiscardRandomHandAction(int amount = 1)
		{
			if (amount <= 0)
			{
				return null;
			}
			return new DiscardManyAction(Enumerable.ToList<Card>(this.Battle.HandZone).SampleManyOrAll(amount, base.GameRun.BattleRng));
		}
		protected bool Overdrive(int consume = 1)
		{
			if (consume <= 0)
			{
				throw new InvalidOperationException("Card: " + this.Name + " is trying to overdrive with consume blow zero.");
			}
			if (this.Battle.Player.GetStatusEffect<Concentration>() != null)
			{
				return true;
			}
			Charging statusEffect = this.Battle.Player.GetStatusEffect<Charging>();
			return statusEffect != null && statusEffect.Level >= consume;
		}
		protected BattleAction OverdriveAction(int consume = 1)
		{
			if (consume <= 0)
			{
				throw new InvalidOperationException("Card: " + this.Name + " is trying to overdrive action with consume blow zero.");
			}
			Concentration statusEffect = this.Battle.Player.GetStatusEffect<Concentration>();
			if (statusEffect != null)
			{
				if (statusEffect.Level == 1)
				{
					return new RemoveStatusEffectAction(statusEffect, true, 0.1f);
				}
				Concentration concentration = statusEffect;
				int num = concentration.Level - 1;
				concentration.Level = num;
				return null;
			}
			else
			{
				Charging statusEffect2 = this.Battle.Player.GetStatusEffect<Charging>();
				if (statusEffect2 == null)
				{
					throw new InvalidOperationException("Card: " + this.Name + " is trying overdrive action but there is no charging se.");
				}
				if (statusEffect2.Level < consume)
				{
					throw new InvalidOperationException("Card: " + this.Name + " is trying overdrive action but charging se level can't afford.");
				}
				if (statusEffect2.Level == consume)
				{
					return new RemoveStatusEffectAction(statusEffect2, true, 0.1f);
				}
				statusEffect2.Level -= consume;
				return null;
			}
		}
		public int GetSeLevel(Type seType)
		{
			if (this.Battle == null || !this.Battle.Player.HasStatusEffect(seType))
			{
				return 0;
			}
			return this.Battle.Player.GetStatusEffect(seType).Level;
		}
		public int GetSeLevel<T>() where T : StatusEffect
		{
			return this.GetSeLevel(typeof(T));
		}
		protected RandomGen BattleRng
		{
			get
			{
				return base.GameRun.BattleRng;
			}
		}
		private static int CompareCost(ManaGroup a, ManaGroup b)
		{
			int num = a.Amount.CompareTo(b.Amount);
			if (num == 0)
			{
				return b.Any.CompareTo(a.Any);
			}
			return num;
		}
		public int InstanceId { get; internal set; }
		public CardConfig Config { get; private set; }
		[UsedImplicitly]
		public virtual DamageInfo Damage
		{
			get
			{
				return DamageInfo.Attack((float)this.RawDamage, this.IsAccuracy);
			}
		}
		[UsedImplicitly]
		public int RawDamage
		{
			get
			{
				return this.BasicCardModify(this.ConfigDamage + this.AdditionalDamage + this.DeltaDamage, true);
			}
		}
		public int ConfigDamage
		{
			get
			{
				int? num;
				if (!this.IsUpgraded)
				{
					num = this.Config.Damage;
				}
				else
				{
					int? upgradedDamage = this.Config.UpgradedDamage;
					num = ((upgradedDamage != null) ? upgradedDamage : this.Config.Damage);
				}
				int? num2 = num;
				if (num2 != null)
				{
					return num2.GetValueOrDefault();
				}
				throw new InvalidDataException("<" + this.DebugName + "> has empty damage config");
			}
		}
		protected virtual int AdditionalDamage
		{
			get
			{
				return 0;
			}
		}
		public int DeltaDamage
		{
			get
			{
				return this._deltaDamage;
			}
			set
			{
				if (this._deltaDamage != value)
				{
					this._deltaDamage = value;
					this.NotifyChanged();
				}
			}
		}
		private int BasicCardModify(int original, bool isDamage = false)
		{
			int num = original;
			if (this.IsBasic && base.GameRun != null)
			{
				if (isDamage && this.CardType == CardType.Attack)
				{
					num += ((this.Config.Cost.Amount > 1) ? base.GameRun.BasicAttackCardExtraDamage2 : base.GameRun.BasicAttackCardExtraDamage1);
				}
				if (base.GameRun.WanbaochuiFlag > 0 && this.PendingManaUsage != null)
				{
					int colorless = this.PendingManaUsage.Value.Colorless;
					if (colorless > 0)
					{
						num *= colorless + 1;
					}
				}
				if (base.GameRun.BasicCardIncrease > 0)
				{
					num += base.GameRun.BasicCardIncrease;
				}
			}
			return num;
		}
		[UsedImplicitly]
		public virtual BlockInfo Block
		{
			get
			{
				return new BlockInfo(this.RawBlock, BlockShieldType.Normal);
			}
		}
		[UsedImplicitly]
		public virtual int RawBlock
		{
			get
			{
				return this.BasicCardModify(this.ConfigBlock + this.AdditionalBlock + this.DeltaBlock, false);
			}
		}
		public int ConfigBlock
		{
			get
			{
				int? num;
				if (!this.IsUpgraded)
				{
					num = this.Config.Block;
				}
				else
				{
					int? upgradedBlock = this.Config.UpgradedBlock;
					num = ((upgradedBlock != null) ? upgradedBlock : this.Config.Block);
				}
				int? num2 = num;
				if (num2 != null)
				{
					return num2.GetValueOrDefault();
				}
				throw new InvalidDataException("<" + this.DebugName + "> has empty block config");
			}
		}
		protected virtual int AdditionalBlock
		{
			get
			{
				return 0;
			}
		}
		public int DeltaBlock
		{
			get
			{
				return this._deltaBlock;
			}
			set
			{
				if (this._deltaBlock != value)
				{
					this._deltaBlock = value;
					this.NotifyChanged();
				}
			}
		}
		[UsedImplicitly]
		public ShieldInfo Shield
		{
			get
			{
				return new ShieldInfo(this.RawShield, BlockShieldType.Normal);
			}
		}
		[UsedImplicitly]
		public int RawShield
		{
			get
			{
				return this.BasicCardModify(this.ConfigShield + this.AdditionalShield + this.DeltaShield, false);
			}
		}
		public int ConfigShield
		{
			get
			{
				int? num;
				if (!this.IsUpgraded)
				{
					num = this.Config.Shield;
				}
				else
				{
					int? upgradedShield = this.Config.UpgradedShield;
					num = ((upgradedShield != null) ? upgradedShield : this.Config.Shield);
				}
				int? num2 = num;
				if (num2 != null)
				{
					return num2.GetValueOrDefault();
				}
				throw new InvalidDataException("<" + this.DebugName + "> has empty Shield config");
			}
		}
		protected virtual int AdditionalShield
		{
			get
			{
				return 0;
			}
		}
		public int DeltaShield
		{
			get
			{
				return this._deltaShield;
			}
			set
			{
				if (this._deltaShield != value)
				{
					this._deltaShield = value;
					this.NotifyChanged();
				}
			}
		}
		public ScryInfo Scry
		{
			get
			{
				int? num;
				if (!this.IsUpgraded)
				{
					num = this.Config.Scry;
				}
				else
				{
					int? upgradedScry = this.Config.UpgradedScry;
					num = ((upgradedScry != null) ? upgradedScry : this.Config.Scry);
				}
				int? num2 = num;
				if (num2 != null)
				{
					int valueOrDefault = num2.GetValueOrDefault();
					return new ScryInfo(valueOrDefault);
				}
				throw new InvalidOperationException("<" + this.DebugName + " has empty Scry config");
			}
		}
		public int Value1
		{
			get
			{
				return this.BasicCardModify(this.ConfigValue1 + this.AdditionalValue1 + this.DeltaValue1, false);
			}
		}
		public int ConfigValue1
		{
			get
			{
				int? num;
				int? num2;
				if (!this.IsUpgraded)
				{
					num = this.Config.Value1;
				}
				else
				{
					num2 = this.Config.UpgradedValue1;
					num = ((num2 != null) ? num2 : this.Config.Value1);
				}
				num2 = num;
				if (num2 == null)
				{
					throw new InvalidDataException("<" + this.DebugName + "> has empty Value1 config");
				}
				return num2.GetValueOrDefault();
			}
		}
		protected virtual int AdditionalValue1
		{
			get
			{
				return 0;
			}
		}
		public int DeltaValue1
		{
			get
			{
				return this._deltaValue1;
			}
			set
			{
				if (this._deltaValue1 != value)
				{
					this._deltaValue1 = value;
					this.NotifyChanged();
				}
			}
		}
		public int Value2
		{
			get
			{
				return this.BasicCardModify(this.ConfigValue2 + this.AdditionalValue2 + this.DeltaValue2, false);
			}
		}
		public int ConfigValue2
		{
			get
			{
				int? num;
				int? num2;
				if (!this.IsUpgraded)
				{
					num = this.Config.Value2;
				}
				else
				{
					num2 = this.Config.UpgradedValue2;
					num = ((num2 != null) ? num2 : this.Config.Value2);
				}
				num2 = num;
				if (num2 == null)
				{
					throw new InvalidDataException("<" + base.Id + "> has empty Value2 config");
				}
				return num2.GetValueOrDefault();
			}
		}
		protected virtual int AdditionalValue2
		{
			get
			{
				return 0;
			}
		}
		public int DeltaValue2
		{
			get
			{
				return this._deltaValue2;
			}
			set
			{
				if (this._deltaValue2 != value)
				{
					this._deltaValue2 = value;
					this.NotifyChanged();
				}
			}
		}
		public int DeltaInt
		{
			get
			{
				return this._deltaInt;
			}
			set
			{
				if (this._deltaInt != value)
				{
					this._deltaInt = value;
					this.NotifyChanged();
				}
			}
		}
		public ManaGroup Mana
		{
			get
			{
				ManaGroup? manaGroup;
				ManaGroup? manaGroup2;
				if (!this.IsUpgraded)
				{
					manaGroup = this.Config.Mana;
				}
				else
				{
					manaGroup2 = this.Config.UpgradedMana;
					manaGroup = ((manaGroup2 != null) ? manaGroup2 : this.Config.Mana);
				}
				manaGroup2 = manaGroup;
				if (manaGroup2 == null)
				{
					throw new InvalidDataException("<" + base.Id + "> has empty Mana config");
				}
				return manaGroup2.GetValueOrDefault();
			}
		}
		public int ToolPlayableTimes
		{
			get
			{
				int? toolPlayableTimes = this.Config.ToolPlayableTimes;
				if (toolPlayableTimes == null)
				{
					throw new InvalidDataException("<" + base.Id + "> has empty ToolPlayableTimes config");
				}
				return toolPlayableTimes.GetValueOrDefault();
			}
		}
		protected PerformAction CardPerformAction(int id, Card.PerformTargetType? performType = 0, [CanBeNull] Unit target = null)
		{
			string[][] perform = this.Config.Perform;
			if (id > perform.Length || id < 0)
			{
				Debug.Log(((this != null) ? this.ToString() : null) + "的CardPerformAction不存在第" + id.ToString() + "项");
				return null;
			}
			string[] array = perform[id];
			float num = 0f;
			if (array.Length > 2)
			{
				num = float.Parse(array[2]);
			}
			Unit unit = this.Battle.Player;
			if (performType != null)
			{
				switch (performType.GetValueOrDefault())
				{
				case Card.PerformTargetType.Player:
					if (target == null)
					{
						target = this.Battle.Player;
					}
					break;
				case Card.PerformTargetType.TargetSelf:
					unit = target;
					break;
				}
			}
			PerformAction performAction;
			switch (int.Parse(array[0]))
			{
			case 1:
			{
				bool flag;
				if (performType != null)
				{
					Card.PerformTargetType performTargetType = performType.GetValueOrDefault();
					if (performTargetType - Card.PerformTargetType.EachEnemy <= 1)
					{
						flag = true;
						goto IL_00F5;
					}
				}
				flag = false;
				IL_00F5:
				if (flag)
				{
					using (IEnumerator<EnemyUnit> enumerator = this.Battle.EnemyGroup.Alives.GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							EnemyUnit enemyUnit = enumerator.Current;
							Card.PerformTargetType? performTargetType2 = performType;
							Card.PerformTargetType performTargetType = Card.PerformTargetType.EachEnemySelf;
							performAction = PerformAction.Gun(((performTargetType2.GetValueOrDefault() == performTargetType) & (performTargetType2 != null)) ? enemyUnit : unit, enemyUnit, array[1], num);
							break;
						}
					}
				}
				return PerformAction.Gun(unit, target, array[1], num);
			}
			case 2:
			{
				bool flag;
				if (performType != null)
				{
					Card.PerformTargetType performTargetType = performType.GetValueOrDefault();
					if (performTargetType - Card.PerformTargetType.EachEnemy <= 1)
					{
						flag = true;
						goto IL_0191;
					}
				}
				flag = false;
				IL_0191:
				if (flag)
				{
					string text = "All enemy names: ";
					using (IEnumerator<ValueTuple<int, EnemyUnit>> enumerator2 = this.Battle.EnemyGroup.Alives.WithIndices<EnemyUnit>().GetEnumerator())
					{
						if (enumerator2.MoveNext())
						{
							ValueTuple<int, EnemyUnit> valueTuple = enumerator2.Current;
							int item = valueTuple.Item1;
							EnemyUnit item2 = valueTuple.Item2;
							if (item == Enumerable.Count<EnemyUnit>(this.Battle.EnemyGroup.Alives) - 1)
							{
								text += item2.Name;
							}
							else
							{
								text = text + item2.Name + ", ";
							}
							performAction = PerformAction.Effect(item2, array[1], 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
							break;
						}
					}
				}
				return PerformAction.Effect(target, array[1], 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			}
			case 3:
				return PerformAction.Animation(this.Battle.Player, array[1], (array.Length > 2) ? num : 0.2f, null, 0f, -1);
			case 4:
				return PerformAction.Sfx(array[1], 0f);
			default:
				Debug.Log(((this != null) ? this.ToString() : null) + "的CardPerformAction的类型不符合预期");
				return null;
			}
			return performAction;
		}
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return (GameEventPriority)this.Config.Order;
			}
		}
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<Card>.LocalizeProperty(base.Id, key, decorated, required);
		}
		protected virtual IReadOnlyList<string> LocalizeListProperty(string key, bool required = true)
		{
			return TypeFactory<Card>.LocalizeListProperty(base.Id, key, required);
		}
		private string UpgradedBaseDescription
		{
			get
			{
				return this.LocalizeProperty("UpgradedDescription", true, false);
			}
		}
		private string NonbattleBaseDescription
		{
			get
			{
				return this.LocalizeProperty("NonbattleDescription", true, false);
			}
		}
		private string UpgradedNonbattleBaseDescription
		{
			get
			{
				return this.LocalizeProperty("UpgradedNonbattleDescription", true, false);
			}
		}
		private string ExtraDescription1
		{
			get
			{
				return this.LocalizeProperty("ExtraDescription1", true, false);
			}
		}
		private string UpgradedExtraDescription1
		{
			get
			{
				return this.LocalizeProperty("UpgradedExtraDescription1", true, false);
			}
		}
		private string ExtraDescription2
		{
			get
			{
				return this.LocalizeProperty("ExtraDescription2", true, false);
			}
		}
		private string UpgradedExtraDescription2
		{
			get
			{
				return this.LocalizeProperty("UpgradedExtraDescription2", true, false);
			}
		}
		private string ExtraDescription3
		{
			get
			{
				return this.LocalizeProperty("ExtraDescription3", true, false);
			}
		}
		private string UpgradedExtraDescription3
		{
			get
			{
				return this.LocalizeProperty("UpgradedExtraDescription3", true, false);
			}
		}
		protected bool HasExtraDescription1
		{
			get
			{
				return !this.ExtraDescription1.IsNullOrEmpty();
			}
		}
		protected string GetExtraDescription1
		{
			get
			{
				return this.FollowByDetailIcon(this.RawExtraDescription1);
			}
		}
		protected string RawExtraDescription1
		{
			get
			{
				if (!this.IsUpgraded)
				{
					return this.ExtraDescription1;
				}
				return this.UpgradedExtraDescription1 ?? this.ExtraDescription1;
			}
		}
		protected bool HasExtraDescription2
		{
			get
			{
				return !this.ExtraDescription2.IsNullOrEmpty();
			}
		}
		protected string GetExtraDescription2
		{
			get
			{
				return this.FollowByDetailIcon(this.RawExtraDescription2);
			}
		}
		protected string RawExtraDescription2
		{
			get
			{
				if (!this.IsUpgraded)
				{
					return this.ExtraDescription2;
				}
				return this.UpgradedExtraDescription2 ?? this.ExtraDescription2;
			}
		}
		protected bool HasExtraDescription3
		{
			get
			{
				return !this.ExtraDescription3.IsNullOrEmpty();
			}
		}
		protected string GetExtraDescription3
		{
			get
			{
				return this.FollowByDetailIcon(this.RawExtraDescription3);
			}
		}
		protected string RawExtraDescription3
		{
			get
			{
				if (!this.IsUpgraded)
				{
					return this.ExtraDescription3;
				}
				return this.UpgradedExtraDescription3 ?? this.ExtraDescription3;
			}
		}
		public int ChoiceCardIndicator { get; set; }
		protected override string GetBaseDescription()
		{
			if (this.Config.EmptyDescription)
			{
				return string.Empty;
			}
			string text = string.Empty;
			if (this.CardType == CardType.Friend)
			{
				switch (this.FriendToken)
				{
				case FriendToken.None:
					if (!this.Config.EmptyDescription)
					{
						text += (this.IsUpgraded ? (this.UpgradedBaseDescription ?? base.BaseDescription) : base.BaseDescription);
					}
					if (this.HasExtraDescription1)
					{
						text += "\n";
						text += this.RawExtraDescription1;
					}
					if (this.HasExtraDescription2)
					{
						text += "\n";
						text += this.RawExtraDescription2;
					}
					if (this.HasExtraDescription3)
					{
						text += "\n";
						text += this.RawExtraDescription3;
					}
					break;
				case FriendToken.Active:
					text += this.RawExtraDescription1;
					break;
				case FriendToken.Active2:
					text += this.RawExtraDescription2;
					break;
				case FriendToken.Ultimate:
					text += this.RawExtraDescription3;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			else
			{
				int choiceCardIndicator = this.ChoiceCardIndicator;
				if (choiceCardIndicator != 1)
				{
					if (choiceCardIndicator != 2)
					{
						if (this.Battle != null)
						{
							text = (this.IsUpgraded ? (this.UpgradedBaseDescription ?? base.BaseDescription) : base.BaseDescription);
						}
						else
						{
							string text2;
							if (!this.IsUpgraded)
							{
								text2 = this.NonbattleBaseDescription ?? base.BaseDescription;
							}
							else if ((text2 = this.UpgradedNonbattleBaseDescription ?? this.UpgradedBaseDescription) == null)
							{
								text2 = this.NonbattleBaseDescription ?? base.BaseDescription;
							}
							text = text2;
						}
					}
					else
					{
						text = this.RawExtraDescription2;
					}
				}
				else
				{
					text = this.RawExtraDescription1;
				}
			}
			return this.FollowByDetailIcon(text);
		}
		private string BaseFlavorText
		{
			get
			{
				return this.LocalizeProperty("FlavorText", false, false);
			}
		}
		public string FlavorText
		{
			get
			{
				string baseFlavorText = this.BaseFlavorText;
				if (baseFlavorText == null)
				{
					return null;
				}
				return baseFlavorText.RuntimeFormat(this);
			}
		}
		private string BaseDetailText
		{
			get
			{
				return this.LocalizeProperty("DetailText", true, false);
			}
		}
		private string FollowByDetailIcon(string des)
		{
			if (this.BaseDetailText != null)
			{
				des += "<sprite=\"TextIcon\" name=\"Info\">";
			}
			return des;
		}
		public string DetailText
		{
			get
			{
				if (this.BaseDetailText == null)
				{
					return null;
				}
				return "<sprite=\"TextIcon\" name=\"Info\"> " + this.BaseDetailText.RuntimeFormat(this);
			}
		}
		public CardType CardType
		{
			get
			{
				return this.Config.Type;
			}
		}
		[UsedImplicitly]
		public int MoneyCost
		{
			get
			{
				int? moneyCost = this.Config.MoneyCost;
				if (moneyCost == null)
				{
					throw new InvalidDataException("<" + this.DebugName + "> has empty MoneyCost");
				}
				return moneyCost.GetValueOrDefault();
			}
		}
		public bool IsXCost
		{
			get
			{
				return this.Config.IsXCost;
			}
		}
		public virtual ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			if (!this.IsXCost)
			{
				throw new InvalidOperationException("Cannot get X cost from non-x-cost card");
			}
			return pooledMana;
		}
		public ManaGroup XCostRequiredMana
		{
			get
			{
				return this.ConfigCost;
			}
		}
		public bool HasKicker
		{
			get
			{
				if (!this.IsUpgraded)
				{
					return this.Config.Kicker != null;
				}
				return this.Config.UpgradedKicker != null;
			}
		}
		public ManaGroup ConfigKicker
		{
			get
			{
				ManaGroup? manaGroup = (this.IsUpgraded ? this.Config.UpgradedKicker : this.Config.Kicker);
				if (manaGroup == null)
				{
					throw new InvalidDataException("<" + this.DebugName + "> has empty Kicker config");
				}
				return manaGroup.GetValueOrDefault();
			}
		}
		public ManaGroup KickerDelta
		{
			get
			{
				return this._kickerDelta;
			}
			set
			{
				this._kickerDelta = value;
				this.NotifyChanged();
			}
		}
		public ManaGroup KickerCost
		{
			get
			{
				return (this.ConfigKicker + this.KickerDelta).Corrected;
			}
		}
		public ManaGroup KickerTotalCost
		{
			get
			{
				if (!this.IsXCost)
				{
					return this.Cost + this.KickerCost;
				}
				return this.XCostRequiredMana + this.KickerCost;
			}
		}
		public ManaGroup ConfigCost
		{
			get
			{
				if (!this.IsUpgraded)
				{
					return this.Config.Cost;
				}
				ManaGroup? upgradedCost = this.Config.UpgradedCost;
				if (upgradedCost == null)
				{
					return this.Config.Cost;
				}
				return upgradedCost.GetValueOrDefault();
			}
		}
		public ManaGroup CostToMana(bool baseCost)
		{
			if (!baseCost && !this.IsXCost)
			{
				return this.Cost.CostToMana();
			}
			return this.ConfigCost.CostToMana();
		}
		public bool FreeCost { get; set; }
		public virtual bool IsForceCost
		{
			get
			{
				return false;
			}
		}
		public virtual ManaGroup ForceCost
		{
			get
			{
				return ManaGroup.Empty;
			}
		}
		public ManaGroup Cost
		{
			get
			{
				if (this.IsXCost)
				{
					return this.BaseCost;
				}
				if (this.FreeCost || this.Summoned)
				{
					return ManaGroup.Empty;
				}
				if (this.IsForceCost)
				{
					return this.ForceCost;
				}
				ManaGroup manaGroup = (this.TurnCost + this.AdditionalCost + this.AuraCost).Corrected;
				manaGroup = (this.IsPurified ? manaGroup.Purified() : manaGroup);
				BattleController battle = this.Battle;
				if (battle != null && battle.ManaFreezeLevel > 0 && this.Zone == CardZone.Hand)
				{
					return manaGroup + ManaGroup.Anys(this.Battle.ManaFreezeLevel);
				}
				return manaGroup;
			}
		}
		protected virtual ManaGroup AdditionalCost
		{
			get
			{
				return ManaGroup.Empty;
			}
		}
		public ManaGroup AuraCost
		{
			get
			{
				return this._auraCost;
			}
			set
			{
				if (this.IsXCost)
				{
					Debug.LogWarning("Cannot set delta cost of " + this.DebugName + " because it is X-cost card");
					return;
				}
				this._auraCost = value;
				this.NotifyChanged();
			}
		}
		public ManaGroup BaseCost
		{
			get
			{
				return this._baseCost;
			}
			private set
			{
				if (this.IsXCost)
				{
					Debug.LogWarning("Cannot set base cost of " + this.DebugName + " because it is X-cost card");
					return;
				}
				this._baseCost = value;
				this.IsMorph = this._baseCost != this.ConfigCost;
				this.NotifyChanged();
			}
		}
		public ManaGroup TurnCost
		{
			get
			{
				return this.BaseCost + this.TurnCostDelta;
			}
		}
		public ManaGroup TurnCostDelta
		{
			get
			{
				return this._turnCostDelta;
			}
			private set
			{
				if (this.IsXCost)
				{
					Debug.LogWarning("Cannot set Turn cost of " + this.DebugName + " because it is X-cost card");
					return;
				}
				this._turnCostDelta = value;
				this.IsTempMorph = this._turnCostDelta != ManaGroup.Empty;
				this.NotifyChanged();
			}
		}
		public void SetTurnCost(ManaGroup cost)
		{
			if (this.IsXCost)
			{
				Debug.LogWarning("Cannot set turn cost of " + this.DebugName + " because it is X-cost card");
				return;
			}
			this.TurnCostDelta = cost - this.BaseCost;
		}
		public void IncreaseTurnCost(ManaGroup cost)
		{
			if (this.IsXCost)
			{
				Debug.LogWarning("Cannot set turn cost of " + this.DebugName + " because it is X-cost card");
				return;
			}
			this.TurnCostDelta += cost;
		}
		public void DecreaseTurnCost(ManaGroup cost)
		{
			if (this.IsXCost)
			{
				Debug.LogWarning("Cannot set turn cost of " + this.DebugName + " because it is X-cost card");
				return;
			}
			this.TurnCostDelta -= cost;
		}
		public void SetBaseCost(ManaGroup cost)
		{
			if (this.IsXCost)
			{
				Debug.LogWarning("Cannot set base cost of " + this.DebugName + " because it is X-cost card");
				return;
			}
			this.BaseCost = cost;
		}
		public void IncreaseBaseCost(ManaGroup cost)
		{
			if (this.IsXCost)
			{
				Debug.LogWarning("Cannot set base cost of " + this.DebugName + " because it is X-cost card");
				return;
			}
			this.BaseCost += cost;
		}
		public void DecreaseBaseCost(ManaGroup cost)
		{
			if (this.IsXCost)
			{
				Debug.LogWarning("Cannot set base cost of " + this.DebugName + " because it is X-cost card");
				return;
			}
			this.BaseCost -= cost;
		}
		public CardZone Zone { get; internal set; }
		public int HandIndexWhenPlaying { get; internal set; }
		public virtual bool Triggered
		{
			get
			{
				return (this.IsDebut && this.DebutActive) || (this.IsInstinct && this.InstinctActive) || (this.IsOverdrive && this.Overdrive(this.Value2)) || (this.CardType == CardType.Friend && this.Config.UltimateCost != null && this.Loyalty >= -this.UltimateCost);
			}
		}
		public bool PlayInTriggered { get; set; }
		public bool TriggeredAnyhow
		{
			get
			{
				return this.Triggered || this.PlayInTriggered;
			}
		}
		public bool DebutCardPlayedOnce
		{
			get
			{
				return this._debutCardPlayOnce;
			}
			set
			{
				if (this._debutCardPlayOnce != value)
				{
					this._debutCardPlayOnce = value;
					this.NotifyChanged();
				}
			}
		}
		public bool KickerPlaying { get; set; }
		public bool DebutActive
		{
			get
			{
				return !this.DebutCardPlayedOnce;
			}
		}
		public int PlayCount
		{
			get
			{
				return this._playCount;
			}
			set
			{
				if (this._playCount != value)
				{
					this._playCount = value;
					this.NotifyChanged();
				}
			}
		}
		public int GrowCount
		{
			get
			{
				if (this.Battle != null)
				{
					return this.PlayCount + this.Battle.CardExtraGrowAmount;
				}
				return this.PlayCount;
			}
		}
		public virtual ManaGroup? PlentifulMana
		{
			get
			{
				return default(ManaGroup?);
			}
		}
		public bool PlentifulHappenThisTurn { get; set; }
		private bool InstinctActive
		{
			get
			{
				return this.Battle != null && this.Zone == CardZone.Hand && this.Battle.HandZone.Count >= 1 && (this == Enumerable.First<Card>(this.Battle.HandZone) || this == Enumerable.Last<Card>(this.Battle.HandZone));
			}
		}
		public bool IsMostLeftHand
		{
			get
			{
				return this.Battle != null && this.Zone == CardZone.Hand && this.Battle.HandZone.Count >= 1 && this == Enumerable.First<Card>(this.Battle.HandZone);
			}
		}
		public bool IsMostRightHand
		{
			get
			{
				return this.Battle != null && this.Zone == CardZone.Hand && this.Battle.HandZone.Count >= 1 && this == Enumerable.Last<Card>(this.Battle.HandZone);
			}
		}
		public int? DeckCounter
		{
			get
			{
				return this._deckCounter;
			}
			set
			{
				int? deckCounter = this._deckCounter;
				int? num = value;
				if (!((deckCounter.GetValueOrDefault() == num.GetValueOrDefault()) & (deckCounter != null == (num != null))))
				{
					this._deckCounter = value;
					this.NotifyChanged();
				}
			}
		}
		public int? UpgradeCounter
		{
			get
			{
				return this._upgradeCounter;
			}
			set
			{
				int? upgradeCounter = this._upgradeCounter;
				int? num = value;
				if (!((upgradeCounter.GetValueOrDefault() == num.GetValueOrDefault()) & (upgradeCounter != null == (num != null))))
				{
					this._upgradeCounter = value;
					this.NotifyChanged();
				}
			}
		}
		public virtual bool CanUpgrade
		{
			get
			{
				return this.Config.IsUpgradable && !this.IsUpgraded;
			}
		}
		public bool CanUpgradeAndPositive
		{
			get
			{
				return this.CanUpgrade && this.UpgradeIsPositive;
			}
		}
		public virtual bool IsUpgraded { get; private set; }
		public virtual bool DiscardCard
		{
			get
			{
				return false;
			}
		}
		public virtual bool ShuffleToBottom
		{
			get
			{
				return false;
			}
		}
		public virtual bool UpgradeIsPositive
		{
			get
			{
				return this.Positive;
			}
		}
		public bool Positive
		{
			get
			{
				return !this.Negative;
			}
		}
		public virtual bool Negative
		{
			get
			{
				CardType cardType = this.CardType;
				return cardType == CardType.Status || cardType == CardType.Misfortune;
			}
		}
		public bool IsPlayerCard
		{
			get
			{
				if (this.Config.Owner == null)
				{
					return false;
				}
				string owner = this.Config.Owner;
				GameRunController gameRun = base.GameRun;
				return owner == ((gameRun != null) ? gameRun.Player.Id : null);
			}
		}
		public bool IsMultiColor
		{
			get
			{
				return this.Config.Colors.Count >= 1;
			}
		}
		public bool UseTransparentTexture
		{
			get
			{
				return this.IsCopy || this.IsAutoExile;
			}
		}
		public Keyword Keywords { get; internal set; }
		public bool HasKeyword(Keyword keyword)
		{
			return this.Keywords.HasFlag(keyword);
		}
		public void SetKeyword(Keyword keyword, bool value)
		{
			if (value != this.HasKeyword(keyword))
			{
				this.Keywords ^= keyword;
				this.NotifyChanged();
			}
		}
		public bool IsGamerunInitial
		{
			get
			{
				return this.HasKeyword(Keyword.GamerunInitial);
			}
			set
			{
				this.SetKeyword(Keyword.GamerunInitial, value);
			}
		}
		public bool IsBasic
		{
			get
			{
				return this.HasKeyword(Keyword.Basic);
			}
			set
			{
				this.SetKeyword(Keyword.Basic, value);
			}
		}
		public bool Unremovable
		{
			get
			{
				return this.HasKeyword(Keyword.Unremovable);
			}
			set
			{
				this.SetKeyword(Keyword.Unremovable, value);
			}
		}
		public bool IsCopy
		{
			get
			{
				return this.HasKeyword(Keyword.Copy);
			}
			set
			{
				this.SetKeyword(Keyword.Copy, value);
			}
		}
		public bool IsAccuracy
		{
			get
			{
				return this.HasKeyword(Keyword.Accuracy);
			}
			set
			{
				this.SetKeyword(Keyword.Accuracy, value);
			}
		}
		public bool IsForbidden
		{
			get
			{
				return this.HasKeyword(Keyword.Forbidden);
			}
			set
			{
				this.SetKeyword(Keyword.Forbidden, value);
			}
		}
		public bool IsExile
		{
			get
			{
				return this.HasKeyword(Keyword.Exile);
			}
			set
			{
				if (this.CardType == CardType.Friend)
				{
					this.IsEthereal = value;
					return;
				}
				this.SetKeyword(Keyword.Exile, value);
			}
		}
		public bool IsTempExile { get; set; }
		public bool IsEcho
		{
			get
			{
				return this.HasKeyword(Keyword.Echo);
			}
			set
			{
				this.SetKeyword(Keyword.Echo, value);
			}
		}
		public bool IsEternalEcho
		{
			get
			{
				return this.HasKeyword(Keyword.EternalEcho);
			}
			set
			{
				this.SetKeyword(Keyword.EternalEcho, value);
			}
		}
		public bool IsEthereal
		{
			get
			{
				return this.HasKeyword(Keyword.Ethereal);
			}
			set
			{
				this.SetKeyword(Keyword.Ethereal, value);
			}
		}
		public bool IsInitial
		{
			get
			{
				return this.HasKeyword(Keyword.Initial);
			}
			set
			{
				this.SetKeyword(Keyword.Initial, value);
			}
		}
		public bool IsRetain
		{
			get
			{
				return this.HasKeyword(Keyword.Retain);
			}
			set
			{
				this.SetKeyword(Keyword.Retain, value);
			}
		}
		public bool IsTempRetain
		{
			get
			{
				return this.HasKeyword(Keyword.TempRetain);
			}
			set
			{
				this.SetKeyword(Keyword.TempRetain, value);
			}
		}
		public bool IsReplenish
		{
			get
			{
				return this.HasKeyword(Keyword.Replenish);
			}
			set
			{
				this.SetKeyword(Keyword.Replenish, value);
			}
		}
		public bool IsPlentiful
		{
			get
			{
				return this.HasKeyword(Keyword.Plentiful);
			}
			set
			{
				this.SetKeyword(Keyword.Plentiful, value);
			}
		}
		public bool IsPurified
		{
			get
			{
				return this.HasKeyword(Keyword.Purified);
			}
			set
			{
				this.SetKeyword(Keyword.Purified, value);
			}
		}
		public bool IsAutoExile
		{
			get
			{
				return this.HasKeyword(Keyword.AutoExile);
			}
			set
			{
				this.SetKeyword(Keyword.AutoExile, value);
			}
		}
		public bool IsFollowCard
		{
			get
			{
				return this.HasKeyword(Keyword.FollowCard);
			}
			set
			{
				this.SetKeyword(Keyword.FollowCard, value);
			}
		}
		public bool IsDreamCard
		{
			get
			{
				return this.HasKeyword(Keyword.DreamCard);
			}
			set
			{
				this.SetKeyword(Keyword.DreamCard, value);
			}
		}
		public bool IsDebut
		{
			get
			{
				return this.HasKeyword(Keyword.Debut);
			}
			set
			{
				this.SetKeyword(Keyword.Debut, value);
			}
		}
		public bool IsInstinct
		{
			get
			{
				return this.HasKeyword(Keyword.Instinct);
			}
			set
			{
				this.SetKeyword(Keyword.Instinct, value);
			}
		}
		public bool IsMorph
		{
			get
			{
				return this.HasKeyword(Keyword.Morph);
			}
			set
			{
				this.SetKeyword(Keyword.Morph, value);
			}
		}
		public bool IsTempMorph
		{
			get
			{
				return this.HasKeyword(Keyword.TempMorph);
			}
			set
			{
				this.SetKeyword(Keyword.TempMorph, value);
			}
		}
		public bool IsOverdrive
		{
			get
			{
				return this.HasKeyword(Keyword.Overdrive);
			}
			set
			{
				this.SetKeyword(Keyword.Overdrive, value);
			}
		}
		public virtual bool CanUse
		{
			get
			{
				return true;
			}
		}
		public virtual string CantUseMessage
		{
			get
			{
				return "ErrorChat.CardNotUsable".Localize(true);
			}
		}
		public ManaGroup? PendingManaUsage
		{
			get
			{
				return this._pendingManaUsage;
			}
			set
			{
				if (value != this._pendingManaUsage)
				{
					this._pendingManaUsage = value;
					this.NotifyChanged();
				}
			}
		}
		public Unit PendingTarget
		{
			get
			{
				return this._pendingTarget;
			}
			set
			{
				if (value != this._pendingTarget)
				{
					this._pendingTarget = value;
					this.NotifyChanged();
				}
			}
		}
		public bool CanBeDuplicated
		{
			get
			{
				return this.CardType != CardType.Tool && !this.IsCopy && this.Config.FindInBattle;
			}
		}
		public int ConfigLoyalty
		{
			get
			{
				int? num;
				int? num2;
				if (!this.IsUpgraded)
				{
					num = this.Config.Loyalty;
				}
				else
				{
					num2 = this.Config.UpgradedLoyalty;
					num = ((num2 != null) ? num2 : this.Config.Loyalty);
				}
				num2 = num;
				if (num2 == null)
				{
					throw new InvalidDataException("<" + this.DebugName + "> has empty Loyalty config");
				}
				return num2.GetValueOrDefault();
			}
		}
		public int Loyalty
		{
			get
			{
				return this._loyalty;
			}
			set
			{
				int num = Math.Min(value, 9);
				if (this._loyalty != num)
				{
					this._loyalty = num;
					this.NotifyChanged();
				}
			}
		}
		public int PassiveCost
		{
			get
			{
				int? num;
				int? num2;
				if (!this.IsUpgraded)
				{
					num = this.Config.PassiveCost;
				}
				else
				{
					num2 = this.Config.UpgradedPassiveCost;
					num = ((num2 != null) ? num2 : this.Config.PassiveCost);
				}
				num2 = num;
				if (num2 == null)
				{
					throw new InvalidDataException("<" + this.DebugName + "> has empty PassiveCost config");
				}
				return num2.GetValueOrDefault();
			}
		}
		public int ActiveCost
		{
			get
			{
				int? num;
				int? num2;
				if (!this.IsUpgraded)
				{
					num = this.Config.ActiveCost;
				}
				else
				{
					num2 = this.Config.UpgradedActiveCost;
					num = ((num2 != null) ? num2 : this.Config.ActiveCost);
				}
				num2 = num;
				if (num2 == null)
				{
					throw new InvalidDataException("<" + this.DebugName + "> has empty ActiveCost1 config");
				}
				return num2.GetValueOrDefault();
			}
		}
		public int ActiveCost2
		{
			get
			{
				int? num;
				int? num2;
				if (!this.IsUpgraded)
				{
					num = this.Config.ActiveCost2;
				}
				else
				{
					num2 = this.Config.UpgradedActiveCost2;
					num = ((num2 != null) ? num2 : this.Config.ActiveCost2);
				}
				num2 = num;
				if (num2 == null)
				{
					throw new InvalidDataException("<" + this.DebugName + "> has empty ActiveCost2 config");
				}
				return num2.GetValueOrDefault();
			}
		}
		public int UltimateCost
		{
			get
			{
				int? num;
				int? num2;
				if (!this.IsUpgraded)
				{
					num = this.Config.UltimateCost;
				}
				else
				{
					num2 = this.Config.UpgradedUltimateCost;
					num = ((num2 != null) ? num2 : this.Config.UltimateCost);
				}
				num2 = num;
				if (num2 == null)
				{
					throw new InvalidDataException("<" + this.DebugName + "> has empty UltimateCost config");
				}
				return num2.GetValueOrDefault();
			}
		}
		private int MinActiveCost
		{
			get
			{
				if (this.Config.ActiveCost == null && this.Config.ActiveCost2 == null && this.Config.UltimateCost == null)
				{
					Debug.LogError(this.DebugName + " has no loyalty cost.");
					return -10;
				}
				int num = -9;
				if (this.Config.ActiveCost != null)
				{
					num = this.ActiveCost;
				}
				if (this.Config.ActiveCost2 != null)
				{
					num = Math.Max(num, this.ActiveCost2);
				}
				if (this.Config.UltimateCost != null)
				{
					num = Math.Max(num, this.UltimateCost);
				}
				return num;
			}
		}
		public int MinLoyaltyToUseSkill
		{
			get
			{
				return -this.MinActiveCost;
			}
		}
		private int MinSecondActiveCost
		{
			get
			{
				int num2;
				int num = (num2 = -10);
				if (this.Config.ActiveCost != null)
				{
					num = this.ActiveCost;
				}
				if (this.Config.ActiveCost2 != null)
				{
					if (this.ActiveCost2 > num)
					{
						num2 = num;
						num = this.ActiveCost2;
					}
					else
					{
						num2 = this.ActiveCost2;
					}
				}
				if (this.Config.UltimateCost != null)
				{
					if (this.UltimateCost > num)
					{
						num2 = num;
						num = this.UltimateCost;
					}
					else if (this.UltimateCost > num2)
					{
						num2 = this.UltimateCost;
					}
				}
				return num2;
			}
		}
		private int MinLoyaltyToChooseSkill
		{
			get
			{
				return -this.MinSecondActiveCost;
			}
		}
		[UsedImplicitly]
		public string FriendS
		{
			get
			{
				return "<indent=80>";
			}
		}
		[UsedImplicitly]
		public virtual FriendCostInfo FriendP
		{
			get
			{
				return new FriendCostInfo(this.PassiveCost, FriendCostType.Passive);
			}
		}
		[UsedImplicitly]
		public virtual FriendCostInfo FriendA
		{
			get
			{
				return new FriendCostInfo(this.ActiveCost, FriendCostType.Active);
			}
		}
		[UsedImplicitly]
		public virtual FriendCostInfo FriendA2
		{
			get
			{
				return new FriendCostInfo(this.ActiveCost2, FriendCostType.Active);
			}
		}
		[UsedImplicitly]
		public virtual FriendCostInfo FriendU
		{
			get
			{
				return new FriendCostInfo(this.UltimateCost, FriendCostType.Ultimate);
			}
		}
		public bool Summoning { get; internal set; }
		public bool Summoned { get; set; }
		public bool UltimateUsed { get; set; }
		public void Summon()
		{
			this.Summoning = true;
			this.Summoned = true;
		}
		public FriendToken FriendToken { get; set; }
		public virtual IEnumerable<BattleAction> GetPassiveActions()
		{
			return null;
		}
		public BattleController Battle
		{
			get
			{
				BattleController battleController;
				if (!this._battle.TryGetTarget(ref battleController))
				{
					return null;
				}
				return battleController;
			}
			private set
			{
				this._battle.SetTarget(value);
			}
		}
		public void SetBattle(BattleController battle)
		{
			this.Battle = battle;
			base.GameRun = battle.GameRun;
		}
		public EntityName GetName()
		{
			return new EntityName("Card." + base.Id, this.Name);
		}
		public override string Description
		{
			get
			{
				string description = base.Description;
				if (this.Keywords == Keyword.None)
				{
					return description;
				}
				string text = UiUtils.WrapByColor(" ".Join(this.EnumerateAutoAppendKeywordNames()), GlobalConfig.DefaultKeywordColor);
				if (string.IsNullOrWhiteSpace(description))
				{
					return text;
				}
				return description + "\n" + text;
			}
		}
		public Keyword ConfigRelativeKeywords
		{
			get
			{
				if (!this.IsUpgraded)
				{
					return this.Config.RelativeKeyword;
				}
				return this.Config.UpgradedRelativeKeyword;
			}
		}
		public IEnumerable<Keyword> EnumerateCardKeywords()
		{
			return LBoL.Core.Keywords.EnumerateComponents(this.Keywords);
		}
		public IEnumerable<IDisplayWord> EnumerateDisplayWords(bool verbose)
		{
			CardConfig config = this.Config;
			IReadOnlyList<string> readOnlyList = (this.IsUpgraded ? config.UpgradedRelativeEffects : config.RelativeEffects);
			Keyword keyword = (this.IsUpgraded ? config.UpgradedRelativeKeyword : config.RelativeKeyword);
			switch (this.CardType)
			{
			case CardType.Ability:
				keyword |= Keyword.Ability;
				break;
			case CardType.Friend:
				keyword |= (this.Summoned ? Keyword.FriendSummoned : Keyword.Friend);
				keyword |= Keyword.Loyalty;
				break;
			case CardType.Tool:
				keyword |= Keyword.Tool;
				break;
			case CardType.Misfortune:
				keyword |= Keyword.Misfortune;
				break;
			}
			if (this.IsXCost)
			{
				keyword |= Keyword.XCost;
			}
			if (this.HasKicker)
			{
				keyword |= Keyword.Kicker;
			}
			if (this.IsUpgraded)
			{
				if (config.UpgradedCost != null || config.Cost.Hybrid <= 0)
				{
					ManaGroup? upgradedCost = config.UpgradedCost;
					if (upgradedCost == null || upgradedCost.GetValueOrDefault().Hybrid <= 0)
					{
						goto IL_0171;
					}
				}
				keyword |= Keyword.HybridCost;
			}
			else if (config.Cost.Hybrid > 0)
			{
				keyword |= Keyword.HybridCost;
			}
			IL_0171:
			foreach (IDisplayWord displayWord in Library.InternalEnumerateDisplayWords(base.GameRun, keyword, readOnlyList, verbose, new Keyword?(this.Keywords)))
			{
				yield return displayWord;
			}
			IEnumerator<IDisplayWord> enumerator = null;
			yield break;
			yield break;
		}
		public virtual IEnumerable<Card> EnumerateRelativeCards()
		{
			CardConfig config = this.Config;
			IReadOnlyList<string> readOnlyList = (this.IsUpgraded ? config.UpgradedRelativeCards : (config.RelativeCards ?? Array.Empty<string>()));
			foreach (string text in readOnlyList)
			{
				Card card;
				if (Enumerable.Last<char>(text) == '+')
				{
					string text2 = text;
					card = Library.CreateCard(text2.Substring(0, text2.Length - 1));
					card.Upgrade();
				}
				else
				{
					card = Library.CreateCard(text);
				}
				card.GameRun = base.GameRun;
				yield return card;
			}
			IEnumerator<string> enumerator = null;
			yield break;
			yield break;
		}
		private IEnumerable<string> EnumerateAutoAppendKeywordNames()
		{
			foreach (Keyword keyword in LBoL.Core.Keywords.EnumerateComponents(this.Keywords))
			{
				KeywordDisplayWord displayWord = LBoL.Core.Keywords.GetDisplayWord(keyword);
				if (displayWord != null)
				{
					if (displayWord.IsAutoAppend)
					{
						yield return displayWord.Name;
					}
				}
				else
				{
					Debug.LogWarning(string.Format("Cannot find display-word for {0}, maybe not localized", keyword));
					yield return "&lt;" + keyword.ToString() + "&gt;";
				}
			}
			IEnumerator<Keyword> enumerator = null;
			yield break;
			yield break;
		}
		public override void Initialize()
		{
			base.Initialize();
			this.Config = CardConfig.FromId(base.Id);
			if (this.Config == null)
			{
				throw new InvalidDataException("Cannot find card config for <" + base.Id + ">");
			}
			this.Keywords = this.Config.Keywords;
			if (!this.Config.IsXCost)
			{
				this.BaseCost = this.ConfigCost;
				this.TurnCostDelta = ManaGroup.Empty;
			}
			if (this.CardType == CardType.Tool)
			{
				this.DeckCounter = new int?(this.ToolPlayableTimes);
			}
			if (this.CardType == CardType.Friend)
			{
				this.Loyalty = this.ConfigLoyalty;
			}
			if (this.PlentifulMana != null)
			{
				this.IsPlentiful = true;
			}
		}
		internal override GameEntityFormatWrapper CreateFormatWrapper()
		{
			return new Card.CardFormatWrapper(this);
		}
		protected void HandleBattleCardsChangedEvents(GameEventHandler<GameEventArgs> handler, bool hand = false, bool draw = false, bool discard = false, bool exile = false)
		{
			this.HandleBattleEvent<CardEventArgs>(this.Battle.CardExiled, handler, (GameEventPriority)0);
			this.HandleBattleEvent<CardEventArgs>(this.Battle.CardRemoved, handler, (GameEventPriority)0);
			this.HandleBattleEvent<CardMovingEventArgs>(this.Battle.CardMoved, handler, (GameEventPriority)0);
			this.HandleBattleEvent<CardMovingToDrawZoneEventArgs>(this.Battle.CardMovedToDrawZone, handler, (GameEventPriority)0);
			if (hand)
			{
				this.HandleBattleEvent<CardsEventArgs>(this.Battle.CardsAddedToHand, handler, (GameEventPriority)0);
			}
			if (draw)
			{
				this.HandleBattleEvent<CardsAddingToDrawZoneEventArgs>(this.Battle.CardsAddedToDrawZone, handler, (GameEventPriority)0);
			}
			if (discard)
			{
				this.HandleBattleEvent<CardsEventArgs>(this.Battle.CardsAddedToDiscard, handler, (GameEventPriority)0);
			}
			if (hand)
			{
				this.HandleBattleEvent<CardUsingEventArgs>(this.Battle.CardUsed, handler, (GameEventPriority)0);
			}
			if (hand && draw)
			{
				this.HandleBattleEvent<CardEventArgs>(this.Battle.CardDrawn, handler, (GameEventPriority)0);
			}
			if (hand && draw && discard)
			{
				this.HandleBattleEvent<CardEventArgs>(this.Battle.CardDiscarded, handler, (GameEventPriority)0);
			}
			if (draw && discard)
			{
				this.HandleBattleEvent<GameEventArgs>(this.Battle.Reshuffled, handler, (GameEventPriority)0);
			}
		}
		public static int CompareCard(Card a, Card b)
		{
			int num = a.Config.Order.CompareTo(b.Config.Order);
			if (num != 0)
			{
				return num;
			}
			return string.Compare(a.Id, b.Id, 4);
		}
		public static int CompareMuseumCardColors(IReadOnlyList<ManaColor> colorsA, IReadOnlyList<ManaColor> colorsB)
		{
			int count = colorsA.Count;
			int count2 = colorsB.Count;
			if (count == 0 && count2 != 0)
			{
				return 1;
			}
			if (count2 == 0 && count != 0)
			{
				return -1;
			}
			if (count == 0 && count2 == 0)
			{
				return 0;
			}
			int num = count - count2;
			if (num > 0)
			{
				return 1;
			}
			if (num >= 0)
			{
				for (int i = 0; i < count; i++)
				{
					int num2 = colorsA[i].CompareTo(colorsB[i]);
					if (num2 != 0)
					{
						return num2;
					}
				}
				return 0;
			}
			return -1;
		}
		protected int SynergyAmount(ManaGroup cost, ManaColor synergyColor, int synergyColorAmount = 1)
		{
			if (synergyColorAmount < 1)
			{
				throw new InvalidOperationException("can not get synergy with mana amount blew zero.");
			}
			int num;
			if (synergyColor == ManaColor.Philosophy)
			{
				num = cost.Philosophy;
			}
			else if (synergyColor == ManaColor.Any)
			{
				num = cost.Amount;
			}
			else
			{
				num = cost.Philosophy + cost.GetValue(synergyColor);
			}
			return num / synergyColorAmount + base.GameRun.SynergyAdditionalCount;
		}
		protected int SynergyAmountComplexMana(ManaGroup cost, ManaGroup synergyMana)
		{
			int num = 0;
			ManaGroup manaGroup = ManaGroup.Empty;
			while (cost.CanAfford(manaGroup + synergyMana))
			{
				manaGroup += synergyMana;
				num++;
			}
			return num + base.GameRun.SynergyAdditionalCount;
		}
		public void Verify()
		{
			if (this.CardType == CardType.Defense && !this.HasBlock && !this.HasShield)
			{
				Debug.LogError("<" + base.Id + "> must set block or shield in config");
			}
			if (this.CardType != CardType.Attack && this.CardType != CardType.Tool && this.IsAccuracy)
			{
				Debug.LogError("<" + base.Id + "> should not be accuracy when it is not an attack card.");
			}
			if (this.Config.UpgradedCost != null)
			{
				ManaGroup cost = this.Config.Cost;
				ManaGroup value = this.Config.UpgradedCost.Value;
				if (cost.Amount < value.Amount)
				{
					throw new InvalidOperationException(this.DebugName + ": upgraded cost is illegal, Amount increase.");
				}
				foreach (ManaColor manaColor in ManaColors.WUBRGC)
				{
					if (cost.GetValue(manaColor) < value.GetValue(manaColor))
					{
						throw new InvalidOperationException(this.DebugName + ": upgraded cost is illegal, There is some single color's value increase.");
					}
				}
			}
		}
		public event Action Activating;
		public void NotifyActivating()
		{
			Action activating = this.Activating;
			if (activating == null)
			{
				return;
			}
			activating.Invoke();
		}
		public virtual bool ShouldPreventOtherCardUsage(Card card)
		{
			return false;
		}
		public virtual string PreventCardUsageMessage
		{
			get
			{
				throw new InvalidOperationException("Cannot get prevent card message key for " + this.DebugName);
			}
		}
		[return: TupleElementNames(new string[] { "card", "upgradedCard" })]
		public ValueTuple<Card, Card> GetDetailInfoCard()
		{
			Card card = Library.CreateCard(base.Id);
			card.GameRun = base.GameRun;
			if (card.CanUpgrade)
			{
				Card card2 = Library.CreateCard(base.Id, true, this.UpgradeCounter);
				card2.GameRun = base.GameRun;
				return new ValueTuple<Card, Card>(card, card2);
			}
			return new ValueTuple<Card, Card>(card, null);
		}
		private readonly GameEventHandlerHolder _handlerHolder = new GameEventHandlerHolder();
		private int _deltaDamage;
		private int _deltaBlock;
		private int _deltaShield;
		private int _deltaValue1;
		private int _deltaValue2;
		private int _deltaInt;
		private ManaGroup _kickerDelta;
		private ManaGroup _auraCost;
		private ManaGroup _baseCost;
		private ManaGroup _turnCostDelta;
		private bool _debutCardPlayOnce;
		private int _playCount;
		private int? _deckCounter;
		private int? _upgradeCounter;
		private ManaGroup? _pendingManaUsage;
		private Unit _pendingTarget;
		private int _loyalty;
		private readonly WeakReference<BattleController> _battle = new WeakReference<BattleController>(null);
		protected enum PerformTargetType
		{
			Player,
			ToTarget,
			TargetSelf,
			EachEnemy,
			EachEnemySelf,
			WorldSpace
		}
		internal sealed class CardFormatWrapper : GameEntityFormatWrapper
		{
			public CardFormatWrapper(Card card)
				: base(card)
			{
				this._card = card;
			}
			protected override string FormatArgument(object arg, string format)
			{
				if (arg is DamageInfo)
				{
					DamageInfo damageInfo = (DamageInfo)arg;
					int num = (int)damageInfo.Damage;
					BattleController battle = this._card.Battle;
					if (battle == null)
					{
						return GameEntityFormatWrapper.WrappedFormatNumber(num, num, format);
					}
					int num2 = battle.CalculateDamage(this._card, battle.Player, this._card.PendingTarget, damageInfo);
					return GameEntityFormatWrapper.WrappedFormatNumber(num, num2, format);
				}
				else if (arg is BlockInfo)
				{
					int block = ((BlockInfo)arg).Block;
					BattleController battle2 = this._card.Battle;
					if (battle2 == null)
					{
						return GameEntityFormatWrapper.WrappedFormatNumber(block, block, format);
					}
					int item = battle2.CalculateBlockShield(this._card, (float)block, 0f, BlockShieldType.Normal).Item1;
					return GameEntityFormatWrapper.WrappedFormatNumber(block, item, format);
				}
				else if (arg is ShieldInfo)
				{
					int shield = ((ShieldInfo)arg).Shield;
					BattleController battle3 = this._card.Battle;
					if (battle3 == null)
					{
						return GameEntityFormatWrapper.WrappedFormatNumber(shield, shield, format);
					}
					int item2 = battle3.CalculateBlockShield(this._card, 0f, (float)shield, BlockShieldType.Normal).Item2;
					return GameEntityFormatWrapper.WrappedFormatNumber(shield, item2, format);
				}
				else if (arg is ScryInfo)
				{
					int count = ((ScryInfo)arg).Count;
					BattleController battle4 = this._card.Battle;
					if (battle4 == null)
					{
						return GameEntityFormatWrapper.WrappedFormatNumber(count, count, format);
					}
					int num3 = battle4.CalculateScry(this._card, count);
					return GameEntityFormatWrapper.WrappedFormatNumber(count, num3, format);
				}
				else
				{
					if (arg is FriendCostInfo)
					{
						FriendCostInfo friendCostInfo = (FriendCostInfo)arg;
						return string.Concat(new string[]
						{
							"<indent=0><size=34><sprite=\"",
							friendCostInfo.CostType.ToString(),
							"\" name=\"",
							friendCostInfo.Cost.ToString(),
							"\"></size><indent=80>"
						});
					}
					return base.FormatArgument(arg, format);
				}
			}
			private readonly Card _card;
		}
	}
}
