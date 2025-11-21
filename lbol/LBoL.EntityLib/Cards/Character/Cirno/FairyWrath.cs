using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Character.Cirno.Friend;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004B3 RID: 1203
	[UsedImplicitly]
	public sealed class FairyWrath : Card
	{
		// Token: 0x170001BF RID: 447
		// (get) Token: 0x06000FF8 RID: 4088 RVA: 0x0001C566 File Offset: 0x0001A766
		public override bool Triggered
		{
			get
			{
				if (base.Battle != null)
				{
					return Enumerable.Any<Card>(base.Battle.HandZone, (Card card) => card is DayaojingFriend && card.Summoned);
				}
				return false;
			}
		}

		// Token: 0x06000FF9 RID: 4089 RVA: 0x0001C5A1 File Offset: 0x0001A7A1
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.TriggeredAnyhow)
			{
				yield return new AddCardsToHandAction(Library.CreateCards<SummerFlower>(base.Value1, false), AddCardsType.Normal);
			}
			yield break;
		}
	}
}
