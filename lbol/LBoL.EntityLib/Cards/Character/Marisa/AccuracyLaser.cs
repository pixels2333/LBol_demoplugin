using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x0200040C RID: 1036
	[UsedImplicitly]
	public sealed class AccuracyLaser : Card
	{
		// Token: 0x06000E4E RID: 3662 RVA: 0x0001A58B File Offset: 0x0001878B
		public override IEnumerable<BattleAction> OnRetain()
		{
			if (base.Zone == CardZone.Hand)
			{
				base.DecreaseBaseCost(base.Mana);
			}
			return null;
		}

		// Token: 0x06000E4F RID: 3663 RVA: 0x0001A5A3 File Offset: 0x000187A3
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}
	}
}
