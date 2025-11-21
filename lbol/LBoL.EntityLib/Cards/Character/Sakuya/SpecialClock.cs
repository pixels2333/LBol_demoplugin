using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003BC RID: 956
	[UsedImplicitly]
	public sealed class SpecialClock : Card
	{
		// Token: 0x17000182 RID: 386
		// (get) Token: 0x06000D81 RID: 3457 RVA: 0x00019630 File Offset: 0x00017830
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000D82 RID: 3458 RVA: 0x00019633 File Offset: 0x00017833
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<SpecialClockSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
