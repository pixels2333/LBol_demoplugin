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
	[UsedImplicitly]
	public sealed class ReimuSilence : Card
	{
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
