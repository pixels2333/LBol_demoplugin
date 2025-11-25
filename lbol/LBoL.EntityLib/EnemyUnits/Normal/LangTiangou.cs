using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal
{
	[UsedImplicitly]
	public sealed class LangTiangou : EnemyUnit
	{
		private LangTiangou.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = LangTiangou.MoveType.AttackDefend;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<PowerByDefense>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<PowerByDefense>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true));
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case LangTiangou.MoveType.AttackDefend:
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage1, 1, false, "Instant", true);
				yield return base.DefendMove(this, null, base.Defend, 0, 0, true, null);
				break;
			case LangTiangou.MoveType.AttackDebuff:
			{
				yield return base.AttackMove(base.GetMove(2), base.Gun3, base.Damage2, 1, false, "Instant", true);
				string text = null;
				Type typeFromHandle = typeof(Vulnerable);
				int? num = new int?(base.Count1);
				yield return base.NegativeMove(text, typeFromHandle, default(int?), num, false, false, null);
				break;
			}
			case LangTiangou.MoveType.DoubleAttack:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, base.Gun1, true);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			switch (this.Next)
			{
			case LangTiangou.MoveType.AttackDefend:
				this.Next = LangTiangou.MoveType.AttackDebuff;
				return;
			case LangTiangou.MoveType.AttackDebuff:
				this.Next = ((base.EnemyMoveRng.NextInt(0, 1) == 0) ? LangTiangou.MoveType.DoubleAttack : LangTiangou.MoveType.AttackDefend);
				return;
			case LangTiangou.MoveType.DoubleAttack:
				this.Next = LangTiangou.MoveType.AttackDefend;
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		private enum MoveType
		{
			AttackDefend,
			AttackDebuff,
			DoubleAttack
		}
	}
}
