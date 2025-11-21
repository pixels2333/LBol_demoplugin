using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.EnemyUnits.Character
{
	// Token: 0x0200024D RID: 589
	[UsedImplicitly]
	public sealed class Yiji : EnemyUnit
	{
		// Token: 0x06000974 RID: 2420 RVA: 0x0001462C File Offset: 0x0001282C
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x06000975 RID: 2421 RVA: 0x0001464B File Offset: 0x0001284B
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			int? num = new int?(4);
			yield return new ApplyStatusEffectAction<BossAct>(this, default(int?), default(int?), default(int?), num, 1f, true);
			yield break;
		}

		// Token: 0x06000976 RID: 2422 RVA: 0x0001465B File Offset: 0x0001285B
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			yield return base.AttackMove(base.GetMove(0), base.Gun1, 10 + base.TurnCounter, 2, false, "Instant", false);
			yield break;
		}
	}
}
