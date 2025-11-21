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
	// Token: 0x0200041A RID: 1050
	[UsedImplicitly]
	public sealed class DropBook : Card
	{
		// Token: 0x06000E72 RID: 3698 RVA: 0x0001A82F File Offset: 0x00018A2F
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return new AddCardsToHandAction(Library.CreateCards<Astrology>(base.Value1, false), AddCardsType.Normal);
			yield break;
		}
	}
}
