using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.DebugCards
{
	[UsedImplicitly]
	public sealed class DebugRemoveAllHandWhenDraw : Card
	{
		public override IEnumerable<BattleAction> OnDraw()
		{
			List<Card> list = Enumerable.ToList<Card>(base.Battle.HandZone);
			foreach (Card card in list)
			{
				if (card.Battle != null)
				{
					yield return new RemoveCardAction(card);
				}
			}
			List<Card>.Enumerator enumerator = default(List<Card>.Enumerator);
			yield break;
			yield break;
		}
	}
}
