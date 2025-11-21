using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004A9 RID: 1193
	[UsedImplicitly]
	public sealed class ColdHearted : Card
	{
		// Token: 0x06000FDF RID: 4063 RVA: 0x0001C36E File Offset: 0x0001A56E
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<ColdHeartedSe>(0, 0, 0, 0, 0.2f);
			List<IceLance> list = Enumerable.ToList<IceLance>(Library.CreateCards<IceLance>(base.Value1, false));
			foreach (IceLance iceLance in list)
			{
				iceLance.SetTurnCost(base.Mana);
				iceLance.IsEthereal = true;
				iceLance.IsExile = true;
			}
			yield return new AddCardsToHandAction(list, AddCardsType.Normal);
			yield break;
		}
	}
}
