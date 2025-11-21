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
	// Token: 0x020004A3 RID: 1187
	[UsedImplicitly]
	public sealed class CirnoDraw : Card
	{
		// Token: 0x06000FCF RID: 4047 RVA: 0x0001C221 File Offset: 0x0001A421
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new DrawManyCardAction(base.Value1);
			if (base.Value2 > 0)
			{
				yield return base.BuffAction<ExtraDraw>(base.Value2, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
