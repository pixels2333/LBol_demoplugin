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
	// Token: 0x02000381 RID: 897
	[UsedImplicitly]
	public sealed class BladePower : Card
	{
		// Token: 0x06000CCD RID: 3277 RVA: 0x00018A15 File Offset: 0x00016C15
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddCardsToHandAction(Library.CreateCards<Knife>(base.Value1, false), AddCardsType.Normal);
			yield return base.BuffAction<BladePowerSe>(1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
