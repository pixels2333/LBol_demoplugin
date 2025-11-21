using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.MultiColor;

namespace LBoL.EntityLib.Cards.Neutral.MultiColor
{
	// Token: 0x020002EB RID: 747
	[UsedImplicitly]
	public sealed class BailianFire : Card
	{
		// Token: 0x06000B2D RID: 2861 RVA: 0x00016996 File Offset: 0x00014B96
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<BailianFireSe>(1, 0, 0, 0, 0.2f);
			if (this.IsUpgraded)
			{
				BailianFireSe statusEffect = base.Battle.Player.GetStatusEffect<BailianFireSe>();
				if (statusEffect != null)
				{
					yield return statusEffect.TakeEffect();
				}
			}
			yield break;
		}
	}
}
