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
	[UsedImplicitly]
	public sealed class TianziRock : Card
	{
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
