using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal.Maoyus
{
	[UsedImplicitly]
	public abstract class MaoyuOrigin : EnemyUnit
	{
		protected virtual Type DebuffType
		{
			get
			{
				return typeof(Fragil);
			}
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			this.SetFirstTurn();
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private void SetFirstTurn()
		{
			if (Enumerable.Count<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit enemy) => enemy is MaoyuOrigin) >= 3)
			{
				List<MaoyuOrigin> list = new List<MaoyuOrigin>();
				foreach (EnemyUnit enemyUnit in base.AllAliveEnemies)
				{
					MaoyuOrigin maoyuOrigin = enemyUnit as MaoyuOrigin;
					if (maoyuOrigin != null)
					{
						list.Add(maoyuOrigin);
					}
				}
				if (Enumerable.First<MaoyuOrigin>(list) != this)
				{
					return;
				}
				list.Shuffle(base.EnemyMoveRng);
				using (IEnumerator<ValueTuple<int, MaoyuOrigin>> enumerator2 = list.WithIndices<MaoyuOrigin>().GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ValueTuple<int, MaoyuOrigin> valueTuple = enumerator2.Current;
						int item = valueTuple.Item1;
						MaoyuOrigin item2 = valueTuple.Item2;
						item2.CountDown = item % 3;
						item2.Next = ((item2.CountDown <= 0) ? MaoyuOrigin.MoveType.OtherMove : MaoyuOrigin.MoveType.Shoot);
					}
					return;
				}
			}
			base.CountDown = base.EnemyMoveRng.NextInt(0, 2);
			this.Next = ((base.CountDown <= 0) ? MaoyuOrigin.MoveType.OtherMove : MaoyuOrigin.MoveType.Shoot);
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<MaoyuBlock>(this, new int?(base.Defend + base.EnemyBattleRng.NextInt(0, 2)), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<MaoyuBlock>(this, new int?(base.Defend + base.EnemyBattleRng.NextInt(0, 2)), default(int?), default(int?), default(int?), 0f, true));
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			MaoyuOrigin.MoveType next = this.Next;
			if (next != MaoyuOrigin.MoveType.Shoot)
			{
				if (next != MaoyuOrigin.MoveType.OtherMove)
				{
					throw new ArgumentOutOfRangeException();
				}
				IEnemyMove enemyMove;
				if (base.Power <= 0)
				{
					string move = base.GetMove(1);
					Type debuffType = this.DebuffType;
					int? num = new int?(base.Count1);
					PerformAction performAction = PerformAction.Animation(this, "shoot3", 0.3f, null, 0f, -1);
					enemyMove = base.NegativeMove(move, debuffType, default(int?), num, false, false, performAction);
				}
				else
				{
					string move2 = base.GetMove(1);
					Type typeFromHandle = typeof(Firepower);
					int? num2 = new int?(base.Power);
					PerformAction performAction = PerformAction.Animation(this, "defend", 0.3f, null, 0f, -1);
					int? num = default(int?);
					enemyMove = base.PositiveMove(move2, typeFromHandle, num2, num, false, performAction);
				}
				yield return enemyMove;
				base.CountDown = base.EnemyMoveRng.NextInt(2, 3);
				if (base.Power > 0)
				{
					int num3 = base.CountDown + 1;
					base.CountDown = num3;
				}
			}
			else
			{
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1 + base.EnemyBattleRng.NextInt(0, 2), 1, false, "Instant", false);
			}
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			this.Next = ((base.CountDown <= 0) ? MaoyuOrigin.MoveType.OtherMove : MaoyuOrigin.MoveType.Shoot);
		}
		private MaoyuOrigin.MoveType Next { get; set; }
		private enum MoveType
		{
			Shoot,
			OtherMove
		}
	}
}
