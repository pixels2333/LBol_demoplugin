using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal.Ravens
{
	[UsedImplicitly]
	public abstract class Raven : EnemyUnit
	{
		private Raven.MoveType Next { get; set; }
		protected virtual bool HasGraze
		{
			get
			{
				return true;
			}
		}
		private int GrazeCountDown { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.RootIndex == 1)
			{
				Raven raven = null;
				foreach (EnemyUnit enemyUnit in base.AllAliveEnemies)
				{
					Raven raven2 = enemyUnit as Raven;
					if (raven2 != null)
					{
						raven = raven2;
						break;
					}
				}
				if (raven != null)
				{
					if (raven.Next == Raven.MoveType.AddCard)
					{
						this.Next = Raven.MoveType.Shoot;
						base.CountDown = 1;
					}
					else
					{
						this.Next = Raven.MoveType.AddCard;
					}
				}
				else
				{
					this.NormalStart();
				}
			}
			else
			{
				this.NormalStart();
			}
			if (this.HasGraze)
			{
				this.GrazeCountDown = base.EnemyMoveRng.NextInt(1, 3);
			}
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private void NormalStart()
		{
			if (base.EnemyMoveRng.NextInt(0, 1) == 0)
			{
				this.Next = Raven.MoveType.Shoot;
				base.CountDown = 1;
				return;
			}
			this.Next = Raven.MoveType.AddCard;
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction(typeof(Graze), this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction(typeof(Graze), this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true));
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Raven.MoveType.Shoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1 + base.EnemyBattleRng.NextInt(0, 2), 1, false, "Instant", false);
				break;
			case Raven.MoveType.AddCard:
				yield return new SimpleEnemyMove(Intention.AddCard(), this.News());
				base.CountDown = base.EnemyMoveRng.NextInt(2, 3);
				break;
			case Raven.MoveType.Graze:
				yield return base.DefendMove(this, base.GetMove(2), 0, 0, base.Defend, true, PerformAction.Animation(this, "defend", 0.3f, null, 0f, -1));
				this.GrazeCountDown = base.EnemyMoveRng.NextInt(3, 4);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}
		protected virtual IEnumerable<BattleAction> News()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield return PerformAction.Sfx("Raven", 0f);
			yield return PerformAction.Animation(this, "shoot3", 0.5f, null, 0f, -1);
			yield return new AddCardsToDiscardAction(Library.CreateCards<AyaNews>(base.Count2, false), AddCardsType.Normal);
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			int num;
			if (this.HasGraze)
			{
				num = this.GrazeCountDown - 1;
				this.GrazeCountDown = num;
				if (this.GrazeCountDown <= 0)
				{
					this.Next = Raven.MoveType.Graze;
					return;
				}
			}
			num = base.CountDown - 1;
			base.CountDown = num;
			this.Next = ((base.CountDown <= 0) ? Raven.MoveType.AddCard : Raven.MoveType.Shoot);
		}
		private enum MoveType
		{
			Shoot,
			AddCard,
			Graze
		}
	}
}
