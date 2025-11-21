using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Marisa;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000413 RID: 1043
	[UsedImplicitly]
	public sealed class ChargingPotion : Card
	{
		// Token: 0x06000E5E RID: 3678 RVA: 0x0001A69C File Offset: 0x0001889C
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<ChargingPotionSe>(base.Value1, 0, 0, 0, 0.2f);
			yield return new AddCardsToHandAction(Library.CreateCards<Potion>(base.Value2, false), AddCardsType.Normal);
			yield break;
		}
	}
}
