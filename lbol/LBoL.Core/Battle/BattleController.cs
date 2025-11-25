using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Exhibits;
using LBoL.Core.Randoms;
using LBoL.Core.Stats;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using UnityEngine;
namespace LBoL.Core.Battle
{
	public class BattleController
	{
		private IEnumerator<object> ResolveDebugActions()
		{
			ValueTuple<BattleAction, string> valueTuple;
			while (this._debugActionQueue.TryDequeue(ref valueTuple))
			{
				ValueTuple<BattleAction, string> valueTuple2 = valueTuple;
				BattleAction item = valueTuple2.Item1;
				string item2 = valueTuple2.Item2;
				yield return this._resolver.Resolve(item, item2);
			}
			yield break;
		}
		private IEnumerator<object> ResolveAction(BattleAction battleAction, [MaybeNull] string recordName = null)
		{
			yield return this.ResolveDebugActions();
			yield return this._resolver.Resolve(battleAction, recordName);
			yield break;
		}
		public BattleStats Stats { get; } = new BattleStats();
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
		public ActionViewer ActionViewer { get; } = new ActionViewer();
		public PlayerUnit Player { get; }
		public EnemyGroup EnemyGroup { get; }
		public ManaGroup BaseTurnMana
		{
			get
			{
				return this.GameRun.BaseMana;
			}
		}
		public ManaGroup ExtraTurnMana { get; private set; }
		public ManaGroup TurnMana
		{
			get
			{
				return this.BaseTurnMana - this.LockedTurnMana + this.ExtraTurnMana;
			}
		}
		public ManaGroup LockedTurnMana { get; private set; }
		public ManaGroup BattleMana { get; private set; }
		public IReadOnlyList<Card> DrawZone
		{
			get
			{
				return this._drawZone.AsReadOnly();
			}
		}
		public IReadOnlyList<Card> DrawZoneIndexOrder
		{
			get
			{
				return Enumerable.ToList<Card>(Enumerable.OrderBy<Card, int>(this._drawZone, (Card card) => card.Config.Index)).AsReadOnly();
			}
		}
		public IReadOnlyList<Card> DrawZoneToShow
		{
			get
			{
				if (this.GameRun.CanViewDrawZoneActualOrder <= 0)
				{
					return this.DrawZoneIndexOrder;
				}
				return this.DrawZone;
			}
		}
		public IReadOnlyList<Card> HandZone
		{
			get
			{
				return this._handZone.AsReadOnly();
			}
		}
		public IReadOnlyList<Card> PlayArea
		{
			get
			{
				return this._playArea.AsReadOnly();
			}
		}
		public IReadOnlyList<Card> FollowArea
		{
			get
			{
				return this._followArea.AsReadOnly();
			}
		}
		public IReadOnlyList<Card> HandZoneAndPlayArea
		{
			get
			{
				return Enumerable.ToList<Card>(Enumerable.Concat<Card>(this._handZone, this._playArea)).AsReadOnly();
			}
		}
		public IReadOnlyList<Card> DiscardZone
		{
			get
			{
				return this._discardZone.AsReadOnly();
			}
		}
		public IReadOnlyList<Card> ExileZone
		{
			get
			{
				return this._exileZone.AsReadOnly();
			}
		}
		public IEnumerable<Card> EnumerateAllCards()
		{
			return Enumerable.Concat<Card>(Enumerable.Concat<Card>(Enumerable.Concat<Card>(Enumerable.Concat<Card>(Enumerable.Concat<Card>(this._handZone, this._playArea), this._followArea), this._drawZone), this._discardZone), this._exileZone);
		}
		public IEnumerable<Card> EnumerateAllCardsButExile()
		{
			return Enumerable.Concat<Card>(Enumerable.Concat<Card>(Enumerable.Concat<Card>(Enumerable.Concat<Card>(this._handZone, this._playArea), this._followArea), this._drawZone), this._discardZone);
		}
		public IEnumerable<Card> EnumerateAllCardsButPlayingAreas()
		{
			return Enumerable.Concat<Card>(Enumerable.Concat<Card>(Enumerable.Concat<Card>(this._handZone, this._drawZone), this._discardZone), this._exileZone);
		}
		public IReadOnlyList<Card> TurnDrawHistory
		{
			get
			{
				return this._turnDrawHistory.AsReadOnly();
			}
		}
		public IReadOnlyList<Card> TurnDiscardHistory
		{
			get
			{
				return this._turnDiscardHistory.AsReadOnly();
			}
		}
		public IReadOnlyList<Card> TurnExileHistory
		{
			get
			{
				return this._turnExileHistory.AsReadOnly();
			}
		}
		public IReadOnlyList<Card> TurnCardUsageHistory
		{
			get
			{
				return this._turnCardUsageHistory.AsReadOnly();
			}
		}
		public IReadOnlyList<Card> BattleCardUsageHistory
		{
			get
			{
				return this._battleCardUsageHistory.AsReadOnly();
			}
		}
		public IReadOnlyList<Card> TurnCardPlayHistory
		{
			get
			{
				return this._turnCardPlayHistory.AsReadOnly();
			}
		}
		public IReadOnlyList<Card> BattleCardPlayHistory
		{
			get
			{
				return this._battleCardPlayHistory.AsReadOnly();
			}
		}
		public IReadOnlyList<Card> TurnCardFollowAttackHistory
		{
			get
			{
				return this._turnCardFollowAttackHistory.AsReadOnly();
			}
		}
		public IReadOnlyList<Card> BattleCardFollowAttackHistory
		{
			get
			{
				return this._battleCardFollowAttackHistory.AsReadOnly();
			}
		}
		public int RoundCounter { get; private set; }
		internal BattleController(GameRunController gameRun, EnemyGroup enemyGroup, IEnumerable<Card> deck)
		{
			this.GameRun = gameRun;
			this._resolver = new ActionResolver(this);
			this.Player = gameRun.Player;
			this.EnemyGroup = enemyGroup;
			this.BattleMana = default(ManaGroup);
			this.DrawCardCount = this.GameRun.DrawCardCount;
			List<Card> list = new List<Card>();
			List<Card> list2 = new List<Card>();
			List<Card> list3 = new List<Card>();
			foreach (Card card in deck)
			{
				Card card2 = card.Clone(false);
				card2.InstanceId = card.InstanceId;
				if (card.IsInitial)
				{
					list.Add(card2);
				}
				else if (card.ShuffleToBottom)
				{
					list3.Add(card2);
				}
				else
				{
					list2.Add(card2);
				}
			}
			list.Shuffle(this.GameRun.ShuffleRng);
			list2.Shuffle(this.GameRun.ShuffleRng);
			list3.Shuffle(gameRun.ShuffleRng);
			this._drawZone.AddRange(list);
			this._drawZone.AddRange(list2);
			this._drawZone.AddRange(list3);
			foreach (Card card3 in this._drawZone)
			{
				card3.GameRun = this.GameRun;
				card3.Zone = CardZone.Draw;
				card3.EnterBattle(this);
			}
			this.Player.EnterBattle(this);
			foreach (EnemyUnit enemyUnit in this.EnemyGroup)
			{
				enemyUnit.EnterBattle(this);
			}
		}
		internal void Leave()
		{
			foreach (Card card in this.EnumerateAllCards())
			{
				card.LeaveBattle();
				card.GameRun = null;
			}
			foreach (EnemyUnit enemyUnit in this.EnemyGroup)
			{
				enemyUnit.LeaveBattle();
			}
			this.Player.LeaveBattle();
			this.Stats.TotalRounds = this.RoundCounter;
			this.Stats.RemainCardCount = this._drawZone.Count + this._discardZone.Count + this._handZone.Count + this._playArea.Count;
		}
		internal void NotifyMessage(BattleMessage message)
		{
			Action<BattleMessage> notification = this.Notification;
			if (notification == null)
			{
				return;
			}
			notification.Invoke(message);
		}
		private void ShuffleDrawPile()
		{
			if (Enumerable.Any<Card>(this._drawZone, (Card card) => card.ShuffleToBottom))
			{
				List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(this._drawZone, (Card card) => !card.ShuffleToBottom));
				List<Card> list2 = Enumerable.ToList<Card>(Enumerable.Where<Card>(this._drawZone, (Card card) => card.ShuffleToBottom));
				list.Shuffle(this.GameRun.ShuffleRng);
				list2.Shuffle(this.GameRun.ShuffleRng);
				this._drawZone.Clear();
				this._drawZone.AddRange(list);
				this._drawZone.AddRange(list2);
				return;
			}
			this._drawZone.Shuffle(this.GameRun.ShuffleRng);
		}
		internal void StartBattle()
		{
			if (this.Player.HasUs)
			{
				this.Player.Us.TurnAvailable = true;
				this.Player.Us.BattleAvailable = true;
				return;
			}
			this.Player.Us.TurnAvailable = false;
			this.Player.Us.BattleAvailable = false;
		}
		internal void EndBattle()
		{
		}
		internal Card DrawCard()
		{
			if (this._handZone.Count >= this.MaxHand)
			{
				this.NotifyMessage(BattleMessage.HandFull);
				return null;
			}
			if (this._drawZone.Empty<Card>())
			{
				this.NotifyMessage(BattleMessage.EmptyDraw);
				return null;
			}
			Card card = this._drawZone[0];
			this._drawZone.RemoveAt(0);
			this._handZone.Add(card);
			card.Zone = CardZone.Hand;
			this._turnDrawHistory.Add(card);
			return card;
		}
		internal Card DrawSelectedCard(Card card)
		{
			if (this._handZone.Count >= this.MaxHand)
			{
				this.NotifyMessage(BattleMessage.HandFull);
				return null;
			}
			if (!this._drawZone.Contains(card))
			{
				throw new InvalidOperationException("Drawing " + card.Name + " not in hand");
			}
			this._drawZone.Remove(card);
			this._handZone.Add(card);
			card.Zone = CardZone.Hand;
			this._turnDrawHistory.Add(card);
			return card;
		}
		internal void Reshuffle()
		{
			foreach (Card card in this._discardZone)
			{
				card.Zone = CardZone.Draw;
			}
			this._drawZone.AddRange(this._discardZone);
			this._discardZone.Clear();
			this.ShuffleDrawPile();
		}
		internal void MoveCardToPlayArea(Card card)
		{
			if (!this._playArea.Empty<Card>())
			{
				string[] array = new string[5];
				array[0] = "Moving <";
				array[1] = card.Name;
				array[2] = "> to play-area while using other card: {";
				array[3] = ", ".Join(Enumerable.Select<Card, string>(this._playArea, (Card c) => c.Name));
				array[4] = "}";
				Debug.LogWarning(string.Concat(array));
			}
			if (!this._handZone.Contains(card))
			{
				string[] array2 = new string[5];
				array2[0] = "Playing card ";
				array2[1] = card.Name;
				array2[2] = " not in hand zone: {";
				array2[3] = ", ".Join(Enumerable.Select<Card, string>(this._handZone, (Card c) => c.Name));
				array2[4] = "}";
				throw new InvalidOperationException(string.Concat(array2));
			}
			card.HandIndexWhenPlaying = this._handZone.IndexOf(card);
			this._handZone.Remove(card);
			this._playArea.Add(card);
			card.Zone = CardZone.PlayArea;
		}
		internal void MoveCardToFollowArea(Card card)
		{
			if (this._drawZone.Contains(card))
			{
				this._drawZone.Remove(card);
			}
			else if (this._discardZone.Contains(card))
			{
				this._discardZone.Remove(card);
			}
			else if (this._exileZone.Contains(card))
			{
				this._exileZone.Remove(card);
			}
			else
			{
				if (!this._handZone.Contains(card))
				{
					throw new InvalidOperationException("Follow-playing card " + card.Name + " not in correct zones.");
				}
				this._handZone.Remove(card);
			}
			card.HandIndexWhenPlaying = 0;
			this._followArea.Add(card);
			card.Zone = CardZone.FollowArea;
		}
		internal CancelCause Discard(Card card)
		{
			if (this._discardZone.Count >= 9999)
			{
				this.NotifyMessage(BattleMessage.ZoneFull);
				return CancelCause.ZoneFull;
			}
			CardZone zone = card.Zone;
			List<Card> list;
			switch (zone)
			{
			case CardZone.None:
				throw new InvalidOperationException("Discarding card '" + card.DebugName + "' is not in battle");
			case CardZone.Draw:
				list = this._drawZone;
				break;
			case CardZone.Hand:
				list = this._handZone;
				break;
			case CardZone.Discard:
				throw new InvalidOperationException("Dicarding card '" + card.DebugName + "' already in discard zone");
			case CardZone.Exile:
				throw new InvalidOperationException("Cannot discard '" + card.DebugName + "' from exile zone");
			case CardZone.PlayArea:
				list = this._playArea;
				break;
			case CardZone.FollowArea:
				list = this._followArea;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			list.Remove(card);
			this._discardZone.Add(card);
			if (zone == CardZone.Hand || zone == CardZone.PlayArea)
			{
				card.OnLeaveHand();
			}
			card.Zone = CardZone.Discard;
			this._turnDiscardHistory.Add(card);
			return CancelCause.None;
		}
		internal CancelCause ExileCard(Card card)
		{
			if (this._exileZone.Count >= 9999)
			{
				this.NotifyMessage(BattleMessage.ZoneFull);
				return CancelCause.ZoneFull;
			}
			CardZone zone = card.Zone;
			List<Card> list;
			switch (zone)
			{
			case CardZone.None:
				throw new InvalidOperationException("Exiling card '" + card.DebugName + "' is not in battle");
			case CardZone.Draw:
				list = this._drawZone;
				break;
			case CardZone.Hand:
				list = this._handZone;
				break;
			case CardZone.Discard:
				list = this._discardZone;
				break;
			case CardZone.Exile:
				throw new InvalidOperationException("Exiling card '" + card.DebugName + "' already in exile zone");
			case CardZone.PlayArea:
				list = this._playArea;
				break;
			case CardZone.FollowArea:
				list = this._followArea;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			list.Remove(card);
			this._exileZone.Add(card);
			if (zone == CardZone.Hand || zone == CardZone.PlayArea)
			{
				card.OnLeaveHand();
			}
			card.Zone = CardZone.Exile;
			this._turnExileHistory.Add(card);
			return CancelCause.None;
		}
		internal CancelCause TransformCard(Card sourceCard, Card destinationCard)
		{
			if (destinationCard.Zone != CardZone.None)
			{
				throw new InvalidOperationException(string.Format("Cannot Transform to {0} while it's already in:{1}", destinationCard.DebugName, destinationCard.Zone));
			}
			destinationCard.Zone = sourceCard.Zone;
			List<Card> list;
			switch (sourceCard.Zone)
			{
			case CardZone.None:
				throw new InvalidOperationException("Cannot transform card " + sourceCard.DebugName + " while it's not in any zone of battle.");
			case CardZone.Draw:
				list = this._drawZone;
				break;
			case CardZone.Hand:
				list = this._handZone;
				break;
			case CardZone.Discard:
				list = this._discardZone;
				break;
			case CardZone.Exile:
				list = this._exileZone;
				break;
			case CardZone.PlayArea:
				list = this._playArea;
				break;
			case CardZone.FollowArea:
				list = this._followArea;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			List<Card> list2 = list;
			int num = list2.IndexOf(sourceCard);
			this.RemoveCard(sourceCard);
			list2.Insert(num, destinationCard);
			destinationCard.GameRun = this.GameRun;
			int num2 = this._cardInstanceId + 1;
			this._cardInstanceId = num2;
			destinationCard.InstanceId = num2;
			destinationCard.EnterBattle(this);
			this.GameRun.RevealCard(destinationCard);
			return CancelCause.None;
		}
		internal static void MoveCardCheck(Card card, CardZone dstZone)
		{
			if (dstZone == CardZone.None)
			{
				throw new InvalidOperationException("Cannot move card '" + card.DebugName + "' to None zone");
			}
			if (dstZone == CardZone.Draw)
			{
				throw new InvalidOperationException("Cannot move card '" + card.DebugName + "' to draw-zone");
			}
			if (dstZone == card.Zone)
			{
				throw new InvalidOperationException(string.Format("Cannot move card '{0}' (in {1}) to same zone", card.DebugName, card.Zone));
			}
		}
		internal CancelCause MoveCard(Card card, CardZone dstZone)
		{
			BattleController.MoveCardCheck(card, dstZone);
			CardZone zone = card.Zone;
			List<Card> list;
			switch (card.Zone)
			{
			case CardZone.None:
				throw new InvalidOperationException(string.Format("Cannot move new card '{0}' to {1}", card.DebugName, dstZone));
			case CardZone.Draw:
				list = this._drawZone;
				break;
			case CardZone.Hand:
				list = this._handZone;
				break;
			case CardZone.Discard:
				list = this._discardZone;
				break;
			case CardZone.Exile:
				list = this._exileZone;
				break;
			case CardZone.PlayArea:
				list = this._playArea;
				break;
			case CardZone.FollowArea:
				list = this._followArea;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			List<Card> list2 = list;
			if (!list2.Contains(card))
			{
				throw new InvalidOperationException(string.Format("Fatal: {0} does not contain card '{1}'.", card.Zone, card.DebugName));
			}
			if (dstZone == CardZone.Hand && this._handZone.Count >= this.MaxHand)
			{
				this.NotifyMessage(BattleMessage.HandFull);
				return CancelCause.HandFull;
			}
			switch (dstZone)
			{
			case CardZone.Hand:
				list = this._handZone;
				break;
			case CardZone.Discard:
				list = this._discardZone;
				break;
			case CardZone.Exile:
				list = this._exileZone;
				break;
			default:
				throw new ArgumentOutOfRangeException("dstZone", dstZone, null);
			}
			List<Card> list3 = list;
			if (list3.Count >= 9999)
			{
				this.NotifyMessage(BattleMessage.ZoneFull);
				return CancelCause.ZoneFull;
			}
			list2.Remove(card);
			list3.Add(card);
			if ((zone == CardZone.Hand || zone == CardZone.PlayArea) && (dstZone == CardZone.Exile || dstZone == CardZone.Discard))
			{
				card.OnLeaveHand();
			}
			card.Zone = dstZone;
			return CancelCause.None;
		}
		internal CancelCause MoveCardToDrawZone(Card card, DrawZoneTarget target)
		{
			CardZone zone = card.Zone;
			if (this._drawZone.Count >= 9999)
			{
				this.NotifyMessage(BattleMessage.ZoneFull);
				return CancelCause.ZoneFull;
			}
			List<Card> list;
			switch (zone)
			{
			case CardZone.None:
				throw new InvalidOperationException("Cannot move new card " + card.DebugName + " to draw zone, use AddCardsToDrawZoneAction instead");
			case CardZone.Draw:
				list = this._drawZone;
				break;
			case CardZone.Hand:
				list = this._handZone;
				break;
			case CardZone.Discard:
				list = this._discardZone;
				break;
			case CardZone.Exile:
				list = this._exileZone;
				break;
			case CardZone.PlayArea:
				list = this._playArea;
				break;
			case CardZone.FollowArea:
				list = this._followArea;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			list.Remove(card);
			int num;
			switch (target)
			{
			case DrawZoneTarget.Top:
				num = 0;
				break;
			case DrawZoneTarget.Bottom:
				num = this._drawZone.Count;
				break;
			case DrawZoneTarget.Random:
				num = this.GameRun.BattleRng.NextInt(0, this._drawZone.Count);
				break;
			default:
				throw new ArgumentOutOfRangeException("target", target, null);
			}
			int num2 = num;
			this._drawZone.Insert(num2, card);
			if (zone == CardZone.Hand || zone == CardZone.PlayArea)
			{
				card.OnLeaveHand();
			}
			card.Zone = CardZone.Draw;
			return CancelCause.None;
		}
		internal CancelCause AddCardToDrawZone(Card card, DrawZoneTarget target)
		{
			if (card.Zone != CardZone.None)
			{
				throw new InvalidOperationException(string.Format("Cannot add {0} from {1} to draw zone, use {2} instead", card.DebugName, card.Zone, "MoveCardToDrawZoneAction"));
			}
			if (this._drawZone.Count >= 9999)
			{
				this.NotifyMessage(BattleMessage.ZoneFull);
				return CancelCause.ZoneFull;
			}
			int num;
			switch (target)
			{
			case DrawZoneTarget.Top:
				num = 0;
				break;
			case DrawZoneTarget.Bottom:
				num = this._drawZone.Count;
				break;
			case DrawZoneTarget.Random:
				num = this.GameRun.BattleRng.NextInt(0, this._drawZone.Count);
				break;
			default:
				throw new ArgumentOutOfRangeException("target", target, null);
			}
			int num2 = num;
			this._drawZone.Insert(num2, card);
			card.GameRun = this.GameRun;
			num = this._cardInstanceId + 1;
			this._cardInstanceId = num;
			card.InstanceId = num;
			card.Zone = CardZone.Draw;
			card.EnterBattle(this);
			this.GameRun.RevealCard(card);
			return CancelCause.None;
		}
		internal CancelCause AddCardToHand(Card card)
		{
			if (card.Zone != CardZone.None)
			{
				throw new InvalidOperationException(string.Format("Cannot add {0} from {1} to hand zone, use {2} or other actions instead", card.DebugName, card.Zone, "MoveCardAction"));
			}
			if (this._handZone.Count >= this.MaxHand)
			{
				this.NotifyMessage(BattleMessage.HandFull);
				return CancelCause.HandFull;
			}
			this._handZone.Add(card);
			card.GameRun = this.GameRun;
			int num = this._cardInstanceId + 1;
			this._cardInstanceId = num;
			card.InstanceId = num;
			card.Zone = CardZone.Hand;
			card.EnterBattle(this);
			this.GameRun.RevealCard(card);
			return CancelCause.None;
		}
		internal CancelCause AddCardToDiscard(Card card)
		{
			if (card.Zone != CardZone.None)
			{
				throw new InvalidOperationException(string.Format("Cannot add {0} from {1} to discard zone, use {2} or other actions instead", card.DebugName, card.Zone, "MoveCardAction"));
			}
			if (this._discardZone.Count >= 9999)
			{
				this.NotifyMessage(BattleMessage.ZoneFull);
				return CancelCause.ZoneFull;
			}
			this._discardZone.Add(card);
			card.GameRun = this.GameRun;
			int num = this._cardInstanceId + 1;
			this._cardInstanceId = num;
			card.InstanceId = num;
			card.Zone = CardZone.Discard;
			card.EnterBattle(this);
			this.GameRun.RevealCard(card);
			return CancelCause.None;
		}
		internal CancelCause AddCardToExile(Card card)
		{
			if (card.Zone != CardZone.None)
			{
				throw new InvalidOperationException(string.Format("Cannot add {0} from {1} to exile zone, use {2} or other actions instead", card.DebugName, card.Zone, "MoveCardAction"));
			}
			if (this._exileZone.Count >= 9999)
			{
				this.NotifyMessage(BattleMessage.ZoneFull);
				return CancelCause.ZoneFull;
			}
			this._exileZone.Add(card);
			card.GameRun = this.GameRun;
			int num = this._cardInstanceId + 1;
			this._cardInstanceId = num;
			card.InstanceId = num;
			card.Zone = CardZone.Exile;
			card.EnterBattle(this);
			this.GameRun.RevealCard(card);
			return CancelCause.None;
		}
		internal CancelCause AddCardToFollowArea(Card card)
		{
			if (card.Zone != CardZone.None)
			{
				throw new InvalidOperationException(string.Format("Cannot add {0} from {1} to follow area, use {2} or other actions instead", card.DebugName, card.Zone, "MoveCardAction"));
			}
			if (this._handZone.Count >= 9999)
			{
				this.NotifyMessage(BattleMessage.ZoneFull);
				return CancelCause.ZoneFull;
			}
			this._followArea.Add(card);
			card.GameRun = this.GameRun;
			int num = this._cardInstanceId + 1;
			this._cardInstanceId = num;
			card.InstanceId = num;
			card.Zone = CardZone.FollowArea;
			card.EnterBattle(this);
			this.GameRun.RevealCard(card);
			return CancelCause.None;
		}
		internal void RemoveCard(Card card)
		{
			List<Card> list;
			switch (card.Zone)
			{
			case CardZone.None:
			{
				string[] array = new string[5];
				array[0] = "Removing card ";
				array[1] = card.Name;
				array[2] = " not in battle: {";
				array[3] = ", ".Join(Enumerable.Select<Card, string>(this._handZone, (Card c) => c.Name));
				array[4] = "}";
				throw new InvalidOperationException(string.Concat(array));
			}
			case CardZone.Draw:
				list = this._drawZone;
				break;
			case CardZone.Hand:
				list = this._handZone;
				break;
			case CardZone.Discard:
				list = this._discardZone;
				break;
			case CardZone.Exile:
				list = this._exileZone;
				break;
			case CardZone.PlayArea:
				list = this._playArea;
				break;
			case CardZone.FollowArea:
				list = this._followArea;
				break;
			default:
				throw new ArgumentOutOfRangeException(string.Format("Cannot move {0} from {1}", card.DebugName, card.Zone));
			}
			list.Remove(card);
			card.Zone = CardZone.None;
			card.OnRemove();
			card.LeaveBattle();
		}
		public int DrawCardCount { get; set; }
		internal void StartPlayerTurn()
		{
			if (this.Player.HasUs && this.Player.Us.BattleRepeatable)
			{
				this.Player.Us.BattleAvailable = true;
				this.Player.Us.TurnAvailable = true;
			}
			this.React(new GainManaAction(this.TurnMana), null, ActionCause.TurnStart);
			if (!this.LockedTurnMana.IsEmpty)
			{
				this.React(new UnlockAllTurnManaAction(), null, ActionCause.TurnStart);
			}
			if (!this.ExtraTurnMana.IsEmpty)
			{
				this.React(new LoseTurnManaAction(this.ExtraTurnMana), null, ActionCause.TurnStart);
			}
			if (this.Player.Dolls.Count > 0)
			{
				this.React(new TriggerAllDollsPassiveAction(), null, ActionCause.TurnEnd);
			}
			int num = this.DrawCardCount;
			if (this.Player.TurnCounter == 1)
			{
				int num2 = Enumerable.Count<Card>(this.DrawZone, (Card card) => card.IsInitial);
				if (this.Player.HasExhibit<ZhinengYinxiang>())
				{
					int? id = this.GameRun.Player.GetExhibit<ZhinengYinxiang>().CardInstanceId;
					if (id != null)
					{
						Card card2 = Enumerable.FirstOrDefault<Card>(this.DrawZone, delegate(Card card)
						{
							int instanceId = card.InstanceId;
							int? id2 = id;
							return (instanceId == id2.GetValueOrDefault()) & (id2 != null);
						});
						if (card2 != null && !card2.IsInitial)
						{
							num2++;
						}
					}
				}
				num = Math.Max(num, num2);
			}
			num = num.Clamp(0, this.MaxHand);
			if (num > 0)
			{
				this.StartTurnDrawing = true;
				this.React(new DrawManyCardAction(num), null, ActionCause.TurnStart);
			}
		}
		internal void EndPlayerTurn()
		{
			this._turnDrawHistory.Clear();
			this._turnDiscardHistory.Clear();
			this._turnExileHistory.Clear();
			this._turnCardUsageHistory.Clear();
			this._turnCardPlayHistory.Clear();
			this._turnCardFollowAttackHistory.Clear();
			this._turnManaGained = ManaGroup.Empty;
			if (!this.BattleShouldEnd)
			{
				this.React(new LoseManaAction(this.BattleMana), null, ActionCause.TurnEnd);
				foreach (Card card in this._handZone)
				{
					if (card.IsEthereal)
					{
						this.React(new ExileCardAction(card), null, ActionCause.TurnEnd);
					}
					else if (!card.Summoned)
					{
						if (card.IsRetain)
						{
							this.React(new RetainAction(card), null, ActionCause.TurnEnd);
						}
						else if (card.IsTempRetain)
						{
							card.IsTempRetain = false;
							this.React(new RetainAction(card), null, ActionCause.TurnEnd);
						}
						else
						{
							this.React(new MoveCardAction(card, CardZone.Discard), null, ActionCause.TurnEnd);
						}
					}
				}
			}
			this.PlayerTurnShouldEnd = true;
		}
		internal void TurnStartDecreaseDuration(Unit target)
		{
			foreach (StatusEffect statusEffect in target.StatusEffects)
			{
				if (statusEffect.HasDuration)
				{
					if (target.IsExtraTurn)
					{
						if (!statusEffect.Config.DurationDecreaseTiming.HasFlag(DurationDecreaseTiming.ExtraTurnStart))
						{
							continue;
						}
					}
					else if (!statusEffect.Config.DurationDecreaseTiming.HasFlag(DurationDecreaseTiming.NormalTurnStart))
					{
						continue;
					}
					if (!statusEffect.IsAutoDecreasing)
					{
						statusEffect.IsAutoDecreasing = true;
					}
					else if (statusEffect.Duration <= 0)
					{
						Debug.LogError("Turn start: found <" + statusEffect.DebugName + ">.Duration = " + statusEffect.Duration.ToString());
					}
					else
					{
						StatusEffect statusEffect2 = statusEffect;
						int num = statusEffect2.Duration - 1;
						statusEffect2.Duration = num;
						statusEffect.NotifyChanged();
						if (statusEffect.Duration == 0)
						{
							this.React(new RemoveStatusEffectAction(statusEffect, true, 0.1f), null, ActionCause.TurnStart);
						}
					}
				}
			}
		}
		internal void TurnEndDecreaseDuration(Unit target)
		{
			foreach (StatusEffect statusEffect in target.StatusEffects)
			{
				if (statusEffect.HasDuration)
				{
					if (target.HasStatusEffect<ExtraTurn>() || (target.HasStatusEffect<SuperExtraTurn>() && target.IsExtraTurn))
					{
						if (!statusEffect.Config.DurationDecreaseTiming.HasFlag(DurationDecreaseTiming.EndTurnForExtra))
						{
							continue;
						}
					}
					else if (!statusEffect.Config.DurationDecreaseTiming.HasFlag(DurationDecreaseTiming.EndTurnForRound))
					{
						continue;
					}
					if (!statusEffect.IsAutoDecreasing)
					{
						statusEffect.IsAutoDecreasing = true;
					}
					else if (statusEffect.Duration <= 0)
					{
						Debug.LogError("Turn end: found <" + statusEffect.DebugName + ">.Duration = " + statusEffect.Duration.ToString());
					}
					else
					{
						StatusEffect statusEffect2 = statusEffect;
						int num = statusEffect2.Duration - 1;
						statusEffect2.Duration = num;
						statusEffect.NotifyChanged();
						if (statusEffect.Duration == 0)
						{
							this.React(new RemoveStatusEffectAction(statusEffect, true, 0.1f), null, ActionCause.TurnStart);
						}
					}
				}
			}
		}
		internal void RoundStartDecreaseDurations()
		{
			foreach (Unit unit in this.AllAliveUnits)
			{
				foreach (StatusEffect statusEffect in unit.StatusEffects)
				{
					if (statusEffect.HasDuration && statusEffect.Config.DurationDecreaseTiming.HasFlag(DurationDecreaseTiming.RoundStart))
					{
						if (!statusEffect.IsAutoDecreasing)
						{
							statusEffect.IsAutoDecreasing = true;
						}
						else if (statusEffect.Duration <= 0)
						{
							Debug.LogError("Round start: found <" + statusEffect.DebugName + ">.Duration = " + statusEffect.Duration.ToString());
						}
						else
						{
							StatusEffect statusEffect2 = statusEffect;
							int num = statusEffect2.Duration - 1;
							statusEffect2.Duration = num;
							statusEffect.NotifyChanged();
							if (statusEffect.Duration == 0)
							{
								this.React(new RemoveStatusEffectAction(statusEffect, true, 0.1f), null, ActionCause.RoundStart);
							}
						}
					}
				}
			}
		}
		internal void RoundEndDecreaseDurations()
		{
			foreach (Unit unit in this.AllAliveUnits)
			{
				foreach (StatusEffect statusEffect in unit.StatusEffects)
				{
					if (statusEffect.HasDuration && statusEffect.Config.DurationDecreaseTiming.HasFlag(DurationDecreaseTiming.RoundEnd))
					{
						if (!statusEffect.IsAutoDecreasing)
						{
							statusEffect.IsAutoDecreasing = true;
						}
						else if (statusEffect.Duration <= 0)
						{
							Debug.LogError("Round end: found <" + statusEffect.DebugName + ">.Duration = " + statusEffect.Duration.ToString());
						}
						else
						{
							StatusEffect statusEffect2 = statusEffect;
							int num = statusEffect2.Duration - 1;
							statusEffect2.Duration = num;
							statusEffect.NotifyChanged();
							if (statusEffect.Duration == 0)
							{
								this.React(new RemoveStatusEffectAction(statusEffect, true, 0.1f), null, ActionCause.RoundEnd);
							}
						}
					}
				}
			}
		}
		internal void StartRound()
		{
			this.RoundStartDecreaseDurations();
			foreach (EnemyUnit enemyUnit in this.EnemyGroup.Alives)
			{
				enemyUnit.UpdateTurnMoves();
			}
		}
		internal void EndRound()
		{
			this.RoundEndDecreaseDurations();
		}
		internal void StartAllEnemyTurn()
		{
			this.RevealHiddenIntentionsByEnemyTurn();
		}
		public void RevealHiddenIntentionsByEnemyTurn()
		{
			this.ShowEnemyIntentionByEnemyTurn = true;
			foreach (EnemyUnit enemyUnit in this.AllAliveEnemies)
			{
				if (Enumerable.Any<Intention>(enemyUnit.Intentions, (Intention i) => i.HiddenFinal))
				{
					foreach (Intention intention in Enumerable.Where<Intention>(enemyUnit.Intentions, (Intention i) => i.HiddenFinal))
					{
						intention.ShowByEnemyTurn = true;
					}
					enemyUnit.NotifyIntentionsChanged();
				}
			}
		}
		public void RevealHiddenIntentions()
		{
			foreach (EnemyUnit enemyUnit in this.AllAliveEnemies)
			{
				enemyUnit.NotifyIntentionsChanged();
			}
		}
		internal void EndAllEnemyTurn()
		{
			this.ShowEnemyIntentionByEnemyTurn = false;
		}
		internal void StartEnemyTurn(EnemyUnit enemy)
		{
		}
		internal void EndEnemyTurn(EnemyUnit enemy)
		{
			enemy.ClearIntentions();
		}
		internal ManaGroup GainMana(ManaGroup group)
		{
			ManaGroup manaGroup = (this.BattleMana + group).ClampComponentMax(99);
			ManaGroup manaGroup2 = manaGroup - this.BattleMana;
			this.BattleMana = manaGroup;
			this._turnManaGained += manaGroup2;
			if (this._turnManaGained.Amount >= 30 && this.GameRun.IsAutoSeed && this.GameRun.JadeBoxes.Empty<JadeBox>())
			{
				this.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.LargeMana);
			}
			return manaGroup2;
		}
		internal ManaGroup LoseMana(ManaGroup group)
		{
			ManaGroup battleMana = this.BattleMana;
			this.BattleMana = (this.BattleMana - group).Corrected;
			return battleMana - this.BattleMana;
		}
		internal ManaGroup LockTurnMana(ManaGroup group)
		{
			ManaGroup manaGroup = this.LockedTurnMana + group;
			manaGroup = ManaGroup.Intersect(manaGroup, this.BaseTurnMana);
			ManaGroup manaGroup2 = manaGroup - this.LockedTurnMana;
			this.LockedTurnMana = manaGroup;
			return manaGroup2;
		}
		internal ManaGroup UnlockTurnMana(ManaGroup group)
		{
			ManaGroup lockedTurnMana = this.LockedTurnMana;
			this.LockedTurnMana = (this.LockedTurnMana - group).Corrected;
			return lockedTurnMana - this.LockedTurnMana;
		}
		internal ManaGroup GainTurnMana(ManaGroup group)
		{
			ManaGroup manaGroup = (this.ExtraTurnMana + group).ClampComponentMax(99);
			ManaGroup manaGroup2 = manaGroup - this.ExtraTurnMana;
			this.ExtraTurnMana = manaGroup;
			return manaGroup2;
		}
		internal ManaGroup LoseTurnMana(ManaGroup group)
		{
			ManaGroup extraTurnMana = this.ExtraTurnMana;
			this.ExtraTurnMana = (this.ExtraTurnMana - group).Corrected;
			return extraTurnMana - this.ExtraTurnMana;
		}
		internal bool ConvertMana(ManaGroup input, ManaGroup output, bool allowPartial, out ManaGroup resultInput, out ManaGroup resultOutput)
		{
			ManaGroup battleMana = this.BattleMana;
			ManaGroup manaGroup = battleMana - input;
			if (manaGroup.IsInvalid)
			{
				if (!allowPartial)
				{
					Debug.LogError(string.Format("[Battle] Cannot convert mana ({0} => {1}): available = {2}", input, output, battleMana));
					resultInput = ManaGroup.Empty;
					resultOutput = ManaGroup.Empty;
					return false;
				}
				manaGroup = manaGroup.Corrected;
			}
			resultInput = battleMana - manaGroup;
			this.BattleMana = (manaGroup + output).ClampComponentMax(99);
			resultOutput = this.BattleMana - manaGroup;
			return true;
		}
		internal void ConsumeMana(ManaGroup group)
		{
			if (!this.BattleMana.CanAfford(group))
			{
				throw new InvalidOperationException(string.Format("Pool ({0}) cannot afford cost {1}", this.BattleMana, group));
			}
			if (group.IsInvalid)
			{
				throw new InvalidOperationException(string.Format("Consuming invalid mana: {0}", group));
			}
			this.BattleMana -= group;
		}
		public int CalculateDamage(GameEntity actionSource, Unit source, Unit target, DamageInfo damageInfo)
		{
			DamageDealingEventArgs damageDealingEventArgs = new DamageDealingEventArgs();
			damageDealingEventArgs.Source = source;
			DamageDealingEventArgs damageDealingEventArgs2 = damageDealingEventArgs;
			object obj;
			if (target == null)
			{
				obj = null;
			}
			else
			{
				(obj = new Unit[1])[0] = target;
			}
			damageDealingEventArgs2.Targets = obj;
			damageDealingEventArgs.DamageInfo = damageInfo;
			damageDealingEventArgs.ActionSource = actionSource;
			damageDealingEventArgs.Cause = ActionCause.OnlyCalculate;
			DamageDealingEventArgs damageDealingEventArgs3 = damageDealingEventArgs;
			source.DamageDealing.Execute(damageDealingEventArgs3);
			damageInfo = damageDealingEventArgs3.DamageInfo;
			if (target != null)
			{
				DamageEventArgs damageEventArgs = new DamageEventArgs
				{
					Source = source,
					Target = target,
					DamageInfo = damageInfo,
					Cause = ActionCause.OnlyCalculate
				};
				target.DamageReceiving.Execute(damageEventArgs);
				damageInfo = damageEventArgs.DamageInfo;
			}
			return Math.Max(0, damageInfo.Damage.RoundToInt(1));
		}
		[return: TupleElementNames(new string[] { "block", "shield" })]
		public ValueTuple<int, int> CalculateBlockShield(GameEntity actionSource, float baseBlock, float baseShield, BlockShieldType type = BlockShieldType.Normal)
		{
			BlockShieldEventArgs blockShieldEventArgs = new BlockShieldEventArgs
			{
				Source = this.Player,
				Target = this.Player,
				Block = baseBlock,
				Shield = baseShield,
				ActionSource = actionSource,
				Type = type,
				HasBlock = (baseBlock > 0f),
				HasShield = (baseShield > 0f),
				Cause = ActionCause.OnlyCalculate
			};
			this.Player.BlockShieldCasting.Execute(blockShieldEventArgs);
			this.Player.BlockShieldGaining.Execute(blockShieldEventArgs);
			return new ValueTuple<int, int>(Math.Max(0, blockShieldEventArgs.Block.RoundToInt(1)), Math.Max(0, blockShieldEventArgs.Shield.RoundToInt(1)));
		}
		public int CalculateScry(GameEntity actionSource, int scry)
		{
			ScryEventArgs scryEventArgs = new ScryEventArgs
			{
				ScryInfo = new ScryInfo(scry),
				ActionSource = actionSource,
				Cause = ActionCause.OnlyCalculate
			};
			this.Scrying.Execute(scryEventArgs);
			return scryEventArgs.ScryInfo.Count;
		}
		public int CalculateDollValue(GameEntity actionSource, int value)
		{
			DollValueArgs dollValueArgs = new DollValueArgs
			{
				ActionSource = actionSource,
				Value = value,
				Cause = ActionCause.OnlyCalculate
			};
			this.Player.DollValueGenerating.Execute(dollValueArgs);
			return Math.Max(0, dollValueArgs.Value);
		}
		internal DamageInfo Damage(Unit source, Unit target, DamageInfo info, GameEntity actionSource)
		{
			DamageInfo damageInfo = target.TakeDamage(info);
			if (!info.DontBreakPerfect && source != this.Player && target == this.Player && damageInfo.Damage > 0f)
			{
				this.Stats.PlayerDamaged = true;
			}
			if (source is PlayerUnit && info.DamageType == DamageType.Attack)
			{
				this.Stats.MaxSingleAttackDamage = Math.Max(this.Stats.MaxSingleAttackDamage, (int)info.Amount);
			}
			return damageInfo;
		}
		internal void StatisticalTotalDamage(Unit damageSource, IReadOnlyDictionary<Unit, IReadOnlyList<DamageEventArgs>> argsTable)
		{
		}
		internal void ForceKill(Unit source, Unit target)
		{
			target.Hp = 0;
			target.Block = 0;
			target.Shield = 0;
			target.Status = UnitStatus.Dying;
		}
		internal int Heal(Unit target, int healValue)
		{
			return target.Heal(healValue);
		}
		[return: TupleElementNames(new string[] { "block", "shield" })]
		internal ValueTuple<int, int> LoseBlockShield(Unit target, float block, float shield)
		{
			return target.LoseBlockShield(block, shield);
		}
		[return: TupleElementNames(new string[] { "block", "shieled" })]
		internal ValueTuple<int, int> CastBlockShield(Unit target, float block, float shield)
		{
			return target.GainBlockShield(block, shield);
		}
		internal StatusEffectAddResult? TryAddStatusEffect(Unit target, StatusEffect effect)
		{
			if (this.BattleShouldEnd)
			{
				Debug.LogWarning(string.Concat(new string[] { "Adding ", effect.DebugName, " to ", target.DebugName, " while battle should end, may cause problems" }));
			}
			effect.GameRun = this.GameRun;
			StatusEffectAddResult? statusEffectAddResult = target.TryAddStatusEffect(effect);
			if (statusEffectAddResult != null)
			{
				this.TriggerGlobalStatusChanged();
				return statusEffectAddResult;
			}
			Debug.LogError(string.Concat(new string[]
			{
				"Adding ",
				effect.GetType().Name,
				" to ",
				target.Name,
				" failed."
			}));
			return default(StatusEffectAddResult?);
		}
		internal bool RemoveStatusEffect(Unit target, StatusEffect effect)
		{
			if (target.TryRemoveStatusEffect(effect))
			{
				this.TriggerGlobalStatusChanged();
				return true;
			}
			Debug.LogError(string.Concat(new string[]
			{
				"Removing ",
				effect.GetType().Name,
				" from ",
				target.Name,
				" failed."
			}));
			return false;
		}
		internal Exhibit[] LoseAllExhibits()
		{
			return this.GameRun.InternalLoseAllExhibits(false);
		}
		internal EnemyUnit Spawn(EnemyUnit spawner, Type type, int rootIndex, bool isServant)
		{
			return this.Spawn(spawner, Library.CreateEnemyUnit(type), rootIndex, isServant);
		}
		private EnemyUnit Spawn(EnemyUnit spawner, EnemyUnit enemyUnit, int rootIndex, bool isServant)
		{
			enemyUnit.EnterGameRun(this.GameRun);
			enemyUnit.RootIndex = rootIndex;
			this.EnemyGroup.Add(enemyUnit);
			enemyUnit.EnterBattle(this);
			if (isServant)
			{
				this.React(new ApplyStatusEffectAction<Servant>(enemyUnit, default(int?), default(int?), default(int?), default(int?), 0f, true), null, ActionCause.None);
			}
			enemyUnit.OnSpawn(spawner);
			return enemyUnit;
		}
		[return: TupleElementNames(new string[] { "power", "bluePoint", "money" })]
		internal ValueTuple<int, int, int> GenerateEnemyPoints(EnemyUnit enemy)
		{
			RandomGen gameRunEventRng = this.GameRun.GameRunEventRng;
			MinMax powerLoot = enemy.Config.PowerLoot;
			int num = gameRunEventRng.NextInt(powerLoot.Min, powerLoot.Max) + gameRunEventRng.NextInt(this.GameRun.ExtraPowerLowerbound, this.GameRun.ExtraPowerUpperbound);
			if (enemy.Config.Type == EnemyType.Boss && this.GameRun.Puzzles.HasFlag(PuzzleFlag.LowStageRegen))
			{
				num = ((float)num * 0.5f).RoundToInt();
			}
			MinMax bluePointLoot = enemy.Config.BluePointLoot;
			int num2 = gameRunEventRng.NextInt(bluePointLoot.Min, bluePointLoot.Max);
			if (enemy.HasStatusEffect<MirrorImage>())
			{
				num = ((float)num * 0.25f).RoundToInt();
				num2 = ((float)num2 * 0.25f).RoundToInt();
			}
			return new ValueTuple<int, int, int>(num, num2, 0);
		}
		internal void Die(Unit unit, bool isSuicide, int power, int bluePoint, int money)
		{
			unit.Die();
			if (unit is PlayerUnit)
			{
				if (isSuicide)
				{
					this.Stats.PlayerSuicide = true;
					return;
				}
			}
			else if (unit is EnemyUnit)
			{
				if (power > 0)
				{
					this.GameRun.InternalGainPower(power);
				}
				if (bluePoint > 0)
				{
					this.Stats.BluePoint += bluePoint;
				}
				if (money > 0)
				{
					this.GameRun.InternalGainMoney(money);
				}
			}
		}
		internal void Escape(EnemyUnit unit)
		{
			unit.Escape();
			unit.Status = UnitStatus.Escaped;
		}
		internal void InstantWin()
		{
			this._forceWin = true;
			this._isWaitingPlayerInput = false;
		}
		internal void RecordCardUsage(Card card)
		{
			this._turnCardUsageHistory.Add(card);
			this._battleCardUsageHistory.Add(card);
			if (this.GameRun.IsAutoSeed && this.GameRun.JadeBoxes.Empty<JadeBox>() && this._turnCardUsageHistory.Count == 30)
			{
				this.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.CardsOneTurn);
			}
		}
		internal void RecordCardPlay(Card card)
		{
			this._turnCardPlayHistory.Add(card);
			this._battleCardPlayHistory.Add(card);
			if (this.GameRun.IsAutoSeed && this.GameRun.JadeBoxes.Empty<JadeBox>() && this._turnCardPlayHistory.Count == 10)
			{
				this.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.FollowPlay);
			}
		}
		internal void RecordCardFollowAttack(IEnumerable<Card> cards)
		{
			Card[] array = (cards as Card[]) ?? Enumerable.ToArray<Card>(cards);
			this._turnCardFollowAttackHistory.AddRange(array);
			this._battleCardFollowAttackHistory.AddRange(array);
		}
		internal bool PlayerTurnShouldEnd { get; set; }
		public bool BattleShouldEnd
		{
			get
			{
				if (!this._forceWin && !this.Player.IsDead)
				{
					return Enumerable.All<EnemyUnit>(this.EnemyGroup, (EnemyUnit e) => e.IsDead || e.IsEscaped || e.IsServant);
				}
				return true;
			}
		}
		private IEnumerator<object> PlayerTurnFlow()
		{
			bool isExtra = false;
			int continuousTurnCount = 0;
			while (!this.BattleShouldEnd)
			{
				this.PlayerTurnShouldEnd = false;
				PlayerUnit player = this.Player;
				int num = player.TurnCounter + 1;
				player.TurnCounter = num;
				num = continuousTurnCount + 1;
				continuousTurnCount = num;
				this.Player.IsExtraTurn = isExtra;
				SuperExtraTurn superExtraTurn = this.Player.GetStatusEffect<SuperExtraTurn>();
				if (isExtra)
				{
					if (superExtraTurn == null)
					{
						ExtraTurn statusEffect = this.Player.GetStatusEffect<ExtraTurn>();
						if (statusEffect.Level == 1)
						{
							yield return this.ResolveAction(new RemoveStatusEffectAction(statusEffect, true, 0.1f), null);
						}
						else
						{
							ExtraTurn extraTurn = statusEffect;
							num = extraTurn.Level - 1;
							extraTurn.Level = num;
						}
					}
					else if (superExtraTurn.Limit == 1)
					{
						superExtraTurn.Status = TurnStatus.ExtraTurnByThis;
						superExtraTurn.Limit = 0;
					}
					else
					{
						superExtraTurn.Status = TurnStatus.ExtraTurn;
						ExtraTurn statusEffect2 = this.Player.GetStatusEffect<ExtraTurn>();
						if (statusEffect2.Level == 1)
						{
							yield return this.ResolveAction(new RemoveStatusEffectAction(statusEffect2, true, 0.1f), null);
						}
						else
						{
							ExtraTurn extraTurn2 = statusEffect2;
							num = extraTurn2.Level - 1;
							extraTurn2.Level = num;
						}
					}
				}
				else if (superExtraTurn != null)
				{
					superExtraTurn.Status = TurnStatus.NaturalTurn;
					superExtraTurn.Limit = 1;
				}
				foreach (ICustomCounter customCounter in this._customCounterTable.Values)
				{
					if (customCounter.AutoResetTiming.HasFlag(CustomCounterResetTiming.PlayerTurnStart))
					{
						customCounter.Reset(this);
					}
				}
				yield return this.ResolveAction(new StartPlayerTurnAction(this.Player, isExtra), null);
				while (!this.PlayerTurnShouldEnd && !this.BattleShouldEnd)
				{
					if (this._playerInputAction != null)
					{
						Debug.LogWarning("[BattleController] Player input action is not null");
						this._playerInputAction = null;
						this._playerInputActionRecordName = null;
					}
					this.IsWaitingPlayerInput = true;
					yield return new WaitUntil(() => !this.IsWaitingPlayerInput || this._debugActionQueue.Count > 0);
					if (this._playerInputAction == null)
					{
						yield return this.ResolveDebugActions();
					}
					else
					{
						BattleAction playerInputAction = this._playerInputAction;
						string playerInputActionRecordName = this._playerInputActionRecordName;
						this._playerInputAction = null;
						this._playerInputActionRecordName = null;
						foreach (ICustomCounter customCounter2 in this._customCounterTable.Values)
						{
							if (customCounter2.AutoResetTiming.HasFlag(CustomCounterResetTiming.PlayerActionStart))
							{
								customCounter2.Reset(this);
							}
						}
						yield return this.ResolveAction(playerInputAction, playerInputActionRecordName);
						foreach (ICustomCounter customCounter3 in this._customCounterTable.Values)
						{
							if (customCounter3.AutoResetTiming.HasFlag(CustomCounterResetTiming.PlayerActionEnd))
							{
								customCounter3.Reset(this);
							}
						}
					}
				}
				if (this.IsWaitingPlayerInput)
				{
					this.IsWaitingPlayerInput = false;
					Debug.LogWarning("IsWaitingPlayerInput is still true");
				}
				if (this._playerInputAction != null)
				{
					Debug.LogWarning("_playerInputAction is still not null: " + this._playerInputAction.GetType().Name);
					this._playerInputAction = null;
					this._playerInputActionRecordName = null;
				}
				foreach (ICustomCounter customCounter4 in this._customCounterTable.Values)
				{
					if (customCounter4.AutoResetTiming.HasFlag(CustomCounterResetTiming.PlayerTurnEnd))
					{
						customCounter4.Reset(this);
					}
				}
				yield return this.ResolveAction(new EndPlayerTurnAction(this.Player), null);
				superExtraTurn = this.Player.GetStatusEffect<SuperExtraTurn>();
				if (superExtraTurn == null)
				{
					if (!this.Player.HasStatusEffect<ExtraTurn>())
					{
						break;
					}
				}
				else if (!this.Player.HasStatusEffect<ExtraTurn>() && superExtraTurn.Limit == 0)
				{
					superExtraTurn.Status = TurnStatus.OutTurn;
					break;
				}
				isExtra = true;
			}
			if (continuousTurnCount > this.Stats.ContinuousTurnCount)
			{
				this.Stats.ContinuousTurnCount = continuousTurnCount;
			}
			yield break;
		}
		private IEnumerator<object> EnemyTurnFlow()
		{
			if (!this.BattleShouldEnd)
			{
				yield return this.ResolveAction(new StartAllEnemyTurnAction(), null);
			}
			EnemyUnit[] array = Enumerable.ToArray<EnemyUnit>(Enumerable.ThenBy<EnemyUnit, int>(Enumerable.OrderBy<EnemyUnit, int>(this.EnemyGroup.Alives, (EnemyUnit enemy) => enemy.MovePriority), (EnemyUnit enemy) => enemy.RootIndex));
			foreach (EnemyUnit enemy2 in array)
			{
				bool flag = false;
				while (!this.BattleShouldEnd && !enemy2.IsDead)
				{
					EnemyUnit enemyUnit = enemy2;
					int num = enemyUnit.TurnCounter + 1;
					enemyUnit.TurnCounter = num;
					enemy2.IsExtraTurn = flag;
					if (flag)
					{
						ExtraTurn statusEffect = enemy2.GetStatusEffect<ExtraTurn>();
						if (statusEffect.Level == 1)
						{
							yield return this.ResolveAction(new RemoveStatusEffectAction(statusEffect, true, 0.1f), null);
						}
						else
						{
							ExtraTurn extraTurn = statusEffect;
							num = extraTurn.Level - 1;
							extraTurn.Level = num;
						}
						enemy2.UpdateTurnMoves();
					}
					yield return this.ResolveAction(new EnemyTurnAction(enemy2), "EnemyTurn: " + enemy2.Name);
					if (!enemy2.HasStatusEffect<ExtraTurn>())
					{
						break;
					}
					flag = true;
				}
				enemy2 = null;
			}
			EnemyUnit[] array2 = null;
			if (!this.BattleShouldEnd)
			{
				yield return this.ResolveAction(new EndAllEnemyTurnAction(), null);
			}
			yield break;
		}
		public IEnumerator<object> Flow()
		{
			yield return this.ResolveAction(new StartBattleAction(), null);
			while (!this.BattleShouldEnd)
			{
				int num = this.RoundCounter + 1;
				this.RoundCounter = num;
				yield return this.ResolveAction(new StartRoundAction(), null);
				if (!this.BattleShouldEnd)
				{
					yield return this.PlayerTurnFlow();
				}
				if (!this.BattleShouldEnd)
				{
					yield return this.EnemyTurnFlow();
				}
				if (!this.BattleShouldEnd)
				{
					yield return this.ResolveAction(new EndRoundAction(), null);
				}
			}
			if (!this._forceWin && this.Player.IsAlive)
			{
				List<Unit> list = new List<Unit>();
				foreach (EnemyUnit enemyUnit in this.EnemyGroup.Alives)
				{
					if (!enemyUnit.IsServant)
					{
						Debug.LogError(enemyUnit.DebugName + " is not servant but still alive while battle end");
					}
					enemyUnit.Status = UnitStatus.Dying;
					list.Add(enemyUnit);
				}
				if (!list.Empty<Unit>())
				{
					yield return this.ResolveAction(new DieAction(list.Zip(Enumerable.Repeat<DieCause>(DieCause.ServantDie, list.Count)), this.Player, null), null);
				}
			}
			yield return this.ResolveAction(new EndBattleAction(), null);
			yield break;
		}
		public EnemyUnit RandomAliveEnemy
		{
			get
			{
				return this.EnemyGroup.Alives.SampleOrDefault(this.GameRun.BattleRng);
			}
		}
		public EnemyUnit FirstAliveEnemy
		{
			get
			{
				return this.EnemyGroup.Alives.MinByOrDefault((EnemyUnit unit) => unit.RootIndex);
			}
		}
		public EnemyUnit LastAliveEnemy
		{
			get
			{
				return this.EnemyGroup.Alives.MaxByOrDefault((EnemyUnit unit) => unit.RootIndex);
			}
		}
		public EnemyUnit LowestHpEnemy
		{
			get
			{
				return this.EnemyGroup.Alives.MinByOrDefault((EnemyUnit unit) => unit.Hp);
			}
		}
		public EnemyUnit HighestHpEnemy
		{
			get
			{
				return this.EnemyGroup.Alives.MaxByOrDefault((EnemyUnit unit) => unit.Hp);
			}
		}
		public IEnumerable<Unit> AllAliveUnits
		{
			get
			{
				if (this.Player.IsAlive)
				{
					yield return this.Player;
				}
				foreach (EnemyUnit enemyUnit in this.EnemyGroup)
				{
					if (enemyUnit.IsAlive)
					{
						yield return enemyUnit;
					}
				}
				IEnumerator<EnemyUnit> enumerator = null;
				yield break;
				yield break;
			}
		}
		public IEnumerable<EnemyUnit> AllAliveEnemies
		{
			get
			{
				return this.EnemyGroup.Alives;
			}
		}
		public EnemyUnit GetEnemyByRootIndex(int rootIndex)
		{
			return Enumerable.FirstOrDefault<EnemyUnit>(this.AllAliveEnemies, (EnemyUnit enemy) => enemy.RootIndex == rootIndex);
		}
		public bool IsAnyoneInRootIndex(int rootIndex)
		{
			return Enumerable.Any<EnemyUnit>(this.AllAliveEnemies, (EnemyUnit enemy) => enemy.RootIndex == rootIndex);
		}
		public Card[] RollCards(CardWeightTable weightTable, int count, [MaybeNull] Predicate<CardConfig> filter = null)
		{
			return this.GameRun.RollCards(this.GameRun.BattleCardRng, weightTable, count, false, true, filter);
		}
		public Card RollCard(CardWeightTable weightTable, [MaybeNull] Predicate<CardConfig> filter = null)
		{
			return Enumerable.FirstOrDefault<Card>(this.RollCards(weightTable, 1, filter));
		}
		public Card[] RollCardsWithoutManaLimit(CardWeightTable weightTable, int count, [MaybeNull] Predicate<CardConfig> filter = null)
		{
			return this.GameRun.RollCardsWithoutManaLimit(this.GameRun.BattleCardRng, weightTable, count, false, true, filter);
		}
		public bool CanAffordUs()
		{
			return this.Player.Power >= this.Player.Us.PowerCost;
		}
		public bool CanUseUs()
		{
			return this.Player.HasUs && this.Player.Us.Available && this.CanAffordUs();
		}
		public bool DoesPlayerStatusEffectForbidUse(Card card, out StatusEffect effect)
		{
			foreach (StatusEffect statusEffect in this.Player.StatusEffects)
			{
				if (statusEffect.ShouldPreventCardUsage(card))
				{
					effect = statusEffect;
					return true;
				}
			}
			effect = null;
			return false;
		}
		public bool DoesHandCardPreventUse(Card targetCard, out Card card)
		{
			foreach (Card card2 in this.HandZone)
			{
				if (card2 != targetCard && card2.ShouldPreventOtherCardUsage(targetCard))
				{
					card = card2;
					return true;
				}
			}
			card = null;
			return false;
		}
		public bool IsWaitingPlayerInput
		{
			get
			{
				return this._isWaitingPlayerInput;
			}
			private set
			{
				this._isWaitingPlayerInput = value;
				if (value)
				{
					this.TriggerWaitingPlayerInput();
				}
			}
		}
		public void RequestUseCard(Card card, UnitSelector selector, ManaGroup mana, bool kicker)
		{
			if (!this.IsWaitingPlayerInput)
			{
				throw new InvalidOperationException("Cannot play card: not waiting for player input");
			}
			Card card2;
			if (this.DoesHandCardPreventUse(card, out card2))
			{
				throw new InvalidOperationException("Cannot play card: prevented by " + card2.DebugName);
			}
			StatusEffect statusEffect;
			if (this.DoesPlayerStatusEffectForbidUse(card, out statusEffect))
			{
				throw new InvalidOperationException("Cannot play card: prevented by " + statusEffect.DebugName);
			}
			if (card.IsForbidden)
			{
				throw new InvalidOperationException("Cannot play forbidden card " + card.Name);
			}
			if (!card.CanUse)
			{
				throw new InvalidOperationException("Cannot play card " + card.Name + " (CanUse == false)");
			}
			if (mana.IsInvalid)
			{
				throw new InvalidOperationException(string.Format("UseCard with invalid '{0}': {1}", "mana", mana));
			}
			int? moneyCost = card.Config.MoneyCost;
			if (moneyCost != null)
			{
				int valueOrDefault = moneyCost.GetValueOrDefault();
				if (this.GameRun.Money < valueOrDefault)
				{
					throw new InvalidOperationException(string.Format("Cannot afford card money cost (consuming: {0}, available: {1})", valueOrDefault, this.GameRun.Money));
				}
			}
			if (!this.BattleMana.CanAfford(mana))
			{
				throw new InvalidOperationException(string.Format("Cannot afford card mana cost (consuming: {0}, available: {1})", mana, this.BattleMana));
			}
			if (card.IsXCost && !mana.CanAfford(card.XCostRequiredMana))
			{
				throw new InvalidOperationException(string.Format("Cannot afford X cost required mana {0} with payment {1}", card.XCostRequiredMana, mana));
			}
			this._playerInputAction = new UseCardAction(card, selector, mana, kicker).SetCause(ActionCause.Player);
			this._playerInputActionRecordName = "UseCard: " + card.Name;
			this._isWaitingPlayerInput = false;
		}
		public void RequestUseUs(UnitSelector selector)
		{
			if (!this.IsWaitingPlayerInput)
			{
				throw new InvalidOperationException("Cannot use US: not waiting for player input");
			}
			if (!this.Player.HasUs)
			{
				throw new InvalidOperationException("Player doesn't has US");
			}
			UltimateSkill us = this.Player.Us;
			if (!us.Available)
			{
				throw new InvalidOperationException("US is not available");
			}
			if (this.Player.Power < us.PowerCost)
			{
				throw new InvalidOperationException(string.Format("Cannot afford US power {0}, current: {1}", us.PowerCost, this.Player.Power));
			}
			this._playerInputAction = new UseUsAction(us, selector, us.PowerCost).SetCause(ActionCause.Player);
			this._playerInputActionRecordName = "UseUs: " + us.DebugName;
			this._isWaitingPlayerInput = false;
		}
		public void RequestUseDoll(Doll doll, UnitSelector selector)
		{
			if (!this.IsWaitingPlayerInput)
			{
				throw new InvalidOperationException("Cannot use US: not waiting for player input");
			}
			if (!this.Player.HasDoll(doll))
			{
				throw new InvalidOperationException("Player doesn't has Doll:" + doll.DebugName);
			}
			if (!doll.Usable)
			{
				throw new InvalidOperationException("Doll is not usable:" + doll.DebugName);
			}
			if (doll.HasMagic && doll.Magic < doll.MagicCost)
			{
				throw new InvalidOperationException(string.Format("Cannot afford Doll magic cost {0}, current: {1}", doll.MagicCost, doll.Magic));
			}
			this._playerInputAction = new UseDollAction(doll, selector).SetCause(ActionCause.Player);
			this._playerInputActionRecordName = "UseDoll: " + doll.DebugName;
			this._isWaitingPlayerInput = false;
		}
		public void RequestEndPlayerTurn()
		{
			if (!this.IsWaitingPlayerInput)
			{
				throw new InvalidOperationException("Cannot end player turn: not waiting for player input");
			}
			this._playerInputAction = new RequestEndPlayerTurnAction();
			this._isWaitingPlayerInput = false;
		}
		public void RequestDebugAction(BattleAction action, string recordName)
		{
			this._debugActionQueue.Enqueue(new ValueTuple<BattleAction, string>(action, recordName));
		}
		internal void React(Reactor reactor, [MaybeNull] GameEntity source, ActionCause cause)
		{
			this._resolver.React(reactor, source, cause);
		}
		public void IncreaseCounter<T>() where T : ICustomCounter, new()
		{
			ICustomCounter customCounter;
			if (!this._customCounterTable.TryGetValue(typeof(T), ref customCounter))
			{
				customCounter = new T();
				this._customCounterTable.Add(typeof(T), customCounter);
			}
			customCounter.Increase(this);
		}
		public GameEvent<GameEventArgs> BattleStarting { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<GameEventArgs> BattleStarted { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<GameEventArgs> BattleEnding { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<GameEventArgs> BattleEnded { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<GameEventArgs> RoundStarting { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<GameEventArgs> RoundStarted { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<GameEventArgs> RoundEnding { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<GameEventArgs> RoundEnded { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<GameEventArgs> AllEnemyTurnStarting { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<GameEventArgs> AllEnemyTurnStarted { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<GameEventArgs> AllEnemyTurnEnding { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<GameEventArgs> AllEnemyTurnEnded { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<CardEventArgs> Predraw { get; } = new GameEvent<CardEventArgs>();
		public GameEvent<CardEventArgs> CardDrawing { get; } = new GameEvent<CardEventArgs>();
		public GameEvent<CardEventArgs> CardDrawn { get; } = new GameEvent<CardEventArgs>();
		public GameEvent<CardEventArgs> CardDiscarding { get; } = new GameEvent<CardEventArgs>();
		public GameEvent<CardEventArgs> CardDiscarded { get; } = new GameEvent<CardEventArgs>();
		public GameEvent<CardUsingEventArgs> CardUsing { get; } = new GameEvent<CardUsingEventArgs>();
		public GameEvent<CardUsingEventArgs> CardUsed { get; } = new GameEvent<CardUsingEventArgs>();
		public GameEvent<CardUsingEventArgs> CardUsingCanceled { get; } = new GameEvent<CardUsingEventArgs>();
		public GameEvent<CardUsingEventArgs> CardPlaying { get; } = new GameEvent<CardUsingEventArgs>();
		public GameEvent<CardUsingEventArgs> CardPlayed { get; } = new GameEvent<CardUsingEventArgs>();
		public GameEvent<FollowAttackEventArgs> FollowAttacking { get; } = new GameEvent<FollowAttackEventArgs>();
		public GameEvent<FollowAttackEventArgs> FollowAttacked { get; } = new GameEvent<FollowAttackEventArgs>();
		public GameEvent<CardEventArgs> CardExiling { get; } = new GameEvent<CardEventArgs>();
		public GameEvent<CardEventArgs> CardExiled { get; } = new GameEvent<CardEventArgs>();
		public GameEvent<CardTransformEventArgs> CardTransforming { get; } = new GameEvent<CardTransformEventArgs>();
		public GameEvent<CardTransformEventArgs> CardTransformed { get; } = new GameEvent<CardTransformEventArgs>();
		public GameEvent<CardEventArgs> CardRetaining { get; } = new GameEvent<CardEventArgs>();
		public GameEvent<CardEventArgs> CardRetained { get; } = new GameEvent<CardEventArgs>();
		public GameEvent<CardEventArgs> CardRemoving { get; } = new GameEvent<CardEventArgs>();
		public GameEvent<CardEventArgs> CardRemoved { get; } = new GameEvent<CardEventArgs>();
		public GameEvent<GameEventArgs> Reshuffling { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<GameEventArgs> Reshuffled { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<CardsAddingToDrawZoneEventArgs> CardsAddingToDrawZone { get; } = new GameEvent<CardsAddingToDrawZoneEventArgs>();
		public GameEvent<CardsAddingToDrawZoneEventArgs> CardsAddedToDrawZone { get; } = new GameEvent<CardsAddingToDrawZoneEventArgs>();
		public GameEvent<CardsEventArgs> CardsAddingToHand { get; } = new GameEvent<CardsEventArgs>();
		public GameEvent<CardsEventArgs> CardsAddedToHand { get; } = new GameEvent<CardsEventArgs>();
		public GameEvent<CardsEventArgs> CardsAddingToDiscard { get; } = new GameEvent<CardsEventArgs>();
		public GameEvent<CardsEventArgs> CardsAddedToDiscard { get; } = new GameEvent<CardsEventArgs>();
		public GameEvent<CardsEventArgs> CardsAddingToExile { get; } = new GameEvent<CardsEventArgs>();
		public GameEvent<CardsEventArgs> CardsAddedToExile { get; } = new GameEvent<CardsEventArgs>();
		public GameEvent<CardMovingEventArgs> CardMoving { get; } = new GameEvent<CardMovingEventArgs>();
		public GameEvent<CardMovingEventArgs> CardMoved { get; } = new GameEvent<CardMovingEventArgs>();
		public GameEvent<CardMovingToDrawZoneEventArgs> CardMovingToDrawZone { get; } = new GameEvent<CardMovingToDrawZoneEventArgs>();
		public GameEvent<CardMovingToDrawZoneEventArgs> CardMovedToDrawZone { get; } = new GameEvent<CardMovingToDrawZoneEventArgs>();
		public GameEvent<UsUsingEventArgs> UsUsing { get; } = new GameEvent<UsUsingEventArgs>();
		public GameEvent<UsUsingEventArgs> UsUsed { get; } = new GameEvent<UsUsingEventArgs>();
		public GameEvent<GameEventArgs> UsUsingCanceled { get; } = new GameEvent<GameEventArgs>();
		public GameEvent<DollUsingEventArgs> DollUsing { get; } = new GameEvent<DollUsingEventArgs>();
		public GameEvent<DollUsingEventArgs> DollUsed { get; } = new GameEvent<DollUsingEventArgs>();
		public GameEvent<ManaEventArgs> TurnManaLocking { get; } = new GameEvent<ManaEventArgs>();
		public GameEvent<ManaEventArgs> TurnManaLocked { get; } = new GameEvent<ManaEventArgs>();
		public GameEvent<ManaEventArgs> TurnManaUnlocking { get; } = new GameEvent<ManaEventArgs>();
		public GameEvent<ManaEventArgs> TurnManaUnlocked { get; } = new GameEvent<ManaEventArgs>();
		public GameEvent<ManaEventArgs> TurnManaGaining { get; } = new GameEvent<ManaEventArgs>();
		public GameEvent<ManaEventArgs> TurnManaGained { get; } = new GameEvent<ManaEventArgs>();
		public GameEvent<ManaEventArgs> TurnManaLosing { get; } = new GameEvent<ManaEventArgs>();
		public GameEvent<ManaEventArgs> TurnManaLost { get; } = new GameEvent<ManaEventArgs>();
		public GameEvent<ManaEventArgs> ManaGaining { get; } = new GameEvent<ManaEventArgs>();
		public GameEvent<ManaEventArgs> ManaGained { get; } = new GameEvent<ManaEventArgs>();
		public GameEvent<ManaEventArgs> ManaConsuming { get; } = new GameEvent<ManaEventArgs>();
		public GameEvent<ManaEventArgs> ManaConsumed { get; } = new GameEvent<ManaEventArgs>();
		public GameEvent<ManaEventArgs> ManaLosing { get; } = new GameEvent<ManaEventArgs>();
		public GameEvent<ManaEventArgs> ManaLost { get; } = new GameEvent<ManaEventArgs>();
		public GameEvent<ManaConvertingEventArgs> ManaConverting { get; } = new GameEvent<ManaConvertingEventArgs>();
		public GameEvent<ManaConvertingEventArgs> ManaConverted { get; } = new GameEvent<ManaConvertingEventArgs>();
		public GameEvent<DollEventArgs> DollAdding { get; } = new GameEvent<DollEventArgs>();
		public GameEvent<DollEventArgs> DollAdded { get; } = new GameEvent<DollEventArgs>();
		public GameEvent<DollEventArgs> DollRemoving { get; } = new GameEvent<DollEventArgs>();
		public GameEvent<DollEventArgs> DollRemoved { get; } = new GameEvent<DollEventArgs>();
		public GameEvent<DollTriggeredEventArgs> DollTriggeredActive { get; } = new GameEvent<DollTriggeredEventArgs>();
		public GameEvent<DollTriggeredEventArgs> DollTriggeredPassive { get; } = new GameEvent<DollTriggeredEventArgs>();
		public GameEvent<UnitEventArgs> EnemySpawning { get; } = new GameEvent<UnitEventArgs>();
		public GameEvent<UnitEventArgs> EnemySpawned { get; } = new GameEvent<UnitEventArgs>();
		public GameEvent<DieEventArgs> EnemyPointGenerating { get; } = new GameEvent<DieEventArgs>();
		public GameEvent<DieEventArgs> EnemyDied { get; } = new GameEvent<DieEventArgs>();
		public GameEvent<UnitEventArgs> EnemyEscaped { get; } = new GameEvent<UnitEventArgs>();
		public GameEvent<ScryEventArgs> Scrying { get; } = new GameEvent<ScryEventArgs>();
		public GameEvent<ScryEventArgs> Scried { get; } = new GameEvent<ScryEventArgs>();
		public event Action GlobalStatusChanged;
		internal void TriggerGlobalStatusChanged()
		{
			Action globalStatusChanged = this.GlobalStatusChanged;
			if (globalStatusChanged == null)
			{
				return;
			}
			globalStatusChanged.Invoke();
		}
		public event Action WaitingPlayerInput;
		internal void TriggerWaitingPlayerInput()
		{
			Action waitingPlayerInput = this.WaitingPlayerInput;
			if (waitingPlayerInput == null)
			{
				return;
			}
			waitingPlayerInput.Invoke();
		}
		public event Action<BattleMessage> Notification;
		public bool HandIsNotFull
		{
			get
			{
				return !this.HandIsFull;
			}
		}
		public bool HandIsFull
		{
			get
			{
				if (this.HandZone.Count == this.MaxHand)
				{
					return Enumerable.All<Card>(this.HandZone, (Card card) => !card.IsAutoExile);
				}
				return false;
			}
		}
		public int CardsToFull
		{
			get
			{
				return this.MaxHand - Enumerable.Count<Card>(this.HandZone, (Card card) => !card.IsAutoExile);
			}
		}
		public int MaxHand { get; set; } = 10;
		public bool StartTurnDrawing { get; set; }
		public int DrawAfterDiscard { get; set; }
		public int CardExtraGrowAmount { get; set; }
		public int ManaFreezeLevel { get; set; }
		public int PlayedCardInManaFreezeLevel { get; set; }
		public void CheckManaFreeze()
		{
			this.ManaFreezeLevel = 0;
			List<ManaFreezer> list = new List<ManaFreezer>();
			foreach (Card card in this.HandZone)
			{
				ManaFreezer manaFreezer = card as ManaFreezer;
				if (manaFreezer != null)
				{
					list.Add(manaFreezer);
				}
			}
			if (list.Count > 0)
			{
				foreach (ManaFreezer manaFreezer2 in list)
				{
					this.ManaFreezeLevel = Math.Max(manaFreezer2.FreezeLevel, this.ManaFreezeLevel);
				}
			}
		}
		public int FriendPassiveTimes { get; set; } = 1;
		public bool PlayerSummonAFriendThisTurn { get; set; }
		public int HideEnemyIntentionLevel { get; set; }
		public bool ShowEnemyIntentionByEnemyTurn { get; set; }
		public bool HideEnemyIntention
		{
			get
			{
				return this.HideEnemyIntentionLevel > 0 && !this.ShowEnemyIntentionByEnemyTurn;
			}
		}
		public int FollowAttackFillerLevel { get; set; }
		public int LimaoSchoolFrogTimes { get; set; }
		public int ReimuHuashanTimes { get; set; }
		private readonly WeakReference<GameRunController> _gameRun = new WeakReference<GameRunController>(null);
		private readonly List<Card> _drawZone = new List<Card>();
		private readonly List<Card> _handZone = new List<Card>();
		private readonly List<Card> _playArea = new List<Card>();
		private readonly List<Card> _discardZone = new List<Card>();
		private readonly List<Card> _exileZone = new List<Card>();
		private readonly List<Card> _followArea = new List<Card>();
		private readonly ActionResolver _resolver;
		[TupleElementNames(new string[] { "action", "name" })]
		private readonly Queue<ValueTuple<BattleAction, string>> _debugActionQueue = new Queue<ValueTuple<BattleAction, string>>();
		private readonly List<Card> _turnDrawHistory = new List<Card>();
		private readonly List<Card> _turnDiscardHistory = new List<Card>();
		private readonly List<Card> _turnExileHistory = new List<Card>();
		private readonly List<Card> _turnCardUsageHistory = new List<Card>();
		private readonly List<Card> _battleCardUsageHistory = new List<Card>();
		private readonly List<Card> _turnCardPlayHistory = new List<Card>();
		private readonly List<Card> _battleCardPlayHistory = new List<Card>();
		private readonly List<Card> _turnCardFollowAttackHistory = new List<Card>();
		private readonly List<Card> _battleCardFollowAttackHistory = new List<Card>();
		private int _cardInstanceId = 1000;
		private ManaGroup _turnManaGained;
		private bool _forceWin;
		private bool _isWaitingPlayerInput;
		private BattleAction _playerInputAction;
		private string _playerInputActionRecordName;
		private readonly Dictionary<Type, ICustomCounter> _customCounterTable = new Dictionary<Type, ICustomCounter>();
	}
}
