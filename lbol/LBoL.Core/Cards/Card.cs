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
	// Token: 0x0200012A RID: 298
	[Localizable]
	public abstract class Card : GameEntity, IVerifiable, INotifyActivating, IXCostFilter
	{
		// Token: 0x1700034F RID: 847
		// (get) Token: 0x06000A88 RID: 2696 RVA: 0x0001D840 File Offset: 0x0001BA40
		public virtual bool OnDrawVisual
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000A89 RID: 2697 RVA: 0x0001D843 File Offset: 0x0001BA43
		public virtual IEnumerable<BattleAction> OnDraw()
		{
			return null;
		}

		// Token: 0x17000350 RID: 848
		// (get) Token: 0x06000A8A RID: 2698 RVA: 0x0001D846 File Offset: 0x0001BA46
		public virtual bool OnDiscardVisual
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000A8B RID: 2699 RVA: 0x0001D849 File Offset: 0x0001BA49
		public virtual IEnumerable<BattleAction> OnDiscard(CardZone srcZone)
		{
			return null;
		}

		// Token: 0x06000A8C RID: 2700 RVA: 0x0001D84C File Offset: 0x0001BA4C
		public virtual IEnumerable<BattleAction> OnTurnStartedInHand()
		{
			return null;
		}

		// Token: 0x06000A8D RID: 2701 RVA: 0x0001D84F File Offset: 0x0001BA4F
		public virtual IEnumerable<BattleAction> OnTurnEndingInHand()
		{
			return null;
		}

		// Token: 0x17000351 RID: 849
		// (get) Token: 0x06000A8E RID: 2702 RVA: 0x0001D852 File Offset: 0x0001BA52
		public virtual bool OnExileVisual
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000A8F RID: 2703 RVA: 0x0001D855 File Offset: 0x0001BA55
		public virtual IEnumerable<BattleAction> OnExile(CardZone srcZone)
		{
			return null;
		}

		// Token: 0x17000352 RID: 850
		// (get) Token: 0x06000A90 RID: 2704 RVA: 0x0001D858 File Offset: 0x0001BA58
		public virtual bool OnMoveVisual
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000A91 RID: 2705 RVA: 0x0001D85B File Offset: 0x0001BA5B
		public virtual IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			return null;
		}

		// Token: 0x17000353 RID: 851
		// (get) Token: 0x06000A92 RID: 2706 RVA: 0x0001D85E File Offset: 0x0001BA5E
		public virtual bool OnRetainVisual
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000A93 RID: 2707 RVA: 0x0001D861 File Offset: 0x0001BA61
		public virtual IEnumerable<BattleAction> OnRetain()
		{
			return null;
		}

		// Token: 0x06000A94 RID: 2708 RVA: 0x0001D864 File Offset: 0x0001BA64
		public virtual void OnRemove()
		{
		}

		// Token: 0x06000A95 RID: 2709 RVA: 0x0001D866 File Offset: 0x0001BA66
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

		// Token: 0x17000354 RID: 852
		// (get) Token: 0x06000A96 RID: 2710 RVA: 0x0001D88A File Offset: 0x0001BA8A
		// (set) Token: 0x06000A97 RID: 2711 RVA: 0x0001D892 File Offset: 0x0001BA92
		public virtual bool RemoveFromBattleAfterPlay { get; set; }

		// Token: 0x06000A98 RID: 2712 RVA: 0x0001D89B File Offset: 0x0001BA9B
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

		// Token: 0x17000355 RID: 853
		// (get) Token: 0x06000A99 RID: 2713 RVA: 0x0001D8AB File Offset: 0x0001BAAB
		// (set) Token: 0x06000A9A RID: 2714 RVA: 0x0001D8B3 File Offset: 0x0001BAB3
		public bool IsPlayTwiceToken { get; set; }

		// Token: 0x17000356 RID: 854
		// (get) Token: 0x06000A9B RID: 2715 RVA: 0x0001D8BC File Offset: 0x0001BABC
		// (set) Token: 0x06000A9C RID: 2716 RVA: 0x0001D8C4 File Offset: 0x0001BAC4
		public Card PlayTwiceSourceCard { get; set; }

		// Token: 0x06000A9D RID: 2717 RVA: 0x0001D8CD File Offset: 0x0001BACD
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

		// Token: 0x06000A9E RID: 2718 RVA: 0x0001D8E0 File Offset: 0x0001BAE0
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

		// Token: 0x06000A9F RID: 2719 RVA: 0x0001D954 File Offset: 0x0001BB54
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

		// Token: 0x06000AA0 RID: 2720 RVA: 0x0001DA1C File Offset: 0x0001BC1C
		protected void ProcessKeywordUpgrade()
		{
			Keyword keyword = this.Keywords & ~this.Config.Keywords;
			this.Keywords = this.Config.UpgradedKeywords | keyword;
		}

		// Token: 0x06000AA1 RID: 2721 RVA: 0x0001DA50 File Offset: 0x0001BC50
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

		// Token: 0x06000AA2 RID: 2722 RVA: 0x0001DDEC File Offset: 0x0001BFEC
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

		// Token: 0x06000AA3 RID: 2723 RVA: 0x0001DE50 File Offset: 0x0001C050
		public IEnumerable<Card> Clone(int count, bool setGameRun = false)
		{
			List<Card> list = new List<Card>();
			for (int i = 0; i < count; i++)
			{
				list.Add(this.Clone(setGameRun));
			}
			return list;
		}

		// Token: 0x06000AA4 RID: 2724 RVA: 0x0001DE80 File Offset: 0x0001C080
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

		// Token: 0x06000AA5 RID: 2725 RVA: 0x0001DF68 File Offset: 0x0001C168
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

		// Token: 0x06000AA6 RID: 2726 RVA: 0x0001E06C File Offset: 0x0001C26C
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

		// Token: 0x06000AA7 RID: 2727 RVA: 0x0001E0FC File Offset: 0x0001C2FC
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

		// Token: 0x06000AA8 RID: 2728 RVA: 0x0001E134 File Offset: 0x0001C334
		protected virtual void OnEnterBattle(BattleController battle)
		{
		}

		// Token: 0x06000AA9 RID: 2729 RVA: 0x0001E136 File Offset: 0x0001C336
		protected virtual void OnLeaveBattle()
		{
		}

		// Token: 0x06000AAA RID: 2730 RVA: 0x0001E138 File Offset: 0x0001C338
		protected void HandleBattleEvent<T>(GameEvent<T> @event, GameEventHandler<T> handler, GameEventPriority priority = (GameEventPriority)0) where T : GameEventArgs
		{
			this._handlerHolder.HandleEvent<T>(@event, handler, priority);
		}

		// Token: 0x06000AAB RID: 2731 RVA: 0x0001E148 File Offset: 0x0001C348
		protected void ReactBattleEvent<TEventArgs>(GameEvent<TEventArgs> @event, EventSequencedReactor<TEventArgs> reactor, GameEventPriority priority) where TEventArgs : GameEventArgs
		{
			this.HandleBattleEvent<TEventArgs>(@event, delegate(TEventArgs args)
			{
				this.React(reactor(args));
			}, priority);
		}

		// Token: 0x06000AAC RID: 2732 RVA: 0x0001E180 File Offset: 0x0001C380
		protected void ReactBattleEvent<TEventArgs>(GameEvent<TEventArgs> @event, EventSequencedReactor<TEventArgs> reactor) where TEventArgs : GameEventArgs
		{
			this.HandleBattleEvent<TEventArgs>(@event, delegate(TEventArgs args)
			{
				this.React(reactor(args));
			}, this.DefaultEventPriority);
		}

		// Token: 0x06000AAD RID: 2733 RVA: 0x0001E1BA File Offset: 0x0001C3BA
		protected override void React(Reactor reactor)
		{
			if (this.Battle == null)
			{
				Debug.LogError("[Card: " + this.DebugName + "] Cannot react outside battle");
				return;
			}
			this.Battle.React(reactor, this, ActionCause.Card);
		}

		// Token: 0x06000AAE RID: 2734 RVA: 0x0001E1F0 File Offset: 0x0001C3F0
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

		// Token: 0x17000357 RID: 855
		// (get) Token: 0x06000AAF RID: 2735 RVA: 0x0001E320 File Offset: 0x0001C520
		protected PerformAction SkillAnime
		{
			get
			{
				return PerformAction.Animation(this.Battle.Player, "skill", 0.2f, null, 0f, -1);
			}
		}

		// Token: 0x17000358 RID: 856
		// (get) Token: 0x06000AB0 RID: 2736 RVA: 0x0001E343 File Offset: 0x0001C543
		protected PerformAction SpellAnime
		{
			get
			{
				return PerformAction.Animation(this.Battle.Player, "spell", 0.2f, null, 0f, -1);
			}
		}

		// Token: 0x06000AB1 RID: 2737 RVA: 0x0001E366 File Offset: 0x0001C566
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

		// Token: 0x06000AB2 RID: 2738 RVA: 0x0001E3A3 File Offset: 0x0001C5A3
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

		// Token: 0x06000AB3 RID: 2739 RVA: 0x0001E3BA File Offset: 0x0001C5BA
		protected virtual IEnumerable<BattleAction> KickerActions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			return this.Actions(selector, consumingMana, precondition);
		}

		// Token: 0x06000AB4 RID: 2740 RVA: 0x0001E3C5 File Offset: 0x0001C5C5
		protected virtual IEnumerable<BattleAction> SummonActions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			this.Summoned = true;
			yield return PerformAction.SummonFriend(this);
			yield break;
		}

		// Token: 0x06000AB5 RID: 2741 RVA: 0x0001E3D8 File Offset: 0x0001C5D8
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

		// Token: 0x06000AB6 RID: 2742 RVA: 0x0001E433 File Offset: 0x0001C633
		protected BattleAction AttackAction(Unit target)
		{
			return this.AttackAction(target, this.Damage, this.GunName);
		}

		// Token: 0x06000AB7 RID: 2743 RVA: 0x0001E448 File Offset: 0x0001C648
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

		// Token: 0x06000AB8 RID: 2744 RVA: 0x0001E4CC File Offset: 0x0001C6CC
		protected BattleAction AttackAction(IEnumerable<Unit> targets, string gunName)
		{
			return this.AttackAction(targets, gunName, this.Damage);
		}

		// Token: 0x06000AB9 RID: 2745 RVA: 0x0001E4DC File Offset: 0x0001C6DC
		protected BattleAction AttackAction(IEnumerable<Unit> targets)
		{
			return this.AttackAction(targets, this.GunName, this.Damage);
		}

		// Token: 0x06000ABA RID: 2746 RVA: 0x0001E4F4 File Offset: 0x0001C6F4
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

		// Token: 0x06000ABB RID: 2747 RVA: 0x0001E655 File Offset: 0x0001C855
		protected BattleAction AttackAction(UnitSelector selector, GunPair gunPair = null)
		{
			return this.AttackAction(selector, this.Damage, gunPair);
		}

		// Token: 0x06000ABC RID: 2748 RVA: 0x0001E665 File Offset: 0x0001C865
		protected BattleAction AttackAction(UnitSelector selector, string gunName)
		{
			return this.AttackAction(selector, this.Damage, new GunPair(gunName, GunType.Single));
		}

		// Token: 0x06000ABD RID: 2749 RVA: 0x0001E67C File Offset: 0x0001C87C
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

		// Token: 0x06000ABE RID: 2750 RVA: 0x0001E6E8 File Offset: 0x0001C8E8
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

		// Token: 0x17000359 RID: 857
		// (get) Token: 0x06000ABF RID: 2751 RVA: 0x0001E770 File Offset: 0x0001C970
		private bool UseBurstGun
		{
			get
			{
				return (double)(this.Damage.Amount + (float)this.Battle.Player.TotalFirepower) > (double)this.ConfigDamage * 1.5 || this.IsUpgraded || this.Battle.Player.HasStatusEffect<Burst>();
			}
		}

		// Token: 0x1700035A RID: 858
		// (get) Token: 0x06000AC0 RID: 2752 RVA: 0x0001E7CB File Offset: 0x0001C9CB
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

		// Token: 0x1700035B RID: 859
		// (get) Token: 0x06000AC1 RID: 2753 RVA: 0x0001E80A File Offset: 0x0001CA0A
		// (set) Token: 0x06000AC2 RID: 2754 RVA: 0x0001E812 File Offset: 0x0001CA12
		protected Guns CardGuns { get; set; }

		// Token: 0x06000AC3 RID: 2755 RVA: 0x0001E81B File Offset: 0x0001CA1B
		protected virtual void SetGuns()
		{
		}

		// Token: 0x1700035C RID: 860
		// (get) Token: 0x06000AC4 RID: 2756 RVA: 0x0001E820 File Offset: 0x0001CA20
		private bool HasBlock
		{
			get
			{
				return this.Config.Block != null || this.Config.UpgradedBlock != null;
			}
		}

		// Token: 0x1700035D RID: 861
		// (get) Token: 0x06000AC5 RID: 2757 RVA: 0x0001E858 File Offset: 0x0001CA58
		private bool HasShield
		{
			get
			{
				return this.Config.Shield != null || this.Config.UpgradedShield != null;
			}
		}

		// Token: 0x06000AC6 RID: 2758 RVA: 0x0001E890 File Offset: 0x0001CA90
		protected BattleAction DefenseAction(bool cast = true)
		{
			return new CastBlockShieldAction(this.Battle.Player, this.HasBlock ? this.Block.Block : 0, this.HasShield ? this.Shield.Shield : 0, BlockShieldType.Normal, cast);
		}

		// Token: 0x06000AC7 RID: 2759 RVA: 0x0001E8E1 File Offset: 0x0001CAE1
		protected BattleAction DefenseAction(int block, int shield, BlockShieldType type = BlockShieldType.Normal, bool cast = true)
		{
			return new CastBlockShieldAction(this.Battle.Player, block, shield, type, cast);
		}

		// Token: 0x06000AC8 RID: 2760 RVA: 0x0001E8F8 File Offset: 0x0001CAF8
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

		// Token: 0x06000AC9 RID: 2761 RVA: 0x0001E947 File Offset: 0x0001CB47
		protected BattleAction BuffAction<TEffect>(int level = 0, int duration = 0, int limit = 0, int count = 0, float occupationTime = 0.2f)
		{
			return this.BuffAction(typeof(TEffect), level, duration, limit, count, occupationTime);
		}

		// Token: 0x06000ACA RID: 2762 RVA: 0x0001E960 File Offset: 0x0001CB60
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

		// Token: 0x06000ACB RID: 2763 RVA: 0x0001E9AC File Offset: 0x0001CBAC
		protected BattleAction DebuffAction<TEffect>(Unit target, int level = 0, int duration = 0, int limit = 0, int count = 0, bool startAutoDecreasing = true, float occupationTime = 0.2f)
		{
			return this.DebuffAction(typeof(TEffect), target, level, duration, limit, count, startAutoDecreasing, occupationTime);
		}

		// Token: 0x06000ACC RID: 2764 RVA: 0x0001E9D4 File Offset: 0x0001CBD4
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

		// Token: 0x06000ACD RID: 2765 RVA: 0x0001EA2C File Offset: 0x0001CC2C
		protected IEnumerable<BattleAction> DebuffAction<TEffect>(IEnumerable<Unit> targets, int level = 0, int duration = 0, int limit = 0, int count = 0, bool startAutoDecreasing = true, float occupationTime = 0.2f)
		{
			return this.DebuffAction(typeof(TEffect), targets, level, duration, limit, count, startAutoDecreasing, occupationTime);
		}

		// Token: 0x06000ACE RID: 2766 RVA: 0x0001EA54 File Offset: 0x0001CC54
		protected BattleAction SacrificeAction(int life)
		{
			return new DamageAction(this.Battle.Player, this.Battle.Player, DamageInfo.HpLose((float)life, false), "Sacrifice", GunType.Single);
		}

		// Token: 0x06000ACF RID: 2767 RVA: 0x0001EA7F File Offset: 0x0001CC7F
		protected BattleAction LoseLifeAction(int life)
		{
			return new DamageAction(this.Battle.Player, this.Battle.Player, DamageInfo.HpLose((float)life, false), "Instant", GunType.Single);
		}

		// Token: 0x06000AD0 RID: 2768 RVA: 0x0001EAAA File Offset: 0x0001CCAA
		protected BattleAction DamageSelfAction(int damage, string gun = "")
		{
			return new DamageAction(this.Battle.Player, this.Battle.Player, DamageInfo.Reaction((float)damage, false), gun, GunType.Single);
		}

		// Token: 0x06000AD1 RID: 2769 RVA: 0x0001EAD1 File Offset: 0x0001CCD1
		protected BattleAction HealAction(int heal)
		{
			return new HealAction(this.Battle.Player, this.Battle.Player, heal, HealType.Normal, 0.2f);
		}

		// Token: 0x06000AD2 RID: 2770 RVA: 0x0001EAF8 File Offset: 0x0001CCF8
		protected BattleAction UpgradeAllHandsAction()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(this.Battle.HandZone, (Card card) => card.CanUpgradeAndPositive));
			if (list.Count <= 0)
			{
				return null;
			}
			return new UpgradeCardsAction(list);
		}

		// Token: 0x06000AD3 RID: 2771 RVA: 0x0001EB4C File Offset: 0x0001CD4C
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

		// Token: 0x06000AD4 RID: 2772 RVA: 0x0001EC35 File Offset: 0x0001CE35
		protected BattleAction DiscardRandomHandAction(int amount = 1)
		{
			if (amount <= 0)
			{
				return null;
			}
			return new DiscardManyAction(Enumerable.ToList<Card>(this.Battle.HandZone).SampleManyOrAll(amount, base.GameRun.BattleRng));
		}

		// Token: 0x06000AD5 RID: 2773 RVA: 0x0001EC64 File Offset: 0x0001CE64
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

		// Token: 0x06000AD6 RID: 2774 RVA: 0x0001ECC8 File Offset: 0x0001CEC8
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

		// Token: 0x06000AD7 RID: 2775 RVA: 0x0001EDA8 File Offset: 0x0001CFA8
		public int GetSeLevel(Type seType)
		{
			if (this.Battle == null || !this.Battle.Player.HasStatusEffect(seType))
			{
				return 0;
			}
			return this.Battle.Player.GetStatusEffect(seType).Level;
		}

		// Token: 0x06000AD8 RID: 2776 RVA: 0x0001EDDD File Offset: 0x0001CFDD
		public int GetSeLevel<T>() where T : StatusEffect
		{
			return this.GetSeLevel(typeof(T));
		}

		// Token: 0x1700035E RID: 862
		// (get) Token: 0x06000AD9 RID: 2777 RVA: 0x0001EDEF File Offset: 0x0001CFEF
		protected RandomGen BattleRng
		{
			get
			{
				return base.GameRun.BattleRng;
			}
		}

		// Token: 0x06000ADA RID: 2778 RVA: 0x0001EDFC File Offset: 0x0001CFFC
		private static int CompareCost(ManaGroup a, ManaGroup b)
		{
			int num = a.Amount.CompareTo(b.Amount);
			if (num == 0)
			{
				return b.Any.CompareTo(a.Any);
			}
			return num;
		}

		// Token: 0x1700035F RID: 863
		// (get) Token: 0x06000ADB RID: 2779 RVA: 0x0001EE3B File Offset: 0x0001D03B
		// (set) Token: 0x06000ADC RID: 2780 RVA: 0x0001EE43 File Offset: 0x0001D043
		public int InstanceId { get; internal set; }

		// Token: 0x17000360 RID: 864
		// (get) Token: 0x06000ADD RID: 2781 RVA: 0x0001EE4C File Offset: 0x0001D04C
		// (set) Token: 0x06000ADE RID: 2782 RVA: 0x0001EE54 File Offset: 0x0001D054
		public CardConfig Config { get; private set; }

		// Token: 0x17000361 RID: 865
		// (get) Token: 0x06000ADF RID: 2783 RVA: 0x0001EE5D File Offset: 0x0001D05D
		[UsedImplicitly]
		public virtual DamageInfo Damage
		{
			get
			{
				return DamageInfo.Attack((float)this.RawDamage, this.IsAccuracy);
			}
		}

		// Token: 0x17000362 RID: 866
		// (get) Token: 0x06000AE0 RID: 2784 RVA: 0x0001EE71 File Offset: 0x0001D071
		[UsedImplicitly]
		public int RawDamage
		{
			get
			{
				return this.BasicCardModify(this.ConfigDamage + this.AdditionalDamage + this.DeltaDamage, true);
			}
		}

		// Token: 0x17000363 RID: 867
		// (get) Token: 0x06000AE1 RID: 2785 RVA: 0x0001EE90 File Offset: 0x0001D090
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

		// Token: 0x17000364 RID: 868
		// (get) Token: 0x06000AE2 RID: 2786 RVA: 0x0001EF03 File Offset: 0x0001D103
		protected virtual int AdditionalDamage
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x17000365 RID: 869
		// (get) Token: 0x06000AE3 RID: 2787 RVA: 0x0001EF06 File Offset: 0x0001D106
		// (set) Token: 0x06000AE4 RID: 2788 RVA: 0x0001EF0E File Offset: 0x0001D10E
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

		// Token: 0x06000AE5 RID: 2789 RVA: 0x0001EF28 File Offset: 0x0001D128
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

		// Token: 0x17000366 RID: 870
		// (get) Token: 0x06000AE6 RID: 2790 RVA: 0x0001EFE6 File Offset: 0x0001D1E6
		[UsedImplicitly]
		public virtual BlockInfo Block
		{
			get
			{
				return new BlockInfo(this.RawBlock, BlockShieldType.Normal);
			}
		}

		// Token: 0x17000367 RID: 871
		// (get) Token: 0x06000AE7 RID: 2791 RVA: 0x0001EFF4 File Offset: 0x0001D1F4
		[UsedImplicitly]
		public virtual int RawBlock
		{
			get
			{
				return this.BasicCardModify(this.ConfigBlock + this.AdditionalBlock + this.DeltaBlock, false);
			}
		}

		// Token: 0x17000368 RID: 872
		// (get) Token: 0x06000AE8 RID: 2792 RVA: 0x0001F014 File Offset: 0x0001D214
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

		// Token: 0x17000369 RID: 873
		// (get) Token: 0x06000AE9 RID: 2793 RVA: 0x0001F087 File Offset: 0x0001D287
		protected virtual int AdditionalBlock
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x1700036A RID: 874
		// (get) Token: 0x06000AEA RID: 2794 RVA: 0x0001F08A File Offset: 0x0001D28A
		// (set) Token: 0x06000AEB RID: 2795 RVA: 0x0001F092 File Offset: 0x0001D292
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

		// Token: 0x1700036B RID: 875
		// (get) Token: 0x06000AEC RID: 2796 RVA: 0x0001F0AA File Offset: 0x0001D2AA
		[UsedImplicitly]
		public ShieldInfo Shield
		{
			get
			{
				return new ShieldInfo(this.RawShield, BlockShieldType.Normal);
			}
		}

		// Token: 0x1700036C RID: 876
		// (get) Token: 0x06000AED RID: 2797 RVA: 0x0001F0B8 File Offset: 0x0001D2B8
		[UsedImplicitly]
		public int RawShield
		{
			get
			{
				return this.BasicCardModify(this.ConfigShield + this.AdditionalShield + this.DeltaShield, false);
			}
		}

		// Token: 0x1700036D RID: 877
		// (get) Token: 0x06000AEE RID: 2798 RVA: 0x0001F0D8 File Offset: 0x0001D2D8
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

		// Token: 0x1700036E RID: 878
		// (get) Token: 0x06000AEF RID: 2799 RVA: 0x0001F14B File Offset: 0x0001D34B
		protected virtual int AdditionalShield
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x1700036F RID: 879
		// (get) Token: 0x06000AF0 RID: 2800 RVA: 0x0001F14E File Offset: 0x0001D34E
		// (set) Token: 0x06000AF1 RID: 2801 RVA: 0x0001F156 File Offset: 0x0001D356
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

		// Token: 0x17000370 RID: 880
		// (get) Token: 0x06000AF2 RID: 2802 RVA: 0x0001F170 File Offset: 0x0001D370
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

		// Token: 0x17000371 RID: 881
		// (get) Token: 0x06000AF3 RID: 2803 RVA: 0x0001F1EA File Offset: 0x0001D3EA
		public int Value1
		{
			get
			{
				return this.BasicCardModify(this.ConfigValue1 + this.AdditionalValue1 + this.DeltaValue1, false);
			}
		}

		// Token: 0x17000372 RID: 882
		// (get) Token: 0x06000AF4 RID: 2804 RVA: 0x0001F208 File Offset: 0x0001D408
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

		// Token: 0x17000373 RID: 883
		// (get) Token: 0x06000AF5 RID: 2805 RVA: 0x0001F279 File Offset: 0x0001D479
		protected virtual int AdditionalValue1
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x17000374 RID: 884
		// (get) Token: 0x06000AF6 RID: 2806 RVA: 0x0001F27C File Offset: 0x0001D47C
		// (set) Token: 0x06000AF7 RID: 2807 RVA: 0x0001F284 File Offset: 0x0001D484
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

		// Token: 0x17000375 RID: 885
		// (get) Token: 0x06000AF8 RID: 2808 RVA: 0x0001F29C File Offset: 0x0001D49C
		public int Value2
		{
			get
			{
				return this.BasicCardModify(this.ConfigValue2 + this.AdditionalValue2 + this.DeltaValue2, false);
			}
		}

		// Token: 0x17000376 RID: 886
		// (get) Token: 0x06000AF9 RID: 2809 RVA: 0x0001F2BC File Offset: 0x0001D4BC
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

		// Token: 0x17000377 RID: 887
		// (get) Token: 0x06000AFA RID: 2810 RVA: 0x0001F32D File Offset: 0x0001D52D
		protected virtual int AdditionalValue2
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x17000378 RID: 888
		// (get) Token: 0x06000AFB RID: 2811 RVA: 0x0001F330 File Offset: 0x0001D530
		// (set) Token: 0x06000AFC RID: 2812 RVA: 0x0001F338 File Offset: 0x0001D538
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

		// Token: 0x17000379 RID: 889
		// (get) Token: 0x06000AFD RID: 2813 RVA: 0x0001F350 File Offset: 0x0001D550
		// (set) Token: 0x06000AFE RID: 2814 RVA: 0x0001F358 File Offset: 0x0001D558
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

		// Token: 0x1700037A RID: 890
		// (get) Token: 0x06000AFF RID: 2815 RVA: 0x0001F370 File Offset: 0x0001D570
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

		// Token: 0x1700037B RID: 891
		// (get) Token: 0x06000B00 RID: 2816 RVA: 0x0001F3E4 File Offset: 0x0001D5E4
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

		// Token: 0x06000B01 RID: 2817 RVA: 0x0001F428 File Offset: 0x0001D628
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

		// Token: 0x1700037C RID: 892
		// (get) Token: 0x06000B02 RID: 2818 RVA: 0x0001F714 File Offset: 0x0001D914
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return (GameEventPriority)this.Config.Order;
			}
		}

		// Token: 0x06000B03 RID: 2819 RVA: 0x0001F721 File Offset: 0x0001D921
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<Card>.LocalizeProperty(base.Id, key, decorated, required);
		}

		// Token: 0x06000B04 RID: 2820 RVA: 0x0001F731 File Offset: 0x0001D931
		protected virtual IReadOnlyList<string> LocalizeListProperty(string key, bool required = true)
		{
			return TypeFactory<Card>.LocalizeListProperty(base.Id, key, required);
		}

		// Token: 0x1700037D RID: 893
		// (get) Token: 0x06000B05 RID: 2821 RVA: 0x0001F740 File Offset: 0x0001D940
		private string UpgradedBaseDescription
		{
			get
			{
				return this.LocalizeProperty("UpgradedDescription", true, false);
			}
		}

		// Token: 0x1700037E RID: 894
		// (get) Token: 0x06000B06 RID: 2822 RVA: 0x0001F74F File Offset: 0x0001D94F
		private string NonbattleBaseDescription
		{
			get
			{
				return this.LocalizeProperty("NonbattleDescription", true, false);
			}
		}

		// Token: 0x1700037F RID: 895
		// (get) Token: 0x06000B07 RID: 2823 RVA: 0x0001F75E File Offset: 0x0001D95E
		private string UpgradedNonbattleBaseDescription
		{
			get
			{
				return this.LocalizeProperty("UpgradedNonbattleDescription", true, false);
			}
		}

		// Token: 0x17000380 RID: 896
		// (get) Token: 0x06000B08 RID: 2824 RVA: 0x0001F76D File Offset: 0x0001D96D
		private string ExtraDescription1
		{
			get
			{
				return this.LocalizeProperty("ExtraDescription1", true, false);
			}
		}

		// Token: 0x17000381 RID: 897
		// (get) Token: 0x06000B09 RID: 2825 RVA: 0x0001F77C File Offset: 0x0001D97C
		private string UpgradedExtraDescription1
		{
			get
			{
				return this.LocalizeProperty("UpgradedExtraDescription1", true, false);
			}
		}

		// Token: 0x17000382 RID: 898
		// (get) Token: 0x06000B0A RID: 2826 RVA: 0x0001F78B File Offset: 0x0001D98B
		private string ExtraDescription2
		{
			get
			{
				return this.LocalizeProperty("ExtraDescription2", true, false);
			}
		}

		// Token: 0x17000383 RID: 899
		// (get) Token: 0x06000B0B RID: 2827 RVA: 0x0001F79A File Offset: 0x0001D99A
		private string UpgradedExtraDescription2
		{
			get
			{
				return this.LocalizeProperty("UpgradedExtraDescription2", true, false);
			}
		}

		// Token: 0x17000384 RID: 900
		// (get) Token: 0x06000B0C RID: 2828 RVA: 0x0001F7A9 File Offset: 0x0001D9A9
		private string ExtraDescription3
		{
			get
			{
				return this.LocalizeProperty("ExtraDescription3", true, false);
			}
		}

		// Token: 0x17000385 RID: 901
		// (get) Token: 0x06000B0D RID: 2829 RVA: 0x0001F7B8 File Offset: 0x0001D9B8
		private string UpgradedExtraDescription3
		{
			get
			{
				return this.LocalizeProperty("UpgradedExtraDescription3", true, false);
			}
		}

		// Token: 0x17000386 RID: 902
		// (get) Token: 0x06000B0E RID: 2830 RVA: 0x0001F7C7 File Offset: 0x0001D9C7
		protected bool HasExtraDescription1
		{
			get
			{
				return !this.ExtraDescription1.IsNullOrEmpty();
			}
		}

		// Token: 0x17000387 RID: 903
		// (get) Token: 0x06000B0F RID: 2831 RVA: 0x0001F7D7 File Offset: 0x0001D9D7
		protected string GetExtraDescription1
		{
			get
			{
				return this.FollowByDetailIcon(this.RawExtraDescription1);
			}
		}

		// Token: 0x17000388 RID: 904
		// (get) Token: 0x06000B10 RID: 2832 RVA: 0x0001F7E5 File Offset: 0x0001D9E5
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

		// Token: 0x17000389 RID: 905
		// (get) Token: 0x06000B11 RID: 2833 RVA: 0x0001F806 File Offset: 0x0001DA06
		protected bool HasExtraDescription2
		{
			get
			{
				return !this.ExtraDescription2.IsNullOrEmpty();
			}
		}

		// Token: 0x1700038A RID: 906
		// (get) Token: 0x06000B12 RID: 2834 RVA: 0x0001F816 File Offset: 0x0001DA16
		protected string GetExtraDescription2
		{
			get
			{
				return this.FollowByDetailIcon(this.RawExtraDescription2);
			}
		}

		// Token: 0x1700038B RID: 907
		// (get) Token: 0x06000B13 RID: 2835 RVA: 0x0001F824 File Offset: 0x0001DA24
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

		// Token: 0x1700038C RID: 908
		// (get) Token: 0x06000B14 RID: 2836 RVA: 0x0001F845 File Offset: 0x0001DA45
		protected bool HasExtraDescription3
		{
			get
			{
				return !this.ExtraDescription3.IsNullOrEmpty();
			}
		}

		// Token: 0x1700038D RID: 909
		// (get) Token: 0x06000B15 RID: 2837 RVA: 0x0001F855 File Offset: 0x0001DA55
		protected string GetExtraDescription3
		{
			get
			{
				return this.FollowByDetailIcon(this.RawExtraDescription3);
			}
		}

		// Token: 0x1700038E RID: 910
		// (get) Token: 0x06000B16 RID: 2838 RVA: 0x0001F863 File Offset: 0x0001DA63
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

		// Token: 0x1700038F RID: 911
		// (get) Token: 0x06000B17 RID: 2839 RVA: 0x0001F884 File Offset: 0x0001DA84
		// (set) Token: 0x06000B18 RID: 2840 RVA: 0x0001F88C File Offset: 0x0001DA8C
		public int ChoiceCardIndicator { get; set; }

		// Token: 0x06000B19 RID: 2841 RVA: 0x0001F898 File Offset: 0x0001DA98
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

		// Token: 0x17000390 RID: 912
		// (get) Token: 0x06000B1A RID: 2842 RVA: 0x0001FA5B File Offset: 0x0001DC5B
		private string BaseFlavorText
		{
			get
			{
				return this.LocalizeProperty("FlavorText", false, false);
			}
		}

		// Token: 0x17000391 RID: 913
		// (get) Token: 0x06000B1B RID: 2843 RVA: 0x0001FA6A File Offset: 0x0001DC6A
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

		// Token: 0x17000392 RID: 914
		// (get) Token: 0x06000B1C RID: 2844 RVA: 0x0001FA7E File Offset: 0x0001DC7E
		private string BaseDetailText
		{
			get
			{
				return this.LocalizeProperty("DetailText", true, false);
			}
		}

		// Token: 0x06000B1D RID: 2845 RVA: 0x0001FA8D File Offset: 0x0001DC8D
		private string FollowByDetailIcon(string des)
		{
			if (this.BaseDetailText != null)
			{
				des += "<sprite=\"TextIcon\" name=\"Info\">";
			}
			return des;
		}

		// Token: 0x17000393 RID: 915
		// (get) Token: 0x06000B1E RID: 2846 RVA: 0x0001FAA5 File Offset: 0x0001DCA5
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

		// Token: 0x17000394 RID: 916
		// (get) Token: 0x06000B1F RID: 2847 RVA: 0x0001FAC7 File Offset: 0x0001DCC7
		public CardType CardType
		{
			get
			{
				return this.Config.Type;
			}
		}

		// Token: 0x17000395 RID: 917
		// (get) Token: 0x06000B20 RID: 2848 RVA: 0x0001FAD4 File Offset: 0x0001DCD4
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

		// Token: 0x17000396 RID: 918
		// (get) Token: 0x06000B21 RID: 2849 RVA: 0x0001FB18 File Offset: 0x0001DD18
		public bool IsXCost
		{
			get
			{
				return this.Config.IsXCost;
			}
		}

		// Token: 0x06000B22 RID: 2850 RVA: 0x0001FB25 File Offset: 0x0001DD25
		public virtual ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			if (!this.IsXCost)
			{
				throw new InvalidOperationException("Cannot get X cost from non-x-cost card");
			}
			return pooledMana;
		}

		// Token: 0x17000397 RID: 919
		// (get) Token: 0x06000B23 RID: 2851 RVA: 0x0001FB3B File Offset: 0x0001DD3B
		public ManaGroup XCostRequiredMana
		{
			get
			{
				return this.ConfigCost;
			}
		}

		// Token: 0x17000398 RID: 920
		// (get) Token: 0x06000B24 RID: 2852 RVA: 0x0001FB44 File Offset: 0x0001DD44
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

		// Token: 0x17000399 RID: 921
		// (get) Token: 0x06000B25 RID: 2853 RVA: 0x0001FB80 File Offset: 0x0001DD80
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

		// Token: 0x1700039A RID: 922
		// (get) Token: 0x06000B26 RID: 2854 RVA: 0x0001FBD9 File Offset: 0x0001DDD9
		// (set) Token: 0x06000B27 RID: 2855 RVA: 0x0001FBE1 File Offset: 0x0001DDE1
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

		// Token: 0x1700039B RID: 923
		// (get) Token: 0x06000B28 RID: 2856 RVA: 0x0001FBF0 File Offset: 0x0001DDF0
		public ManaGroup KickerCost
		{
			get
			{
				return (this.ConfigKicker + this.KickerDelta).Corrected;
			}
		}

		// Token: 0x1700039C RID: 924
		// (get) Token: 0x06000B29 RID: 2857 RVA: 0x0001FC16 File Offset: 0x0001DE16
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

		// Token: 0x1700039D RID: 925
		// (get) Token: 0x06000B2A RID: 2858 RVA: 0x0001FC44 File Offset: 0x0001DE44
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

		// Token: 0x06000B2B RID: 2859 RVA: 0x0001FC8D File Offset: 0x0001DE8D
		public ManaGroup CostToMana(bool baseCost)
		{
			if (!baseCost && !this.IsXCost)
			{
				return this.Cost.CostToMana();
			}
			return this.ConfigCost.CostToMana();
		}

		// Token: 0x1700039E RID: 926
		// (get) Token: 0x06000B2C RID: 2860 RVA: 0x0001FCB1 File Offset: 0x0001DEB1
		// (set) Token: 0x06000B2D RID: 2861 RVA: 0x0001FCB9 File Offset: 0x0001DEB9
		public bool FreeCost { get; set; }

		// Token: 0x1700039F RID: 927
		// (get) Token: 0x06000B2E RID: 2862 RVA: 0x0001FCC2 File Offset: 0x0001DEC2
		public virtual bool IsForceCost
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003A0 RID: 928
		// (get) Token: 0x06000B2F RID: 2863 RVA: 0x0001FCC5 File Offset: 0x0001DEC5
		public virtual ManaGroup ForceCost
		{
			get
			{
				return ManaGroup.Empty;
			}
		}

		// Token: 0x170003A1 RID: 929
		// (get) Token: 0x06000B30 RID: 2864 RVA: 0x0001FCCC File Offset: 0x0001DECC
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

		// Token: 0x170003A2 RID: 930
		// (get) Token: 0x06000B31 RID: 2865 RVA: 0x0001FD78 File Offset: 0x0001DF78
		protected virtual ManaGroup AdditionalCost
		{
			get
			{
				return ManaGroup.Empty;
			}
		}

		// Token: 0x170003A3 RID: 931
		// (get) Token: 0x06000B32 RID: 2866 RVA: 0x0001FD7F File Offset: 0x0001DF7F
		// (set) Token: 0x06000B33 RID: 2867 RVA: 0x0001FD87 File Offset: 0x0001DF87
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

		// Token: 0x170003A4 RID: 932
		// (get) Token: 0x06000B34 RID: 2868 RVA: 0x0001FDB9 File Offset: 0x0001DFB9
		// (set) Token: 0x06000B35 RID: 2869 RVA: 0x0001FDC4 File Offset: 0x0001DFC4
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

		// Token: 0x170003A5 RID: 933
		// (get) Token: 0x06000B36 RID: 2870 RVA: 0x0001FE18 File Offset: 0x0001E018
		public ManaGroup TurnCost
		{
			get
			{
				return this.BaseCost + this.TurnCostDelta;
			}
		}

		// Token: 0x170003A6 RID: 934
		// (get) Token: 0x06000B37 RID: 2871 RVA: 0x0001FE2B File Offset: 0x0001E02B
		// (set) Token: 0x06000B38 RID: 2872 RVA: 0x0001FE34 File Offset: 0x0001E034
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

		// Token: 0x06000B39 RID: 2873 RVA: 0x0001FE87 File Offset: 0x0001E087
		public void SetTurnCost(ManaGroup cost)
		{
			if (this.IsXCost)
			{
				Debug.LogWarning("Cannot set turn cost of " + this.DebugName + " because it is X-cost card");
				return;
			}
			this.TurnCostDelta = cost - this.BaseCost;
		}

		// Token: 0x06000B3A RID: 2874 RVA: 0x0001FEBE File Offset: 0x0001E0BE
		public void IncreaseTurnCost(ManaGroup cost)
		{
			if (this.IsXCost)
			{
				Debug.LogWarning("Cannot set turn cost of " + this.DebugName + " because it is X-cost card");
				return;
			}
			this.TurnCostDelta += cost;
		}

		// Token: 0x06000B3B RID: 2875 RVA: 0x0001FEF5 File Offset: 0x0001E0F5
		public void DecreaseTurnCost(ManaGroup cost)
		{
			if (this.IsXCost)
			{
				Debug.LogWarning("Cannot set turn cost of " + this.DebugName + " because it is X-cost card");
				return;
			}
			this.TurnCostDelta -= cost;
		}

		// Token: 0x06000B3C RID: 2876 RVA: 0x0001FF2C File Offset: 0x0001E12C
		public void SetBaseCost(ManaGroup cost)
		{
			if (this.IsXCost)
			{
				Debug.LogWarning("Cannot set base cost of " + this.DebugName + " because it is X-cost card");
				return;
			}
			this.BaseCost = cost;
		}

		// Token: 0x06000B3D RID: 2877 RVA: 0x0001FF58 File Offset: 0x0001E158
		public void IncreaseBaseCost(ManaGroup cost)
		{
			if (this.IsXCost)
			{
				Debug.LogWarning("Cannot set base cost of " + this.DebugName + " because it is X-cost card");
				return;
			}
			this.BaseCost += cost;
		}

		// Token: 0x06000B3E RID: 2878 RVA: 0x0001FF8F File Offset: 0x0001E18F
		public void DecreaseBaseCost(ManaGroup cost)
		{
			if (this.IsXCost)
			{
				Debug.LogWarning("Cannot set base cost of " + this.DebugName + " because it is X-cost card");
				return;
			}
			this.BaseCost -= cost;
		}

		// Token: 0x170003A7 RID: 935
		// (get) Token: 0x06000B3F RID: 2879 RVA: 0x0001FFC6 File Offset: 0x0001E1C6
		// (set) Token: 0x06000B40 RID: 2880 RVA: 0x0001FFCE File Offset: 0x0001E1CE
		public CardZone Zone { get; internal set; }

		// Token: 0x170003A8 RID: 936
		// (get) Token: 0x06000B41 RID: 2881 RVA: 0x0001FFD7 File Offset: 0x0001E1D7
		// (set) Token: 0x06000B42 RID: 2882 RVA: 0x0001FFDF File Offset: 0x0001E1DF
		public int HandIndexWhenPlaying { get; internal set; }

		// Token: 0x170003A9 RID: 937
		// (get) Token: 0x06000B43 RID: 2883 RVA: 0x0001FFE8 File Offset: 0x0001E1E8
		public virtual bool Triggered
		{
			get
			{
				return (this.IsDebut && this.DebutActive) || (this.IsInstinct && this.InstinctActive) || (this.IsOverdrive && this.Overdrive(this.Value2)) || (this.CardType == CardType.Friend && this.Config.UltimateCost != null && this.Loyalty >= -this.UltimateCost);
			}
		}

		// Token: 0x170003AA RID: 938
		// (get) Token: 0x06000B44 RID: 2884 RVA: 0x00020061 File Offset: 0x0001E261
		// (set) Token: 0x06000B45 RID: 2885 RVA: 0x00020069 File Offset: 0x0001E269
		public bool PlayInTriggered { get; set; }

		// Token: 0x170003AB RID: 939
		// (get) Token: 0x06000B46 RID: 2886 RVA: 0x00020072 File Offset: 0x0001E272
		public bool TriggeredAnyhow
		{
			get
			{
				return this.Triggered || this.PlayInTriggered;
			}
		}

		// Token: 0x170003AC RID: 940
		// (get) Token: 0x06000B47 RID: 2887 RVA: 0x00020084 File Offset: 0x0001E284
		// (set) Token: 0x06000B48 RID: 2888 RVA: 0x0002008C File Offset: 0x0001E28C
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

		// Token: 0x170003AD RID: 941
		// (get) Token: 0x06000B49 RID: 2889 RVA: 0x000200A4 File Offset: 0x0001E2A4
		// (set) Token: 0x06000B4A RID: 2890 RVA: 0x000200AC File Offset: 0x0001E2AC
		public bool KickerPlaying { get; set; }

		// Token: 0x170003AE RID: 942
		// (get) Token: 0x06000B4B RID: 2891 RVA: 0x000200B5 File Offset: 0x0001E2B5
		public bool DebutActive
		{
			get
			{
				return !this.DebutCardPlayedOnce;
			}
		}

		// Token: 0x170003AF RID: 943
		// (get) Token: 0x06000B4C RID: 2892 RVA: 0x000200C0 File Offset: 0x0001E2C0
		// (set) Token: 0x06000B4D RID: 2893 RVA: 0x000200C8 File Offset: 0x0001E2C8
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

		// Token: 0x170003B0 RID: 944
		// (get) Token: 0x06000B4E RID: 2894 RVA: 0x000200E0 File Offset: 0x0001E2E0
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

		// Token: 0x170003B1 RID: 945
		// (get) Token: 0x06000B4F RID: 2895 RVA: 0x00020104 File Offset: 0x0001E304
		public virtual ManaGroup? PlentifulMana
		{
			get
			{
				return default(ManaGroup?);
			}
		}

		// Token: 0x170003B2 RID: 946
		// (get) Token: 0x06000B50 RID: 2896 RVA: 0x0002011A File Offset: 0x0001E31A
		// (set) Token: 0x06000B51 RID: 2897 RVA: 0x00020122 File Offset: 0x0001E322
		public bool PlentifulHappenThisTurn { get; set; }

		// Token: 0x170003B3 RID: 947
		// (get) Token: 0x06000B52 RID: 2898 RVA: 0x0002012C File Offset: 0x0001E32C
		private bool InstinctActive
		{
			get
			{
				return this.Battle != null && this.Zone == CardZone.Hand && this.Battle.HandZone.Count >= 1 && (this == Enumerable.First<Card>(this.Battle.HandZone) || this == Enumerable.Last<Card>(this.Battle.HandZone));
			}
		}

		// Token: 0x170003B4 RID: 948
		// (get) Token: 0x06000B53 RID: 2899 RVA: 0x00020187 File Offset: 0x0001E387
		public bool IsMostLeftHand
		{
			get
			{
				return this.Battle != null && this.Zone == CardZone.Hand && this.Battle.HandZone.Count >= 1 && this == Enumerable.First<Card>(this.Battle.HandZone);
			}
		}

		// Token: 0x170003B5 RID: 949
		// (get) Token: 0x06000B54 RID: 2900 RVA: 0x000201C2 File Offset: 0x0001E3C2
		public bool IsMostRightHand
		{
			get
			{
				return this.Battle != null && this.Zone == CardZone.Hand && this.Battle.HandZone.Count >= 1 && this == Enumerable.Last<Card>(this.Battle.HandZone);
			}
		}

		// Token: 0x170003B6 RID: 950
		// (get) Token: 0x06000B55 RID: 2901 RVA: 0x000201FD File Offset: 0x0001E3FD
		// (set) Token: 0x06000B56 RID: 2902 RVA: 0x00020208 File Offset: 0x0001E408
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

		// Token: 0x170003B7 RID: 951
		// (get) Token: 0x06000B57 RID: 2903 RVA: 0x0002024E File Offset: 0x0001E44E
		// (set) Token: 0x06000B58 RID: 2904 RVA: 0x00020258 File Offset: 0x0001E458
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

		// Token: 0x170003B8 RID: 952
		// (get) Token: 0x06000B59 RID: 2905 RVA: 0x0002029E File Offset: 0x0001E49E
		public virtual bool CanUpgrade
		{
			get
			{
				return this.Config.IsUpgradable && !this.IsUpgraded;
			}
		}

		// Token: 0x170003B9 RID: 953
		// (get) Token: 0x06000B5A RID: 2906 RVA: 0x000202B8 File Offset: 0x0001E4B8
		public bool CanUpgradeAndPositive
		{
			get
			{
				return this.CanUpgrade && this.UpgradeIsPositive;
			}
		}

		// Token: 0x170003BA RID: 954
		// (get) Token: 0x06000B5B RID: 2907 RVA: 0x000202CA File Offset: 0x0001E4CA
		// (set) Token: 0x06000B5C RID: 2908 RVA: 0x000202D2 File Offset: 0x0001E4D2
		public virtual bool IsUpgraded { get; private set; }

		// Token: 0x170003BB RID: 955
		// (get) Token: 0x06000B5D RID: 2909 RVA: 0x000202DB File Offset: 0x0001E4DB
		public virtual bool DiscardCard
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003BC RID: 956
		// (get) Token: 0x06000B5E RID: 2910 RVA: 0x000202DE File Offset: 0x0001E4DE
		public virtual bool ShuffleToBottom
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003BD RID: 957
		// (get) Token: 0x06000B5F RID: 2911 RVA: 0x000202E1 File Offset: 0x0001E4E1
		public virtual bool UpgradeIsPositive
		{
			get
			{
				return this.Positive;
			}
		}

		// Token: 0x170003BE RID: 958
		// (get) Token: 0x06000B60 RID: 2912 RVA: 0x000202E9 File Offset: 0x0001E4E9
		public bool Positive
		{
			get
			{
				return !this.Negative;
			}
		}

		// Token: 0x170003BF RID: 959
		// (get) Token: 0x06000B61 RID: 2913 RVA: 0x000202F4 File Offset: 0x0001E4F4
		public virtual bool Negative
		{
			get
			{
				CardType cardType = this.CardType;
				return cardType == CardType.Status || cardType == CardType.Misfortune;
			}
		}

		// Token: 0x170003C0 RID: 960
		// (get) Token: 0x06000B62 RID: 2914 RVA: 0x00020319 File Offset: 0x0001E519
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

		// Token: 0x170003C1 RID: 961
		// (get) Token: 0x06000B63 RID: 2915 RVA: 0x00020351 File Offset: 0x0001E551
		public bool IsMultiColor
		{
			get
			{
				return this.Config.Colors.Count >= 1;
			}
		}

		// Token: 0x170003C2 RID: 962
		// (get) Token: 0x06000B64 RID: 2916 RVA: 0x00020369 File Offset: 0x0001E569
		public bool UseTransparentTexture
		{
			get
			{
				return this.IsCopy || this.IsAutoExile;
			}
		}

		// Token: 0x170003C3 RID: 963
		// (get) Token: 0x06000B65 RID: 2917 RVA: 0x0002037B File Offset: 0x0001E57B
		// (set) Token: 0x06000B66 RID: 2918 RVA: 0x00020383 File Offset: 0x0001E583
		public Keyword Keywords { get; internal set; }

		// Token: 0x06000B67 RID: 2919 RVA: 0x0002038C File Offset: 0x0001E58C
		public bool HasKeyword(Keyword keyword)
		{
			return this.Keywords.HasFlag(keyword);
		}

		// Token: 0x06000B68 RID: 2920 RVA: 0x000203A4 File Offset: 0x0001E5A4
		public void SetKeyword(Keyword keyword, bool value)
		{
			if (value != this.HasKeyword(keyword))
			{
				this.Keywords ^= keyword;
				this.NotifyChanged();
			}
		}

		// Token: 0x170003C4 RID: 964
		// (get) Token: 0x06000B69 RID: 2921 RVA: 0x000203C4 File Offset: 0x0001E5C4
		// (set) Token: 0x06000B6A RID: 2922 RVA: 0x000203D5 File Offset: 0x0001E5D5
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

		// Token: 0x170003C5 RID: 965
		// (get) Token: 0x06000B6B RID: 2923 RVA: 0x000203E7 File Offset: 0x0001E5E7
		// (set) Token: 0x06000B6C RID: 2924 RVA: 0x000203F1 File Offset: 0x0001E5F1
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

		// Token: 0x170003C6 RID: 966
		// (get) Token: 0x06000B6D RID: 2925 RVA: 0x000203FC File Offset: 0x0001E5FC
		// (set) Token: 0x06000B6E RID: 2926 RVA: 0x00020407 File Offset: 0x0001E607
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

		// Token: 0x170003C7 RID: 967
		// (get) Token: 0x06000B6F RID: 2927 RVA: 0x00020413 File Offset: 0x0001E613
		// (set) Token: 0x06000B70 RID: 2928 RVA: 0x00020421 File Offset: 0x0001E621
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

		// Token: 0x170003C8 RID: 968
		// (get) Token: 0x06000B71 RID: 2929 RVA: 0x00020430 File Offset: 0x0001E630
		// (set) Token: 0x06000B72 RID: 2930 RVA: 0x0002043E File Offset: 0x0001E63E
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

		// Token: 0x170003C9 RID: 969
		// (get) Token: 0x06000B73 RID: 2931 RVA: 0x0002044D File Offset: 0x0001E64D
		// (set) Token: 0x06000B74 RID: 2932 RVA: 0x0002045B File Offset: 0x0001E65B
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

		// Token: 0x170003CA RID: 970
		// (get) Token: 0x06000B75 RID: 2933 RVA: 0x0002046A File Offset: 0x0001E66A
		// (set) Token: 0x06000B76 RID: 2934 RVA: 0x00020478 File Offset: 0x0001E678
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

		// Token: 0x170003CB RID: 971
		// (get) Token: 0x06000B77 RID: 2935 RVA: 0x00020498 File Offset: 0x0001E698
		// (set) Token: 0x06000B78 RID: 2936 RVA: 0x000204A0 File Offset: 0x0001E6A0
		public bool IsTempExile { get; set; }

		// Token: 0x170003CC RID: 972
		// (get) Token: 0x06000B79 RID: 2937 RVA: 0x000204A9 File Offset: 0x0001E6A9
		// (set) Token: 0x06000B7A RID: 2938 RVA: 0x000204B7 File Offset: 0x0001E6B7
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

		// Token: 0x170003CD RID: 973
		// (get) Token: 0x06000B7B RID: 2939 RVA: 0x000204C6 File Offset: 0x0001E6C6
		// (set) Token: 0x06000B7C RID: 2940 RVA: 0x000204D4 File Offset: 0x0001E6D4
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

		// Token: 0x170003CE RID: 974
		// (get) Token: 0x06000B7D RID: 2941 RVA: 0x000204E3 File Offset: 0x0001E6E3
		// (set) Token: 0x06000B7E RID: 2942 RVA: 0x000204F1 File Offset: 0x0001E6F1
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

		// Token: 0x170003CF RID: 975
		// (get) Token: 0x06000B7F RID: 2943 RVA: 0x00020500 File Offset: 0x0001E700
		// (set) Token: 0x06000B80 RID: 2944 RVA: 0x0002050E File Offset: 0x0001E70E
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

		// Token: 0x170003D0 RID: 976
		// (get) Token: 0x06000B81 RID: 2945 RVA: 0x0002051D File Offset: 0x0001E71D
		// (set) Token: 0x06000B82 RID: 2946 RVA: 0x0002052B File Offset: 0x0001E72B
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

		// Token: 0x170003D1 RID: 977
		// (get) Token: 0x06000B83 RID: 2947 RVA: 0x0002053A File Offset: 0x0001E73A
		// (set) Token: 0x06000B84 RID: 2948 RVA: 0x00020548 File Offset: 0x0001E748
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

		// Token: 0x170003D2 RID: 978
		// (get) Token: 0x06000B85 RID: 2949 RVA: 0x00020557 File Offset: 0x0001E757
		// (set) Token: 0x06000B86 RID: 2950 RVA: 0x00020565 File Offset: 0x0001E765
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

		// Token: 0x170003D3 RID: 979
		// (get) Token: 0x06000B87 RID: 2951 RVA: 0x00020574 File Offset: 0x0001E774
		// (set) Token: 0x06000B88 RID: 2952 RVA: 0x00020585 File Offset: 0x0001E785
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

		// Token: 0x170003D4 RID: 980
		// (get) Token: 0x06000B89 RID: 2953 RVA: 0x00020597 File Offset: 0x0001E797
		// (set) Token: 0x06000B8A RID: 2954 RVA: 0x000205A5 File Offset: 0x0001E7A5
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

		// Token: 0x170003D5 RID: 981
		// (get) Token: 0x06000B8B RID: 2955 RVA: 0x000205B4 File Offset: 0x0001E7B4
		// (set) Token: 0x06000B8C RID: 2956 RVA: 0x000205C2 File Offset: 0x0001E7C2
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

		// Token: 0x170003D6 RID: 982
		// (get) Token: 0x06000B8D RID: 2957 RVA: 0x000205D1 File Offset: 0x0001E7D1
		// (set) Token: 0x06000B8E RID: 2958 RVA: 0x000205DF File Offset: 0x0001E7DF
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

		// Token: 0x170003D7 RID: 983
		// (get) Token: 0x06000B8F RID: 2959 RVA: 0x000205EE File Offset: 0x0001E7EE
		// (set) Token: 0x06000B90 RID: 2960 RVA: 0x000205FC File Offset: 0x0001E7FC
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

		// Token: 0x170003D8 RID: 984
		// (get) Token: 0x06000B91 RID: 2961 RVA: 0x0002060B File Offset: 0x0001E80B
		// (set) Token: 0x06000B92 RID: 2962 RVA: 0x00020619 File Offset: 0x0001E819
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

		// Token: 0x170003D9 RID: 985
		// (get) Token: 0x06000B93 RID: 2963 RVA: 0x00020628 File Offset: 0x0001E828
		// (set) Token: 0x06000B94 RID: 2964 RVA: 0x00020636 File Offset: 0x0001E836
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

		// Token: 0x170003DA RID: 986
		// (get) Token: 0x06000B95 RID: 2965 RVA: 0x00020645 File Offset: 0x0001E845
		// (set) Token: 0x06000B96 RID: 2966 RVA: 0x00020656 File Offset: 0x0001E856
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

		// Token: 0x170003DB RID: 987
		// (get) Token: 0x06000B97 RID: 2967 RVA: 0x00020668 File Offset: 0x0001E868
		// (set) Token: 0x06000B98 RID: 2968 RVA: 0x00020679 File Offset: 0x0001E879
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

		// Token: 0x170003DC RID: 988
		// (get) Token: 0x06000B99 RID: 2969 RVA: 0x0002068B File Offset: 0x0001E88B
		// (set) Token: 0x06000B9A RID: 2970 RVA: 0x0002069C File Offset: 0x0001E89C
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

		// Token: 0x170003DD RID: 989
		// (get) Token: 0x06000B9B RID: 2971 RVA: 0x000206AE File Offset: 0x0001E8AE
		public virtual bool CanUse
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170003DE RID: 990
		// (get) Token: 0x06000B9C RID: 2972 RVA: 0x000206B1 File Offset: 0x0001E8B1
		public virtual string CantUseMessage
		{
			get
			{
				return "ErrorChat.CardNotUsable".Localize(true);
			}
		}

		// Token: 0x170003DF RID: 991
		// (get) Token: 0x06000B9D RID: 2973 RVA: 0x000206BE File Offset: 0x0001E8BE
		// (set) Token: 0x06000B9E RID: 2974 RVA: 0x000206C8 File Offset: 0x0001E8C8
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

		// Token: 0x170003E0 RID: 992
		// (get) Token: 0x06000B9F RID: 2975 RVA: 0x0002071F File Offset: 0x0001E91F
		// (set) Token: 0x06000BA0 RID: 2976 RVA: 0x00020727 File Offset: 0x0001E927
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

		// Token: 0x170003E1 RID: 993
		// (get) Token: 0x06000BA1 RID: 2977 RVA: 0x0002073F File Offset: 0x0001E93F
		public bool CanBeDuplicated
		{
			get
			{
				return this.CardType != CardType.Tool && !this.IsCopy && this.Config.FindInBattle;
			}
		}

		// Token: 0x170003E2 RID: 994
		// (get) Token: 0x06000BA2 RID: 2978 RVA: 0x00020768 File Offset: 0x0001E968
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

		// Token: 0x170003E3 RID: 995
		// (get) Token: 0x06000BA3 RID: 2979 RVA: 0x000207D9 File Offset: 0x0001E9D9
		// (set) Token: 0x06000BA4 RID: 2980 RVA: 0x000207E4 File Offset: 0x0001E9E4
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

		// Token: 0x170003E4 RID: 996
		// (get) Token: 0x06000BA5 RID: 2981 RVA: 0x00020810 File Offset: 0x0001EA10
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

		// Token: 0x170003E5 RID: 997
		// (get) Token: 0x06000BA6 RID: 2982 RVA: 0x00020884 File Offset: 0x0001EA84
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

		// Token: 0x170003E6 RID: 998
		// (get) Token: 0x06000BA7 RID: 2983 RVA: 0x000208F8 File Offset: 0x0001EAF8
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

		// Token: 0x170003E7 RID: 999
		// (get) Token: 0x06000BA8 RID: 2984 RVA: 0x0002096C File Offset: 0x0001EB6C
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

		// Token: 0x170003E8 RID: 1000
		// (get) Token: 0x06000BA9 RID: 2985 RVA: 0x000209E0 File Offset: 0x0001EBE0
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

		// Token: 0x170003E9 RID: 1001
		// (get) Token: 0x06000BAA RID: 2986 RVA: 0x00020AA8 File Offset: 0x0001ECA8
		public int MinLoyaltyToUseSkill
		{
			get
			{
				return -this.MinActiveCost;
			}
		}

		// Token: 0x170003EA RID: 1002
		// (get) Token: 0x06000BAB RID: 2987 RVA: 0x00020AB4 File Offset: 0x0001ECB4
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

		// Token: 0x170003EB RID: 1003
		// (get) Token: 0x06000BAC RID: 2988 RVA: 0x00020B4C File Offset: 0x0001ED4C
		private int MinLoyaltyToChooseSkill
		{
			get
			{
				return -this.MinSecondActiveCost;
			}
		}

		// Token: 0x170003EC RID: 1004
		// (get) Token: 0x06000BAD RID: 2989 RVA: 0x00020B55 File Offset: 0x0001ED55
		[UsedImplicitly]
		public string FriendS
		{
			get
			{
				return "<indent=80>";
			}
		}

		// Token: 0x170003ED RID: 1005
		// (get) Token: 0x06000BAE RID: 2990 RVA: 0x00020B5C File Offset: 0x0001ED5C
		[UsedImplicitly]
		public virtual FriendCostInfo FriendP
		{
			get
			{
				return new FriendCostInfo(this.PassiveCost, FriendCostType.Passive);
			}
		}

		// Token: 0x170003EE RID: 1006
		// (get) Token: 0x06000BAF RID: 2991 RVA: 0x00020B6A File Offset: 0x0001ED6A
		[UsedImplicitly]
		public virtual FriendCostInfo FriendA
		{
			get
			{
				return new FriendCostInfo(this.ActiveCost, FriendCostType.Active);
			}
		}

		// Token: 0x170003EF RID: 1007
		// (get) Token: 0x06000BB0 RID: 2992 RVA: 0x00020B78 File Offset: 0x0001ED78
		[UsedImplicitly]
		public virtual FriendCostInfo FriendA2
		{
			get
			{
				return new FriendCostInfo(this.ActiveCost2, FriendCostType.Active);
			}
		}

		// Token: 0x170003F0 RID: 1008
		// (get) Token: 0x06000BB1 RID: 2993 RVA: 0x00020B86 File Offset: 0x0001ED86
		[UsedImplicitly]
		public virtual FriendCostInfo FriendU
		{
			get
			{
				return new FriendCostInfo(this.UltimateCost, FriendCostType.Ultimate);
			}
		}

		// Token: 0x170003F1 RID: 1009
		// (get) Token: 0x06000BB2 RID: 2994 RVA: 0x00020B94 File Offset: 0x0001ED94
		// (set) Token: 0x06000BB3 RID: 2995 RVA: 0x00020B9C File Offset: 0x0001ED9C
		public bool Summoning { get; internal set; }

		// Token: 0x170003F2 RID: 1010
		// (get) Token: 0x06000BB4 RID: 2996 RVA: 0x00020BA5 File Offset: 0x0001EDA5
		// (set) Token: 0x06000BB5 RID: 2997 RVA: 0x00020BAD File Offset: 0x0001EDAD
		public bool Summoned { get; set; }

		// Token: 0x170003F3 RID: 1011
		// (get) Token: 0x06000BB6 RID: 2998 RVA: 0x00020BB6 File Offset: 0x0001EDB6
		// (set) Token: 0x06000BB7 RID: 2999 RVA: 0x00020BBE File Offset: 0x0001EDBE
		public bool UltimateUsed { get; set; }

		// Token: 0x06000BB8 RID: 3000 RVA: 0x00020BC7 File Offset: 0x0001EDC7
		public void Summon()
		{
			this.Summoning = true;
			this.Summoned = true;
		}

		// Token: 0x170003F4 RID: 1012
		// (get) Token: 0x06000BB9 RID: 3001 RVA: 0x00020BD7 File Offset: 0x0001EDD7
		// (set) Token: 0x06000BBA RID: 3002 RVA: 0x00020BDF File Offset: 0x0001EDDF
		public FriendToken FriendToken { get; set; }

		// Token: 0x06000BBB RID: 3003 RVA: 0x00020BE8 File Offset: 0x0001EDE8
		public virtual IEnumerable<BattleAction> GetPassiveActions()
		{
			return null;
		}

		// Token: 0x170003F5 RID: 1013
		// (get) Token: 0x06000BBC RID: 3004 RVA: 0x00020BEC File Offset: 0x0001EDEC
		// (set) Token: 0x06000BBD RID: 3005 RVA: 0x00020C0B File Offset: 0x0001EE0B
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

		// Token: 0x06000BBE RID: 3006 RVA: 0x00020C19 File Offset: 0x0001EE19
		public void SetBattle(BattleController battle)
		{
			this.Battle = battle;
			base.GameRun = battle.GameRun;
		}

		// Token: 0x06000BBF RID: 3007 RVA: 0x00020C2E File Offset: 0x0001EE2E
		public EntityName GetName()
		{
			return new EntityName("Card." + base.Id, this.Name);
		}

		// Token: 0x170003F6 RID: 1014
		// (get) Token: 0x06000BC0 RID: 3008 RVA: 0x00020C4C File Offset: 0x0001EE4C
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

		// Token: 0x170003F7 RID: 1015
		// (get) Token: 0x06000BC1 RID: 3009 RVA: 0x00020C9B File Offset: 0x0001EE9B
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

		// Token: 0x06000BC2 RID: 3010 RVA: 0x00020CBC File Offset: 0x0001EEBC
		public IEnumerable<Keyword> EnumerateCardKeywords()
		{
			return LBoL.Core.Keywords.EnumerateComponents(this.Keywords);
		}

		// Token: 0x06000BC3 RID: 3011 RVA: 0x00020CC9 File Offset: 0x0001EEC9
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

		// Token: 0x06000BC4 RID: 3012 RVA: 0x00020CE0 File Offset: 0x0001EEE0
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

		// Token: 0x06000BC5 RID: 3013 RVA: 0x00020CF0 File Offset: 0x0001EEF0
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

		// Token: 0x06000BC6 RID: 3014 RVA: 0x00020D00 File Offset: 0x0001EF00
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

		// Token: 0x06000BC7 RID: 3015 RVA: 0x00020DC2 File Offset: 0x0001EFC2
		internal override GameEntityFormatWrapper CreateFormatWrapper()
		{
			return new Card.CardFormatWrapper(this);
		}

		// Token: 0x06000BC8 RID: 3016 RVA: 0x00020DCC File Offset: 0x0001EFCC
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

		// Token: 0x06000BC9 RID: 3017 RVA: 0x00020ECC File Offset: 0x0001F0CC
		public static int CompareCard(Card a, Card b)
		{
			int num = a.Config.Order.CompareTo(b.Config.Order);
			if (num != 0)
			{
				return num;
			}
			return string.Compare(a.Id, b.Id, 4);
		}

		// Token: 0x06000BCA RID: 3018 RVA: 0x00020F10 File Offset: 0x0001F110
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

		// Token: 0x06000BCB RID: 3019 RVA: 0x00020F90 File Offset: 0x0001F190
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

		// Token: 0x06000BCC RID: 3020 RVA: 0x00020FEC File Offset: 0x0001F1EC
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

		// Token: 0x06000BCD RID: 3021 RVA: 0x0002102C File Offset: 0x0001F22C
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

		// Token: 0x1400000D RID: 13
		// (add) Token: 0x06000BCE RID: 3022 RVA: 0x00021164 File Offset: 0x0001F364
		// (remove) Token: 0x06000BCF RID: 3023 RVA: 0x0002119C File Offset: 0x0001F39C
		public event Action Activating;

		// Token: 0x06000BD0 RID: 3024 RVA: 0x000211D1 File Offset: 0x0001F3D1
		public void NotifyActivating()
		{
			Action activating = this.Activating;
			if (activating == null)
			{
				return;
			}
			activating.Invoke();
		}

		// Token: 0x06000BD1 RID: 3025 RVA: 0x000211E3 File Offset: 0x0001F3E3
		public virtual bool ShouldPreventOtherCardUsage(Card card)
		{
			return false;
		}

		// Token: 0x170003F8 RID: 1016
		// (get) Token: 0x06000BD2 RID: 3026 RVA: 0x000211E6 File Offset: 0x0001F3E6
		public virtual string PreventCardUsageMessage
		{
			get
			{
				throw new InvalidOperationException("Cannot get prevent card message key for " + this.DebugName);
			}
		}

		// Token: 0x06000BD3 RID: 3027 RVA: 0x00021200 File Offset: 0x0001F400
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

		// Token: 0x0400052C RID: 1324
		private readonly GameEventHandlerHolder _handlerHolder = new GameEventHandlerHolder();

		// Token: 0x04000530 RID: 1328
		private int _deltaDamage;

		// Token: 0x04000531 RID: 1329
		private int _deltaBlock;

		// Token: 0x04000532 RID: 1330
		private int _deltaShield;

		// Token: 0x04000533 RID: 1331
		private int _deltaValue1;

		// Token: 0x04000534 RID: 1332
		private int _deltaValue2;

		// Token: 0x04000535 RID: 1333
		private int _deltaInt;

		// Token: 0x04000537 RID: 1335
		private ManaGroup _kickerDelta;

		// Token: 0x04000539 RID: 1337
		private ManaGroup _auraCost;

		// Token: 0x0400053A RID: 1338
		private ManaGroup _baseCost;

		// Token: 0x0400053B RID: 1339
		private ManaGroup _turnCostDelta;

		// Token: 0x0400053F RID: 1343
		private bool _debutCardPlayOnce;

		// Token: 0x04000541 RID: 1345
		private int _playCount;

		// Token: 0x04000543 RID: 1347
		private int? _deckCounter;

		// Token: 0x04000544 RID: 1348
		private int? _upgradeCounter;

		// Token: 0x04000548 RID: 1352
		private ManaGroup? _pendingManaUsage;

		// Token: 0x04000549 RID: 1353
		private Unit _pendingTarget;

		// Token: 0x0400054A RID: 1354
		private int _loyalty;

		// Token: 0x0400054F RID: 1359
		private readonly WeakReference<BattleController> _battle = new WeakReference<BattleController>(null);

		// Token: 0x0200027E RID: 638
		protected enum PerformTargetType
		{
			// Token: 0x040009D9 RID: 2521
			Player,
			// Token: 0x040009DA RID: 2522
			ToTarget,
			// Token: 0x040009DB RID: 2523
			TargetSelf,
			// Token: 0x040009DC RID: 2524
			EachEnemy,
			// Token: 0x040009DD RID: 2525
			EachEnemySelf,
			// Token: 0x040009DE RID: 2526
			WorldSpace
		}

		// Token: 0x0200027F RID: 639
		internal sealed class CardFormatWrapper : GameEntityFormatWrapper
		{
			// Token: 0x0600133D RID: 4925 RVA: 0x000341F0 File Offset: 0x000323F0
			public CardFormatWrapper(Card card)
				: base(card)
			{
				this._card = card;
			}

			// Token: 0x0600133E RID: 4926 RVA: 0x00034200 File Offset: 0x00032400
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

			// Token: 0x040009DF RID: 2527
			private readonly Card _card;
		}
	}
}
