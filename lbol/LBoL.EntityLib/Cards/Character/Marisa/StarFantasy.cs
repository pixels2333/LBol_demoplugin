using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000449 RID: 1097
	[UsedImplicitly]
	public sealed class StarFantasy : Card
	{
		// Token: 0x06000EEE RID: 3822 RVA: 0x0001B147 File Offset: 0x00019347
		public override IEnumerable<BattleAction> OnRetain()
		{
			if (base.Zone == CardZone.Hand)
			{
				base.DeltaDamage += base.Value1;
			}
			return null;
		}
	}
}
