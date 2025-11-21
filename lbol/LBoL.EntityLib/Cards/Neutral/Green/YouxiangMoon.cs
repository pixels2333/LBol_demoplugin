using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Character.Cirno;

namespace LBoL.EntityLib.Cards.Neutral.Green
{
	// Token: 0x02000302 RID: 770
	[UsedImplicitly]
	public sealed class YouxiangMoon : Card
	{
		// Token: 0x06000B75 RID: 2933 RVA: 0x0001702A File Offset: 0x0001522A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddCardsToHandAction(Library.CreateCards<SummerFlower>(base.Value1, false), AddCardsType.Normal);
			yield return base.BuffAction<YouxiangMoonSe>(1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
