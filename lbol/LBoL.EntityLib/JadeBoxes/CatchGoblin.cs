using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;

namespace LBoL.EntityLib.JadeBoxes
{
	// Token: 0x0200010F RID: 271
	[UsedImplicitly]
	public sealed class CatchGoblin : JadeBox
	{
		// Token: 0x060003B6 RID: 950 RVA: 0x0000A5D7 File Offset: 0x000087D7
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}

		// Token: 0x060003B7 RID: 951 RVA: 0x0000A5F6 File Offset: 0x000087F6
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			Unit randomAliveEnemy = base.Battle.RandomAliveEnemy;
			int? num = new int?(base.Value1);
			yield return new ApplyStatusEffectAction<CatchGoblinSe>(randomAliveEnemy, default(int?), num, default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
