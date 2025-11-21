using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Neutral.NoColor;

namespace LBoL.EntityLib.Cards.Neutral.Green
{
	// Token: 0x020002FE RID: 766
	[UsedImplicitly]
	public sealed class RangziSeed : Card
	{
		// Token: 0x06000B67 RID: 2919 RVA: 0x00016EE8 File Offset: 0x000150E8
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> list = new List<Card>();
			if (this.IsUpgraded)
			{
				PManaCard pmanaCard = Library.CreateCard<PManaCard>();
				pmanaCard.IsReplenish = true;
				list.Add(pmanaCard);
			}
			else
			{
				GManaCard gmanaCard = Library.CreateCard<GManaCard>();
				gmanaCard.IsReplenish = true;
				list.Add(gmanaCard);
			}
			yield return new AddCardsToDrawZoneAction(list, DrawZoneTarget.Top, AddCardsType.Normal);
			yield break;
		}
	}
}
