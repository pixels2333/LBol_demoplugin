using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus
{
	public abstract class YinyangyuBlueOrigin : EnemyUnit
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.RootIndex % 2 == 0)
			{
				this.Next = YinyangyuBlueOrigin.MoveType.Shoot;
				base.CountDown = 0;
			}
			else
			{
				this.Next = YinyangyuBlueOrigin.MoveType.Defend;
				base.CountDown = base.EnemyMoveRng.NextInt(2, 3);
			}
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<AbsorbSpirit>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<AbsorbSpirit>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true));
		}
		private IEnumerable<BattleAction> DefendActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			List<EnemyUnit> list = Enumerable.ToList<EnemyUnit>(Enumerable.Where<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit enemy) => enemy != this));
			EnemyUnit enemyUnit = this;
			if (list.Count > 0)
			{
				enemyUnit = list.Sample(base.EnemyMoveRng);
			}
			yield return new CastBlockShieldAction(this, enemyUnit, 0, base.Defend, BlockShieldType.Normal, true);
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			YinyangyuBlueOrigin.MoveType next = this.Next;
			IEnemyMove enemyMove;
			if (next != YinyangyuBlueOrigin.MoveType.Shoot)
			{
				if (next != YinyangyuBlueOrigin.MoveType.Defend)
				{
					throw new ArgumentOutOfRangeException();
				}
				enemyMove = new SimpleEnemyMove(Intention.Defend(), this.DefendActions());
			}
			else
			{
				enemyMove = base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 1, false, "Instant", false);
			}
			yield return enemyMove;
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = YinyangyuBlueOrigin.MoveType.Defend;
				base.CountDown = base.EnemyMoveRng.NextInt(2, 3);
				return;
			}
			this.Next = YinyangyuBlueOrigin.MoveType.Shoot;
		}
		private YinyangyuBlueOrigin.MoveType Next { get; set; }
		private enum MoveType
		{
			Shoot,
			Defend
		}
	}
}
