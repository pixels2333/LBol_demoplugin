using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Misfortune
{
	// Token: 0x02000352 RID: 850
	[UsedImplicitly]
	public sealed class Yuebiao : Card
	{
		// Token: 0x06000C57 RID: 3159 RVA: 0x000181EF File Offset: 0x000163EF
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
