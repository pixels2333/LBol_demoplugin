using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;
namespace LBoL.EntityLib.EnemyUnits.Normal.Shenlings
{
	[UsedImplicitly]
	public abstract class Shenling : EnemyUnit
	{
		private Shenling.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = ((base.RootIndex % 2 == 0) ? Shenling.MoveType.DoubleShoot : Shenling.MoveType.ShootAndBuff);
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		protected virtual IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<Amulet>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			yield return new CastBlockShieldAction(this, this, 0, base.MaxHp, BlockShieldType.Normal, false);
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<Amulet>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true));
			this.React(new CastBlockShieldAction(this, this, 0, base.MaxHp, BlockShieldType.Normal, false));
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			Shenling.MoveType next = this.Next;
			if (next != Shenling.MoveType.DoubleShoot)
			{
				if (next != Shenling.MoveType.ShootAndBuff)
				{
					throw new ArgumentOutOfRangeException();
				}
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 1, false, "Instant", false);
				yield return base.PositiveMove(null, typeof(Firepower), new int?(base.Power), default(int?), false, null);
			}
			else
			{
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", false);
			}
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			this.Next = ((this.Next == Shenling.MoveType.DoubleShoot) ? Shenling.MoveType.ShootAndBuff : Shenling.MoveType.DoubleShoot);
		}
		private enum MoveType
		{
			DoubleShoot,
			ShootAndBuff
		}
	}
}
