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
	// Token: 0x020003E0 RID: 992
	[UsedImplicitly]
	public sealed class MengzhiYuzha : Card
	{
		// Token: 0x06000DE7 RID: 3559 RVA: 0x00019DE5 File Offset: 0x00017FE5
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<Fengmozhen>() });
			yield break;
		}
	}
}
