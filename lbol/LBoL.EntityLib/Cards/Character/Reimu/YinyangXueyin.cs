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
	// Token: 0x02000409 RID: 1033
	[UsedImplicitly]
	public sealed class YinyangXueyin : Card
	{
		// Token: 0x06000E48 RID: 3656 RVA: 0x0001A53C File Offset: 0x0001873C
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.SacrificeAction(base.Value2);
			yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<YinyangCard>() });
			yield return base.BuffAction<YinyangXueyinSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
