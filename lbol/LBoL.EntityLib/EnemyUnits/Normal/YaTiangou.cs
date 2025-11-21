using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Normal
{
	// Token: 0x020001E8 RID: 488
	[UsedImplicitly]
	public sealed class YaTiangou : EnemyUnit
	{
		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x060007AE RID: 1966 RVA: 0x00011281 File Offset: 0x0000F481
		// (set) Token: 0x060007AF RID: 1967 RVA: 0x00011289 File Offset: 0x0000F489
		private YaTiangou.MoveType Next { get; set; }

		// Token: 0x060007B0 RID: 1968 RVA: 0x00011292 File Offset: 0x0000F492
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = YaTiangou.MoveType.Graze;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x060007B1 RID: 1969 RVA: 0x000112B8 File Offset: 0x0000F4B8
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<Graze>(this, new int?(base.Defend), default(int?), default(int?), default(int?), 0f, true);
			yield return new ApplyStatusEffectAction<FastAttack>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x060007B2 RID: 1970 RVA: 0x000112C8 File Offset: 0x0000F4C8
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<Graze>(this, new int?(base.Defend), default(int?), default(int?), default(int?), 0f, true));
			this.React(new ApplyStatusEffectAction<FastAttack>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x060007B3 RID: 1971 RVA: 0x0001134F File Offset: 0x0000F54F
		private IEnumerable<BattleAction> GrazeToAll()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield return PerformAction.Animation(this, "defend", 0.3f, null, 0f, -1);
			foreach (EnemyUnit enemyUnit in base.AllAliveEnemies)
			{
				Unit unit = enemyUnit;
				int? num = new int?(base.Defend);
				bool flag = enemyUnit.RootIndex <= base.RootIndex;
				yield return new ApplyStatusEffectAction<Graze>(unit, num, default(int?), default(int?), default(int?), 0f, flag);
			}
			IEnumerator<EnemyUnit> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x060007B4 RID: 1972 RVA: 0x0001135F File Offset: 0x0000F55F
		private IEnumerable<BattleAction> BuffActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(2), true);
			yield return PerformAction.Animation(this, "shoot2", 0.3f, null, 0f, -1);
			yield return new ApplyStatusEffectAction<FastAttack>(this, new int?(base.Power / 2), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x060007B5 RID: 1973 RVA: 0x0001136F File Offset: 0x0000F56F
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case YaTiangou.MoveType.Attack:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", true);
				break;
			case YaTiangou.MoveType.Graze:
				yield return new SimpleEnemyMove(Intention.Graze().WithMoveName(base.GetMove(1)), this.GrazeToAll());
				break;
			case YaTiangou.MoveType.Buff:
				yield return new SimpleEnemyMove(Intention.PositiveEffect().WithMoveName(base.GetMove(2)), this.BuffActions());
				yield return base.AddCardMove(null, typeof(AyaNews), base.Count1, EnemyUnit.AddCardZone.Draw, null, false);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}

		// Token: 0x060007B6 RID: 1974 RVA: 0x00011380 File Offset: 0x0000F580
		protected override void UpdateMoveCounters()
		{
			int turnCounter = base.TurnCounter;
			YaTiangou.MoveType moveType;
			if (turnCounter < 3)
			{
				if (turnCounter != 1)
				{
					if (turnCounter != 2)
					{
						moveType = this.Next;
					}
					else
					{
						moveType = YaTiangou.MoveType.Buff;
					}
				}
				else
				{
					moveType = YaTiangou.MoveType.Attack;
				}
			}
			else
			{
				YaTiangou.MoveType moveType2;
				switch ((base.TurnCounter - 3) % 4)
				{
				case 0:
				case 2:
					moveType2 = YaTiangou.MoveType.Attack;
					break;
				case 1:
					moveType2 = YaTiangou.MoveType.Graze;
					break;
				case 3:
					moveType2 = YaTiangou.MoveType.Buff;
					break;
				default:
					moveType2 = this.Next;
					break;
				}
				moveType = moveType2;
			}
			this.Next = moveType;
		}

		// Token: 0x020006EA RID: 1770
		private enum MoveType
		{
			// Token: 0x04000930 RID: 2352
			Attack,
			// Token: 0x04000931 RID: 2353
			Graze,
			// Token: 0x04000932 RID: 2354
			Buff
		}
	}
}
