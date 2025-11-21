using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.EnemyUnits.Normal
{
	// Token: 0x020001E1 RID: 481
	[UsedImplicitly]
	public abstract class MaskOrigin : EnemyUnit
	{
		// Token: 0x06000781 RID: 1921 RVA: 0x00010D70 File Offset: 0x0000EF70
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x06000782 RID: 1922 RVA: 0x00010D8F File Offset: 0x0000EF8F
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<Servant>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x06000783 RID: 1923 RVA: 0x00010D9F File Offset: 0x0000EF9F
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(PerformAction.Sfx("GhostSpawn", 0f));
		}

		// Token: 0x06000784 RID: 1924 RVA: 0x00010DBB File Offset: 0x0000EFBB
		private IEnumerable<BattleAction> Debuff()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield return PerformAction.Animation(this, "shoot3", 0.3f, null, 0f, -1);
			if (base.EnemyBattleRng.NextInt(0, 1) == 0)
			{
				yield return new ApplyStatusEffectAction<TempFirepowerNegative>(base.Battle.Player, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			}
			else
			{
				yield return new ApplyStatusEffectAction<TempSpiritNegative>(base.Battle.Player, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}

		// Token: 0x06000785 RID: 1925 RVA: 0x00010DCB File Offset: 0x0000EFCB
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			if ((base.TurnCounter + base.RootIndex) % 3 == 0)
			{
				yield return new SimpleEnemyMove(Intention.NegativeEffect(null), this.Debuff());
			}
			else
			{
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1 + base.EnemyBattleRng.NextInt(0, base.Damage2), 1, false, "Instant", false);
			}
			yield break;
		}
	}
}
