using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002C1 RID: 705
	[UsedImplicitly]
	public sealed class ZoufangWater : Card
	{
		// Token: 0x17000137 RID: 311
		// (get) Token: 0x06000AC9 RID: 2761 RVA: 0x00016228 File Offset: 0x00014428
		protected override int AdditionalDamage
		{
			get
			{
				if (base.Battle != null)
				{
					return Enumerable.Count<Card>(base.Battle.BattleCardUsageHistory, (Card card) => card.CardType == CardType.Ability) * base.Value2;
				}
				return 0;
			}
		}

		// Token: 0x06000ACA RID: 2762 RVA: 0x00016275 File Offset: 0x00014475
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}
	}
}
