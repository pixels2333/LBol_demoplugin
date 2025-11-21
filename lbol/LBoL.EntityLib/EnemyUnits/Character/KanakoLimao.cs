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
	// Token: 0x0200023B RID: 571
	[UsedImplicitly]
	public sealed class KanakoLimao : EnemyUnit
	{
		// Token: 0x170000EC RID: 236
		// (get) Token: 0x060008BC RID: 2236 RVA: 0x00012EA2 File Offset: 0x000110A2
		// (set) Token: 0x060008BD RID: 2237 RVA: 0x00012EAA File Offset: 0x000110AA
		private KanakoLimao.MoveType Next { get; set; }

		// Token: 0x060008BE RID: 2238 RVA: 0x00012EB4 File Offset: 0x000110B4
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = KanakoLimao.MoveType.DoubleShoot;
			this._summonRootIndex = -1;
			this._summonType = ((base.EnemyBattleRng.NextInt(0, 1) == 0) ? typeof(ShenlingPurple) : typeof(ShenlingWhite));
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			base.ReactBattleEvent<DieEventArgs>(base.Dying, new Func<DieEventArgs, IEnumerable<BattleAction>>(this.OnDying));
		}

		// Token: 0x060008BF RID: 2239 RVA: 0x00012F2F File Offset: 0x0001112F
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

		// Token: 0x060008C0 RID: 2240 RVA: 0x00012F3F File Offset: 0x0001113F
		private IEnumerable<BattleAction> OnDying(DieEventArgs arg)
		{
			yield return PerformAction.Effect(this, "Bianshen", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			yield return PerformAction.Sfx("Graze", 0f);
			yield return PerformAction.TransformModel(this, "Limao");
			yield return PerformAction.DeathAnimation(this);
			yield break;
		}

		// Token: 0x060008C1 RID: 2241 RVA: 0x00012F4F File Offset: 0x0001114F
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

		// Token: 0x060008C2 RID: 2242 RVA: 0x00012F5F File Offset: 0x0001115F
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

		// Token: 0x060008C3 RID: 2243 RVA: 0x00012F70 File Offset: 0x00011170
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

		// Token: 0x040000AA RID: 170
		private Type _summonType;

		// Token: 0x040000AB RID: 171
		private int _summonRootIndex;

		// Token: 0x040000AC RID: 172
		private bool _summonedOnce;

		// Token: 0x02000735 RID: 1845
		private enum MoveType
		{
			// Token: 0x04000A71 RID: 2673
			DoubleShoot,
			// Token: 0x04000A72 RID: 2674
			Spawn,
			// Token: 0x04000A73 RID: 2675
			ShootAndDefend
		}
	}
}
