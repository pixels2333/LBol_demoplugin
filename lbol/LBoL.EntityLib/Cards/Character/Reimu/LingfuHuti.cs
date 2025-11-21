using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003DF RID: 991
	[UsedImplicitly]
	public sealed class LingfuHuti : Card
	{
		// Token: 0x06000DE5 RID: 3557 RVA: 0x00019DCD File Offset: 0x00017FCD
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<Shenyuzha>() });
			yield break;
		}
	}
}
