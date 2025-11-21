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
	// Token: 0x02000436 RID: 1078
	[UsedImplicitly]
	public sealed class PoisonPotion : Card
	{
		// Token: 0x06000EB9 RID: 3769 RVA: 0x0001ADC5 File Offset: 0x00018FC5
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<PoisonPotionSe>(base.Value1, 0, 0, 0, 0.2f);
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<Potion>(base.Value2, false), DrawZoneTarget.Random, AddCardsType.Normal);
			yield break;
		}
	}
}
