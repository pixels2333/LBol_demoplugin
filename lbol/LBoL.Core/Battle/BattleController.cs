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
	// Token: 0x0200013F RID: 319
	public class BattleController
	{
		// Token: 0x06000C12 RID: 3090 RVA: 0x0002178F File Offset: 0x0001F98F
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

		// Token: 0x06000C13 RID: 3091 RVA: 0x0002179E File Offset: 0x0001F99E
		private IEnumerator<object> ResolveAction(BattleAction battleAction, [MaybeNull] string recordName = null)
		{
			yield return this.ResolveDebugActions();
			yield return this._resolver.Resolve(battleAction, recordName);
			yield break;
		}

		// Token: 0x17000408 RID: 1032
		// (get) Token: 0x06000C14 RID: 3092 RVA: 0x000217BB File Offset: 0x0001F9BB
		public BattleStats Stats { get; } = new BattleStats();

		// Token: 0x17000409 RID: 1033
		// (get) Token: 0x06000C15 RID: 3093 RVA: 0x000217C4 File Offset: 0x0001F9C4
		// (set) Token: 0x06000C16 RID: 3094 RVA: 0x000217E3 File Offset: 0x0001F9E3
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

		// Token: 0x1700040A RID: 1034
		// (get) Token: 0x06000C17 RID: 3095 RVA: 0x000217F1 File Offset: 0x0001F9F1
		public ActionViewer ActionViewer { get; } = new ActionViewer();

		// Token: 0x1700040B RID: 1035
		// (get) Token: 0x06000C18 RID: 3096 RVA: 0x000217F9 File Offset: 0x0001F9F9
		public PlayerUnit Player { get; }

		// Token: 0x1700040C RID: 1036
		// (get) Token: 0x06000C19 RID: 3097 RVA: 0x00021801 File Offset: 0x0001FA01
		public EnemyGroup EnemyGroup { get; }

		// Token: 0x1700040D RID: 1037
		// (get) Token: 0x06000C1A RID: 3098 RVA: 0x00021809 File Offset: 0x0001FA09
		public ManaGroup BaseTurnMana
		{
			get
			{
				return this.GameRun.BaseMana;
			}
		}

		// Token: 0x1700040E RID: 1038
		// (get) Token: 0x06000C1B RID: 3099 RVA: 0x00021816 File Offset: 0x0001FA16
		// (set) Token: 0x06000C1C RID: 3100 RVA: 0x0002181E File Offset: 0x0001FA1E
		public ManaGroup ExtraTurnMana { get; private set; }

		// Token: 0x1700040F RID: 1039
		// (get) Token: 0x06000C1D RID: 3101 RVA: 0x00021827 File Offset: 0x0001FA27
		public ManaGroup TurnMana
		{
			get
			{
				return this.BaseTurnMana - this.LockedTurnMana + this.ExtraTurnMana;
			}
		}

		// Token: 0x17000410 RID: 1040
		// (get) Token: 0x06000C1E RID: 3102 RVA: 0x00021845 File Offset: 0x0001FA45
		// (set) Token: 0x06000C1F RID: 3103 RVA: 0x0002184D File Offset: 0x0001FA4D
		public ManaGroup LockedTurnMana { get; private set; }

		// Token: 0x17000411 RID: 1041
		// (get) Token: 0x06000C20 RID: 3104 RVA: 0x00021856 File Offset: 0x0001FA56
		// (set) Token: 0x06000C21 RID: 3105 RVA: 0x0002185E File Offset: 0x0001FA5E
		public ManaGroup BattleMana { get; private set; }

		// Token: 0x17000412 RID: 1042
		// (get) Token: 0x06000C22 RID: 3106 RVA: 0x00021867 File Offset: 0x0001FA67
		public IReadOnlyList<Card> DrawZone
		{
			get
			{
				return this._drawZone.AsReadOnly();
			}
		}

		// Token: 0x17000413 RID: 1043
		// (get) Token: 0x06000C23 RID: 3107 RVA: 0x00021874 File Offset: 0x0001FA74
		public IReadOnlyList<Card> DrawZoneIndexOrder
		{
			get
			{
				return Enumerable.ToList<Card>(Enumerable.OrderBy<Card, int>(this._drawZone, (Card card) => card.Config.Index)).AsReadOnly();
			}
		}

		// Token: 0x17000414 RID: 1044
		// (get) Token: 0x06000C24 RID: 3108 RVA: 0x000218AA File Offset: 0x0001FAAA
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

		// Token: 0x17000415 RID: 1045
		// (get) Token: 0x06000C25 RID: 3109 RVA: 0x000218C7 File Offset: 0x0001FAC7
		public IReadOnlyList<Card> HandZone
		{
			get
			{
				return this._handZone.AsReadOnly();
			}
		}

		// Token: 0x17000416 RID: 1046
		// (get) Token: 0x06000C26 RID: 3110 RVA: 0x000218D4 File Offset: 0x0001FAD4
		public IReadOnlyList<Card> PlayArea
		{
			get
			{
				return this._playArea.AsReadOnly();
			}
		}

		// Token: 0x17000417 RID: 1047
		// (get) Token: 0x06000C27 RID: 3111 RVA: 0x000218E1 File Offset: 0x0001FAE1
		public IReadOnlyList<Card> FollowArea
		{
			get
			{
				return this._followArea.AsReadOnly();
			}
		}

		// Token: 0x17000418 RID: 1048
		// (get) Token: 0x06000C28 RID: 3112 RVA: 0x000218EE File Offset: 0x0001FAEE
		public IReadOnlyList<Card> HandZoneAndPlayArea
		{
			get
			{
				return Enumerable.ToList<Card>(Enumerable.Concat<Card>(this._handZone, this._playArea)).AsReadOnly();
			}
		}

		// Token: 0x17000419 RID: 1049
		// (get) Token: 0x06000C29 RID: 3113 RVA: 0x0002190B File Offset: 0x0001FB0B
		public IReadOnlyList<Card> DiscardZone
		{
			get
			{
				return this._discardZone.AsReadOnly();
			}
		}

		// Token: 0x1700041A RID: 1050
		// (get) Token: 0x06000C2A RID: 3114 RVA: 0x00021918 File Offset: 0x0001FB18
		public IReadOnlyList<Card> ExileZone
		{
			get
			{
				return this._exileZone.AsReadOnly();
			}
		}

		// Token: 0x06000C2B RID: 3115 RVA: 0x00021925 File Offset: 0x0001FB25
		public IEnumerable<Card> EnumerateAllCards()
		{
			return Enumerable.Concat<Card>(Enumerable.Concat<Card>(Enumerable.Concat<Card>(Enumerable.Concat<Card>(Enumerable.Concat<Card>(this._handZone, this._playArea), this._followArea), this._drawZone), this._discardZone), this._exileZone);
		}

		// Token: 0x06000C2C RID: 3116 RVA: 0x00021964 File Offset: 0x0001FB64
		public IEnumerable<Card> EnumerateAllCardsButExile()
		{
			return Enumerable.Concat<Card>(Enumerable.Concat<Card>(Enumerable.Concat<Card>(Enumerable.Concat<Card>(this._handZone, this._playArea), this._followArea), this._drawZone), this._discardZone);
		}

		// Token: 0x06000C2D RID: 3117 RVA: 0x00021998 File Offset: 0x0001FB98
		public IEnumerable<Card> EnumerateAllCardsButPlayingAreas()
		{
			return Enumerable.Concat<Card>(Enumerable.Concat<Card>(Enumerable.Concat<Card>(this._handZone, this._drawZone), this._discardZone), this._exileZone);
		}

		// Token: 0x1700041B RID: 1051
		// (get) Token: 0x06000C2E RID: 3118 RVA: 0x000219C1 File Offset: 0x0001FBC1
		public IReadOnlyList<Card> TurnDrawHistory
		{
			get
			{
				return this._turnDrawHistory.AsReadOnly();
			}
		}

		// Token: 0x1700041C RID: 1052
		// (get) Token: 0x06000C2F RID: 3119 RVA: 0x000219CE File Offset: 0x0001FBCE
		public IReadOnlyList<Card> TurnDiscardHistory
		{
			get
			{
				return this._turnDiscardHistory.AsReadOnly();
			}
		}

		// Token: 0x1700041D RID: 1053
		// (get) Token: 0x06000C30 RID: 3120 RVA: 0x000219DB File Offset: 0x0001FBDB
		public IReadOnlyList<Card> TurnExileHistory
		{
			get
			{
				return this._turnExileHistory.AsReadOnly();
			}
		}

		// Token: 0x1700041E RID: 1054
		// (get) Token: 0x06000C31 RID: 3121 RVA: 0x000219E8 File Offset: 0x0001FBE8
		public IReadOnlyList<Card> TurnCardUsageHistory
		{
			get
			{
				return this._turnCardUsageHistory.AsReadOnly();
			}
		}

		// Token: 0x1700041F RID: 1055
		// (get) Token: 0x06000C32 RID: 3122 RVA: 0x000219F5 File Offset: 0x0001FBF5
		public IReadOnlyList<Card> BattleCardUsageHistory
		{
			get
			{
				return this._battleCardUsageHistory.AsReadOnly();
			}
		}

		// Token: 0x17000420 RID: 1056
		// (get) Token: 0x06000C33 RID: 3123 RVA: 0x00021A02 File Offset: 0x0001FC02
		public IReadOnlyList<Card> TurnCardPlayHistory
		{
			get
			{
				return this._turnCardPlayHistory.AsReadOnly();
			}
		}

		// Token: 0x17000421 RID: 1057
		// (get) Token: 0x06000C34 RID: 3124 RVA: 0x00021A0F File Offset: 0x0001FC0F
		public IReadOnlyList<Card> BattleCardPlayHistory
		{
			get
			{
				return this._battleCardPlayHistory.AsReadOnly();
			}
		}

		// Token: 0x17000422 RID: 1058
		// (get) Token: 0x06000C35 RID: 3125 RVA: 0x00021A1C File Offset: 0x0001FC1C
		public IReadOnlyList<Card> TurnCardFollowAttackHistory
		{
			get
			{
				return this._turnCardFollowAttackHistory.AsReadOnly();
			}
		}

		// Token: 0x17000423 RID: 1059
		// (get) Token: 0x06000C36 RID: 3126 RVA: 0x00021A29 File Offset: 0x0001FC29
		public IReadOnlyList<Card> BattleCardFollowAttackHistory
		{
			get
			{
				return this._battleCardFollowAttackHistory.AsReadOnly();
			}
		}

		// Token: 0x17000424 RID: 1060
		// (get) Token: 0x06000C37 RID: 3127 RVA: 0x00021A36 File Offset: 0x0001FC36
		// (set) Token: 0x06000C38 RID: 3128 RVA: 0x00021A3E File Offset: 0x0001FC3E
		public int RoundCounter { get; private set; }

		// Token: 0x06000C39 RID: 3129 RVA: 0x00021A48 File Offset: 0x0001FC48
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

		// Token: 0x06000C3A RID: 3130 RVA: 0x0002208C File Offset: 0x0002028C
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

		// Token: 0x06000C3B RID: 3131 RVA: 0x0002216C File Offset: 0x0002036C
		internal void NotifyMessage(BattleMessage message)
		{
			Action<BattleMessage> notification = this.Notification;
			if (notification == null)
			{
				return;
			}
			notification.Invoke(message);
		}

		// Token: 0x06000C3C RID: 3132 RVA: 0x00022180 File Offset: 0x00020380
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

		// Token: 0x06000C3D RID: 3133 RVA: 0x00022278 File Offset: 0x00020478
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

		// Token: 0x06000C3E RID: 3134 RVA: 0x000222D7 File Offset: 0x000204D7
		internal void EndBattle()
		{
		}

		// Token: 0x06000C3F RID: 3135 RVA: 0x000222DC File Offset: 0x000204DC
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

		// Token: 0x06000C40 RID: 3136 RVA: 0x00022354 File Offset: 0x00020554
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

		// Token: 0x06000C41 RID: 3137 RVA: 0x000223D4 File Offset: 0x000205D4
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

		// Token: 0x06000C42 RID: 3138 RVA: 0x00022448 File Offset: 0x00020648
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

		// Token: 0x06000C43 RID: 3139 RVA: 0x00022574 File Offset: 0x00020774
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

		// Token: 0x06000C44 RID: 3140 RVA: 0x0002262C File Offset: 0x0002082C
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

		// Token: 0x06000C45 RID: 3141 RVA: 0x00022730 File Offset: 0x00020930
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

		// Token: 0x06000C46 RID: 3142 RVA: 0x00022824 File Offset: 0x00020A24
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

		// Token: 0x06000C47 RID: 3143 RVA: 0x00022938 File Offset: 0x00020B38
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

		// Token: 0x06000C48 RID: 3144 RVA: 0x000229AC File Offset: 0x00020BAC
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

		// Token: 0x06000C49 RID: 3145 RVA: 0x00022B20 File Offset: 0x00020D20
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

		// Token: 0x06000C4A RID: 3146 RVA: 0x00022C50 File Offset: 0x00020E50
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

		// Token: 0x06000C4B RID: 3147 RVA: 0x00022D4C File Offset: 0x00020F4C
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

		// Token: 0x06000C4C RID: 3148 RVA: 0x00022DF0 File Offset: 0x00020FF0
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

		// Token: 0x06000C4D RID: 3149 RVA: 0x00022E90 File Offset: 0x00021090
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

		// Token: 0x06000C4E RID: 3150 RVA: 0x00022F30 File Offset: 0x00021130
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

		// Token: 0x06000C4F RID: 3151 RVA: 0x00022FD0 File Offset: 0x000211D0
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

		// Token: 0x17000425 RID: 1061
		// (get) Token: 0x06000C50 RID: 3152 RVA: 0x000230E6 File Offset: 0x000212E6
		// (set) Token: 0x06000C51 RID: 3153 RVA: 0x000230EE File Offset: 0x000212EE
		public int DrawCardCount { get; set; }

		// Token: 0x06000C52 RID: 3154 RVA: 0x000230F8 File Offset: 0x000212F8
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

		// Token: 0x06000C53 RID: 3155 RVA: 0x000232B0 File Offset: 0x000214B0
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

		// Token: 0x06000C54 RID: 3156 RVA: 0x000233F8 File Offset: 0x000215F8
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

		// Token: 0x06000C55 RID: 3157 RVA: 0x00023514 File Offset: 0x00021714
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

		// Token: 0x06000C56 RID: 3158 RVA: 0x00023640 File Offset: 0x00021840
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

		// Token: 0x06000C57 RID: 3159 RVA: 0x00023770 File Offset: 0x00021970
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

		// Token: 0x06000C58 RID: 3160 RVA: 0x000238A0 File Offset: 0x00021AA0
		internal void StartRound()
		{
			this.RoundStartDecreaseDurations();
			foreach (EnemyUnit enemyUnit in this.EnemyGroup.Alives)
			{
				enemyUnit.UpdateTurnMoves();
			}
		}

		// Token: 0x06000C59 RID: 3161 RVA: 0x000238F8 File Offset: 0x00021AF8
		internal void EndRound()
		{
			this.RoundEndDecreaseDurations();
		}

		// Token: 0x06000C5A RID: 3162 RVA: 0x00023900 File Offset: 0x00021B00
		internal void StartAllEnemyTurn()
		{
			this.RevealHiddenIntentionsByEnemyTurn();
		}

		// Token: 0x06000C5B RID: 3163 RVA: 0x00023908 File Offset: 0x00021B08
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

		// Token: 0x06000C5C RID: 3164 RVA: 0x000239EC File Offset: 0x00021BEC
		public void RevealHiddenIntentions()
		{
			foreach (EnemyUnit enemyUnit in this.AllAliveEnemies)
			{
				enemyUnit.NotifyIntentionsChanged();
			}
		}

		// Token: 0x06000C5D RID: 3165 RVA: 0x00023A38 File Offset: 0x00021C38
		internal void EndAllEnemyTurn()
		{
			this.ShowEnemyIntentionByEnemyTurn = false;
		}

		// Token: 0x06000C5E RID: 3166 RVA: 0x00023A41 File Offset: 0x00021C41
		internal void StartEnemyTurn(EnemyUnit enemy)
		{
		}

		// Token: 0x06000C5F RID: 3167 RVA: 0x00023A43 File Offset: 0x00021C43
		internal void EndEnemyTurn(EnemyUnit enemy)
		{
			enemy.ClearIntentions();
		}

		// Token: 0x06000C60 RID: 3168 RVA: 0x00023A4C File Offset: 0x00021C4C
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

		// Token: 0x06000C61 RID: 3169 RVA: 0x00023AD8 File Offset: 0x00021CD8
		internal ManaGroup LoseMana(ManaGroup group)
		{
			ManaGroup battleMana = this.BattleMana;
			this.BattleMana = (this.BattleMana - group).Corrected;
			return battleMana - this.BattleMana;
		}

		// Token: 0x06000C62 RID: 3170 RVA: 0x00023B10 File Offset: 0x00021D10
		internal ManaGroup LockTurnMana(ManaGroup group)
		{
			ManaGroup manaGroup = this.LockedTurnMana + group;
			manaGroup = ManaGroup.Intersect(manaGroup, this.BaseTurnMana);
			ManaGroup manaGroup2 = manaGroup - this.LockedTurnMana;
			this.LockedTurnMana = manaGroup;
			return manaGroup2;
		}

		// Token: 0x06000C63 RID: 3171 RVA: 0x00023B4C File Offset: 0x00021D4C
		internal ManaGroup UnlockTurnMana(ManaGroup group)
		{
			ManaGroup lockedTurnMana = this.LockedTurnMana;
			this.LockedTurnMana = (this.LockedTurnMana - group).Corrected;
			return lockedTurnMana - this.LockedTurnMana;
		}

		// Token: 0x06000C64 RID: 3172 RVA: 0x00023B84 File Offset: 0x00021D84
		internal ManaGroup GainTurnMana(ManaGroup group)
		{
			ManaGroup manaGroup = (this.ExtraTurnMana + group).ClampComponentMax(99);
			ManaGroup manaGroup2 = manaGroup - this.ExtraTurnMana;
			this.ExtraTurnMana = manaGroup;
			return manaGroup2;
		}

		// Token: 0x06000C65 RID: 3173 RVA: 0x00023BBC File Offset: 0x00021DBC
		internal ManaGroup LoseTurnMana(ManaGroup group)
		{
			ManaGroup extraTurnMana = this.ExtraTurnMana;
			this.ExtraTurnMana = (this.ExtraTurnMana - group).Corrected;
			return extraTurnMana - this.ExtraTurnMana;
		}

		// Token: 0x06000C66 RID: 3174 RVA: 0x00023BF4 File Offset: 0x00021DF4
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

		// Token: 0x06000C67 RID: 3175 RVA: 0x00023C9C File Offset: 0x00021E9C
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

		// Token: 0x06000C68 RID: 3176 RVA: 0x00023D0C File Offset: 0x00021F0C
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

		// Token: 0x06000C69 RID: 3177 RVA: 0x00023DB8 File Offset: 0x00021FB8
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

		// Token: 0x06000C6A RID: 3178 RVA: 0x00023E70 File Offset: 0x00022070
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

		// Token: 0x06000C6B RID: 3179 RVA: 0x00023EB8 File Offset: 0x000220B8
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

		// Token: 0x06000C6C RID: 3180 RVA: 0x00023F00 File Offset: 0x00022100
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

		// Token: 0x06000C6D RID: 3181 RVA: 0x00023F80 File Offset: 0x00022180
		internal void StatisticalTotalDamage(Unit damageSource, IReadOnlyDictionary<Unit, IReadOnlyList<DamageEventArgs>> argsTable)
		{
		}

		// Token: 0x06000C6E RID: 3182 RVA: 0x00023F82 File Offset: 0x00022182
		internal void ForceKill(Unit source, Unit target)
		{
			target.Hp = 0;
			target.Block = 0;
			target.Shield = 0;
			target.Status = UnitStatus.Dying;
		}

		// Token: 0x06000C6F RID: 3183 RVA: 0x00023FA0 File Offset: 0x000221A0
		internal int Heal(Unit target, int healValue)
		{
			return target.Heal(healValue);
		}

		// Token: 0x06000C70 RID: 3184 RVA: 0x00023FA9 File Offset: 0x000221A9
		[return: TupleElementNames(new string[] { "block", "shield" })]
		internal ValueTuple<int, int> LoseBlockShield(Unit target, float block, float shield)
		{
			return target.LoseBlockShield(block, shield);
		}

		// Token: 0x06000C71 RID: 3185 RVA: 0x00023FB3 File Offset: 0x000221B3
		[return: TupleElementNames(new string[] { "block", "shieled" })]
		internal ValueTuple<int, int> CastBlockShield(Unit target, float block, float shield)
		{
			return target.GainBlockShield(block, shield);
		}

		// Token: 0x06000C72 RID: 3186 RVA: 0x00023FC0 File Offset: 0x000221C0
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

		// Token: 0x06000C73 RID: 3187 RVA: 0x0002407C File Offset: 0x0002227C
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

		// Token: 0x06000C74 RID: 3188 RVA: 0x000240DA File Offset: 0x000222DA
		internal Exhibit[] LoseAllExhibits()
		{
			return this.GameRun.InternalLoseAllExhibits(false);
		}

		// Token: 0x06000C75 RID: 3189 RVA: 0x000240E8 File Offset: 0x000222E8
		internal EnemyUnit Spawn(EnemyUnit spawner, Type type, int rootIndex, bool isServant)
		{
			return this.Spawn(spawner, Library.CreateEnemyUnit(type), rootIndex, isServant);
		}

		// Token: 0x06000C76 RID: 3190 RVA: 0x000240FC File Offset: 0x000222FC
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

		// Token: 0x06000C77 RID: 3191 RVA: 0x00024178 File Offset: 0x00022378
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

		// Token: 0x06000C78 RID: 3192 RVA: 0x0002425C File Offset: 0x0002245C
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

		// Token: 0x06000C79 RID: 3193 RVA: 0x000242CB File Offset: 0x000224CB
		internal void Escape(EnemyUnit unit)
		{
			unit.Escape();
			unit.Status = UnitStatus.Escaped;
		}

		// Token: 0x06000C7A RID: 3194 RVA: 0x000242DA File Offset: 0x000224DA
		internal void InstantWin()
		{
			this._forceWin = true;
			this._isWaitingPlayerInput = false;
		}

		// Token: 0x06000C7B RID: 3195 RVA: 0x000242EC File Offset: 0x000224EC
		internal void RecordCardUsage(Card card)
		{
			this._turnCardUsageHistory.Add(card);
			this._battleCardUsageHistory.Add(card);
			if (this.GameRun.IsAutoSeed && this.GameRun.JadeBoxes.Empty<JadeBox>() && this._turnCardUsageHistory.Count == 30)
			{
				this.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.CardsOneTurn);
			}
		}

		// Token: 0x06000C7C RID: 3196 RVA: 0x00024354 File Offset: 0x00022554
		internal void RecordCardPlay(Card card)
		{
			this._turnCardPlayHistory.Add(card);
			this._battleCardPlayHistory.Add(card);
			if (this.GameRun.IsAutoSeed && this.GameRun.JadeBoxes.Empty<JadeBox>() && this._turnCardPlayHistory.Count == 10)
			{
				this.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.FollowPlay);
			}
		}

		// Token: 0x06000C7D RID: 3197 RVA: 0x000243BC File Offset: 0x000225BC
		internal void RecordCardFollowAttack(IEnumerable<Card> cards)
		{
			Card[] array = (cards as Card[]) ?? Enumerable.ToArray<Card>(cards);
			this._turnCardFollowAttackHistory.AddRange(array);
			this._battleCardFollowAttackHistory.AddRange(array);
		}

		// Token: 0x17000426 RID: 1062
		// (get) Token: 0x06000C7E RID: 3198 RVA: 0x000243F2 File Offset: 0x000225F2
		// (set) Token: 0x06000C7F RID: 3199 RVA: 0x000243FA File Offset: 0x000225FA
		internal bool PlayerTurnShouldEnd { get; set; }

		// Token: 0x17000427 RID: 1063
		// (get) Token: 0x06000C80 RID: 3200 RVA: 0x00024404 File Offset: 0x00022604
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

		// Token: 0x06000C81 RID: 3201 RVA: 0x00024452 File Offset: 0x00022652
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

		// Token: 0x06000C82 RID: 3202 RVA: 0x00024461 File Offset: 0x00022661
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

		// Token: 0x06000C83 RID: 3203 RVA: 0x00024470 File Offset: 0x00022670
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

		// Token: 0x17000428 RID: 1064
		// (get) Token: 0x06000C84 RID: 3204 RVA: 0x0002447F File Offset: 0x0002267F
		public EnemyUnit RandomAliveEnemy
		{
			get
			{
				return this.EnemyGroup.Alives.SampleOrDefault(this.GameRun.BattleRng);
			}
		}

		// Token: 0x17000429 RID: 1065
		// (get) Token: 0x06000C85 RID: 3205 RVA: 0x0002449C File Offset: 0x0002269C
		public EnemyUnit FirstAliveEnemy
		{
			get
			{
				return this.EnemyGroup.Alives.MinByOrDefault((EnemyUnit unit) => unit.RootIndex);
			}
		}

		// Token: 0x1700042A RID: 1066
		// (get) Token: 0x06000C86 RID: 3206 RVA: 0x000244CD File Offset: 0x000226CD
		public EnemyUnit LastAliveEnemy
		{
			get
			{
				return this.EnemyGroup.Alives.MaxByOrDefault((EnemyUnit unit) => unit.RootIndex);
			}
		}

		// Token: 0x1700042B RID: 1067
		// (get) Token: 0x06000C87 RID: 3207 RVA: 0x000244FE File Offset: 0x000226FE
		public EnemyUnit LowestHpEnemy
		{
			get
			{
				return this.EnemyGroup.Alives.MinByOrDefault((EnemyUnit unit) => unit.Hp);
			}
		}

		// Token: 0x1700042C RID: 1068
		// (get) Token: 0x06000C88 RID: 3208 RVA: 0x0002452F File Offset: 0x0002272F
		public EnemyUnit HighestHpEnemy
		{
			get
			{
				return this.EnemyGroup.Alives.MaxByOrDefault((EnemyUnit unit) => unit.Hp);
			}
		}

		// Token: 0x1700042D RID: 1069
		// (get) Token: 0x06000C89 RID: 3209 RVA: 0x00024560 File Offset: 0x00022760
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

		// Token: 0x1700042E RID: 1070
		// (get) Token: 0x06000C8A RID: 3210 RVA: 0x00024570 File Offset: 0x00022770
		public IEnumerable<EnemyUnit> AllAliveEnemies
		{
			get
			{
				return this.EnemyGroup.Alives;
			}
		}

		// Token: 0x06000C8B RID: 3211 RVA: 0x00024580 File Offset: 0x00022780
		public EnemyUnit GetEnemyByRootIndex(int rootIndex)
		{
			return Enumerable.FirstOrDefault<EnemyUnit>(this.AllAliveEnemies, (EnemyUnit enemy) => enemy.RootIndex == rootIndex);
		}

		// Token: 0x06000C8C RID: 3212 RVA: 0x000245B4 File Offset: 0x000227B4
		public bool IsAnyoneInRootIndex(int rootIndex)
		{
			return Enumerable.Any<EnemyUnit>(this.AllAliveEnemies, (EnemyUnit enemy) => enemy.RootIndex == rootIndex);
		}

		// Token: 0x06000C8D RID: 3213 RVA: 0x000245E5 File Offset: 0x000227E5
		public Card[] RollCards(CardWeightTable weightTable, int count, [MaybeNull] Predicate<CardConfig> filter = null)
		{
			return this.GameRun.RollCards(this.GameRun.BattleCardRng, weightTable, count, false, true, filter);
		}

		// Token: 0x06000C8E RID: 3214 RVA: 0x00024602 File Offset: 0x00022802
		public Card RollCard(CardWeightTable weightTable, [MaybeNull] Predicate<CardConfig> filter = null)
		{
			return Enumerable.FirstOrDefault<Card>(this.RollCards(weightTable, 1, filter));
		}

		// Token: 0x06000C8F RID: 3215 RVA: 0x00024612 File Offset: 0x00022812
		public Card[] RollCardsWithoutManaLimit(CardWeightTable weightTable, int count, [MaybeNull] Predicate<CardConfig> filter = null)
		{
			return this.GameRun.RollCardsWithoutManaLimit(this.GameRun.BattleCardRng, weightTable, count, false, true, filter);
		}

		// Token: 0x06000C90 RID: 3216 RVA: 0x0002462F File Offset: 0x0002282F
		public bool CanAffordUs()
		{
			return this.Player.Power >= this.Player.Us.PowerCost;
		}

		// Token: 0x06000C91 RID: 3217 RVA: 0x00024651 File Offset: 0x00022851
		public bool CanUseUs()
		{
			return this.Player.HasUs && this.Player.Us.Available && this.CanAffordUs();
		}

		// Token: 0x06000C92 RID: 3218 RVA: 0x0002467C File Offset: 0x0002287C
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

		// Token: 0x06000C93 RID: 3219 RVA: 0x000246E0 File Offset: 0x000228E0
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

		// Token: 0x1700042F RID: 1071
		// (get) Token: 0x06000C94 RID: 3220 RVA: 0x00024740 File Offset: 0x00022940
		// (set) Token: 0x06000C95 RID: 3221 RVA: 0x00024748 File Offset: 0x00022948
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

		// Token: 0x06000C96 RID: 3222 RVA: 0x0002475C File Offset: 0x0002295C
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

		// Token: 0x06000C97 RID: 3223 RVA: 0x00024914 File Offset: 0x00022B14
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

		// Token: 0x06000C98 RID: 3224 RVA: 0x000249E0 File Offset: 0x00022BE0
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

		// Token: 0x06000C99 RID: 3225 RVA: 0x00024AAF File Offset: 0x00022CAF
		public void RequestEndPlayerTurn()
		{
			if (!this.IsWaitingPlayerInput)
			{
				throw new InvalidOperationException("Cannot end player turn: not waiting for player input");
			}
			this._playerInputAction = new RequestEndPlayerTurnAction();
			this._isWaitingPlayerInput = false;
		}

		// Token: 0x06000C9A RID: 3226 RVA: 0x00024AD6 File Offset: 0x00022CD6
		public void RequestDebugAction(BattleAction action, string recordName)
		{
			this._debugActionQueue.Enqueue(new ValueTuple<BattleAction, string>(action, recordName));
		}

		// Token: 0x06000C9B RID: 3227 RVA: 0x00024AEA File Offset: 0x00022CEA
		internal void React(Reactor reactor, [MaybeNull] GameEntity source, ActionCause cause)
		{
			this._resolver.React(reactor, source, cause);
		}

		// Token: 0x06000C9C RID: 3228 RVA: 0x00024AFC File Offset: 0x00022CFC
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

		// Token: 0x17000430 RID: 1072
		// (get) Token: 0x06000C9D RID: 3229 RVA: 0x00024B4A File Offset: 0x00022D4A
		public GameEvent<GameEventArgs> BattleStarting { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x17000431 RID: 1073
		// (get) Token: 0x06000C9E RID: 3230 RVA: 0x00024B52 File Offset: 0x00022D52
		public GameEvent<GameEventArgs> BattleStarted { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x17000432 RID: 1074
		// (get) Token: 0x06000C9F RID: 3231 RVA: 0x00024B5A File Offset: 0x00022D5A
		public GameEvent<GameEventArgs> BattleEnding { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x17000433 RID: 1075
		// (get) Token: 0x06000CA0 RID: 3232 RVA: 0x00024B62 File Offset: 0x00022D62
		public GameEvent<GameEventArgs> BattleEnded { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x17000434 RID: 1076
		// (get) Token: 0x06000CA1 RID: 3233 RVA: 0x00024B6A File Offset: 0x00022D6A
		public GameEvent<GameEventArgs> RoundStarting { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x17000435 RID: 1077
		// (get) Token: 0x06000CA2 RID: 3234 RVA: 0x00024B72 File Offset: 0x00022D72
		public GameEvent<GameEventArgs> RoundStarted { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x17000436 RID: 1078
		// (get) Token: 0x06000CA3 RID: 3235 RVA: 0x00024B7A File Offset: 0x00022D7A
		public GameEvent<GameEventArgs> RoundEnding { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x17000437 RID: 1079
		// (get) Token: 0x06000CA4 RID: 3236 RVA: 0x00024B82 File Offset: 0x00022D82
		public GameEvent<GameEventArgs> RoundEnded { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x17000438 RID: 1080
		// (get) Token: 0x06000CA5 RID: 3237 RVA: 0x00024B8A File Offset: 0x00022D8A
		public GameEvent<GameEventArgs> AllEnemyTurnStarting { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x17000439 RID: 1081
		// (get) Token: 0x06000CA6 RID: 3238 RVA: 0x00024B92 File Offset: 0x00022D92
		public GameEvent<GameEventArgs> AllEnemyTurnStarted { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x1700043A RID: 1082
		// (get) Token: 0x06000CA7 RID: 3239 RVA: 0x00024B9A File Offset: 0x00022D9A
		public GameEvent<GameEventArgs> AllEnemyTurnEnding { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x1700043B RID: 1083
		// (get) Token: 0x06000CA8 RID: 3240 RVA: 0x00024BA2 File Offset: 0x00022DA2
		public GameEvent<GameEventArgs> AllEnemyTurnEnded { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x1700043C RID: 1084
		// (get) Token: 0x06000CA9 RID: 3241 RVA: 0x00024BAA File Offset: 0x00022DAA
		public GameEvent<CardEventArgs> Predraw { get; } = new GameEvent<CardEventArgs>();

		// Token: 0x1700043D RID: 1085
		// (get) Token: 0x06000CAA RID: 3242 RVA: 0x00024BB2 File Offset: 0x00022DB2
		public GameEvent<CardEventArgs> CardDrawing { get; } = new GameEvent<CardEventArgs>();

		// Token: 0x1700043E RID: 1086
		// (get) Token: 0x06000CAB RID: 3243 RVA: 0x00024BBA File Offset: 0x00022DBA
		public GameEvent<CardEventArgs> CardDrawn { get; } = new GameEvent<CardEventArgs>();

		// Token: 0x1700043F RID: 1087
		// (get) Token: 0x06000CAC RID: 3244 RVA: 0x00024BC2 File Offset: 0x00022DC2
		public GameEvent<CardEventArgs> CardDiscarding { get; } = new GameEvent<CardEventArgs>();

		// Token: 0x17000440 RID: 1088
		// (get) Token: 0x06000CAD RID: 3245 RVA: 0x00024BCA File Offset: 0x00022DCA
		public GameEvent<CardEventArgs> CardDiscarded { get; } = new GameEvent<CardEventArgs>();

		// Token: 0x17000441 RID: 1089
		// (get) Token: 0x06000CAE RID: 3246 RVA: 0x00024BD2 File Offset: 0x00022DD2
		public GameEvent<CardUsingEventArgs> CardUsing { get; } = new GameEvent<CardUsingEventArgs>();

		// Token: 0x17000442 RID: 1090
		// (get) Token: 0x06000CAF RID: 3247 RVA: 0x00024BDA File Offset: 0x00022DDA
		public GameEvent<CardUsingEventArgs> CardUsed { get; } = new GameEvent<CardUsingEventArgs>();

		// Token: 0x17000443 RID: 1091
		// (get) Token: 0x06000CB0 RID: 3248 RVA: 0x00024BE2 File Offset: 0x00022DE2
		public GameEvent<CardUsingEventArgs> CardUsingCanceled { get; } = new GameEvent<CardUsingEventArgs>();

		// Token: 0x17000444 RID: 1092
		// (get) Token: 0x06000CB1 RID: 3249 RVA: 0x00024BEA File Offset: 0x00022DEA
		public GameEvent<CardUsingEventArgs> CardPlaying { get; } = new GameEvent<CardUsingEventArgs>();

		// Token: 0x17000445 RID: 1093
		// (get) Token: 0x06000CB2 RID: 3250 RVA: 0x00024BF2 File Offset: 0x00022DF2
		public GameEvent<CardUsingEventArgs> CardPlayed { get; } = new GameEvent<CardUsingEventArgs>();

		// Token: 0x17000446 RID: 1094
		// (get) Token: 0x06000CB3 RID: 3251 RVA: 0x00024BFA File Offset: 0x00022DFA
		public GameEvent<FollowAttackEventArgs> FollowAttacking { get; } = new GameEvent<FollowAttackEventArgs>();

		// Token: 0x17000447 RID: 1095
		// (get) Token: 0x06000CB4 RID: 3252 RVA: 0x00024C02 File Offset: 0x00022E02
		public GameEvent<FollowAttackEventArgs> FollowAttacked { get; } = new GameEvent<FollowAttackEventArgs>();

		// Token: 0x17000448 RID: 1096
		// (get) Token: 0x06000CB5 RID: 3253 RVA: 0x00024C0A File Offset: 0x00022E0A
		public GameEvent<CardEventArgs> CardExiling { get; } = new GameEvent<CardEventArgs>();

		// Token: 0x17000449 RID: 1097
		// (get) Token: 0x06000CB6 RID: 3254 RVA: 0x00024C12 File Offset: 0x00022E12
		public GameEvent<CardEventArgs> CardExiled { get; } = new GameEvent<CardEventArgs>();

		// Token: 0x1700044A RID: 1098
		// (get) Token: 0x06000CB7 RID: 3255 RVA: 0x00024C1A File Offset: 0x00022E1A
		public GameEvent<CardTransformEventArgs> CardTransforming { get; } = new GameEvent<CardTransformEventArgs>();

		// Token: 0x1700044B RID: 1099
		// (get) Token: 0x06000CB8 RID: 3256 RVA: 0x00024C22 File Offset: 0x00022E22
		public GameEvent<CardTransformEventArgs> CardTransformed { get; } = new GameEvent<CardTransformEventArgs>();

		// Token: 0x1700044C RID: 1100
		// (get) Token: 0x06000CB9 RID: 3257 RVA: 0x00024C2A File Offset: 0x00022E2A
		public GameEvent<CardEventArgs> CardRetaining { get; } = new GameEvent<CardEventArgs>();

		// Token: 0x1700044D RID: 1101
		// (get) Token: 0x06000CBA RID: 3258 RVA: 0x00024C32 File Offset: 0x00022E32
		public GameEvent<CardEventArgs> CardRetained { get; } = new GameEvent<CardEventArgs>();

		// Token: 0x1700044E RID: 1102
		// (get) Token: 0x06000CBB RID: 3259 RVA: 0x00024C3A File Offset: 0x00022E3A
		public GameEvent<CardEventArgs> CardRemoving { get; } = new GameEvent<CardEventArgs>();

		// Token: 0x1700044F RID: 1103
		// (get) Token: 0x06000CBC RID: 3260 RVA: 0x00024C42 File Offset: 0x00022E42
		public GameEvent<CardEventArgs> CardRemoved { get; } = new GameEvent<CardEventArgs>();

		// Token: 0x17000450 RID: 1104
		// (get) Token: 0x06000CBD RID: 3261 RVA: 0x00024C4A File Offset: 0x00022E4A
		public GameEvent<GameEventArgs> Reshuffling { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x17000451 RID: 1105
		// (get) Token: 0x06000CBE RID: 3262 RVA: 0x00024C52 File Offset: 0x00022E52
		public GameEvent<GameEventArgs> Reshuffled { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x17000452 RID: 1106
		// (get) Token: 0x06000CBF RID: 3263 RVA: 0x00024C5A File Offset: 0x00022E5A
		public GameEvent<CardsAddingToDrawZoneEventArgs> CardsAddingToDrawZone { get; } = new GameEvent<CardsAddingToDrawZoneEventArgs>();

		// Token: 0x17000453 RID: 1107
		// (get) Token: 0x06000CC0 RID: 3264 RVA: 0x00024C62 File Offset: 0x00022E62
		public GameEvent<CardsAddingToDrawZoneEventArgs> CardsAddedToDrawZone { get; } = new GameEvent<CardsAddingToDrawZoneEventArgs>();

		// Token: 0x17000454 RID: 1108
		// (get) Token: 0x06000CC1 RID: 3265 RVA: 0x00024C6A File Offset: 0x00022E6A
		public GameEvent<CardsEventArgs> CardsAddingToHand { get; } = new GameEvent<CardsEventArgs>();

		// Token: 0x17000455 RID: 1109
		// (get) Token: 0x06000CC2 RID: 3266 RVA: 0x00024C72 File Offset: 0x00022E72
		public GameEvent<CardsEventArgs> CardsAddedToHand { get; } = new GameEvent<CardsEventArgs>();

		// Token: 0x17000456 RID: 1110
		// (get) Token: 0x06000CC3 RID: 3267 RVA: 0x00024C7A File Offset: 0x00022E7A
		public GameEvent<CardsEventArgs> CardsAddingToDiscard { get; } = new GameEvent<CardsEventArgs>();

		// Token: 0x17000457 RID: 1111
		// (get) Token: 0x06000CC4 RID: 3268 RVA: 0x00024C82 File Offset: 0x00022E82
		public GameEvent<CardsEventArgs> CardsAddedToDiscard { get; } = new GameEvent<CardsEventArgs>();

		// Token: 0x17000458 RID: 1112
		// (get) Token: 0x06000CC5 RID: 3269 RVA: 0x00024C8A File Offset: 0x00022E8A
		public GameEvent<CardsEventArgs> CardsAddingToExile { get; } = new GameEvent<CardsEventArgs>();

		// Token: 0x17000459 RID: 1113
		// (get) Token: 0x06000CC6 RID: 3270 RVA: 0x00024C92 File Offset: 0x00022E92
		public GameEvent<CardsEventArgs> CardsAddedToExile { get; } = new GameEvent<CardsEventArgs>();

		// Token: 0x1700045A RID: 1114
		// (get) Token: 0x06000CC7 RID: 3271 RVA: 0x00024C9A File Offset: 0x00022E9A
		public GameEvent<CardMovingEventArgs> CardMoving { get; } = new GameEvent<CardMovingEventArgs>();

		// Token: 0x1700045B RID: 1115
		// (get) Token: 0x06000CC8 RID: 3272 RVA: 0x00024CA2 File Offset: 0x00022EA2
		public GameEvent<CardMovingEventArgs> CardMoved { get; } = new GameEvent<CardMovingEventArgs>();

		// Token: 0x1700045C RID: 1116
		// (get) Token: 0x06000CC9 RID: 3273 RVA: 0x00024CAA File Offset: 0x00022EAA
		public GameEvent<CardMovingToDrawZoneEventArgs> CardMovingToDrawZone { get; } = new GameEvent<CardMovingToDrawZoneEventArgs>();

		// Token: 0x1700045D RID: 1117
		// (get) Token: 0x06000CCA RID: 3274 RVA: 0x00024CB2 File Offset: 0x00022EB2
		public GameEvent<CardMovingToDrawZoneEventArgs> CardMovedToDrawZone { get; } = new GameEvent<CardMovingToDrawZoneEventArgs>();

		// Token: 0x1700045E RID: 1118
		// (get) Token: 0x06000CCB RID: 3275 RVA: 0x00024CBA File Offset: 0x00022EBA
		public GameEvent<UsUsingEventArgs> UsUsing { get; } = new GameEvent<UsUsingEventArgs>();

		// Token: 0x1700045F RID: 1119
		// (get) Token: 0x06000CCC RID: 3276 RVA: 0x00024CC2 File Offset: 0x00022EC2
		public GameEvent<UsUsingEventArgs> UsUsed { get; } = new GameEvent<UsUsingEventArgs>();

		// Token: 0x17000460 RID: 1120
		// (get) Token: 0x06000CCD RID: 3277 RVA: 0x00024CCA File Offset: 0x00022ECA
		public GameEvent<GameEventArgs> UsUsingCanceled { get; } = new GameEvent<GameEventArgs>();

		// Token: 0x17000461 RID: 1121
		// (get) Token: 0x06000CCE RID: 3278 RVA: 0x00024CD2 File Offset: 0x00022ED2
		public GameEvent<DollUsingEventArgs> DollUsing { get; } = new GameEvent<DollUsingEventArgs>();

		// Token: 0x17000462 RID: 1122
		// (get) Token: 0x06000CCF RID: 3279 RVA: 0x00024CDA File Offset: 0x00022EDA
		public GameEvent<DollUsingEventArgs> DollUsed { get; } = new GameEvent<DollUsingEventArgs>();

		// Token: 0x17000463 RID: 1123
		// (get) Token: 0x06000CD0 RID: 3280 RVA: 0x00024CE2 File Offset: 0x00022EE2
		public GameEvent<ManaEventArgs> TurnManaLocking { get; } = new GameEvent<ManaEventArgs>();

		// Token: 0x17000464 RID: 1124
		// (get) Token: 0x06000CD1 RID: 3281 RVA: 0x00024CEA File Offset: 0x00022EEA
		public GameEvent<ManaEventArgs> TurnManaLocked { get; } = new GameEvent<ManaEventArgs>();

		// Token: 0x17000465 RID: 1125
		// (get) Token: 0x06000CD2 RID: 3282 RVA: 0x00024CF2 File Offset: 0x00022EF2
		public GameEvent<ManaEventArgs> TurnManaUnlocking { get; } = new GameEvent<ManaEventArgs>();

		// Token: 0x17000466 RID: 1126
		// (get) Token: 0x06000CD3 RID: 3283 RVA: 0x00024CFA File Offset: 0x00022EFA
		public GameEvent<ManaEventArgs> TurnManaUnlocked { get; } = new GameEvent<ManaEventArgs>();

		// Token: 0x17000467 RID: 1127
		// (get) Token: 0x06000CD4 RID: 3284 RVA: 0x00024D02 File Offset: 0x00022F02
		public GameEvent<ManaEventArgs> TurnManaGaining { get; } = new GameEvent<ManaEventArgs>();

		// Token: 0x17000468 RID: 1128
		// (get) Token: 0x06000CD5 RID: 3285 RVA: 0x00024D0A File Offset: 0x00022F0A
		public GameEvent<ManaEventArgs> TurnManaGained { get; } = new GameEvent<ManaEventArgs>();

		// Token: 0x17000469 RID: 1129
		// (get) Token: 0x06000CD6 RID: 3286 RVA: 0x00024D12 File Offset: 0x00022F12
		public GameEvent<ManaEventArgs> TurnManaLosing { get; } = new GameEvent<ManaEventArgs>();

		// Token: 0x1700046A RID: 1130
		// (get) Token: 0x06000CD7 RID: 3287 RVA: 0x00024D1A File Offset: 0x00022F1A
		public GameEvent<ManaEventArgs> TurnManaLost { get; } = new GameEvent<ManaEventArgs>();

		// Token: 0x1700046B RID: 1131
		// (get) Token: 0x06000CD8 RID: 3288 RVA: 0x00024D22 File Offset: 0x00022F22
		public GameEvent<ManaEventArgs> ManaGaining { get; } = new GameEvent<ManaEventArgs>();

		// Token: 0x1700046C RID: 1132
		// (get) Token: 0x06000CD9 RID: 3289 RVA: 0x00024D2A File Offset: 0x00022F2A
		public GameEvent<ManaEventArgs> ManaGained { get; } = new GameEvent<ManaEventArgs>();

		// Token: 0x1700046D RID: 1133
		// (get) Token: 0x06000CDA RID: 3290 RVA: 0x00024D32 File Offset: 0x00022F32
		public GameEvent<ManaEventArgs> ManaConsuming { get; } = new GameEvent<ManaEventArgs>();

		// Token: 0x1700046E RID: 1134
		// (get) Token: 0x06000CDB RID: 3291 RVA: 0x00024D3A File Offset: 0x00022F3A
		public GameEvent<ManaEventArgs> ManaConsumed { get; } = new GameEvent<ManaEventArgs>();

		// Token: 0x1700046F RID: 1135
		// (get) Token: 0x06000CDC RID: 3292 RVA: 0x00024D42 File Offset: 0x00022F42
		public GameEvent<ManaEventArgs> ManaLosing { get; } = new GameEvent<ManaEventArgs>();

		// Token: 0x17000470 RID: 1136
		// (get) Token: 0x06000CDD RID: 3293 RVA: 0x00024D4A File Offset: 0x00022F4A
		public GameEvent<ManaEventArgs> ManaLost { get; } = new GameEvent<ManaEventArgs>();

		// Token: 0x17000471 RID: 1137
		// (get) Token: 0x06000CDE RID: 3294 RVA: 0x00024D52 File Offset: 0x00022F52
		public GameEvent<ManaConvertingEventArgs> ManaConverting { get; } = new GameEvent<ManaConvertingEventArgs>();

		// Token: 0x17000472 RID: 1138
		// (get) Token: 0x06000CDF RID: 3295 RVA: 0x00024D5A File Offset: 0x00022F5A
		public GameEvent<ManaConvertingEventArgs> ManaConverted { get; } = new GameEvent<ManaConvertingEventArgs>();

		// Token: 0x17000473 RID: 1139
		// (get) Token: 0x06000CE0 RID: 3296 RVA: 0x00024D62 File Offset: 0x00022F62
		public GameEvent<DollEventArgs> DollAdding { get; } = new GameEvent<DollEventArgs>();

		// Token: 0x17000474 RID: 1140
		// (get) Token: 0x06000CE1 RID: 3297 RVA: 0x00024D6A File Offset: 0x00022F6A
		public GameEvent<DollEventArgs> DollAdded { get; } = new GameEvent<DollEventArgs>();

		// Token: 0x17000475 RID: 1141
		// (get) Token: 0x06000CE2 RID: 3298 RVA: 0x00024D72 File Offset: 0x00022F72
		public GameEvent<DollEventArgs> DollRemoving { get; } = new GameEvent<DollEventArgs>();

		// Token: 0x17000476 RID: 1142
		// (get) Token: 0x06000CE3 RID: 3299 RVA: 0x00024D7A File Offset: 0x00022F7A
		public GameEvent<DollEventArgs> DollRemoved { get; } = new GameEvent<DollEventArgs>();

		// Token: 0x17000477 RID: 1143
		// (get) Token: 0x06000CE4 RID: 3300 RVA: 0x00024D82 File Offset: 0x00022F82
		public GameEvent<DollTriggeredEventArgs> DollTriggeredActive { get; } = new GameEvent<DollTriggeredEventArgs>();

		// Token: 0x17000478 RID: 1144
		// (get) Token: 0x06000CE5 RID: 3301 RVA: 0x00024D8A File Offset: 0x00022F8A
		public GameEvent<DollTriggeredEventArgs> DollTriggeredPassive { get; } = new GameEvent<DollTriggeredEventArgs>();

		// Token: 0x17000479 RID: 1145
		// (get) Token: 0x06000CE6 RID: 3302 RVA: 0x00024D92 File Offset: 0x00022F92
		public GameEvent<UnitEventArgs> EnemySpawning { get; } = new GameEvent<UnitEventArgs>();

		// Token: 0x1700047A RID: 1146
		// (get) Token: 0x06000CE7 RID: 3303 RVA: 0x00024D9A File Offset: 0x00022F9A
		public GameEvent<UnitEventArgs> EnemySpawned { get; } = new GameEvent<UnitEventArgs>();

		// Token: 0x1700047B RID: 1147
		// (get) Token: 0x06000CE8 RID: 3304 RVA: 0x00024DA2 File Offset: 0x00022FA2
		public GameEvent<DieEventArgs> EnemyPointGenerating { get; } = new GameEvent<DieEventArgs>();

		// Token: 0x1700047C RID: 1148
		// (get) Token: 0x06000CE9 RID: 3305 RVA: 0x00024DAA File Offset: 0x00022FAA
		public GameEvent<DieEventArgs> EnemyDied { get; } = new GameEvent<DieEventArgs>();

		// Token: 0x1700047D RID: 1149
		// (get) Token: 0x06000CEA RID: 3306 RVA: 0x00024DB2 File Offset: 0x00022FB2
		public GameEvent<UnitEventArgs> EnemyEscaped { get; } = new GameEvent<UnitEventArgs>();

		// Token: 0x1700047E RID: 1150
		// (get) Token: 0x06000CEB RID: 3307 RVA: 0x00024DBA File Offset: 0x00022FBA
		public GameEvent<ScryEventArgs> Scrying { get; } = new GameEvent<ScryEventArgs>();

		// Token: 0x1700047F RID: 1151
		// (get) Token: 0x06000CEC RID: 3308 RVA: 0x00024DC2 File Offset: 0x00022FC2
		public GameEvent<ScryEventArgs> Scried { get; } = new GameEvent<ScryEventArgs>();

		// Token: 0x1400000E RID: 14
		// (add) Token: 0x06000CED RID: 3309 RVA: 0x00024DCC File Offset: 0x00022FCC
		// (remove) Token: 0x06000CEE RID: 3310 RVA: 0x00024E04 File Offset: 0x00023004
		public event Action GlobalStatusChanged;

		// Token: 0x06000CEF RID: 3311 RVA: 0x00024E39 File Offset: 0x00023039
		internal void TriggerGlobalStatusChanged()
		{
			Action globalStatusChanged = this.GlobalStatusChanged;
			if (globalStatusChanged == null)
			{
				return;
			}
			globalStatusChanged.Invoke();
		}

		// Token: 0x1400000F RID: 15
		// (add) Token: 0x06000CF0 RID: 3312 RVA: 0x00024E4C File Offset: 0x0002304C
		// (remove) Token: 0x06000CF1 RID: 3313 RVA: 0x00024E84 File Offset: 0x00023084
		public event Action WaitingPlayerInput;

		// Token: 0x06000CF2 RID: 3314 RVA: 0x00024EB9 File Offset: 0x000230B9
		internal void TriggerWaitingPlayerInput()
		{
			Action waitingPlayerInput = this.WaitingPlayerInput;
			if (waitingPlayerInput == null)
			{
				return;
			}
			waitingPlayerInput.Invoke();
		}

		// Token: 0x14000010 RID: 16
		// (add) Token: 0x06000CF3 RID: 3315 RVA: 0x00024ECC File Offset: 0x000230CC
		// (remove) Token: 0x06000CF4 RID: 3316 RVA: 0x00024F04 File Offset: 0x00023104
		public event Action<BattleMessage> Notification;

		// Token: 0x17000480 RID: 1152
		// (get) Token: 0x06000CF5 RID: 3317 RVA: 0x00024F39 File Offset: 0x00023139
		public bool HandIsNotFull
		{
			get
			{
				return !this.HandIsFull;
			}
		}

		// Token: 0x17000481 RID: 1153
		// (get) Token: 0x06000CF6 RID: 3318 RVA: 0x00024F44 File Offset: 0x00023144
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

		// Token: 0x17000482 RID: 1154
		// (get) Token: 0x06000CF7 RID: 3319 RVA: 0x00024F90 File Offset: 0x00023190
		public int CardsToFull
		{
			get
			{
				return this.MaxHand - Enumerable.Count<Card>(this.HandZone, (Card card) => !card.IsAutoExile);
			}
		}

		// Token: 0x17000483 RID: 1155
		// (get) Token: 0x06000CF8 RID: 3320 RVA: 0x00024FC3 File Offset: 0x000231C3
		// (set) Token: 0x06000CF9 RID: 3321 RVA: 0x00024FCB File Offset: 0x000231CB
		public int MaxHand { get; set; } = 10;

		// Token: 0x17000484 RID: 1156
		// (get) Token: 0x06000CFA RID: 3322 RVA: 0x00024FD4 File Offset: 0x000231D4
		// (set) Token: 0x06000CFB RID: 3323 RVA: 0x00024FDC File Offset: 0x000231DC
		public bool StartTurnDrawing { get; set; }

		// Token: 0x17000485 RID: 1157
		// (get) Token: 0x06000CFC RID: 3324 RVA: 0x00024FE5 File Offset: 0x000231E5
		// (set) Token: 0x06000CFD RID: 3325 RVA: 0x00024FED File Offset: 0x000231ED
		public int DrawAfterDiscard { get; set; }

		// Token: 0x17000486 RID: 1158
		// (get) Token: 0x06000CFE RID: 3326 RVA: 0x00024FF6 File Offset: 0x000231F6
		// (set) Token: 0x06000CFF RID: 3327 RVA: 0x00024FFE File Offset: 0x000231FE
		public int CardExtraGrowAmount { get; set; }

		// Token: 0x17000487 RID: 1159
		// (get) Token: 0x06000D00 RID: 3328 RVA: 0x00025007 File Offset: 0x00023207
		// (set) Token: 0x06000D01 RID: 3329 RVA: 0x0002500F File Offset: 0x0002320F
		public int ManaFreezeLevel { get; set; }

		// Token: 0x17000488 RID: 1160
		// (get) Token: 0x06000D02 RID: 3330 RVA: 0x00025018 File Offset: 0x00023218
		// (set) Token: 0x06000D03 RID: 3331 RVA: 0x00025020 File Offset: 0x00023220
		public int PlayedCardInManaFreezeLevel { get; set; }

		// Token: 0x06000D04 RID: 3332 RVA: 0x0002502C File Offset: 0x0002322C
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

		// Token: 0x17000489 RID: 1161
		// (get) Token: 0x06000D05 RID: 3333 RVA: 0x000250E8 File Offset: 0x000232E8
		// (set) Token: 0x06000D06 RID: 3334 RVA: 0x000250F0 File Offset: 0x000232F0
		public int FriendPassiveTimes { get; set; } = 1;

		// Token: 0x1700048A RID: 1162
		// (get) Token: 0x06000D07 RID: 3335 RVA: 0x000250F9 File Offset: 0x000232F9
		// (set) Token: 0x06000D08 RID: 3336 RVA: 0x00025101 File Offset: 0x00023301
		public bool PlayerSummonAFriendThisTurn { get; set; }

		// Token: 0x1700048B RID: 1163
		// (get) Token: 0x06000D09 RID: 3337 RVA: 0x0002510A File Offset: 0x0002330A
		// (set) Token: 0x06000D0A RID: 3338 RVA: 0x00025112 File Offset: 0x00023312
		public int HideEnemyIntentionLevel { get; set; }

		// Token: 0x1700048C RID: 1164
		// (get) Token: 0x06000D0B RID: 3339 RVA: 0x0002511B File Offset: 0x0002331B
		// (set) Token: 0x06000D0C RID: 3340 RVA: 0x00025123 File Offset: 0x00023323
		public bool ShowEnemyIntentionByEnemyTurn { get; set; }

		// Token: 0x1700048D RID: 1165
		// (get) Token: 0x06000D0D RID: 3341 RVA: 0x0002512C File Offset: 0x0002332C
		public bool HideEnemyIntention
		{
			get
			{
				return this.HideEnemyIntentionLevel > 0 && !this.ShowEnemyIntentionByEnemyTurn;
			}
		}

		// Token: 0x1700048E RID: 1166
		// (get) Token: 0x06000D0E RID: 3342 RVA: 0x00025142 File Offset: 0x00023342
		// (set) Token: 0x06000D0F RID: 3343 RVA: 0x0002514A File Offset: 0x0002334A
		public int FollowAttackFillerLevel { get; set; }

		// Token: 0x1700048F RID: 1167
		// (get) Token: 0x06000D10 RID: 3344 RVA: 0x00025153 File Offset: 0x00023353
		// (set) Token: 0x06000D11 RID: 3345 RVA: 0x0002515B File Offset: 0x0002335B
		public int LimaoSchoolFrogTimes { get; set; }

		// Token: 0x17000490 RID: 1168
		// (get) Token: 0x06000D12 RID: 3346 RVA: 0x00025164 File Offset: 0x00023364
		// (set) Token: 0x06000D13 RID: 3347 RVA: 0x0002516C File Offset: 0x0002336C
		public int ReimuHuashanTimes { get; set; }

		// Token: 0x04000583 RID: 1411
		private readonly WeakReference<GameRunController> _gameRun = new WeakReference<GameRunController>(null);

		// Token: 0x04000584 RID: 1412
		private readonly List<Card> _drawZone = new List<Card>();

		// Token: 0x04000585 RID: 1413
		private readonly List<Card> _handZone = new List<Card>();

		// Token: 0x04000586 RID: 1414
		private readonly List<Card> _playArea = new List<Card>();

		// Token: 0x04000587 RID: 1415
		private readonly List<Card> _discardZone = new List<Card>();

		// Token: 0x04000588 RID: 1416
		private readonly List<Card> _exileZone = new List<Card>();

		// Token: 0x04000589 RID: 1417
		private readonly List<Card> _followArea = new List<Card>();

		// Token: 0x0400058A RID: 1418
		private readonly ActionResolver _resolver;

		// Token: 0x0400058B RID: 1419
		[TupleElementNames(new string[] { "action", "name" })]
		private readonly Queue<ValueTuple<BattleAction, string>> _debugActionQueue = new Queue<ValueTuple<BattleAction, string>>();

		// Token: 0x0400058C RID: 1420
		private readonly List<Card> _turnDrawHistory = new List<Card>();

		// Token: 0x0400058D RID: 1421
		private readonly List<Card> _turnDiscardHistory = new List<Card>();

		// Token: 0x0400058E RID: 1422
		private readonly List<Card> _turnExileHistory = new List<Card>();

		// Token: 0x0400058F RID: 1423
		private readonly List<Card> _turnCardUsageHistory = new List<Card>();

		// Token: 0x04000590 RID: 1424
		private readonly List<Card> _battleCardUsageHistory = new List<Card>();

		// Token: 0x04000591 RID: 1425
		private readonly List<Card> _turnCardPlayHistory = new List<Card>();

		// Token: 0x04000592 RID: 1426
		private readonly List<Card> _battleCardPlayHistory = new List<Card>();

		// Token: 0x04000593 RID: 1427
		private readonly List<Card> _turnCardFollowAttackHistory = new List<Card>();

		// Token: 0x04000594 RID: 1428
		private readonly List<Card> _battleCardFollowAttackHistory = new List<Card>();

		// Token: 0x04000595 RID: 1429
		private int _cardInstanceId = 1000;

		// Token: 0x0400059D RID: 1437
		private ManaGroup _turnManaGained;

		// Token: 0x0400059F RID: 1439
		private bool _forceWin;

		// Token: 0x040005A2 RID: 1442
		private bool _isWaitingPlayerInput;

		// Token: 0x040005A3 RID: 1443
		private BattleAction _playerInputAction;

		// Token: 0x040005A4 RID: 1444
		private string _playerInputActionRecordName;

		// Token: 0x040005A5 RID: 1445
		private readonly Dictionary<Type, ICustomCounter> _customCounterTable = new Dictionary<Type, ICustomCounter>();
	}
}
