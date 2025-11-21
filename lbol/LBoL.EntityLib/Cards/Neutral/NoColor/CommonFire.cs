using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.Green;

namespace LBoL.EntityLib.Cards.Neutral.NoColor
{
	// Token: 0x020002DB RID: 731
	[UsedImplicitly]
	public sealed class CommonFire : Card
	{
		// Token: 0x06000B0F RID: 2831 RVA: 0x00016759 File Offset: 0x00014959
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<CommonFireSe>(base.Value1, 0, 0, 0, 0.2f);
			if (this.IsUpgraded)
			{
				yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<CManaCard>() });
			}
			yield break;
		}
	}
}
