using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x02000332 RID: 818
	[UsedImplicitly]
	public sealed class HinaAttack : Card
	{
		// Token: 0x17000159 RID: 345
		// (get) Token: 0x06000BF6 RID: 3062 RVA: 0x0001799E File Offset: 0x00015B9E
		protected override int AdditionalValue1
		{
			get
			{
				if (base.GameRun != null)
				{
					return Enumerable.Count<Card>(base.GameRun.BaseDeck, (Card card) => card.CardType == CardType.Misfortune);
				}
				return 0;
			}
		}

		// Token: 0x06000BF7 RID: 3063 RVA: 0x000179D9 File Offset: 0x00015BD9
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}
	}
}
