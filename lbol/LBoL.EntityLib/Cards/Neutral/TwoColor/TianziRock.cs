using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002B3 RID: 691
	[UsedImplicitly]
	public sealed class TianziRock : Card
	{
		// Token: 0x06000AA6 RID: 2726 RVA: 0x00015F6A File Offset: 0x0001416A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<TianziRockSe>(base.Value1, 0, this.IsUpgraded ? 1 : 0, 0, 0.2f);
			LockedOn lockedOn = base.Battle.Player.GetStatusEffect<LockedOn>();
			if (lockedOn != null)
			{
				yield return PerformAction.Sfx("JingHua", 0f);
				yield return new RemoveStatusEffectAction(lockedOn, true, 0.1f);
			}
			yield break;
		}
	}
}
