using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Normal.Shenlings
{
	// Token: 0x020001F1 RID: 497
	[UsedImplicitly]
	public sealed class ShenlingWhite : Shenling
	{
		// Token: 0x060007D9 RID: 2009 RVA: 0x0001179C File Offset: 0x0000F99C
		protected override IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			foreach (BattleAction battleAction in base.OnBattleStarted(arg))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield return new ApplyStatusEffectAction<ShenlingGold>(this, new int?(base.GameRun.GameRunEventRng.NextInt(base.Count1, base.Count2)), default(int?), default(int?), default(int?), 0f, true);
			yield break;
			yield break;
		}
	}
}
