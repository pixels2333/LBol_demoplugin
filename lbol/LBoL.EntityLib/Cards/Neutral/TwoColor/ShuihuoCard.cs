using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Character.Reimu;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002AE RID: 686
	[UsedImplicitly]
	public sealed class ShuihuoCard : YinyangCardBase
	{
		// Token: 0x06000A98 RID: 2712 RVA: 0x00015DF7 File Offset: 0x00013FF7
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return base.DefenseAction(false);
			yield break;
		}

		// Token: 0x06000A99 RID: 2713 RVA: 0x00015E0E File Offset: 0x0001400E
		public override IEnumerable<BattleAction> OnRetain()
		{
			if (base.Zone == CardZone.Hand)
			{
				base.DeltaDamage += base.Value1;
				base.DeltaBlock += base.Value1;
			}
			return null;
		}
	}
}
