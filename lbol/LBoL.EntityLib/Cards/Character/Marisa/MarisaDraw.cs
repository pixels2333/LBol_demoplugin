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
	// Token: 0x0200042C RID: 1068
	[UsedImplicitly]
	public sealed class MarisaDraw : Card
	{
		// Token: 0x06000E9C RID: 3740 RVA: 0x0001AAE9 File Offset: 0x00018CE9
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new DrawManyCardAction(base.Value1);
			if (base.Battle.Player.HasStatusEffect<Burst>())
			{
				yield return new GainManaAction(base.Mana);
			}
			yield break;
		}
	}
}
