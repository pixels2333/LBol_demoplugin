using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Normal.Shenlings
{
	// Token: 0x020001F0 RID: 496
	[UsedImplicitly]
	public sealed class ShenlingPurple : Shenling
	{
		// Token: 0x060007D6 RID: 2006 RVA: 0x00011774 File Offset: 0x0000F974
		protected override IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			foreach (BattleAction battleAction in base.OnBattleStarted(arg))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield return new ApplyStatusEffectAction<ShenlingHp>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
			yield break;
		}
	}
}
