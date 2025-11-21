using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Enemy
{
	// Token: 0x0200035E RID: 862
	[UsedImplicitly]
	public sealed class Chunguang : Card
	{
		// Token: 0x06000C6C RID: 3180 RVA: 0x0001831A File Offset: 0x0001651A
		public override IEnumerable<BattleAction> OnDraw()
		{
			if (base.Battle.BattleMana.HasTrivial)
			{
				yield return ConvertManaAction.Purify(base.Battle.BattleMana, base.Value1);
			}
			yield break;
		}
	}
}
