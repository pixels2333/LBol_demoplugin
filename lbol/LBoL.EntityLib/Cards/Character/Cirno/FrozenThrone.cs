using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004BB RID: 1211
	[UsedImplicitly]
	public sealed class FrozenThrone : Card
	{
		// Token: 0x0600100E RID: 4110 RVA: 0x0001C774 File Offset: 0x0001A974
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<IceBolt>() });
			yield return base.BuffAction<FrozenThroneSe>(1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
