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
	// Token: 0x0200037E RID: 894
	[UsedImplicitly]
	public sealed class AutoKnife : Card
	{
		// Token: 0x06000CC6 RID: 3270 RVA: 0x0001897D File Offset: 0x00016B7D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<AutoKnifeSe>(0, 0, 0, 0, 0.2f);
			yield return new AddCardsToHandAction(Library.CreateCards<Knife>(base.Value1, false), AddCardsType.Normal);
			yield break;
		}
	}
}
