using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Adventure
{
	// Token: 0x020004FC RID: 1276
	[UsedImplicitly]
	public sealed class NewsTidbit : Card
	{
		// Token: 0x170001DA RID: 474
		// (get) Token: 0x060010C4 RID: 4292 RVA: 0x0001D3B7 File Offset: 0x0001B5B7
		public override bool Negative
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060010C5 RID: 4293 RVA: 0x0001D3BA File Offset: 0x0001B5BA
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DebuffAction<LockedOn>(base.Battle.Player, base.Value1, 0, 0, 0, true, 0.2f);
			yield break;
		}
	}
}
