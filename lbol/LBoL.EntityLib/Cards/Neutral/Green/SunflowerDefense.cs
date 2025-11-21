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
	// Token: 0x020002FF RID: 767
	[UsedImplicitly]
	public sealed class SunflowerDefense : Card
	{
		// Token: 0x06000B69 RID: 2921 RVA: 0x00016F00 File Offset: 0x00015100
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return new AddCardsToHandAction(Library.CreateCards<SummerFlower>(base.Value1, false), AddCardsType.Normal);
			yield break;
		}
	}
}
