using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x02000395 RID: 917
	[UsedImplicitly]
	public sealed class KnifeHell : Card
	{
		// Token: 0x06000D0F RID: 3343 RVA: 0x00018EC2 File Offset: 0x000170C2
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddCardsToHandAction(Library.CreateCards<Knife>(base.Value1, false), AddCardsType.Normal);
			yield return new UpgradeCardsAction(Enumerable.Where<Card>(base.Battle.EnumerateAllCards(), (Card card) => card.CanUpgradeAndPositive && card is Knife));
			yield break;
		}
	}
}
