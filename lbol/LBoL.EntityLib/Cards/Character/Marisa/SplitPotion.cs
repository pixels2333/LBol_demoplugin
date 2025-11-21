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
	// Token: 0x02000448 RID: 1096
	[UsedImplicitly]
	public sealed class SplitPotion : Card
	{
		// Token: 0x06000EEC RID: 3820 RVA: 0x0001B12F File Offset: 0x0001932F
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<SplitPotionSe>(1, 0, 0, 0, 0.2f);
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<Potion>(base.Value1, false), DrawZoneTarget.Random, AddCardsType.Normal);
			yield break;
		}
	}
}
