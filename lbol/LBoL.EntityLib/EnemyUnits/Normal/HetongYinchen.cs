using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal.Drones;

namespace LBoL.EntityLib.EnemyUnits.Normal
{
	// Token: 0x020001DA RID: 474
	[UsedImplicitly]
	public sealed class HetongYinchen : EnemyUnit
	{
		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x06000752 RID: 1874 RVA: 0x0001088B File Offset: 0x0000EA8B
		// (set) Token: 0x06000753 RID: 1875 RVA: 0x00010893 File Offset: 0x0000EA93
		private HetongYinchen.MoveType Next { get; set; }

		// Token: 0x06000754 RID: 1876 RVA: 0x0001089C File Offset: 0x0000EA9C
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = HetongYinchen.MoveType.Repair;
			base.CountDown = 5;
			base.ReactBattleEvent<DieEventArgs>(base.Battle.EnemyDied, new Func<DieEventArgs, IEnumerable<BattleAction>>(this.OnEnemyDied));
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x06000755 RID: 1877 RVA: 0x000108F1 File Offset: 0x0000EAF1
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			int? num = new int?(base.Count2);
			yield return new ApplyStatusEffectAction<GuangxueMicai>(this, default(int?), num, default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x06000756 RID: 1878 RVA: 0x00010901 File Offset: 0x0000EB01
		private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs arg)
		{
			if (base.IsAlive)
			{
				Unit unit = arg.Unit;
				if (unit is Drone)
				{
					this._chatFlag = !this._chatFlag;
					string text = "Chat.HetongYinchen";
					if (this._chatFlag)
					{
						text += 1.ToString();
					}
					else
					{
						text += 2.ToString();
					}
					string text2 = string.Format(text.Localize(true), ("Chat." + unit.Id).Localize(true));
					yield return PerformAction.Chat(this, text2, 3f, 0f, 0f, true);
					if (Enumerable.Count<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit e) => e is Drone) == 0)
					{
						this.Next = HetongYinchen.MoveType.Shoot;
						if (!base.IsInTurn)
						{
							base.UpdateTurnMoves();
						}
					}
				}
			}
			yield break;
		}

		// Token: 0x06000757 RID: 1879 RVA: 0x00010918 File Offset: 0x0000EB18
		private IEnumerable<BattleAction> RepairActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			List<EnemyUnit> list = Enumerable.ToList<EnemyUnit>(Enumerable.Where<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit enemy) => enemy is Drone));
			if (list.Count >= 1)
			{
				HetongYinchen.<>c__DisplayClass8_0 CS$<>8__locals1 = new HetongYinchen.<>c__DisplayClass8_0();
				CS$<>8__locals1.target = Enumerable.First<EnemyUnit>(list);
				IEnumerable<EnemyUnit> enumerable = list;
				Func<EnemyUnit, bool> func;
				if ((func = CS$<>8__locals1.<>9__1) == null)
				{
					func = (CS$<>8__locals1.<>9__1 = (EnemyUnit enemy) => enemy.MaxHp - enemy.Hp > CS$<>8__locals1.target.MaxHp - CS$<>8__locals1.target.Hp);
				}
				foreach (EnemyUnit enemyUnit in Enumerable.Where<EnemyUnit>(enumerable, func))
				{
					CS$<>8__locals1.target = enemyUnit;
				}
				yield return PerformAction.Animation(this, "shoot3", 1f, "MetalHit3", 0.8f, -1);
				yield return new ApplyStatusEffectAction<Firepower>(CS$<>8__locals1.target, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
				yield return new HealAction(this, CS$<>8__locals1.target, base.Count1, HealType.Normal, 0.2f);
				CS$<>8__locals1 = null;
			}
			else
			{
				yield return new CastBlockShieldAction(this, this, base.Defend, 0, BlockShieldType.Normal, true);
			}
			yield break;
		}

		// Token: 0x06000758 RID: 1880 RVA: 0x00010928 File Offset: 0x0000EB28
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			HetongYinchen.MoveType next = this.Next;
			IEnemyMove enemyMove;
			if (next != HetongYinchen.MoveType.Shoot)
			{
				if (next != HetongYinchen.MoveType.Repair)
				{
					throw new ArgumentOutOfRangeException();
				}
				enemyMove = new SimpleEnemyMove(Intention.Repair(), this.RepairActions());
			}
			else
			{
				enemyMove = base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 1, false, "Instant", false);
			}
			yield return enemyMove;
			yield break;
		}

		// Token: 0x04000071 RID: 113
		private bool _chatFlag;

		// Token: 0x020006BE RID: 1726
		private enum MoveType
		{
			// Token: 0x0400087D RID: 2173
			Shoot,
			// Token: 0x0400087E RID: 2174
			Repair
		}
	}
}
