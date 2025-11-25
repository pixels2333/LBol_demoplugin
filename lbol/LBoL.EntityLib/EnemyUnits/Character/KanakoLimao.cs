using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal.Shenlings;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Character
{
	[UsedImplicitly]
	public sealed class KanakoLimao : EnemyUnit
	{
		private KanakoLimao.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = KanakoLimao.MoveType.DoubleShoot;
			this._summonRootIndex = -1;
			this._summonType = ((base.EnemyBattleRng.NextInt(0, 1) == 0) ? typeof(ShenlingPurple) : typeof(ShenlingWhite));
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			base.ReactBattleEvent<DieEventArgs>(base.Dying, new Func<DieEventArgs, IEnumerable<BattleAction>>(this.OnDying));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return PerformAction.Chat(this, "Chat.Limao1".Localize(true), 3f, 0f, 1f, true);
			yield return PerformAction.Effect(this, "Bianshen", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			yield return PerformAction.Sfx("Graze", 0f);
			yield return PerformAction.TransformModel(this, base.Id);
			yield return new ApplyStatusEffectAction<LimaoDisguiser>(this, default(int?), default(int?), default(int?), default(int?), 0.5f, true);
			if (base.Difficulty == GameDifficulty.Lunatic)
			{
				if (Enumerable.Any<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit enemy) => enemy is SuwakoLimao))
				{
					foreach (BattleAction battleAction in this.SummonActions())
					{
						yield return battleAction;
					}
					IEnumerator<BattleAction> enumerator = null;
				}
			}
			yield break;
			yield break;
		}
		private IEnumerable<BattleAction> OnDying(DieEventArgs arg)
		{
			yield return PerformAction.Effect(this, "Bianshen", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			yield return PerformAction.Sfx("Graze", 0f);
			yield return PerformAction.TransformModel(this, "Limao");
			yield return PerformAction.DeathAnimation(this);
			yield break;
		}
		private IEnumerable<BattleAction> SummonActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(2), true);
			if (Enumerable.Count<EnemyUnit>(base.AllAliveEnemies) <= 3)
			{
				yield return PerformAction.Animation(this, "shoot2", 0f, null, 0f, -1);
				int num = -1;
				for (int i = 0; i < 4; i++)
				{
					this._summonRootIndex++;
					this._summonRootIndex %= 4;
					if (!base.Battle.IsAnyoneInRootIndex(this._summonRootIndex))
					{
						num = this._summonRootIndex;
						break;
					}
				}
				if (num < 0)
				{
					throw new InvalidOperationException("Kanako trying to summon when there is no place.");
				}
				yield return new SpawnEnemyAction(this, this._summonType, num, 0f, 0.3f, true);
				this._summonType = ((this._summonType == typeof(ShenlingPurple)) ? typeof(ShenlingWhite) : typeof(ShenlingPurple));
			}
			this._summonedOnce = true;
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case KanakoLimao.MoveType.DoubleShoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", false);
				break;
			case KanakoLimao.MoveType.Spawn:
				yield return new SimpleEnemyMove(Intention.Spawn(), this.SummonActions());
				if (this._summonedOnce)
				{
					yield return base.PositiveMove(null, typeof(Firepower), new int?(base.Power), default(int?), false, null);
				}
				break;
			case KanakoLimao.MoveType.ShootAndDefend:
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 1, false, "Instant", false);
				yield return base.DefendMove(this, null, 0, base.Defend, 0, false, null);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			KanakoLimao.MoveType moveType;
			switch (this.Next)
			{
			case KanakoLimao.MoveType.DoubleShoot:
				moveType = KanakoLimao.MoveType.Spawn;
				break;
			case KanakoLimao.MoveType.Spawn:
				moveType = KanakoLimao.MoveType.ShootAndDefend;
				break;
			case KanakoLimao.MoveType.ShootAndDefend:
				moveType = KanakoLimao.MoveType.DoubleShoot;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			this.Next = moveType;
		}
		private Type _summonType;
		private int _summonRootIndex;
		private bool _summonedOnce;
		private enum MoveType
		{
			DoubleShoot,
			Spawn,
			ShootAndDefend
		}
	}
}
