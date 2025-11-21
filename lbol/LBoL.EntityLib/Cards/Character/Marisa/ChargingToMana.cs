using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000414 RID: 1044
	[UsedImplicitly]
	public sealed class ChargingToMana : Card
	{
		// Token: 0x06000E60 RID: 3680 RVA: 0x0001A6B4 File Offset: 0x000188B4
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Charging statusEffect = base.Battle.Player.GetStatusEffect<Charging>();
			if (statusEffect == null)
			{
				yield break;
			}
			int level = statusEffect.Level;
			yield return new RemoveStatusEffectAction(statusEffect, true, 0.1f);
			yield return new GainManaAction(base.Mana * level);
			yield return new DrawManyCardAction(level);
			yield break;
		}
	}
}
