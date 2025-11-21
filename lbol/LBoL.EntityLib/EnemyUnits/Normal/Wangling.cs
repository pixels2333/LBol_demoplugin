using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Normal
{
	// Token: 0x020001E4 RID: 484
	[UsedImplicitly]
	public sealed class Wangling : EnemyUnit
	{
		// Token: 0x06000790 RID: 1936 RVA: 0x00010EC8 File Offset: 0x0000F0C8
		public override void OnSpawn(EnemyUnit spawner)
		{
			Yuyuko yuyuko = spawner as Yuyuko;
			if (yuyuko != null)
			{
				this._grazeTarget = yuyuko;
			}
			this.React(PerformAction.Sfx("GhostSpawn", 0f));
		}

		// Token: 0x06000791 RID: 1937 RVA: 0x00010F00 File Offset: 0x0000F100
		private IEnumerable<BattleAction> Graze()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield return PerformAction.Animation(this, "shoot3", 0.3f, null, 0f, -1);
			Unit unit = this._grazeTarget ?? this;
			int? num = new int?(2);
			bool flag = this._grazeTarget.RootIndex <= base.RootIndex;
			yield return new ApplyStatusEffectAction<Graze>(unit, num, default(int?), default(int?), default(int?), 0f, flag);
			yield break;
		}

		// Token: 0x06000792 RID: 1938 RVA: 0x00010F10 File Offset: 0x0000F110
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			if ((base.TurnCounter + base.RootIndex) % 3 == 0)
			{
				yield return new SimpleEnemyMove(Intention.Graze(), this.Graze());
			}
			else
			{
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1 + base.EnemyBattleRng.NextInt(0, base.Damage2), 1, false, "Instant", false);
				if (base.TurnCounter > 1)
				{
					yield return base.NegativeMove(null, typeof(YuyukoDeath), new int?(1), default(int?), true, false, null);
				}
			}
			yield break;
		}

		// Token: 0x04000079 RID: 121
		private EnemyUnit _grazeTarget;
	}
}
