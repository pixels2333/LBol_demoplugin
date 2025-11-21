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
	// Token: 0x02000400 RID: 1024
	[UsedImplicitly]
	public sealed class TaijiLiangyi : Card
	{
		// Token: 0x06000E30 RID: 3632 RVA: 0x0001A394 File Offset: 0x00018594
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<YinyangCard>() });
			yield return base.BuffAction<TaijiLiangyiSe>(1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
