using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal
{
	[UsedImplicitly]
	public sealed class YaTiangou : EnemyUnit
	{
		private YaTiangou.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = YaTiangou.MoveType.Graze;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<Graze>(this, new int?(base.Defend), default(int?), default(int?), default(int?), 0f, true);
			yield return new ApplyStatusEffectAction<FastAttack>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<Graze>(this, new int?(base.Defend), default(int?), default(int?), default(int?), 0f, true));
			this.React(new ApplyStatusEffectAction<FastAttack>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true));
		}
		private IEnumerable<BattleAction> GrazeToAll()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield return PerformAction.Animation(this, "defend", 0.3f, null, 0f, -1);
			foreach (EnemyUnit enemyUnit in base.AllAliveEnemies)
			{
				Unit unit = enemyUnit;
				int? num = new int?(base.Defend);
				bool flag = enemyUnit.RootIndex <= base.RootIndex;
				yield return new ApplyStatusEffectAction<Graze>(unit, num, default(int?), default(int?), default(int?), 0f, flag);
			}
			IEnumerator<EnemyUnit> enumerator = null;
			yield break;
			yield break;
		}
		private IEnumerable<BattleAction> BuffActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(2), true);
			yield return PerformAction.Animation(this, "shoot2", 0.3f, null, 0f, -1);
			yield return new ApplyStatusEffectAction<FastAttack>(this, new int?(base.Power / 2), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case YaTiangou.MoveType.Attack:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", true);
				break;
			case YaTiangou.MoveType.Graze:
				yield return new SimpleEnemyMove(Intention.Graze().WithMoveName(base.GetMove(1)), this.GrazeToAll());
				break;
			case YaTiangou.MoveType.Buff:
				yield return new SimpleEnemyMove(Intention.PositiveEffect().WithMoveName(base.GetMove(2)), this.BuffActions());
				yield return base.AddCardMove(null, typeof(AyaNews), base.Count1, EnemyUnit.AddCardZone.Draw, null, false);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			int turnCounter = base.TurnCounter;
			YaTiangou.MoveType moveType;
			if (turnCounter < 3)
			{
				if (turnCounter != 1)
				{
					if (turnCounter != 2)
					{
						moveType = this.Next;
					}
					else
					{
						moveType = YaTiangou.MoveType.Buff;
					}
				}
				else
				{
					moveType = YaTiangou.MoveType.Attack;
				}
			}
			else
			{
				YaTiangou.MoveType moveType2;
				switch ((base.TurnCounter - 3) % 4)
				{
				case 0:
				case 2:
					moveType2 = YaTiangou.MoveType.Attack;
					break;
				case 1:
					moveType2 = YaTiangou.MoveType.Graze;
					break;
				case 3:
					moveType2 = YaTiangou.MoveType.Buff;
					break;
				default:
					moveType2 = this.Next;
					break;
				}
				moveType = moveType2;
			}
			this.Next = moveType;
		}
		private enum MoveType
		{
			Attack,
			Graze,
			Buff
		}
	}
}
