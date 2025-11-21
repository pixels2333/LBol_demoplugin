using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Character.Cirno.Friend;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004B2 RID: 1202
	[UsedImplicitly]
	public sealed class FairyTrick : Card
	{
		// Token: 0x06000FF6 RID: 4086 RVA: 0x0001C54E File Offset: 0x0001A74E
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.SacrificeAction(base.Value1);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			List<Card> list = new List<Card>();
			list.Add(Library.CreateCard<SunnyFriend>());
			list.Add(Library.CreateCard<LunaFriend>());
			list.Add(Library.CreateCard<StarFriend>());
			List<Card> cards = list;
			if (this.IsUpgraded)
			{
				cards.Add(Library.CreateCard<ClownpieceFriend>());
			}
			foreach (Card card in cards)
			{
				card.Summon();
			}
			if (base.Battle.HandIsNotFull)
			{
				Card card2 = cards.Sample(base.BattleRng);
				cards.Remove(card2);
				yield return new AddCardsToHandAction(new Card[] { card2 });
			}
			yield return new AddCardsToDrawZoneAction(cards, DrawZoneTarget.Random, AddCardsType.Normal);
			yield break;
		}
	}
}
