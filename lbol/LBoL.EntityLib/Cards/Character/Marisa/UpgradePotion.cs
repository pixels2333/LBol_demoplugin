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
	// Token: 0x02000450 RID: 1104
	[UsedImplicitly]
	public sealed class UpgradePotion : Card
	{
		// Token: 0x06000EFE RID: 3838 RVA: 0x0001B281 File Offset: 0x00019481
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<Potion>(base.Value2, false), DrawZoneTarget.Random, AddCardsType.Normal);
			yield return base.BuffAction<PotionBaseDamageSe>(base.Value1, 0, 0, 0, 0.2f);
			if (this.IsUpgraded)
			{
				yield return base.BuffAction<UpgradePotionSe>(base.Value1, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
