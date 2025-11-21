using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Reimu;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003F7 RID: 1015
	[UsedImplicitly]
	public sealed class ReimuSilence : Card
	{
		// Token: 0x06000E1D RID: 3613 RVA: 0x0001A25A File Offset: 0x0001845A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.Config.Damage != null && base.Config.UpgradedDamage != null)
			{
				yield return base.BuffAction<ReimuSilenceSe>(this.IsUpgraded ? base.Config.UpgradedDamage.Value : base.Config.Damage.Value, 0, base.Value1, 0, 0.2f);
			}
			else
			{
				yield return base.BuffAction<ReimuSilenceSe>(this.IsUpgraded ? 25 : 20, 0, base.Value1, 0, 0.2f);
			}
			yield return new DrawManyCardAction(base.Value1);
			yield break;
		}
	}
}
