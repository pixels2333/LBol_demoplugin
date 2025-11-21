using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000453 RID: 1107
	[UsedImplicitly]
	public sealed class Yinhe : Card
	{
		// Token: 0x06000F06 RID: 3846 RVA: 0x0001B323 File Offset: 0x00019523
		public override IEnumerable<BattleAction> OnRetain()
		{
			if (base.Zone == CardZone.Hand)
			{
				base.DeltaBlock += base.Value1;
			}
			return null;
		}
	}
}
