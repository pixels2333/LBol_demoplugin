using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x0200044D RID: 1101
	[UsedImplicitly]
	public sealed class StarSky : Card
	{
		// Token: 0x06000EF8 RID: 3832 RVA: 0x0001B232 File Offset: 0x00019432
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Charging>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
