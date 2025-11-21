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
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Normal.Guihuos
{
	// Token: 0x020001FD RID: 509
	[UsedImplicitly]
	public abstract class Guihuo : EnemyUnit
	{
		// Token: 0x170000CD RID: 205
		// (get) Token: 0x06000801 RID: 2049 RVA: 0x00011C55 File Offset: 0x0000FE55
		protected virtual Type DebuffType
		{
			get
			{
				return typeof(Weak);
			}
		}

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x06000802 RID: 2050 RVA: 0x00011C61 File Offset: 0x0000FE61
		protected virtual string SkillVFX
		{
			get
			{
				return "GuihuoUskill";
			}
		}

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x06000803 RID: 2051 RVA: 0x00011C68 File Offset: 0x0000FE68
		// (set) Token: 0x06000804 RID: 2052 RVA: 0x00011C70 File Offset: 0x0000FE70
		public Guihuo.MoveType Next { get; set; }

		// Token: 0x06000805 RID: 2053 RVA: 0x00011C79 File Offset: 0x0000FE79
		protected override void OnEnterBattle(BattleController battle)
		{
			this.SetFirstTurn();
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x06000806 RID: 2054 RVA: 0x00011C9E File Offset: 0x0000FE9E
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			int? num = new int?(base.Count1 + base.EnemyBattleRng.NextInt(0, base.Count2));
			int? num2 = new int?(base.EnemyMoveRng.NextInt(3, 5));
			yield return new ApplyStatusEffectAction<DeathExplodeCount>(this, num, default(int?), default(int?), num2, 0f, true);
			yield break;
		}

		// Token: 0x06000807 RID: 2055 RVA: 0x00011CB0 File Offset: 0x0000FEB0
		public override void OnSpawn(EnemyUnit spawner)
		{
			int num = 0;
			Rin rin = spawner as Rin;
			if (rin != null && rin.HasStatusEffect<RinAura>())
			{
				num = rin.GetStatusEffect<RinAura>().Level;
			}
			this.SetFirstTurn();
			this.React(PerformAction.Sfx("GhostSpawn", 0f));
			this.React(new ApplyStatusEffectAction<DeathExplodeNotCount>(this, new int?(base.Count1 + base.EnemyBattleRng.NextInt(0, base.Count2) + num), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x06000808 RID: 2056 RVA: 0x00011D50 File Offset: 0x0000FF50
		private void SetFirstTurn()
		{
			if (Enumerable.Count<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit enemy) => enemy is Guihuo) >= 3)
			{
				List<Guihuo> list = new List<Guihuo>();
				foreach (EnemyUnit enemyUnit in base.AllAliveEnemies)
				{
					Guihuo guihuo = enemyUnit as Guihuo;
					if (guihuo != null)
					{
						list.Add(guihuo);
					}
				}
				if (Enumerable.First<Guihuo>(list) != this)
				{
					return;
				}
				list.Shuffle(base.EnemyMoveRng);
				using (IEnumerator<ValueTuple<int, Guihuo>> enumerator2 = list.WithIndices<Guihuo>().GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ValueTuple<int, Guihuo> valueTuple = enumerator2.Current;
						int item = valueTuple.Item1;
						Guihuo item2 = valueTuple.Item2;
						item2.CountDown = item % 3;
						item2.Next = ((item2.CountDown <= 0) ? Guihuo.MoveType.Debuff : Guihuo.MoveType.Shoot);
					}
					return;
				}
			}
			base.CountDown = base.EnemyMoveRng.NextInt(0, 2);
			this.Next = ((base.CountDown <= 0) ? Guihuo.MoveType.Debuff : Guihuo.MoveType.Shoot);
		}

		// Token: 0x06000809 RID: 2057 RVA: 0x00011E78 File Offset: 0x00010078
		private IEnumerable<BattleAction> Explode()
		{
			yield return new EnemyMoveAction(this, base.GetMove(2), true);
			yield return new ForceKillAction(this, this);
			yield break;
		}

		// Token: 0x0600080A RID: 2058 RVA: 0x00011E88 File Offset: 0x00010088
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Guihuo.MoveType.Shoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1 + base.EnemyBattleRng.NextInt(0, 2), 1, false, "Instant", false);
				break;
			case Guihuo.MoveType.Debuff:
			{
				string move = base.GetMove(1);
				Type debuffType = this.DebuffType;
				int? num = new int?(base.Count2);
				PerformAction performAction = PerformAction.Effect(this, this.SkillVFX, 0f, "GhostDebuff", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return base.NegativeMove(move, debuffType, default(int?), num, false, false, performAction);
				base.CountDown = base.EnemyBattleRng.NextInt(2, 3);
				break;
			}
			case Guihuo.MoveType.Explode:
				yield return new SimpleEnemyMove(Intention.Explode((base.HasStatusEffect<DeathExplodeCount>() ? base.GetStatusEffect<DeathExplodeCount>().Level : base.GetStatusEffect<DeathExplodeNotCount>().Level) * 2), this.Explode());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}

		// Token: 0x0600080B RID: 2059 RVA: 0x00011E98 File Offset: 0x00010098
		protected override void UpdateMoveCounters()
		{
			DeathExplodeCount statusEffect = base.GetStatusEffect<DeathExplodeCount>();
			if (statusEffect != null && statusEffect.Count == 1)
			{
				this.Next = Guihuo.MoveType.Explode;
				return;
			}
			int num = base.CountDown - 1;
			base.CountDown = num;
			this.Next = ((base.CountDown <= 0) ? Guihuo.MoveType.Debuff : Guihuo.MoveType.Shoot);
		}

		// Token: 0x02000705 RID: 1797
		public enum MoveType
		{
			// Token: 0x0400099D RID: 2461
			Shoot,
			// Token: 0x0400099E RID: 2462
			Debuff,
			// Token: 0x0400099F RID: 2463
			Explode
		}
	}
}
