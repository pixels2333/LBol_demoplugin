using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;

namespace LBoL.EntityLib.EnemyUnits.Normal.Bats
{
	// Token: 0x0200020B RID: 523
	[UsedImplicitly]
	public sealed class Bat : BatOrigin
	{
		// Token: 0x06000842 RID: 2114 RVA: 0x0001242D File Offset: 0x0001062D
		protected override IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			foreach (BattleAction battleAction in base.OnBattleStarted(arg))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}
	}
}
