using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x02000290 RID: 656
	[UsedImplicitly]
	public sealed class HatateShot : Card
	{
		// Token: 0x1700012E RID: 302
		// (get) Token: 0x06000A47 RID: 2631 RVA: 0x000157FC File Offset: 0x000139FC
		[UsedImplicitly]
		public int Special
		{
			get
			{
				if (base.Battle == null)
				{
					return 0;
				}
				if (this.Playing)
				{
					return base.Value1 * base.Battle.BattleMana.Amount;
				}
				return base.Value1 * Math.Max(base.Battle.BattleMana.Amount - base.Cost.Amount, 0);
			}
		}

		// Token: 0x1700012F RID: 303
		// (get) Token: 0x06000A48 RID: 2632 RVA: 0x00015865 File Offset: 0x00013A65
		protected override int AdditionalBlock
		{
			get
			{
				return this.Special;
			}
		}

		// Token: 0x17000130 RID: 304
		// (get) Token: 0x06000A49 RID: 2633 RVA: 0x0001586D File Offset: 0x00013A6D
		// (set) Token: 0x06000A4A RID: 2634 RVA: 0x00015875 File Offset: 0x00013A75
		private bool Playing { get; set; }

		// Token: 0x06000A4B RID: 2635 RVA: 0x0001587E File Offset: 0x00013A7E
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			this.Playing = true;
			yield return base.DefenseAction(true);
			this.Playing = false;
			yield break;
		}
	}
}
