using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Neutral.NoColor;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000441 RID: 1089
	[UsedImplicitly]
	public sealed class SatelliteIllusion : Card
	{
		// Token: 0x06000EDC RID: 3804 RVA: 0x0001B050 File Offset: 0x00019250
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return new AddCardsToHandAction(Library.CreateCards<Astrology>(base.Value1, false), AddCardsType.Normal);
			yield break;
		}
	}
}
