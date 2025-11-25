using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy;
using UnityEngine;
namespace LBoL.EntityLib.EnemyUnits.Normal
{
	[UsedImplicitly]
	public sealed class FraudRabbit : EnemyUnit
	{
		private FraudRabbit.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = FraudRabbit.MoveType.Start;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			base.ReactBattleEvent<UnitEventArgs>(base.TurnStarted, new Func<UnitEventArgs, IEnumerable<BattleAction>>(this.OnTurnStarted));
			base.ReactBattleEvent<DieEventArgs>(base.Died, new Func<DieEventArgs, IEnumerable<BattleAction>>(this.OnDied));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			this.DebuffType = base.EnemyBattleRng.NextInt(0, 1) == 0;
			Type typeFromHandle = typeof(PhoneBillSe);
			int? num = new int?(base.Power);
			int? num2 = new int?(0);
			yield return new ApplyStatusEffectAction(typeFromHandle, this, num, default(int?), num2, default(int?), 0f, true);
			yield break;
		}
		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs arg)
		{
			if (this._escaping || this.Next == FraudRabbit.MoveType.Start)
			{
				yield break;
			}
			if (Enumerable.Count<EnemyUnit>(base.AllAliveEnemies) == 1)
			{
				yield return PerformAction.Chat(this, "Chat.FraudRabbitOnlyOne".Localize(true), 3f, 0f, 0f, true);
				this._escaping = true;
			}
			else if (base.GameRun.Money == 0)
			{
				yield return PerformAction.Chat(this, "Chat.FraudRabbitNoMoney".Localize(true), 3f, 0f, 0f, true);
				this._escaping = true;
			}
			else if (base.TurnCounter >= 4)
			{
				yield return PerformAction.Chat(this, "Chat.FraudRabbitLongTurn".Localize(true), 3f, 0f, 0f, true);
				this._escaping = true;
			}
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			IEnemyMove enemyMove;
			switch (this.Next)
			{
			case FraudRabbit.MoveType.Start:
				enemyMove = new SimpleEnemyMove(Intention.AddCard(), this.Start());
				break;
			case FraudRabbit.MoveType.Debuff:
				enemyMove = new SimpleEnemyMove(Intention.NegativeEffect(null), this.Debuff());
				break;
			case FraudRabbit.MoveType.WannaEscape:
				enemyMove = base.DefendMove(this, base.GetMove(2), 0, 0, base.Defend, true, PerformAction.Animation(this, "defend", 0.3f, null, 0f, -1));
				break;
			case FraudRabbit.MoveType.Escape:
				enemyMove = new SimpleEnemyMove(Intention.Escape(), this.EscapeActions());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield return enemyMove;
			yield break;
		}
		private IEnumerable<BattleAction> Start()
		{
			yield return new EnemyMoveAction(this, base.GetMove(0), true);
			yield return PerformAction.Animation(this, "shoot3", 0.3f, "PhoneCall", 0.1f, -1);
			char c = '1';
			if (Random.Range(0f, 1f) > 0.5f)
			{
				c = '2';
			}
			yield return PerformAction.Chat(this, ("Chat.FraudRabbitStart" + c.ToString()).Localize(true), 3f, 0f, 0f, true);
			yield return new AddCardsToHandAction(Library.CreateCards<Payment>(1, false), AddCardsType.Normal);
			int money = Math.Min(base.GetStatusEffect<PhoneBillSe>().Level, base.GameRun.Money);
			yield return new LoseMoneyAction(money);
			base.GetStatusEffect<PhoneBillSe>().Count += money;
			yield break;
		}
		private bool DebuffType { get; set; }
		private IEnumerable<BattleAction> Debuff()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield return PerformAction.Animation(this, "shoot3", 0.3f, "PhoneCall", 0.1f, -1);
			Type type = (this.DebuffType ? typeof(TempFirepowerNegative) : typeof(TempSpiritNegative));
			yield return new ApplyStatusEffectAction(type, base.Battle.Player, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			this.DebuffType = !this.DebuffType;
			int money = Math.Min(base.GetStatusEffect<PhoneBillSe>().Level, base.GameRun.Money);
			yield return new LoseMoneyAction(money);
			base.GetStatusEffect<PhoneBillSe>().Count += money;
			yield break;
		}
		private IEnumerable<BattleAction> EscapeActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(3), true);
			yield return new EscapeAction(this);
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.EnumerateAllCards(), (Card c) => c is Payment && c.Zone != CardZone.Exile));
			if (list.Count > 1)
			{
				Debug.LogError("Multiple Payment exists");
				Card card = Enumerable.First<Card>(list);
				yield return new ExileCardAction(card);
			}
			else if (list.Count == 1)
			{
				Card card2 = Enumerable.First<Card>(list);
				yield return new ExileCardAction(card2);
			}
			yield break;
		}
		private IEnumerable<BattleAction> OnDied(DieEventArgs arg)
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.EnumerateAllCards(), (Card c) => c is Payment && c.Zone != CardZone.Exile));
			if (list.Count > 1)
			{
				Debug.LogError("Multiple Payment exists");
				Card card = Enumerable.First<Card>(list);
				yield return new ExileCardAction(card);
			}
			else if (list.Count == 1)
			{
				Card card2 = Enumerable.First<Card>(list);
				yield return new ExileCardAction(card2);
			}
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			if (this._escaping)
			{
				this.Next = ((this.Next == FraudRabbit.MoveType.WannaEscape) ? FraudRabbit.MoveType.Escape : FraudRabbit.MoveType.WannaEscape);
				return;
			}
			this.Next = FraudRabbit.MoveType.Debuff;
		}
		private bool _escaping;
		private enum MoveType
		{
			Start,
			Debuff,
			WannaEscape,
			Escape
		}
	}
}
