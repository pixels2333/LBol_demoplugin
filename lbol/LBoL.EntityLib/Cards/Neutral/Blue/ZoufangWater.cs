using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	// Token: 0x0200032A RID: 810
	[UsedImplicitly]
	public sealed class ZoufangWater : Card
	{
		// Token: 0x17000158 RID: 344
		// (get) Token: 0x06000BE4 RID: 3044 RVA: 0x0001785C File Offset: 0x00015A5C
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

		// Token: 0x06000BE5 RID: 3045 RVA: 0x000178A9 File Offset: 0x00015AA9
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}
	}
}
