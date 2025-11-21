using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal.Drones;

namespace LBoL.EntityLib.EnemyUnits.Character
{
	// Token: 0x02000242 RID: 578
	[UsedImplicitly]
	public sealed class Nitori : EnemyUnit
	{
		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x060008F8 RID: 2296 RVA: 0x00013632 File Offset: 0x00011832
		// (set) Token: 0x060008F9 RID: 2297 RVA: 0x0001363A File Offset: 0x0001183A
		private Nitori.MoveType Next { get; set; }

		// Token: 0x060008FA RID: 2298 RVA: 0x00013644 File Offset: 0x00011844
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Nitori.MoveType.Summon;
			base.CountDown = 3;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			base.ReactBattleEvent<DieEventArgs>(base.Battle.EnemyDied, new Func<DieEventArgs, IEnumerable<BattleAction>>(this.OnEnemyDied));
		}

		// Token: 0x060008FB RID: 2299 RVA: 0x00013699 File Offset: 0x00011899
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			int? num = new int?(2);
			yield return new ApplyStatusEffectAction<GuangxueMicai>(this, default(int?), num, default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x060008FC RID: 2300 RVA: 0x000136A9 File Offset: 0x000118A9
		private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs arg)
		{
			if (base.IsAlive && arg.Unit is Drone)
			{
				yield return PerformAction.Chat(this, "Chat.Nitori2".Localize(true), 3f, 0f, 0f, true);
			}
			yield break;
		}

		// Token: 0x060008FD RID: 2301 RVA: 0x000136C0 File Offset: 0x000118C0
		private IEnumerable<BattleAction> SummonActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(0), true);
			yield return PerformAction.Sfx("DroneSummon", 0f);
			yield return PerformAction.Animation(this, "shoot2", 0.3f, null, 0f, -1);
			yield return new SpawnEnemyAction(this, this._droneTypes.Sample(base.EnemyBattleRng), 0, 0f, 0.3f, false);
			yield return PerformAction.Chat(this, "Chat.Nitori1".Localize(true), 3f, 0f, 0f, true);
			yield break;
		}

		// Token: 0x060008FE RID: 2302 RVA: 0x000136D0 File Offset: 0x000118D0
		private IEnumerable<BattleAction> GuangxueMicaiToAll()
		{
			yield return new EnemyMoveAction(this, base.GetMove(2), true);
			yield return PerformAction.Animation(this, "defend", 0.3f, null, 0f, -1);
			if (Enumerable.Count<EnemyUnit>(base.AllAliveEnemies) >= 2)
			{
				foreach (EnemyUnit enemyUnit in base.AllAliveEnemies)
				{
					Unit unit = enemyUnit;
					int? num = new int?(1);
					yield return new ApplyStatusEffectAction<GuangxueMicai>(unit, default(int?), num, default(int?), default(int?), 0f, true);
				}
				IEnumerator<EnemyUnit> enumerator = null;
			}
			else
			{
				int? num = new int?(2);
				yield return new ApplyStatusEffectAction<GuangxueMicai>(this, default(int?), num, default(int?), default(int?), 0f, true);
			}
			yield break;
			yield break;
		}

		// Token: 0x060008FF RID: 2303 RVA: 0x000136E0 File Offset: 0x000118E0
		private IEnumerable<BattleAction> RepairActions()
		{
			if (Enumerable.Count<EnemyUnit>(base.AllAliveEnemies) == 1)
			{
				yield return new ApplyStatusEffectAction<Firepower>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			}
			EnemyUnit drone = Enumerable.FirstOrDefault<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit unit) => unit is Drone);
			if (drone != null)
			{
				yield return PerformAction.Animation(this, "shoot3", 0.5f, "MetalHit3", 0.2f, -1);
				yield return new HealAction(this, drone, base.Count1, HealType.Normal, 0.2f);
			}
			yield break;
		}

		// Token: 0x06000900 RID: 2304 RVA: 0x000136F0 File Offset: 0x000118F0
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Nitori.MoveType.Summon:
				yield return new SimpleEnemyMove(Intention.SpawnDrone().WithMoveName(base.GetMove(0)), this.SummonActions());
				this.Next = Nitori.MoveType.Shoot;
				break;
			case Nitori.MoveType.Shoot:
				yield return base.AttackMove(base.GetMove(1), base.Gun1, base.Damage1, 2, false, "Instant", true);
				break;
			case Nitori.MoveType.Defend:
				yield return new SimpleEnemyMove(Intention.Defend().WithMoveName(base.GetMove(2)), this.GuangxueMicaiToAll());
				if (Enumerable.Count<EnemyUnit>(base.AllAliveEnemies) >= 2)
				{
					yield return new SimpleEnemyMove(Intention.Repair(), this.RepairActions());
				}
				else
				{
					yield return new SimpleEnemyMove(Intention.PositiveEffect(), this.RepairActions());
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}

		// Token: 0x06000901 RID: 2305 RVA: 0x00013700 File Offset: 0x00011900
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = Nitori.MoveType.Defend;
				base.CountDown = base.EnemyMoveRng.NextInt(3, 4);
				return;
			}
			this.Next = Nitori.MoveType.Shoot;
		}

		// Token: 0x040000C0 RID: 192
		private readonly RepeatableRandomPool<Type> _droneTypes = new RepeatableRandomPool<Type>
		{
			{
				typeof(ScoutElite),
				1f
			},
			{
				typeof(PurifierElite),
				1f
			},
			{
				typeof(TerminatorElite),
				1f
			}
		};

		// Token: 0x02000750 RID: 1872
		private enum MoveType
		{
			// Token: 0x04000AF0 RID: 2800
			Summon,
			// Token: 0x04000AF1 RID: 2801
			Shoot,
			// Token: 0x04000AF2 RID: 2802
			Defend
		}
	}
}
