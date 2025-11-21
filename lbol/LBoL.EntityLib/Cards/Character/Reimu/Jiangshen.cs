using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Reimu;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003D9 RID: 985
	[UsedImplicitly]
	public sealed class Jiangshen : Card
	{
		// Token: 0x06000DD1 RID: 3537 RVA: 0x00019BEE File Offset: 0x00017DEE
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<JiangshenSe>(0, 0, 0, 0, 0.2f);
			yield return new AddCardsToHandAction(Library.CreateCards<YinyangCard>(base.Value1, false), AddCardsType.Normal);
			yield break;
		}
	}
}
