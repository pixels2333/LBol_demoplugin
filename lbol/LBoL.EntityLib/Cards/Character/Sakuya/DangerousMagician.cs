using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x02000386 RID: 902
	[UsedImplicitly]
	public sealed class DangerousMagician : Card
	{
		// Token: 0x06000CDA RID: 3290 RVA: 0x00018AF8 File Offset: 0x00016CF8
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<DangerousMagicianSe>(base.Value1, 0, 0, 0, 0.2f);
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<Knife>(base.Value2, false), DrawZoneTarget.Random, AddCardsType.Normal);
			yield break;
		}
	}
}
