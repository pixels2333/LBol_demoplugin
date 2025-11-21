using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000420 RID: 1056
	[UsedImplicitly]
	public sealed class GreenMogu : Card
	{
		// Token: 0x06000E84 RID: 3716 RVA: 0x0001A9B3 File Offset: 0x00018BB3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<Potion>(base.Value1, false), DrawZoneTarget.Random, AddCardsType.Normal);
			yield break;
		}
	}
}
