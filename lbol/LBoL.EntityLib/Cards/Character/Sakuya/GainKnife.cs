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
	// Token: 0x0200038F RID: 911
	[UsedImplicitly]
	public sealed class GainKnife : Card
	{
		// Token: 0x06000CF8 RID: 3320 RVA: 0x00018D2F File Offset: 0x00016F2F
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<GainKnifeSe>(base.Value1, 0, 0, 0, 0.2f);
			if (base.Value2 > 0)
			{
				yield return new AddCardsToHandAction(Library.CreateCards<Knife>(base.Value2, false), AddCardsType.Normal);
			}
			yield break;
		}
	}
}
