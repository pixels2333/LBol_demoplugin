using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal
{
	[UsedImplicitly]
	public sealed class BlackFairy : EnemyUnit
	{
		private BlackFairy.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.EnterBattle();
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private void EnterBattle()
		{
			this.Next = BlackFairy.MoveType.ShootCards;
			base.CountDown = ((base.RootIndex % 2 == 0) ? 2 : 3);
			this.HighDamageTurn = 1;
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new CastBlockShieldAction(this, this, 0, base.Count1, BlockShieldType.Normal, false);
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.EnterBattle();
			if (spawner is Clownpiece)
			{
				this.React(new ApplyStatusEffectAction<LBoL.EntityLib.StatusEffects.Enemy.Lunatic>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
			}
			this.React(new CastBlockShieldAction(this, this, 0, base.Count1, BlockShieldType.Normal, false));
		}
		private IEnumerable<BattleAction> BuffActions()
		{
			if (base.HasStatusEffect<LBoL.EntityLib.StatusEffects.Enemy.Lunatic>())
			{
				yield return new ApplyStatusEffectAction<Firepower>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			}
			else
			{
				yield return new ApplyStatusEffectAction<LBoL.EntityLib.StatusEffects.Enemy.Lunatic>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			}
			base.CountDown = base.EnemyMoveRng.NextInt(4, 5);
			this.HighDamageTurn = base.EnemyMoveRng.NextInt(1, 3);
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			if (!base.HasStatusEffect<LBoL.EntityLib.StatusEffects.Enemy.Lunatic>())
			{
				switch (this.Next)
				{
				case BlackFairy.MoveType.ShootCards:
					yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 1, false, "Instant", false);
					yield return base.AddCardMove(null, Library.CreateCards<BlackResidue>(1, false), EnemyUnit.AddCardZone.Discard, null, false);
					break;
				case BlackFairy.MoveType.ShootAccuracy:
					yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 2, true, "Instant", false);
					break;
				case BlackFairy.MoveType.DefendBuff:
					yield return base.DefendMove(this, base.GetMove(2), base.Defend, 0, 0, true, null);
					yield return new SimpleEnemyMove(Intention.PositiveEffect(), this.BuffActions());
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			else
			{
				switch (this.Next)
				{
				case BlackFairy.MoveType.ShootCards:
					yield return base.AttackMove(base.GetMove(0), base.Gun3, base.Damage1, 1, false, "Instant", false);
					yield return base.AddCardMove(null, Library.CreateCards<BlackResidue>(2, false), EnemyUnit.AddCardZone.Discard, null, false);
					break;
				case BlackFairy.MoveType.ShootAccuracy:
					yield return base.AttackMove(base.GetMove(1), base.Gun4, base.Damage2 + base.Power, 2, true, "Instant", false);
					break;
				case BlackFairy.MoveType.DefendBuff:
					yield return base.DefendMove(this, base.GetMove(2), 0, base.Defend, 0, true, null);
					yield return new SimpleEnemyMove(Intention.PositiveEffect(), this.BuffActions());
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			yield break;
		}
		private int HighDamageTurn { get; set; }
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = BlackFairy.MoveType.DefendBuff;
				return;
			}
			if (base.CountDown == this.HighDamageTurn)
			{
				this.Next = BlackFairy.MoveType.ShootAccuracy;
				return;
			}
			this.Next = BlackFairy.MoveType.ShootCards;
		}
		private enum MoveType
		{
			ShootCards,
			ShootAccuracy,
			DefendBuff
		}
	}
}
