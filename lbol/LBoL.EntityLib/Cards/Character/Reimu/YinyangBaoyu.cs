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
	// Token: 0x02000406 RID: 1030
	[UsedImplicitly]
	public sealed class YinyangBaoyu : Card
	{
		// Token: 0x06000E3F RID: 3647 RVA: 0x0001A4B8 File Offset: 0x000186B8
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<YinyangCard>() });
			yield break;
		}
	}
}
