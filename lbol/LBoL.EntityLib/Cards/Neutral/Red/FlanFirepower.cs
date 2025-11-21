using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002C4 RID: 708
	[UsedImplicitly]
	public sealed class FlanFirepower : Card
	{
		// Token: 0x17000138 RID: 312
		// (get) Token: 0x06000AD0 RID: 2768 RVA: 0x000162C7 File Offset: 0x000144C7
		[UsedImplicitly]
		public int Count
		{
			get
			{
				if (base.Battle != null)
				{
					return Enumerable.Count<Card>(base.Battle.HandZone, (Card card) => card != this);
				}
				return 0;
			}
		}

		// Token: 0x06000AD1 RID: 2769 RVA: 0x000162EF File Offset: 0x000144EF
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (this.Count > 0)
			{
				yield return base.BuffAction<Firepower>(this.Count, 0, 0, 0, 0.2f);
			}
			using (IEnumerator<Card> enumerator = Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card != this).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Card card2 = enumerator.Current;
					card2.IsEthereal = true;
				}
				yield break;
			}
			yield break;
		}
	}
}
