using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000462 RID: 1122
	[UsedImplicitly]
	public sealed class FindInspiration : Card
	{
		// Token: 0x06000F27 RID: 3879 RVA: 0x0001B4F6 File Offset: 0x000196F6
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<InspirationCard>(base.Value1, false), DrawZoneTarget.Random, AddCardsType.Normal);
			yield break;
		}
	}
}
