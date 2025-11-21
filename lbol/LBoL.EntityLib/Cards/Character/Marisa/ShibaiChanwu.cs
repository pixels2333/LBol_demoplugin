using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Neutral.Black;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000444 RID: 1092
	[UsedImplicitly]
	public sealed class ShibaiChanwu : Card
	{
		// Token: 0x06000EE3 RID: 3811 RVA: 0x0001B0C5 File Offset: 0x000192C5
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<Potion>(base.Value1, false), DrawZoneTarget.Random, AddCardsType.Normal);
			yield return new AddCardsToHandAction(Library.CreateCards<Shadow>(base.Value2, false), AddCardsType.Normal);
			yield break;
		}
	}
}
