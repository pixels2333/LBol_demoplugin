using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x0200040D RID: 1037
	[UsedImplicitly]
	public sealed class Alioth : Card
	{
		// Token: 0x06000E51 RID: 3665 RVA: 0x0001A5C5 File Offset: 0x000187C5
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ScryAction(base.Scry);
			yield return new DrawManyCardAction(base.Value1);
			yield return base.BuffAction<Charging>(base.Value2, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
