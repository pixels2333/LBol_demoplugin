using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal
{
	[UsedImplicitly]
	public sealed class WhiteFairy : EnemyUnit
	{
		private WhiteFairy.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.EnterBattle();
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.EnterBattle();
			if (spawner is Clownpiece)
			{
				this.React(new ApplyStatusEffectAction<Lunatic>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
			}
		}
		private void EnterBattle()
		{
			this.Next = ((base.RootIndex % 2 == 0) ? WhiteFairy.MoveType.DoubleShoot : WhiteFairy.MoveType.ShootAndDefend);
			base.CountDown = ((base.RootIndex % 2 == 0) ? 3 : 2);
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			if (!base.HasStatusEffect<Lunatic>())
			{
				switch (this.Next)
				{
				case WhiteFairy.MoveType.ShootAndDefend:
					yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 1, false, "Instant", false);
					yield return base.DefendMove(this, null, base.Defend, 0, 0, false, null);
					break;
				case WhiteFairy.MoveType.DoubleShoot:
					yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 2, false, "Instant", false);
					break;
				case WhiteFairy.MoveType.DefendAndBuff:
					yield return base.DefendMove(this, base.GetMove(2), base.Defend, 0, 0, true, null);
					yield return base.PositiveMove(null, typeof(Firepower), new int?(base.Power), default(int?), false, null);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			else
			{
				switch (this.Next)
				{
				case WhiteFairy.MoveType.ShootAndDefend:
					yield return base.AttackMove(base.GetMove(0), base.Gun3, base.Damage1, 1, false, "Instant", false);
					yield return base.DefendMove(this, null, 0, base.Defend, 0, false, null);
					break;
				case WhiteFairy.MoveType.DoubleShoot:
					yield return base.AttackMove(base.GetMove(1), base.Gun4, base.Damage2, 2, false, "Instant", false);
					break;
				case WhiteFairy.MoveType.DefendAndBuff:
					yield return base.DefendMove(this, base.GetMove(2), 0, base.Defend, 0, true, null);
					yield return base.PositiveMove(null, typeof(Firepower), new int?(base.Power), default(int?), false, null);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = WhiteFairy.MoveType.DefendAndBuff;
				base.CountDown = base.EnemyMoveRng.NextInt(4, 5);
				return;
			}
			WhiteFairy.MoveType moveType;
			switch (this.Next)
			{
			case WhiteFairy.MoveType.ShootAndDefend:
				moveType = WhiteFairy.MoveType.DoubleShoot;
				break;
			case WhiteFairy.MoveType.DoubleShoot:
				moveType = WhiteFairy.MoveType.ShootAndDefend;
				break;
			case WhiteFairy.MoveType.DefendAndBuff:
				moveType = this._pool.Sample(base.EnemyMoveRng);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			this.Next = moveType;
		}
		private readonly RepeatableRandomPool<WhiteFairy.MoveType> _pool = new RepeatableRandomPool<WhiteFairy.MoveType>
		{
			{
				WhiteFairy.MoveType.ShootAndDefend,
				1f
			},
			{
				WhiteFairy.MoveType.DoubleShoot,
				1f
			}
		};
		private enum MoveType
		{
			ShootAndDefend,
			DoubleShoot,
			DefendAndBuff
		}
	}
}
