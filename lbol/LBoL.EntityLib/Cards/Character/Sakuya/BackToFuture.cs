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
	// Token: 0x02000380 RID: 896
	[UsedImplicitly]
	public sealed class BackToFuture : Card
	{
		// Token: 0x06000CCB RID: 3275 RVA: 0x000189FD File Offset: 0x00016BFD
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Reverse<Card>(base.Battle.DrawZone));
			List<Card> oriDiscard = Enumerable.ToList<Card>(base.Battle.DiscardZone);
			foreach (Card card in list)
			{
				if (card.Zone == CardZone.Draw)
				{
					yield return new MoveCardAction(card, CardZone.Discard);
				}
			}
			List<Card>.Enumerator enumerator = default(List<Card>.Enumerator);
			foreach (Card card2 in oriDiscard)
			{
				if (card2.Zone == CardZone.Discard)
				{
					yield return new MoveCardToDrawZoneAction(card2, DrawZoneTarget.Top);
				}
			}
			enumerator = default(List<Card>.Enumerator);
			if (base.Value1 > 0)
			{
				yield return new DrawManyCardAction(base.Value1);
			}
			yield break;
			yield break;
		}
	}
}
