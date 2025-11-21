using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	// Token: 0x02000322 RID: 802
	[UsedImplicitly]
	public sealed class ShuimiUnderWater : Card
	{
		// Token: 0x06000BCE RID: 3022 RVA: 0x00017700 File Offset: 0x00015900
		public override IEnumerable<BattleAction> OnDiscard(CardZone srcZone)
		{
			yield return new GainManaAction(base.Mana);
			yield break;
		}

		// Token: 0x06000BCF RID: 3023 RVA: 0x00017710 File Offset: 0x00015910
		public override IEnumerable<BattleAction> OnExile(CardZone srcZone)
		{
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
