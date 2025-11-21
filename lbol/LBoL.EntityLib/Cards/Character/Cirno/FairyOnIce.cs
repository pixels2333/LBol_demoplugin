using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004AF RID: 1199
	[UsedImplicitly]
	public sealed class FairyOnIce : Card
	{
		// Token: 0x06000FEE RID: 4078 RVA: 0x0001C4A0 File Offset: 0x0001A6A0
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<IceWing>(base.Value1, false), DrawZoneTarget.Top, AddCardsType.Normal);
			yield return base.BuffAction<Graze>(base.Value2, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
