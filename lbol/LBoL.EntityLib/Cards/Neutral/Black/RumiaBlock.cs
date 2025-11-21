using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x0200033A RID: 826
	[UsedImplicitly]
	public sealed class RumiaBlock : Card
	{
		// Token: 0x06000C0D RID: 3085 RVA: 0x00017B63 File Offset: 0x00015D63
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return new AddCardsToHandAction(Library.CreateCards<Shadow>(base.Value1, false), AddCardsType.Normal);
			yield break;
		}
	}
}
